#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Seeds the ARINC 424 cycle2508 schema from a local Docker container into Azure PostgreSQL,
    then updates the aiatc-scenario Container App connection string.

.DESCRIPTION
    Dumps the cycle2508 schema from the local arinc424 container (port 5430) using a
    throwaway postgres:17-alpine Docker container, creates the arinc424 database on
    Azure PostgreSQL if needed, restores the dump, and optionally updates the Container App
    environment variable so the ScenarioService uses live ARINC 424 data.

    Re-running is safe: the dump uses --clean --if-exists so all objects are dropped and
    recreated each run.

.PREREQUISITES
    - Docker Desktop must be running
    - az CLI must be logged in (az login)
    - Local ARINC 424 container must be running on port 5430

.EXAMPLE
    # Full run (dump, restore, update Container App)
    ./seed-airspace-db.ps1

    # Reuse existing dump file, skip dump step
    ./seed-airspace-db.ps1 -SkipDump

    # Dump only, no Azure restore
    ./seed-airspace-db.ps1 -SkipRestore -SkipContainerAppUpdate

    # Use a specific Azure host instead of auto-discovery
    ./seed-airspace-db.ps1 -TargetHost "myserver.postgres.database.azure.com"
#>

param(
    [string]$ResourceGroup   = "aiatc-rg",
    [string]$ContainerApp    = "aiatc-scenario",
    [string]$TargetDb        = "arinc424",
    [string]$TargetSchema    = "cycle2508",
    [string]$TargetUser      = "aiatcadmin",
    [string]$TargetPassword  = "AiAtcDb2026!",
    [string]$TargetHost      = "",            # auto-discovered from Azure if empty

    [string]$SourceHost      = "host.docker.internal",
    [string]$SourcePort      = "5430",
    [string]$SourceDb        = "arinc424",
    [string]$SourceUser      = "arinc424",
    [string]$SourcePassword  = "fly_@irline_RADA4!",
    [string]$SourceSchema    = "cycle2508",

    [string]$DumpFile        = "$PSScriptRoot\arinc424_cycle2508.sql",

    [switch]$SkipDump,                # reuse existing DumpFile
    [switch]$SkipRestore,             # dump only, skip Azure restore
    [switch]$SkipContainerAppUpdate   # skip Container App env-var update
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# --- Helpers ------------------------------------------------------------------

function Write-Step([string]$msg) {
    Write-Host ""
    Write-Host "==> $msg" -ForegroundColor Cyan
}

function Write-Success([string]$msg) {
    Write-Host "    OK: $msg" -ForegroundColor Green
}

function Write-Warn([string]$msg) {
    Write-Host "    WARN: $msg" -ForegroundColor Yellow
}

function Assert-Command([string]$name) {
    if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
        throw "Required command '$name' not found in PATH."
    }
}

# --- Preflight checks ---------------------------------------------------------

Write-Step "Preflight checks"

Assert-Command "docker"
Assert-Command "az"

# Verify Docker is running
# Lower ErrorActionPreference so Docker's stderr warnings don't become terminating
# errors; restore it immediately after so the rest of the script stays strict.
$ErrorActionPreference = "Continue"
docker info *> $null
$dockerExit = $LASTEXITCODE
$ErrorActionPreference = "Stop"
if ($dockerExit -ne 0) {
    throw "Docker is not running. Please start Docker Desktop and try again."
}
Write-Success "Docker is running"

# Verify az is logged in
$ErrorActionPreference = "Continue"
az account show *> $null
$azExit = $LASTEXITCODE
$ErrorActionPreference = "Stop"
if ($azExit -ne 0) {
    throw "Not logged in to Azure CLI. Run 'az login' first."
}
Write-Success "Azure CLI is authenticated"

# --- Step 1: Discover Azure PostgreSQL FQDN and server name ------------------

Write-Step "Step 1: Discover Azure PostgreSQL host"

if ($TargetHost -eq "") {
    Write-Host "    Querying Azure for PostgreSQL Flexible Server details..."
    $serverJson = az postgres flexible-server list `
        --resource-group $ResourceGroup `
        --query "[0].{fqdn:fullyQualifiedDomainName, name:name}" -o json 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to query Azure PostgreSQL servers in resource group '$ResourceGroup'."
    }
    $serverInfo = $serverJson | ConvertFrom-Json
    $TargetHost   = $serverInfo.fqdn
    $script:PgServerName = $serverInfo.name
    if (-not $TargetHost) {
        throw "No PostgreSQL Flexible Server found in resource group '$ResourceGroup'."
    }
    Write-Success "Discovered host: $TargetHost (server: $script:PgServerName)"
} else {
    # Derive server name from FQDN (strip .postgres.database.azure.com suffix)
    $script:PgServerName = $TargetHost -replace '\.postgres\.database\.azure\.com$', ''
    Write-Success "Using provided host: $TargetHost (server: $script:PgServerName)"
}

