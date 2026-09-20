BEGIN;

-- Seed only a new database. Failing fast avoids silently duplicating data when
-- this script is run manually against an existing environment.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM restaurants) THEN
        RAISE EXCEPTION 'Seed requires an empty database';
    END IF;
END;
$$;

INSERT INTO restaurants (name, address, phone_number, opening_hours)
SELECT
    format(
        '%s %s',
        (ARRAY[
            'Cedar', 'Olive', 'Saffron', 'Harbor', 'Garden',
            'Stone', 'Lantern', 'Jasmine', 'Terrace', 'Copper'
        ])[((restaurant_number - 1) % 10) + 1],
        (ARRAY['Kitchen', 'Bistro', 'Grill', 'Table', 'House'])[((restaurant_number - 1) / 10) + 1]
    ),
    format(
        '%s %s Street, District %s',
        10 + restaurant_number,
        (ARRAY['Market', 'Olive', 'Garden', 'Harbor', 'Cedar'])[((restaurant_number - 1) % 5) + 1],
        ((restaurant_number - 1) % 10) + 1
    ),
    format('+970-2-55%s', lpad(restaurant_number::text, 5, '0')),
    (ARRAY[
        '07:00-22:00',
        '08:00-23:00',
        '09:00-00:00',
        '11:00-23:30',
        '12:00-00:00'
    ])[((restaurant_number - 1) % 5) + 1]
FROM generate_series(1, 50) AS restaurant_number
ORDER BY restaurant_number;

WITH menu_catalog AS (
    SELECT *
    FROM unnest(
        ARRAY[
            'Margherita Pizza', 'Grilled Chicken', 'Beef Burger', 'Falafel Plate',
            'Classic Hummus', 'Caesar Salad', 'Lentil Soup', 'Chicken Shawarma',
            'Beef Kebab', 'Pasta Primavera', 'Seafood Rice', 'Vegetable Wrap',
            'French Fries', 'Garlic Bread', 'Cheesecake', 'Chocolate Cake',
            'Fresh Lemonade', 'Cola', 'Mineral Water', 'Turkish Coffee'
        ]::varchar[],
        ARRAY[
            'Stone-baked pizza with tomato, mozzarella, and basil.',
            'Herb-marinated chicken served with seasonal vegetables.',
            'Grilled beef patty with cheese and house sauce.',
            'Crisp falafel served with hummus, salad, and warm bread.',
            'Creamy chickpea dip finished with olive oil.',
            'Romaine lettuce, parmesan, croutons, and Caesar dressing.',
            'Slow-cooked lentils with cumin and lemon.',
            'Spiced chicken, pickles, garlic sauce, and flatbread.',
            'Char-grilled beef skewers with rice and vegetables.',
            'Pasta with seasonal vegetables and tomato herb sauce.',
            'Seasoned rice with grilled fish and lemon.',
            'Fresh vegetables, tahini, and herbs in flatbread.',
            'Golden potato fries with house seasoning.',
            'Toasted bread with garlic butter and herbs.',
            'Baked cheesecake with a biscuit crust.',
            'Rich chocolate layer cake.',
            'Fresh lemon juice, mint, and a touch of sugar.',
            'Chilled carbonated cola.',
            'Chilled still mineral water.',
            'Traditional finely ground coffee.'
        ]::text[],
        ARRAY[
            11.50, 16.00, 12.75, 8.50, 5.25,
            8.00, 6.00, 10.50, 17.50, 13.00,
            18.50, 9.25, 4.25, 4.75, 6.50,
            6.75, 3.75, 2.50, 1.75, 3.25
        ]::numeric[]
    ) WITH ORDINALITY AS catalog(item_name, item_description, base_price, item_number)
)
INSERT INTO menu_items (restaurant_id, name, description, price)
SELECT
    restaurant.restaurant_id,
    catalog.item_name,
    catalog.item_description,
    round(catalog.base_price + (((restaurant.restaurant_id - 1) % 5) * 0.25), 2)
FROM restaurants AS restaurant
CROSS JOIN menu_catalog AS catalog
ORDER BY restaurant.restaurant_id, catalog.item_number;

WITH employee_names AS (
    SELECT *
    FROM unnest(
        ARRAY[
            'Lina', 'Omar', 'Maya', 'Yousef', 'Nour',
            'Sami', 'Rana', 'Kareem', 'Hala', 'Tariq'
        ]::varchar[],
        ARRAY[
            'Haddad', 'Khalil', 'Nasser', 'Saleh', 'Mansour',
            'Awad', 'Darwish', 'Hamdan', 'Qasim', 'Shahin'
        ]::varchar[]
    ) WITH ORDINALITY AS names(first_name, last_name, name_number)
),
employee_slots AS (
    SELECT 1 AS employee_slot, 'Manager'::varchar AS position
    UNION ALL
    SELECT 2, NULL::varchar
)
INSERT INTO employees (restaurant_id, first_name, last_name, position)
SELECT
    restaurant.restaurant_id,
    names.first_name,
    names.last_name,
    CASE
        WHEN slot.employee_slot = 1 THEN slot.position
        ELSE (ARRAY[
            'VIPOrdersWaiter',
            'StandardWaiter',
            'AssistantWaiter'
        ])[((restaurant.restaurant_id - 1) % 3) + 1]
    END
FROM restaurants AS restaurant
CROSS JOIN employee_slots AS slot
JOIN employee_names AS names
  ON names.name_number = (((restaurant.restaurant_id * 2 + slot.employee_slot - 3) % 10) + 1)
ORDER BY restaurant.restaurant_id, slot.employee_slot;

