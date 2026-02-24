#!/usr/bin/env bash
# seed-airspace-db.sh
#
# Seeds the ARINC 424 cycle2508 schema from a local Docker container into
# Azure PostgreSQL, then updates the aiatc-scenario Container App connection string.
#
# PREREQUISITES:
#   - Docker must be running
#   - az CLI must be logged in (az login)
#   - Local ARINC 424 container must be running on port 5430
#
# USAGE:
#   ./seed-airspace-db.sh [OPTIONS]
#
# OPTIONS:
#   --resource-group  AIATC-RG name (default: aiatc-rg)
#   --container-app   Container App name (default: aiatc-scenario)
#   --target-db       Target database name (default: arinc424)
#   --target-schema   Target schema name (default: cycle2508)
#   --target-user     Azure admin user (default: aiatcadmin)
#   --target-password Azure admin password (default: AiAtcDb2026!)
#   --target-host     Azure PostgreSQL FQDN (auto-discovered if not set)
#   --dump-file       Path to dump file (default: same directory as script)
#   --skip-dump       Reuse existing dump file
#   --skip-restore    Dump only, no Azure restore
#   --skip-app-update Skip Container App env-var update
#
# EXAMPLES:
#   # Full run
#   ./seed-airspace-db.sh
#
#   # Reuse existing dump
#   ./seed-airspace-db.sh --skip-dump
#
#   # Dump only
#   ./seed-airspace-db.sh --skip-restore --skip-app-update

set -euo pipefail

# ─── Defaults ─────────────────────────────────────────────────────────────────

RESOURCE_GROUP="aiatc-rg"
CONTAINER_APP="aiatc-scenario"
TARGET_DB="arinc424"
TARGET_SCHEMA="cycle2508"
TARGET_USER="aiatcadmin"
TARGET_PASSWORD="AiAtcDb2026!"
TARGET_HOST=""

SOURCE_HOST="host.docker.internal"
SOURCE_PORT="5430"
SOURCE_DB="arinc424"
SOURCE_USER="arinc424"
SOURCE_PASSWORD='fly_@irline_RADA4!'
SOURCE_SCHEMA="cycle2508"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DUMP_FILE="$SCRIPT_DIR/arinc424_cycle2508.sql"

SKIP_DUMP=false
SKIP_RESTORE=false
SKIP_APP_UPDATE=false

# ─── Argument parsing ─────────────────────────────────────────────────────────

while [[ $# -gt 0 ]]; do
    case "$1" in
        --resource-group)   RESOURCE_GROUP="$2";   shift 2 ;;
        --container-app)    CONTAINER_APP="$2";    shift 2 ;;
        --target-db)        TARGET_DB="$2";        shift 2 ;;
        --target-schema)    TARGET_SCHEMA="$2";    shift 2 ;;
        --target-user)      TARGET_USER="$2";      shift 2 ;;
        --target-password)  TARGET_PASSWORD="$2";  shift 2 ;;
        --target-host)      TARGET_HOST="$2";      shift 2 ;;
        --dump-file)        DUMP_FILE="$2";        shift 2 ;;
        --skip-dump)        SKIP_DUMP=true;        shift   ;;
        --skip-restore)     SKIP_RESTORE=true;     shift   ;;
        --skip-app-update)  SKIP_APP_UPDATE=true;  shift   ;;
        *) echo "Unknown option: $1" >&2; exit 1 ;;
    esac
done

# ─── Helpers ──────────────────────────────────────────────────────────────────

cyan()   { printf "\n\033[36m==> %s\033[0m\n" "$*"; }
green()  { printf "    \033[32mOK: %s\033[0m\n" "$*"; }
yellow() { printf "    \033[33mWARN: %s\033[0m\n" "$*"; }
red()    { printf "    \033[31mERROR: %s\033[0m\n" "$*" >&2; }

die() { red "$*"; exit 1; }

require_cmd() {
    command -v "$1" >/dev/null 2>&1 || die "Required command '$1' not found in PATH."
}

# ─── Preflight checks ─────────────────────────────────────────────────────────

cyan "Preflight checks"

require_cmd docker
require_cmd az

docker info >/dev/null 2>&1 || die "Docker is not running. Please start Docker and try again."
green "Docker is running"

