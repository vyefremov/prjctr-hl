# Homework: CI/CD

## Task

## Solution

Setup AWS EC2 instance with Nginx and Docker installed.

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

```bash
# Add webapi.conf to Nginx
sudo nano /etc/nginx/sites-available/webapi.conf
sudo ln -s /etc/nginx/sites-available/webapi.conf /etc/nginx/sites-enabled/

# Test Nginx configuration
sudo nginx -t
```

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

```bash
docker pull mongo; docker run -d -p 27017:27017 --name mongodb mongo

docker pull vladyslavyefremov/hw25-webapi:0.1.0-hw25-ci-cd-run7.1
docker run -d -p 8080:8080 vladyslavyefremov/hw25-webapi:0.1.0-hw25-ci-cd-run7.1 --name hw25-webapi
```
