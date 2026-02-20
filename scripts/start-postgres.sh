#!/bin/bash
# Start PostgreSQL container on port 4360 (Development)

echo -e "\033[0;32mStarting PostgreSQL on port 4360 (Development)...\033[0m"
docker-compose up -d postgres

echo -e "\033[0;33mWaiting for PostgreSQL to be ready...\033[0m"
sleep 5

CONTAINER_NAME=$(docker ps --filter "name=postgres" --format "{{.Names}}" | head -n 1)

if [ ! -z "$CONTAINER_NAME" ]; then
    echo -e "\033[0;33mChecking PostgreSQL health...\033[0m"
    docker exec -it $CONTAINER_NAME pg_isready -U aiatc
    
    if [ $? -eq 0 ]; then
        echo -e "\033[0;32mPostgreSQL is ready on port 4360!\033[0m"
        echo -e "\033[0;36mConnection string: Host=localhost;Port=4360;Database=aiatc;Username=aiatc;Password=aiatc_dev_password\033[0m"
    else
        echo -e "\033[0;33mPostgreSQL is starting up. Please wait a moment and try again.\033[0m"
    fi
else
    echo -e "\033[0;31mPostgreSQL container not found. Please check Docker.\033[0m"
fi
