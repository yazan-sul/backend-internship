SELECT
    employee.employee_id,
    employee.first_name,
    employee.last_name
FROM
    employees AS employee
WHERE
    employee.position = 'Manager';