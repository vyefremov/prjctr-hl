truncate table books;
drop table if exists books;

create table books (
    id uuid primary key default gen_random_uuid(),
    name varchar(255) not null,
    pages int not null,
    category_id int not null check (category_id >= 6 and category_id <= 10)
);

-- create index on category_id
create index ix_books_category_id on books(category_id);
