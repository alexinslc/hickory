# GitHub Actions Workflow Documentation

## Overview

The Hickory project uses a single, optimized CI/CD workflow (`.github/workflows/ci.yml`) that handles all continuous integration tasks including linting, building, testing, and security scanning.

## Workflow Triggers

The workflow runs on:
- **Push** to: `main`, `develop`, `feature/**`, `fix/**`, `001-help-desk-core`
- **Pull Requests** targeting: `main`, `develop`

## Concurrency Control

The workflow includes concurrency control to cancel in-progress runs when a new commit is pushed:
```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

This saves CI minutes and provides faster feedback on the latest changes.

## Workflow Architecture

The workflow is organized into the following job stages:

### 1. Setup Dependencies (setup)
- Installs and caches Node.js dependencies
- Restores and caches .NET dependencies
- **Duration**: ~1-2 minutes (first run), ~30 seconds (cached)
- **Key Optimization**: Creates shared caches used by all subsequent jobs

### 2. Lint & Format (lint)
- Lints Web and CLI projects with ESLint
- Checks .NET formatting with `dotnet format`
- **Duration**: ~1 minute
- **Runs in parallel with**: Build jobs

### 3. Build Stage (parallel)
Three separate build jobs run in parallel:
- **build-api**: Builds .NET API and test projects, uploads build artifacts
- **build-web**: Builds Next.js web application, uploads build artifacts
- **build-cli**: Builds CLI application
- **Duration**: ~2-3 minutes
- **Key Optimization**: Build artifacts are cached and reused in test jobs

### 4. Test Stage (parallel)
Five test jobs run in parallel:
- **test-api-unit**: Runs .NET unit tests using cached build artifacts
- **test-api-integration**: Runs .NET integration tests using cached build artifacts
- **test-web**: Runs Jest tests for Web application
- **test-cli**: Runs Jest tests for CLI application
- **test-e2e**: Runs Playwright E2E tests with API running in background
- **Duration**: ~2-5 minutes per job
- **Key Optimization**: Tests don't rebuild, they use cached artifacts

### 5. Docker & Security (parallel)
- **docker**: Validates Docker builds for API and Web (matrix strategy)
- **security**: Runs Trivy security scanner and uploads results to GitHub Security
- **Duration**: ~2-3 minutes

### 6. CI Success (ci-success)
- Final status check that validates all jobs passed
- Required for branch protection rules
- **Duration**: ~5 seconds

## Key Improvements Over Previous Workflows

### Performance Improvements
1. **Dependency Caching**: Setup job creates shared caches reducing install time from 2+ minutes to ~30 seconds
2. **Build Artifact Reuse**: Test jobs download pre-built artifacts instead of rebuilding (~3 minutes saved)
3. **Parallel Execution**: Jobs run in parallel where possible (previously sequential)
4. **Concurrency Control**: Cancels stale runs saving CI minutes

**Total Time Savings**: ~5-10 minutes per run

### Reliability Improvements
1. **Fixed Node Version**: Corrected from non-existent '25.x' to '20' (LTS)
2. **Fixed Package Lock**: Synchronized package-lock.json with package.json
3. **No Error Masking**: Removed `|| echo "skipped"` patterns that hid failures
4. **Proper Test Reporting**: All test failures now properly fail the CI
5. **Integration Tests**: Added previously missing Hickory.Api.IntegrationTests

### Maintainability Improvements
1. **Single Workflow**: Consolidated 3 workflows (test.yml, quick-test.yml, ci.yml) into one
2. **Consistent Configuration**: Environment variables defined once at workflow level
3. **Clear Job Dependencies**: Explicit `needs` relationships show execution order
4. **Better Job Names**: Descriptive names make it clear what each job does
5. **Reduced Duplication**: Shared caching patterns eliminate repeated code

## Environment Variables

The workflow uses two environment variables for version consistency:
- `NODE_VERSION: '20'` - Node.js LTS version
- `DOTNET_VERSION: '9.0.x'` - .NET version

To update versions, change these values at the workflow level.

## Caching Strategy

### npm Dependencies
```yaml
path: node_modules
key: ${{ runner.os }}-node-${{ hashFiles('package-lock.json') }}
```
Cache is invalidated when package-lock.json changes.

### .NET Dependencies
```yaml
path: ~/.nuget/packages
key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
```
Cache is invalidated when any .csproj file changes.

### Build Artifacts
- **API Builds**: Uploaded for 1 day, used by test jobs
- **Web Builds**: Uploaded for 1 day, used by E2E tests
- **Playwright Reports**: Uploaded for 7 days for debugging

## Code Coverage

Coverage reports are automatically uploaded to Codecov with flags:
- `api-unit`: API unit test coverage
- `api-integration`: API integration test coverage
- `web`: Web application coverage
- `cli`: CLI application coverage (if tests exist)

## Security Scanning

Trivy scans the repository for:
- Known vulnerabilities in dependencies
- Infrastructure as Code issues
- Misconfigurations
- Secrets in code

Results are uploaded to GitHub Security tab.

## Branch Protection

The `ci-success` job should be required for branch protection on `main` and `develop` branches. This ensures:
- All linting passes
- All tests pass
- All builds succeed
- Docker images build successfully
- Security scan completes

## Troubleshooting

### Workflow fails with "npm ci" error
**Cause**: package-lock.json out of sync with package.json  
**Solution**: Run `npm install` locally and commit the updated package-lock.json

### Build artifacts not found
**Cause**: Build job failed or artifact retention expired  
**Solution**: Check build job logs, artifacts expire after 1 day

### E2E tests timeout
**Cause**: API not starting or not responding  
**Solution**: Check "Wait for API" step logs, API has 60 second timeout

### Cached dependencies causing issues
**Cause**: Cache contains stale or corrupted files  
**Solution**: Change cache key or clear GitHub Actions cache via UI

### Docker build fails
**Cause**: Dockerfile issues or missing dependencies  
**Solution**: Test Docker build locally: `docker build -f docker/api.Dockerfile .`

## Local Development

To run the same checks locally before pushing:

```bash
# Install dependencies
npm ci

