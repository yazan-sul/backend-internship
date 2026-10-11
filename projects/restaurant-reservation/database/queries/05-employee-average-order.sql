SELECT
    employee.employee_id,
    CONCAT (employee.first_name, ' ', employee.last_name) AS employee_name,
    employee.position,
    -- order info
    COUNT(restaurant_order.order_id) AS order_count,
    AVG(restaurant_order.total_amount) AS average_order_amount
FROM
    employees AS employee
    JOIN orders AS restaurant_order ON restaurant_order.employee_id = employee.employee_id
WHERE
    employee.employee_id = 2
ORDER BY
    employee.employee_id,
    employee.first_name,
    employee.last_name;