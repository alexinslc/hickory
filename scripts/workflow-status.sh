#!/bin/bash
# Check GitHub Actions workflow status
# Requires: gh CLI (GitHub CLI) to be installed and authenticated

set -e

# Configuration
WORKFLOW_FILE="${WORKFLOW_FILE:-ci.yml}"  # Can be overridden via environment variable

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Hickory CI/CD Workflow Status"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Check if gh CLI is installed
if ! command -v gh &> /dev/null; then
    echo -e "${RED}✗ GitHub CLI (gh) is not installed${NC}"
    echo ""
    echo "Install instructions:"
    echo "  macOS:   brew install gh"
    echo "  Linux:   See https://github.com/cli/cli#installation"
    echo "  Windows: winget install GitHub.cli"
    echo ""
    echo "After installing, run: gh auth login"
    exit 1
fi

# Check authentication
if ! gh auth status &> /dev/null; then
    echo -e "${RED}✗ Not authenticated with GitHub${NC}"
    echo ""
    echo "Please run: gh auth login"
    exit 1
fi

# Get current branch and default branch
BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || git symbolic-ref refs/remotes/origin/HEAD 2>/dev/null | sed 's@^refs/remotes/origin/@@' || echo "main")
echo -e "${BLUE}Branch:${NC} $BRANCH"
echo ""

# Get latest workflow runs
echo "Latest CI Pipeline runs ($WORKFLOW_FILE):"
echo ""

gh run list --workflow="$WORKFLOW_FILE" --limit 5 --json status,conclusion,displayTitle,createdAt,url \
    --jq '.[] | 
        if .status == "completed" then
            if .conclusion == "success" then
                "  ✓ " + .displayTitle + " - " + .createdAt[:10] + " " + .createdAt[11:19] + " - " + .url
            elif .conclusion == "failure" then
                "  ✗ " + .displayTitle + " - " + .createdAt[:10] + " " + .createdAt[11:19] + " - " + .url
            else
                "  ? " + .displayTitle + " - " + .createdAt[:10] + " " + .createdAt[11:19] + " - " + .url
            end
        else
            "  ⋯ " + .displayTitle + " - " + .status + " - " + .url
        end' \
    | while IFS= read -r line; do
        if [[ $line == *"✓"* ]]; then
            echo -e "${GREEN}${line}${NC}"
        elif [[ $line == *"✗"* ]]; then
            echo -e "${RED}${line}${NC}"
        elif [[ $line == *"⋯"* ]]; then
            echo -e "${YELLOW}${line}${NC}"
        else
            echo "$line"
        fi
    done

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "Commands:"
echo "  View all runs:         gh run list --workflow=$WORKFLOW_FILE"
echo "  View specific run:     gh run view <run-id>"
echo "  Watch latest run:      gh run watch"
echo "  Open in browser:       gh run view --web"
echo ""
echo "Configuration:"
echo "  Set workflow file:     export WORKFLOW_FILE=custom.yml && $0"
echo ""
