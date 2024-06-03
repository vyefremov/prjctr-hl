# L24 Homework: Backups

## Task
1. Take/create the database from your pet project
2. Implement all kinds of repository models (Full, Incremental, Differential, Reverse Delta, CDP - Continuous Data protection)
3. Compare their parameters: size, ability to roll back at specific time point, speed of roll back, cost of implementation and maintenance

## Solution

The database of choice is MongoDB. It's setup via [docker-compose](docker-compose.yml) as replica set to enable oplog collection for backups.

### Full Backup

>  A complete backup at a specific point in time. It's comprehensive but can be time and resource-intensive.

#### Script

```bash
BACKUP_DIR="./backup/full"
TIMESTAMP=$(date +%s)
BACKUP_PATH="$BACKUP_DIR/$TIMESTAMP"

mongodump --out "$BACKUP_PATH"
```

### Incremental Backup

> Only backs up data that has changed since the last backup. It's efficient in terms of storage space and time but requires all previous backups to fully restore data.

If we have a full backup on Sunday and incremental backups on Monday, Tuesday, and Wednesday, we need to restore the full backup and apply the incremental backups in order to restore the database to Wednesday:
- Sun:full
- Sun:full + Mon
- Sun:full + Mon + Tue
- Sun:full + Mon + Tue + Wed

#### Script

```bash
BACKUP_DIR="./backup/incremental"
TIMESTAMP=$(date +%s)
BACKUP_PATH="$BACKUP_DIR/$TIMESTAMP"
LATEST_BACKUP_TS_PATH="$BACKUP_DIR/latest_ts.txt"

latest_ts=$(cat $LATEST_BACKUP_TS_PATH)

# if latest_ts is empty, set it to 0
if [ -z "$latest_ts" ]; then
  latest_ts=0
fi

# Backup only the changes since the last backup (or since the beginning if no backup yet)
mongodump -v \
  --out "$BACKUP_PATH" \
  --collection oplog.rs \
  --db local \
  --query "{ \"ts\": { \"\$gte\": { \"\$timestamp\": { \"t\": $latest_ts, \"i\": 1 } } } }"

# Save the timestamp of the latest backup to use in the next incremental backup
echo "$TIMESTAMP" > $LATEST_BACKUP_TS_PATH
```

### Differential Backup

> Similar to incremental, but it backs up all changes since the last full backup. This means each differential backup contains all changes since the last full backup, making it easier to restore data compared to incremental backups.

If we have a full backup on Sunday and differential backups on Monday, Tuesday, and Wednesday, we need to restore the full backup and the one differential backup to restore the database to Thursday for instance:

- Sun:full
- Sun:full + Mon
- Sun:full + (Mon, and Tue)
- Sun:full + (Mon, Tue, and Wed)

```bash
BACKUP_DIR="./backup/differential"
TIMESTAMP=$(date +%s)
LATEST_FULL_BACKUP_TS_PATH="$BACKUP_DIR/latest_full_ts.txt"

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

# Backup only the changes since the last full backup (or since the beginning if no backup yet)
mongodump -v --out "$BACKUP_PATH" \
 --collection oplog.rs \
 --db local \
 --query "{ \"ts\": { \"\$gte\": { \"\$timestamp\": { \"t\": $latest_full_ts, \"i\": 1 } } } }"
```

With differential backups we can remember the timestamp of the last full backup (`latest_full_ts.txt`) and only backup the changes since that timestamp (MongoDB filter).

### Reverse Delta Backup

> Reverse delta backup is a backup of the diff changes since the last backup, but in reverse order. The algorithm is more complex and resource-intensive than incremental and differential backups, but it's more reliable and faster to restore.

