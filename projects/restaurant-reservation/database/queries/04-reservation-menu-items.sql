SELECT
    menu_item.item_id,
    menu_item.name AS menu_item_name,
    menu_item.description,
    menu_item.price AS current_menu_price
FROM
    menu_items AS menu_item
WHERE
    EXISTS (
        SELECT
            1
        FROM
            orders AS restaurant_order
            JOIN order_items AS order_item ON order_item.order_id = restaurant_order.order_id
        WHERE
            restaurant_order.reservation_id = 1
            AND order_item.item_id = menu_item.item_id
    )
ORDER BY
    menu_item.name,
    menu_item.item_id;