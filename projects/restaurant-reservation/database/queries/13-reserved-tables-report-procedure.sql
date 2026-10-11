CREATE OR REPLACE PROCEDURE sp_ResrvedTablesReport(
      StartDate date,
      EndDate date
  )
  LANGUAGE plpgsql
  AS $$
  -- declares a variable that points to the report results
  DECLARE
      ReportCursor refcursor := 'reserved_tables_report';
  BEGIN
      OPEN ReportCursor FOR
          SELECT
              -- Reservation information
              reservation.reservation_id,
              reservation.reservation_date,
              reservation.party_size,

              -- Reserved table information
              restaurant_table.table_id,
              restaurant_table.table_number,
              restaurant_table.capacity,

              -- Restaurant information
              restaurant.restaurant_id,
              restaurant.name AS restaurant_name,
              restaurant.address AS restaurant_address
          FROM reservations AS reservation
          JOIN restaurant_tables AS restaurant_table
              ON restaurant_table.table_id = reservation.table_id
          JOIN restaurants AS restaurant
              ON restaurant.restaurant_id = reservation.restaurant_id
          WHERE reservation.reservation_date >= StartDate
            AND reservation.reservation_date < EndDate + 1
          ORDER BY reservation.reservation_date;
  END;
  $$;