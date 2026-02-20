# AI-ATC Deployment Verification Script
# Phase 8: Automated Testing

param(
    [switch]$SkipBuild,
    [switch]$SkipDocker,
    [switch]$Verbose
)

$ErrorActionPreference = "Continue"
$testsPassed = 0
$testsFailed = 0

function Write-TestHeader {
    param([string]$Title)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host " $Title" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Details = ""
    )

    if ($Passed) {
        Write-Host "[PASS]" -ForegroundColor Green -NoNewline
        Write-Host " $TestName" -ForegroundColor White
        if ($Details -and $Verbose) {
            Write-Host "       $Details" -ForegroundColor Gray
        }
        $script:testsPassed++
    } else {
        Write-Host "[FAIL]" -ForegroundColor Red -NoNewline
        Write-Host " $TestName" -ForegroundColor White
        if ($Details) {
            Write-Host "       $Details" -ForegroundColor Yellow
        }
        $script:testsFailed++
    }
}

# Test 1: Build Verification
Write-TestHeader "Test 1: Build Verification"

if (-not $SkipBuild) {
    Write-Host "Building solution..." -ForegroundColor Yellow

    $buildOutput = dotnet build --no-incremental 2>&1
    $buildSuccess = $LASTEXITCODE -eq 0

    Write-TestResult -TestName "Solution builds successfully" -Passed $buildSuccess `
        -Details "Exit code: $LASTEXITCODE"

    if ($buildSuccess) {
        # Count projects
        $projectCount = ($buildOutput | Select-String "-> .*\.dll").Count
        Write-TestResult -TestName "All projects compiled" -Passed ($projectCount -gt 0) `
            -Details "$projectCount projects built"
    }
} else {
    Write-Host "Skipping build tests (--SkipBuild specified)" -ForegroundColor Yellow
}

# Test 2: Project Structure
Write-TestHeader "Test 2: Project Structure"

$requiredProjects = @(
    "src\AIATC.ReferenceData\AIATC.ReferenceData.csproj",
    "src\AIATC.ReferenceData.Context\AIATC.ReferenceData.Context.csproj",
    "src\AIATC.ScenarioService.Data\AIATC.ScenarioService.Data.csproj",
    "src\AIATC.ScenarioService\AIATC.ScenarioService.csproj",
    "src\AIATC.Web\AIATC.Web.csproj"
)

foreach ($project in $requiredProjects) {
    $exists = Test-Path $project
    $projectName = Split-Path $project -Leaf
    Write-TestResult -TestName "Project exists: $projectName" -Passed $exists `
        -Details $project
}

# Test 3: Generated Files
Write-TestHeader "Test 3: Generated Files"

$generatedFiles = @(
    "src\AIATC.ReferenceData\Models\Airport.cs",
    "src\AIATC.ReferenceData.Context\AirspaceReferenceDbContext.cs",
    "src\AIATC.ScenarioService\Protos\scenario_service.proto",
    "src\AIATC.Web\Services\ScenarioServiceClient.cs"
)

foreach ($file in $generatedFiles) {
    $exists = Test-Path $file
    $fileName = Split-Path $file -Leaf
    Write-TestResult -TestName "Generated file exists: $fileName" -Passed $exists `
        -Details $file
}

# Test 4: Docker Configuration
Write-TestHeader "Test 4: Docker Configuration"

$dockerFiles = @(
    "docker-compose.yml",
    "src\AIATC.ScenarioService\Dockerfile",
    "Dockerfile"
)

foreach ($file in $dockerFiles) {
    $exists = Test-Path $file
    $fileName = Split-Path $file -Leaf
    Write-TestResult -TestName "Docker file exists: $fileName" -Passed $exists `
        -Details $file
}

# Test 5: Helm Configuration
Write-TestHeader "Test 5: Helm Configuration"

$helmFiles = @(
    "helm\aiatc\Chart.yaml",
    "helm\aiatc\values.yaml",
    "helm\aiatc\templates\deployment.yaml",
    "helm\aiatc\templates\scenario-service-deployment.yaml",
    "helm\aiatc\templates\scenario-service-service.yaml",
    "helm\aiatc\templates\secrets.yaml",
    "helm\aiatc\templates\dapr-components.yaml"
)

foreach ($file in $helmFiles) {
    $exists = Test-Path $file
    $fileName = Split-Path $file -Leaf
    Write-TestResult -TestName "Helm file exists: $fileName" -Passed $exists `
        -Details $file
}

