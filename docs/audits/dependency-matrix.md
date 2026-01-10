# Dependency Matrix - Hickory Help Desk

**Generated:** January 10, 2026  
**Framework:** .NET 9.0  
**Target:** .NET 10 Upgrade Preparation

---

## Main API Project: Hickory.Api

| Package Name | Current Version | Category | License | .NET 10 Ready | Action Required | Priority |
|--------------|----------------|----------|---------|---------------|-----------------|----------|
| AspNetCore.HealthChecks.NpgSql | 9.0.0 | Health Checks | MIT | ✅ Yes | None | Low |
| FluentValidation.DependencyInjectionExtensions | 12.0.0 | Validation | Apache-2.0 | ✅ Yes | None | Low |
| MassTransit | 8.5.5 | Messaging | Apache-2.0 | ⚠️ Verify | Check .NET 10 support | Medium |
| MassTransit.Redis | 8.5.5 | Messaging | Apache-2.0 | ⚠️ Verify | Check .NET 10 support | Medium |
| MediatR | 13.1.0 | CQRS | Apache-2.0 | ✅ Yes | None | Low |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.10 | Authentication | MIT | ✅ Yes | Update to 10.0.x | High |
| Microsoft.AspNetCore.OpenApi | 9.0.10 | API Docs | MIT | ✅ Yes | Update to 10.0.x | High |
| Microsoft.AspNetCore.SignalR.Core | 1.2.0 | Real-Time | MIT | ❌ No | **REMOVE** - Use framework version | High |
| Microsoft.EntityFrameworkCore.Design | 9.0.10 | ORM | MIT | ✅ Yes | Update to 10.0.x | High |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 9.0.10 | Health Checks | MIT | ✅ Yes | Update to 10.0.x | High |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | Database | PostgreSQL | ✅ Yes | Update to 10.0.x when available | High |
| NSwag.MSBuild | 14.6.1 | API Docs | MIT | ✅ Yes | None | Low |
| OpenTelemetry.Extensions.Hosting | 1.13.1 | Observability | Apache-2.0 | ✅ Yes | Check for updates | Low |
| OpenTelemetry.Instrumentation.AspNetCore | 1.13.0 | Observability | Apache-2.0 | ✅ Yes | Check for updates | Low |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.13.0-beta.1 | Observability | Apache-2.0 | ⚠️ Beta | Check for stable release | Medium |
| Serilog.AspNetCore | 9.0.0 | Logging | Apache-2.0 | ✅ Yes | None | Low |
| Swashbuckle.AspNetCore | 9.0.6 | API Docs | MIT | ✅ Yes | None | Low |
| System.IdentityModel.Tokens.Jwt | 8.14.0 | Authentication | MIT | ✅ Yes | None | Low |

**Summary:**
- Total Packages: 18
- Ready for .NET 10: 14 (78%)
- Need Verification: 3 (17%)
- Action Required: 1 (6%)

---

## Unit Test Project: Hickory.Api.Tests

| Package Name | Current Version | Category | License | .NET 10 Ready | Action Required | Priority |
|--------------|----------------|----------|---------|---------------|-----------------|----------|
| coverlet.collector | 6.0.2 | Testing | MIT | ✅ Yes | None | Low |
| FluentAssertions | 6.12.0 | Testing | Apache-2.0 | ✅ Yes | None | Low |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.0 | Testing | MIT | ✅ Yes | Update to 10.0.x | Medium |
| Microsoft.NET.Test.Sdk | 17.11.0 | Testing | MIT | ✅ Yes | None | Low |
| Moq | 4.20.70 | Testing | BSD-3-Clause | ✅ Yes | None | Low |
| xunit | 2.9.0 | Testing | Apache-2.0 | ✅ Yes | None | Low |
| xunit.runner.visualstudio | 2.8.2 | Testing | Apache-2.0 | ✅ Yes | None | Low |

**Summary:**
- Total Packages: 7
- Ready for .NET 10: 7 (100%)
- Need Verification: 0 (0%)
- Action Required: 0 (0%)

---

