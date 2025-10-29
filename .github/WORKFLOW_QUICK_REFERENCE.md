# CI/CD Quick Reference

## Workflow Overview

**File**: `.github/workflows/ci.yml`  
**Triggers**: Push to main/develop/feature/fix branches, PRs to main/develop  
**Average Duration**: 8-12 minutes  

## Job Execution Order

1. **setup** (2 min) - Cache dependencies
2. **lint** + **build-*** (3 min parallel) - Lint & build all projects
3. **test-*** + **docker** + **security** (5 min parallel) - Run all tests & security
4. **ci-success** (5 sec) - Final status check

## Common Commands

### Run Locally
```bash
# Quick pre-commit check (runs all CI checks locally)
./scripts/pre-commit-check.sh

# Quick check before pushing
npm ci && npx nx run-many --target=lint,test --all

# Full build
dotnet build apps/api/Hickory.Api.csproj --configuration Release
npx nx build web

# Run tests
dotnet test apps/api/Hickory.Api.Tests/Hickory.Api.Tests.csproj
npx nx test web
```

### Monitor Workflow Status
```bash
# Check workflow status (requires gh CLI)
./scripts/workflow-status.sh

# Watch current workflow run
gh run watch

# View workflow in browser
gh run view --web
```

### Fix Common Issues
```bash
# Fix package lock sync
npm install

# Clear node_modules
rm -rf node_modules && npm ci

# Fix .NET format
cd apps/api && dotnet format
```

## Key Metrics

| Metric | Target | Current |
|--------|--------|---------|
| Total CI Time | <15 min | ~10 min |
| Setup Time | <2 min | ~1.5 min |
| Test Time | <5 min | ~3 min |
| Cache Hit Rate | >80% | ~90% |

## Status Badges

Add to README.md:
```markdown
[![CI](https://github.com/alexinslc/hickory/workflows/CI%20Pipeline/badge.svg)](https://github.com/alexinslc/hickory/actions)
```

## Tips

- **Faster Feedback**: Push to feature branches - lint/build run in parallel
- **Debug Failures**: Click on failed job → view logs → scroll to red section
- **Skip CI**: Add `[skip ci]` to commit message (use sparingly!)
- **Re-run Jobs**: Click "Re-run failed jobs" button on failed runs
- **Check Coverage**: Coverage reports in PR comments from Codecov bot

## See Full Documentation

For detailed information, see [WORKFLOW.md](./WORKFLOW.md)
