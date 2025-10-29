# Hickory Help Desk

Modern, full-stack help desk ticket management system built with .NET 9, Next.js 15, PostgreSQL, and Redis.

## 🚀 Quick Start

### Option 1: Docker (Recommended)

The fastest way to get started:

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

### Option 2: Local Development

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

### E2E Tests (Playwright) - **130+ comprehensive tests**

Full end-to-end test coverage for all major features:

```bash
# Run all E2E tests
npm run test:e2e

# Run with UI mode (recommended for development)
npm run test:e2e:ui

# Run specific test suites
npm run test:e2e:dashboard
npm run test:e2e:auth
npm run test:e2e:search

# View test report
npm run test:e2e:report

# See detailed guide
cat apps/web-e2e/E2E_TEST_GUIDE.md
```

**Test Coverage:**
- ✅ User ticket workflows (7 tests)
- ✅ Agent ticket management (10 tests)
- ✅ Dashboard functionality (15 tests)
- ✅ Authentication flows (25 tests)
- ✅ Search functionality (15 tests)
- ✅ Knowledge base (20 tests)
- ✅ Admin features (20 tests)
- ✅ Settings & notifications (20 tests)

### Performance Tests

```bash
# Performance tests
npm run test:performance
```

**Documentation:**
- [E2E Test Guide](apps/web-e2e/E2E_TEST_GUIDE.md) - Comprehensive testing guide
- [E2E README](apps/web-e2e/README.md) - Test coverage details
- [Quick Start](apps/web-e2e/QUICKSTART.md) - Getting started

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