WITH
    reservation_order AS (
        SELECT
            reservation.reservation_id,
            COUNT(orders.order_id) AS total_orders
        FROM
            reservations AS reservation
            JOIN orders ON orders.reservation_id = reservation.reservation_id
        GROUP BY
            reservation.reservation_id
        HAVING
            COUNT(orders.order_id) >= 2
    )
SELECT
    reservation_order.reservation_id,
    reservation_order.total_orders
FROM
    reservation_order;