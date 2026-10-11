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