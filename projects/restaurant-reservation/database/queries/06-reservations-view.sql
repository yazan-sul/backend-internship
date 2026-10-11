CREATEOR REPLACE VIEW reservation_details AS
SELECT
    reservation.reservation_id,
    reservation.reservation_date,
    reservation.party_size,
    -- Customer information
    customer.customer_id,
    CONCAT (customer.first_name, ' ', customer.last_name) AS customer_name,
    customer.email AS customer_email,
    customer.phone_number AS customer_phone_number,
    -- Restaurant information
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    -- Reserved table information
    restaurant_table.table_id,
    restaurant_table.table_number,
    restaurant_table.capacity AS table_capacity
FROM
    reservations AS reservation
    -- Retrieve the customer who made the reservation
    JOIN customers AS customer ON customer.customer_id = reservation.customer_id
    -- Retrieve the restaurant where the reservation was made
    JOIN restaurants AS restaurant ON restaurant.restaurant_id = reservation.restaurant_id
    -- Retrieve the table assigned to the reservation
    JOIN restaurant_tables AS restaurant_table ON restaurant_table.table_id = reservation.table_id;