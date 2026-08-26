CREATE TABLE IF NOT EXISTS flights (
    id uuid PRIMARY KEY,
    code varchar(12) NOT NULL,
    departure_country varchar(80) NOT NULL,
    destination_country varchar(80) NOT NULL,
    departure_airport varchar(12) NOT NULL,
    arrival_airport varchar(12) NOT NULL,
    departure_at timestamptz NOT NULL,
    economy_price numeric(12, 2) NOT NULL,
    business_price numeric(12, 2) NOT NULL,
    first_price numeric(12, 2) NOT NULL,
    economy_capacity integer NOT NULL,
    business_capacity integer NOT NULL,
    first_capacity integer NOT NULL,
    economy_remaining integer NOT NULL,
    business_remaining integer NOT NULL,
    first_remaining integer NOT NULL
);

CREATE TABLE IF NOT EXISTS passengers (
    id uuid PRIMARY KEY,
    name varchar(120) NOT NULL,
    email varchar(320) NOT NULL UNIQUE,
    contact_details text NOT NULL
);

CREATE TABLE IF NOT EXISTS bookings (
    id uuid PRIMARY KEY,
    passenger_id uuid NOT NULL REFERENCES passengers(id),
    flight_id uuid NOT NULL REFERENCES flights(id),
    travel_class varchar(20) NOT NULL,
    final_price numeric(12, 2) NOT NULL,
    booked_at timestamptz NOT NULL,
    status varchar(20) NOT NULL
);
