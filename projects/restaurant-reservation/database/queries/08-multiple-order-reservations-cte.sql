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
    restaurant.name AS restaurant_name,
FROM
    reservation_order_summary AS order_summary
    JOIN reservations AS reservation ON reservation.reservation_id = order_summary.reservation_id
    JOIN customers as customer on customer.customer_id = reservation.customer_id
    JOIN restaurants AS restaurant ON restaurant.restaurant_id = reservation.restaurant_id
ORDER BY
    order_summary.order_count DESC,
    order_summary.total_spent DESC,
    reservation.reservation_id;