CREATE OR REPLACE PROCEDURE sp_ResrvedTablesReport(
    StartDate date,
    EndDate date
)
LANGUAGE plpgsql
AS $$
BEGIN
    DROP TABLE IF EXISTS temp_reserved_tables_report;

    CREATE TEMP TABLE temp_reserved_tables_report AS
    SELECT
        reservation_date,
        party_size,
        restaurants.name AS restaurant_name,
        restaurant_tables.table_number,
        restaurant_tables.capacity AS table_capacity,
        restaurants.address AS restaurant_address
    FROM reservations
    JOIN restaurants
        ON reservations.restaurant_id = restaurants.restaurant_id
    JOIN restaurant_tables
        ON reservations.table_id = restaurant_tables.table_id
    WHERE reservation_date >= StartDate
      AND reservation_date < EndDate + 1;
END;
$$;

  CALL sp_ResrvedTablesReport(
      DATE '2020-01-01',
      DATE '2030-12-31'
  );

  SELECT * FROM temp_reserved_tables_report;
-- docker compose exec db psql -U postgres -d restaurant_reservation
-- CALL sp_ResrvedTablesReport('2020-01-01', '2030-12-31');
-- SELECT * FROM temp_reserved_tables_report;
