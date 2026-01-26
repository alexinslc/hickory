# GitHub Copilot Instructions for Hickory

## Project Overview
Hickory is a modern help desk ticket management system built with .NET 9 (ASP.NET Core) backend and Next.js 15 frontend. Uses PostgreSQL for persistence and Redis for caching/messaging.

## Tech Stack
- **Backend**: .NET 9, ASP.NET Core, Entity Framework Core, MediatR (CQRS), FluentValidation
- **Frontend**: Next.js 15, React, TypeScript, Tailwind CSS
- **Database**: PostgreSQL 16 (via Npgsql)
- **Caching/Messaging**: Redis (MassTransit)
- **Auth**: JWT Bearer tokens
- **Real-time**: SignalR
- **Observability**: OpenTelemetry, Serilog
- **Monorepo**: Nx workspace

## Project Structure
```
apps/
├── api/           # .NET 9 ASP.NET Core API
│   ├── Controllers/
│   ├── src/
│   └── *.Tests/
├── web/           # Next.js 15 frontend
│   └── src/
├── cli/           # CLI tool
└── *-e2e/         # E2E tests
docker/            # Docker Compose configs
tests/             # Performance tests
```

## Commands
```bash
# Backend
dotnet run                    # Run API
dotnet test                   # Run tests
dotnet ef database update     # Apply migrations

# Frontend
npm run dev                   # Next.js dev server
npm test                      # Jest tests
```

## Code Conventions
- Use MediatR for CQRS (commands/queries)
- FluentValidation for all request DTOs
- RESTful API design with NSwag/Swagger
- Serilog for structured logging
- TypeScript strict mode in frontend
- Tailwind CSS for styling

## Setup
Use Dev Container (VS Code) or Docker Compose for consistent environment. See `docker/QUICKSTART.md`.
