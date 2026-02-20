# AI-ATC Testing Guide
## Phase 8: End-to-End Verification

This guide provides step-by-step instructions for testing the dual-database architecture with ScenarioService.

---

## Prerequisites

### Required Software
- Docker Desktop (with Docker Compose)
- .NET 10.0 SDK
- grpcurl (for gRPC testing)
- PostgreSQL client (psql) - optional
- kubectl and Helm (for Kubernetes testing) - optional

### Install grpcurl
```bash
# Windows (using Chocolatey)
choco install grpcurl

# Or download from: https://github.com/fullstorydev/grpcurl/releases
```

### External Reference Database
Ensure the ARINC 424 reference database is running:
- **Host**: localhost
- **Port**: 5430
- **Database**: arinc424
- **Username**: arinc424
- **Password**: fly_@irline_RADA4!
- **Schema**: cycle2508

```bash
# Test connection
psql -h localhost -p 5430 -U arinc424 -d arinc424 -c "SELECT COUNT(*) FROM cycle2508.airports;"
```

---

## Test 1: Build All Projects

### Build Solution
```bash
# From repository root
dotnet build

# Expected: All projects build successfully
# Check for: 0 Error(s)
```

### Build Docker Images
```bash
# Build ScenarioService image
docker build -f src/AIATC.ScenarioService/Dockerfile -t aiatc-scenario-service:test .

# Build Web image (if Dockerfile exists for web)
docker build -f Dockerfile -t aiatc-web:test .

# Verify images created
docker images | grep aiatc
```

---

## Test 2: Docker Compose Deployment

### Start Services
```bash
# Start all services
docker-compose up -d

# Check service status
docker-compose ps

# Expected services:
# - postgres-usage (healthy)
# - redis (healthy)
# - scenario-service (running)
# - dapr-scenario-sidecar (running)
# - dapr-placement (running)
```

### View Logs
```bash
# ScenarioService logs
docker-compose logs -f scenario-service

# Look for:
# - "Database migrations applied successfully"
# - "Now listening on: http://[::]:5001"

# Dapr sidecar logs
docker-compose logs -f dapr-scenario-sidecar

# Look for:
# - "dapr initialized. Status: Running"
```

### Verify Network Connectivity
```bash
# Test postgres-usage connection
docker exec aiatc-scenario-service pg_isready -h postgres-usage -p 5432

# Test external reference DB (from container)
docker exec aiatc-scenario-service ping -c 1 host.docker.internal
```

---

## Test 3: Database Verification

### Check Usage Database
```bash
# Connect to usage database
docker exec -it aiatc-postgres-usage psql -U aiatc -d aiatc_usage

# Verify tables exist
\dt

# Expected tables:
# - scenarios, users, sessions
# - scores, saved_scenarios
# - session_commands, session_events
# - __EFMigrationsHistory

# Check migration history
SELECT * FROM "__EFMigrationsHistory";

# Exit psql
\q
```

### Check Reference Database Access
```bash
# Test from ScenarioService container
docker exec -it aiatc-scenario-service /bin/sh

# Try connecting to reference DB (if psql available)
# Or check via logs that queries work

# Exit container
exit
```

---

## Test 4: gRPC Endpoint Testing

### Health Check
```bash
# HTTP health endpoint
curl http://localhost:5001/health

# Expected: {"status":"Healthy"}
```

### List gRPC Services
```bash
# List available services
grpcurl -plaintext localhost:5001 list

# Expected output:
# aiatc.scenario.ScenarioService
# grpc.health.v1.Health
# grpc.reflection.v1alpha.ServerReflection
```

### List Service Methods
```bash
# List all RPC methods
grpcurl -plaintext localhost:5001 list aiatc.scenario.ScenarioService

# Expected methods:
# - ListScenarios
# - GetScenario
# - StartScenario
# - GetAirportData
# - GetInitialAircraftPositions
# - HealthCheck
# ...etc
```

### Test Health Check RPC
```bash
# Call HealthCheck method
grpcurl -plaintext localhost:5001 aiatc.scenario.ScenarioService/HealthCheck

# Expected:
# {
#   "status": "healthy",
#   "version": "1.0.0"
# }
```

