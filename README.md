# Hickory Help Desk

[![CI Pipeline](https://github.com/alexinslc/hickory/workflows/CI%20Pipeline/badge.svg)](https://github.com/alexinslc/hickory/actions/workflows/ci.yml)

Modern, full-stack help desk ticket management system built with .NET 9, Next.js 15, PostgreSQL, and Redis.

## 🚀 Quick Start

### Option 1: Dev Container (Recommended for VS Code)

The easiest way to get started with a complete development environment:

**Prerequisites:**
- [Docker](https://www.docker.com/products/docker-desktop)
- [VS Code](https://code.visualstudio.com/)
- [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

**Steps:**
1. Open this repository in VS Code
2. Click "Reopen in Container" when prompted (or use Command Palette: "Dev Containers: Reopen in Container")
3. Wait for the container to build and dependencies to install (5-10 minutes first time)
4. Start development! All services (PostgreSQL, Redis, MailHog) are automatically configured.

See [.devcontainer/README.md](.devcontainer/README.md) for more details.

### Option 2: Docker Compose

Run the full application stack with Docker:

```bash
# Start all services
docker compose -f docker/docker-compose.yml up -d

# Initialize database
docker compose -f docker/docker-compose.yml exec api \
  dotnet ef database update

# Access the application
open http://localhost:3000
```

See [docker/QUICKSTART.md](docker/QUICKSTART.md) for more details.

### Option 3: Local Development

**Prerequisites:**
- .NET 9.0 SDK
- Node.js 20+
- PostgreSQL 16
- Redis 7

**Backend:**
```bash
cd apps/api
dotnet restore
dotnet ef database update
dotnet run
```

**Frontend:**
```bash
cd apps/web
npm install
npm run dev
```

## 📖 Documentation

- **[Dev Container Setup](.devcontainer/README.md)** - VS Code devcontainer documentation
- **[Docker Setup](docker/README.md)** - Complete Docker documentation
- **[Specification](specs/001-help-desk-core/spec.md)** - Product requirements
- **[API Documentation](specs/001-help-desk-core/contracts/openapi.yaml)** - OpenAPI spec
- **[Data Model](specs/001-help-desk-core/data-model.md)** - Database schema

## 🏗️ Architecture

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Next.js   │────▶│  .NET API   │────▶│ PostgreSQL  │
│   Frontend  │     │   Backend   │     │  Database   │
│   (Port     │     │   (Port     │     │  (Port      │
│    3000)    │     │    5000)    │     │    5432)    │
└─────────────┘     └─────────────┘     └─────────────┘
                           │
                           │
                    ┌──────▼──────┐
                    │    Redis    │
                    │    Cache    │
                    │  (Port 6379)│
                    └─────────────┘
```

## 🎯 Features

- **User Stories Implemented:**
  - ✅ US-001: Submit and track support tickets
  - ✅ US-002: Agent ticket management and responses
  
- **Technical Features:**
  - Real-time updates with WebSockets
  - Optimistic concurrency control
  - Performance: <2s ticket submission, <30s agent response
  - Full-text search capabilities
  - Email notifications via SMTP
  - Caching with Redis

## 🧪 Testing

**Run tests locally:**
```bash
# All tests
npx nx run-many --target=test --all

# Specific projects
dotnet test apps/api/Hickory.Api.Tests/Hickory.Api.Tests.csproj
npx nx test web
npx nx test cli

# E2E tests (Playwright)
npx nx e2e web-e2e

# View E2E report
npx playwright show-report

# Performance tests
npm run test:performance
```

## 🛠️ Development

This project uses [Nx](https://nx.dev) for monorepo management.

```bash
# Build all projects
npx nx run-many --target=build --all

# Run tests
npx nx run-many --target=test --all

# Lint
npx nx run-many --target=lint --all
```

## 📦 Project Structure

```
hickory/
├── apps/
│   ├── api/              # .NET 9 ASP.NET Core API
│   ├── web/              # Next.js 15 frontend
│   ├── cli/              # CLI tool (TypeScript)
│   └── *-e2e/            # End-to-end tests
├── docker/               # Docker configuration
│   ├── docker-compose.yml
│   ├── api.Dockerfile
│   ├── web.Dockerfile
│   └── README.md
├── specs/                # Product specifications
└── tests/                # Shared test utilities
```

## 🔐 Security

- JWT authentication with configurable expiration
- Password hashing with bcrypt
- SQL injection protection via parameterized queries
- XSS protection in frontend
- CORS configuration
- Rate limiting (Redis-backed)

## 🚢 Deployment

See [docker/README.md](docker/README.md) for production deployment instructions.

Key considerations:
- Update JWT secret
- Configure production SMTP
- Set up SSL/TLS
- Configure reverse proxy
- Set up monitoring
- Implement backup strategy

## 📝 License

See [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests and linting
5. Submit a pull request

## 📧 Support

For issues and questions:
- Create an issue in the repository
- Check existing documentation
- Review troubleshooting guides