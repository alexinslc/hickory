# Database Initialization Scripts

This directory contains SQL scripts that run automatically when the PostgreSQL container is first created.

## How It Works

- Scripts are executed in alphabetical order
- Use numbered prefixes (01-, 02-, etc.) to control execution order
- Scripts only run on **first container creation** (not on restart)
- To re-run, delete the postgres volume: `docker compose down -v`

## Existing Scripts

- `01-extensions.sql` - Enables PostgreSQL extensions (uuid-ossp, pg_trgm)

## Adding Custom Scripts

Create a new SQL file with a numbered prefix:

```bash
# Example: Add custom indexes
cat > docker/init-db/02-indexes.sql << 'EOF'
-- Custom indexes for performance
CREATE INDEX IF NOT EXISTS idx_tickets_status ON tickets(status);
CREATE INDEX IF NOT EXISTS idx_tickets_created_at ON tickets(created_at);
EOF
```

## Testing Initialization

To test your initialization scripts:

```bash
# Remove existing database
docker compose -f docker/docker-compose.yml down -v

# Recreate with initialization
docker compose -f docker/docker-compose.yml up -d postgres

# Check logs for initialization messages
docker compose -f docker/docker-compose.yml logs postgres
```

## Common Use Cases

### Add Extensions
```sql
CREATE EXTENSION IF NOT EXISTS "extension_name";
```

### Create Additional Users
```sql
CREATE USER app_user WITH PASSWORD 'password';
GRANT CONNECT ON DATABASE hickory TO app_user;
```

### Seed Initial Data
```sql
INSERT INTO categories (name, description) VALUES
  ('Technical', 'Technical support requests'),
  ('Billing', 'Billing and payment issues');
```

### Configure Database Settings
```sql
ALTER DATABASE hickory SET timezone TO 'UTC';
```

## Notes

- Use `IF NOT EXISTS` clauses to make scripts idempotent
- Avoid hardcoding sensitive data (use environment variables instead)
- Test scripts locally before deploying
- Scripts run as the PostgreSQL superuser
