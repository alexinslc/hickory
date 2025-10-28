-- Hickory Help Desk - Database Initialization
-- This script runs automatically when PostgreSQL container is first created
-- Place additional initialization scripts in this directory with numbered prefixes (01-, 02-, etc.)

-- Enable required PostgreSQL extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";  -- For full-text search

-- Create a read-only user for reporting/analytics (optional)
-- CREATE USER hickory_readonly WITH PASSWORD 'readonly_password';
-- GRANT CONNECT ON DATABASE hickory TO hickory_readonly;
-- GRANT USAGE ON SCHEMA public TO hickory_readonly;
-- GRANT SELECT ON ALL TABLES IN SCHEMA public TO hickory_readonly;
-- ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO hickory_readonly;

-- Log initialization
DO $$
BEGIN
  RAISE NOTICE 'Hickory Help Desk database initialized successfully';
  RAISE NOTICE 'Extensions enabled: uuid-ossp, pg_trgm';
END $$;
