#!/bin/bash

BACKUP_DIR="./backup/reverse-delta"
TIMESTAMP=$(date +%s)

mkdir "$BACKUP_DIR"

echo "Starting reverse delta backup..."
echo "Making full backup..."

mongodump -v --out "$BACKUP_DIR/full-backup" \
 --collection oplog.rs \
 --db local
 
echo "Making reverse delta backup..."

TIMESTAMP_24h_ago=$((TIMESTAMP - 86400))

mongodump -v --out "$BACKUP_DIR/$TIMESTAMP_24h_ago-$TIMESTAMP" \
 --collection oplog.rs \
 --db local \
 --query "{ \"ts\": { \"\$gte\": { \"\$timestamp\": { \"t\": $TIMESTAMP_24h_ago, \"i\": 1 } } } }"
