provider "aws" {
  region = "eu-north-1"
}

resource "aws_s3_bucket" "hw27-aws-s3-logs" {
  bucket = "hw27-aws-s3-logs"
}

resource "aws_s3_bucket" "hw27-aws-s3" {
  bucket = "hw27-aws-s3"
}

resource "aws_s3_bucket_ownership_controls" "hw27-aws-s3" {
  bucket = aws_s3_bucket.hw27-aws-s3.id
  
  rule {
    object_ownership = "BucketOwnerPreferred"
  }
}

resource "aws_s3_bucket_public_access_block" "hw27-aws-s3" {
  bucket = aws_s3_bucket.hw27-aws-s3.id
  
  block_public_acls = false
  block_public_policy = false
  ignore_public_acls = false
  restrict_public_buckets = false
}

resource "aws_s3_bucket_acl" "hw27-aws-s3" {
  depends_on = [
    aws_s3_bucket_ownership_controls.hw27-aws-s3,
    aws_s3_bucket_public_access_block.hw27-aws-s3
  ]
  bucket = aws_s3_bucket.hw27-aws-s3.id
  acl = "public-read"
}

# Enable logging when accessing "hw27-aws-s3" bucket
resource "aws_s3_bucket_logging" "hw27-aws-s3" {
  depends_on = [aws_s3_bucket.hw27-aws-s3]
  bucket = aws_s3_bucket.hw27-aws-s3.id
  target_bucket = aws_s3_bucket.hw27-aws-s3-logs.id
  target_prefix = "logs/"
}

# Enable versioning in order to enable object locking later
resource "aws_s3_bucket_versioning" "hw27-aws-s3" {
  depends_on = [aws_s3_bucket.hw27-aws-s3]
  bucket = aws_s3_bucket.hw27-aws-s3.id

  versioning_configuration {
    status = "Enabled"
  }
}

# Enable object locking (cannot modify or delete objects)
resource "aws_s3_bucket_object_lock_configuration" "hw27-aws-s3" {
  depends_on = [aws_s3_bucket.hw27-aws-s3, aws_s3_bucket_versioning.hw27-aws-s3]
  bucket = aws_s3_bucket.hw27-aws-s3.id

  object_lock_enabled = "Enabled"
}
