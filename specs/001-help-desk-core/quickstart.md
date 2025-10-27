# Quickstart Guide: Hickory Help Desk System

**Feature**: 001-help-desk-core  
**Last Updated**: October 26, 2025  
**Target Audience**: Developers setting up local development environment

## Prerequisites

Before you begin, ensure you have the following installed:

- **Node.js**: 20.x or later (LTS recommended)
- **npm**: 10.x or later (comes with Node.js)
- **Docker**: 24.x or later with Docker Compose
- **Git**: For cloning the repository
- **.NET SDK**: 9.0 or later (for backend development)
- **IDE** (recommended):
  - Visual Studio Code with C#, ESLint, Prettier extensions
  - Visual Studio 2022 (17.8+) or JetBrains Rider

## Quick Start (5 Minutes)

### 1. Clone the Repository

```bash
git clone https://github.com/alexinslc/hickory.git
cd hickory
```

### 2. Start Infrastructure with Docker Compose

This command starts PostgreSQL, Redis, and all required services:

```bash
docker-compose up -d
```

**What this does**:
- PostgreSQL 16 on `localhost:5432`
- Redis 7 on `localhost:6379`
- Initializes database with schema and seed data
- Creates default admin user (credentials in `.env` file)

### 3. Install Dependencies

```bash
# Install root dependencies and bootstrap monorepo
npm install

# Nx will automatically install dependencies for all apps
```

### 4. Run Database Migrations

```bash
# Navigate to API project
cd apps/api

# Apply migrations
dotnet ef database update

# Return to root
cd ../..
```

### 5. Start the Applications

In separate terminals or use the provided npm scripts:

```bash
# Terminal 1: Start API (backend)
npm run start:api

# Terminal 2: Start Web UI (frontend)
npm run start:web

# Terminal 3 (optional): Build CLI
npm run build:cli
```

### 6. Access the Application

- **Web UI**: http://localhost:3000
- **API**: http://localhost:5000
- **API Documentation (Swagger)**: http://localhost:5000/swagger
- **Health Checks**: http://localhost:5000/health

### 7. Login with Default Admin

```
Email: admin@hickory.local
Password: (check .env file or docker-compose logs)
```

## Detailed Setup

### Environment Configuration

Create a `.env` file in the project root (copy from `.env.example`):

```env
# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=hickory
DATABASE_USER=hickory_user
DATABASE_PASSWORD=your_secure_password_here

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# JWT Authentication
JWT_SECRET=your_jwt_secret_key_here_minimum_32_chars
JWT_EXPIRATION_MINUTES=60
REFRESH_TOKEN_EXPIRATION_DAYS=30

# OAuth / OIDC (optional)
OIDC_AUTHORITY=https://your-identity-provider.com
OIDC_CLIENT_ID=your_client_id
OIDC_CLIENT_SECRET=your_client_secret

# Email (SMTP)
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USER=your_smtp_user
SMTP_PASSWORD=your_smtp_password
SMTP_FROM_EMAIL=noreply@hickory.local

# File Storage
STORAGE_TYPE=local  # or 's3'
STORAGE_PATH=./uploads
# For S3:
# AWS_ACCESS_KEY_ID=your_access_key
# AWS_SECRET_ACCESS_KEY=your_secret_key
# AWS_S3_BUCKET=your_bucket_name
# AWS_REGION=us-east-1

# Observability
SERILOG_MIN_LEVEL=Information
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317  # For Jaeger/Zipkin

# Admin User (created on first migration)
ADMIN_EMAIL=admin@hickory.local
ADMIN_PASSWORD=Change_This_Password_123!
ADMIN_FIRST_NAME=System
ADMIN_LAST_NAME=Administrator
```

### Development Workflow

#### Backend Development (ASP.NET Core)

```bash
# Watch mode (auto-reload on file changes)
cd apps/api
dotnet watch run

# Run tests
dotnet test

# Run specific test project
dotnet test tests/Unit/
dotnet test tests/Integration/

# Create new migration
dotnet ef migrations add MigrationName

# Revert migration
dotnet ef migrations remove

# Generate OpenAPI spec
dotnet run --project src/Hickory.Api -- swagger tofile --output ../../specs/001-help-desk-core/contracts/openapi.yaml
```

#### Frontend Development (Next.js)

```bash
# Development server with hot reload
cd apps/web
npm run dev

# Generate TypeScript client from OpenAPI
npm run generate:client

# Run tests
npm run test

# Run E2E tests
npm run test:e2e

# Lint and format
npm run lint
npm run format
```

#### CLI Development (Node.js)

```bash
# Build CLI
cd apps/cli
npm run build

# Link CLI globally for testing
npm link

# Run CLI commands
hickory --help
hickory ticket create
hickory ticket list --status Open

# Unlink after testing
npm unlink -g hickory
```

### Database Management

#### Reset Database

```bash
# Stop containers
docker-compose down

# Remove volumes (WARNING: deletes all data)
docker-compose down -v

# Restart
docker-compose up -d

# Reapply migrations
cd apps/api
dotnet ef database update
```

#### Seed Test Data

```bash
cd apps/api
dotnet run --project src/Hickory.Api -- seed --environment Development

# Or use specific seed script
dotnet run --project src/Hickory.Api -- seed --tickets 100 --users 10
```

#### Backup/Restore Database

```bash
# Backup
docker exec hickory-postgres pg_dump -U hickory_user hickory > backup.sql

# Restore
docker exec -i hickory-postgres psql -U hickory_user hickory < backup.sql
```

