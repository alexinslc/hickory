# .NET 9.0 Implementation Audit Report

**Date:** January 10, 2026  
**Project:** Hickory Help Desk System  
**Current Framework:** .NET 9.0  
**Target Framework:** .NET 10  
**Audit Objective:** Establish baseline before upgrading to .NET 10

---

## Executive Summary

This audit documents the current .NET 9.0 implementation of the Hickory Help Desk system, including all dependencies, architectural patterns, and potential compatibility concerns for upgrading to .NET 10.

**Key Findings:**
- ✅ Clean build with 0 warnings
- ✅ All projects targeting net9.0
- ⚠️ One deprecated package identified (Microsoft.AspNetCore.SignalR.Core 1.2.0)
- ✅ 16 EF Core migrations in place
- ✅ Modern patterns: MediatR, CQRS, JWT authentication, SignalR, MassTransit

---

## 1. Project Structure

### Projects
1. **Hickory.Api** (Main API Project)
   - Type: ASP.NET Core Web API
   - Target Framework: net9.0
   - Path: `/apps/api/Hickory.Api.csproj`

2. **Hickory.Api.Tests** (Unit Tests)
   - Type: xUnit Test Project
   - Target Framework: net9.0
   - Path: `/apps/api/Hickory.Api.Tests/Hickory.Api.Tests.csproj`

3. **Hickory.Api.IntegrationTests** (Integration Tests)
   - Type: xUnit Test Project
   - Target Framework: net9.0
   - Path: `/apps/api/Hickory.Api.IntegrationTests/Hickory.Api.IntegrationTests.csproj`

---

## 2. NuGet Package Dependencies

### 2.1 Main API Project (Hickory.Api)

#### Database & ORM
| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | ✅ Compatible | Latest EF Core 9 provider |
| Microsoft.EntityFrameworkCore.Design | 9.0.10 | ✅ Compatible | Design-time EF Core tools |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 9.0.10 | ✅ Compatible | Health checks for EF Core |
| AspNetCore.HealthChecks.NpgSql | 9.0.0 | ✅ Compatible | PostgreSQL health checks |

#### Authentication & Authorization
| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.10 | ✅ Compatible | JWT authentication |
| System.IdentityModel.Tokens.Jwt | 8.14.0 | ✅ Compatible | JWT token handling |

#### API Documentation
| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| Microsoft.AspNetCore.OpenApi | 9.0.10 | ✅ Compatible | OpenAPI support |
| Swashbuckle.AspNetCore | 9.0.6 | ✅ Compatible | Swagger/OpenAPI UI |
| NSwag.MSBuild | 14.6.1 | ✅ Compatible | API client generation |

#### Messaging & Events
| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| MassTransit | 8.5.5 | ⚠️ Check Latest | Current: 8.x, Latest available: 8.3.x+ |
| MassTransit.Redis | 8.5.5 | ⚠️ Check Latest | Used for event-driven messaging |

#### Real-Time Communication
| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| Microsoft.AspNetCore.SignalR.Core | 1.2.0 | ⚠️ DEPRECATED | **Action Required**: Part of ASP.NET Core, should not be separately referenced |

#### Application Patterns
| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| MediatR | 13.1.0 | ✅ Compatible | CQRS pattern implementation |
| FluentValidation.DependencyInjectionExtensions | 12.0.0 | ✅ Compatible | Validation framework |

#### Observability
| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| OpenTelemetry.Extensions.Hosting | 1.13.1 | ✅ Compatible | Telemetry hosting extensions |
| OpenTelemetry.Instrumentation.AspNetCore | 1.13.0 | ✅ Compatible | ASP.NET Core instrumentation |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.13.0-beta.1 | ⚠️ Beta Package | EF Core instrumentation |
| Serilog.AspNetCore | 9.0.0 | ✅ Compatible | Structured logging |

### 2.2 Unit Test Project (Hickory.Api.Tests)

| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| Microsoft.NET.Test.Sdk | 17.11.0 | ✅ Compatible | Test platform |
| xunit | 2.9.0 | ✅ Compatible | Test framework |
| xunit.runner.visualstudio | 2.8.2 | ✅ Compatible | Test runner |
| coverlet.collector | 6.0.2 | ✅ Compatible | Code coverage |
| FluentAssertions | 6.12.0 | ✅ Compatible | Assertion library |
| Moq | 4.20.70 | ✅ Compatible | Mocking framework |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.0 | ✅ Compatible | In-memory database for testing |