## Integration Test Project: Hickory.Api.IntegrationTests

| Package Name | Current Version | Category | License | .NET 10 Ready | Action Required | Priority |
|--------------|----------------|----------|---------|---------------|-----------------|----------|
| coverlet.collector | 6.0.2 | Testing | MIT | ✅ Yes | None | Low |
| FluentAssertions | 6.12.0 | Testing | Apache-2.0 | ✅ Yes | None | Low |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.0 | Testing | MIT | ✅ Yes | Update to 10.0.x | Medium |
| Microsoft.NET.Test.Sdk | 17.11.0 | Testing | MIT | ✅ Yes | None | Low |
| Moq | 4.20.70 | Testing | BSD-3-Clause | ✅ Yes | None | Low |
| Respawn | 6.2.1 | Testing | Apache-2.0 | ✅ Yes | None | Low |
| Testcontainers.PostgreSql | 3.10.0 | Testing | MIT | ✅ Yes | None | Low |
| xunit | 2.9.0 | Testing | Apache-2.0 | ✅ Yes | None | Low |
| xunit.runner.visualstudio | 2.8.2 | Testing | Apache-2.0 | ✅ Yes | None | Low |

**Summary:**
- Total Packages: 9
- Ready for .NET 10: 9 (100%)
- Need Verification: 0 (0%)
- Action Required: 0 (0%)

---

## Overall Dependency Statistics

### By Project
- **Main API:** 18 packages
- **Unit Tests:** 7 packages
- **Integration Tests:** 9 packages
- **Total Unique:** 22 packages (accounting for shared dependencies)

### By Category
- **Database & ORM:** 4 packages
- **Authentication:** 2 packages
- **API Documentation:** 3 packages
- **Messaging:** 2 packages
- **Real-Time:** 1 package
- **CQRS/Validation:** 2 packages
- **Observability:** 4 packages
- **Testing:** 9 packages

### By Readiness Status
- ✅ **Ready (No Action):** 29 (85%)
- ⚠️ **Needs Verification:** 4 (12%)
- ❌ **Action Required:** 1 (3%)

### By Priority
- **High Priority:** 6 packages (framework updates + deprecated package)
- **Medium Priority:** 5 packages (verification needed)
- **Low Priority:** 23 packages (stable, no action needed)

---

## Dependency Graph

### Core Dependencies
```
Hickory.Api
├── ASP.NET Core 9.0 Framework
│   ├── Authentication (JwtBearer)
│   ├── OpenApi
│   └── SignalR (Framework-included) ⚠️ Currently using deprecated package
├── Entity Framework Core 9.0
│   ├── Npgsql Provider
│   └── Design Tools
├── MediatR 13.1.0
│   └── FluentValidation 12.0.0
├── MassTransit 8.5.5
│   └── MassTransit.Redis 8.5.5
├── OpenTelemetry 1.13.x
│   ├── AspNetCore Instrumentation
│   ├── EF Core Instrumentation (Beta)
│   └── Hosting Extensions
└── Serilog 9.0.0
```

### Testing Dependencies
```
Test Projects
├── xUnit 2.9.0
│   └── xUnit Runner 2.8.2
├── FluentAssertions 6.12.0
├── Moq 4.20.70
├── Testcontainers 3.10.0
├── Respawn 6.2.1
└── Microsoft Test SDK 17.11.0
    └── Coverlet Collector 6.0.2
```

---

## Critical Dependencies Analysis

### High Impact Packages (Breaking changes would affect core functionality)
1. **Microsoft.AspNetCore.* (9.0.10)**
   - Impact: Core framework, authentication, API functionality
   - Risk: Low (Microsoft provides clear upgrade path)
   - Action: Upgrade to 10.0.x versions

2. **Entity Framework Core (9.0.x)**
   - Impact: All database operations, migrations
   - Risk: Low (Microsoft provides migration guide)
   - Action: Upgrade to 10.0.x versions

3. **MassTransit (8.5.5)**
   - Impact: Event-driven messaging, async operations
   - Risk: Medium (third-party, needs verification)
   - Action: Check .NET 10 compatibility, test thoroughly

