ALTER TABLE flights
    ADD CONSTRAINT flights_prices_positive CHECK (economy_price > 0 AND business_price > 0 AND first_price > 0),
    ADD CONSTRAINT flights_capacities_non_negative CHECK (economy_capacity >= 0 AND business_capacity >= 0 AND first_capacity >= 0),
    ADD CONSTRAINT flights_remaining_valid CHECK (
        economy_remaining BETWEEN 0 AND economy_capacity AND
        business_remaining BETWEEN 0 AND business_capacity AND
        first_remaining BETWEEN 0 AND first_capacity
    ),
    ADD CONSTRAINT flights_route_distinct CHECK (
        lower(departure_country) <> lower(destination_country) AND
        lower(departure_airport) <> lower(arrival_airport)
    );

CREATE UNIQUE INDEX IF NOT EXISTS flights_code_lower_unique ON flights (lower(code));

ALTER TABLE bookings
    ADD CONSTRAINT bookings_class_valid CHECK (travel_class IN ('Economy', 'Business', 'First')),
    ADD CONSTRAINT bookings_status_valid CHECK (status IN ('Active', 'Cancelled'));
