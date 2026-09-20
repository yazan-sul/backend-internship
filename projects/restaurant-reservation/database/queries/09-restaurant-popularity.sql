-- Requirement 9: Rank restaurants by reservation popularity.
SELECT
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    count(reservation.reservation_id) AS reservation_count,
    count(DISTINCT reservation.customer_id) AS unique_customer_count,
    coalesce(sum(reservation.party_size), 0) AS total_reserved_guests,
    dense_rank() OVER (
        ORDER BY count(reservation.reservation_id) DESC
    ) AS popularity_rank
FROM restaurants AS restaurant
LEFT JOIN reservations AS reservation
    ON reservation.restaurant_id = restaurant.restaurant_id
GROUP BY restaurant.restaurant_id, restaurant.name
ORDER BY popularity_rank, restaurant.name, restaurant.restaurant_id;
