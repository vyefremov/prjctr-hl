create table books (
    id uuid primary key default gen_random_uuid(),
    name varchar(255) not null,
    pages int not null,
    category_id int not null
);

create index ix_books_category_id on books(category_id);

insert into books (name, pages, category_id)
    select
        'book_' || generate_series(1, 1000000) AS id,
        (random() * 999 + 1)::int AS pages,
        (random() * 9 + 1)::int AS category_id;

select
    ct as category_id,
    (select count(*) from books as b where b.category_id = ct) as count_by_category
from generate_series(1, 10) as ct;

select count(*) as total_by_cat, sum(pages) as pages_by_cat from books where category_id = 3; -- books_vw for fdw

select count(*) as total_by_cat, sum(pages) as pages_by_cat from books where category_id = 7; -- books_vw for fdw
