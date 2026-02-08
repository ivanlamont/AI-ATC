# Task List Amendments

**Date:** 2026-01-31
**Purpose:** Update deployment and CI/CD tasks to include production and test environments with GitHub Actions integration

## Amended Tasks

### Task #28: Azure Deployment with CI/CD (AMENDED)

**Original:** Create Terraform scripts for Azure deployment

**Amended Title:** Create Azure deployment with Terraform and GitHub Actions CI/CD

**Updated Requirements:**

#### Infrastructure as Code (Terraform)
1. **Resource Groups**
   - `ai-atc-test-rg` - Test environment resources
   - `ai-atc-prod-rg` - Production environment resources

2. **App Services**
   - Azure App Service for Blazor WebAssembly
   - Test: `ai-atc-test-app.azurewebsites.net`
   - Production: `ai-atc-prod-app.azurewebsites.net`
   - Auto-scaling configuration
   - Health checks and monitoring

3. **Database**
   - Azure Database for PostgreSQL
   - Test: `ai-atc-test-db.postgres.database.azure.com`
   - Production: `ai-atc-prod-db.postgres.database.azure.com`
   - Point-in-time restore enabled
   - Automated backups

4. **Cache**
   - Azure Cache for Redis
   - Test and production instances
   - Connection string configuration

5. **Storage**
   - Azure Blob Storage for static assets
   - CDN integration for performance
   - Separate containers for test/prod

6. **Monitoring**
   - Application Insights for telemetry
   - Log Analytics workspace
   - Alerts for critical metrics

7. **Networking**
   - Virtual Network (VNet) configuration
   - Network Security Groups (NSGs)
   - Private endpoints for databases

#### GitHub Actions Workflows

**File:** `.github/workflows/azure-deploy-test.yml`
```yaml
name: Deploy to Azure Test

on:
  push:
    branches: [develop]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Run tests
        run: dotnet test --no-build --verbosity normal

  deploy-to-test:
    needs: build-and-test
    runs-on: ubuntu-latest
    environment: azure-test
    steps:
      - uses: actions/checkout@v4
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_TEST_CREDENTIALS }}
      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v2
        with:
          app-name: ai-atc-test-app
          package: ./publish
```

**File:** `.github/workflows/azure-deploy-prod.yml`
```yaml
name: Deploy to Azure Production

on:
  push:
    branches: [main]
  release:
    types: [published]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore --configuration Release
      - name: Run tests
        run: dotnet test --no-build --verbosity normal
      - name: Publish
        run: dotnet publish -c Release -o ./publish

  deploy-to-production:
    needs: build-and-test
    runs-on: ubuntu-latest
    environment: azure-production
    steps:
      - uses: actions/checkout@v4
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_PROD_CREDENTIALS }}
      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v2
        with:
          app-name: ai-atc-prod-app
          package: ./publish
      - name: Run smoke tests
        run: |
          curl -f https://ai-atc-prod-app.azurewebsites.net/health || exit 1
```

#### Environment Configuration
- Separate Terraform workspaces for test/prod
- Environment-specific variable files
- Azure Key Vault for secrets management
- Managed identities for secure access

#### Deployment Stages
1. **Test Environment (Automatic)**
   - Triggered on push to `develop` branch
   - Runs full test suite
   - Deploys if tests pass
   - No manual approval required

2. **Production Environment (Gated)**
   - Triggered on push to `main` branch or release
   - Runs full test suite
   - Requires manual approval
   - Smoke tests after deployment
   - Rollback capability

---

### Task #29: AWS Deployment with CI/CD (AMENDED)

**Original:** Create CloudFormation templates for AWS deployment

**Amended Title:** Create AWS deployment with CloudFormation and GitHub Actions CI/CD

**Updated Requirements:**

#### Infrastructure as Code (CloudFormation)
1. **VPC and Networking**
   - Separate VPCs for test and production
   - Public and private subnets across 3 AZs
   - NAT Gateways for outbound connectivity
   - Security groups with least privilege

2. **Compute**
   - ECS Fargate for containerized services
   - Application Load Balancer (ALB)
   - Test: `test.ai-atc.com`
   - Production: `ai-atc.com`
   - Auto-scaling policies
   - Health checks

3. **Database**
   - Amazon RDS for PostgreSQL
   - Multi-AZ deployment (production)
   - Single-AZ (test)
   - Automated backups and snapshots
   - Read replicas for production