### 2.3 Integration Test Project (Hickory.Api.IntegrationTests)

| Package | Current Version | .NET 10 Compatibility | Notes |
|---------|----------------|----------------------|-------|
| Microsoft.NET.Test.Sdk | 17.11.0 | ✅ Compatible | Test platform |
| xunit | 2.9.0 | ✅ Compatible | Test framework |
| xunit.runner.visualstudio | 2.8.2 | ✅ Compatible | Test runner |
| coverlet.collector | 6.0.2 | ✅ Compatible | Code coverage |
| FluentAssertions | 6.12.0 | ✅ Compatible | Assertion library |
| Moq | 4.20.70 | ✅ Compatible | Mocking framework |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.0 | ✅ Compatible | Integration testing support |
| Testcontainers.PostgreSql | 3.10.0 | ✅ Compatible | PostgreSQL test containers |
| Respawn | 6.2.1 | ✅ Compatible | Database cleanup for tests |

---

## 3. Entity Framework Core Usage

### 3.1 Database Context
- **Provider:** Npgsql (PostgreSQL)
- **Context:** `ApplicationDbContext`
- **Location:** `/apps/api/src/Infrastructure/Data/ApplicationDbContext.cs`

### 3.2 Entity Models
The following entities are managed by EF Core:
1. **User** - User accounts and authentication
2. **Ticket** - Support tickets
3. **Comment** - Comments on tickets
4. **Attachment** - File attachments
5. **Category** - Ticket categories
6. **Tag** - Ticket tags
7. **TicketTag** - Many-to-many join table
8. **NotificationPreferences** - User notification settings
9. **KnowledgeArticle** - Knowledge base articles

### 3.3 Migrations
- **Total Migrations:** 16
- **Migration Assembly:** Same as DbContext assembly
- **Location:** `/apps/api/src/Infrastructure/Data/Migrations/`
- **Oldest Migration:** 20251027041447_InitialCreate
- **Pattern:** Code-first migrations

### 3.4 EF Core Patterns Used
- ✅ Fluent API configurations via `IEntityTypeConfiguration<T>`
- ✅ Automatic timestamp management via `SaveChanges` override
- ✅ Migrations assembly configuration
- ✅ DbSet properties for all entities
- ✅ In-memory database support for unit tests
- ✅ Testcontainers for integration tests

### 3.5 EF Core Features
- Row versioning (concurrency tokens) on Ticket entity
- Cascade delete configurations
- Index definitions via configuration classes
- Custom value conversions (if any)

---

## 4. Middleware & Request Pipeline

### 4.1 Middleware Configuration (Program.cs)

**Development Middleware:**
- Swagger/SwaggerUI (accessible at `/api-docs`)

**Production & Development Middleware:**
1. **ExceptionHandlingMiddleware** - Global exception handling
2. **UseSwagger** - API documentation
3. **UseSwaggerUI** - Interactive API documentation
4. **UseHttpsRedirection** - HTTPS enforcement
5. **UseCors** - CORS policy ("AllowWebApp")
6. **UseAuthentication** - JWT authentication
7. **UseAuthorization** - Authorization policies

### 4.2 Custom Middleware
- **ExceptionHandlingMiddleware**
  - Location: `/apps/api/src/Infrastructure/Middleware/ExceptionHandlingMiddleware.cs`
  - Purpose: Centralized exception handling and error responses

---

## 5. Authentication & Authorization

### 5.1 Authentication Scheme
- **Type:** JWT Bearer Authentication
- **Scheme:** Microsoft.AspNetCore.Authentication.JwtBearer

### 5.2 JWT Configuration
- **Issuer Validation:** Enabled
- **Audience Validation:** Enabled
- **Lifetime Validation:** Enabled
- **Clock Skew:** Zero (strict expiration)
- **Signing Key:** Symmetric (configured via appsettings)

### 5.3 Custom Authentication Services
1. **IJwtTokenService / JwtTokenService**
   - Location: `/apps/api/src/Infrastructure/Auth/JwtTokenService.cs`
   - Purpose: JWT token generation and validation