az account show >/dev/null 2>&1 || die "Not logged in to Azure CLI. Run 'az login' first."
green "Azure CLI is authenticated"

# ─── Step 1: Discover Azure PostgreSQL FQDN ───────────────────────────────────

cyan "Step 1: Discover Azure PostgreSQL host"

if [[ -z "$TARGET_HOST" ]]; then
    echo "    Querying Azure for PostgreSQL Flexible Server FQDN..."
    TARGET_HOST=$(az postgres flexible-server list \
        --resource-group "$RESOURCE_GROUP" \
        --query "[0].fullyQualifiedDomainName" -o tsv 2>/dev/null)
    [[ -n "$TARGET_HOST" ]] || die "Failed to discover Azure PostgreSQL FQDN. Ensure the server exists in resource group '$RESOURCE_GROUP'."
    green "Discovered host: $TARGET_HOST"
else
    green "Using provided host: $TARGET_HOST"
fi

# ─── Step 2: Dump cycle2508 schema from local container ───────────────────────

cyan "Step 2: Dump '$SOURCE_SCHEMA' schema from local container"

if [[ "$SKIP_DUMP" == true && -f "$DUMP_FILE" ]]; then
    yellow "--skip-dump specified and '$DUMP_FILE' exists — skipping dump."
else
    if [[ "$SKIP_DUMP" == true ]]; then
        yellow "--skip-dump specified but '$DUMP_FILE' does not exist — running dump anyway."
    fi

    echo "    Dump destination: $DUMP_FILE"
    echo "    Source: $SOURCE_USER@$SOURCE_HOST:$SOURCE_PORT/$SOURCE_DB (schema: $SOURCE_SCHEMA)"

    DUMP_DIR="$(dirname "$DUMP_FILE")"
    DUMP_NAME="$(basename "$DUMP_FILE")"

    docker run --rm \
        -e PGPASSWORD="$SOURCE_PASSWORD" \
        -v "$DUMP_DIR:/mnt" \
        postgres:17-alpine \
        pg_dump \
            -h "$SOURCE_HOST" \
            -p "$SOURCE_PORT" \
            -U "$SOURCE_USER" \
            -d "$SOURCE_DB" \
            -n "$SOURCE_SCHEMA" \
            --no-owner \
            --no-privileges \
            --clean \
            --if-exists \
            -F p \
            -f "/mnt/$DUMP_NAME" \
    || die "pg_dump failed. Check that the local ARINC 424 container is running on port $SOURCE_PORT."

    DUMP_SIZE_MB=$(awk "BEGIN { printf \"%.1f\", $(stat -c%s "$DUMP_FILE" 2>/dev/null || stat -f%z "$DUMP_FILE") / 1048576 }")
    green "Dump complete: $DUMP_FILE (${DUMP_SIZE_MB} MB)"
fi

if [[ "$SKIP_RESTORE" == true ]]; then
    yellow "--skip-restore specified — stopping after dump."
    exit 0
fi

# ─── Step 3: Create arinc424 database on Azure (idempotent) ───────────────────

cyan "Step 3: Create '$TARGET_DB' database on Azure PostgreSQL (if not exists)"

docker run --rm \
    -e PGPASSWORD="$TARGET_PASSWORD" \
    postgres:17-alpine \
    psql "host=$TARGET_HOST port=5432 dbname=postgres user=$TARGET_USER sslmode=require" \
    -c "SELECT 'CREATE DATABASE $TARGET_DB' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname='$TARGET_DB')\gexec" \
|| die "Failed to create database '$TARGET_DB' on '$TARGET_HOST'. Check credentials and network access."

green "Database '$TARGET_DB' is ready"

# ─── Step 4: Restore dump to Azure ────────────────────────────────────────────

cyan "Step 4: Restore dump to '$TARGET_HOST/$TARGET_DB'"

echo "    This may take several minutes for large datasets..."

docker run --rm -i \
    -e PGPASSWORD="$TARGET_PASSWORD" \
    postgres:17-alpine \
    psql "host=$TARGET_HOST port=5432 dbname=$TARGET_DB user=$TARGET_USER sslmode=require" \
