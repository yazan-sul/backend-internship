SELECT
    customer.customer_id,
    customer.first_name,
    customer.last_name,
    -- reservation info
    reservation.reservation_id,
    reservation.reservation_date
FROM
    customers AS customer
    JOIN reservations AS reservation ON reservation.customer_id = customer.customer_id
where
    customer.customer_id = 6;