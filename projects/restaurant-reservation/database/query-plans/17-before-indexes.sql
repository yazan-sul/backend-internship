EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
SELECT
    reservation.reservation_id,
    restaurant_order.order_id,
    restaurant_order.order_date,
    restaurant_order.total_amount AS order_total,
    -- order items
    order_item.order_item_id,
    order_item.item_id,
    order_item.quantity,
    order_item.unit_price
FROM
    reservations AS reservation
    JOIN orders AS restaurant_order ON restaurant_order.reservation_id = reservation.reservation_id
    JOIN order_items AS order_item ON order_item.order_id = restaurant_order.order_id
    JOIN menu_items AS menu_item ON menu_item.item_id = order_item.item_id
WHERE
    reservation.reservation_id = 1
ORDER BY
    restaurant_order.order_date,
    restaurant_order.order_id,
    order_item.order_item_id;

-- Query 2: Reservations with two or more orders
EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
WITH
    reservation_order_summary AS (
        SELECT
            restaurant_order.reservation_id,
            COUNT(restaurant_order.order_id) AS order_count,
            SUM(restaurant_order.total_amount) AS total_spent
        FROM
            orders AS restaurant_order
        GROUP BY
            restaurant_order.reservation_id
        HAVING
            COUNT(restaurant_order.order_id) >= 2
    )
SELECT
    reservation.reservation_id,
    reservation.reservation_date,
    order_summary.order_count,
    order_summary.total_spent,
    -- customer info
    CONCAT (customer.first_name, ' ', customer.last_name) AS customer_name,
    -- restaurant info
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name
FROM
    reservation_order_summary AS order_summary
    JOIN reservations AS reservation ON reservation.reservation_id = order_summary.reservation_id
    JOIN customers as customer on customer.customer_id = reservation.customer_id
    JOIN restaurants AS restaurant ON restaurant.restaurant_id = reservation.restaurant_id
ORDER BY
    order_summary.order_count DESC,
    order_summary.total_spent DESC,
    reservation.reservation_id;

-- Query 3: Most popular menu items for a given month
EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)
WITH
    item_sales AS (
        SELECT
            restaurant.restaurant_id,
            restaurant.name AS restaurant_name,
            menu_item.item_id,
            menu_item.name AS menu_item_name,
            SUM(order_item.quantity) AS quantity_sold
        FROM
            restaurants AS restaurant
            -- This join for reservations belong to restaurant.
            JOIN reservations AS reservation ON reservation.restaurant_id = restaurant.restaurant_id
            -- This join for orders that created for a reservation.
            JOIN orders AS restaurant_order ON restaurant_order.reservation_id = reservation.reservation_id
            -- This join to find individual items inside each order.
            JOIN order_items AS order_item ON order_item.order_id = restaurant_order.order_id
            -- This join to find the menu items names from menu_item table.
            JOIN menu_items AS menu_item ON menu_item.item_id = order_item.item_id
        WHERE
            restaurant_order.order_date >= DATE '2026-09-01'
            AND restaurant_order.order_date < DATE '2026-10-01'
        GROUP BY
            restaurant.restaurant_id,
            restaurant.name,
            menu_item.item_id,
            menu_item.name
        ORDER BY
            -- sort by restaurant then quantity sold
            restaurant.restaurant_id,
            quantity_sold DESC
    ),
    -- rank from most populer to least
    ranked_items AS (
        SELECT
            item_sales.*,
            DENSE_RANK() OVER (
                PARTITION BY
                    restaurant_id
                ORDER BY
                    quantity_sold DESC
            ) AS popularity_rank
        FROM
            item_sales
    )
SELECT
    restaurant_id,
    restaurant_name,
    item_id,
    menu_item_name,
    quantity_sold
FROM
    ranked_items
WHERE
    popularity_rank = 1
ORDER BY
    restaurant_name,
    menu_item_name;