#!/bin/bash

# AI-ATC Deployment Script
# This script deploys the AI-ATC application to Azure with cost-efficient configuration

set -e

# Use cmd.exe to run Azure CLI commands (works in bash on Windows)
AZ_CMD='cmd.exe /c "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"'

# Configuration
RESOURCE_GROUP="aiatc-rg"
LOCATION="eastus"
SUBSCRIPTION_ID="${AZURE_SUBSCRIPTION_ID}"
STATIC_WEB_APP_NAME="aiatc-web"
AKS_CLUSTER_NAME="aiatc-aks"
ACR_NAME="aiatcregistry"
DOMAIN_NAME="approachcontroller.com"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."

    # Check Azure CLI
    if ! command -v az &> /dev/null && [ ! -f "$AZ_CMD" ]; then
        log_error "Azure CLI is not found in PATH or at expected location."
        log_error "Expected location: C:\\Program Files\\Microsoft SDKs\\Azure\\CLI2\\wbin\\az"
        log_error "Please ensure Azure CLI is installed."
        log_error "Installation: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi

    # Test Azure CLI
    if ! "$AZ_CMD" --version &> /dev/null; then
        log_error "Azure CLI is installed but not working properly."
        log_error "Try running 'az login' in PowerShell first to authenticate."
        exit 1
    fi

    if ! command -v terraform &> /dev/null; then
        log_error "Terraform is not installed. Please install it first."
        exit 1
    fi

    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed. Please install it first."
        exit 1
    fi

    if ! command -v kubectl &> /dev/null; then
        log_error "kubectl is not installed. Please install it first."
        exit 1
    fi

    if ! command -v helm &> /dev/null; then
        log_error "Helm is not installed. Please install it first."
        exit 1
    fi

    # Check if logged in to Azure
    if ! "$AZ_CMD" account show &> /dev/null; then
        log_error "Not logged in to Azure. Please run 'az login' first."
        exit 1
    fi

    log_info "Prerequisites check passed."
}

# Deploy Static Web App
deploy_static_web_app() {
    log_info "Deploying Static Web App..."

    # Create resource group if it doesn't exist
    "$AZ_CMD" group create --name $RESOURCE_GROUP --location $LOCATION --output none

    # Build the Blazor WebAssembly app
    log_info "Building Blazor WebAssembly app..."
    cd src/AIATC.Web
    dotnet publish -c Release -o dist --nologo

    # Create static web app
    "$AZ_CMD" staticwebapp create \
        --name $STATIC_WEB_APP_NAME \
        --resource-group $RESOURCE_GROUP \
        --location $LOCATION \
        --source ./dist/wwwroot \
        --output none

    # Get the static web app URL
    STATIC_WEB_APP_URL=$("$AZ_CMD" staticwebapp show --name $STATIC_WEB_APP_NAME --resource-group $RESOURCE_GROUP --query "defaultHostname" -o tsv)

    log_info "Static Web App deployed at: https://$STATIC_WEB_APP_URL"

    # Set up custom domain (if domain is configured)
    if [ -n "$DOMAIN_NAME" ]; then
        log_info "Setting up custom domain: $DOMAIN_NAME"

        # Add custom domain
        "$AZ_CMD" staticwebapp custom-domain set \
            --name $STATIC_WEB_APP_NAME \
            --resource-group $RESOURCE_GROUP \
            --domain-name $DOMAIN_NAME \
            --output none

        log_info "Custom domain configured. Please update your DNS records to point to the static web app."
    fi

    cd ../..
}

# Deploy Infrastructure with Terraform
deploy_infrastructure() {
    log_info "Deploying infrastructure with Terraform..."

    cd terraform/azure

    # Initialize Terraform
    terraform init

    # Create terraform.tfvars file
    cat > terraform.tfvars << EOF
subscription_id = "$SUBSCRIPTION_ID"
resource_group_name = "$RESOURCE_GROUP"
location = "$LOCATION"
cluster_name = "$AKS_CLUSTER_NAME"
container_registry_name = "$ACR_NAME"
node_count = 1
vm_size = "Standard_B2s"
postgres_server_name = "aiatc-db"
db_admin_username = "aiatcadmin"
db_admin_password = "$(openssl rand -base64 16)"
redis_cache_name = "aiatc-redis"
storage_account_name = "aiatcstorage$(date +%s)"
EOF

    # Plan and apply
    terraform plan -out=tfplan
    terraform apply tfplan

    # Get outputs
    AKS_CLUSTER_NAME=$(terraform output -raw kubernetes_cluster_name)
    ACR_LOGIN_SERVER=$(terraform output -raw container_registry_login_server)

    cd ../..
}

# Build and push Docker images
build_and_push_images() {
    log_info "Building and pushing Docker images..."

    # Login to ACR
    "$AZ_CMD" acr login --name $ACR_NAME

    # Build and push the main application image
    docker build -t $ACR_LOGIN_SERVER/aiatc-web:latest .
    docker push $ACR_LOGIN_SERVER/aiatc-web:latest

    log_info "Docker images built and pushed."
}

# Deploy to Kubernetes
deploy_to_kubernetes() {
    log_info "Deploying to Kubernetes..."

    # Get AKS credentials
    "$AZ_CMD" aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME --overwrite-existing

    # Update Helm values for cost efficiency
    cd helm/aiatc

    cat > values.yaml << EOF
replicaCount: 1

image:
  repository: $ACR_LOGIN_SERVER/aiatc-web
  tag: "latest"
  pullPolicy: IfNotPresent

service:
  type: LoadBalancer
  port: 80
  targetPort: 5000

ingress:
  enabled: true
  className: nginx
  hosts:
    - host: api.approachcontroller.com
      paths:
        - path: /
          pathType: Prefix

resources:
  requests:
    memory: "128Mi"
    cpu: "100m"
  limits:
    memory: "256Mi"
    cpu: "200m"

autoscaling:
  enabled: false

postgresql:
  enabled: true
  auth:
    username: aiatc
    password: aiatc_k8s_password
    database: aiatc

redis:
  enabled: true
  auth:
    enabled: false

env:
  ASPNETCORE_ENVIRONMENT: Production
  LOG_LEVEL: Information
EOF

    # Install/upgrade Helm chart
    helm upgrade --install aiatc . --namespace aiatc --create-namespace

    cd ../..
}

# Main deployment function
main() {
    log_info "Starting AI-ATC deployment to Azure..."

    check_prerequisites
    deploy_static_web_app
    deploy_infrastructure
    build_and_push_images
    deploy_to_kubernetes

    log_info "Deployment completed successfully!"
    log_info "Web App: https://$DOMAIN_NAME"
    log_info "API: https://api.$DOMAIN_NAME"
    log_info ""
    log_info "Cost-efficient configuration:"
    log_info "- AKS: 1 node, Standard_B2s VM (~$30/month)"
    log_info "- PostgreSQL: Basic tier (~$20/month)"
    log_info "- Redis: Basic C0 (~$15/month)"
    log_info "- Static Web App: Free tier"
    log_info "Total estimated monthly cost: ~$65/month"
}

# Run main function
main "$@"