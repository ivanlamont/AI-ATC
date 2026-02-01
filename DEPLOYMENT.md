# AI-ATC Deployment Guide

Complete instructions for deploying AI-ATC to production on Azure or AWS.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Building the Application](#building-the-application)
3. [Azure Deployment](#azure-deployment)
4. [AWS Deployment](#aws-deployment)
5. [Post-Deployment Configuration](#post-deployment-configuration)
6. [Monitoring and Maintenance](#monitoring-and-maintenance)
7. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools

- **Docker**: 20.10+ ([Install Docker](https://docs.docker.com/get-docker/))
- **Kubernetes CLI (kubectl)**: 1.24+ ([Install kubectl](https://kubernetes.io/docs/tasks/tools/))
- **Helm**: 3.10+ ([Install Helm](https://helm.sh/docs/intro/install/))
- **.NET CLI**: 7.0+ ([Install .NET](https://dotnet.microsoft.com/download))

### For Azure Deployment

- **Azure CLI**: 2.40+ ([Install Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli))
- **Terraform**: 1.3+ ([Install Terraform](https://www.terraform.io/downloads))
- **Azure Subscription** with appropriate permissions

### For AWS Deployment

- **AWS CLI**: 2.10+ ([Install AWS CLI](https://aws.amazon.com/cli/))
- **AWS Account** with appropriate IAM permissions

### Environment Variables

Create a `.env` file in the project root:

```bash
# Application Settings
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80

# Database
DB_HOST=postgresql.example.com
DB_PORT=5432
DB_NAME=aiatc
DB_USER=postgres
DB_PASSWORD=secure_password_here

# Authentication
JWT_SECRET=your_jwt_secret_key_here
JWT_EXPIRY_MINUTES=1440

# AI Agent
AI_AGENT_HOST=ai-agent-service
AI_AGENT_PORT=50051

# Redis Cache
REDIS_HOST=redis.example.com
REDIS_PORT=6379

# ADSBexchange API
ADSB_API_KEY=your_adsb_api_key_here

# Email Configuration
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your_email@gmail.com
SMTP_PASSWORD=your_email_password

# Application URLs
DOMAIN=aiatc.example.com
```

## Building the Application

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/AI-ATC.git
cd AI-ATC
```

### 2. Build .NET Solution

```bash
dotnet restore
dotnet build -c Release
dotnet test
```

### 3. Build Docker Images

```bash
# Build all service images
docker build -f src/AIATC.Web/Dockerfile -t aiatc-web:latest src/AIATC.Web/
docker build -f src/AIATC.SimulationService/Dockerfile -t aiatc-simulation:latest src/AIATC.SimulationService/
docker build -f src/AIATC.AIAgentService/Dockerfile -t aiatc-ai-agent:latest src/AIATC.AIAgentService/
docker build -f src/AIATC.ScenarioService/Dockerfile -t aiatc-scenario:latest src/AIATC.ScenarioService/
docker build -f src/AIATC.UserService/Dockerfile -t aiatc-user:latest src/AIATC.UserService/
docker build -f src/AIATC.AudioService/Dockerfile -t aiatc-audio:latest src/AIATC.AudioService/

# Tag for container registry
docker tag aiatc-web:latest your-registry.azurecr.io/aiatc-web:latest
docker tag aiatc-simulation:latest your-registry.azurecr.io/aiatc-simulation:latest
# ... repeat for other services
```

### 4. Push to Container Registry

**For Azure Container Registry (ACR)**:

```bash
az acr login --name your-registry
docker push your-registry.azurecr.io/aiatc-web:latest
docker push your-registry.azurecr.io/aiatc-simulation:latest
# ... push other services
```

**For AWS Elastic Container Registry (ECR)**:

```bash
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin your-account.dkr.ecr.us-east-1.amazonaws.com
docker tag aiatc-web:latest your-account.dkr.ecr.us-east-1.amazonaws.com/aiatc-web:latest
docker push your-account.dkr.ecr.us-east-1.amazonaws.com/aiatc-web:latest
# ... push other services
```

## Azure Deployment

### Option 1: Using Terraform (Recommended)

#### Step 1: Configure Terraform Variables

Create `terraform/terraform.tfvars`:

```hcl
project_name       = "aiatc"
environment         = "production"
location            = "East US"
kubernetes_version  = "1.28"

# Container Registry
container_registry_sku = "Premium"

# Database
postgres_sku_name = "B_Standard_B2s"
postgres_version  = "14"

# Kubernetes
node_count  = 3
node_vm_size = "Standard_D4s_v3"

# Application
app_replicas = 3
app_cpu      = "500m"
app_memory   = "512Mi"

domain = "aiatc.example.com"
```

#### Step 2: Initialize and Deploy

```bash
cd terraform/azure

# Initialize Terraform
terraform init

# Validate configuration
terraform validate

# Plan deployment
terraform plan -out=tfplan

# Apply deployment
terraform apply tfplan
```

#### Step 3: Retrieve Kubeconfig

```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group aiatc-rg \
  --name aiatc-aks \
  --overwrite-existing

# Verify connection
kubectl cluster-info
kubectl get nodes
```

### Option 2: Manual Azure Deployment

#### Step 1: Create Resource Group

```bash
az group create \
  --name aiatc-rg \
  --location eastus
```

#### Step 2: Create Container Registry

```bash
az acr create \
  --resource-group aiatc-rg \
  --name aiatcregistry \
  --sku Premium
```

#### Step 3: Create AKS Cluster

```bash
az aks create \
  --resource-group aiatc-rg \
  --name aiatc-aks \
  --node-count 3 \
  --vm-set-type VirtualMachineScaleSets \
  --load-balancer-sku standard \
  --enable-managed-identity \
  --network-plugin azure \
  --docker-bridge-address 172.17.0.1/16 \
  --service-cidr 10.0.0.0/16 \
  --dns-service-ip 10.0.0.10 \
  --attach-acr aiatcregistry \
  --generate-ssh-keys
```

#### Step 4: Create PostgreSQL Database

```bash
az postgres server create \
  --resource-group aiatc-rg \
  --name aiatc-postgres \
  --location eastus \
  --admin-user pgadmin \
  --admin-password <secure-password> \
  --sku-name B_Gen5_2 \
  --storage-mb 51200 \
  --version 14 \
  --ssl-enforcement Enabled \
  --minimal-tls-version TLS1_2
```

#### Step 5: Create Redis Cache

```bash
az redis create \
  --resource-group aiatc-rg \
  --name aiatc-redis \
  --location eastus \
  --sku Basic \
  --vm-size c0 \
  --enable-non-ssl-port false \
  --minimum-tls-version 1.2
```

#### Step 6: Deploy with Helm

```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group aiatc-rg \
  --name aiatc-aks

# Create namespace
kubectl create namespace aiatc

# Create secrets
kubectl create secret docker-registry acr-secret \
  --docker-server=aiatcregistry.azurecr.io \
  --docker-username=<username> \
  --docker-password=<password> \
  --namespace=aiatc

# Deploy with Helm
helm install aiatc helm/aiatc \
  --namespace aiatc \
  --values helm/values-azure.yaml
```

## AWS Deployment

### Option 1: Using CloudFormation (Recommended)

#### Step 1: Prepare Template

AWS CloudFormation templates are in `cloudformation/`:

```bash
cd cloudformation
```

#### Step 2: Create Stack

```bash
aws cloudformation create-stack \
  --stack-name aiatc-stack \
  --template-body file://aiatc-stack.yaml \
  --parameters \
    ParameterKey=Environment,ParameterValue=production \
    ParameterKey=ClusterName,ParameterValue=aiatc-cluster \
    ParameterKey=NodeCount,ParameterValue=3 \
    ParameterKey=InstanceType,ParameterValue=t3.large \
  --capabilities CAPABILITY_NAMED_IAM \
  --region us-east-1
```

#### Step 3: Monitor Stack Creation

```bash
aws cloudformation wait stack-create-complete \
  --stack-name aiatc-stack \
  --region us-east-1

# Get outputs
aws cloudformation describe-stacks \
  --stack-name aiatc-stack \
  --query 'Stacks[0].Outputs' \
  --region us-east-1
```

#### Step 4: Configure kubectl

```bash
# Get cluster details
CLUSTER_NAME=$(aws cloudformation describe-stacks \
  --stack-name aiatc-stack \
  --query 'Stacks[0].Outputs[?OutputKey==`ClusterName`].OutputValue' \
  --output text)

# Update kubeconfig
aws eks update-kubeconfig \
  --region us-east-1 \
  --name $CLUSTER_NAME
```

### Option 2: Manual AWS Deployment

#### Step 1: Create EKS Cluster

```bash
aws eks create-cluster \
  --name aiatc-cluster \
  --version 1.28 \
  --role-arn arn:aws:iam::ACCOUNT_ID:role/eks-service-role \
  --resources-vpc-config subnetIds=subnet-xxx,subnet-yyy,subnet-zzz \
  --region us-east-1
```

#### Step 2: Create Node Group

```bash
aws eks create-nodegroup \
  --cluster-name aiatc-cluster \
  --nodegroup-name aiatc-nodes \
  --subnets subnet-xxx subnet-yyy subnet-zzz \
  --node-role arn:aws:iam::ACCOUNT_ID:role/NodeInstanceRole \
  --scaling-config minSize=2,maxSize=6,desiredSize=3 \
  --instance-types t3.large \
  --region us-east-1
```

#### Step 3: Create RDS PostgreSQL

```bash
aws rds create-db-instance \
  --db-instance-identifier aiatc-postgres \
  --db-instance-class db.t3.medium \
  --engine postgres \
  --engine-version 14.7 \
  --master-username pgadmin \
  --master-user-password <secure-password> \
  --allocated-storage 100 \
  --storage-type gp3 \
  --storage-encrypted \
  --region us-east-1
```

#### Step 4: Create ElastiCache Redis

```bash
aws elasticache create-cache-cluster \
  --cache-cluster-id aiatc-redis \
  --cache-node-type cache.t3.micro \
  --engine redis \
  --engine-version 7.0 \
  --num-cache-nodes 1 \
  --port 6379 \
  --security-group-ids sg-xxxxxxxx \
  --region us-east-1
```

#### Step 5: Deploy with Helm

```bash
# Update kubeconfig
aws eks update-kubeconfig --name aiatc-cluster --region us-east-1

# Create namespace
kubectl create namespace aiatc

# Create secrets
kubectl create secret docker-registry ecr-secret \
  --docker-server=ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com \
  --docker-username=AWS \
  --docker-password=$(aws ecr get-login-password --region us-east-1) \
  --namespace=aiatc

# Deploy with Helm
helm install aiatc helm/aiatc \
  --namespace aiatc \
  --values helm/values-aws.yaml
```

## Post-Deployment Configuration

### 1. Initialize Database

```bash
# Port-forward to PostgreSQL
kubectl port-forward -n aiatc svc/postgresql 5432:5432 &

# Run migrations
dotnet ef database update \
  --project src/AIATC.Domain \
  --startup-project src/AIATC.Web \
  --connection "Host=localhost;Username=postgres;Password=<password>;Database=aiatc"
```

### 2. Configure TLS Certificate

**Using Let's Encrypt (Recommended)**:

```bash
# Install cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Create certificate
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: aiatc-cert
  namespace: aiatc
spec:
  secretName: aiatc-tls
  issuerRef:
    name: letsencrypt-prod
    kind: ClusterIssuer
  dnsNames:
  - aiatc.example.com
  - www.aiatc.example.com
EOF
```

### 3. Configure Ingress

```bash
kubectl apply -f - <<EOF
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: aiatc-ingress
  namespace: aiatc
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - aiatc.example.com
    secretName: aiatc-tls
  rules:
  - host: aiatc.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: aiatc-web
            port:
              number: 80
EOF
```

### 4. Configure DNS

Point your domain to the ingress load balancer:

```bash
# Get ingress IP/hostname
kubectl get ingress -n aiatc

# Create CNAME record pointing to load balancer
aiatc.example.com. CNAME xxx.elb.amazonaws.com
```

## Monitoring and Maintenance

### 1. Access Grafana Dashboard

```bash
# Port-forward to Grafana
kubectl port-forward -n monitoring svc/prometheus-grafana 3000:80

# Open browser: http://localhost:3000
# Default credentials: admin / prom-operator
```

### 2. View Logs

```bash
# View application logs
kubectl logs -n aiatc -l app=aiatc-web --tail=100 -f

# View pod events
kubectl describe pod -n aiatc <pod-name>

# View all events
kubectl get events -n aiatc --sort-by='.lastTimestamp'
```

### 3. Scale Services

```bash
# Scale web service
kubectl scale deployment aiatc-web -n aiatc --replicas=5

# Auto-scaling (if HPA configured)
kubectl get hpa -n aiatc
```

### 4. Backup Database

**Azure**:
```bash
az postgres server backup create \
  --resource-group aiatc-rg \
  --server-name aiatc-postgres \
  --backup-name backup-$(date +%Y%m%d-%H%M%S)
```

**AWS**:
```bash
aws rds create-db-snapshot \
  --db-instance-identifier aiatc-postgres \
  --db-snapshot-identifier aiatc-backup-$(date +%Y%m%d-%H%M%S)
```

### 5. Update Application

```bash
# Build new image
docker build -t aiatc-web:v2.0 src/AIATC.Web/

# Push to registry
docker push your-registry.azurecr.io/aiatc-web:v2.0

# Update Helm values
helm upgrade aiatc helm/aiatc \
  --namespace aiatc \
  --set image.tag=v2.0
```

## Troubleshooting

### Pods Not Starting

```bash
# Check pod status
kubectl describe pod -n aiatc <pod-name>

# Check logs
kubectl logs -n aiatc <pod-name> --previous

# Check events
kubectl get events -n aiatc
```

### Database Connection Issues

```bash
# Verify database is accessible
kubectl run -it --rm psql --image=postgres:14 --restart=Never -- \
  psql -h postgresql.example.com -U postgres -d aiatc -c "SELECT 1"
```

### Out of Memory

```bash
# Check resource usage
kubectl top nodes
kubectl top pods -n aiatc

# Increase pod limits
kubectl set resources deployment aiatc-web -n aiatc \
  --limits=memory=1Gi,cpu=1000m \
  --requests=memory=512Mi,cpu=500m
```

### Slow Performance

```bash
# Check Redis connection
kubectl exec -it -n aiatc <redis-pod> -- redis-cli ping

# View slow queries
kubectl exec -it -n aiatc <postgres-pod> -- \
  psql -U postgres -d aiatc -c "SELECT * FROM pg_stat_statements"
```

## Security Checklist

- [ ] Change default passwords for all services
- [ ] Enable network policies in Kubernetes
- [ ] Configure pod security policies
- [ ] Enable RBAC (role-based access control)
- [ ] Use TLS for all communications
- [ ] Enable audit logging
- [ ] Regular security updates
- [ ] Backup encryption enabled
- [ ] Database encryption at rest
- [ ] Restrict ingress to trusted IPs (if applicable)

## Support and Resources

- **Documentation**: See `/docs` directory
- **Issues**: Create GitHub issues for bugs
- **Architecture**: See architecture diagrams in `/docs`
- **API Documentation**: Swagger UI at `/swagger`

---

**Deployment Complete!** Your AI-ATC instance is now live and ready for use.
