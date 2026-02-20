#!/usr/bin/env pwsh
# Start PostgreSQL container on port 4360 (Development)

Write-Host "Starting PostgreSQL on port 4360 (Development)..." -ForegroundColor Green
docker-compose up -d postgres

Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

$containerName = docker ps --filter "name=postgres" --format "{{.Names}}" | Select-Object -First 1

if ($containerName) {
    Write-Host "Checking PostgreSQL health..." -ForegroundColor Yellow
    docker exec -it $containerName pg_isready -U aiatc
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "PostgreSQL is ready on port 4360!" -ForegroundColor Green
        Write-Host "Connection string: Host=localhost;Port=4360;Database=aiatc;Username=aiatc;Password=aiatc_dev_password" -ForegroundColor Cyan
    } else {
        Write-Host "PostgreSQL is starting up. Please wait a moment and try again." -ForegroundColor Yellow
    }
} else {
    Write-Host "PostgreSQL container not found. Please check Docker." -ForegroundColor Red
}