4. **Cache**
   - Amazon ElastiCache for Redis
   - Cluster mode enabled (production)
   - Separate clusters for test/prod

5. **Storage**
   - Amazon S3 for static assets
   - CloudFront CDN distribution
   - Separate buckets: `ai-atc-test`, `ai-atc-prod`
   - Versioning and lifecycle policies

6. **Monitoring**
   - CloudWatch Logs and Metrics
   - X-Ray for distributed tracing
   - CloudWatch Alarms for critical metrics
   - SNS topics for notifications

7. **Security**
   - AWS Secrets Manager for credentials
   - IAM roles with least privilege
   - WAF rules on ALB
   - Certificate Manager for SSL/TLS

#### GitHub Actions Workflows

**File:** `.github/workflows/aws-deploy-test.yml`
```yaml
name: Deploy to AWS Test

on:
  push:
    branches: [develop]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Build and Test
        run: |
          dotnet restore
          dotnet build --no-restore
          dotnet test --no-build --verbosity normal
      - name: Build Docker images
        run: |
          docker build -t ai-atc-web:${{ github.sha }} -f src/AIATC.Web/Dockerfile .
          docker build -t ai-atc-simulation:${{ github.sha }} -f src/AIATC.SimulationService/Dockerfile .

  deploy-to-test:
    needs: build-and-test
    runs-on: ubuntu-latest
    environment: aws-test
    steps:
      - uses: actions/checkout@v4
      - name: Configure AWS Credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_TEST_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_TEST_SECRET_ACCESS_KEY }}
          aws-region: us-east-1
      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v2
      - name: Push images to ECR
        run: |
          docker tag ai-atc-web:${{ github.sha }} ${{ steps.login-ecr.outputs.registry }}/ai-atc-web:latest
          docker push ${{ steps.login-ecr.outputs.registry }}/ai-atc-web:latest
      - name: Deploy to ECS
        run: |
          aws ecs update-service --cluster ai-atc-test --service ai-atc-web --force-new-deployment
```

**File:** `.github/workflows/aws-deploy-prod.yml`
```yaml
name: Deploy to AWS Production

on:
  push:
    branches: [main]
  release:
    types: [published]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Build and Test
        run: |
          dotnet restore
          dotnet build --no-restore --configuration Release
          dotnet test --no-build --verbosity normal
      - name: Build Docker images
        run: |
          docker build -t ai-atc-web:${{ github.sha }} -f src/AIATC.Web/Dockerfile .
          docker build -t ai-atc-simulation:${{ github.sha }} -f src/AIATC.SimulationService/Dockerfile .

  deploy-to-production:
    needs: build-and-test
    runs-on: ubuntu-latest
    environment: aws-production
    steps:
      - uses: actions/checkout@v4
      - name: Configure AWS Credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_PROD_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_PROD_SECRET_ACCESS_KEY }}
          aws-region: us-east-1
      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v2
      - name: Push images to ECR
        run: |
          docker tag ai-atc-web:${{ github.sha }} ${{ steps.login-ecr.outputs.registry }}/ai-atc-web:${{ github.sha }}
          docker tag ai-atc-web:${{ github.sha }} ${{ steps.login-ecr.outputs.registry }}/ai-atc-web:latest
          docker push ${{ steps.login-ecr.outputs.registry }}/ai-atc-web:${{ github.sha }}
          docker push ${{ steps.login-ecr.outputs.registry }}/ai-atc-web:latest
      - name: Deploy to ECS (Blue/Green)
        run: |
          aws ecs update-service --cluster ai-atc-prod --service ai-atc-web --force-new-deployment --deployment-configuration "deploymentCircuitBreaker={enable=true,rollback=true}"
      - name: Wait for deployment
        run: |
          aws ecs wait services-stable --cluster ai-atc-prod --services ai-atc-web
      - name: Run smoke tests
        run: |
          curl -f https://ai-atc.com/health || exit 1
          curl -f https://ai-atc.com/api/health || exit 1
```

#### CloudFormation Stacks
1. **Network Stack** - VPC, subnets, security groups
2. **Database Stack** - RDS instances
3. **Cache Stack** - ElastiCache clusters
4. **Compute Stack** - ECS clusters, services, ALB
5. **Storage Stack** - S3 buckets, CloudFront
6. **Monitoring Stack** - CloudWatch, X-Ray, SNS

