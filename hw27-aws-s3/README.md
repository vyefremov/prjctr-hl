# Homework: AWS S3

## Task

1. Create a new S3 bucket where objects can't be modified after they're uploaded.
2. Requests to the bucket must be logged.

## Solution

1. Create a new S3 bucket for logging — `hw27-aws-s3-logs`
2. Create a new S3 bucket — `hw27-aws-s3`
   1. Enable bucket logging for the `hw27-aws-s3` and configure `hw27-aws-s3-logs` as the target bucket
   2. Enable object lock for the `hw27-aws-s3` bucket

### Bucket Configuration

#### CLI Commands

```bash
# Get bucket logging configuration
aws s3api get-bucket-logging --bucket hw27-aws-s3 > bucket/bucket-logging.json

# Get bucket object lock configuration
aws s3api get-object-lock-configuration --bucket hw27-aws-s3 > bucket/bucket-object-lock.json

# Get bucket versioning configuration
aws s3api get-bucket-versioning --bucket hw27-aws-s3 > bucket/bucket-versioning.json
```

#### Bucket Logging Configuration

```json
{
  "LoggingEnabled": {
    "TargetBucket": "hw27-aws-s3-logs",
    "TargetPrefix": "",
    "TargetObjectKeyFormat": {
      "SimplePrefix": {}
    }
  }
}
```

#### Bucket Object Lock Configuration

```json
{
  "ObjectLockConfiguration": {
    "ObjectLockEnabled": "Enabled"
  }
}
```

#### Bucket Versioning Configuration

```json
{
  "Status": "Enabled",
  "MFADelete": "Disabled"
}
```

### Logging Check

```bash
# List objects in the logging bucket
aws s3api list-objects-v2 --bucket hw27-aws-s3-logs --query "sort_by(Contents, &LastModified)[-1].Key" --output text

# Get the object by key
aws s3api get-object --bucket hw27-aws-s3-logs --key "${todo_paste_latest_log_key}" bucket/latest-log.txt
```
