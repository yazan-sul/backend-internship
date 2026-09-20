DO $$
DECLARE
    actual_count bigint;
    invalid_count bigint;
BEGIN
    SELECT count(*) INTO actual_count FROM restaurants;
    IF actual_count <> 50 THEN
        RAISE EXCEPTION 'Expected 50 restaurants, found %', actual_count;
    END IF;

    SELECT count(*) INTO actual_count FROM menu_items;
    IF actual_count <> 1000 THEN
        RAISE EXCEPTION 'Expected 1000 menu items, found %', actual_count;
    END IF;

    SELECT count(*) INTO actual_count FROM order_items;
    IF actual_count <> 1500 THEN
        RAISE EXCEPTION 'Expected 1500 order items, found %', actual_count;
    END IF;

    SELECT count(*) INTO actual_count FROM orders;
    IF actual_count <> 500 THEN
        RAISE EXCEPTION 'Expected 500 orders, found %', actual_count;
    END IF;

    SELECT count(*) INTO actual_count FROM employees;
    IF actual_count <> 100 THEN
        RAISE EXCEPTION 'Expected 100 employees, found %', actual_count;
    END IF;

    SELECT count(*) INTO actual_count FROM reservations;
    IF actual_count <> 500 THEN
        RAISE EXCEPTION 'Expected 500 reservations, found %', actual_count;
    END IF;

    SELECT count(*) INTO actual_count FROM customers;
    IF actual_count <> 400 THEN
        RAISE EXCEPTION 'Expected 400 customers, found %', actual_count;
    END IF;

    SELECT count(*) INTO actual_count FROM restaurant_tables;
    IF actual_count <> 100 THEN
        RAISE EXCEPTION 'Expected 100 restaurant tables, found %', actual_count;
    END IF;

    SELECT count(*)
    INTO invalid_count
    FROM (
        SELECT restaurant_id
        FROM menu_items
        GROUP BY restaurant_id
        HAVING count(*) <> 20
    ) AS invalid_restaurants;
    IF invalid_count <> 0 THEN
        RAISE EXCEPTION '% restaurants do not have exactly 20 menu items', invalid_count;
    END IF;

    SELECT count(*)
    INTO invalid_count
    FROM (
        SELECT restaurant_id
        FROM employees
        GROUP BY restaurant_id
        HAVING count(*) <> 2
    ) AS invalid_restaurants;
    IF invalid_count <> 0 THEN
        RAISE EXCEPTION '% restaurants do not have exactly 2 employees', invalid_count;
    END IF;

    SELECT count(*)
    INTO invalid_count
    FROM (
        SELECT restaurant_id
        FROM restaurant_tables
        GROUP BY restaurant_id
        HAVING count(*) <> 2
    ) AS invalid_restaurants;
    IF invalid_count <> 0 THEN
        RAISE EXCEPTION '% restaurants do not have exactly 2 tables', invalid_count;
    END IF;

    SELECT count(*)
    INTO invalid_count
    FROM reservations AS reservation
    JOIN restaurant_tables AS restaurant_table
      ON restaurant_table.table_id = reservation.table_id
    WHERE restaurant_table.restaurant_id <> reservation.restaurant_id
       OR reservation.party_size > restaurant_table.capacity;
    IF invalid_count <> 0 THEN
        RAISE EXCEPTION 'Found % invalid reservation/table assignments', invalid_count;
    END IF;

    SELECT count(*)
    INTO invalid_count
    FROM orders AS restaurant_order
    JOIN reservations AS reservation
      ON reservation.reservation_id = restaurant_order.reservation_id
    JOIN employees AS employee
      ON employee.employee_id = restaurant_order.employee_id
    WHERE employee.restaurant_id <> reservation.restaurant_id;
    IF invalid_count <> 0 THEN
        RAISE EXCEPTION 'Found % orders handled by another restaurant employee', invalid_count;
    END IF;

    SELECT count(*)
    INTO invalid_count
    FROM order_items AS order_item
    JOIN orders AS restaurant_order
      ON restaurant_order.order_id = order_item.order_id
    JOIN reservations AS reservation
      ON reservation.reservation_id = restaurant_order.reservation_id
    JOIN menu_items AS menu_item
      ON menu_item.item_id = order_item.item_id
    WHERE menu_item.restaurant_id <> reservation.restaurant_id;
    IF invalid_count <> 0 THEN
        RAISE EXCEPTION 'Found % order items from another restaurant menu', invalid_count;
    END IF;

    SELECT count(*)
    INTO invalid_count
    FROM orders AS restaurant_order
    JOIN (
        SELECT order_id, sum(quantity * unit_price)::numeric(12, 2) AS calculated_total
        FROM order_items
        GROUP BY order_id
    ) AS calculated
      ON calculated.order_id = restaurant_order.order_id
    WHERE restaurant_order.total_amount <> calculated.calculated_total;
    IF invalid_count <> 0 THEN
        RAISE EXCEPTION 'Found % orders with incorrect totals', invalid_count;
    END IF;

    SELECT count(*)
    INTO invalid_count
    FROM (
        SELECT order_id
        FROM order_items
        GROUP BY order_id
        HAVING count(*) <> 3
    ) AS invalid_orders;
    IF invalid_count <> 0 THEN
        RAISE EXCEPTION '% orders do not have exactly 3 line items', invalid_count;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM reservations AS reservation
        LEFT JOIN orders AS restaurant_order
          ON restaurant_order.reservation_id = reservation.reservation_id
        WHERE restaurant_order.order_id IS NULL
    ) THEN
        RAISE EXCEPTION 'Expected some reservations without orders';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM orders
        GROUP BY reservation_id
        HAVING count(*) >= 2
    ) THEN
        RAISE EXCEPTION 'Expected some reservations with multiple orders';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM reservations WHERE reservation_date < current_timestamp)
       OR NOT EXISTS (SELECT 1 FROM reservations WHERE reservation_date > current_timestamp) THEN
        RAISE EXCEPTION 'Expected both past and future reservations';
    END IF;

    IF (
        SELECT count(DISTINCT position)
        FROM employees
        WHERE position IN ('Manager', 'VIPOrdersWaiter', 'StandardWaiter', 'AssistantWaiter')
    ) <> 4 THEN
        RAISE EXCEPTION 'Not all required employee positions are represented';
    END IF;
END;
$$;

SELECT 'restaurants' AS entity, count(*) AS record_count FROM restaurants
UNION ALL
SELECT 'menu_items', count(*) FROM menu_items
UNION ALL
SELECT 'order_items', count(*) FROM order_items
UNION ALL
SELECT 'orders', count(*) FROM orders
UNION ALL
SELECT 'employees', count(*) FROM employees
UNION ALL
SELECT 'reservations', count(*) FROM reservations
UNION ALL
SELECT 'customers', count(*) FROM customers
UNION ALL
SELECT 'restaurant_tables', count(*) FROM restaurant_tables
ORDER BY entity;

