-- Requirement 8: Use a CTE to retrieve reservations with two or more orders.
WITH reservation_order_summary AS (
    SELECT
        restaurant_order.reservation_id,
        count(restaurant_order.order_id) AS order_count,
        sum(restaurant_order.total_amount)::numeric(12, 2) AS total_spent
    FROM orders AS restaurant_order
    GROUP BY restaurant_order.reservation_id
    HAVING count(restaurant_order.order_id) >= 2
)
SELECT
    reservation.reservation_id,
    reservation.reservation_date,
    concat(customer.first_name, ' ', customer.last_name) AS customer_name,
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    order_summary.order_count,
    order_summary.total_spent
FROM reservation_order_summary AS order_summary
INNER JOIN reservations AS reservation
    ON reservation.reservation_id = order_summary.reservation_id
INNER JOIN customers AS customer
    ON customer.customer_id = reservation.customer_id
INNER JOIN restaurants AS restaurant
    ON restaurant.restaurant_id = reservation.restaurant_id
ORDER BY
    order_summary.order_count DESC,
    order_summary.total_spent DESC,
    reservation.reservation_id;
