#!/bin/bash
# Test script to verify Docker health checks are working

set -e

echo "🏥 Testing Docker Health Checks"
echo "================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if containers are running
echo "📊 Container Status:"
docker compose -f docker/docker-compose.yml ps
echo ""

# Test API health endpoint
echo "🔍 Testing API Health Endpoint..."
if curl -f -s http://localhost:5000/health > /dev/null; then
    echo -e "${GREEN}✓${NC} API /health endpoint is responding"
else
    echo -e "${RED}✗${NC} API /health endpoint is not responding"
    exit 1
fi

if curl -f -s http://localhost:5000/health/ready > /dev/null; then
    echo -e "${GREEN}✓${NC} API /health/ready endpoint is responding"
else
    echo -e "${YELLOW}⚠${NC} API /health/ready endpoint is not responding"
fi

if curl -f -s http://localhost:5000/health/live > /dev/null; then
    echo -e "${GREEN}✓${NC} API /health/live endpoint is responding"
else
    echo -e "${YELLOW}⚠${NC} API /health/live endpoint is not responding"
fi
echo ""

# Test Web health
echo "🔍 Testing Web Health..."
if curl -f -s http://localhost:3000/ > /dev/null; then
    echo -e "${GREEN}✓${NC} Web root endpoint is responding"
else
    echo -e "${RED}✗${NC} Web root endpoint is not responding"
    exit 1
fi
echo ""

# Check Docker health status
echo "🏥 Docker Health Status:"
API_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' hickory-api 2>/dev/null || echo "N/A")
WEB_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' hickory-web 2>/dev/null || echo "N/A")
POSTGRES_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' hickory-postgres 2>/dev/null || echo "N/A")
REDIS_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' hickory-redis 2>/dev/null || echo "N/A")

echo "  API:        $API_HEALTH"
echo "  Web:        $WEB_HEALTH"
echo "  PostgreSQL: $POSTGRES_HEALTH"
echo "  Redis:      $REDIS_HEALTH"
echo ""

# Summary
if [ "$API_HEALTH" = "healthy" ] && [ "$WEB_HEALTH" = "healthy" ]; then
    echo -e "${GREEN}✓ All services are healthy!${NC}"
    exit 0
else
    echo -e "${YELLOW}⚠ Some services are not healthy. Check docker logs for details.${NC}"
    echo ""
    echo "To view logs:"
    echo "  docker compose -f docker/docker-compose.yml logs api"
    echo "  docker compose -f docker/docker-compose.yml logs web"
    exit 1
fi
