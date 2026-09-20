BEGIN;

CREATE TABLE restaurants (
    restaurant_id integer GENERATED ALWAYS AS IDENTITY,
    name varchar(120) NOT NULL,
    address varchar(250) NOT NULL,
    phone_number varchar(30) NOT NULL,
    opening_hours varchar(120) NOT NULL,
    CONSTRAINT pk_restaurants PRIMARY KEY (restaurant_id),
    CONSTRAINT ck_restaurants_name_not_blank CHECK (btrim(name) <> ''),
    CONSTRAINT ck_restaurants_address_not_blank CHECK (btrim(address) <> ''),
    CONSTRAINT ck_restaurants_phone_not_blank CHECK (btrim(phone_number) <> ''),
    CONSTRAINT ck_restaurants_opening_hours_not_blank CHECK (btrim(opening_hours) <> '')
);

CREATE TABLE menu_items (
    item_id integer GENERATED ALWAYS AS IDENTITY,
    restaurant_id integer NOT NULL,
    name varchar(120) NOT NULL,
    description text NOT NULL,
    price numeric(12, 2) NOT NULL,
    CONSTRAINT pk_menu_items PRIMARY KEY (item_id),
    CONSTRAINT fk_menu_items_restaurant
        FOREIGN KEY (restaurant_id) REFERENCES restaurants (restaurant_id),
    CONSTRAINT uq_menu_items_restaurant_name UNIQUE (restaurant_id, name),
    CONSTRAINT ck_menu_items_name_not_blank CHECK (btrim(name) <> ''),
    CONSTRAINT ck_menu_items_description_not_blank CHECK (btrim(description) <> ''),
    CONSTRAINT ck_menu_items_price_positive CHECK (price > 0)
);

CREATE TABLE employees (
    employee_id integer GENERATED ALWAYS AS IDENTITY,
    restaurant_id integer NOT NULL,
    first_name varchar(80) NOT NULL,
    last_name varchar(80) NOT NULL,
    position varchar(40) NOT NULL,
    CONSTRAINT pk_employees PRIMARY KEY (employee_id),
    CONSTRAINT fk_employees_restaurant
        FOREIGN KEY (restaurant_id) REFERENCES restaurants (restaurant_id),
    CONSTRAINT ck_employees_first_name_not_blank CHECK (btrim(first_name) <> ''),
    CONSTRAINT ck_employees_last_name_not_blank CHECK (btrim(last_name) <> ''),
    CONSTRAINT ck_employees_position CHECK (
        position IN ('Manager', 'VIPOrdersWaiter', 'StandardWaiter', 'AssistantWaiter')
    )
);

CREATE TABLE customers (
    customer_id integer GENERATED ALWAYS AS IDENTITY,
    first_name varchar(80) NOT NULL,
    last_name varchar(80) NOT NULL,
    email varchar(254) NOT NULL,
    phone_number varchar(30) NOT NULL,
    CONSTRAINT pk_customers PRIMARY KEY (customer_id),
    CONSTRAINT ck_customers_first_name_not_blank CHECK (btrim(first_name) <> ''),
    CONSTRAINT ck_customers_last_name_not_blank CHECK (btrim(last_name) <> ''),
    CONSTRAINT ck_customers_email_not_blank CHECK (btrim(email) <> ''),
    CONSTRAINT ck_customers_phone_not_blank CHECK (btrim(phone_number) <> '')
);

-- This functional unique index treats differently-cased versions of one email
-- address as the same customer identity.
CREATE UNIQUE INDEX uq_customers_email_ci ON customers (lower(email));