# Lint
npx nx lint web
npx nx lint cli
dotnet format apps/api/Hickory.Api.csproj --verify-no-changes

# Build
dotnet build apps/api/Hickory.Api.csproj --configuration Release
npx nx build web
npx nx build cli

# Test
dotnet test apps/api/Hickory.Api.Tests/Hickory.Api.Tests.csproj
dotnet test apps/api/Hickory.Api.IntegrationTests/Hickory.Api.IntegrationTests.csproj
npx nx test web
npx nx test cli
npx nx e2e web-e2e

# Security scan
docker run --rm -v $(pwd):/workspace aquasec/trivy fs /workspace
```

## Future Enhancements

Potential improvements for consideration:
1. Add performance testing job
2. Add deployment jobs for staging/production
3. Implement test result caching for unchanged code
4. Add automatic version bumping and changelog generation
5. Integrate additional security tools (SAST, dependency scanning)
6. Add notification hooks for Slack/Teams
7. Implement blue-green deployment strategy
8. Add automated rollback on deployment failure

## Workflow Visualization

```
trigger (push/PR)
    ↓
setup (cache dependencies)
    ↓
    ├─→ lint ────────────────┐
    ├─→ build-api ───────────┤
    ├─→ build-web ───────────┤
    └─→ build-cli ───────────┤
                              ↓
    ┌─────────────────────────┴─────────────┐
    ├─→ test-api-unit ────────────────────┐ │
    ├─→ test-api-integration ─────────────┤ │
    ├─→ test-web ─────────────────────────┤ │
    ├─→ test-cli ─────────────────────────┤ │
    ├─→ test-e2e ─────────────────────────┤ │
    ├─→ docker ───────────────────────────┤ │
    └─→ security ─────────────────────────┘ │
                              ↓               │
                        ci-success ◄──────────┘
```

## Questions or Issues?

For issues with the workflow:
1. Check this documentation first
2. Review the workflow file: `.github/workflows/ci.yml`
3. Check GitHub Actions logs for specific job failures
4. Create an issue with the `ci` label in the repository