```bash
BACKUP_DIR="./backup/reverse-delta"
TIMESTAMP=$(date +%s)

mkdir "$BACKUP_DIR"

# Full backup
mongodump -v --out "$BACKUP_DIR/full-backup" \
 --collection oplog.rs \
 --db local

TIMESTAMP_24h_ago=$((TIMESTAMP - 86400))

# Backup the changes since the last 24 hours (for instance) leveraging the oplog collection in MongoDB
mongodump -v --out "$BACKUP_DIR/$TIMESTAMP_24h_ago-$TIMESTAMP" \
 --collection oplog.rs \
 --db local \
 --query "{ \"ts\": { \"\$gte\": { \"\$timestamp\": { \"t\": $TIMESTAMP_24h_ago, \"i\": 1 } } } }"
```

### CDP — Continuous Data Protection

> CDP continuously captures changes and backs them up.

```bash
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

# Pull oplog changes since the last backup (or since the beginning if no backup yet) leveraging the oplog collection in MongoDB
mongoexport --db local \
 --collection oplog.rs \
 --query "{ \"ts\": { \"\$gt\": { \"\$timestamp\": { \"t\": $latest_ts, \"i\": 1 } } } }" > "$TEMP_FILE"
 
# Append the changes to the backup file and remove the temp file
cat "$TEMP_FILE" >> $BACKUP_PATH
rm "$TEMP_FILE"
 
echo "$TIMESTAMP" > $LATEST_TS_PATH
```

## Comparison & Conclusion

| Backup Type         | Size     | Ability to Roll Back at specific time point | Speed of Roll Back | Cost     |
|---------------------|----------|---------------------------------------------|--------------------|----------|
| Full                | Large    | Low                                         | High               | High     |
| Incremental Backup  | Small    | Moderate                                    | Slow               | Moderate |
| Differential Backup | Moderate | Moderate                                    | Moderate to Fast   | Moderate |
| Reverse Delta       | Moderate | Moderate                                    | Moderate to Fast   | Moderate |
| CDP                 | Large    | High                                        | Slow to Fast       | High     |

### Full Backup
- **Size**: Large as it contains many copies of the same data.
- **Ability to Roll Back at specific time point**: Low. Otherwise, we need to have huge number of full backups.
- **Speed of Roll Back**: It has the highest speed as it contains all the data in one backup.
- **Cost**: High due to the large size and resource requirements. But it's the easiest to implement.

### Incremental Backup
- **Size**: Small as it only contains the changes since the last backup.
- **Ability to Roll Back at specific time point**: Moderate. It's easier to achieve than full backups because of smaller incremental backups.
- **Speed of Roll Back**: Slow as we need to restore all previous backups to fully restore data.
- **Cost**: Low as it's efficient in terms of storage space and time. But it's more complex to implement.

### Differential Backup
- **Size**: Moderate as compared to incremental, it has copies of the same data.
- **Ability to Roll Back at specific time point**: Moderate. It's easier to achieve than full backups because of smaller differential backups.
- **Speed of Roll Back**: Moderate to Fast. It's faster than incremental backups as we only need to restore the full backup and the latest differential backup.
- **Cost**: Moderate. It's slightly more expensive than incremental backups but should be cheaper than full backups.

### Reverse Delta Backup
- **Size**: Small. Close to incremental backups.
- **Ability to Roll Back at specific time point**: Moderate. Similar to Differential backups and Incremental backups, we need to restore the full backup and the latest reverse delta backups.
- **Speed of Roll Back**: Moderate to Fast. It's more common to restore to the recent state, so it's faster than incremental backups.
- **Cost**: Moderate. It's complex to implement but should be efficient in terms of storage space and time.

### CDP — Continuous Data Protection

- **Size**: Large. As it operates with sequential changes, it can grow large quickly.
- **Ability to Roll Back at specific time point**: High. It's the most efficient in terms of rolling back to a specific time point.
- **Speed of Roll Back**: Slow to Fast. It depends on the specific time and format of the backup. CDP can leverage any backup type if needed.
- **Cost**: High. It's the most complex to implement, maintain, and it requires large storage space.
