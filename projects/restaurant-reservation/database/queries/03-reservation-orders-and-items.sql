-- Requirement 3: Retrieve every order and line item for one reservation.
-- Run with psql -v reservation_id=<id> so the ID is not hardcoded here.
SELECT
    reservation.reservation_id,
    restaurant_order.order_id,
    restaurant_order.order_date,
    restaurant_order.total_amount AS order_total,
    menu_item.item_id,
    menu_item.name AS menu_item_name,
    order_item.quantity,
    order_item.unit_price,
    (order_item.quantity * order_item.unit_price)::numeric(12, 2) AS line_total
FROM reservations AS reservation
INNER JOIN orders AS restaurant_order
    ON restaurant_order.reservation_id = reservation.reservation_id
INNER JOIN order_items AS order_item
    ON order_item.order_id = restaurant_order.order_id
INNER JOIN menu_items AS menu_item
    ON menu_item.item_id = order_item.item_id
WHERE reservation.reservation_id = :reservation_id
ORDER BY restaurant_order.order_date, restaurant_order.order_id, order_item.order_item_id;
