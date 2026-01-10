# Update Project Target Frameworks to .NET 10

## Summary
Updated all project files to target .NET 10.0 framework, completing the core framework migration.

## Changes Made

### Project Files Updated

#### 1. Hickory.Api.csproj (Main API)
```xml
<TargetFramework>net9.0</TargetFramework>  →  <TargetFramework>net10.0</TargetFramework>
```

#### 2. Hickory.Api.Tests.csproj (Unit Tests)
```xml
<TargetFramework>net9.0</TargetFramework>  →  <TargetFramework>net10.0</TargetFramework>
```

#### 3. Hickory.Api.IntegrationTests.csproj (Integration Tests)
```xml
<TargetFramework>net9.0</TargetFramework>  →  <TargetFramework>net10.0</TargetFramework>
```

## Build Requirements

### ⚠️ Important: .NET 10 SDK Required
To build these projects, you must have .NET 10 SDK installed:

```bash
# Verify .NET 10 SDK is installed
dotnet --list-sdks

# Should show: 10.0.xxx
```

### Installing .NET 10 SDK

**macOS:**
```bash
brew install --cask dotnet-sdk
# or download from: https://dotnet.microsoft.com/download/dotnet/10.0
```

**Windows:**
```powershell
winget install Microsoft.DotNet.SDK.10
```

**Linux (Ubuntu):**
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
```

## Clean Build Steps

After pulling this PR, perform a clean build:

```bash
# Navigate to API directory
cd apps/api

# Clean previous build artifacts
rm -rf bin obj */bin */obj

# Restore NuGet packages
dotnet restore Hickory.Api.csproj

# Build the solution
dotnet build Hickory.Api.csproj --configuration Release
```

## What This Changes

### ✅ Changes Made
- Target framework for all projects: net9.0 → net10.0
- Build output directory: bin/Release/net9.0 → bin/Release/net10.0
- Runtime identifier now targets .NET 10

### ❌ NOT Changed (Yet)
- NuGet package versions still 9.x (next issue)
- SignalR package still 1.2.0 (will be addressed)
- No code changes required
- Application behavior unchanged

## Compatibility

### Package Versions
Current NuGet packages (version 9.x) are **compatible** with .NET 10 target framework. This is intentional:

1. .NET supports forward compatibility
2. Allows us to verify framework change independently
3. Package updates happen in next issue (#101)

### Known Issues
- ⚠️ **SignalR.Core 1.2.0** will show warnings - this is expected and will be fixed in issue #103
- Some packages may emit warnings about preferring 10.x versions - this is normal

## Testing

### CI/CD Pipeline
GitHub Actions will automatically:
1. Use .NET 10 SDK (configured in previous issue)
2. Build all projects targeting net10.0
3. Run all unit tests
4. Run all integration tests
5. Build Docker images with .NET 10 runtime

### Local Testing

**Build:**
```bash
dotnet build apps/api/Hickory.Api.csproj
```

**Run Tests:**
```bash
dotnet test apps/api/Hickory.Api.Tests/Hickory.Api.Tests.csproj
dotnet test apps/api/Hickory.Api.IntegrationTests/Hickory.Api.IntegrationTests.csproj
```

**Run API:**
```bash
cd apps/api
dotnet run
```

## Rollback Plan

If issues are found:

```bash
# Revert target framework changes
git checkout main -- apps/api/Hickory.Api.csproj
git checkout main -- apps/api/Hickory.Api.Tests/Hickory.Api.Tests.csproj
git checkout main -- apps/api/Hickory.Api.IntegrationTests/Hickory.Api.IntegrationTests.csproj

# Clean and rebuild
cd apps/api
rm -rf bin obj */bin */obj
dotnet build
```

## Next Steps

After this PR is merged:

1. **Issue #101**: Update Microsoft NuGet packages from 9.x to 10.x
   - Microsoft.AspNetCore.* packages
   - Microsoft.EntityFrameworkCore.* packages
   - Microsoft.Extensions.* packages

2. **Issue #102**: Update third-party packages
   - MassTransit to latest version
   - Npgsql.EntityFrameworkCore.PostgreSQL to 10.x
   - OpenTelemetry packages
   - Test packages

3. **Issue #103**: Remove SignalR.Core 1.2.0 package
   - This old package will be replaced by built-in SignalR

## Breaking Changes

### Expected Warnings
You may see build warnings like:
- "Package 'Microsoft.AspNetCore.X' 9.0.10 is not compatible with net10.0"
- These are **informational only** and don't prevent building
- Will be resolved in issue #101

### No Code Changes Required
The .NET 10 framework is highly compatible with .NET 9 code. No source code modifications are required for this change.

## Verification

### Success Criteria
- ✅ All three .csproj files target net10.0
- ✅ Solution restores without errors (may have warnings)
- ✅ Solution builds successfully
- ✅ All tests pass
- ✅ CI/CD pipeline completes successfully

### Post-Merge Verification
```bash
# Check target frameworks
grep -r "TargetFramework" apps/api/*.csproj apps/api/*/*.csproj

# Should output:
# <TargetFramework>net10.0</TargetFramework>
```

## Notes

- This is a **prerequisite** for updating package versions
- Projects will build with mixed versions (net10.0 + packages 9.x)
- This is safe and intentional
- Allows incremental migration with verification at each step

## References
- [Target frameworks in .NET](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [.NET 10 what's new](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview)
- [Migration guide](https://learn.microsoft.com/en-us/aspnet/core/migration/90-to-100)
