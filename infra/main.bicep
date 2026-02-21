// ============================================================================
// AIATC — Azure Container Apps infrastructure
//
// Creates (all in resource group aiatc-rg):
//   • Azure Container Registry           — stores BFF and ScenarioService images
//   • User-assigned Managed Identity     — passwordless ACR pull for Container Apps
//   • Log Analytics Workspace            — required by Container Apps environment
//   • PostgreSQL Flexible Server         — ScenarioService usage database
//   • Container Apps Environment         — shared runtime
//   • ScenarioService Container App      — gRPC/gRPC-Web, external HTTPS
//   • BFF Container App                  — ASP.NET Core, external HTTPS,
//                                          serves Blazor WASM + auth/speech/flights
//
// ── First-time deploy ────────────────────────────────────────────────────────
//
//   az deployment group create \
//     --resource-group aiatc-rg \
//     --template-file infra/main.bicep \
//     --parameters @infra/main.parameters.json
//
// ── Re-running Bicep for infra changes (e.g. rotating a secret) ─────────────
//
//   Pass the current image tags so Bicep does not reset them to placeholders:
//
//   az deployment group create \
//     --resource-group aiatc-rg \
//     --template-file infra/main.bicep \
//     --parameters @infra/main.parameters.json \
//     --parameters bffImage=<acrLoginServer>/aiatc-bff:<tag> \
//                  scenarioImage=<acrLoginServer>/aiatc-scenario-service:<tag>
//
// ── After first deploy — add these outputs as GitHub Actions secrets ─────────
//
//   ACR_LOGIN_SERVER       ← acrLoginServer output
//   ACR_NAME               ← acrName output
//   SCENARIO_SERVICE_URL   ← https://{scenarioServiceHostname output}
//
//   The CI/CD pipeline injects SCENARIO_SERVICE_URL into the WASM build so the
//   browser knows where to open its gRPC-Web channel.
//
//   The BFF's new production URL is:  https://{bffHostname output}
//   Update the OAuth redirect URIs in Azure Portal and Google Cloud Console to:
//     https://{bffHostname}/auth/callback
// ============================================================================

targetScope = 'resourceGroup'

// ── Parameters ───────────────────────────────────────────────────────────────

@description('Azure region. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Base name prefix used to derive all resource names.')
param appName string = 'aiatc'

@description('Azure Speech Service region (must match where the resource was created).')
param azureSpeechRegion string = 'eastus'

@description('OAuth authority for Azure AD. Use common for multi-tenant or your tenant GUID.')
param oauthAzureAuthority string = 'https://login.microsoftonline.com/common'

param oauthAzureClientId string
param oauthGoogleClientId string

@description('BFF container image. Defaults to a harmless placeholder; CI/CD updates this on each push.')
param bffImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('ScenarioService container image. Defaults to placeholder; CI/CD updates this on each push.')
param scenarioImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@secure()
param postgresAdminPassword string

@secure()
param oauthAzureClientSecret string

@secure()
param oauthGoogleClientSecret string

@secure()
param azureSpeechSubscriptionKey string

@secure()
param flightAwareApiKey string

@secure()
@description('Full Npgsql connection string for the external ARINC 424 airspace database. Leave empty to disable the feature.')
param airspaceDbConnectionString string = ''

// ── Derived names ─────────────────────────────────────────────────────────────
//
// uniqueString is deterministic per resource group so names are stable across
// re-runs while still being globally unique.

var uid          = uniqueString(resourceGroup().id)
// PostgreSQL uses a separate seed so its name can be changed independently
// if the first name gets stuck in Azure's global reservation after a failed deploy.
var pgUid        = uniqueString(resourceGroup().id, 'pg2')
var acrName      = '${appName}registry${uid}'       // alphanumeric only — ACR requirement
var logWsName    = '${appName}-logs'
var envName      = '${appName}-env'
var identityName = '${appName}-identity'
var pgName       = '${appName}-postgres-${pgUid}'
var pgDb         = 'aiatc_usage'
var pgAdmin      = 'aiatcadmin'
var bffName      = '${appName}-bff'
var scenarioName = '${appName}-scenario'

// ── User-assigned Managed Identity ───────────────────────────────────────────
//
// Both Container Apps use this identity to pull images from ACR without storing
// registry credentials anywhere.

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

// ── Azure Container Registry ──────────────────────────────────────────────────

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: { name: 'Basic' }
  properties: {
    adminUserEnabled: false   // managed-identity pull only; no shared password
  }
}