### Common Tasks

#### Generate New Feature Slice (Backend)

```bash
# Using Nx generator
nx generate @nx/dotnet:library tickets-create --directory=apps/api/src/Features/Tickets/Create

# Manual structure:
mkdir -p apps/api/src/Features/Tickets/Create
touch apps/api/src/Features/Tickets/Create/CreateTicketCommand.cs
touch apps/api/src/Features/Tickets/Create/CreateTicketValidator.cs
touch apps/api/src/Features/Tickets/Create/CreateTicketHandler.cs
touch apps/api/tests/Unit/Features/Tickets/CreateTicketTests.cs
```

#### Add New React Component (Frontend)

```bash
# Using Nx generator
nx generate @nx/react:component ticket-card --project=web --directory=src/components/tickets

# Add ShadCN UI component
npx shadcn-ui@latest add button
```

#### Run All Tests

```bash
# Root level - runs all tests across all projects
npm run test:all

# Or use Nx
nx run-many --target=test --all
```

#### Build for Production

```bash
# Build all apps
npm run build

# Build specific app
npm run build:api
npm run build:web
npm run build:cli

# Build Docker images
docker build -f docker/api.Dockerfile -t hickory-api:latest .
docker build -f docker/web.Dockerfile -t hickory-web:latest .
```

### Debugging

#### Backend (ASP.NET Core)

##### Visual Studio Code

Add to `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (API)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/apps/api/bin/Debug/net9.0/Hickory.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/apps/api",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    }
  ]
}
```

##### Visual Studio / Rider

Open `Hickory.sln` and press F5 to start debugging.

#### Frontend (Next.js)

##### Visual Studio Code

Add to `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Next.js: debug server-side",
      "type": "node-terminal",
      "request": "launch",
      "command": "npm run dev",
      "cwd": "${workspaceFolder}/apps/web",
      "serverReadyAction": {
        "pattern": "started server on .+, url: (https?://.+)",
        "uriFormat": "%s",
        "action": "debugWithChrome"
      }
    }
  ]
}
```

#### Database Queries

Use database client of your choice:

```
Host: localhost
Port: 5432
Database: hickory
Username: hickory_user
Password: (from .env)
```

**Recommended Clients**:
- pgAdmin
- DBeaver
- DataGrip
- TablePlus

### Troubleshooting

#### Port Already in Use

```bash
# Check what's using port 5000 (API)
lsof -i :5000
# Or on Windows:
netstat -ano | findstr :5000

# Kill process or change port in appsettings.json
```

#### Docker Containers Won't Start

```bash
# Check logs
docker-compose logs postgres
docker-compose logs redis

# Rebuild containers
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

#### TypeScript Types Out of Sync

```bash
# Regenerate client from OpenAPI
cd apps/web
npm run generate:client

# Restart TypeScript server in VS Code
Cmd/Ctrl + Shift + P -> "TypeScript: Restart TS Server"
```

#### EF Core Migrations Fail

```bash
# Check database connection
docker exec hickory-postgres psql -U hickory_user -d hickory -c "SELECT version();"

# Drop and recreate database
docker exec hickory-postgres psql -U hickory_user -c "DROP DATABASE hickory;"
docker exec hickory-postgres psql -U hickory_user -c "CREATE DATABASE hickory;"

# Reapply migrations
cd apps/api
dotnet ef database update
```

#### Tests Failing

```bash
# Clean test databases (Testcontainers)
docker ps -a | grep testcontainers | awk '{print $1}' | xargs docker rm -f

# Clear test cache
rm -rf apps/api/tests/**/bin apps/api/tests/**/obj
dotnet clean

# Rebuild and run
dotnet build
dotnet test
```

### Performance Tips

1. **Use Nx Cache**: Nx caches build and test results to speed up subsequent runs
2. **Selective Builds**: Use `nx affected` to only build changed projects
3. **Database Indexes**: Monitor slow queries in development logs
4. **Hot Reload**: Both .NET and Next.js support hot reload for fast iteration

### Next Steps

1. ✅ Complete this quickstart guide
2. 📖 Read [data-model.md](./data-model.md) to understand the database schema
3. 📖 Review [contracts/openapi.yaml](./contracts/openapi.yaml) for API endpoints
4. 🏗️ Check [plan.md](./plan.md) for architecture details
5. ✅ Review [constitution.md](../../.specify/memory/constitution.md) for coding standards
6. 🎯 Start implementing user stories from [spec.md](./spec.md)

### Useful Commands Reference

```bash
# Nx Commands
nx graph                          # View project dependency graph
nx affected:build                 # Build only affected projects
nx affected:test                  # Test only affected projects
nx run-many --target=lint --all  # Lint all projects

# Docker Commands
docker-compose ps                 # List running containers
docker-compose logs -f api        # Follow API logs
docker-compose restart postgres   # Restart database
docker system prune -a            # Clean up unused images/containers

# Git Commands (with Speckit)
git checkout -b ###-feature-name  # Feature branch naming convention
git commit -m "feat: description" # Conventional commits
```

### Support & Resources

- **Documentation**: `/specs/001-help-desk-core/`
- **API Docs**: http://localhost:5000/swagger
- **Constitution**: `.specify/memory/constitution.md`
- **Issue Tracker**: GitHub Issues (link TBD)
- **Discussions**: GitHub Discussions (link TBD)

---

**Quickstart Version**: 1.0  
**Last Tested**: October 26, 2025  
**Estimated Setup Time**: 5-10 minutes
