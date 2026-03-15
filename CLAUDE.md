# CLAUDE.md - Hickory Help Desk

## What is this?
Hickory is a modern help desk ticket management system built with a .NET 10 backend and Next.js 15 frontend. Uses PostgreSQL for data storage and Redis for caching/messaging.

## Tech Stack
- **Backend**: .NET 10 / ASP.NET Core, Entity Framework Core, MediatR (CQRS), FluentValidation
- **Frontend**: Next.js 15, React, TypeScript, Tailwind CSS
- **Database**: PostgreSQL (via Npgsql)
- **Caching/Messaging**: Redis (MassTransit)
- **Auth**: JWT Bearer tokens
- **Real-time**: SignalR
- **Observability**: OpenTelemetry, Serilog
- **Monorepo**: Nx workspace

## Project Structure
```
hickory/
├── apps/
│   ├── api/              # .NET 10 ASP.NET Core API
│   │   ├── Controllers/  # API endpoints
│   │   ├── src/          # Core logic
│   │   └── Hickory.Api.Tests/  # Unit tests
│   ├── web/              # Next.js 15 frontend
│   │   └── src/          # React components, pages
│   ├── cli/              # CLI tool (if applicable)
│   ├── cli-e2e/          # CLI e2e tests
│   └── web-e2e/          # Frontend e2e tests
├── docker/               # Docker Compose configs
├── tests/                # Integration/performance tests
└── scripts/              # Dev/build scripts
```

## Development

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- PostgreSQL 16
- Redis 7

### Quick Start
```bash
# Dev Container (recommended)
# Open in VS Code and "Reopen in Container"

# Or Docker Compose (one-command startup - migrations run automatically)
docker compose -f docker/docker-compose.yml up -d
```

### Commands
```bash
# Backend (from apps/api)
dotnet run                    # Run API
dotnet test                   # Run tests
dotnet ef database update     # Apply migrations

# Frontend (from apps/web or root)
npm run dev                   # Start Next.js dev server
npm test                      # Run Jest tests

# Performance tests
npm run test:performance
```

## Code Patterns
- **CQRS**: Use MediatR for commands/queries
- **Validation**: FluentValidation for request validation
- **API Design**: RESTful with NSwag for OpenAPI docs
- **Frontend State**: React Query or similar for server state

## Environment
Uses `.devcontainer` for consistent development. Services (PostgreSQL, Redis, MailHog) auto-configured.

See `docker/README.md` for detailed setup.