#### Environment Configuration
- Separate CloudFormation stack sets for test/prod
- Parameter files for environment-specific values
- Cross-stack references for resource sharing
- AWS Systems Manager Parameter Store for configuration

#### Deployment Stages
1. **Test Environment (Automatic)**
   - Triggered on push to `develop` branch
   - Full CI/CD pipeline
   - Automatic deployment
   - Integration tests

2. **Production Environment (Gated)**
   - Triggered on push to `main` or release
   - Manual approval gate
   - Blue/green deployment
   - Automatic rollback on failure
   - Post-deployment validation

---

### Task #30: GitHub Actions CI/CD Pipeline (AMENDED)

**Original:** Implement CI/CD pipelines with GitHub Actions

**Amended Title:** Implement comprehensive GitHub Actions CI/CD pipeline

**Updated Requirements:**

#### Core Workflows

**1. Continuous Integration** (`.github/workflows/ci.yml`)
```yaml
name: Continuous Integration

on:
  push:
    branches: [develop, main]
  pull_request:
    branches: [develop, main]

jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: ['10.0.x']
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET ${{ matrix.dotnet-version }}
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ matrix.dotnet-version }}
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore --configuration Release
      - name: Test
        run: dotnet test --no-build --verbosity normal --logger trx --collect:"XPlat Code Coverage"
      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: '**/*.trx'
      - name: Code Coverage Report
        uses: codecov/codecov-action@v3
        with:
          files: '**/coverage.cobertura.xml'
      - name: SonarCloud Scan
        uses: SonarSource/sonarcloud-github-action@master
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}

  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Run dotnet format
        run: dotnet format --verify-no-changes

  security-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Run Snyk Security Scan
        uses: snyk/actions/dotnet@master
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
```

**2. Pull Request Validation** (`.github/workflows/pr-validation.yml`)
```yaml
name: Pull Request Validation

on:
  pull_request:
    types: [opened, synchronize, reopened]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - name: Check commit messages
        run: |
          # Validate conventional commits
          npm install -g @commitlint/cli @commitlint/config-conventional
          npx commitlint --from ${{ github.event.pull_request.base.sha }} --to HEAD
      - name: Check branch naming
        run: |
          BRANCH_NAME="${{ github.head_ref }}"
          if [[ ! $BRANCH_NAME =~ ^(feature|bugfix|hotfix|release)/.+ ]]; then
            echo "Branch name must follow pattern: feature/*, bugfix/*, hotfix/*, or release/*"
            exit 1
          fi
      - name: Run tests
        run: |
          dotnet restore
          dotnet build
          dotnet test --logger "console;verbosity=detailed"
```

**3. Release Management** (`.github/workflows/release.yml`)
```yaml
name: Create Release

on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  create-release:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - name: Generate changelog
        id: changelog
        uses: metcalfc/changelog-generator@v4.1.0
        with:
          myToken: ${{ secrets.GITHUB_TOKEN }}
      - name: Create Release
        uses: actions/create-release@v1
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        with:
          tag_name: ${{ github.ref }}
          release_name: Release ${{ github.ref }}
          body: ${{ steps.changelog.outputs.changelog }}
          draft: false
          prerelease: false
```

**4. Dependency Updates** (`.github/workflows/dependency-update.yml`)
```yaml
name: Dependency Update

on:
  schedule:
    - cron: '0 0 * * 1'  # Weekly on Monday
  workflow_dispatch:

jobs:
  update-dependencies:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Update NuGet packages
        run: |
          dotnet tool install -g dotnet-outdated-tool
          dotnet outdated -u
      - name: Create Pull Request
        uses: peter-evans/create-pull-request@v5
        with:
          commit-message: 'chore: update dependencies'
          title: 'Automated Dependency Update'
          body: 'This PR updates NuGet packages to their latest versions'
          branch: automated-dependency-update
```

#### Environment Configuration

**GitHub Environments:**
1. **aws-test**
   - Secrets: AWS_TEST_ACCESS_KEY_ID, AWS_TEST_SECRET_ACCESS_KEY
   - Variables: AWS_REGION=us-east-1
   - No deployment protection rules

2. **aws-production**
   - Secrets: AWS_PROD_ACCESS_KEY_ID, AWS_PROD_SECRET_ACCESS_KEY
   - Variables: AWS_REGION=us-east-1
   - Required reviewers: 2
   - Deployment branch: main only

