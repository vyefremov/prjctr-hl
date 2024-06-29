# Use an official Nginx base image
FROM nginx:1.27.0

# Install build dependencies
RUN apt-get update \
    && apt-get install -y \
       build-essential \
       wget \
       libgeoip-dev \
       libtool \
       git \
       libmaxminddb-dev \
       libmaxminddb0 \
       libpcre3 \
       libpcre3-dev \
       zlib1g-dev

# Set Nginx version
ARG NGINX_VERSION=1.27.0

# Download Nginx source and ngx_http_geoip2_module
RUN wget https://nginx.org/download/nginx-${NGINX_VERSION}.tar.gz && \
    tar -zxvf nginx-${NGINX_VERSION}.tar.gz && \
    git clone https://github.com/leev/ngx_http_geoip2_module.git && \
    cd nginx-${NGINX_VERSION} && \
    ./configure --with-compat --add-dynamic-module=../ngx_http_geoip2_module --modules-path=/usr/lib/nginx/modules && \
    make modules && \
    mkdir -p /usr/lib/nginx/modules/ && \
    cp objs/ngx_http_geoip2_module.so /usr/lib/nginx/modules/ \

# Expose ports
EXPOSE 80
EXPOSE 8080