### Medium Impact Packages (Breaking changes would affect specific features)
1. **MediatR (13.1.0)**
   - Impact: CQRS pattern, all commands/queries
   - Risk: Low (stable, well-maintained)
   - Action: Monitor for updates

2. **SignalR (currently 1.2.0 - deprecated)**
   - Impact: Real-time notifications
   - Risk: Medium (needs package removal)
   - Action: Remove package, use framework version

### Low Impact Packages (Breaking changes would be isolated)
1. **Serilog (9.0.0)**
   - Impact: Logging only
   - Risk: Very Low
   - Action: None required

2. **OpenTelemetry (1.13.x)**
   - Impact: Telemetry only
   - Risk: Low
   - Action: Monitor for updates

---

## Recommended Upgrade Sequence

### Phase 1: Pre-Upgrade Cleanup (Before .NET 10)
1. Remove Microsoft.AspNetCore.SignalR.Core package reference
2. Test SignalR functionality with framework-included version
3. Update MassTransit to latest 8.x version if available
4. Check OpenTelemetry.Instrumentation.EntityFrameworkCore for stable release

### Phase 2: Framework Upgrade (Day 1)
1. Update TargetFramework to net10.0 in all .csproj files
2. Update all Microsoft.* packages to 10.0.x versions
3. Update Npgsql.EntityFrameworkCore.PostgreSQL to 10.0.x (when available)
4. Build and fix any immediate compilation errors

### Phase 3: Third-Party Package Updates (Day 2)
1. Update MassTransit and verify messaging works
2. Update OpenTelemetry packages
3. Update test packages if needed
4. Build and fix any remaining compilation errors

### Phase 4: Testing & Validation (Day 2-3)
1. Run unit test suite
2. Run integration test suite
3. Manual testing of critical paths
4. Performance testing
5. Security testing

---

## Package Update Commands

### Main API Project
```bash
cd apps/api

# Remove deprecated package
dotnet remove package Microsoft.AspNetCore.SignalR.Core

# Update Microsoft packages (after .NET 10 release)
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.0
dotnet add package Microsoft.AspNetCore.OpenApi --version 10.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.0
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore --version 10.0.0

# Update Npgsql (when 10.0 version is available)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0

# Check and update MassTransit if newer version available
dotnet list package --outdated
```

### Test Projects
```bash
# Update test packages in unit test project
cd apps/api/Hickory.Api.Tests
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 10.0.0

# Update test packages in integration test project
cd ../Hickory.Api.IntegrationTests
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.0.0
```

---

## Security Considerations

### Packages with Security Implications
1. **Microsoft.AspNetCore.Authentication.JwtBearer**
   - Security Impact: High
   - Recommendation: Update immediately to 10.0.x for security patches

2. **System.IdentityModel.Tokens.Jwt**
   - Security Impact: High
   - Recommendation: Check for updates, apply security patches

3. **Npgsql.EntityFrameworkCore.PostgreSQL**
   - Security Impact: Medium (SQL injection prevention)
   - Recommendation: Update to 10.0.x when available

### Security Scanning
- Run `dotnet list package --vulnerable` before and after upgrade
- Review CVE databases for known vulnerabilities
- Update any packages with known security issues

---

## Monitoring & Rollback Plan

### Metrics to Monitor Post-Upgrade
1. Application startup time
2. Request latency (P50, P95, P99)
3. Database query performance
4. SignalR connection success rate
5. MassTransit message processing rate
6. Error rates and exceptions
7. Memory usage
8. CPU usage

### Rollback Criteria
- Critical functionality broken (auth, ticket creation, etc.)
- Performance degradation >20%
- Security vulnerabilities introduced
- Integration test failure rate >10%

### Rollback Process
1. Revert to previous .NET 9.0 branch
2. Deploy previous container image
3. Verify system stability
4. Document issues encountered
5. Plan remediation

---

**Document Owner:** Development Team  
**Last Updated:** January 10, 2026  
**Next Review:** After .NET 10 upgrade completion