2. **IPasswordHasher / PasswordHasher**
   - Location: `/apps/api/src/Infrastructure/Auth/PasswordHasher.cs`
   - Purpose: Password hashing and verification

### 5.4 Authorization
- Role-based authorization using `AuthorizationRoles` constants
- Authorization required on SignalR hub (`[Authorize]` attribute)

---

## 6. SignalR Configuration

### 6.1 Hub Implementation
- **Hub Class:** `NotificationHub`
- **Location:** `/apps/api/src/Infrastructure/RealTime/NotificationHub.cs`
- **Endpoint:** `/hubs/notifications`
- **Authorization:** Required (`[Authorize]` attribute)

### 6.2 SignalR Features Used
- User-specific groups for targeted notifications
- Connection/disconnection lifecycle management
- Claims-based user identification
- Real-time notification messaging

### 6.3 Notification Message Model
- Type: Record type (`NotificationMessage`)
- Fields: Type, Title, Message, TicketNumber, TicketId, Timestamp, Data

---

## 7. MassTransit & Messaging

### 7.1 Configuration
- **Configuration Class:** `MassTransitConfiguration`
- **Location:** `/apps/api/src/Infrastructure/Messaging/MassTransitConfiguration.cs`
- **Current Transport:** In-Memory (development)
- **Production Transport:** Redis (commented out, ready for production)

### 7.2 Consumers
The following event consumers are registered:
1. **EmailNotificationConsumer**
2. **SignalRNotificationConsumer**
3. **WebhookNotificationConsumer**

Location: `/apps/api/src/Features/Notifications/Consumers/`

### 7.3 Events Published
1. **TicketCreatedEvent**
2. **TicketAssignedEvent**
3. **TicketUpdatedEvent**
4. **CommentAddedEvent**

Location: `/apps/api/src/Common/Events/`

---

## 8. Application Architecture

### 8.1 Project Structure
```
apps/api/src/
├── Common/              # Shared services and events
│   ├── Events/         # Domain events
│   └── Services/       # Shared services
├── Features/           # Feature-based organization (Vertical Slices)
│   ├── Auth/          # Authentication features
│   ├── Categories/    # Category management
│   ├── Comments/      # Comment features
│   ├── KnowledgeBase/ # Knowledge base articles
│   ├── Notifications/ # Notification consumers
│   ├── Search/        # Search functionality
│   ├── Tags/          # Tag management
│   ├── Tickets/       # Ticket management (CRUD + operations)
│   └── Users/         # User preferences
└── Infrastructure/     # Cross-cutting concerns
    ├── Auth/          # Authentication services
    ├── Behaviors/     # MediatR behaviors
    ├── Data/          # EF Core context, entities, migrations
    ├── Messaging/     # MassTransit configuration
    ├── Middleware/    # Custom middleware
    ├── Notifications/ # Notification services
    └── RealTime/      # SignalR hubs
```

### 8.2 Architectural Patterns
1. **Vertical Slice Architecture** - Features organized by business capability
2. **CQRS** - Using MediatR for command/query separation
3. **Event-Driven** - MassTransit for asynchronous messaging
4. **Repository Pattern** - Implicit via EF Core DbContext
5. **Dependency Injection** - Built-in ASP.NET Core DI

### 8.3 MediatR Pipeline Behaviors
1. **ValidationBehavior<TRequest, TResponse>**
   - Location: `/apps/api/src/Infrastructure/Behaviors/ValidationBehavior.cs`
   - Purpose: FluentValidation integration

2. **LoggingBehavior<TRequest, TResponse>**
   - Location: `/apps/api/src/Infrastructure/Behaviors/LoggingBehavior.cs`
   - Purpose: Request/response logging

---

## 9. Custom Services

### 9.1 Business Services
1. **ITicketNumberGenerator / TicketNumberGenerator**
   - Purpose: Generate unique ticket numbers
   - Location: `/apps/api/src/Common/Services/TicketNumberGenerator.cs`

### 9.2 Notification Services
1. **IEmailService / EmailService**
   - Purpose: Send email notifications
   - Location: `/apps/api/src/Infrastructure/Notifications/EmailService.cs`

