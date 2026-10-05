CREATE OR REPLACE PROCEDURE FutureReservedTables()
  LANGUAGE plpgsql
  AS $$
  BEGIN
      DROP TABLE IF EXISTS temp_future_tables;

      CREATE TEMP TABLE temp_future_tables AS
      SELECT DISTINCT table_id, restaurant_id
      FROM reservations
      WHERE reservation_date > CURRENT_TIMESTAMP;
  END;
  $$;

CALL FutureReservedTables();

SELECT
    temp_future_tables.table_id,
    restaurants.name,
    restaurants.address
FROM
    temp_future_tables
    JOIN restaurants ON temp_future_tables.restaurant_id = restaurants.restaurant_id;

-- docker compose exec -T db psql -U postgres -d restaurant_reservation < database/new-queries/15.sql