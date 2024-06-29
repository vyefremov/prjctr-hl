# Homework: Balancing

## Task

Set up load balancer on nginx that will have:
- `1` server for `UA`
- `2` servers for `US`
- `1` server for the rest of the world
- In case of failure, it should send all traffic to the `backup` server

Health check should happen every 5 seconds.

## Solution

1. Setup Nginx
   1. Setup `ngx_http_geoip2_module`
   2. Download `GeoLite2` database
   3. Configure servers, upstreams, and usage of geoip2 module
   4. Configure usage of real ip from docker network
2. Use ngrok and vpn for testing

### GeoIP

Nginx is built with `ngx_http_geoip2_module` and `libmaxminddb` to determine the country of the client. It also uses `GeoLite2` database from `MaxMind`.

The setup is done in [Dockefile](./nginx/Dockerfile) and [docker-compose.yml](./nginx/docker-compose.yml).

After numerous attempts to build the correct image, I resorted to manually mounting the `ngx_http_geoip2_module.so` that I had built myself as a volume within the container. The Dockerfile was not configured correctly, and I encountered issues where `ngx_http_geoip2_module.so` was not copied to the target directory during the docker build process.

### Nginx Configuration

```nginx
load_module /usr/lib/nginx/modules/ngx_http_geoip2_module.so;
events {
    worker_connections  1024;
}
http {
    # Trust the X-Forwarded-For header set by Docker Desktop
    set_real_ip_from 192.168.65.1; # Docker Desktop default network
    real_ip_header X-Forwarded-For;
    real_ip_recursive on;

    geoip2 /etc/GeoLite2-Country.mmdb {
        # auto_reload 5m;
        $geoip2_metadata_country_build metadata build_epoch;
        $geoip2_data_country_code default=US source=$remote_addr country iso_code;
    }
    
    map $geoip2_data_country_code $backend {
        default default_server;
        US usa_server;
        UA ua_server;
    }
    
    upstream ua_server {
        server 127.0.0.1:8090;
    }
    
    upstream usa_server {
        server 127.0.0.1:8095;
        server 127.0.0.1:8096;
    }
    
    upstream default_server {
        server 127.0.0.1:8085;
    }
    
    upstream backup_server {
        server 127.0.0.1:8086;
    }

    server {
        listen 8080;
        
        # Disable caching
        add_header Cache-Control "no-store, no-cache, must-revalidate, proxy-revalidate, max-age=0";
        add_header Pragma "no-cache";
        add_header Expires "0";
        
        location / {
            proxy_pass http://$backend;
            proxy_next_upstream error timeout invalid_header http_500 http_502 http_503 http_504;
        
            # If all ua_servers fail, try backup_upstream
            error_page 502 503 = @fallback;
        }
        
        location @fallback {
            proxy_pass http://backup_server;
        }
    }
    
    server {
        listen 8090;
        
        location / {
            return 200 "UA";
            # return 503 "UA FAILED"; # Test Error
        }
    }
    
    server {
        listen 8095;
        
        location / {
            return 200 "USA 1";
        }
    }
    
    server {
        listen 8096;
        
        location / {
            return 200 "USA 2";
        }
    }
    
    server {
        listen 8085;
        
        location / {
            return 200 "Hey there [${geoip2_data_country_code}]!";
        }
    }
    
    server {
        listen 8086;
        
        location / {
            return 200 "Hey there, I'm backup [${geoip2_data_country_code}]!";
        }
    }
}
```

### Ngrok

```bash
docker pull ngrok/ngrok
docker run --net=host -it -e NGROK_AUTHTOKEN=<token> ngrok/ngrok:latest http 8080
```

### Health Check

I've tried using [nginx_upstream_check_module](https://github.com/yaoweibin/nginx_upstream_check_module/), but I couldn't get them to work.

Installation documentation among other steps says:

```bash
patch -p1 < /path/to/nginx_http_upstream_check_module/check.patch
```

But it fails with the Nginx versions I tried.
