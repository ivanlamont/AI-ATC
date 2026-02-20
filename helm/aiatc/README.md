# AI-ATC Helm Chart

Helm chart for deploying the AI Air Traffic Control application to Kubernetes with dual-database architecture and Dapr integration.

## Architecture

This chart deploys:
- **Web Application** (Blazor WebAssembly) - User interface
- **Scenario Service** (gRPC) - Microservice for scenario management and airspace data
- **PostgreSQL** - Scenario usage database (sessions, scores, saves)
- **Redis** - Dapr state store and pub/sub
- **Dapr** - Service mesh for service-to-service communication

### External Dependencies

- **Reference Database** (port 5430) - ARINC 424 airspace reference data (not managed by this chart)
- **Dapr Control Plane** - Must be installed in the cluster

## Prerequisites

- Kubernetes 1.24+
- Helm 3.10+
- Dapr 1.16+ installed in the cluster
- kubectl configured
- Access to external ARINC 424 reference database

### Install Dapr

```bash
# Install Dapr CLI
wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash

# Initialize Dapr in Kubernetes
dapr init -k

# Verify Dapr installation
kubectl get pods -n dapr-system
```

## Installation

### 1. Add PostgreSQL and Redis Helm repositories

```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
```

### 2. Create namespace

```bash
kubectl create namespace aiatc
```

### 3. Configure values

Copy `values.yaml` and customize:

```yaml
# values.yaml
scenarioService:
  image:
    repository: your-registry/aiatc-scenario-service
    tag: "v1.0.0"

web:
  image:
    repository: your-registry/aiatc-web
    tag: "v1.0.0"

externalReferenceDb:
  host: "your-reference-db-host"
  passwordValue: "your-secure-password"

flightAware:
  apiKey: "your-flightaware-api-key"
```

### 4. Install the chart

```bash
helm install aiatc ./helm/aiatc -n aiatc -f your-values.yaml
```

### 5. Verify deployment

```bash
# Check pod status
kubectl get pods -n aiatc

# Check services
kubectl get svc -n aiatc

# Check Dapr components
kubectl get components -n aiatc

# View deployment notes
helm get notes aiatc -n aiatc
```

## Configuration

### Key Values

| Parameter | Description | Default |
|-----------|-------------|---------|
| `web.replicaCount` | Number of web replicas | `3` |
| `scenarioService.replicaCount` | Number of scenario service replicas | `2` |
| `scenarioService.service.grpcPort` | gRPC port for scenario service | `5001` |
| `postgresql.auth.database` | Usage database name | `aiatc_usage` |
| `postgresql.auth.password` | Database password | `aiatc_k8s_password` |
| `externalReferenceDb.host` | Reference DB hostname | `reference-db-service.default.svc.cluster.local` |
| `externalReferenceDb.port` | Reference DB port | `5432` |
| `externalReferenceDb.passwordValue` | Reference DB password | `fly_@irline_RADA4!` |
| `flightAware.apiKey` | FlightAware API key | `""` |
| `dapr.enabled` | Enable Dapr integration | `true` |

### Autoscaling

Both web and scenario service support horizontal pod autoscaling:

```yaml
web:
  autoscaling:
    enabled: true
    minReplicas: 3
    maxReplicas: 10
    targetCPUUtilizationPercentage: 80

scenarioService:
  autoscaling:
    enabled: true
    minReplicas: 2
    maxReplicas: 8
    targetCPUUtilizationPercentage: 70
```

### Resource Limits

```yaml
scenarioService:
  resources:
    requests:
      memory: "256Mi"
      cpu: "250m"
    limits:
      memory: "512Mi"
      cpu: "500m"
```

## Database Management

### Usage Database (PostgreSQL)

Managed by this chart. Migrations run automatically on scenario service startup.

**Connection string format:**
```
Host=<release-name>-postgresql;Port=5432;Database=aiatc_usage;Username=aiatc;Password=<password>
```

