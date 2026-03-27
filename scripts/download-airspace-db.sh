#!/usr/bin/env bash
# Downloads the ARINC 424 cycle 2508 SQL dump from Azure Blob Storage.
# Requires: az CLI, logged in with access to the aiatcdata storage account.
#
# Usage:
#   ./scripts/download-airspace-db.sh
#
# The downloaded file is used by docker-compose to initialize the
# postgres-airspace container on first run.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_FILE="$SCRIPT_DIR/arinc424_cycle2508.sql"
STORAGE_ACCOUNT="aiatcdata"
CONTAINER="airspace-data"
BLOB_NAME="arinc424_cycle2508.sql"

if [[ -f "$OUTPUT_FILE" ]]; then
    echo "File already exists: $OUTPUT_FILE"
    echo "Delete it first if you want to re-download."
    exit 0
fi

echo "Downloading $BLOB_NAME from Azure Blob Storage..."
az storage blob download \
    --account-name "$STORAGE_ACCOUNT" \
    --container-name "$CONTAINER" \
    --name "$BLOB_NAME" \
    --file "$OUTPUT_FILE" \
    --auth-mode key \
    --no-progress \
    --output none

echo "Downloaded to $OUTPUT_FILE ($(du -h "$OUTPUT_FILE" | cut -f1))"
