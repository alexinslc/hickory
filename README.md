# Hickory Help Desk

[![CI Pipeline](https://github.com/alexinslc/hickory/workflows/CI%%20Pipeline/badge.svg)](https://github.com/alexinslc/hickory/actions/workflows/ci.yml)

A simple help desk. That's it.

## Quick Start

```bash
# One command to start everything
docker compose -f docker/docker-compose.yml up

# Open http://localhost:3000
# Login: admin@hickory.dev / Admin123!
```

Done. Database migrations run automatically.

## What Hickory Does

- Customers submit tickets
- Agents respond to tickets
- Everyone gets email notifications
- Real-time updates

That's the whole product.

## Architecture

```
Web :3000  →  API :5000  →  PostgreSQL :5432
                  ↓
              Redis :6379
```

## Development (without Docker)

```bash
# Requirements: .NET 10, Node.js 20+, PostgreSQL, Redis

# Start infrastructure only
docker compose -f docker/docker-compose.yml up postgres redis -d

# Backend (auto-migrates in development)
cd apps/api && dotnet run

# Frontend (separate terminal)
cd apps/web && npm install && npm run dev
```

## Testing

```bash
npx nx run-many --target=test --all    # All tests
npx nx e2e web-e2e                     # E2E tests
```

## Philosophy

Hickory prioritizes simplicity over features. If you need:
- Multi-tenancy → Deploy separate instances
- Complex analytics → Use Metabase/Grafana
- SSO/SAML → Use an enterprise solution
- Workflow automation → Use Zapier

We keep the core simple so it stays maintainable.

## License

See [LICENSE](LICENSE) file.
