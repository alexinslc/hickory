# AGENTS.md - Hickory Help Desk

## Project Summary
Hickory is a modern, full-stack help desk ticket management system. Built with .NET 9 backend and Next.js 15 frontend, using PostgreSQL and Redis.

## Tech Stack
| Layer | Technology |
|-------|------------|
| Backend | .NET 9, ASP.NET Core, Entity Framework Core |
| CQRS | MediatR |
| Validation | FluentValidation |
| Frontend | Next.js 15, React, TypeScript, Tailwind CSS |
| Database | PostgreSQL 16 (Npgsql) |
| Cache/Messaging | Redis (MassTransit) |
| Auth | JWT Bearer tokens |
| Real-time | SignalR |
| Observability | OpenTelemetry, Serilog |
| Monorepo | Nx |

## Directory Structure
```
hickory/
├── apps/
│   ├── api/              # .NET 9 ASP.NET Core API
│   │   ├── Controllers/  # REST endpoints
│   │   ├── src/          # Business logic
│   │   └── *.Tests/      # Unit/integration tests
│   ├── web/              # Next.js 15 frontend
│   │   └── src/          # React components
│   ├── cli/              # CLI tooling
│   └── *-e2e/            # E2E test projects
├── docker/               # Docker Compose files
├── tests/                # Integration/performance tests
├── scripts/              # Build/dev scripts
└── hickory.sln           # .NET solution file
```

## Development Commands
| Command | Purpose |
|---------|---------|
| `dotnet run` | Run API (from apps/api) |
| `dotnet test` | Run .NET tests |
| `dotnet ef database update` | Apply EF migrations |
| `npm run dev` | Start Next.js dev server |
| `npm test` | Run Jest tests |
| `npm run test:performance` | Run perf tests |

## Code Conventions
1. **CQRS Pattern**: Use MediatR for commands and queries
2. **Validation**: FluentValidation for all request DTOs
3. **API**: RESTful design with NSwag/Swagger documentation
4. **Dependency Injection**: Use .NET built-in DI container
5. **Logging**: Serilog with structured logging
6. **Frontend**: Tailwind for styling, TypeScript strict mode

## Setup Options
1. **Dev Container** (recommended): Open in VS Code → "Reopen in Container"
2. **Docker Compose**: `docker compose -f docker/docker-compose.yml up -d`
3. **Local**: Install .NET 9, Node.js 20+, PostgreSQL 16, Redis 7

## Environment
- PostgreSQL: Port 5432
- Redis: Port 6379
- API: Port 5000 (default)
- Web: Port 3000 (default)
- MailHog: Port 8025 (dev mail)

See `.devcontainer/README.md` and `docker/QUICKSTART.md` for details.