### Test Airport Data
```bash
# Get airport data for KSFO
grpcurl -plaintext -d '{"airport_code":"KSFO"}' \
  localhost:5001 aiatc.scenario.ScenarioService/GetAirportData

# Expected: Airport data from reference database
# {
#   "icaoCode": "KSFO",
#   "name": "...",
#   "latitude": 37.xxx,
#   "longitude": -122.xxx,
#   "elevationFt": 13
# }
```

### Test Live Flight Data (if FlightAware configured)
```bash
# Get live flights near KJFK
grpcurl -plaintext -d '{"airport_code":"KJFK","radius_nm":50}' \
  localhost:5001 aiatc.scenario.ScenarioService/GetInitialAircraftPositions

# Expected: List of aircraft or empty array
# {
#   "aircraft": [...]
# }
```

---

## Test 5: Dapr Integration

### Check Dapr Status
```bash
# Check Dapr placement
docker exec aiatc-dapr-placement dapr --version

# Check sidecar health
curl http://localhost:3500/v1.0/healthz

# Expected: HTTP 204 No Content
```

### Test State Store
```bash
# Save state via Dapr
curl -X POST http://localhost:3500/v1.0/state/statestore \
  -H "Content-Type: application/json" \
  -d '[{"key":"test-key","value":"test-value"}]'

# Get state
curl http://localhost:3500/v1.0/state/statestore/test-key

# Expected: "test-value"

# Delete state
curl -X DELETE http://localhost:3500/v1.0/state/statestore/test-key
```

### Test Service Invocation
```bash
# Invoke scenario service via Dapr
curl http://localhost:3500/v1.0/invoke/scenario-service/method/health

# Expected: {"status":"healthy",...}
```

---

## Test 6: Scenario Workflow

### Create Test Scenario
```bash
# Create a test scenario directly in database
docker exec -it aiatc-postgres-usage psql -U aiatc -d aiatc_usage

INSERT INTO scenarios (id, name, description, airport_code, difficulty, duration_minutes, is_active, created_at, updated_at)
VALUES (
  'f47ac10b-58cc-4372-a567-0e02b2c3d479',
  'KSFO Morning Rush',
  'Handle morning traffic at San Francisco',
  'KSFO',
  'Medium',
  30,
  true,
  NOW(),
  NOW()
);

\q
```

### List Scenarios
```bash
# List scenarios via gRPC
grpcurl -plaintext -d '{"page":1,"page_size":10}' \
  localhost:5001 aiatc.scenario.ScenarioService/ListScenarios

# Expected: List with test scenario
```

### Get Specific Scenario
```bash
# Get scenario by ID
grpcurl -plaintext -d '{"scenario_id":"f47ac10b-58cc-4372-a567-0e02b2c3d479"}' \
  localhost:5001 aiatc.scenario.ScenarioService/GetScenario

# Expected: Scenario details
```

---

## Test 7: Web Client Integration

### Start Web Application
```bash
# If using docker-compose
docker-compose up -d web

# Or run locally
cd src/AIATC.Web
dotnet run

# Access in browser
# http://localhost:5000
```

### Test gRPC-Web Connection
```bash
# Check browser console for:
# - gRPC-Web connections to localhost:5001
# - Successful RPC calls
# - No CORS errors
```

---

## Test 8: Performance & Load

### Connection Pool Test
```bash
# Make multiple concurrent requests
for i in {1..10}; do
  curl http://localhost:5001/health &
done
wait

# All should succeed
```

### Database Connection Test
```bash
# Check active connections
docker exec aiatc-postgres-usage psql -U aiatc -d aiatc_usage \
  -c "SELECT count(*) FROM pg_stat_activity WHERE datname='aiatc_usage';"

# Should see active connections from scenario-service
```

---

## Test 9: Error Handling

### Test Invalid Scenario ID
```bash
grpcurl -plaintext -d '{"scenario_id":"invalid-uuid"}' \
  localhost:5001 aiatc.scenario.ScenarioService/GetScenario

# Expected: gRPC error with status INVALID_ARGUMENT
```

### Test Non-Existent Airport
```bash
grpcurl -plaintext -d '{"airport_code":"XXXX"}' \
  localhost:5001 aiatc.scenario.ScenarioService/GetAirportData

# Expected: gRPC error with status NOT_FOUND
```

---

## Test 10: Kubernetes Deployment (Optional)

### Prerequisites
```bash
# Ensure Kubernetes cluster is running
kubectl cluster-info

# Install Dapr on cluster
dapr init -k

# Verify Dapr installation
kubectl get pods -n dapr-system
```

