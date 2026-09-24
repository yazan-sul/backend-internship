-- Supports orders filtered by reservation.
CREATE INDEX idx_orders_reservation_id ON orders (reservation_id);

-- Supports orders filtered by date range.
CREATE INDEX idx_orders_order_date ON orders (order_date);

-- Refresh planner statistics.
ANALYZE orders;