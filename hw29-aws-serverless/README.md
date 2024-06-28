# Homework: AWS Serverless Calculations

## Task

Create Lambda function that will convert JPEG to BMP, GIF, PNG.
When user uploads JPEG file to S3 bucket, the Lambda function should be triggered and convert the file to BMP, GIF, PNG and save them to appropriate S3 buckets.

## Solution

1. Create 4 S3 buckets for each file type: `jpeg`, `bmp`, `gif`, `png`.
    1. Apply the terraform script: [terraform](./terraform/main.tf)
2. Create Lambda function
   1. Add trigger for S3 bucket `jpeg` on created object events
   2. Add read and write permissions for Lambda function to access S3 buckets (via IAM role)
3. Prepare Lambda function code
   1. Code is written in JavaScript and uses `jimp` library for image processing
   2. Code is located in [./src/lambda-node-js](./src/lambda-node-js) folder
4. Deploy Lambda function
   1. Zip the code `zip -r9 ../hw29-lambda.zip .`
   2. Upload the zip file to AWS Lambda
5. Test the solution
   1. Upload JPEG file to S3 bucket `hw29-jpeg`
   2. Check if BMP, GIF, PNG files are created in appropriate S3 buckets
   3. Check the CloudWatch logs for execution details

## Code

```javascript
const AWS = require('aws-sdk');
const Jimp = require('jimp');

const s3 = new AWS.S3();

const pngBucket = 'hw29-png';
const bmpBucket = 'hw29-bmp';
const gifBucket = 'hw29-gif';

exports.handler = async (event) => {
    try {
        // Get the file from the S3 bucket
        const record = event.Records[0];
        const sourceBucket = record.s3.bucket.name;

        console.log('Processing file (original name):', record.s3.object.key);

        const filename = decodeURIComponent(record.s3.object.key.replace(/\+/g, ' '));

        console.log("Processing file:", filename);

        const params = {
            Bucket: sourceBucket,
            Key: filename
        };

        console.log('Reading file from S3:', params);

        const viewUrl = await s3.getSignedUrlPromise("getObject", {
            Bucket: sourceBucket,
            Key: record.s3.object.key,
            Expires: 600
        });

        console.log(`File is located at: ${viewUrl}. Jimp reading image...`);

        const image = await Jimp.read(viewUrl);

        console.log('Jimp Image read successfully.');

        // Convert JPEG to PNG
        const pngBuffer = await image.getBufferAsync(Jimp.MIME_PNG);
        await uploadToS3(pngBucket, filename.replace('.jpeg', '.png'), pngBuffer);

        // Convert JPEG to BMP
        const bmpBuffer = await image.getBufferAsync(Jimp.MIME_BMP);
        await uploadToS3(bmpBucket, filename.replace('.jpg', '.bmp'), bmpBuffer);

        // Convert JPEG to GIF
        const gifBuffer = await image.getBufferAsync(Jimp.MIME_GIF);
        await uploadToS3(gifBucket, filename.replace('.jpg', '.gif'), gifBuffer);

        console.log(`File processed successfully: ${filename}`);
        return `File processed successfully: ${filename}`;

    } catch (error) {
        console.log('Error processing file:', error);
        return 'Error processing file.';
    }
}

async function uploadToS3(bucket, key, buffer) {
    const params = {
        Bucket: bucket,
        Key: key,
        Body: buffer
    };

    console.log('Uploading file to S3:', { bucket: params.Bucket, key: params.Key });

    await s3.upload(params).promise();

    console.log('File uploaded successfully:', key);
}
```