### Deploy to Kubernetes
```bash
# Create namespace
kubectl create namespace aiatc

# Install Helm chart
helm install aiatc ./helm/aiatc -n aiatc

# Wait for pods to be ready
kubectl wait --for=condition=ready pod -l app.kubernetes.io/instance=aiatc -n aiatc --timeout=300s
```

### Verify Deployment
```bash
# Check pods
kubectl get pods -n aiatc

# Check services
kubectl get svc -n aiatc

# Check Dapr components
kubectl get components -n aiatc

# View scenario service logs
kubectl logs -l app.kubernetes.io/component=scenario-service -n aiatc -c scenario-service
```

### Port Forward and Test
```bash
# Forward scenario service port
kubectl port-forward svc/aiatc-scenario-service 5001:5001 -n aiatc

# Test in another terminal
grpcurl -plaintext localhost:5001 aiatc.scenario.ScenarioService/HealthCheck
```

### Check HPA (Horizontal Pod Autoscaler)
```bash
# View HPA status
kubectl get hpa -n aiatc

# Describe HPA
kubectl describe hpa aiatc-scenario-service -n aiatc

# Expected: Current/Target CPU metrics
```

---

## Troubleshooting

### Scenario Service Won't Start
```bash
# Check logs
docker-compose logs scenario-service

# Common issues:
# 1. Can't connect to postgres-usage
#    - Wait for postgres to be healthy: docker-compose ps
#    - Check network: docker network ls
#
# 2. Can't connect to reference DB
#    - Verify host.docker.internal resolves
#    - Check reference DB is running on port 5430
#
# 3. Migration errors
#    - Check connection string in docker-compose.yml
#    - Try connecting manually with psql
```

### gRPC Calls Failing
```bash
# Check if service is listening
docker exec aiatc-scenario-service netstat -tulpn | grep 5001

# Check firewall
# Windows: Allow Docker in Windows Firewall

# Check Dapr sidecar
docker-compose logs dapr-scenario-sidecar

# Ensure reflection is enabled (should be by default)
```

### CORS Errors in Browser
```bash
# Check ScenarioService Program.cs has:
# - app.UseGrpcWeb()
# - app.UseCors() with proper policy
# - .EnableGrpcWeb() on MapGrpcService

# Verify in browser Network tab:
# - Access-Control-Allow-Origin header present
# - Grpc-Status header exposed
```

### Database Connection Issues
```bash
# Test postgres-usage
docker exec -it aiatc-postgres-usage psql -U aiatc -d aiatc_usage -c "SELECT 1;"

# Test reference DB from host
psql -h localhost -p 5430 -U arinc424 -d arinc424 -c "SELECT 1;"

# Check Docker DNS
docker exec aiatc-scenario-service nslookup postgres-usage
docker exec aiatc-scenario-service nslookup host.docker.internal
```

---

## Success Criteria

### ✅ All Tests Pass When:
1. ✅ All projects build without errors
2. ✅ Docker Compose services start and remain healthy
3. ✅ Database migrations apply successfully
4. ✅ gRPC health check returns "healthy"
5. ✅ Airport data retrieval from reference DB works
6. ✅ Dapr state store operations succeed
7. ✅ Web client connects via gRPC-Web
8. ✅ Scenario CRUD operations work correctly
9. ✅ Error handling returns proper gRPC status codes
10. ✅ (Optional) Kubernetes deployment succeeds

---

## Cleanup

### Stop Docker Compose
```bash
# Stop all services
docker-compose down

# Remove volumes (WARNING: deletes data)
docker-compose down -v

# Remove images
docker-compose down --rmi all
```

### Cleanup Kubernetes
```bash
# Uninstall Helm release
helm uninstall aiatc -n aiatc

# Delete namespace
kubectl delete namespace aiatc

# Delete Dapr (if needed)
dapr uninstall -k
```

---

## Next Steps

After successful testing:
1. Update README.md with deployment instructions
2. Create CI/CD pipeline for automated testing
3. Set up monitoring (Prometheus/Grafana)
4. Configure production secrets and credentials
5. Implement backup strategy for usage database
6. Performance tune based on load testing results

---

For issues or questions, see:
- Docker logs: `docker-compose logs -f [service-name]`
- Kubernetes logs: `kubectl logs [pod-name] -n aiatc`
- GitHub Issues: Create an issue with logs and error messages
