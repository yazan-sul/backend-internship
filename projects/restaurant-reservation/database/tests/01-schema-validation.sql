BEGIN;

DO $$
DECLARE
    first_restaurant_id integer;
    second_restaurant_id integer;
    customer_id_value integer;
    employee_id_value integer;
    table_id_value integer;
    menu_item_id_value integer;
    reservation_id_value integer;
    order_id_value integer;
    table_count integer;
BEGIN
    SELECT count(*)
    INTO table_count
    FROM information_schema.tables
    WHERE table_schema = 'public'
      AND table_type = 'BASE TABLE'
      AND table_name IN (
          'restaurants',
          'menu_items',
          'employees',
          'customers',
          'restaurant_tables',
          'reservations',
          'orders',
          'order_items',
          'audit_log'
      );

    IF table_count <> 9 THEN
        RAISE EXCEPTION 'Expected 9 project tables, found %', table_count;
    END IF;

    INSERT INTO restaurants (name, address, phone_number, opening_hours)
    VALUES ('Schema Test One', '1 Test Street', '+970-2-000-0001', '09:00-23:00')
    RETURNING restaurant_id INTO first_restaurant_id;

    INSERT INTO restaurants (name, address, phone_number, opening_hours)
    VALUES ('Schema Test Two', '2 Test Street', '+970-2-000-0002', '09:00-23:00')
    RETURNING restaurant_id INTO second_restaurant_id;

    INSERT INTO customers (first_name, last_name, email, phone_number)
    VALUES ('Schema', 'Customer', 'schema.customer@example.test', '+970-59-000-0001')
    RETURNING customer_id INTO customer_id_value;

    INSERT INTO employees (restaurant_id, first_name, last_name, position)
    VALUES (first_restaurant_id, 'Schema', 'Waiter', 'StandardWaiter')
    RETURNING employee_id INTO employee_id_value;

    INSERT INTO restaurant_tables (restaurant_id, table_number, capacity)
    VALUES (first_restaurant_id, 1, 4)
    RETURNING table_id INTO table_id_value;

    INSERT INTO menu_items (restaurant_id, name, description, price)
    VALUES (first_restaurant_id, 'Test Pizza', 'Pizza used by schema validation.', 12.00)
    RETURNING item_id INTO menu_item_id_value;

    INSERT INTO reservations (
        customer_id,
        restaurant_id,
        table_id,
        reservation_date,
        party_size
    )
    VALUES (
        customer_id_value,
        first_restaurant_id,
        table_id_value,
        timestamptz '2030-01-01 18:00:00+00',
        2
    )
    RETURNING reservation_id INTO reservation_id_value;

    INSERT INTO orders (reservation_id, employee_id, order_date, total_amount)
    VALUES (
        reservation_id_value,
        employee_id_value,
        timestamptz '2030-01-01 18:15:00+00',
        24.00
    )
    RETURNING order_id INTO order_id_value;

    INSERT INTO order_items (order_id, item_id, quantity, unit_price)
    VALUES (order_id_value, menu_item_id_value, 2, 12.00);

    IF NOT EXISTS (
        SELECT 1
        FROM orders AS o
        JOIN order_items AS oi ON oi.order_id = o.order_id
        WHERE o.order_id = order_id_value
        GROUP BY o.order_id, o.total_amount
        HAVING o.total_amount = sum(oi.quantity * oi.unit_price)
    ) THEN
        RAISE EXCEPTION 'Order total does not match its validated order items';
    END IF;

    BEGIN
        INSERT INTO reservations (
            customer_id,
            restaurant_id,
            table_id,
            reservation_date,
            party_size
        )
        VALUES (
            customer_id_value,
            first_restaurant_id,
            table_id_value,
            timestamptz '2030-01-01 18:00:00+00',
            2
        );

        RAISE EXCEPTION 'Duplicate table booking was accepted';
    EXCEPTION
        WHEN unique_violation THEN
            NULL;
    END;

    BEGIN
        INSERT INTO reservations (
            customer_id,
            restaurant_id,
            table_id,
            reservation_date,
            party_size
        )
        VALUES (
            customer_id_value,
            second_restaurant_id,
            table_id_value,
            timestamptz '2030-01-02 18:00:00+00',
            2
        );

        RAISE EXCEPTION 'Reservation accepted a table from another restaurant';
    EXCEPTION
        WHEN foreign_key_violation THEN
            NULL;
    END;

    BEGIN
        INSERT INTO customers (first_name, last_name, email, phone_number)
        VALUES ('Duplicate', 'Email', 'SCHEMA.CUSTOMER@EXAMPLE.TEST', '+970-59-000-0002');

        RAISE EXCEPTION 'Case-insensitive duplicate email was accepted';
    EXCEPTION
        WHEN unique_violation THEN
            NULL;
    END;
END;
$$;

ROLLBACK;

