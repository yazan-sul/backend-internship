 CREATE OR REPLACE FUNCTION fn_CalculateEmployeeSalary(EmployeeId integer)
  RETURNS bigint
  LANGUAGE sql
  AS $$
      SELECT
          COUNT(restaurant_order.order_id) *
          CASE employee.position
              WHEN 'VIPOrdersWaiter' THEN 5
              WHEN 'StandardWaiter' THEN 4
              WHEN 'AssistantWaiter' THEN 3
              ELSE 0
          END
      FROM employees AS employee
      LEFT JOIN orders AS restaurant_order
          ON restaurant_order.employee_id = employee.employee_id
      WHERE employee.employee_id = EmployeeId
      GROUP BY
          employee.employee_id,
          employee.position;
  $$;
