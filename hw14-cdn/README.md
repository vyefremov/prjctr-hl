# Homework: Content Delivery Networks

## Task

Create own CDN for delivering millions of images across the globe.

1. Set up 7 containers - bind server (or any other Geo DNS), load balancer 1, load balancer 2, node1, node2, node3,
   node4.
2. Try to implement different load balancing approaches: the least number of hops, least time, highest availability.
    1. Test with VPN (make sure real IP is sent to docker container)
3. Write down pros and cons of each balancing approach

## Solution

1. Setup containers for load balancers and nodes — [docker-compose.yml](docker-compose.yml)
2. Load balancer configuration — [nginx.conf](./loadbalancer/nginx.conf)
3. Node configuration — [nginx.conf](./node/nginx.conf) based on [Homework 08: Web servers](../hw08-web-servers)

## Get DNS

1. Go to Azure Portal
2. Create a new Traffic Manager profile with "Geographic" routing method
3. Add External endpoints pointing to the load balancers (use Ngrok for that)

## Ngrok

1. Download and install Ngrok from [https://ngrok.com/download](https://ngrok.com/download)
2. Run `./ngrok http 80` to expose the local port 80 to the internet (load balancer 1)
3. Run `./ngrok http 81` to expose the local port 81 to the internet (load balancer 2)

## Load Balancing Approaches

### Default Round Robin

> Requests are distributed evenly across the servers, with server weights taken into consideration. This method is used by default (there is no directive for enabling it)

```nginx
upstream backend {
     server node1:80;
     server node2:80;
 }
```

### Least Time

> For each request, NGINX Plus selects the server with the lowest average latency and the lowest number of active connections, where the lowest average latency is calculated based on which of the following parameters to the least_time directive is included:
> 
> **header** – Time to receive the first byte from the server<br>
> **last_byte** – Time to receive the full response from the server<br>
> **last_byte inflight** – Time to receive the full response from the server, taking into account incomplete requests

```nginx
upstream backend {
   least_time header;
   server node1:80;
   server node2:80;
}
```

If I'd need to implement this, I would use OpenResty and make something like this:

1. Make sure nodes have health checks endpoints (e.g. `/health`) to track the latency based on the response time
2. Use Lua to periodically check the latency of each node
3. Use Lua to select the node based on the collected latency of each node

### Highest Availability

NGINX does not support this out of the box. If I'd need to implement this, I would use OpenResty and make something like "Least Time" approach:

1. Make sure nodes have health checks endpoints (e.g. `/health`) to track the availability
2. Use Lua to periodically check the availability of each node
3. Use Loa to select the node based on the collected availability information of each node

### Least Number of Hops

NGINX does not support this out of the box. If I'd need to implement this, I would make something like this:

1. Upon starting the container, I would make a `traceroute` to each node and get the number of hops
2. Based on the number of hops, I would update nginx.conf with higher weight for the node with the least number of hops

