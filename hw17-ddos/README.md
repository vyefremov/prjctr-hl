# Homework: DDoS Attacks

## Task

1. Setup two docker containers:
   1. Attacker container - executing scripts that will implement 6 attacks:
   UDP Flood, ICMP flood, HTTP flood, Slowloris, SYN flood,  Ping of Death
   2. Defender container - ubuntu & nginx with simple website
2. Try to implement protection on Defender container
3. Launch attacker scripts and examine you protection

## Solution

The solution contains the following components:
1. Nginx configuration — [nginx.conf](./nginx/nginx.conf)
2. Nginx Docker setup:
   1. Via docker-compose — [docker-compose.yml](./docker-compose.yml)
   2. Alternative setup — [Dockerfile](./nginx/Dockerfile)
3. Health status monitoring — [health.sh](./health.sh)
4. Attempt to run `hping3` in a docker container — [Dockerfile](./hping/Dockerfile)

### Defender

#### UDP Flood

> User Datagram Protocol (UDP) floods attack random ports on a remote server with requests called UDP packets. The host checks the ports for the appropriate applications. When no application can be found, the system responds to every request with a “destination unreachable” packet. The resulting traffic can overwhelm the service.

If we want to protect against UDP Flood, we can use `iptables` to drop all UDP packets. 

```bash
iptables -A INPUT -p udp -j DROP
```

If we need to allow some UDP packets from a specific port and IP address, we can use the following command:

```bash
iptables -A INPUT -p udp --sport 53 -s 8.8.8.8 -j ACCEPT
```

#### ICMP Flood

> An Internet Control Message Protocol (ICMP) flood sends ICMP echo request packets (pings) to a host. Pings are common requests used to measure the connectivity of two servers. When a ping is sent, the server quickly responds. In a ping flood, however, an attacker uses an extensive series of pings to exhaust the incoming and outgoing bandwidth of the targeted server.

If we want to protect against ICMP Flood, we can use `iptables` to drop all ICMP packets. 

```bash
iptables -A INPUT --proto icmp -j DROP
```

Or add the kernel parameter:

```bash
echo "net.ipv4.icmp_echo_ignore_all = 1" >> /etc/sysctl.conf 
sysctl -p # to apply the changes
```

#### HTTP Flood

> An HTTP flood is a Layer 7 application attack that uses botnets, often referred to as a “zombie army.” In this type of attack, standard GET and POST requests flood a web server or application. The server is inundated with requests and may shut down. These attacks can be particularly difficult to detect because they appear as perfectly valid traffic.

Protecting Nginx:

```nginx
# Rate Limiting
# Allow 1 request per second
limit_req_zone $binary_remote_addr zone=one:10m rate=60r/m;
server { 
   location / { 
      limit_req zone=one;
   }
}

# Limiting the number of connections per IP
# Allow each client IP address to open no more than 10 connections
limit_conn_zone $binary_remote_addr zone=addr:10m;
server { 
   location / { 
      limit_conn addr 10;
   }
}
```

#### Slowloris

> Slowloris is a denial-of-service attack program which allows an attacker to overwhelm a targeted server by opening and maintaining many simultaneous HTTP connections between the attacker and the target.

Protecting Nginx:

```nginx
# Closing Slow Connections
server {
    client_body_timeout 10; # Defines a timeout for reading client request body.
    client_header_timeout 10; # Defines a timeout for reading client request header.
    keepalive_timeout 5 5; # The first parameter sets a timeout during which a keep-alive client connection will stay open on the server side. The second parameter sets a timeout during which a keep-alive client connection will stay open on the client side.
}
```

#### SYN flood

> A SYN flood (half-open attack) is a type of denial-of-service (DDoS) attack which aims to make a server unavailable to legitimate traffic by consuming all available server resources. By repeatedly sending initial connection request (SYN) packets, the attacker is able to overwhelm all available ports on a targeted server machine, causing the targeted device to respond to legitimate traffic sluggishly or not at all.

Protecting Nginx:

```nginx
# Increase Backlog Queue Size
# The backlog parameter defines the maximum length of the queue of pending connections.
# This can help in handling bursts of incoming connections more effectively.
events {
    worker_connections  1024;
    backlog 512;
}

# Combine with rate limiting per IP
```

#### Ping of Death

> PoD is caused by an attacker deliberately sending an IP packet larger than the 65,536 bytes allowed by the IP protocol. Can be executed by running the following command: ping -l 65610 somesite.com. Before 1997 many operating systems didn't know what to do when they received an oversized packet, so they froze, crashed, or rebooted.

Modern systems and network devices typically have mechanisms in place to detect and block malformed packets.

### Attacker

I have a problem running `hping3`. 

When I use docker `utkudarilmaz/hping3`:

```bash
docker pull utkudarilmaz/hping3:latest

# UDP Flood
docker run utkudarilmaz/hping3:latest --rand-source --flood localhost -p 8080
```
I get the following error:

```bash
[open_pcap] pcap_open_live: lo: SIOCETHTOOL(ETHTOOL_GET_TS_INFO) ioctl failed: Function not implemented
[main] open_pcap failed
```

When I try to build the image from the [hping/Dockerfile](./hping/Dockerfile) it stuck on the:

```
# Connecting to www.hping.org (192.81.221.216:80) 
```

When I try installing hping3 on my local machine, I get the following error:

```bash
Error: hping has been disabled because it is not maintained upstream!
```
