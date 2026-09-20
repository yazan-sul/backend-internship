-- Requirement 5: Calculate the average order value handled by one employee.
-- Run with psql -v employee_id=<id> so the ID is not hardcoded here.
SELECT
    employee.employee_id,
    concat(employee.first_name, ' ', employee.last_name) AS employee_name,
    employee.position,
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    count(restaurant_order.order_id) AS order_count,
    coalesce(avg(restaurant_order.total_amount), 0)::numeric(12, 2) AS average_order_value
FROM employees AS employee
INNER JOIN restaurants AS restaurant
    ON restaurant.restaurant_id = employee.restaurant_id
LEFT JOIN orders AS restaurant_order
    ON restaurant_order.employee_id = employee.employee_id
WHERE employee.employee_id = :employee_id
GROUP BY
    employee.employee_id,
    employee.first_name,
    employee.last_name,
    employee.position,
    restaurant.restaurant_id,
    restaurant.name;
