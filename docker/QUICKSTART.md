# Docker Quick Start Guide

Get Hickory Help Desk running in 5 minutes.

## Prerequisites

- Docker and Docker Compose installed
- Ports available: 3000, 5000, 5432, 6379, 8025

## Start Everything

```bash
# From project root
cd hickory

# Start all services
docker compose -f docker/docker-compose.yml up -d

# Watch logs (optional)
docker compose -f docker/docker-compose.yml logs -f
```

Wait 30-60 seconds for all services to become healthy.

## Initialize Database

```bash
# Run migrations (first time only)
docker compose -f docker/docker-compose.yml exec api \
  dotnet ef database update
```

## Access the Application

- **Web UI**: http://localhost:3000
- **API**: http://localhost:5000
- **API Health**: http://localhost:5000/health
- **Email Testing**: http://localhost:8025

## Stop Everything

```bash
docker compose -f docker/docker-compose.yml down
```

## Common Commands

```bash
# View logs
docker compose -f docker/docker-compose.yml logs -f

# Restart a service
docker compose -f docker/docker-compose.yml restart api

# Check status
docker compose -f docker/docker-compose.yml ps

# Rebuild and restart
docker compose -f docker/docker-compose.yml up -d --build

# Stop and remove all data
docker compose -f docker/docker-compose.yml down -v
```

## Troubleshooting

**Services not starting?**
```bash
# Check what's wrong
docker compose -f docker/docker-compose.yml ps
docker compose -f docker/docker-compose.yml logs

# Nuclear option: start fresh
docker compose -f docker/docker-compose.yml down -v
docker compose -f docker/docker-compose.yml up -d --build
```

**Port conflicts?**
```bash
# Check what's using the ports
netstat -tuln | grep -E '3000|5000|5432|6379'

# Change ports in docker-compose.yml if needed
```

**Need more help?**
See the full [Docker README](./README.md) for detailed documentation.

---

**Quick Tip**: Set an alias to save typing:
```bash
# Add to ~/.zshrc or ~/.bashrc
alias dch='docker compose -f docker/docker-compose.yml'

# Then use:
dch up -d
dch logs -f
dch down
```
