# HSA L5 Homework: Stress Testing

# Setup

1. Create simple web page that accept requests and stores data from request to database
   1. Use a reference [.NET application implementing an eCommerce web site using a services-based architecture](https://github.com/dotnet/eShop.git).
   2. `git clone https://github.com/dotnet/eShop.git`
   3. Walk through the [prerequisites](https://github.com/dotnet/eShop?tab=readme-ov-file#prerequisites)
   4. `dotnet run --project src/eShop.AppHost/eShop.AppHost.csproj`
2. Install Siege
   1. `git clone https://github.com/JoeDog/siege.git`
   2. `./configure`
   3. `make`
   4. `make install`

# Run
## Command

```
siege -f request.txt --content-type "application/json" -c 100 -r 50 -v

# where 
# -f: file with request
# -c: concurrent users
# -r: repetitions per each user
# -v: verbose
# http://localhost:5222/api/v1/catalog/items - the API endpoint which creates catalog item
```

## Results

Runs are logged in the [runs.txt](runs.txt) file.

> I've noticed that Siege executed POST request and then GET request automatically.
This affected the test run, causing the request count to double.

| Concurrency | Repetitions | Availability | AVG Response Time | Throughput | Successful Transactions | Failed Transactions |
|-------------|-------------|--------------|-------------------|------------|------------------------|---------------------|
| 10          | 10          | 98.99%       | 0.00 secs         | 0.41 MB/sec| 196                    | 2                   |
| 10          | 25          | 99.60%       | 0.00 secs         | 0.37 MB/sec| 496                    | 2                   |
| 25          | 10          | 99.40%       | 0.01 secs         | 0.55 MB/sec| 494                    | 3                   |
| 50          | 10          | 99.84%       | 0.01 secs         | 0.66 MB/sec| 1246                   | 2                   |
| 50          | 25          | 99.84%       | 0.01 secs         | 0.82 MB/sec| 2492                   | 4                   |
| 100         | 10          | 99.55%       | 0.04 secs         | 0.44 MB/sec| 1982                   | 9                   |
| 100         | 50          | 99.73%       | 0.11 secs         | 0.08 MB/sec| 9946                   | 27                  |


