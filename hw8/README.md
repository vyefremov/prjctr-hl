# HSA L8 Homework: Web Servers

## Task
> Configure nginx that will cache only images, that were requested at least twice. Add ability to drop nginx cache by request. You should drop cache for specific file only ( not all cache ).

## Setup

### Docker compose file

**Openresty** is used as a base image for nginx. As it adds some additional features to nginx, like **lua** support.

```yaml
services: 
  nginx: 
    image: openresty/openresty:alpine
    ports: 
      - "8080:8080"
    volumes: 
      - ./nginx/nginx.conf:/usr/local/openresty/nginx/conf/nginx.conf
      - ./nginx/logs:/var/log/nginx
      - ./nginx/script/purge.lua:/var/www/script/purge.lua
      - ./static:/var/www/static
```

### Nginx configuration

```nginx
error_log /var/log/nginx/error.log debug;
events { }
http {
    proxy_cache_path /var/www/cache levels=1:2 keys_zone=STATIC:10m inactive=24h max_size=1g;
    
    server {
        listen 8080;
        
        # Setup for images
        location ~ /static/.*\.(png|jpg|jpeg)$ {	
            # If request is PURGE, then run purge.lua script
            if ($request_method = PURGE) { 
                set $lua_purge_path "/var/www/cache/";
                set $lua_purge_levels "1:2";
                set $lua_purge_upstream "http://localhost:8081";

                content_by_lua_file /var/www/script/purge.lua;
            }
            proxy_buffering on;
            proxy_cache STATIC;
            proxy_cache_min_uses 2;
            proxy_cache_valid any 30m; # does not work without this setting
            proxy_ignore_headers Set-Cookie; 
            proxy_ignore_headers Cache-Control; 
            proxy_pass http://localhost:8081;
            add_header X-Cache-Status $upstream_cache_status;
        }
        
        # Setup other static files than images
        location /static/ {	
            proxy_pass http://localhost:8081;
            add_header X-Cache-Status $upstream_cache_status;
        }
    }
    
    server {
        listen 8081;
        location /static/ {
            root /var/www;
            allow all;
        }
    }
}
```

### Lua script

The script is located in the [./nginx/script/purge.lua](./nginx/script/purge.lua) file. It is used to drop cache for specific file only.

## Results

```output
# Get TXT file, three attempts, all cache miss (see proxy pass)
2024-04-04 21:26:18 127.0.0.1 - - [04/Apr/2024:18:26:18 +0000] "GET /static/3.txt HTTP/1.0" 200 17 "-" "-"
2024-04-04 21:26:18 192.168.65.1 - - [04/Apr/2024:18:26:18 +0000] "GET /static/3.txt HTTP/1.1" 200 17 "-" "-"

2024-04-04 21:26:23 127.0.0.1 - - [04/Apr/2024:18:26:23 +0000] "GET /static/3.txt HTTP/1.0" 200 17 "-" "-"
2024-04-04 21:26:23 192.168.65.1 - - [04/Apr/2024:18:26:23 +0000] "GET /static/3.txt HTTP/1.1" 200 17 "-" "-"

2024-04-04 21:26:24 127.0.0.1 - - [04/Apr/2024:18:26:24 +0000] "GET /static/3.txt HTTP/1.0" 200 17 "-" "-"
2024-04-04 21:26:24 192.168.65.1 - - [04/Apr/2024:18:26:24 +0000] "GET /static/3.txt HTTP/1.1" 200 17 "-" "-"

# Get 1.png, 1st attempt, cache miss
2024-04-04 21:38:33 127.0.0.1 - - [04/Apr/2024:18:38:33 +0000] "GET /static/1.png HTTP/1.0" 200 1571 "-" "-"
2024-04-04 21:38:33 192.168.65.1 - - [04/Apr/2024:18:38:33 +0000] "GET /static/1.png HTTP/1.1" 200 1571 "-" "-"

# Get 1.png, 2nd attempt, cache miss
2024-04-04 21:38:34 127.0.0.1 - - [04/Apr/2024:18:38:34 +0000] "GET /static/1.png HTTP/1.0" 200 1571 "-" "-"
2024-04-04 21:38:34 192.168.65.1 - - [04/Apr/2024:18:38:34 +0000] "GET /static/1.png HTTP/1.1" 200 1571 "-" "-"

# Get 1.png, 3rd attempt, cache hit
2024-04-04 21:38:35 192.168.65.1 - - [04/Apr/2024:18:38:35 +0000] "GET /static/1.png HTTP/1.1" 200 1571 "-" "-"

# Get 2.png, 1st attempt, cache miss
2024-04-04 21:38:45 127.0.0.1 - - [04/Apr/2024:18:38:45 +0000] "GET /static/2.png HTTP/1.0" 200 261 "-" "-"
2024-04-04 21:38:45 192.168.65.1 - - [04/Apr/2024:18:38:45 +0000] "GET /static/2.png HTTP/1.1" 200 261 "-" "-"

# Get 2.png, 2nd attempt, cache miss
2024-04-04 21:38:46 127.0.0.1 - - [04/Apr/2024:18:38:46 +0000] "GET /static/2.png HTTP/1.0" 200 261 "-" "-"
2024-04-04 21:38:46 192.168.65.1 - - [04/Apr/2024:18:38:46 +0000] "GET /static/2.png HTTP/1.1" 200 261 "-" "-"

# Get 2.png, 3rd attempt, cache hit
2024-04-04 21:38:47 192.168.65.1 - - [04/Apr/2024:18:38:47 +0000] "GET /static/2.png HTTP/1.1" 200 261 "-" "-"

# Purge 1.png cache
2024-04-04 21:38:52 192.168.65.1 - - [04/Apr/2024:18:38:52 +0000] "PURGE /static/1.png HTTP/1.1" 200 13 "-" "-"

# Get 1.png, cache miss after purge
2024-04-04 21:38:56 127.0.0.1 - - [04/Apr/2024:18:38:56 +0000] "GET /static/1.png HTTP/1.0" 200 1571 "-" "-"
2024-04-04 21:38:56 192.168.65.1 - - [04/Apr/2024:18:38:56 +0000] "GET /static/1.png HTTP/1.1" 200 1571 "-" "-"

# Get 2.png, cache hit
2024-04-04 21:39:01 192.168.65.1 - - [04/Apr/2024:18:39:01 +0000] "GET /static/2.png HTTP/1.1" 200 261 "-" "-"
```
