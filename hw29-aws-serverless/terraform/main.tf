provider "aws" {
  region = "eu-north-1"
}

resource "aws_s3_bucket" "hw29-jpeg-bucket" {
  bucket = "hw29-jpeg"
}

resource "aws_s3_bucket" "hw29-gif-bucket" {
  bucket = "hw29-gif"
}

resource "aws_s3_bucket" "hw29-bmp-bucket" {
  bucket = "hw29-bmp"
}

resource "aws_s3_bucket" "hw29-png-bucket" {
  bucket = "hw29-png"
}
