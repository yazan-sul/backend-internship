-- Requirement 10: Rank menu items by monthly popularity at each restaurant.
WITH monthly_item_sales AS (
    SELECT
        date_trunc('month', restaurant_order.order_date)::date AS sales_month,
        restaurant.restaurant_id,
        restaurant.name AS restaurant_name,
        menu_item.item_id,
        menu_item.name AS menu_item_name,
        sum(order_item.quantity) AS quantity_sold,
        count(DISTINCT restaurant_order.order_id) AS order_count,
        sum(order_item.quantity * order_item.unit_price)::numeric(12, 2) AS revenue
    FROM orders AS restaurant_order
    INNER JOIN reservations AS reservation
        ON reservation.reservation_id = restaurant_order.reservation_id
    INNER JOIN restaurants AS restaurant
        ON restaurant.restaurant_id = reservation.restaurant_id
    INNER JOIN order_items AS order_item
        ON order_item.order_id = restaurant_order.order_id
    INNER JOIN menu_items AS menu_item
        ON menu_item.item_id = order_item.item_id
    GROUP BY
        date_trunc('month', restaurant_order.order_date)::date,
        restaurant.restaurant_id,
        restaurant.name,
        menu_item.item_id,
        menu_item.name
),
ranked_monthly_items AS (
    SELECT
        monthly_item_sales.*,
        dense_rank() OVER (
            PARTITION BY sales_month, restaurant_id
            ORDER BY quantity_sold DESC
        ) AS popularity_rank
    FROM monthly_item_sales
)
SELECT
    sales_month,
    restaurant_id,
    restaurant_name,
    item_id,
    menu_item_name,
    quantity_sold,
    order_count,
    revenue,
    popularity_rank
FROM ranked_monthly_items
ORDER BY
    sales_month DESC,
    restaurant_name,
    popularity_rank,
    revenue DESC,
    menu_item_name,
    item_id;
