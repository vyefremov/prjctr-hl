# Homework: Database Sharding

## Task

- Create 3 docker containers: postgresql-b, postgresql-b1, postgresql-b2
- Setup horizontal/vertical sharding as it’s described in this lesson and  with alternative tool (citus, pgpool-|| postgres-xl)
- Insert 1 000 000 rows into books
- Measure performance for reads and writes
- Do the same without sharding
- Compare performance of 3 cases (without sharding, FDW, and approach of your choice)

## Setup

### No Sharding 

1. [Docker-compose file](./src/no-sharding/docker-compose.yml) with single Postgres instance
   1. Limit memory to `1G` and CPU to `1.0` to simulate the close environment to the sharded setup
2. Create `books` table
3. Create index on `category_id` column

### FDW

1. [Docker-compose file](./src/fdw-sharding/docker-compose.yml) with 3 Postgres instances
   1. Limit memory to `512M` and CPU to `1.0` for each shard to simulate the close environment to the sharded setup
2. Setup each shard ([fdw_2.sql](./src/fdw-sharding/fdw_2.sql), [fdw_3.sql](./src/fdw-sharding/fdw_3.sql))
   1. Create `books` table
   2. Create index on `category_id` column
3. Setup **main** node with `postgres_fdw` extension ([fdw_main.sql](./src/fdw-sharding/fdw_main.sql))
   1. `create extension pg_stat_statements;`
   2. Create servers: `book_server_2` and `book_server_3`
   3. Create user mapping for each server
   4. Create foreign tables for each server
   5. Create `books` table
   6. Create index on `category_id` column
   7. Create default rules `do instead nothing`
   8. Create rules to partition inserts to the shards by `category_id`
   9. Create view to union all shards

### Citus

1. Clone Citus Docker compose setup `git clone https://github.com/citusdata/docker.git`
   1. Limit memory to `512M` and CPU to `1.0` for each container to simulate the close environment to the sharded setup
2. Run `docker-compose up -d`
3. Scale workers up to 2 `docker-compose -p citus up --scale worker=2`
4. Check active workers `select master_get_active_worker_nodes();`
5. Create `books` table
6. Create index on `category_id` column
7. Create distributed table `select create_distributed_table('books', 'category_id');`

### Performance Comparison

Scripts to perform the performance comparison are located in the [scripts.sql](./src/scripts.sql).

1. Insert 1 000 000 rows into books table, measure time of execution
2. Make a few reads from books table by random category (partition key), measure time of execution
3. Make 10 000 more inserts to books table by random category (partition key), measure time of execution

#### Results

| Case        | Insert 1M rows | Read by category 3 | Read by category 7 | Insert 10k more rows |
|-------------|----------------|--------------------|--------------------|----------------------|
| No Sharding | 3.267s         | 73ms               | 35ms               | 74ms                 |
| FDW         | 1m 4.510s      | 152ms              | 146ms              | 735ms                |
| Citus       | 20.299s        | 110ms              | 172ms              | 1022ms               |

#### Conclusion

- Results cannot be considered as a fair comparison because it's not a real distributed setup