3. **azure-test**
   - Secrets: AZURE_TEST_CREDENTIALS
   - No deployment protection rules

4. **azure-production**
   - Secrets: AZURE_PROD_CREDENTIALS
   - Required reviewers: 2
   - Deployment branch: main only

#### Branch Strategy

**Branches:**
- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `hotfix/*` - Urgent production fixes
- `release/*` - Release preparation

**Protection Rules:**
- `main`: Require PR reviews (2), status checks, no force push
- `develop`: Require PR reviews (1), status checks

#### Deployment Pipeline Flow

```
┌─────────────┐
│ Push Code   │
└──────┬──────┘
       │
       v
┌─────────────┐
│ Run Tests   │
└──────┬──────┘
       │
       v
┌─────────────┐
│ Build       │
└──────┬──────┘
       │
       v
┌─────────────┐
│ Security    │
│ Scan        │
└──────┬──────┘
       │
       v
┌─────────────┐
│ Deploy to   │
│ Test Env    │
└──────┬──────┘
       │
       v
┌─────────────┐
│ Integration │
│ Tests       │
└──────┬──────┘
       │
       v
┌─────────────┐
│ Manual      │
│ Approval    │ (for production)
└──────┬──────┘
       │
       v
┌─────────────┐
│ Deploy to   │
│ Production  │
└──────┬──────┘
       │
       v
┌─────────────┐
│ Smoke Tests │
└──────┬──────┘
       │
       v
┌─────────────┐
│ Notify Team │
└─────────────┘
```

---

## Additional Requirements

### Common to All Tasks

#### Monitoring and Alerts
- Application health endpoints
- Database connection monitoring
- Cache hit/miss ratios
- Response time metrics
- Error rate tracking
- Resource utilization alerts

#### Security
- Secrets stored in cloud provider secret managers
- No credentials in code or configuration files
- HTTPS only (TLS 1.3)
- Web Application Firewall (WAF)
- DDoS protection
- Regular security scans

#### Disaster Recovery
- Automated backups (daily for test, hourly for prod)
- Point-in-time recovery
- Cross-region replication (production)
- Documented recovery procedures
- Regular DR testing (quarterly)

#### Cost Optimization
- Auto-scaling to handle load
- Scheduled shutdown of test environment (nights/weekends)
- Reserved instances for stable workloads
- Cost allocation tags
- Monthly cost reviews

#### Documentation
- Infrastructure diagrams
- Deployment runbooks
- Rollback procedures
- Troubleshooting guides
- Secrets management procedures

---

## Implementation Order

1. **Task #30: GitHub Actions CI/CD** (Foundation)
   - Set up basic CI pipeline
   - Configure GitHub environments
   - Establish branch protection rules

2. **Task #28: Azure Deployment** (Test First)
   - Deploy test environment
   - Validate with test workflows
   - Deploy production environment

3. **Task #29: AWS Deployment** (Test First)
   - Deploy test environment
   - Validate with test workflows
   - Deploy production environment

4. **Validation**
   - End-to-end testing on both clouds
   - Load testing
   - Failover testing
   - Cost analysis

---

## Success Criteria

### Task #30 (CI/CD)
- ✅ All tests run automatically on PR
- ✅ Code coverage reports generated
- ✅ Security scans pass
- ✅ Deployments automated to test environment
- ✅ Manual approval gate for production
- ✅ Rollback capability tested

### Task #28 (Azure)
- ✅ Infrastructure deployed via Terraform
- ✅ Test and production environments functional
- ✅ Database backups configured
- ✅ Monitoring and alerts active
- ✅ CI/CD pipeline successfully deploys
- ✅ SSL certificates valid

### Task #29 (AWS)
- ✅ Infrastructure deployed via CloudFormation
- ✅ Test and production environments functional
- ✅ Database backups configured
- ✅ Monitoring and alerts active
- ✅ CI/CD pipeline successfully deploys
- ✅ SSL certificates valid

---

## Estimated Timeline

- **Task #30 (CI/CD):** 1-2 weeks
- **Task #28 (Azure):** 2-3 weeks
- **Task #29 (AWS):** 2-3 weeks
- **Total:** 5-8 weeks (can be parallelized after CI/CD is complete)

---

**Status:** ✅ Amendments Approved
**Next Action:** Begin implementation with Task #30 (CI/CD foundation)