# Test 6: Dapr Configuration
Write-TestHeader "Test 6: Dapr Configuration"

$daprFiles = @(
    "dapr\components\pubsub.yaml",
    "dapr\components\statestore.yaml",
    "dapr\config\config.yaml"
)

foreach ($file in $daprFiles) {
    $exists = Test-Path $file
    $fileName = Split-Path $file -Leaf
    Write-TestResult -TestName "Dapr file exists: $fileName" -Passed $exists `
        -Details $file
}

# Test 7: Docker Services (if running)
if (-not $SkipDocker) {
    Write-TestHeader "Test 7: Docker Services"

    Write-Host "Checking Docker status..." -ForegroundColor Yellow

    try {
        $dockerRunning = docker info 2>&1 | Select-String "Server Version"
        Write-TestResult -TestName "Docker daemon is running" -Passed ($null -ne $dockerRunning)

        if ($dockerRunning) {
            # Check if services are running
            $services = docker-compose ps --services 2>&1

            if ($LASTEXITCODE -eq 0) {
                $expectedServices = @(
                    "postgres-usage",
                    "redis",
                    "scenario-service",
                    "dapr-scenario-sidecar",
                    "dapr-placement"
                )

                foreach ($service in $expectedServices) {
                    $serviceRunning = docker-compose ps $service 2>&1 | Select-String "Up|running"
                    Write-TestResult -TestName "Service running: $service" `
                        -Passed ($null -ne $serviceRunning) `
                        -Details "Run 'docker-compose up -d' to start services"
                }
            } else {
                Write-Host "       Docker Compose not started. Run 'docker-compose up -d' to test." -ForegroundColor Yellow
            }
        }
    } catch {
        Write-TestResult -TestName "Docker daemon is accessible" -Passed $false `
            -Details "Error: $_"
    }
} else {
    Write-Host "Skipping Docker tests (--SkipDocker specified)" -ForegroundColor Yellow
}

# Test 8: Reference Database Connection
Write-TestHeader "Test 8: Reference Database"

Write-Host "Testing reference database connection..." -ForegroundColor Yellow

try {
    $env:PGPASSWORD = "fly_@irline_RADA4!"
    $dbTest = psql -h localhost -p 5430 -U arinc424 -d arinc424 -c "SELECT 1;" 2>&1
    $dbConnected = $LASTEXITCODE -eq 0

    Write-TestResult -TestName "Reference DB connection (port 5430)" -Passed $dbConnected `
        -Details "Database: arinc424, Schema: cycle2508"

    if ($dbConnected) {
        $tableCount = psql -h localhost -p 5430 -U arinc424 -d arinc424 `
            -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='cycle2508';" 2>&1

        Write-TestResult -TestName "Reference DB has tables" -Passed ($tableCount -gt 0) `
            -Details "Tables in cycle2508: $($tableCount.Trim())"
    }
} catch {
    Write-TestResult -TestName "Reference DB connection" -Passed $false `
        -Details "psql not found or connection failed. Install PostgreSQL client."
}

# Summary
Write-TestHeader "Test Summary"

$totalTests = $testsPassed + $testsFailed
$passRate = if ($totalTests -gt 0) { [math]::Round(($testsPassed / $totalTests) * 100, 1) } else { 0 }

Write-Host "Total Tests:  $totalTests" -ForegroundColor White
Write-Host "Passed:       " -NoNewline
Write-Host "$testsPassed" -ForegroundColor Green
Write-Host "Failed:       " -NoNewline
Write-Host "$testsFailed" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Red" })
Write-Host "Pass Rate:    $passRate%" -ForegroundColor $(if ($passRate -ge 90) { "Green" } elseif ($passRate -ge 70) { "Yellow" } else { "Red" })

Write-Host "`n========================================`n" -ForegroundColor Cyan

if ($testsFailed -eq 0) {
    Write-Host "All tests passed! System is ready for deployment." -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some tests failed. Review the output above for details." -ForegroundColor Yellow
    exit 1
}
