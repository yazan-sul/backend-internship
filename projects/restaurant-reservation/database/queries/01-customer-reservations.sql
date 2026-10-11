SELECT
    customer.customer_id,
    customer.first_name,
    customer.last_name,
    reservation.reservation_id,
    reservation.reservation_date,
    reservation.party_size
FROM
    customers AS customer
    JOIN reservations AS reservation ON reservation.customer_id = customer.customer_id
WHERE
    customer.customer_id = 1
ORDER BY
    reservation.reservation_date DESC;