# Hickory Help Desk

[![CI Pipeline](https://github.com/alexinslc/hickory/workflows/CI%20Pipeline/badge.svg)](https://github.com/alexinslc/hickory/actions/workflows/ci.yml)

A simple help desk. That's it.

## Quick Start

```bash
# One command to start everything
docker compose -f docker/docker-compose.yml up

# Open http://localhost:3000
# Login: admin@hickory.dev / Admin123!
```

Database migrations run automatically.

## What Hickory Does

- Customers submit and track tickets
- Agents manage, assign, and resolve tickets
- Knowledge base for self-service
- File attachments, categories, and tags
- Real-time updates and email notifications
- Two-factor authentication

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10 / ASP.NET Core |
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS 4 |
| Database | PostgreSQL (EF Core, Npgsql) |
| Cache/Messaging | Redis (MassTransit) |
| Auth | JWT + TOTP two-factor |
| Real-time | SignalR |
| Patterns | CQRS (MediatR), FluentValidation |
| Monorepo | Nx |

## Architecture

```
Web :3000  ->  API :5000  ->  PostgreSQL :5432
                   |
               Redis :6379
```

## Project Structure

```
hickory/
├── apps/
│   ├── api/                # .NET 10 API (CQRS, feature folders)
│   ├── web/                # Next.js 16 frontend
│   ├── cli/                # CLI tool
│   ├── cli-e2e/            # CLI end-to-end tests
│   └── web-e2e/            # Playwright end-to-end tests
├── docker/                 # Docker Compose, Dockerfiles
├── tests/                  # Performance tests
└── scripts/                # Dev/build scripts
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
npx nx run-many --target=test --all    # All frontend tests (Jest)
npx nx e2e web-e2e                     # E2E tests (Playwright)
dotnet test                            # .NET unit + integration tests (Docker required for integration)
npm run test:performance               # Performance tests
```

## Philosophy

Hickory prioritizes simplicity over features. If you need multi-tenancy, complex analytics, SSO/SAML, or workflow automation -- use purpose-built tools for those. We keep the core simple so it stays maintainable.

## License

See [LICENSE](LICENSE) file.
