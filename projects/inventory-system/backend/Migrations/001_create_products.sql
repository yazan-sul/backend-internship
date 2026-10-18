-- Initial inventory schema: the product fields required by the API.
CREATE TABLE IF NOT EXISTS products (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name varchar(200) NOT NULL,
    price numeric(12, 2) NOT NULL CHECK (price >= 0),
    quantity integer NOT NULL CHECK (quantity >= 0),
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Product names are unique without treating capitalization as significant.
CREATE UNIQUE INDEX IF NOT EXISTS ix_products_name_lower
    ON products (LOWER(name));
