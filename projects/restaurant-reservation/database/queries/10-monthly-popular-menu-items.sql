WITH
    item_sales AS (
        SELECT
            restaurant.restaurant_id,
            restaurant.name AS restaurant_name,
            menu_item.item_id,
            menu_item.name AS menu_item_name,
            SUM(order_item.quantity) AS quantity_sold
        FROM
            restaurants AS restaurant
            -- This join for reservations belong to restaurant.
            JOIN reservations AS reservation ON reservation.restaurant_id = restaurant.restaurant_id
            -- This join for orders that created for a reservation.
            JOIN orders AS restaurant_order ON restaurant_order.reservation_id = reservation.reservation_id
            -- This join to find individual items inside each order.
            JOIN order_items AS order_item ON order_item.order_id = restaurant_order.order_id
            -- This join to find the menu items names from menu_item table.
            JOIN menu_items AS menu_item ON menu_item.item_id = order_item.item_id
        WHERE
            restaurant_order.order_date >= DATE '2026-09-01'
            AND restaurant_order.order_date < DATE '2026-10-01'
        GROUP BY
            restaurant.restaurant_id,
            restaurant.name,
            menu_item.item_id,
            menu_item.name
        ORDER BY
            -- sort by restaurant then quantity sold
            restaurant.restaurant_id,
            quantity_sold DESC
    ),
    -- rank from most populer to least
    ranked_items AS (
        SELECT
            item_sales.*,
            DENSE_RANK() OVER (
                PARTITION BY
                    restaurant_id
                ORDER BY
                    quantity_sold DESC
            ) AS popularity_rank
        FROM
            item_sales
    )
SELECT
    restaurant_id,
    restaurant_name,
    item_id,
    menu_item_name,
    quantity_sold
FROM
    ranked_items
WHERE
    popularity_rank = 1
ORDER BY
    restaurant_name,
    menu_item_name;