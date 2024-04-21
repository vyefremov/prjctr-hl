# HSA L9 Homework: SQL Databases

## Task

- Use MySQL (or fork) with InnoDB
- Make a table for 40M users
- Compare performance of selections by date of birth:
  - Without index 
  - With BTREE index 
  - With HASH index
- Check insert speed difference with different `innodb_flush_log_at_trx_commit` value and different `ops per second`

## Setup

- [x] Run MySQL via [docker-compose](docker-compose.yml)
- [x] Create a table for users
```mysql
create table if not exists `users` (
    id varchar(36) primary key,
    name nvarchar(100) not null,
    surname nvarchar(100) not null,
    birthday date not null
) ENGINE=InnoDb;
```
- [x] Fill table with 40M users
```mysql
-- Set session variables for optimizing insert speed
SET autocommit=0;
SET unique_checks=0;
SET foreign_key_checks=0;

-- Set variables for the procedure
SET @count = 40000000;
SET @dateFrom = '1950-01-01';
SET @dateTo = '2014-04-08';

DELIMITER //

CREATE PROCEDURE IF NOT EXISTS generate_users()
BEGIN
  DECLARE i INT DEFAULT 1;
  WHILE i <= @count DO
    INSERT INTO users (id, name, surname, birthday) 
    VALUES (
      UUID(),
      CONCAT('Name-', i),
      CONCAT('Surname-', i),
      DATE_ADD(@dateFrom, INTERVAL FLOOR(RAND() * (DATEDIFF(@dateTo, @dateFrom) + 1)) DAY)
    );
    SET i = i + 1;
  END WHILE;
END //

DELIMITER ;

CALL generate_users();

-- Reset session variables
SET autocommit=1;
SET unique_checks=1;
SET foreign_key_checks=1;
```
Execution log:
```
hw9> CALL generate_users()
[2024-04-08 16:11:24] 1 row affected in 7 m 54 s 352 ms
hw9> SET autocommit=1
[2024-04-08 16:11:36] completed in 11 s 312 ms
hw9> SET unique_checks=1
[2024-04-08 16:11:36] completed in 1 ms
hw9> SET foreign_key_checks=1
[2024-04-08 16:11:36] completed in 1 ms
```

## Compare performance of selections by date of birth

### Queries

```mysql
SELECT SQL_NO_CACHE count(*) FROM users WHERE birthday <= '1956-06-05'; -- 10%
SELECT SQL_NO_CACHE count(*) FROM users WHERE birthday <= '1966-01-25'; -- 25%
SELECT SQL_NO_CACHE count(*) FROM users WHERE birthday BETWEEN '1970-01-01' and '1986-01-25'; -- 25% inner range
SELECT SQL_NO_CACHE count(*) FROM users WHERE birthday > '1982-02-18'; -- 50%
SELECT SQL_NO_CACHE count(*) FROM users WHERE birthday between '1966-01-01' and '1988-01-01'; -- 50% inner range
SELECT SQL_NO_CACHE count(*) FROM users WHERE birthday between '1900-01-01' and '2020-01-01'; -- 100%
SELECT SQL_NO_CACHE count(*) FROM users WHERE birthday = '1985-07-01'; -- Specific date

SELECT SQL_NO_CACHE count(*) FROM users WHERE YEAR(birthday) BETWEEN 1950 AND 1960; -- 17%
SELECT SQL_NO_CACHE count(*) FROM users WHERE YEAR(birthday) BETWEEN 1994 AND 2014; -- 31.5%

SELECT SQL_NO_CACHE count(*) FROM users WHERE MONTH(birthday) BETWEEN 3 AND 6; -- 35%
SELECT SQL_NO_CACHE count(*) FROM users WHERE MONTH(birthday) BETWEEN 5 AND 12; -- 65%

CREATE INDEX idx_birthday ON users (birthday) USING BTREE; -- completed in 1 m 6 s 681 ms

DROP INDEX idx_birthday ON users;

CREATE INDEX idx_birthday ON users (birthday) USING HASH; -- This storage engine does not support the HASH index algorithm, storage engine default was used instead.
```

### Comparison table