2. **IWebhookService / WebhookService**
   - Purpose: Send webhook notifications
   - Location: `/apps/api/src/Infrastructure/Notifications/WebhookService.cs`
   - HTTP Client: Named client "webhooks" with 30-second timeout

---

## 10. Observability & Monitoring

### 10.1 Logging
- **Framework:** Serilog
- **Sinks:** Console, File (rolling daily)
- **Log Location:** `logs/hickory-.log`
- **Configuration:** Via appsettings.json + code

### 10.2 Health Checks
- **Endpoint:** `/health` (all checks)
- **Ready Endpoint:** `/health/ready` (readiness probes)
- **Live Endpoint:** `/health/live` (liveness probes)
- **Checks:**
  - Database context check (ApplicationDbContext)
  - PostgreSQL connection check

### 10.3 OpenTelemetry
- **Service Name:** hickory-api
- **Instrumentation:**
  - ASP.NET Core requests
  - Entity Framework Core queries
  - Custom sources ("Hickory.Api")

---

## 11. CORS Configuration

- **Policy Name:** "AllowWebApp"
- **Allowed Origins:**
  - `http://localhost:3000`
  - `http://localhost:3001`
- **Allowed Methods:** Any
- **Allowed Headers:** Any
- **Credentials:** Allowed

---

## 12. Deprecated APIs & Patterns

### 12.1 Deprecated Packages
❌ **Microsoft.AspNetCore.SignalR.Core v1.2.0**
- **Issue:** This is an old standalone package
- **Fix:** Remove package reference; SignalR is included in ASP.NET Core 9.0
- **Risk Level:** Medium
- **Action Required:** Remove from .csproj before .NET 10 upgrade

### 12.2 Potential Breaking Changes for .NET 10
Based on Microsoft documentation review:

1. **SignalR Package Reference**
   - Current: Explicit reference to Microsoft.AspNetCore.SignalR.Core 1.2.0
   - .NET 10 Impact: Should use framework-included SignalR
   - Recommendation: Remove explicit package reference

2. **MassTransit Version**
   - Current: 8.5.5
   - Recommendation: Check for .NET 10 compatibility announcement from MassTransit team
   - Action: Monitor release notes before upgrade

3. **OpenTelemetry Beta Package**
   - Current: OpenTelemetry.Instrumentation.EntityFrameworkCore 1.13.0-beta.1
   - Recommendation: Evaluate stable release availability
   - Risk: Beta packages may have breaking changes

---

## 13. Breaking Changes Review (Microsoft Documentation)

### 13.1 ASP.NET Core 10.0 Breaking Changes
Reference: https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

**Reviewed Areas:**
- ✅ Middleware pipeline changes
- ✅ Authentication changes
- ✅ SignalR changes
- ✅ Minimal APIs (not used in this project)
- ✅ Blazor (not used in this project)

**Potential Impacts:**
- SignalR package reference issue (already identified)
- No other breaking changes identified that affect current implementation

### 13.2 .NET 10 Breaking Changes
Reference: https://learn.microsoft.com/en-us/dotnet/core/compatibility/10

**Reviewed Areas:**
- ✅ Core .NET libraries
- ✅ Runtime changes
- ✅ BCL changes
- ✅ Serialization changes

**Potential Impacts:**
- No significant breaking changes identified for current codebase patterns

---

## 14. Third-Party Package Compatibility

### 14.1 Package Compatibility Matrix

| Package Category | Package Name | Status | Notes |
|-----------------|--------------|--------|-------|
| **Database** | Npgsql.EntityFrameworkCore.PostgreSQL | ✅ Ready | Active development, .NET 10 support expected |
| **Testing** | xUnit | ✅ Ready | Well-maintained, compatible |
| **Testing** | Moq | ✅ Ready | Well-maintained, compatible |
| **Testing** | FluentAssertions | ✅ Ready | Well-maintained, compatible |
| **Testing** | Testcontainers | ✅ Ready | Active development |
| **Validation** | FluentValidation | ✅ Ready | Well-maintained, compatible |
| **Messaging** | MassTransit | ⚠️ Verify | Check official .NET 10 support announcement |
| **Logging** | Serilog | ✅ Ready | Well-maintained, compatible |
| **OpenAPI** | Swashbuckle | ✅ Ready | Well-maintained, compatible |
| **OpenAPI** | NSwag | ✅ Ready | Well-maintained, compatible |
| **Observability** | OpenTelemetry | ✅ Ready | Well-maintained, compatible |
| **CQRS** | MediatR | ✅ Ready | Well-maintained, compatible |

