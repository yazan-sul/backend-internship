SELECT
    employee.employee_id,
    CONCAT (employee.first_name, ' ', employee.last_name) AS employee_name,
    AVG(orders.total_amount) AS avg_order_amount
FROM
    employees AS employee
    JOIN orders ON orders.employee_id = employee.employee_id
WHERE
    employee.employee_id = 6
GROUP BY
    employee.employee_id;