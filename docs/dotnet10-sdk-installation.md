# .NET 10 SDK Installation and Environment Configuration

## Summary
Updated all development environment configurations to use .NET 10 SDK instead of .NET 9.

## Changes Made

### 1. GitHub Actions CI/CD Pipeline (`.github/workflows/ci.yml`)
- ✅ Updated `DOTNET_VERSION` environment variable from `9.0.x` to `10.0.x`
- ✅ Updated `dotnet-ef` tool installation to use version `10.*`
- ✅ Updated runtime path from `net9.0` to `net10.0` in E2E test startup

### 2. DevContainer Configuration (`.devcontainer/Dockerfile`)
- ✅ Updated base image from `mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm` to `1-10.0-bookworm`
- Note: Image now uses Ubuntu-based .NET 10 (Microsoft's new default)

### 3. Docker Production Images (`docker/api.Dockerfile`)
- ✅ Updated SDK image from `mcr.microsoft.com/dotnet/sdk:9.0` to `sdk:10.0`
- ✅ Updated runtime image from `mcr.microsoft.com/dotnet/aspnet:9.0` to `aspnet:10.0`
- Note: These are now Ubuntu-based images (breaking change from Debian)

### 4. SDK Version Pinning (`global.json` - NEW)
- ✅ Created `global.json` to pin SDK version to 10.0.100
- Configured with `rollForward: latestMinor` for flexibility
- Prevents accidental builds with wrong SDK version

## Container Base Image Change

### ⚠️ Important: Ubuntu vs Debian
.NET 10 container images now default to **Ubuntu** instead of Debian. This affects:
- Package manager commands (`apt-get` still works)
- Available system packages
- Potential differences in file paths or system behavior

### What to Test
- [ ] Container builds successfully
- [ ] Health checks work
- [ ] Database connectivity
- [ ] All system dependencies are available
- [ ] File system paths work correctly

## Verification Steps

### Local Development
```bash
# Check SDK version
dotnet --version
# Should output: 10.0.xxx

# Verify global.json is respected
dotnet --list-sdks
```

### CI/CD Pipeline
- All jobs now use .NET 10 SDK via GitHub Actions
- Automated builds will verify compatibility
- E2E tests will run against .NET 10 runtime

### Docker Builds
```bash
# Test building the API container
docker build -f docker/api.Dockerfile -t hickory-api:net10 .

# Verify the runtime version
docker run --rm hickory-api:net10 dotnet --version
```

### DevContainer
- Open project in VS Code with Dev Containers
- Verify .NET 10 SDK is installed
- Test debugging and hot reload

## Next Steps

After this PR is merged:
1. ✅ Issue #100: Update project `.csproj` files to target `net10.0`
2. ✅ Issue #101: Update NuGet packages to version 10.x
3. ✅ Issue #102: Update third-party packages

## Manual Installation (Optional)

If team members need to install .NET 10 SDK locally:

### Windows
```powershell
winget install Microsoft.DotNet.SDK.10
```

### macOS
```bash
brew install --cask dotnet-sdk
# or download from: https://dotnet.microsoft.com/download/dotnet/10.0
```

### Linux (Ubuntu)
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
```

## Rollback Plan

If issues are discovered:
1. Revert CI/CD changes: Change `DOTNET_VERSION` back to `9.0.x`
2. Revert Docker images: Change `sdk:10.0` and `aspnet:10.0` back to `9.0`
3. Delete `global.json` file
4. Revert devcontainer: Change base image back to `9.0-bookworm`

## Notes

- ✅ No code changes required in this phase
- ✅ All changes are infrastructure/tooling only
- ⚠️ Projects still target `net9.0` until next issue
- ⚠️ Package versions still 9.x until next issue
- 🔄 Actual migration happens in subsequent issues

## References
- [.NET 10 SDK Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET 10 Container Images](https://hub.docker.com/_/microsoft-dotnet)
- [global.json overview](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)
