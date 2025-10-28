#!/usr/bin/env bash

# Performance Test Setup Script
# This script sets up the test user required for running performance tests

set -e

echo "======================================"
echo "Hickory Performance Test Setup"
echo "======================================"
echo ""

# Configuration
API_URL="${API_URL:-http://localhost:5000}"
TEST_EMAIL="${TEST_USER_EMAIL:-perftest@example.com}"
TEST_PASSWORD="${TEST_USER_PASSWORD:-TestPassword123!}"

echo "Configuration:"
echo "  API URL: $API_URL"
echo "  Test User Email: $TEST_EMAIL"
echo ""

# Check if API is running
echo "Checking if API is running..."
if ! curl -s -f "${API_URL}/health" > /dev/null 2>&1; then
    echo "❌ ERROR: API is not running at ${API_URL}"
    echo ""
    echo "Please start the API server first:"
    echo "  cd apps/api"
    echo "  dotnet run"
    echo ""
    exit 1
fi

echo "✓ API is running"
echo ""

# Register test user
echo "Creating test user..."
RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "${API_URL}/api/v1/auth/register" \
    -H "Content-Type: application/json" \
    -d "{
        \"email\": \"${TEST_EMAIL}\",
        \"password\": \"${TEST_PASSWORD}\",
        \"firstName\": \"Performance\",
        \"lastName\": \"Test\"
    }" 2>&1)

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')

if [ "$HTTP_CODE" = "201" ] || [ "$HTTP_CODE" = "200" ]; then
    echo "✓ Test user created successfully"
elif [ "$HTTP_CODE" = "400" ] && echo "$BODY" | grep -q "already exists"; then
    echo "✓ Test user already exists"
else
    echo "❌ Failed to create test user (HTTP $HTTP_CODE)"
    echo "Response: $BODY"
    exit 1
fi

echo ""
echo "======================================"
echo "Setup Complete!"
echo "======================================"
echo ""
echo "You can now run performance tests:"
echo "  npm run test:performance"
echo ""
echo "Or with custom configuration:"
echo "  API_URL=${API_URL} npm run test:performance"
echo ""