| Range                         | % of records | Without index | With BTREE index | With HASH index |
|-------------------------------|--------------|---------------|------------------|-----------------|
| <= '1956-06-05'               | 10%          | 6 s 211 ms    | 581 ms           | -               |
| <= '1966-01-25'               | 25%          | 6 s 906 ms    | 1 s 323 ms       | -               |
| '1970-01-01' and '1986-01-25' | 25%          | 10 s 612 ms   | 2 s 480 ms       | -               |
| more than '1982-02-18'        | 50%          | 6 s 996 ms    | 2 s 567 ms       | -               |
| '1966-01-01' and '1988-01-01' | 50%          | 10 s 639 ms   | 3 s 403 ms       | -               |
| '1900-01-01' and '2020-01-01' | 100%         | 10 s 891 ms   | 9 s 863 ms       | -               |
| = '1985-07-01'                | 0.0000025%   | 7 s 334 ms    | 16 ms            | -               |
| YEAR between 1950 and 1960    | 17%          | 7 s 459 ms    | 3 s 792 ms       | -               |
| YEAR between 1994 and 2014    | 31%          | 7 s 565 ms    | 3 s 810 ms       | -               |
| MONTH between 3 and 6         | 35%          | 7 s 564 ms    | 3 s 818 ms       | -               |
| MONTH between 5 and 12        | 65%          | 7 s 627 ms    | 3 s 895 ms       | -               |

### Explain With BTREE index

 | type  | possible_keys | key          | key_len | ref   | rows     | filtered | extra                    |
|-------|---------------|--------------|---------|-------|----------|----------|--------------------------|
| range | idx_birthday  | idx_birthday | 3       |       | 8093070  | 100      | Using where; Using index |
| range | idx_birthday  | idx_birthday | 3       |       | 20045241 | 100      | Using where; Using index |
| range | idx_birthday  | idx_birthday | 3       |       | 20045241 | 100      | Using where; Using index |
| range | idx_birthday  | idx_birthday | 3       |       | 20045241 | 100      | Using where; Using index |
| range | idx_birthday  | idx_birthday | 3       |       | 20045241 | 100      | Using where; Using index |
| range | idx_birthday  | idx_birthday | 3       |       | 20045241 | 100      | Using where; Using index |
| ref   | idx_birthday  | idx_birthday | 3       | const | 1651     | 100      | Using index              |
| index |               | idx_birthday | 3       |       | 40090483 | 100      | Using where; Using index |
| index |               | idx_birthday | 3       |       | 40090483 | 100      | Using where; Using index |
| index |               | idx_birthday | 3       |       | 40090483 | 100      | Using where; Using index |
| index |               | idx_birthday | 3       |       | 40090483 | 100      | Using where; Using index |

### Conclusion

- BTREE index is the best choice for range queries
- BTREE index has ultimate performance for queries by specific key
- HASH index is not supported by InnoDB and fallbacks to BTREE
- `MONTH`, `YEAR` and other functions can lead to full index scan

## Check insert speed difference

With different `innodb_flush_log_at_trx_commit` value and ops per second. Run with Apache JMeter with the following settings:
- Rump-up period (seconds): 5
- Duration (seconds): 10
- Startup delay (seconds): 1

```mysql
SHOW VARIABLES LIKE 'innodb_flush_log_at_trx_commit';
     
SET GLOBAL innodb_flush_log_at_trx_commit = 0;
```

| innodb_flush_log_at_trx_commit | Number of threads (users) | Inserted records |
|--------------------------------|---------------------------|------------------|
| 0 🥇                           | 50                        | 154 240          |
| 1                              | 50                        | 119 362          |
| 2 🥈                           | 50                        | 138 049          |
| 0 🥇                           | 100                       | 187 864          |
| 1                              | 100                       | 147 154          |
| 2 🥈                           | 100                       | 176 514          |

### Conclusion

- `innodb_flush_log_at_trx_commit` value of `0` has the best performance as it flushes the log to disk once per second
- `innodb_flush_log_at_trx_commit` value of `1` has the worst performance as it flushes the log to disk on each transaction commit, but it's the **safest** option
- `innodb_flush_log_at_trx_commit` value of `2` has the second best performance as it flushes to the OS cache on each transaction commit and flushes to disk once per second
