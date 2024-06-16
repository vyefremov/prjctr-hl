create extension if not exists postgres_fdw;

DROP SERVER IF EXISTS books_server_2 CASCADE;
DROP SERVER IF EXISTS books_server_3 CASCADE;

CREATE SERVER books_server_2
    FOREIGN DATA WRAPPER postgres_fdw
    OPTIONS(host 'postgres_fdw_2', port '5432', dbname 'postgres');

CREATE USER MAPPING FOR postgres
    SERVER books_server_2
    OPTIONS (user 'postgres', password 'postgres');

CREATE SERVER books_server_3
    FOREIGN DATA WRAPPER postgres_fdw
    OPTIONS(host 'postgres_fdw_3', port '5432', dbname 'postgres');

CREATE USER MAPPING FOR postgres
    SERVER books_server_3
    OPTIONS (user 'postgres', password 'postgres');

create table books (
   id uuid primary key default gen_random_uuid(),
   name varchar(255) not null,
   pages int not null,
   category_id int not null
);

-- create index on category_id
create index ix_books_category_id on books(category_id);

drop foreign table if exists books_2;
drop foreign table if exists books_3;

create foreign table books_2 (
   id uuid not null,
   name varchar(255) not null,
   pages int not null,
   category_id int not null
)
SERVER books_server_2
OPTIONS (table_name 'books', schema_name 'public');

create foreign table books_3 (
    id uuid not null,
    name varchar(255) not null,
    pages int not null,
    category_id int not null
)
SERVER books_server_3
OPTIONS (table_name 'books', schema_name 'public');

--
-- Create view
--
CREATE VIEW books_vw AS
    SELECT * FROM books_2
    UNION ALL
    SELECT * FROM books_3;

select * from books_vw;

--
-- Create default rules
--

CREATE RULE books_insert AS ON INSERT TO books
    DO INSTEAD NOTHING;
CREATE RULE books_update AS ON UPDATE TO books
    DO INSTEAD NOTHING;
CREATE RULE books_delete AS ON DELETE TO books
    DO INSTEAD NOTHING;

-- 
-- Create redirect rules
--

DROP RULE IF EXISTS books_insert_2 ON books;
DROP RULE IF EXISTS books_insert_3 ON books;

CREATE RULE books_insert_2 AS
    ON INSERT TO books
    WHERE (NEW.category_id between 1 and 5)
    DO INSTEAD
    INSERT INTO books_2 VALUES (NEW.*);

CREATE RULE books_insert_3 AS
    ON INSERT TO books
    WHERE (NEW.category_id between 6 and 10)
    DO INSTEAD
    INSERT INTO books_3 VALUES (NEW.*);

--
-- Insert data
--

insert into books (name, pages, category_id)
select
    'book_' || generate_series(1, 1000000) AS id,
    (random() * 999 + 1)::int AS pages,
    (random() * 9 + 1)::int AS category_id;

select count(*) as total_by_cat, sum(pages) as pages_by_cat from books_vw where category_id = 3;

select count(*) as total_by_cat, sum(pages) as pages_by_cat from books_vw where category_id = 7;
