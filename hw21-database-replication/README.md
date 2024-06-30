# Homework: Database Replication

## Task

1. Create 3 docker containers: mysql-source, mysql-replica1, mysql-replica2
2. Setup replication
3. Write script that will frequently write data to database 
4. Ensure, that replication is working 
5. Try to turn off mysql-replica1 (stop replica)
6. Try to remove a column in  database on replica node (try to delete last column and column from the middle)

## Solution

### Docker Containers

1. Setup mysql-source container
   1. Use [source/mysql.env](./source/mysql.env) file to setup environment variables
   2. Use [source/mysql.conf.cnf](./source/mysql.conf.cnf) file to setup mysql configuration
      1. `server-id=1` - set unique id for each mysql instance
      2. `log-bin=1` - enable binary logging (can be file name)
      3. `binlog-do-db=hw21` - set database name for replication
      4. `binlog_format=ROW` - set binary logging format (ROW, STATEMENT, MIXED)
2. Setup mysql-replica1 container
   1. Use [replica/mysql.env](./replica/mysql.env) file to setup environment variables
   2. Use [replica/mysql1.conf.cnf](./replica/mysql1.conf.cnf) file to setup mysql configuration
      1. `server-id=2` - set unique id for each mysql instance
      2. `log-bin=1` - enable binary logging (can be file name)
      3. `binlog-do-db=hw21` - set database name for replication
3. Setup mysql-replica2 container
   1. Use [replica/mysql.env](./replica/mysql.env) file to setup environment variables
   2. Use [replica/mysql2.conf.cnf](./replica/mysql2.conf.cnf) file to setup mysql configuration
      1. `server-id=3` - set unique id for each mysql instance
      2. `log-bin=1` - enable binary logging (can be file name)
      3. `binlog-do-db=hw21` - set database name for replication

### Source Database

```sql
CREATE USER 'replica'@'%' IDENTIFIED BY 'password';
GRANT REPLICATION SLAVE ON *.* TO 'replica'@'%';
FLUSH PRIVILEGES;

USE hw21;
FLUSH TABLES WITH READ LOCK;

-- Optional: dump
SHOW MASTER STATUS;
-- Create a dump of the database 
UNLOCK TABLES;
```

### Replica Databases

```sql
CHANGE REPLICATION SOURCE TO GET_SOURCE_PUBLIC_KEY=1;
START REPLICA;
SHOW REPLICA STATUS;
```

Make sure that the replica is running and the `Replica_IO_Running` and `Replica_SQL_Running` are set to `Yes` and there is no errors.

### Test

```sql
-- On Source Database

CREATE TABLE IF NOT EXISTS `users` (
   `id` int NOT NULL AUTO_INCREMENT,
   `username` varchar(255) NOT NULL,
   `age` int NOT NULL,
   `email` varchar(255) NOT NULL,
   `height` int NOT NULL,
   `weight` int NOT NULL,
   PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1;

-- Make sure that the table is created on the replica databases: OK

-- Insert data into the table on the source database

INSERT INTO `users` (`username`, `age`, `email`, `height`, `weight`)
VALUES (
   CONCAT('User_', FLOOR(RAND() * 100)),
   FLOOR(RAND() * 50) + 20,
   CONCAT('email_', FLOOR(RAND() * 1000)),
   FLOOR(RAND() * 50) + 150,
   FLOOR(RAND() * 50) + 50);

-- Make sure that the data is replicated to the replica databases: OK

-- Turn off mysql-replica1
STOP REPLICA;

-- Make some more inserts on the source database
-- Make sure that the data is replicated to the mysql-replica1 but not to the mysql-replica2: OK

-- Turn on mysql-replica1
START REPLICA;

-- Make sure that all the data is replicated to the mysql-replica1 and mysql-replica2: OK

-- Remove the LAST column in the database on the mysql-replica1 node
ALTER TABLE hw21.users DROP COLUMN `weight`;

-- Make sure that the column is removed from the mysql-replica1 but not from the mysql-replica2: OK
-- Make sure that the data is replicated correctly to the mysql-replica1: OK

-- v1 Remove the MIDDLE column in the database on the mysql-replica2 node
ALTER TABLE hw21.users DROP COLUMN `height`;

-- Make sure that the column is removed from the mysql-replica2 but not from the mysql-replica1: OK
-- Make sure that the data is replicated correctly to the mysql-replica2:
--      FAILURE The replication works but now new records contain the value of `height` column in `weight` column

-- v2 Remove the MIDDLE column in the database on the mysql-replica2 node
ALTER TABLE hw21.users DROP COLUMN `age`;

-- Make sure that the data is replicated correctly to the mysql-replica2:
--      FAILURE The replication stopped working.
--      Column 2 of table 'hw21.users' cannot be converted from type 'int' to type 'varchar(1020(bytes) utf8mb4)'
--      The replica coordinator and worker threads are stopped, possibly leaving data in inconsistent state.
```

## Conclusion

Replication works well when the schema is the same on all the nodes. If the schema is different, the replication can fail.

When we stop the replica, the data is not replicated to the replica node. But, when we start it again, the data is replicated since the last change.

When we remove the last column from the database, the data is replicated correctly to the replica node.

But, when we remove the middle column, the consequences can be different.
If shifted columns types are compatible, the replication keeps working, but the new records will contain the value of the removed column in the shifted column.
If shifted columns types are not compatible, the replication stops working.
