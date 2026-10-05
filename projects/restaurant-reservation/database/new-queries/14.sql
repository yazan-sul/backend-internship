CREATE OR REPLACE PROCEDURE sp_AddNewOrder(
    IN ReservationId integer,
    IN EmployeeId integer,
    IN OrderDate timestamptz,
    IN TotalAmount numeric(12, 2),
    OUT NewOrderId integer
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM reservations AS reservation
        WHERE reservation.reservation_id = ReservationId
    ) THEN
        RAISE EXCEPTION 'Reservation % does not exist', ReservationId;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM employees AS employee
        WHERE employee.employee_id = EmployeeId
    ) THEN
        RAISE EXCEPTION 'Employee % does not exist', EmployeeId;
    END IF;

    INSERT INTO orders (
        reservation_id,
        employee_id,
        order_date,
        total_amount
    )
    VALUES (
        ReservationId,
        EmployeeId,
        OrderDate,
        TotalAmount
    )
    RETURNING order_id INTO NewOrderId;
END;
$$;
