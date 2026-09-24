CREATE OR REPLACE FUNCTION sp_ResrvedTablesReport(StartDate date, EndDate date)
  RETURNS TABLE (
    table_id integer,
    table_number integer,
    reserved_date date
  )
  LANGUAGE sql
  AS $$
    SELECT
      restaurant_table.table_id,
      restaurant_table.table_number,
      reservation.reservation_date
    FROM
      reservations AS reservation
      JOIN restaurant_tables AS restaurant_table ON restaurant_table.table_id = reservation.table_id
    WHERE
      reservation.reservation_date BETWEEN StartDate AND EndDate;
  $$;

SELECT
    employee.employee_id,
    employee.position,
    COUNT(restaurant_order.order_id) AS order_count,
    CASE employee.position
        WHEN 'VIPOrdersWaiter' THEN 5
        WHEN 'StandardWaiter' THEN 4
        WHEN 'AssistantWaiter' THEN 3
        ELSE 0
    END AS position_rank
FROM employees AS employee
LEFT JOIN orders AS restaurant_order
    ON restaurant_order.employee_id = employee.employee_id
WHERE employee.employee_id = 2
GROUP BY
    employee.employee_id,
    employee.position;
