-- Requirement 1: Retrieve the reservation history for one customer.
-- Run with psql -v customer_id=<id> so the ID is not hardcoded here.
SELECT
    customer.customer_id,
    concat(customer.first_name, ' ', customer.last_name) AS customer_name,
    customer.email,
    reservation.reservation_id,
    restaurant.name AS restaurant_name,
    restaurant_table.table_number,
    reservation.reservation_date,
    reservation.party_size
FROM customers AS customer
INNER JOIN reservations AS reservation
    ON reservation.customer_id = customer.customer_id
INNER JOIN restaurants AS restaurant
    ON restaurant.restaurant_id = reservation.restaurant_id
INNER JOIN restaurant_tables AS restaurant_table
    ON restaurant_table.table_id = reservation.table_id
WHERE customer.customer_id = :customer_id
ORDER BY reservation.reservation_date DESC, reservation.reservation_id DESC;
