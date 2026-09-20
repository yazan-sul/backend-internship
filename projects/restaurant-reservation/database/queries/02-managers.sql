-- Requirement 2: Retrieve every manager and the restaurant they manage.
SELECT
    employee.employee_id,
    concat(employee.first_name, ' ', employee.last_name) AS manager_name,
    employee.position,
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name
FROM employees AS employee
INNER JOIN restaurants AS restaurant
    ON restaurant.restaurant_id = employee.restaurant_id
WHERE employee.position = 'Manager'
ORDER BY restaurant.name, employee.last_name, employee.first_name;
