CREATE OR REPLACE FUNCTION log_reservation_audit()
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

DROP TRIGGER IF EXISTS trg_reservation_audit ON reservations;

CREATE TRIGGER trg_reservation_audit
AFTER INSERT ON reservations
FOR EACH ROW
EXECUTE FUNCTION log_reservation_audit();
SELECT * FROM audit_log ORDER BY audit_log_id DESC LIMIT 5;
--   docker compose exec db psql -U postgres -d restaurant_reservation
--   SELECT * FROM audit_log ORDER BY audit_log_id DESC LIMIT 5;