# Development Container

This directory contains the configuration for developing Hickory Help Desk in a containerized environment using VS Code Dev Containers.

## Prerequisites

- [Docker](https://www.docker.com/products/docker-desktop) installed and running
- [Visual Studio Code](https://code.visualstudio.com/)
- [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) for VS Code

## Quick Start

1. **Open in Dev Container**
   - Open this repository in VS Code
   - Click the green button in the bottom-left corner (or press `F1`)
   - Select "Dev Containers: Reopen in Container"
   - Wait for the container to build and start (first time may take 5-10 minutes)

2. **Automatic Setup**
   - Node.js dependencies will be installed automatically
   - .NET packages will be restored
   - Database migrations will run
   - All services (PostgreSQL, Redis, MailHog) will be available

3. **Start Development**
   ```bash
   # Terminal 1: Start the API
   cd apps/api
   dotnet run

   # Terminal 2: Start the Web Frontend
   cd apps/web
   npm run dev
   ```

4. **Access Services**
   - Web Frontend: http://localhost:3000
   - API Backend: http://localhost:5000
   - API Swagger: http://localhost:5000/swagger
   - MailHog UI: http://localhost:8025
   - PostgreSQL: localhost:5432
   - Redis: localhost:6379

## What's Included

### Development Tools
- .NET 9.0 SDK
- Node.js 20.x with npm
- Git & GitHub CLI
- Docker CLI (docker-from-docker)
- PostgreSQL client tools
- Redis CLI tools
- Nx CLI
- TypeScript & ts-node

### VS Code Extensions
- C# Dev Kit (.NET development)
- ESLint & Prettier (code formatting)
- Tailwind CSS IntelliSense
- Docker extension
- SQLTools with PostgreSQL driver
- GitLens
- EditorConfig support

### Services
- **PostgreSQL 16**: Primary database
- **Redis 7**: Caching layer
- **MailHog**: Email testing (SMTP on 1025, UI on 8025)

## Common Tasks

### Database Management
```bash
# Run migrations
cd apps/api
dotnet ef database update

# Create a new migration
dotnet ef migrations add MigrationName

# Reset database (caution!)
dotnet ef database drop
dotnet ef database update
```

### Testing
```bash
# Run all tests
npm test

# Run performance tests
npm run test:performance

# Run E2E tests
cd apps/web-e2e
npx playwright test
```

### Building
```bash
# Build all projects
npx nx run-many --target=build --all

# Build specific project
npx nx build api
npx nx build web
```

### Linting
```bash
# Lint all projects
npx nx run-many --target=lint --all

# Format code
npx prettier --write .
```

## Troubleshooting

### Container Won't Start
- Ensure Docker is running
- Check Docker has enough resources (4GB RAM minimum, 8GB recommended)
- Try rebuilding: `Dev Containers: Rebuild Container`

### Database Connection Issues
- Check PostgreSQL is running: `pg_isready -h localhost -U hickory`
- Verify connection string in environment variables
- Try restarting the container

### Port Already in Use
- Stop other services using ports 3000, 5000, 5432, 6379, 8025
- Or modify port mappings in `docker-compose.yml`

### Node Modules Issues
```bash
# Clean install
rm -rf node_modules package-lock.json
npm install
```

### .NET Build Issues
```bash
# Clean and restore
cd apps/api
dotnet clean
dotnet restore
```

## Architecture

The devcontainer uses a multi-service setup:
- **devcontainer**: Main development container with .NET, Node.js, and tools
- **db**: PostgreSQL database service
- **redis**: Redis cache service  
- **mailhog**: Email testing service

All services share the same network, allowing seamless communication between them.

## Customization

### Add VS Code Extensions
Edit `.devcontainer/devcontainer.json` and add to the `extensions` array:
```json
"customizations": {
  "vscode": {
    "extensions": [
      "your-extension-id"
    ]
  }
}
```

### Change Environment Variables
Edit `.devcontainer/docker-compose.yml` and modify the `environment` section.

### Install Additional Tools
Edit `.devcontainer/Dockerfile` and add installation commands.

## Best Practices

1. **Commit Often**: Your work is persisted in the mounted volume
2. **Use Multiple Terminals**: Run API and Web in separate terminals
3. **Check Services**: Ensure PostgreSQL and Redis are running before starting the app
4. **Git Configuration**: Set your git user.name and user.email in the container
5. **Resource Management**: Close the devcontainer when not in use to free resources

## Support

For issues or questions:
- Check the main [README.md](../README.md)
- Review [Docker documentation](../docker/README.md)
- Open an issue in the repository
