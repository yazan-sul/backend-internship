SELECT
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    COUNT(reservation.reservation_id) AS reservation_count,
    DENSE_RANK() OVER (
        ORDER BY
            COUNT(reservation.reservation_id) DESC
    ) AS popularity_rank
FROM
    reservations AS reservation
    JOIN restaurants AS restaurant ON restaurant.restaurant_id = reservation.restaurant_id
GROUP BY
    restaurant.restaurant_id,
    restaurant.name;