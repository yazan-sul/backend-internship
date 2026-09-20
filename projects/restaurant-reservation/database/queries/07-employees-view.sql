-- Requirement 7: Create a reusable view of employee details.
CREATE OR REPLACE VIEW employee_details AS
SELECT
    employee.employee_id,
    employee.first_name,
    employee.last_name,
    concat(employee.first_name, ' ', employee.last_name) AS employee_name,
    employee.position,
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    restaurant.address AS restaurant_address,
    restaurant.phone_number AS restaurant_phone_number,
    restaurant.opening_hours AS restaurant_opening_hours
FROM employees AS employee
INNER JOIN restaurants AS restaurant
    ON restaurant.restaurant_id = employee.restaurant_id;