CREATE TABLE restaurant_tables (
    table_id integer GENERATED ALWAYS AS IDENTITY,
    restaurant_id integer NOT NULL,
    table_number integer NOT NULL,
    capacity integer NOT NULL,
    CONSTRAINT pk_restaurant_tables PRIMARY KEY (table_id),
    CONSTRAINT fk_restaurant_tables_restaurant
        FOREIGN KEY (restaurant_id) REFERENCES restaurants (restaurant_id),
    CONSTRAINT uq_restaurant_tables_number UNIQUE (restaurant_id, table_number),
    CONSTRAINT uq_restaurant_tables_id_restaurant UNIQUE (table_id, restaurant_id),
    CONSTRAINT ck_restaurant_tables_number_positive CHECK (table_number > 0),
    CONSTRAINT ck_restaurant_tables_capacity_positive CHECK (capacity > 0)
);

CREATE TABLE reservations (
    reservation_id integer GENERATED ALWAYS AS IDENTITY,
    customer_id integer NOT NULL,
    restaurant_id integer NOT NULL,
    table_id integer NOT NULL,
    reservation_date timestamptz NOT NULL,
    party_size integer NOT NULL,
    CONSTRAINT pk_reservations PRIMARY KEY (reservation_id),
    CONSTRAINT fk_reservations_customer
        FOREIGN KEY (customer_id) REFERENCES customers (customer_id),
    CONSTRAINT fk_reservations_restaurant
        FOREIGN KEY (restaurant_id) REFERENCES restaurants (restaurant_id),
    CONSTRAINT fk_reservations_table_restaurant
        FOREIGN KEY (table_id, restaurant_id)
        REFERENCES restaurant_tables (table_id, restaurant_id),
    CONSTRAINT uq_reservations_table_date UNIQUE (table_id, reservation_date),
    CONSTRAINT ck_reservations_party_size_positive CHECK (party_size > 0)
);

CREATE TABLE orders (
    order_id integer GENERATED ALWAYS AS IDENTITY,
    reservation_id integer NOT NULL,
    employee_id integer NOT NULL,
    order_date timestamptz NOT NULL,
    total_amount numeric(12, 2) NOT NULL,
    CONSTRAINT pk_orders PRIMARY KEY (order_id),
    CONSTRAINT fk_orders_reservation
        FOREIGN KEY (reservation_id) REFERENCES reservations (reservation_id),
    CONSTRAINT fk_orders_employee
        FOREIGN KEY (employee_id) REFERENCES employees (employee_id),
    CONSTRAINT ck_orders_total_amount_nonnegative CHECK (total_amount >= 0)
);

CREATE TABLE order_items (
    order_item_id integer GENERATED ALWAYS AS IDENTITY,
    order_id integer NOT NULL,
    item_id integer NOT NULL,
    quantity integer NOT NULL,
    unit_price numeric(12, 2) NOT NULL,
    CONSTRAINT pk_order_items PRIMARY KEY (order_item_id),
    CONSTRAINT fk_order_items_order
        FOREIGN KEY (order_id) REFERENCES orders (order_id),
    CONSTRAINT fk_order_items_menu_item
        FOREIGN KEY (item_id) REFERENCES menu_items (item_id),
    CONSTRAINT uq_order_items_order_item UNIQUE (order_id, item_id),
    CONSTRAINT ck_order_items_quantity_positive CHECK (quantity > 0),
    CONSTRAINT ck_order_items_unit_price_nonnegative CHECK (unit_price >= 0)
);

CREATE TABLE audit_log (
    audit_log_id integer GENERATED ALWAYS AS IDENTITY,
    restaurant_id integer NOT NULL,
    table_id integer NOT NULL,
    reservation_date timestamptz NOT NULL,
    change_date timestamptz NOT NULL DEFAULT current_timestamp,
    CONSTRAINT pk_audit_log PRIMARY KEY (audit_log_id)
);

COMMENT ON TABLE restaurant_tables IS
    'Physical dining tables owned by restaurants.';
COMMENT ON COLUMN order_items.unit_price IS
    'Price charged when the order was placed; retained when menu prices change.';
COMMENT ON TABLE audit_log IS
    'Append-only reservation snapshots populated by the reservation audit trigger.';

COMMIT;

