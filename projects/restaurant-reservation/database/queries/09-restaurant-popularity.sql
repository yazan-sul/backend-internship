WITH
    restaurant_totals AS (
        SELECT
            restaurant.restaurant_id,
            restaurant.name AS restaurant_name,
            COUNT(reservation.reservation_id) AS reservation_count
        FROM
            restaurants AS restaurant
            LEFT JOIN reservations AS reservation ON reservation.restaurant_id = restaurant.restaurant_id
        GROUP BY
            restaurant.restaurant_id,
            restaurant.name
        ORDER BY
            reservation_count DESC
    )
SELECT
    restaurant_id,
    restaurant_name,
    reservation_count,
    DENSE_RANK() OVER (
        ORDER BY
            reservation_count DESC
    ) AS popularity_rank
FROM
    restaurant_totals
ORDER BY
    popularity_rank,
    restaurant_name;