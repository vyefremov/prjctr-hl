# HSA L10 Homework: Transactions, Isolation, and Locks

## Task

- Setup Percona and Postgres
- By changing isolation levels and making parallel queries, reproduce the main problems of parallel access:
  - Lost update
  - Dirty read
  - Non-repeatable read
  - Phantom read
- Create two summary tables for each database
  - Rows - the problem of parallel access
  - Columns - isolation level

## Setup

- [x] Run `docker-compose up -d` to start Percona and Postgres
- [x] Setup percona
```mysql
CREATE TABLE IF NOT EXISTS `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `username` varchar(255) NOT NULL,
  `age` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1;

SET autocommit=0; -- disable autocommit on each request
SET GLOBAL innodb_status_output=ON; -- enable standard monitoring
SET GLOBAL innodb_status_output_locks=ON; -- enable locks monitoring
```
- [x] Setup postgres
```sql
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    age INT NOT NULL
);
```

### Prepare query console

#### Percona

```mysql
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
# SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
# SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
# SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
START TRANSACTION;

SELECT * FROM users WHERE id = 1;
SELECT * FROM users WHERE age = 10; -- for phantom read check
UPDATE users SET age = 10 WHERE id = 1; -- use age = 20 for session 2

COMMIT;
```

#### Postgres

```postgresql
SET SESSION CHARACTERISTICS AS TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
-- SET SESSION CHARACTERISTICS AS TRANSACTION ISOLATION LEVEL READ COMMITTED;
-- SET SESSION CHARACTERISTICS AS TRANSACTION ISOLATION LEVEL REPEATABLE READ;
-- SET SESSION CHARACTERISTICS AS TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN;

SELECT * FROM users WHERE id = 1;
SELECT * FROM users WHERE age = 10; -- for phantom read check
UPDATE users SET age = 10 WHERE id = 1; -- use age = 20 for session 2

COMMIT;
```

## Problems

### Lost update

| Session 1            | Session 2            |
|----------------------|----------------------|
| `START TRANSACTION`  | `START TRANSACTION`  |
| `SELECT`             |                      |
|                      | `SELECT`             |
| `UPDATE SET val = 1` |                      |
|                      | `UPDATE SET val = 2` |
| `COMMIT`             |                      |
|                      | `COMMIT`             |

In the event of a _lost update_, the value of `val` will be `2`, as the changes made in the second session will overwrite those of the first.

### Non-repeatable read

| Session 1           | Session 2            |
|---------------------|----------------------|
| `START TRANSACTION` | `START TRANSACTION`  |
| `SELECT`            |                      |
|                     | `UPDATE SET val = 2` |
| `SELECT`            |                      |
|                     | `COMMIT`             |
| `SELECT`            |                      |
| `COMMIT/ROLLBACK`   |                      |

In the event of a _non-repeatable read_, the first session will eventually read the new value of `val` as `2`.

### Dirty read

| Session 1             | Session 2           |
|-----------------------|---------------------|
| `START TRANSACTION`   | `START TRANSACTION` |
| `UPDATE SET val = 10` |                     |
|                       | `SELECT`            |
| `COMMIT/ROLLBACK`     |                     |

In the event of a _dirty read_, the second session will read the value of `val` before the first session has committed the changes.

### Phantom read

| Session 1           | Session 2            |
|---------------------|----------------------|
| `START TRANSACTION` | `START TRANSACTION`  |
| `SELECT`            |                      |
|                     | `INSERT` or `UPDATE` |
|                     | `COMMIT`             |
| `SELECT`            |                      |
| `COMMIT/ROLLBACK`   |                      |

In the event of a _phantom read_, the second session will insert a new row or update existing one, which the first session will read as a new row.

## Results

### Percona

| Problem / Isolation level | Read Uncommitted | Read Committed  | Repeatable Read | Serializable    |
|---------------------------|------------------|-----------------|-----------------|-----------------|
| Lost update               | possible 🔺      | possible 🔺     | possible 🔺     | not possible 🔹 |
| Non-repeatable read       | possible 🔺      | possible 🔺     | not possible 🔹 | not possible 🔹 |
| Phantom read              | possible 🔺      | possible 🔺     | not possible 🔹 | not possible 🔹 |
| Dirty read                | possible 🔺      | not possible 🔹 | not possible 🔹 | not possible 🔹 |

### Postgres

| Problem / Isolation level | Read Uncommitted | Read Committed  | Repeatable Read | Serializable    |
|---------------------------|------------------|-----------------|-----------------|-----------------|
| Lost update               | possible 🔺      | possible 🔺     | not possible 🔹 | not possible 🔹 |
| Non-repeatable read       | possible 🔺      | possible 🔺     | not possible 🔹 | not possible 🔹 |
| Phantom read              | possible 🔺      | possible 🔺     | not possible 🔹 | not possible 🔹 |
| Dirty read                | not possible 🔹  | not possible 🔹 | not possible 🔹 | not possible 🔹 |

## Summary

- In Postgres `Read Uncommitted` level is not supported. The lowest level is `Read Committed`;
- In Postgres `Dirty read` is not possible in any isolation level compared to Percona;
- Postgres prevents `Lost updates` by acquiring locks on the selected rows for the duration of the transaction. This ensures that no other transactions can modify or delete the locked rows until the current transaction commits or rolls back.
