CREATE OR REPLACE PROCEDURE sp_FutureReservedTablesReport()
  LANGUAGE plpgsql
  AS $$
  DECLARE
      ReportCursor refcursor := 'future_reserved_tables_report';
  BEGIN
      DROP TABLE IF EXISTS temp_future_reserved_tables;

      -- Store tables with future reservations
      CREATE TEMP TABLE temp_future_reserved_tables
      ON COMMIT PRESERVE ROWS
      AS
          SELECT
              reservation.reservation_id,
              reservation.reservation_date,
              reservation.party_size,
              restaurant_table.table_id,
              restaurant_table.restaurant_id,
              restaurant_table.table_number,
              restaurant_table.capacity
          FROM reservations AS reservation
          JOIN restaurant_tables AS restaurant_table
              ON restaurant_table.table_id = reservation.table_id
          WHERE reservation.reservation_date > CURRENT_TIMESTAMP;

      -- Join the temporary table with restaurants
      OPEN ReportCursor FOR
          SELECT
              -- Future reservation information
              future_table.reservation_id,
              future_table.reservation_date,
              future_table.party_size,

              -- Reserved table information
              future_table.table_id,
              future_table.table_number,
              future_table.capacity,

              -- Associated restaurant information
              restaurant.restaurant_id,
              restaurant.name AS restaurant_name,
              restaurant.address,
              restaurant.phone_number,
              restaurant.opening_hours
          FROM temp_future_reserved_tables AS future_table
          JOIN restaurants AS restaurant
              ON restaurant.restaurant_id = future_table.restaurant_id
          ORDER BY future_table.reservation_date;
  END;
  $$;