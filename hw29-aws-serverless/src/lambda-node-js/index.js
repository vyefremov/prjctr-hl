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
        
        // const r = await s3.getObject(params).promise();
        
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