# --- Step 1b: Open temporary firewall rule for this machine ------------------

Write-Step "Step 1b: Add temporary firewall rule for current public IP"

$myIp = (Invoke-RestMethod -Uri "https://api.ipify.org?format=text").Trim()
$firewallRule = "seed-script-$(Get-Date -Format 'yyyyMMddHHmmss')"
Write-Host "    Public IP: $myIp"
Write-Host "    Rule name: $firewallRule"

$ErrorActionPreference = "Continue"
az postgres flexible-server firewall-rule create `
    --resource-group $ResourceGroup `
    --name $script:PgServerName `
    --rule-name $firewallRule `
    --start-ip-address $myIp `
    --end-ip-address $myIp *> $null
$fwCreateExit = $LASTEXITCODE
$ErrorActionPreference = "Stop"

if ($fwCreateExit -ne 0) {
    throw "Failed to create firewall rule on '$script:PgServerName'. Check that your az account has Contributor access."
}
Write-Success "Firewall rule '$firewallRule' added for $myIp"

# Register cleanup so the rule is always removed, even on error
$script:FirewallRuleToCleanup = $firewallRule
Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
    if ($script:FirewallRuleToCleanup) {
        az postgres flexible-server firewall-rule delete `
            --resource-group $using:ResourceGroup `
            --name $using:script:PgServerName `
            --rule-name $script:FirewallRuleToCleanup `
            --yes *> $null
    }
} | Out-Null

# Short pause for the firewall rule to propagate
Write-Host "    Waiting 10 seconds for firewall rule to propagate..."
Start-Sleep -Seconds 10

# --- Step 2: Dump cycle2508 schema from local container ----------------------

Write-Step "Step 2: Dump '$SourceSchema' schema from local container"

if ($SkipDump -and (Test-Path $DumpFile)) {
    Write-Warn "-SkipDump specified and '$DumpFile' exists - skipping dump."
} else {
    if ($SkipDump) {
        Write-Warn "-SkipDump specified but '$DumpFile' does not exist - running dump anyway."
    }

    Write-Host "    Dump destination: $DumpFile"
    Write-Host "    Source: $SourceUser@${SourceHost}:${SourcePort}/$SourceDb (schema: $SourceSchema)"

    # Mount the scripts directory so pg_dump can write the file there
    $mountDir = (Split-Path $DumpFile -Parent).Replace('\', '/')
    $dumpFileName = Split-Path $DumpFile -Leaf

    docker run --rm `
        -e PGPASSWORD=$SourcePassword `
        -v "${mountDir}:/mnt" `
        postgres:17-alpine `
        pg_dump `
            -h $SourceHost `
            -p $SourcePort `
            -U $SourceUser `
            -d $SourceDb `
            -n $SourceSchema `
            --no-owner `
            --no-privileges `
            --clean `
            --if-exists `
            -F p `
            -f "/mnt/$dumpFileName"

    if ($LASTEXITCODE -ne 0) {
        throw "pg_dump failed. Check that the local ARINC 424 container is running on port $SourcePort."
    }

    $dumpSize = (Get-Item $DumpFile).Length / 1MB
    Write-Success "Dump complete: $DumpFile ($([math]::Round($dumpSize, 1)) MB)"
}

if ($SkipRestore) {
    Write-Warn "-SkipRestore specified - stopping after dump."
    exit 0
}

# --- Step 3: Create arinc424 database on Azure (idempotent) ------------------

Write-Step "Step 3: Create '$TargetDb' database on Azure PostgreSQL (if not exists)"

# \gexec is a psql metacommand and only works via stdin, not -c.
# Pipe the SQL so psql processes \gexec interactively.
$createDbSql = "SELECT 'CREATE DATABASE $TargetDb' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname='$TargetDb')\gexec"
$createDbSql | docker run --rm -i `
    -e PGPASSWORD=$TargetPassword `
    postgres:17-alpine `
    psql "host=$TargetHost port=5432 dbname=postgres user=$TargetUser sslmode=require"

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create database '$TargetDb' on '$TargetHost'. Check credentials and network access."
}
Write-Success "Database '$TargetDb' is ready"

# --- Step 4: Restore dump to Azure -------------------------------------------

Write-Step "Step 4: Restore dump to '${TargetHost}/$TargetDb'"

Write-Host "    This may take several minutes for large datasets..."

$dumpContent = Get-Content $DumpFile -Raw -Encoding UTF8
$dumpContent | docker run --rm -i `
    -e PGPASSWORD=$TargetPassword `
    postgres:17-alpine `
    psql "host=$TargetHost port=5432 dbname=$TargetDb user=$TargetUser sslmode=require"