< "$DUMP_FILE" \
|| die "psql restore failed. The dump may have been partially applied — check the output above."

green "Schema '$TARGET_SCHEMA' restored to '$TARGET_DB' on Azure"

# ─── Step 5: Verify row counts ────────────────────────────────────────────────

cyan "Step 5: Verify restore (spot-check row counts)"

ROW_COUNT_SQL="SELECT table_name, (xpath('/row/cnt/text()', query_to_xml(
    format('SELECT COUNT(*) AS cnt FROM %I.%I', table_schema, table_name), true, true, ''
)))[1]::text::int AS row_count
FROM information_schema.tables
WHERE table_schema = '$TARGET_SCHEMA' AND table_type = 'BASE TABLE'
ORDER BY table_name;"

docker run --rm \
    -e PGPASSWORD="$TARGET_PASSWORD" \
    postgres:17-alpine \
    psql "host=$TARGET_HOST port=5432 dbname=$TARGET_DB user=$TARGET_USER sslmode=require" \
    -c "$ROW_COUNT_SQL" \
&& green "Row count verification complete" \
|| yellow "Row count verification failed — non-fatal. Check the database manually."

# ─── Step 6: Update Container App environment variable ────────────────────────

if [[ "$SKIP_APP_UPDATE" == true ]]; then
    yellow "--skip-app-update specified — skipping Container App update."
else
    cyan "Step 6: Update '$CONTAINER_APP' Container App connection string"

    CONN_STR="Host=$TARGET_HOST;Port=5432;Database=$TARGET_DB;Username=$TARGET_USER;Password=$TARGET_PASSWORD;Search Path=$TARGET_SCHEMA;Ssl Mode=Require"

    echo "    Updating ConnectionStrings__AirspaceDb on '$CONTAINER_APP'..."

    az containerapp update \
        --name "$CONTAINER_APP" \
        --resource-group "$RESOURCE_GROUP" \
        --set-env-vars "ConnectionStrings__AirspaceDb=$CONN_STR" \
    || die "Failed to update Container App '$CONTAINER_APP'. The database is seeded but the app still uses the old connection string. Update it manually."

    green "Container App '$CONTAINER_APP' updated — new revision will be created automatically"

    # ─── Step 7: Wait for new revision ────────────────────────────────────────

    cyan "Step 7: Wait for new revision to become active"

    echo "    Waiting 30 seconds for new revision to start..."
    sleep 30

    LATEST=$(az containerapp revision list \
        --name "$CONTAINER_APP" \
        --resource-group "$RESOURCE_GROUP" \
        --query "sort_by([].{name:name, created:properties.createdTime, state:properties.runningState}, &created)[-1]" \
        -o json 2>/dev/null || echo "{}")

    if [[ "$LATEST" != "{}" ]]; then
        REV_NAME=$(echo "$LATEST" | grep -o '"name":"[^"]*"' | head -1 | cut -d'"' -f4)
        REV_STATE=$(echo "$LATEST" | grep -o '"state":"[^"]*"' | head -1 | cut -d'"' -f4)
        echo "    Latest revision: $REV_NAME"
        echo "    State:           $REV_STATE"

        if [[ "$REV_STATE" == "Running" ]]; then
            green "New revision is running"
        else
            yellow "Revision state is '$REV_STATE' — may still be starting. Check with:"
            yellow "  az containerapp revision list --name $CONTAINER_APP --resource-group $RESOURCE_GROUP"
        fi
    else
        yellow "Could not query revision list — check manually."
    fi
fi

# ─── Summary ──────────────────────────────────────────────────────────────────

printf "\n\033[32m========================================\033[0m\n"
printf "\033[32m  ARINC 424 seeding complete!\033[0m\n"
printf "\033[32m========================================\033[0m\n\n"
echo "Next steps:"
echo "  1. Check health endpoint: https://<your-app-domain>/health"
echo "     - 'airspace_db' should now be Healthy"
echo "  2. Test in browser: load KSFO — should show full ARINC 424 runway data"
echo "     (not just the 4 well-known fallback runways)"
echo ""
echo "To verify manually:"
echo "  az containerapp revision list --name $CONTAINER_APP --resource-group $RESOURCE_GROUP"
echo ""
