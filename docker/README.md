# Hickory Help Desk - Docker Setup

Complete Docker configuration for local development and production deployment of the Hickory Help Desk system.

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Service Details](#service-details)
- [Configuration](#configuration)
- [Development Workflow](#development-workflow)
- [Production Deployment](#production-deployment)
- [Troubleshooting](#troubleshooting)
- [Maintenance](#maintenance)

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Docker Network                           │
│                   (hickory-network)                          │
│                                                              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │          │  │          │  │          │  │          │   │
│  │ Next.js  │──│ .NET API │──│PostgreSQL│  │  Redis   │   │
│  │   Web    │  │ Backend  │  │ Database │  │  Cache   │   │
│  │  :3000   │  │  :8080   │  │  :5432   │  │  :6379   │   │
│  │          │  │          │  │          │  │          │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│                      │                                       │
│                      │                                       │
│                 ┌────────┐                                  │
│                 │MailHog │                                  │
│                 │ :8025  │                                  │
│                 └────────┘                                  │
└─────────────────────────────────────────────────────────────┘
```

### Services

1. **PostgreSQL** (postgres:16-alpine) - Primary database
2. **Redis** (redis:7-alpine) - Caching and session storage
3. **API** (.NET 9.0) - ASP.NET Core backend
4. **Web** (Node 25) - Next.js frontend
5. **MailHog** (optional) - Email testing during development

## 📦 Prerequisites

- **Docker**: Version 24.0 or later
- **Docker Compose**: Version 2.20 or later
- **System Resources**:
  - 4GB RAM minimum (8GB recommended)
  - 10GB disk space
- **Ports Available**:
  - 3000 (Web UI)
  - 5000 (API)
  - 5432 (PostgreSQL)
  - 6379 (Redis)
  - 8025 (MailHog Web UI)
  - 1025 (MailHog SMTP)

### Verify Prerequisites

```bash
# Check Docker version
docker --version

# Check Docker Compose version
docker compose version

# Verify Docker is running
docker info
```

## 🚀 Quick Start

### 1. Initial Setup

```bash
# Clone the repository (if not already done)
git clone <repository-url>
cd hickory

# Copy environment configuration
cp docker/.env.example docker/.env

# Edit .env with your specific values (optional for development)
# nano docker/.env
```

### 2. Start All Services

```bash
# Build and start all services
docker compose -f docker/docker-compose.yml up -d

# Watch logs
docker compose -f docker/docker-compose.yml logs -f

# Check service health
docker compose -f docker/docker-compose.yml ps
```

### 3. Access the Application

- **Web UI**: http://localhost:3000
- **API**: http://localhost:5000
- **API Health**: http://localhost:5000/health
- **MailHog UI**: http://localhost:8025 (email testing)

### 4. Initialize Database

```bash
# Run migrations (first time only)
docker compose -f docker/docker-compose.yml exec api \
  dotnet ef database update --project Hickory.Api.csproj

# Or, if migrations aren't set up yet:
docker compose -f docker/docker-compose.yml exec api \
  dotnet ef migrations add InitialCreate --project Hickory.Api.csproj
docker compose -f docker/docker-compose.yml exec api \
  dotnet ef database update --project Hickory.Api.csproj
```

## 🏥 Health Checks

All services include Docker health checks to ensure proper orchestration and monitoring:

### API Health Check
- **Endpoint**: `/health`
- **Method**: HTTP GET via curl
- **Interval**: Every 30 seconds
- **Timeout**: 10 seconds
- **Start Period**: 40 seconds (allows time for startup)
- **Retries**: 3 consecutive failures before marking unhealthy

```bash
# Check API health status
docker ps --filter "name=api"

# Or use docker inspect
docker inspect --format='{{.State.Health.Status}}' hickory-api
```

### Web Health Check
- **Endpoint**: `/` (root endpoint)
- **Method**: HTTP GET via Node.js http module
- **Interval**: Every 30 seconds
- **Timeout**: 10 seconds
- **Start Period**: 40 seconds
- **Retries**: 3 consecutive failures before marking unhealthy

```bash
# Check web health status
docker ps --filter "name=web"

# Or use docker inspect
docker inspect --format='{{.State.Health.Status}}' hickory-web
```

### Database and Cache Health Checks
- **PostgreSQL**: Uses `pg_isready` command
- **Redis**: Uses `redis-cli ping` command

### Monitoring Health Status

```bash
# View all container health statuses
docker compose -f docker/docker-compose.yml ps

# Continuously monitor health
watch -n 5 'docker compose -f docker/docker-compose.yml ps'

# View detailed health check logs
docker inspect hickory-api | jq '.[0].State.Health'
docker inspect hickory-web | jq '.[0].State.Health'
```

### Unhealthy Container Behavior

Docker Compose automatically detects unhealthy containers but does not restart them by default. To enable automatic restarts on health check failure, you can use health check-aware orchestration like Docker Swarm or Kubernetes.

For development, you can manually restart an unhealthy container:

```bash
# Restart specific unhealthy service
docker compose -f docker/docker-compose.yml restart api

# Force recreation
docker compose -f docker/docker-compose.yml up -d --force-recreate api
```

## 🔧 Service Details

### PostgreSQL Database

- **Image**: postgres:16-alpine
- **Port**: 5432
- **Default Credentials**:
  - User: `hickory`
  - Password: `hickory_dev_password`
  - Database: `hickory`
- **Health Check**: pg_isready every 10s
- **Volume**: `postgres_data` (persists database)

**Connect to PostgreSQL CLI:**
```bash
docker compose -f docker/docker-compose.yml exec postgres \
  psql -U hickory -d hickory
```

### Redis Cache

- **Image**: redis:7-alpine
- **Port**: 6379
- **Health Check**: PING every 10s
- **Volume**: `redis_data` (persists cache)

**Connect to Redis CLI:**
```bash
docker compose -f docker/docker-compose.yml exec redis redis-cli
```

### .NET API Backend

- **Base Image**: .NET 9.0 SDK (build) / Runtime (production)
- **Port**: 5000 → 8080 (internal)
- **Health Endpoint**: http://localhost:5000/health
- **Dependencies**: PostgreSQL, Redis
- **Environment**: Development
- **Volume**: `api_logs` (persists application logs)

**View API Logs:**
```bash
docker compose -f docker/docker-compose.yml logs -f api
```

**Execute Commands in API Container:**
```bash
# Run migrations
docker compose -f docker/docker-compose.yml exec api \
  dotnet ef database update

# Run tests
docker compose -f docker/docker-compose.yml exec api \
  dotnet test
```

### Next.js Web Frontend

- **Base Image**: Node 20 Alpine
- **Port**: 3000
- **Build Mode**: Standalone (optimized)
- **Dependencies**: API service
- **Health Check**: HTTP GET / every 30s

**View Web Logs:**
```bash
docker compose -f docker/docker-compose.yml logs -f web
```

### MailHog (Development Email Testing)

- **Image**: mailhog/mailhog
- **SMTP Port**: 1025 (for sending)
- **Web UI Port**: 8025 (for viewing)
- **Purpose**: Captures all outgoing emails during development

**Access MailHog:**
- Open http://localhost:8025 in your browser
- All emails sent by the application will appear here

## ⚙️ Configuration

### Environment Variables

Key environment variables are configured in `docker-compose.yml` and can be overridden via `.env` file:

#### API Configuration
```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=postgres;...
ConnectionStrings__Redis=redis:6379
JWT__Secret=your-secret-key
```

#### Web Configuration
```env
NODE_ENV=production
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_WS_URL=ws://localhost:5000
```

### Custom Configuration

To customize settings:

1. Copy `.env.example` to `.env`
2. Update values in `.env`
3. Restart services: `docker compose -f docker/docker-compose.yml restart`

### Database Initialization Scripts

Place SQL scripts in `docker/init-db/` to run on first PostgreSQL startup:

```bash
# Create init-db directory
mkdir -p docker/init-db

# Add initialization script
cat > docker/init-db/01-extensions.sql << 'EOF'
-- Enable required PostgreSQL extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
EOF
```

## 🔄 Development Workflow

### Starting Services

```bash
# Start all services
docker compose -f docker/docker-compose.yml up -d

# Start specific service
docker compose -f docker/docker-compose.yml up -d postgres redis

# Start with rebuild
docker compose -f docker/docker-compose.yml up -d --build
```

### Stopping Services

```bash
# Stop all services
docker compose -f docker/docker-compose.yml down

# Stop and remove volumes (CAUTION: deletes data)
docker compose -f docker/docker-compose.yml down -v

# Stop specific service
docker compose -f docker/docker-compose.yml stop web
```

### Rebuilding Services

```bash
# Rebuild all services
docker compose -f docker/docker-compose.yml build

# Rebuild specific service
docker compose -f docker/docker-compose.yml build api

# Force rebuild without cache
docker compose -f docker/docker-compose.yml build --no-cache api
```

### Viewing Logs

```bash
# All services
docker compose -f docker/docker-compose.yml logs -f

# Specific service
docker compose -f docker/docker-compose.yml logs -f api

# Last 100 lines
docker compose -f docker/docker-compose.yml logs --tail=100 api
```

### Accessing Containers

```bash
# API shell
docker compose -f docker/docker-compose.yml exec api /bin/bash

# Web shell
docker compose -f docker/docker-compose.yml exec web /bin/sh

# PostgreSQL shell
docker compose -f docker/docker-compose.yml exec postgres /bin/sh
```

### Running Commands

```bash
# Run EF Core migrations
docker compose -f docker/docker-compose.yml exec api \
  dotnet ef migrations add MigrationName

# Run tests
docker compose -f docker/docker-compose.yml exec api \
  dotnet test

# Install npm packages (if needed)
docker compose -f docker/docker-compose.yml exec web \
  npm install package-name
```

## 🚢 Production Deployment

### Pre-Deployment Checklist

- [ ] Update JWT secret in `.env`
- [ ] Set strong PostgreSQL password
- [ ] Configure production SMTP settings
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Set `NODE_ENV=production`
- [ ] Review and restrict exposed ports
- [ ] Configure reverse proxy (nginx/Caddy)
- [ ] Set up SSL/TLS certificates
- [ ] Configure backup strategy
- [ ] Set up monitoring and logging
- [ ] Review security settings

### Production Environment Variables

```env
# Production overrides in .env
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET=<generate-strong-secret>
POSTGRES_PASSWORD=<strong-password>
NEXT_PUBLIC_API_URL=https://api.yourdomain.com
```

### Deployment Steps

```bash
# 1. Pull latest code
git pull origin main

# 2. Update environment variables
nano docker/.env

# 3. Build production images
docker compose -f docker/docker-compose.yml build --no-cache

# 4. Start services
docker compose -f docker/docker-compose.yml up -d

# 5. Run migrations
docker compose -f docker/docker-compose.yml exec api \
  dotnet ef database update

# 6. Verify health
curl http://localhost:5000/health
curl http://localhost:3000/
```

### Production Monitoring

```bash
# Check service status
docker compose -f docker/docker-compose.yml ps

# View resource usage
docker stats

# Check logs
docker compose -f docker/docker-compose.yml logs --tail=100 -f
```

## 🔍 Troubleshooting

### Services Not Starting

**Symptom**: Containers exit immediately

**Solutions**:
```bash
# Check logs
docker compose -f docker/docker-compose.yml logs

# Verify port conflicts
netstat -tuln | grep -E '3000|5000|5432|6379'

# Check disk space
df -h

# Rebuild from scratch
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml build --no-cache
docker compose -f docker/docker-compose.yml up -d
```

### Database Connection Errors

**Symptom**: API can't connect to PostgreSQL

**Solutions**:
```bash
# Check PostgreSQL is running
docker compose -f docker/docker-compose.yml ps postgres

# Check PostgreSQL logs
docker compose -f docker/docker-compose.yml logs postgres

# Verify connection string in .env
docker compose -f docker/docker-compose.yml exec api env | grep Connection

# Test connection manually
docker compose -f docker/docker-compose.yml exec postgres \
  psql -U hickory -d hickory -c "SELECT version();"
```

### Web App Can't Reach API

**Symptom**: Frontend shows API connection errors

**Solutions**:
```bash
# Check API health
curl http://localhost:5000/health

# Verify API logs
docker compose -f docker/docker-compose.yml logs api

# Check network connectivity
docker compose -f docker/docker-compose.yml exec web \
  wget -O- http://api:8080/health

# Verify environment variables
docker compose -f docker/docker-compose.yml exec web env | grep NEXT_PUBLIC
```

### Performance Issues

**Symptom**: Slow response times

**Solutions**:
```bash
# Check resource usage
docker stats

# Increase Docker resources (Docker Desktop)
# Settings → Resources → Increase CPU/Memory

# Check database performance
docker compose -f docker/docker-compose.yml exec postgres \
  psql -U hickory -d hickory -c "SELECT * FROM pg_stat_activity;"

# Clear Redis cache
docker compose -f docker/docker-compose.yml exec redis redis-cli FLUSHALL
```

### Permission Errors

**Symptom**: Permission denied errors in containers

**Solutions**:
```bash
# Fix volume permissions
sudo chown -R $USER:$USER ./docker

# Rebuild with proper permissions
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml build --no-cache
docker compose -f docker/docker-compose.yml up -d
```

## 🛠️ Maintenance

### Backup Database

```bash
# Create backup
docker compose -f docker/docker-compose.yml exec postgres \
  pg_dump -U hickory hickory > backup-$(date +%Y%m%d).sql

# Restore backup
docker compose -f docker/docker-compose.yml exec -T postgres \
  psql -U hickory hickory < backup-20250101.sql
```

### Update Services

```bash
# Pull latest images
docker compose -f docker/docker-compose.yml pull

# Rebuild custom images
docker compose -f docker/docker-compose.yml build --pull

# Restart with new images
docker compose -f docker/docker-compose.yml up -d
```

### Clean Up

```bash
# Remove unused images
docker image prune -a

# Remove unused volumes (CAUTION)
docker volume prune

# Remove everything (CAUTION: deletes all data)
docker compose -f docker/docker-compose.yml down -v --rmi all
```

### View Resource Usage

```bash
# Check container stats
docker stats

# Check volume sizes
docker system df

# Detailed disk usage
docker system df -v
```

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [Redis Docker Hub](https://hub.docker.com/_/redis)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Node.js Docker Images](https://hub.docker.com/_/node)

## 🆘 Support

For issues and questions:
- Check logs: `docker compose -f docker/docker-compose.yml logs`
- Review troubleshooting section above
- Create an issue in the repository
- Contact the development team

---

**Last Updated**: 2025-01-26  
**Docker Compose Version**: 3.8  
**Tested With**: Docker 24.0+, Docker Compose 2.20+
