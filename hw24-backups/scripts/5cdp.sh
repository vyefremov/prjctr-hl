#!/bin/bash

BACKUP_DIR="./backup/cdp"
BACKUP_PATH="$BACKUP_DIR/oplog.json"
TIMESTAMP=$(date +%s)
LATEST_TS_PATH="$BACKUP_DIR/latest_ts.txt"

mkdir $BACKUP_DIR

latest_ts=$(cat $LATEST_TS_PATH)

if [ -z "$latest_ts" ]; then
  latest_ts=0
fi

if [ ! -e "$BACKUP_PATH" ]; then
    touch "$BACKUP_PATH"
fi

TEMP_FILE=$(mktemp)

mongoexport --db local \
 --collection oplog.rs \
 --query "{ \"ts\": { \"\$gt\": { \"\$timestamp\": { \"t\": $latest_ts, \"i\": 1 } } } }" > "$TEMP_FILE"
 
cat "$TEMP_FILE" >> $BACKUP_PATH
rm "$TEMP_FILE"

echo "$TIMESTAMP" > $LATEST_TS_PATH
