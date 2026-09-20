-- Requirement 4: Retrieve the distinct menu items ordered during one reservation.
-- Run with psql -v reservation_id=<id> so the ID is not hardcoded here.
SELECT
    menu_item.item_id,
    menu_item.name AS menu_item_name,
    menu_item.description,
    menu_item.price AS current_menu_price
FROM menu_items AS menu_item
WHERE EXISTS (
    SELECT 1
    FROM order_items AS order_item
    INNER JOIN orders AS restaurant_order
        ON restaurant_order.order_id = order_item.order_id
    WHERE order_item.item_id = menu_item.item_id
      AND restaurant_order.reservation_id = :reservation_id
)
ORDER BY menu_item.name, menu_item.item_id;
