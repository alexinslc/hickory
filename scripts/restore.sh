#!/usr/bin/env bash
set -euo pipefail

# Database restore script for Hickory Help Desk
# Usage: ./scripts/restore.sh <backup_file>
#
# Environment variables:
#   PGHOST     - PostgreSQL host (default: localhost)
#   PGPORT     - PostgreSQL port (default: 5432)
#   PGUSER     - PostgreSQL user (default: postgres)
#   PGDATABASE - Database name (default: hickory)

if [ $# -eq 0 ]; then
  echo "Usage: $0 <backup_file>"
  echo ""
  echo "Available backups:"
  ls -lht backups/hickory_*.dump 2>/dev/null || echo "  No backups found in ./backups/"
  exit 1
fi

BACKUP_FILE="$1"
PGHOST="${PGHOST:-localhost}"
PGPORT="${PGPORT:-5432}"
PGUSER="${PGUSER:-postgres}"
PGDATABASE="${PGDATABASE:-hickory}"

if [ ! -f "$BACKUP_FILE" ]; then
  echo "Error: Backup file not found: ${BACKUP_FILE}"
  exit 1
fi

echo "WARNING: This will overwrite the database '${PGDATABASE}' on ${PGHOST}:${PGPORT}"
read -p "Are you sure? (yes/no): " CONFIRM

if [ "$CONFIRM" != "yes" ]; then
  echo "Restore cancelled."
  exit 0
fi

echo "Restoring from ${BACKUP_FILE}..."

pg_restore -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$PGDATABASE" \
  --clean --if-exists --verbose \
  "$BACKUP_FILE" 2>&1

echo "Restore complete."