// AcrPull built-in role ID — allows reading images, nothing else.
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource acrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, identity.id, acrPullRoleId)
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ── Log Analytics Workspace ───────────────────────────────────────────────────
//
// Container Apps Environment requires a Log Analytics workspace for structured
// log shipping. 30-day retention keeps costs minimal.

resource logWs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logWsName
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

// ── PostgreSQL Flexible Server ────────────────────────────────────────────────
//
// Burstable B1ms (1 vCore, 2 GiB RAM) is the lowest cost tier. Scale up by
// changing sku.name and sku.tier and re-running the Bicep.

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: pgName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: pgAdmin
    administratorLoginPassword: postgresAdminPassword
    storage: { storageSizeGB: 32 }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: { mode: 'Disabled' }
  }
}

resource pgDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgres
  name: pgDb
}

// 0.0.0.0 → 0.0.0.0 is the Azure-reserved sentinel that means
// "allow all traffic originating inside Azure" (Container Apps included).
resource pgFirewall 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgres
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

var pgConnStr = 'Host=${postgres.properties.fullyQualifiedDomainName};Port=5432;Database=${pgDb};Username=${pgAdmin};Password=${postgresAdminPassword};Ssl Mode=Require;Trust Server Certificate=true'

// ── Container Apps Environment ────────────────────────────────────────────────

resource caEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: envName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logWs.properties.customerId
        sharedKey: logWs.listKeys().primarySharedKey
      }
    }
  }
}

// ── ScenarioService Container App ─────────────────────────────────────────────
//
// External HTTPS ingress with transport: 'http' because the Blazor WASM client
// uses gRPC-Web (Grpc.Net.Client.Web), which wraps gRPC in HTTP/1.1-compatible
// frames. The ScenarioService's own Program.cs already calls UseGrpcWeb() and
// configures CORS — no changes needed there.
//
// Health check: GET /health (port 5001) — defined in ScenarioService Program.cs.
//
// The placeholder image starts at 0 replicas so it never actually runs.
// CI/CD sets the real image and raises minReplicas to 1:
//
//   az containerapp update \
//     --name aiatc-scenario --resource-group aiatc-rg \
//     --image <acrLoginServer>/aiatc-scenario-service:<sha> \
//     --min-replicas 1

resource scenarioApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: scenarioName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    environmentId: caEnv.id
    configuration: {
      registries: [{
        server: acr.properties.loginServer
        identity: identity.id
      }]
      ingress: {
        external: true
        targetPort: 5001
        transport: 'http'       // gRPC-Web works over HTTP/1.1; 'http' accepts both HTTP/1.1 and HTTP/2
        allowInsecure: false
        corsPolicy: {
          // Only the BFF origin is allowed — ScenarioService is not a public API.
          // Bicep creates bffApp first (implicit dependency) so the FQDN is known.
          allowedOrigins: ['https://${bffApp.properties.configuration.ingress.fqdn}']
          allowedMethods: ['GET', 'POST', 'OPTIONS']
          allowedHeaders: ['*']
          exposeHeaders: ['Grpc-Status', 'Grpc-Message', 'Grpc-Encoding', 'Grpc-Accept-Encoding']
          allowCredentials: false
        }
      }
      secrets: union(
        [{ name: 'pg-conn', value: pgConnStr }],
        !empty(airspaceDbConnectionString) ? [{ name: 'airspace-conn', value: airspaceDbConnectionString }] : []
      )
    }
    template: {
      containers: [{
        name: 'scenario-service'
        image: scenarioImage
        resources: {
          cpu: json('0.5')
          memory: '1Gi'
        }
        probes: [
          {
            type: 'Liveness'
            httpGet: { path: '/health', port: 5001, scheme: 'HTTP' }
            initialDelaySeconds: 30
            periodSeconds: 15
          }
          {
            type: 'Readiness'
            httpGet: { path: '/health', port: 5001, scheme: 'HTTP' }
            initialDelaySeconds: 10
            periodSeconds: 5
          }
        ]
        env: union(
          [
            { name: 'ASPNETCORE_ENVIRONMENT',             value: 'Production' }
            { name: 'ASPNETCORE_URLS',                    value: 'http://+:5001' }
            { name: 'ConnectionStrings__ScenarioUsageDb', secretRef: 'pg-conn' }
          ],
          !empty(airspaceDbConnectionString) ? [{ name: 'ConnectionStrings__AirspaceDb', secretRef: 'airspace-conn' }] : []
        )
      }]
      scale: {
        minReplicas: 0    // stays at 0 until CI/CD deploys the real image
        maxReplicas: 5
      }
    }
  }
  dependsOn: [acrPull]
}

