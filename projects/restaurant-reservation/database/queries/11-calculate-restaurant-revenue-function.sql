 CREATE OR REPLACE FUNCTION fn_CalculateRevenue(RestaurantId integer)
  RETURNS numeric
  LANGUAGE sql
  AS $$
      SELECT SUM(restaurant_order.total_amount)
      FROM reservations AS reservation
      JOIN orders AS restaurant_order
          ON restaurant_order.reservation_id = reservation.reservation_id
      WHERE reservation.restaurant_id = RestaurantId;
  $$;