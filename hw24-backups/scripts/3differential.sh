#!/bin/bash

BACKUP_DIR="./backup/differential"
TIMESTAMP=$(date +%s)
LATEST_FULL_BACKUP_TS_PATH="$BACKUP_DIR/latest_full_ts.txt"

echo "Starting differential backup..."

latest_full_ts=$(cat $LATEST_FULL_BACKUP_TS_PATH)

# If there is no full backup yet, need to create one
if [ -z "$latest_full_ts" ]; then
  latest_full_ts=0
  BACKUP_PATH="$BACKUP_DIR/$TIMESTAMP-full"
  echo "Creating full backup..."
  echo "$TIMESTAMP" > $LATEST_FULL_BACKUP_TS_PATH
else
  BACKUP_PATH="$BACKUP_DIR/$latest_full_ts-$TIMESTAMP"
fi

echo "Backup path: $BACKUP_PATH"

mongodump -v --out "$BACKUP_PATH" \
 --collection oplog.rs \
 --db local \
 --query "{ \"ts\": { \"\$gte\": { \"\$timestamp\": { \"t\": $latest_full_ts, \"i\": 1 } } } }"