// ── BFF Container App ─────────────────────────────────────────────────────────
//
// External HTTPS ingress. Serves the Blazor WASM static files (compiled into
// the Docker image via the ProjectReference to AIATC.Web) and all server-side
// API routes: /auth/*, /api/speech/*, /api/flights/*.
//
// All secrets are stored as Container App secrets and injected as env vars —
// they never appear in plain text in logs or the Azure Portal's env var view.
//
// The placeholder image starts at 0 replicas.
// CI/CD sets the real image and raises minReplicas to 1:
//
//   az containerapp update \
//     --name aiatc-bff --resource-group aiatc-rg \
//     --image <acrLoginServer>/aiatc-bff:<sha> \
//     --min-replicas 1

resource bffApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: bffName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    environmentId: caEnv.id
    configuration: {
      registries: [{
        server: acr.properties.loginServer
        identity: identity.id
      }]
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      secrets: [
        { name: 'oauth-azure-secret',  value: oauthAzureClientSecret }
        { name: 'oauth-google-secret', value: oauthGoogleClientSecret }
        { name: 'speech-key',          value: azureSpeechSubscriptionKey }
        { name: 'flightaware-key',     value: flightAwareApiKey }
      ]
    }
    template: {
      containers: [{
        name: 'bff'
        image: bffImage
        resources: {
          cpu: json('0.5')
          memory: '1Gi'
        }
        probes: [
          {
            // /healthz is a minimal API endpoint (no session/auth/static-file pipeline)
            // that returns 200 in <5 ms. The default probe timeout is 1 second; on a
            // cold .NET 10 container, MapStaticAssets builds the endpoint routing table
            // on the first request which can exceed 1 s on 0.5 vCPU — causing false
            // liveness failures. timeoutSeconds:10 prevents that race.
            type: 'Liveness'
            httpGet: { path: '/healthz', port: 8080, scheme: 'HTTP' }
            initialDelaySeconds: 30
            periodSeconds: 15
            timeoutSeconds: 10
          }
          {
            type: 'Readiness'
            httpGet: { path: '/healthz', port: 8080, scheme: 'HTTP' }
            initialDelaySeconds: 10
            periodSeconds: 5
            timeoutSeconds: 10
          }
        ]
        env: [
          { name: 'ASPNETCORE_ENVIRONMENT',       value: 'Production' }
          { name: 'ASPNETCORE_HTTP_PORTS',        value: '8080' }
          { name: 'OAuth__Azure__ClientId',       value: oauthAzureClientId }
          { name: 'OAuth__Azure__ClientSecret',   secretRef: 'oauth-azure-secret' }
          { name: 'OAuth__Azure__Authority',      value: oauthAzureAuthority }
          { name: 'OAuth__Google__ClientId',      value: oauthGoogleClientId }
          { name: 'OAuth__Google__ClientSecret',  secretRef: 'oauth-google-secret' }
          { name: 'AzureSpeech__SubscriptionKey', secretRef: 'speech-key' }
          { name: 'AzureSpeech__Region',          value: azureSpeechRegion }
          { name: 'FlightAware__ApiKey',          secretRef: 'flightaware-key' }
        ]
      }]
      scale: {
        minReplicas: 0    // stays at 0 until CI/CD deploys the real image
        maxReplicas: 5
      }
    }
  }
  dependsOn: [acrPull]
}

// ── Outputs ───────────────────────────────────────────────────────────────────
//
// After deployment, run:
//   az deployment group show -g aiatc-rg -n main --query properties.outputs
// to retrieve all values at once.

@description('ACR login server (e.g. aiatcregistryXXXX.azurecr.io). Add as ACR_LOGIN_SERVER GitHub secret.')
output acrLoginServer string = acr.properties.loginServer

@description('ACR resource name. Add as ACR_NAME GitHub secret.')
output acrName string = acr.name

@description('BFF public hostname (no scheme). This replaces the Static Web App. Update OAuth redirect URIs to https://{this}/auth/callback.')
output bffHostname string = bffApp.properties.configuration.ingress.fqdn

@description('ScenarioService public hostname (no scheme). Add https://{this} as SCENARIO_SERVICE_URL GitHub secret — the CI/CD pipeline injects it into the WASM appsettings at build time.')
output scenarioServiceHostname string = scenarioApp.properties.configuration.ingress.fqdn

@description('Managed identity client ID. Needed if you configure federated credentials for GitHub Actions OIDC login.')
output managedIdentityClientId string = identity.properties.clientId
