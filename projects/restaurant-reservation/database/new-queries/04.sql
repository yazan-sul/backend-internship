SELECT
    reservation.reservation_id,
    orders.order_id,
    menu_items.name AS menu_item_name,
    order_items.unit_price
FROM
    reservations AS reservation
    JOIN orders ON orders.reservation_id = reservation.reservation_id
    JOIN order_items ON order_items.order_id = orders.order_id
    JOIN menu_items ON menu_items.item_id = order_items.item_id
WHERE
    reservation.reservation_id = 1
ORDER BY
    orders.order_date DESC;