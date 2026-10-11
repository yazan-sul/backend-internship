DROP FUNCTION fn_CalculateEmployeeSalary(integer);
CREATE OR REPLACE FUNCTION fn_CalculateEmployeeSalary(EmployeeId integer)
RETURNS numeric
LANGUAGE sql
AS $$
WITH EmployeeOrders AS (
    SELECT
        employee.position,
        COUNT(orders.order_id) AS total_orders
    FROM employees AS employee
    LEFT JOIN orders
        ON orders.employee_id = employee.employee_id
    WHERE employee.employee_id = EmployeeId
    GROUP BY employee.employee_id,
          employee.position
)
SELECT
    CASE
        WHEN position = 'VIPOrdersWaiter' THEN total_orders * 5
        WHEN position = 'StandardWaiter' THEN total_orders * 4
        WHEN position = 'AssistantWaiter' THEN total_orders * 3
        ELSE 0
    END
FROM EmployeeOrders;
$$;
SELECT fn_CalculateEmployeeSalary(2);
-- docker compose exec db psql -U postgres -d restaurant_reservation \    -c "SELECT fn_CalculateEmployeeSalary(2);"