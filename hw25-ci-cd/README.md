# Homework: CI/CD

## Task

Setup CI/CD for your pet project.

## Solution

1. Prepare WebAPI application.
2. Prepare Unit and Integration Tests. Integration tests should use real instance of MongoDB.
3. Setup AWS EC2 instance with Nginx and Docker installed.
4. Setup GitHub Actions for CI/CD.

### Actions

Actions are defined in the [.github/workflows/hw25-ci-cd.yml](../.github/workflows/hw25-ci-cd.yml) file.
It contains the following jobs:

#### Version

This job calculates the version of the application based on the number of commits in the repository and branch name.

> This version is later used in the Docker image tag.

#### Unit Tests

This job runs the unit tests of the application. Unit Tests are located in the [unit-tests/AnalyticsEventSummaryBuilderTests.cs](./src/unit-tests/AnalyticsEventSummaryBuilderTests.cs) file.

#### Integration Tests

This job runs the integration tests of the application. Integration Tests are located in the [integration-tests/AnalyticsEndpointsTests.cs](./src/integration-tests/AnalyticsEndpointsTests.cs) file.

It also spins up a MongoDB container for the tests. And uses .NET `WebApplicationFactory` to simulate test server.

#### Build and Push Docker Image

1. Builds the Docker image based on the [Dockerfile](./src/webapi/Dockerfile).
2. Pushes the Docker image to the Docker Hub: [vladyslavyefremov/hw25-webapi](https://hub.docker.com/repository/docker/vladyslavyefremov/hw25-webapi/general).

#### Deploy to Staging

This job connects to the AWS EC2 instance via SSH, stops the running Docker container, pulls the latest Docker image, and starts the container with new version.

```bash
docker stop hw25-webapi || true
docker rm hw25-webapi || true
docker pull ${{ secrets.DOCKERHUB_USERNAME }}/hw25-webapi:${{ env.VERSION }}
docker run -d --name hw25-webapi -p 8080:8080 ${{ secrets.DOCKERHUB_USERNAME }}/hw25-webapi:${{ env.VERSION }}
```

#### Post Deployment Tests

This jobs runs `GET` [/api/v1/version](http://ec2-13-60-41-136.eu-north-1.compute.amazonaws.com/api/v1/version) predefined API endpoint to check if the deployment was successful and the version is updated.

---

## Additional Setup Steps

#### Setup AWS EC2 instance with Nginx and Docker installed

```bash
# Root user
sudo -i

# Update and install Nginx
apt update
apt install nginx
systemctl start nginx
systemctl status nginx
systemctl enable nginx

# Install Docker
sudo apt install apt-transport-https ca-certificates curl software-properties-common
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
sudo apt update
sudo apt install docker-ce
systemctl status docker
systemctl enable docker
```

#### Configure Nginx

```bash
# Add webapi.conf to Nginx
sudo nano /etc/nginx/sites-available/default

# Update the configuration...

# Test Nginx configuration
sudo nginx -t
```

#### Nginx Configuration

```
    server {
        listen 80;

        location /api {
            proxy_pass         http://localhost:8080;
            proxy_redirect     off;
            proxy_set_header   Host $host;
            proxy_set_header   X-Real-IP $remote_addr;
            proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header   X-Forwarded-Host $server_name;
        }
    }
```

#### Docker Commands

```bash
# Start MongoDB
docker pull mongo; docker run -d -p 27017:27017 --name mongodb mongo

# Add ssh user to docker group
sudo usermod -aG docker ubuntu 

docker pull vladyslavyefremov/hw25-webapi:0.1.0-hw25-ci-cd-run8.1
docker run -d -p 8080:8080 vladyslavyefremov/hw25-webapi:0.1.0-hw25-ci-cd-run8.1 --name hw25-webapi
```
