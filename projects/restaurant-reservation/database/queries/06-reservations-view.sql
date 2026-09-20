-- Requirement 6: Create a reusable view of reservation details.
CREATE OR REPLACE VIEW reservation_details AS
SELECT
    reservation.reservation_id,
    reservation.reservation_date,
    reservation.party_size,
    customer.customer_id,
    concat(customer.first_name, ' ', customer.last_name) AS customer_name,
    customer.email AS customer_email,
    customer.phone_number AS customer_phone_number,
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    restaurant_table.table_id,
    restaurant_table.table_number,
    restaurant_table.capacity AS table_capacity
FROM reservations AS reservation
INNER JOIN customers AS customer
    ON customer.customer_id = reservation.customer_id
INNER JOIN restaurants AS restaurant
    ON restaurant.restaurant_id = reservation.restaurant_id
INNER JOIN restaurant_tables AS restaurant_table
    ON restaurant_table.table_id = reservation.table_id;
