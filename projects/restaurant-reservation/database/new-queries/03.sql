SELECT
    reservation.reservation_id,
    orders.order_id,
    order_item.unit_price,
    menu_item.name AS menu_item_name
FROM
    orders
    JOIN reservations AS reservation ON reservation.reservation_id = orders.reservation_id
    JOIN order_items AS order_item ON order_item.order_id = orders.order_id
    JOIN menu_items AS menu_item ON menu_item.item_id = order_item.item_id
WHERE
    reservation.reservation_id = 1