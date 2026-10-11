CREATE
OR REPLACE VIEW employee_details AS
SELECT
    employee.employee_id,
    employee.first_name,
    employee.last_name,
    employee.position,
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name
FROM
    employees AS employee
    JOIN restaurants AS restaurant ON restaurant.restaurant_id = employee.restaurant_id;