if ($LASTEXITCODE -ne 0) {
    throw "psql restore failed. The dump may be incomplete - check the output above."
}
Write-Success "Schema '$TargetSchema' restored to '$TargetDb' on Azure"

# --- Step 5: Verify row counts -----------------------------------------------

Write-Step "Step 5: Verify restore (spot-check row counts)"

$rowCountSql = @"
SELECT table_name, (xpath('/row/cnt/text()', query_to_xml(
    format('SELECT COUNT(*) AS cnt FROM %I.%I', table_schema, table_name), true, true, ''
)))[1]::text::int AS row_count
FROM information_schema.tables
WHERE table_schema = '$TargetSchema' AND table_type = 'BASE TABLE'
ORDER BY table_name;
"@

docker run --rm `
    -e PGPASSWORD=$TargetPassword `
    postgres:17-alpine `
    psql "host=$TargetHost port=5432 dbname=$TargetDb user=$TargetUser sslmode=require" `
    -c $rowCountSql

if ($LASTEXITCODE -ne 0) {
    Write-Warn "Row count verification failed - non-fatal. Check the database manually."
} else {
    Write-Success "Row count verification complete"
}

# --- Step 6: Update Container App environment variable -----------------------

if ($SkipContainerAppUpdate) {
    Write-Warn "-SkipContainerAppUpdate specified - skipping Container App update."
} else {
    Write-Step "Step 6: Update '$ContainerApp' Container App connection string"

    $connStr = "Host=$TargetHost;Port=5432;Database=$TargetDb;Username=$TargetUser;Password=$TargetPassword;Search Path=$TargetSchema;Ssl Mode=Require"

    Write-Host "    Updating ConnectionStrings__AirspaceDb on '$ContainerApp'..."

    $ErrorActionPreference = "Continue"
    az containerapp update `
        --name $ContainerApp `
        --resource-group $ResourceGroup `
        --set-env-vars "ConnectionStrings__AirspaceDb=$connStr" *> $null
    $appUpdateExit = $LASTEXITCODE
    $ErrorActionPreference = "Stop"

    if ($appUpdateExit -ne 0) {
        throw "Failed to update Container App '$ContainerApp'. The database is seeded but the app still uses the old connection string. Update it manually."
    }
    Write-Success "Container App '$ContainerApp' updated - new revision will be created automatically"

    # --- Step 7: Wait for new revision ----------------------------------------

    Write-Step "Step 7: Wait for new revision to become active"

    Write-Host "    Waiting 30 seconds for new revision to start..."
    Start-Sleep -Seconds 30

    $ErrorActionPreference = "Continue"
    $latestRevision = az containerapp revision list `
        --name $ContainerApp `
        --resource-group $ResourceGroup `
        --query "sort_by([].{name:name, created:properties.createdTime, state:properties.runningState}, &created)[-1]" `
        -o json 2>&1
    $revListExit = $LASTEXITCODE
    $ErrorActionPreference = "Stop"

    if ($revListExit -eq 0) {
        $rev = $latestRevision | ConvertFrom-Json
        Write-Host "    Latest revision: $($rev.name)"
        Write-Host "    State: $($rev.state)"
        Write-Host "    Created: $($rev.created)"

        if ($rev.state -eq "Running") {
            Write-Success "New revision is running"
        } else {
            Write-Warn "Revision state is '$($rev.state)' - may still be starting. Check with:"
            Write-Warn "  az containerapp revision list --name $ContainerApp --resource-group $ResourceGroup"
        }
    } else {
        Write-Warn "Could not query revision list - check manually."
    }
}

# --- Cleanup: Remove temporary firewall rule ----------------------------------

Write-Step "Cleanup: Remove temporary firewall rule"

$ErrorActionPreference = "Continue"
az postgres flexible-server firewall-rule delete `
    --resource-group $ResourceGroup `
    --name $script:PgServerName `
    --rule-name $script:FirewallRuleToCleanup `
    --yes *> $null
$fwDeleteExit = $LASTEXITCODE
$ErrorActionPreference = "Stop"

if ($fwDeleteExit -eq 0) {
    Write-Success "Firewall rule '$script:FirewallRuleToCleanup' removed"
    $script:FirewallRuleToCleanup = $null   # prevent double-delete on exit
} else {
    Write-Warn "Could not remove firewall rule '$script:FirewallRuleToCleanup' - delete it manually in the Azure portal."
}

# --- Summary ------------------------------------------------------------------

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ARINC 424 seeding complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Check health endpoint: https://<your-app-domain>/health"
Write-Host "     - 'airspace_db' should now be Healthy"
Write-Host "  2. Test in browser: load KSFO - should show full ARINC 424 runway data"
Write-Host "     (not just the 4 well-known fallback runways)"
Write-Host ""
Write-Host "To verify manually:"
Write-Host "  az containerapp revision list --name $ContainerApp --resource-group $ResourceGroup"
Write-Host ""
