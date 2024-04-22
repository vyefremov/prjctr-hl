# HSA L12 Homework: Redis

## Task

- Build master-slave redis cluster 
- Try all eviction strategies 
- Write a wrapper for Redis Client that implement probabilistic cache clearing 

## Setup

### Redis configuration
```
# Limit memory
maxmemory 2mb
maxmemory-policy <policy>

# Enable eviction events
# E - Keyevent events, published with __keyevent@<db>__ prefix
# e - Evicted events (events generated when a key is evicted for maxmemory)
notify-keyspace-events Ee
```

The `notify-keyspace-events` allows to subscribe to events when keys are evicted by the following pattern: `__keyevent@<db>__:evicted`

## Results

### Methodology for testing eviction strategies

1. Subscribe to eviction events: `__keyevent@*evicted`
2. Insert keys with random data and random expiration time
3. In parallel simulate usage of random keys
4. Stop on eviction event and log the statistics

### noeviction

> New values aren’t saved when memory limit is reached. When a database uses replication, this applies to the primary database.

1. Start inserting GUIDs in infinite loop
2. Redis exception: `OOM command not allowed when used memory > 'maxmemory'`

```
redis-cli INFO memory

    used_memory:2089408
    used_memory_human:1.99M

    maxmemory:2097152
    maxmemory_human:2.00M
```

### allkeys-lru

> Keeps most recently used keys; removes least recently used (LRU) keys

The following log shows that the lest recently used keys are removed when memory limit is reached.

```
Key evicted: data286 [expires in 00:04:32, used 1 times]
  % of keys:
    0,0% less frequently; 10,9% more frequently
    86,0% more recently; 13,9% less recently <-------------------

Key evicted: data193 [expires in 00:05:58, used 1 times]
  % of keys:
    0,0% less frequently; 10,9% more frequently
    91,2% more recently; 8,8% less recently <-------------------
```

But it also shows that:
> Redis LRU algorithm is not an exact implementation. This means that Redis is not able to pick the best candidate for eviction, that is, **the key that was accessed the furthest in the past**. Instead it will try to run an approximation of the LRU algorithm, by sampling a small number of keys, and evicting the one that is the best (with the oldest access time) among the sampled keys.
> 
> What is important about the Redis LRU algorithm is that you are able to tune the precision of the algorithm by changing the number of samples to check for every eviction. This parameter is controlled by the following configuration directive: maxmemory-samples

### allkeys-lfu

> Keeps frequently used keys; removes least frequently used (LFU) keys

```
Key evicted: data492 [expires in 00:13:54, used 1 times]
  % of keys:
    0,0% less frequently; 11,0% more frequently <-------------------
    75,7% more recently; 24,3% less recently

Key evicted: data1570 [expires in <never>, used 1 times]
  % of keys:
    0,0% less frequently; 11,1% more frequently <-------------------
    10,0% more recently; 89,9% less recently
```

### volatile-lru

> Removes least recently used keys with the expire field set to true.

Numerous of runs show that the keys with expire field set are removed first. Other is same as for `allkeys-lru`.

```
Key evicted: data236 [expires in 00:07:54, used 1 times]
  % of keys:
    0,0% less frequently; 11,0% more frequently
    89,2% more recently; 10,7% less recently <-------------------

Key evicted: data392 [expires in 00:04:38, used 1 times]
  % of keys:
    0,0% less frequently; 11,0% more frequently
    81,0% more recently; 19,0% less recently <-------------------
```

### volatile-lfu

> Removes least frequently used keys with the expire field set to true.

Same as for `allkeys-lru` but accounts for expire field.

```
Key evicted: data740 [expires in 00:08:40, used 1 times]
  % of keys:
    0,0% less frequently; 11,7% more frequently <-------------------
    60,9% more recently; 39,1% less recently

Key evicted: data114 [expires in 00:09:49, used 1 times]
  % of keys:
    0,0% less frequently; 11,7% more frequently <-------------------
    95,4% more recently; 4,5% less recently
```

### allkeys-random

> Randomly removes keys to make space for the new data added.

```
Key evicted: data165 [expires in 00:09:18, used 1 times]
  % of keys:
    0,0% less frequently; 11,2% more frequently
    93,0% more recently; 6,9% less recently

Key evicted: data1120 [expires in <never>, used 1 times]
  % of keys:
    0,0% less frequently; 11,2% more frequently
    38,3% more recently; 61,7% less recently
```

### volatile-random

> Randomly removes keys with the expire field set to true.

```
Key evicted: data1494 [expires in 00:10:57, used 1 times]
  % of keys:
    0,0% less frequently; 11,3% more frequently
    14,6% more recently; 85,4% less recently
```

### volatile-ttl

> Removes keys with the nearest expire time.

```
Key evicted: data1549 [expires in 00:02:00, used 1 times]
  % of keys:
    0,0% less frequently; 10,9% more frequently
    8,6% more recently; 91,3% less recently
    78,8% expire later; 0,0% expire sooner <-------------------

Key evicted: data942 [expires in 00:02:04, used 1 times]
  % of keys:
    0,0% less frequently; 10,9% more frequently
    47,5% more recently; 52,4% less recently
    78,5% expire later; 0,4% expire sooner <-------------------
```

## Probabilistic cache clearing

### Methodology for testing probabilistic cache clearing

1. Concurrently get keys, `concurrency` is set to `100`
2. If key is not found in cache, get it from database and put it in cache
3. If there are multiple threads trying to get the same key from database - increase database response time

| Algorithm                  | Operations count | Ops/s  | Database ops count | Database ops % | Probabilistic hits count | Probabilistic hits % |
|----------------------------|------------------|--------|--------------------|----------------|--------------------------|----------------------|
| Get                        | 1 178 000        | 37 417 | 859                | 0,0729%        | 0                        | 0,0000%              |
| Get + Probabilistic        | 1 333 000        | 44 390 | 607                | 0,0455%        | 507                      | 0,0380%              |
| Get + Probabilistic + Lock | 983 000          | 32 039 | 636                | 0,0647%        | 536                      | 0,0545%              |
