-- CREATE OR REPLACE FUNCTION fn_CalculateRevenue(RestaurantId integer)
--   RETURNS numeric
--   AS $$
--   SELECT
--     SUM(restaurant_order.total_amount) AS total_revenue
--   FROM
--     reservations AS reservation
--     JOIN orders AS restaurant_order ON restaurant_order.reservation_id = reservation.reservation_id
--   WHERE
--     reservation.restaurant_id = RestaurantId
--     $$;
-- SELECT fn_CalculateRevenue(1), fn_CalculateRevenue(2);
-- -- docker compose exec db psql -U postgres -d restaurant_reservation \    -c "SELECT fn_CalculateRevenue(1), fn_CalculateRevenue(2);"
CREATE OR REPLACE FUNCTION Menu_Items(OrderId integer)
  RETURNS TABLE (
    name text,
    price numeric
  )
  Language sql
  AS $$ 
SELECT
  name,
  unit_price AS price
FROM
  orders
  JOIN order_items ON orders.order_id = order_items.order_id
  JOIN menu_items ON order_items.item_id = menu_items.item_id
  WHERE orders.order_id = OrderID
  $$;

  SELECT * FROM Menu_Items(2);