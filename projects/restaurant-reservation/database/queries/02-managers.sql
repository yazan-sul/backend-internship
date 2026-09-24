SELECT
    employee.employee_id,
    CONCAT (employee.first_name, ' ', employee.last_name) AS employee_name,
    emploee.position,
    -- restaurant info
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name
FROM
    employees AS employee
    JOIN restaurants AS restaurant ON restaurant.restaurant_id = employee.restaurant_id
WHERE
    employee.position = 'Manager'
ORDER BY
    restaurant.name,
    employee.first_name,
    employee.last_name;