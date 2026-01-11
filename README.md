# Hickory Help Desk

[![CI Pipeline](https://github.com/alexinslc/hickory/workflows/CI%%20Pipeline/badge.svg)](https://github.com/alexinslc/hickory/actions/workflows/ci.yml)

Full-stack help desk system built with .NET 10, Next.js 15, PostgreSQL, and Redis.

## Quick Start

**Requirements:** .NET 10.0 SDK, Node.js 20+, PostgreSQL 16, Redis 7

```bash
# Start infrastructure
docker compose -f docker/docker-compose.yml up -d

# Backend
cd apps/api && dotnet restore && dotnet ef database update && dotnet run

# Frontend
cd apps/web && npm install && npm run dev

# Open http://localhost:3000
```

## Architecture

```
Next.js :3000  →  .NET API :5000  →  PostgreSQL :5432
                       ↓
                   Redis :6379
```

**Stack:** .NET 10, Next.js 15, PostgreSQL 16, Redis 7, MediatR, SignalR, TanStack Query

## Features

- Ticket submission and tracking
- Agent management dashboard
- Real-time updates (WebSockets)
- Full-text search
- Email notifications
- JWT authentication with refresh tokens
- Database connection pooling with retry/circuit breaker
- Health checks (`/health`, `/health/ready`, `/health/live`)

## Testing

```bash
npx nx run-many --target=test --all    # All tests
dotnet test apps/api/Hickory.Api.Tests # API tests
npx nx test web                        # Web tests
npx nx e2e web-e2e                     # E2E tests
```

## Project Structure

```
apps/
├── api/      # .NET 10 API
├── web/      # Next.js 15 frontend
├── cli/      # TypeScript CLI
└── *-e2e/    # E2E tests
```

## License

See [LICENSE](LICENSE) file.
