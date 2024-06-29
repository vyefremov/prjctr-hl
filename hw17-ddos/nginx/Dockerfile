# Use the official Ubuntu base image
FROM ubuntu:latest

# Update package lists and install Nginx
RUN apt-get update && \
    apt-get install -y nginx && \
    apt-get install -y sudo && \
    apt-get install -y iptables && \
    apt-get clean

# Remove the default Nginx configuration file
RUN rm -v /etc/nginx/nginx.conf

# Copy a configuration file from the current directory to replace the default Nginx configuration
ADD nginx.conf /etc/nginx/

# Expose ports
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]