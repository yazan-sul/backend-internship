SET NOCOUNT ON;

SELECT 'Restaurants' AS table_name, COUNT(*) AS row_count FROM dbo.Restaurants
UNION ALL SELECT 'Customers', COUNT(*) FROM dbo.Customers
UNION ALL SELECT 'RestaurantTables', COUNT(*) FROM dbo.RestaurantTables
UNION ALL SELECT 'Employees', COUNT(*) FROM dbo.Employees
UNION ALL SELECT 'MenuItems', COUNT(*) FROM dbo.MenuItems
UNION ALL SELECT 'Reservations', COUNT(*) FROM dbo.Reservations
UNION ALL SELECT 'Orders', COUNT(*) FROM dbo.Orders
UNION ALL SELECT 'OrderItems', COUNT(*) FROM dbo.OrderItems;

IF (SELECT COUNT(*) FROM dbo.Restaurants) <> 5
    OR (SELECT COUNT(*) FROM dbo.Customers) <> 5
    OR (SELECT COUNT(*) FROM dbo.RestaurantTables) <> 5
    OR (SELECT COUNT(*) FROM dbo.Employees) <> 6
    OR (SELECT COUNT(*) FROM dbo.MenuItems) <> 6
    OR (SELECT COUNT(*) FROM dbo.Reservations) <> 6
    OR (SELECT COUNT(*) FROM dbo.Orders) <> 6
    OR (SELECT COUNT(*) FROM dbo.OrderItems) <> 7
    THROW 50001, 'Seed row counts do not match the baseline fixture.', 1;

IF (SELECT COALESCE(SUM(total_amount), 0) FROM dbo.Orders) <> 171.00
    THROW 50002, 'Seed order total must be 171.00.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.Reservations AS reservation
    JOIN dbo.RestaurantTables AS restaurantTable ON restaurantTable.table_id = reservation.table_id
    WHERE reservation.party_size > restaurantTable.capacity
        OR reservation.restaurant_id <> restaurantTable.restaurant_id)
    THROW 50003, 'Seed reservation does not fit its restaurant table.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.Orders AS customerOrder
    JOIN dbo.Reservations AS reservation ON reservation.reservation_id = customerOrder.reservation_id
    JOIN dbo.Employees AS employee ON employee.employee_id = customerOrder.employee_id
    WHERE employee.restaurant_id <> reservation.restaurant_id)
    THROW 50004, 'Seed order employee belongs to the wrong restaurant.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.OrderItems AS orderItem
    JOIN dbo.Orders AS customerOrder ON customerOrder.order_id = orderItem.order_id
    JOIN dbo.Reservations AS reservation ON reservation.reservation_id = customerOrder.reservation_id
    JOIN dbo.MenuItems AS menuItem ON menuItem.menu_item_id = orderItem.item_id
    WHERE menuItem.restaurant_id <> reservation.restaurant_id)
    THROW 50005, 'Seed order item belongs to the wrong restaurant.', 1;

IF EXISTS (
    SELECT customerOrder.order_id
    FROM dbo.Orders AS customerOrder
    JOIN dbo.OrderItems AS orderItem ON orderItem.order_id = customerOrder.order_id
    JOIN dbo.MenuItems AS menuItem ON menuItem.menu_item_id = orderItem.item_id
    GROUP BY customerOrder.order_id, customerOrder.total_amount
    HAVING SUM(orderItem.quantity * menuItem.price) <> customerOrder.total_amount)
    THROW 50006, 'Seed line amounts do not match recorded order totals.', 1;

SELECT SUM(total_amount) AS total_order_amount FROM dbo.Orders;
PRINT 'Seed verification passed.';
