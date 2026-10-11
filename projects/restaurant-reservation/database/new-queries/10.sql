SELECT
    restaurants.name AS restaurant_name,
    menu_items.name AS menu_item_name,
    SUM(order_items.quantity) AS total_sold
FROM
    restaurants
    JOIN menu_items ON restaurants.restaurant_id = menu_items.restaurant_id
    JOIN order_items ON menu_items.item_id = order_items.item_id
    JOIN orders ON order_items.order_id = orders.order_id
WHERE
    EXTRACT(
        MONTH
        FROM
            orders.order_date
    ) = 7
GROUP BY
    restaurants.restaurant_id,
    restaurants.name,
    menu_items.item_id,
    menu_items.name
ORDER BY
    restaurants.name,
    total_sold DESC;