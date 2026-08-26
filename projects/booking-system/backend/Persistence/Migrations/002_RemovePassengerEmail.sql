ALTER TABLE passengers
    DROP COLUMN IF EXISTS email;

ALTER TABLE passengers
    ALTER COLUMN contact_details DROP NOT NULL;