### Reference Database (External)

Not managed by this chart. Must be accessible from the cluster.

**Connection string format:**
```
Host=<host>;Port=<port>;Database=arinc424;Username=arinc424;Password=<password>;Search Path=cycle2508
```

## Accessing Services

### Web Application

```bash
# Get LoadBalancer IP
kubectl get svc aiatc-web -n aiatc

# Port forward (if using ClusterIP)
kubectl port-forward svc/aiatc-web 8080:80 -n aiatc
# Visit http://localhost:8080
```

### Scenario Service (gRPC)

```bash
# Port forward
kubectl port-forward svc/aiatc-scenario-service 5001:5001 -n aiatc

# Test with grpcurl
grpcurl -plaintext localhost:5001 list
grpcurl -plaintext localhost:5001 aiatc.scenario.ScenarioService/HealthCheck
```

## Monitoring

### View Logs

```bash
# Scenario service logs
kubectl logs -l app.kubernetes.io/component=scenario-service -n aiatc -c scenario-service

# Scenario service Dapr sidecar logs
kubectl logs -l app.kubernetes.io/component=scenario-service -n aiatc -c daprd

# Web service logs
kubectl logs -l app.kubernetes.io/component=web -n aiatc -c web
```

### Health Checks

```bash
# Check scenario service health
kubectl port-forward svc/aiatc-scenario-service 5001:5001 -n aiatc
curl http://localhost:5001/health

# Check web service
kubectl port-forward svc/aiatc-web 8080:80 -n aiatc
curl http://localhost:8080/
```

### Metrics

```bash
# View HPA status
kubectl get hpa -n aiatc

# Describe HPA
kubectl describe hpa aiatc-scenario-service -n aiatc
```

## Upgrading

```bash
helm upgrade aiatc ./helm/aiatc -n aiatc -f your-values.yaml
```

## Uninstalling

```bash
# Delete the release
helm uninstall aiatc -n aiatc

# Delete persistent volumes (if needed)
kubectl delete pvc -l app.kubernetes.io/instance=aiatc -n aiatc

# Delete namespace
kubectl delete namespace aiatc
```

## Troubleshooting

### Pods not starting

```bash
# Check pod status
kubectl describe pod <pod-name> -n aiatc

# Check events
kubectl get events -n aiatc --sort-by='.lastTimestamp'
```

### Database connection issues

```bash
# Check PostgreSQL pod
kubectl get pods -l app.kubernetes.io/name=postgresql -n aiatc
kubectl logs <postgresql-pod> -n aiatc

# Test connection from scenario service
kubectl exec -it <scenario-service-pod> -n aiatc -c scenario-service -- /bin/bash
```

### Dapr issues

```bash
# Check Dapr system
kubectl get pods -n dapr-system

# Check Dapr components
kubectl get components -n aiatc

# View Dapr sidecar logs
kubectl logs <pod-name> -n aiatc -c daprd
```

### gRPC communication issues

```bash
# Test from within the cluster
kubectl run test-pod --image=fullstorydev/grpcurl:latest --rm -it --restart=Never -- \
  -plaintext aiatc-scenario-service:5001 list

# Check service endpoints
kubectl get endpoints -n aiatc
```

## Production Considerations

1. **Secrets Management**: Use external secret management (e.g., Vault, AWS Secrets Manager)
2. **TLS/SSL**: Enable TLS for gRPC and HTTPS for web
3. **Ingress**: Configure ingress controller for external access
4. **Monitoring**: Integrate with Prometheus/Grafana
5. **Backup**: Schedule PostgreSQL backups
6. **Resource Limits**: Tune based on load testing
7. **Network Policies**: Implement pod-to-pod network policies
8. **Image Security**: Scan images for vulnerabilities

## Support

For issues or questions:
- GitHub Issues: https://github.com/your-org/ai-atc/issues
- Documentation: ./docs/
