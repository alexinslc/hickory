#!/usr/bin/env bash
set -euo pipefail

# Database backup script for Hickory Help Desk
# Usage: ./scripts/backup.sh [backup_dir]
#
# Environment variables:
#   PGHOST     - PostgreSQL host (default: localhost)
#   PGPORT     - PostgreSQL port (default: 5432)
#   PGUSER     - PostgreSQL user (default: postgres)
#   PGPASSWORD - PostgreSQL password
#   PGDATABASE - Database name (default: hickory)
#   BACKUP_DIR - Backup directory (default: ./backups)
#   BACKUP_RETENTION - Number of backups to keep (default: 7)

BACKUP_DIR="${1:-${BACKUP_DIR:-./backups}}"
BACKUP_RETENTION="${BACKUP_RETENTION:-7}"
PGHOST="${PGHOST:-localhost}"
PGPORT="${PGPORT:-5432}"
PGUSER="${PGUSER:-postgres}"
PGDATABASE="${PGDATABASE:-hickory}"

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/hickory_${TIMESTAMP}.sql.gz"

mkdir -p "$BACKUP_DIR"

echo "Starting backup of ${PGDATABASE}@${PGHOST}:${PGPORT}..."

pg_dump -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$PGDATABASE" \
  --format=custom --compress=6 --verbose \
  -f "${BACKUP_DIR}/hickory_${TIMESTAMP}.dump" 2>&1

BACKUP_FILE="${BACKUP_DIR}/hickory_${TIMESTAMP}.dump"
BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)

echo "Backup complete: ${BACKUP_FILE} (${BACKUP_SIZE})"

# Enforce retention policy
BACKUP_COUNT=$(ls -1 "${BACKUP_DIR}"/hickory_*.dump 2>/dev/null | wc -l)
if [ "$BACKUP_COUNT" -gt "$BACKUP_RETENTION" ]; then
  REMOVE_COUNT=$((BACKUP_COUNT - BACKUP_RETENTION))
  echo "Removing ${REMOVE_COUNT} old backup(s) (keeping last ${BACKUP_RETENTION})..."
  ls -1t "${BACKUP_DIR}"/hickory_*.dump | tail -n "$REMOVE_COUNT" | xargs rm -f
fi

echo "Done. ${BACKUP_COUNT} backup(s) in ${BACKUP_DIR}."
