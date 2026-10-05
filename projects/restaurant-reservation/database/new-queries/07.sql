CREATE
OR REPLACE VIEW employee_report AS
SELECT
    employee.employee_id,
    CONCAT (employee.first_name, ' ', employee.last_name) AS employee_name,
    employee.position,
    -- restaurant info
    restaurant.restaurant_id,
    restaurant.name AS restaurant_name,
    restaurant.address AS restaurant_address,
    restaurant.phone_number AS restaurant_phone_number
FROM
    employees AS employee
    JOIN restaurants AS restaurant ON restaurant.restaurant_id = employee.restaurant_id;

SELECT
    *
FROM
    employee_report;

-- command   docker compose exec db psql -U postgres -d restaurant_reservation -c "SELECT * FROM employee_report;"