WITH first_names AS (
    SELECT *
    FROM unnest(
        ARRAY[
            'Ahmad', 'Aya', 'Bilal', 'Dana', 'Elias',
            'Farah', 'George', 'Haneen', 'Ibrahim', 'Jana',
            'Khaled', 'Lama', 'Mahmoud', 'Nadia', 'Omar',
            'Rania', 'Samer', 'Tala', 'Waleed', 'Yara'
        ]::varchar[]
    ) WITH ORDINALITY AS first_name_values(first_name, first_number)
),
last_names AS (
    SELECT *
    FROM unnest(
        ARRAY[
            'Abbas', 'Barakat', 'Daoud', 'Essa', 'Farhat',
            'Ghannam', 'Habib', 'Issa', 'Jaber', 'Khoury',
            'Masri', 'Najjar', 'Odeh', 'Qattan', 'Rashed',
            'Sabbagh', 'Tannous', 'Yasin', 'Zayed', 'Zoubi'
        ]::varchar[]
    ) WITH ORDINALITY AS last_name_values(last_name, last_number)
),
customer_source AS (
    SELECT
        row_number() OVER (ORDER BY first_names.first_number, last_names.last_number) AS customer_number,
        first_names.first_name,
        last_names.last_name
    FROM first_names
    CROSS JOIN last_names
)
INSERT INTO customers (first_name, last_name, email, phone_number)
SELECT
    first_name,
    last_name,
    format(
        '%s.%s.%s@example.com',
        lower(first_name),
        lower(last_name),
        lpad(customer_number::text, 3, '0')
    ),
    format('+970-59-%s', lpad(customer_number::text, 6, '0'))
FROM customer_source
ORDER BY customer_number;

INSERT INTO restaurant_tables (restaurant_id, table_number, capacity)
SELECT
    restaurant.restaurant_id,
    table_number,
    CASE table_number WHEN 1 THEN 4 ELSE 6 END
FROM restaurants AS restaurant
CROSS JOIN generate_series(1, 2) AS table_number
ORDER BY restaurant.restaurant_id, table_number;

WITH reservation_slots AS (
    SELECT
        reservation_number,
        CASE WHEN reservation_number % 2 = 1 THEN 1 ELSE 2 END AS table_number,
        date_trunc('day', current_timestamp)
            + ((reservation_number - 5) * interval '14 days')
            + (interval '17 hours' + ((reservation_number % 3) * interval '1 hour'))
            AS reservation_date
    FROM generate_series(1, 10) AS reservation_number
)
INSERT INTO reservations (
    customer_id,
    restaurant_id,
    table_id,
    reservation_date,
    party_size
)
SELECT
    (((restaurant.restaurant_id - 1) * 10 + slot.reservation_number - 1) % 400) + 1,
    restaurant.restaurant_id,
    restaurant_table.table_id,
    slot.reservation_date,
    2 + ((restaurant.restaurant_id + slot.reservation_number) % 3)
FROM restaurants AS restaurant
CROSS JOIN reservation_slots AS slot
JOIN restaurant_tables AS restaurant_table
  ON restaurant_table.restaurant_id = restaurant.restaurant_id
 AND restaurant_table.table_number = slot.table_number
ORDER BY restaurant.restaurant_id, slot.reservation_number;

WITH ranked_reservations AS (
    SELECT
        reservation.reservation_id,
        reservation.restaurant_id,
        reservation.reservation_date,
        row_number() OVER (
            PARTITION BY reservation.restaurant_id
            ORDER BY reservation.reservation_date, reservation.reservation_id
        ) AS reservation_number
    FROM reservations AS reservation
),
restaurant_waiters AS (
    SELECT employee_id, restaurant_id
    FROM employees
    WHERE position <> 'Manager'
)
INSERT INTO orders (reservation_id, employee_id, order_date, total_amount)
SELECT
    reservation.reservation_id,
    waiter.employee_id,
    reservation.reservation_date + (order_number * interval '20 minutes'),
    0.00
FROM ranked_reservations AS reservation
JOIN restaurant_waiters AS waiter
  ON waiter.restaurant_id = reservation.restaurant_id
CROSS JOIN LATERAL generate_series(
    1,
    CASE WHEN reservation.reservation_number <= 2 THEN 2 ELSE 1 END
) AS order_number
WHERE reservation.reservation_number <= 8
ORDER BY reservation.restaurant_id, reservation.reservation_number, order_number;

WITH ranked_menu_items AS (
    SELECT
        menu_item.item_id,
        menu_item.restaurant_id,
        menu_item.price,
        row_number() OVER (
            PARTITION BY menu_item.restaurant_id
            ORDER BY menu_item.item_id
        ) AS item_number
    FROM menu_items AS menu_item
),
order_context AS (
    SELECT
        restaurant_order.order_id,
        reservation.restaurant_id
    FROM orders AS restaurant_order
    JOIN reservations AS reservation
      ON reservation.reservation_id = restaurant_order.reservation_id
)
INSERT INTO order_items (order_id, item_id, quantity, unit_price)
SELECT
    order_context.order_id,
    menu_item.item_id,
    1 + ((order_context.order_id + line_number) % 3),
    menu_item.price
FROM order_context
CROSS JOIN generate_series(1, 3) AS line_number
JOIN ranked_menu_items AS menu_item
  ON menu_item.restaurant_id = order_context.restaurant_id
 AND menu_item.item_number = (((order_context.order_id * 3 + line_number * 5 - 1) % 20) + 1)
ORDER BY order_context.order_id, line_number;

UPDATE orders AS restaurant_order
SET total_amount = calculated.total_amount
FROM (
    SELECT
        order_id,
        sum(quantity * unit_price)::numeric(12, 2) AS total_amount
    FROM order_items
    GROUP BY order_id
) AS calculated
WHERE calculated.order_id = restaurant_order.order_id;

COMMIT;
