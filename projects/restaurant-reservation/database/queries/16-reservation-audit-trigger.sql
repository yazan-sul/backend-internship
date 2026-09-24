
  CREATE OR REPLACE FUNCTION fn_LogReservationAudit()
  RETURNS trigger
  LANGUAGE plpgsql
  AS $$
  BEGIN
      INSERT INTO audit_log (
          restaurant_id,
          table_id,
          reservation_date,
          change_date
      )
      VALUES (
          NEW.restaurant_id,
          NEW.table_id,
          NEW.reservation_date,
          CURRENT_TIMESTAMP
      );

      RETURN NEW;
  END;
  $$;


  DROP TRIGGER IF EXISTS trg_reservation_audit
  ON reservations;

  CREATE TRIGGER trg_reservation_audit
  AFTER INSERT ON reservations
  FOR EACH ROW
  EXECUTE FUNCTION fn_LogReservationAudit();