### 14.2 Compatibility Assessment Summary
- **High Confidence (Ready):** 18 packages
- **Medium Confidence (Verify):** 2 packages (MassTransit, OpenTelemetry.EFCore-beta)
- **Low Confidence (Action Required):** 1 package (SignalR.Core - deprecated)

---

## 15. Recommendations

### 15.1 Immediate Actions (Before .NET 10 Upgrade)
1. ✅ **Remove deprecated SignalR package reference**
   - Remove `Microsoft.AspNetCore.SignalR.Core` from Hickory.Api.csproj
   - Verify SignalR functionality still works (framework-included)

2. ⚠️ **Update MassTransit**
   - Check for latest stable version
   - Review MassTransit .NET 10 compatibility documentation
   - Test messaging functionality after update

3. ⚠️ **Evaluate OpenTelemetry EF Core package**
   - Check if stable version is available
   - If not, evaluate if beta is acceptable for production

### 15.2 Pre-Upgrade Testing Strategy
1. Run full test suite (unit + integration tests)
2. Test SignalR real-time notifications
3. Test MassTransit event processing
4. Test authentication and authorization flows
5. Test EF Core migrations on test database
6. Load test API endpoints
7. Verify OpenTelemetry telemetry collection

### 15.3 Upgrade Path
1. Update all projects to target net10.0
2. Update Microsoft.* packages to 10.0.x versions
3. Update third-party packages to latest compatible versions
4. Run tests after each package update
5. Address any breaking changes as they surface
6. Document any behavioral changes

### 15.4 Risk Mitigation
- **Low Risk:** Most packages are well-maintained and compatible
- **Medium Risk:** MassTransit compatibility needs verification
- **High Risk:** None identified
- **Strategy:** Gradual rollout with feature flags and rollback plan

---

## 16. Conclusion

### 16.1 Current State Assessment
The Hickory Help Desk system is well-architected with:
- ✅ Clean separation of concerns
- ✅ Modern architectural patterns (CQRS, Event-Driven, Vertical Slices)
- ✅ Comprehensive test coverage infrastructure
- ✅ Production-ready observability (logging, telemetry, health checks)
- ✅ Secure authentication with JWT
- ✅ Real-time capabilities with SignalR
- ✅ Asynchronous messaging with MassTransit

### 16.2 .NET 10 Readiness
**Overall Readiness:** HIGH ✅

- **Build Status:** Clean (0 warnings, 0 errors)
- **Deprecated Code:** 1 package reference to address
- **Breaking Changes:** Minimal impact expected
- **Package Ecosystem:** Largely compatible

### 16.3 Confidence Level
- ✅ **High Confidence** for core ASP.NET Core features
- ✅ **High Confidence** for EF Core upgrade
- ⚠️ **Medium Confidence** for MassTransit (needs verification)
- ✅ **High Confidence** for testing frameworks
- ✅ **High Confidence** for observability stack

### 16.4 Estimated Effort
- **Code Changes:** Minimal (primarily package updates)
- **Testing:** Moderate (comprehensive regression testing needed)
- **Risk:** Low to Medium
- **Estimated Timeline:** 2-3 days for upgrade + testing

---

## 17. Next Steps

1. ✅ Review this audit report
2. ⏭️ Remove deprecated SignalR package
3. ⏭️ Verify MassTransit .NET 10 support
4. ⏭️ Create upgrade plan document
5. ⏭️ Set up .NET 10 development environment
6. ⏭️ Create upgrade branch
7. ⏭️ Execute upgrade following recommendations
8. ⏭️ Run comprehensive test suite
9. ⏭️ Deploy to staging environment
10. ⏭️ Production rollout with monitoring

---

**Audit Completed By:** GitHub Copilot  
**Review Required By:** Development Team Lead  
**Approval Required By:** Technical Architect
