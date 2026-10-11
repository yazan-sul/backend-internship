CREATE
OR REPLACE VIEW reservation_report AS
SELECT
    reservation.reservation_id,
    reservation.reservation_date,
    reservation.party_size,
    -- customer info
    customer.customer_id,
    customer.first_name AS customer_first_name,
    customer.last_name AS customer_last_name,
    customer.email AS customer_email,
    customer.phone_number AS customer_phone_number,
    -- restaurant info
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    restaurant.address AS restaurant_address,
    restaurant.phone_number AS restaurant_phone_number
FROM
    reservations AS reservation
    JOIN customers AS customer ON customer.customer_id = reservation.customer_id
    JOIN restaurants AS restaurant ON restaurant.restaurant_id = reservation.restaurant_id
ORDER BY
    reservation.reservation_date;

SELECT
    *
FROM
    reservation_report
ORDER BY
    reservation_date;

-- command   docker compose exec db psql -U postgres -d restaurant_reservation -c "SELECT * FROM reservation_report ORDER BY reservation_date;"