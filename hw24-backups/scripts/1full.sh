#!/bin/bash

BACKUP_DIR="./backup/full"
TIMESTAMP=$(date +%s)
BACKUP_PATH="$BACKUP_DIR/$TIMESTAMP"

echo "Starting full backup..."
echo "Backup path: $BACKUP_PATH"

# Create a full backup
mongodump --out "$BACKUP_PATH"

echo "Full backup completed!"
