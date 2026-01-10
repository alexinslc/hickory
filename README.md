# Hickory Help Desk

[![CI Pipeline](https://github.com/alexinslc/hickory/workflows/CI%20Pipeline/badge.svg)](https://github.com/alexinslc/hickory/actions/workflows/ci.yml)

Full-stack help desk system built with .NET 10, Next.js 15, PostgreSQL, and Redis.

## 🚀 Quick Start

### Dev Container (Recommended)
1. Open in VS Code with [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
2. Click "Reopen in Container"
3. Wait for setup (~5-10 minutes)

See [.devcontainer/README.md](.devcontainer/README.md) for details.

### Docker Compose
```bash
docker compose -f docker/docker-compose.yml up -d
docker compose -f docker/docker-compose.yml exec api dotnet ef database update
open http://localhost:3000
```

See [docker/QUICKSTART.md](docker/QUICKSTART.md) for details.

### Local Development
**Requirements:** .NET 10.0 SDK, Node.js 20+, PostgreSQL 16, Redis 7

```bash
# Backend
cd apps/api && dotnet restore && dotnet ef database update && dotnet run

# Frontend
cd apps/web && npm install && npm run dev
```

## 📖 Documentation

- [Dev Container](.devcontainer/README.md) - VS Code devcontainer
- [Docker Setup](docker/README.md) - Docker documentation
- [Specification](specs/001-help-desk-core/spec.md) - Product requirements
- [API Docs](specs/001-help-desk-core/contracts/openapi.yaml) - OpenAPI spec
- [Data Model](specs/001-help-desk-core/data-model.md) - Database schema

## 🏗️ Architecture

```
┌─────────┐     ┌─────────┐     ┌────────────┐
│ Next.js │────▶│ .NET 10  │────▶│ PostgreSQL │
│  :3000  │     │  :5000  │     │   :5432    │
└─────────┘     └────┬────┘     └────────────┘
                     │
                ┌────▼────┐
                │  Redis  │
                │  :6379  │
                └─────────┘
```

## 🎯 Features

- Submit and track support tickets (US-001)
- Agent ticket management (US-002)
- Real-time updates (WebSockets)
- Optimistic concurrency control
- Performance: <2s submission, <30s response
- Full-text search, email notifications, Redis caching
- Health checks for monitoring (PostgreSQL, Redis)

## 🏥 Health Checks

The API exposes health check endpoints for monitoring:

- **`/health`** - Overall application health (all dependencies)
- **`/health/ready`** - Readiness check (database + Redis)
- **`/health/live`** - Liveness check (application is running)

All health checks complete within 5 seconds and return:
- `200 OK` with "Healthy" when all checks pass
- `503 Service Unavailable` when any check fails

Dependencies monitored:
- PostgreSQL database connection
- Redis cache connection

## 🧪 Testing

```bash
# All tests
npx nx run-many --target=test --all

# Specific projects
dotnet test apps/api/Hickory.Api.Tests/Hickory.Api.Tests.csproj
npx nx test web

# E2E tests
npx nx e2e web-e2e
npx playwright show-report

# Performance tests
npm run test:performance
```

## 🛠️ Development

Uses [Nx](https://nx.dev) for monorepo management.

```bash
npx nx run-many --target=build --all  # Build all
npx nx run-many --target=test --all   # Test all
npx nx run-many --target=lint --all   # Lint all
```

## 📦 Project Structure

```
hickory/
├── apps/
│   ├── api/          # .NET 10 API
│   ├── web/          # Next.js 15 frontend
│   ├── cli/          # TypeScript CLI
│   └── *-e2e/        # E2E tests
├── docker/           # Docker config
├── specs/            # Specifications
└── tests/            # Test utilities
```

## 🔐 Security

- JWT authentication
- Password hashing (bcrypt)
- Parameterized queries (SQL injection protection)
- XSS protection, CORS, rate limiting

## 🚢 Deployment

See [docker/README.md](docker/README.md) for production deployment.

**Key considerations:** Update JWT secret, configure SMTP, set up SSL/TLS, reverse proxy, monitoring, and backups.

## 📝 License

See [LICENSE](LICENSE) file.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes and run tests
4. Submit a pull request

## 📧 Support

- Create an issue
- Check documentation
- Review troubleshooting guides