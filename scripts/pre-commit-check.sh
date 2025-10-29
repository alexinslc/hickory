#!/bin/bash
# Pre-commit CI validation script
# Run this before pushing to catch issues early

set -e

echo "🔍 Running pre-commit CI checks..."
echo ""

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Track failures
FAILED=0

# Function to run a check
run_check() {
    local name=$1
    local cmd=$2
    local tmpfile=$(mktemp)
    
    echo -n "  → $name... "
    if eval "$cmd" > "$tmpfile" 2>&1; then
        echo -e "${GREEN}✓${NC}"
        rm -f "$tmpfile"
    else
        echo -e "${RED}✗${NC}"
        echo -e "${YELLOW}    See $tmpfile for details${NC}"
        FAILED=1
    fi
}

# 1. Check Node.js version
echo "📦 Checking environment..."
NODE_VERSION=$(node -v | cut -d'v' -f2 | cut -d'.' -f1)
if [ "$NODE_VERSION" -ge 20 ]; then
    echo -e "  → Node.js version... ${GREEN}✓${NC} (v$(node -v))"
else
    echo -e "  → Node.js version... ${RED}✗${NC} (Need v20+, got v$(node -v))"
    FAILED=1
fi

# Check .NET version
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version | cut -d'.' -f1)
    if [ "$DOTNET_VERSION" -ge 9 ]; then
        echo -e "  → .NET version... ${GREEN}✓${NC} ($(dotnet --version))"
    else
        echo -e "  → .NET version... ${RED}✗${NC} (Need 9.0+, got $(dotnet --version))"
        FAILED=1
    fi
else
    echo -e "  → .NET version... ${YELLOW}⚠${NC} (Not installed)"
fi

echo ""

# 2. Dependency checks
echo "📚 Checking dependencies..."
if [ ! -d "node_modules" ]; then
    echo "  → Installing npm dependencies..."
    npm ci > /dev/null 2>&1
fi
echo -e "  → npm dependencies... ${GREEN}✓${NC}"
echo ""

# 3. Linting
echo "🔎 Running linters..."
run_check "Web linting" "npx nx lint web --quiet"
run_check "CLI linting" "npx nx lint cli --quiet"

if command -v dotnet &> /dev/null; then
    run_check ".NET format check" "dotnet format apps/api/Hickory.Api.csproj --verify-no-changes --no-restore --verbosity quiet"
fi
echo ""

# 4. Build checks
echo "🔨 Running builds..."
run_check "Web build" "npx nx build web --skip-nx-cache"
run_check "CLI build" "npx nx build cli --skip-nx-cache"

if command -v dotnet &> /dev/null; then
    run_check ".NET API build" "dotnet build apps/api/Hickory.Api.csproj --configuration Release --no-restore --verbosity quiet"
fi
echo ""

# 5. Unit tests
echo "🧪 Running unit tests..."
run_check "Web tests" "npx nx test web --skip-nx-cache --silent"
run_check "CLI tests" "npx nx test cli --skip-nx-cache --silent"

if command -v dotnet &> /dev/null; then
    run_check ".NET unit tests" "dotnet test apps/api/Hickory.Api.Tests/Hickory.Api.Tests.csproj --no-build --configuration Release --verbosity quiet --nologo"
fi
echo ""

# 6. Security checks (optional, fast check)
echo "🔒 Running quick security check..."
if command -v npm &> /dev/null; then
    # npm audit returns exit code 0 if no vulnerabilities found
    if npm audit --audit-level=high > /dev/null 2>&1; then
        echo -e "  → npm audit... ${GREEN}✓${NC}"
    else
        echo -e "  → npm audit... ${YELLOW}⚠${NC} (Run 'npm audit' for details)"
    fi
fi
echo ""

# Summary
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ All checks passed!${NC} Ready to commit and push."
    exit 0
else
    echo -e "${RED}✗ Some checks failed.${NC} Please fix the issues above."
    exit 1
fi
