#!/bin/bash

BACKUP_DIR="./backup/incremental"
TIMESTAMP=$(date +%s)
BACKUP_PATH="$BACKUP_DIR/$TIMESTAMP"
LATEST_BACKUP_TS_PATH="$BACKUP_DIR/latest_ts.txt"

echo "Starting incremental backup..."
echo "Backup path: $BACKUP_PATH"

latest_ts=$(cat $LATEST_BACKUP_TS_PATH)

if [ -z "$latest_ts" ]; then
  latest_ts=0
fi

echo "Latest backup timestamp: $latest_ts"

mongodump -v --out "$BACKUP_PATH" --collection oplog.rs --db local --query "{ \"ts\": { \"\$gte\": { \"\$timestamp\": { \"t\": $latest_ts, \"i\": 1 } } } }"

echo "$TIMESTAMP" > $LATEST_BACKUP_TS_PATH

