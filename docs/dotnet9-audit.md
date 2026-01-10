# .NET 9 Implementation Audit Report
**Date:** January 10, 2026  
**Prepared for:** .NET 10 Upgrade Planning  
**Current Framework:** .NET 9.0

## Executive Summary

This document provides a comprehensive audit of the Hickory Help Desk API's current .NET 9.0 implementation, including all dependencies, patterns, and potential compatibility issues for the planned upgrade to .NET 10.

## Project Structure

### Main Projects
- **Hickory.Api** - Main API project (ASP.NET Core Web API)
- **Hickory.Api.Tests** - Unit tests (xUnit)
- **Hickory.Api.IntegrationTests** - Integration tests with Testcontainers

### Target Framework
All projects currently target: `net9.0`

## NuGet Package Dependencies

### Main API Project (Hickory.Api.csproj)

#### Microsoft Packages
| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.10 | ✅ Update to 10.0.x | Core authentication |
| Microsoft.AspNetCore.OpenApi | 9.0.10 | ✅ Update to 10.0.x | OpenAPI generation |
| Microsoft.EntityFrameworkCore.Design | 9.0.10 | ✅ Update to 10.0.x | EF Core migrations |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 9.0.10 | ✅ Update to 10.0.x | Health checks |
| Microsoft.AspNetCore.SignalR.Core | 1.2.0 | ⚠️ Update to latest | Very old version - needs upgrade |

#### Database Packages
| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | ✅ Update to 10.x | PostgreSQL provider |
| AspNetCore.HealthChecks.NpgSql | 9.0.0 | ✅ Update to 10.x | Database health checks |

#### Messaging & Events
| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| MassTransit | 8.5.5 | ⚠️ Check compatibility | Event-driven messaging |
| MassTransit.Redis | 8.5.5 | ⚠️ Check compatibility | Redis transport |

#### CQRS & Validation
| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| MediatR | 13.1.0 | ✅ Latest version | CQRS pattern |
| FluentValidation.DependencyInjectionExtensions | 12.0.0 | ✅ Latest version | Request validation |

#### Observability
| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| OpenTelemetry.Extensions.Hosting | 1.13.1 | ✅ Check for updates | Telemetry |
| OpenTelemetry.Instrumentation.AspNetCore | 1.13.0 | ✅ Check for updates | ASP.NET Core tracing |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.13.0-beta.1 | ⚠️ Beta version | EF Core tracing |
| Serilog.AspNetCore | 9.0.0 | ✅ Check for updates | Structured logging |

#### API Documentation
| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| Swashbuckle.AspNetCore | 9.0.6 | ✅ Check for updates | Swagger UI |
| NSwag.MSBuild | 14.6.1 | ✅ Check for updates | OpenAPI client generation |

#### Security
| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| System.IdentityModel.Tokens.Jwt | 8.14.0 | ✅ Check for updates | JWT token handling |

### Unit Test Project (Hickory.Api.Tests)

| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| xunit | 2.9.0 | ✅ Latest | Testing framework |
| xunit.runner.visualstudio | 2.8.2 | ✅ Check for updates | Test runner |
| Microsoft.NET.Test.Sdk | 17.11.0 | ✅ Check for updates | Test SDK |
| Moq | 4.20.70 | ✅ Latest | Mocking framework |
| FluentAssertions | 6.12.0 | ✅ Latest | Assertion library |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.0 | ✅ Update to 10.0.x | In-memory database |
| coverlet.collector | 6.0.2 | ✅ Latest | Code coverage |

### Integration Test Project (Hickory.Api.IntegrationTests)

| Package | Current Version | .NET 10 Compatible | Notes |
|---------|----------------|-------------------|-------|
| Microsoft.AspNetCore.Mvc.Testing | 9.0.0 | ✅ Update to 10.0.x | WebApplicationFactory |
| Testcontainers.PostgreSql | 3.10.0 | ✅ Check for updates | Docker test containers |
| Respawn | 6.2.1 | ✅ Latest | Database cleanup |

## Architecture & Patterns

### Design Patterns in Use

#### CQRS with MediatR
- ✅ **Status:** Modern pattern, fully compatible
- **Location:** `src/Features/` - organized by feature
- **Implementation:** Request handlers with pipeline behaviors
- **Behaviors:**
  - ValidationBehavior (FluentValidation)
  - LoggingBehavior

#### Feature-Based Organization
- ✅ **Status:** Clean architecture approach
- **Structure:**
  - Comments/
  - Tickets/
  - Notifications/
- Each feature contains: Controllers, Handlers, Validators, DTOs

#### Repository Pattern with EF Core
- ✅ **Status:** Direct DbContext usage via dependency injection
- **Database:** PostgreSQL via Npgsql
- **Migrations:** Entity Framework Core migrations

#### Event-Driven Architecture
- ✅ **Status:** MassTransit with Redis transport
- **Usage:** 
  - Email notifications
  - SignalR real-time updates
  - Webhook notifications
- **Consumers:**
  - EmailNotificationConsumer
  - SignalRNotificationConsumer
  - WebhookNotificationConsumer

#### Real-Time Communication
- ⚠️ **Status:** SignalR Core 1.2.0 is very outdated
- **Implementation:** NotificationHub for real-time notifications
- **Integration:** Connected to MassTransit consumers
- **Action Required:** Upgrade to modern SignalR (bundled with ASP.NET Core)

### Middleware & Cross-Cutting Concerns

#### Authentication & Authorization
- **Pattern:** JWT Bearer authentication
- **Implementation:** Custom IJwtTokenService and IPasswordHasher
- **Configuration:** Token validation with issuer/audience validation
- **CORS:** Configured for localhost:3000 and localhost:3001

#### Logging
- **Framework:** Serilog
- **Sinks:** Console and File (rolling daily)
- **Context:** Enriched with LogContext

#### Observability
- **Telemetry:** OpenTelemetry with distributed tracing
- **Instrumentation:**
  - ASP.NET Core requests
  - Entity Framework Core queries
- **Service Name:** "hickory-api"

#### Health Checks
- **Database:** DbContext health check
- **PostgreSQL:** Direct Npgsql connection check
- **Endpoints:** 
  - `/health` - all checks
  - `/health/ready` - readiness probe
  - `/health/live` - liveness probe

#### API Documentation
- **Swagger UI:** Configured at `/api-docs`
- **Security:** JWT Bearer token support in Swagger
- **OpenAPI:** Version v1

### Service Architecture

#### Business Services
- `ITicketNumberGenerator` - Ticket number generation
- `IEmailService` - Email notifications
- `IWebhookService` - Webhook integrations

#### Infrastructure Services
- `IJwtTokenService` - JWT token generation/validation
- `IPasswordHasher` - Password hashing
- `ApplicationDbContext` - EF Core database context

## Potential Breaking Changes for .NET 10

### High Priority Issues

#### 1. SignalR Package Version
- **Current:** Microsoft.AspNetCore.SignalR.Core 1.2.0
- **Issue:** This is from .NET Core 2.x era
- **Action:** Remove explicit package reference; use built-in SignalR from ASP.NET Core 10
- **Impact:** HIGH - Core real-time functionality

#### 2. Cookie Authentication Redirects
- **Breaking Change:** .NET 10 disables automatic redirects for known API endpoints
- **Current Usage:** JWT Bearer only (not using cookie auth)
- **Action:** Verify no cookie authentication in use
- **Impact:** LOW - Not using this feature

#### 3. MassTransit Compatibility
- **Current:** Version 8.5.5
- **Issue:** Need to verify .NET 10 compatibility
- **Action:** Update to latest version compatible with .NET 10
- **Impact:** HIGH - Core messaging infrastructure

#### 4. OpenTelemetry Beta Package
- **Current:** OpenTelemetry.Instrumentation.EntityFrameworkCore 1.13.0-beta.1
- **Issue:** Using beta package
- **Action:** Check for stable release
- **Impact:** MEDIUM - Production readiness concern

### Medium Priority Issues

#### 5. OpenAPI 3.1 Support
- **Opportunity:** .NET 10 adds OpenAPI 3.1 and YAML support
- **Current:** OpenAPI 3.0 via Swashbuckle
- **Action:** Update to use new OpenAPI features
- **Impact:** MEDIUM - Enhanced API documentation

#### 6. W3C Trace Context
- **Change:** .NET 10 defaults to W3C trace context propagation
- **Current:** Using OpenTelemetry default propagation
- **Action:** Verify compatibility, update if needed
- **Impact:** MEDIUM - Distributed tracing

### Low Priority Issues

#### 7. Container Image Base
- **Change:** .NET 10 uses Ubuntu instead of Debian
- **Current:** Unknown (need to check Dockerfile)
- **Action:** Update Dockerfile and test
- **Impact:** LOW - Container infrastructure

## API Patterns & Features

### Controllers
- Feature-based organization
- Async/await throughout
- Dependency injection via constructor
- MediatR command/query pattern

### Validation
- FluentValidation for request validation
- ValidationBehavior pipeline in MediatR
- Automatic model validation

### Error Handling
- Custom ExceptionHandlingMiddleware
- Centralized error handling
- Structured error responses

### Database
- PostgreSQL with EF Core
- Code-first migrations
- Connection string from configuration

## Configuration

### AppSettings Structure
- `ConnectionStrings:DefaultConnection` - Database
- `JWT:Secret` - JWT signing key
- `JWT:Issuer` - Token issuer
- `JWT:Audience` - Token audience
- Serilog configuration

### Environment Support
- Development
- Production
- Separate appsettings files

## Testing Strategy

### Unit Tests
- xUnit framework
- Moq for mocking
- FluentAssertions for readable assertions
- In-memory database for data tests

### Integration Tests
- WebApplicationFactory for API testing
- Testcontainers for real PostgreSQL
- Respawn for database cleanup between tests
- Full HTTP request/response testing

## Recommendations for .NET 10 Upgrade

### Phase 1: Critical Updates
1. ✅ Remove Microsoft.AspNetCore.SignalR.Core package reference
2. ✅ Update all Microsoft.* packages to 10.0.x
3. ✅ Update MassTransit to latest stable version
4. ✅ Update Npgsql.EntityFrameworkCore.PostgreSQL to 10.x

### Phase 2: Infrastructure Updates
1. ✅ Update OpenTelemetry packages to stable versions
2. ✅ Update test packages to .NET 10 compatible versions
3. ✅ Update Docker base images to .NET 10 Ubuntu

### Phase 3: Feature Enhancements
1. ✅ Implement OpenAPI 3.1 features
2. ✅ Add new .NET 10 metrics and telemetry
3. ✅ Optimize with .NET 10 performance improvements
4. ✅ Adopt C# 14 language features

### Phase 4: Validation
1. ✅ Run full test suite
2. ✅ Performance benchmarking
3. ✅ Integration testing
4. ✅ Security scanning

## Deprecated API Usage

### To Investigate
- [ ] Check for any usage of IActionContextAccessor or ActionContextAccessor (marked obsolete)
- [ ] Check for WebHostBuilder usage (marked obsolete)
- [ ] Verify no Razor runtime compilation in use (marked obsolete)

## Security Considerations

### Current Security Features
- JWT Bearer authentication
- Password hashing (custom implementation)
- HTTPS redirection
- CORS configuration
- Health check endpoints (no sensitive data exposure)

### .NET 10 Security Enhancements
- Consider implementing passkey authentication support
- Review new authentication metrics
- Update to latest security patches

## Performance Baseline

### Areas to Benchmark
- Ticket creation endpoint
- Query endpoints (list, search)
- SignalR connection establishment
- Database query performance
- MassTransit message throughput

## Dependencies External to .NET

### Frontend Integration
- SignalR JavaScript client (`@microsoft/signalr: ^9.0.6`)
- Action: Update to 10.x compatible version

### Database
- PostgreSQL (version not specified in project)
- Action: Verify PostgreSQL version compatibility

### Infrastructure
- Redis (for MassTransit)
- Docker containers
- CI/CD pipelines

## Conclusion

The Hickory Help Desk API is well-structured and follows modern .NET patterns. The codebase is in good shape for upgrading to .NET 10, with the following key actions required:

1. **Critical:** Update SignalR package (remove explicit reference)
2. **Critical:** Update all framework packages from 9.x to 10.x
3. **Important:** Verify MassTransit .NET 10 compatibility
4. **Important:** Update test infrastructure
5. **Enhancement:** Leverage new .NET 10 features (OpenAPI 3.1, enhanced telemetry, C# 14)

Overall assessment: **LOW TO MEDIUM RISK** upgrade with significant benefits from .NET 10's performance and feature improvements.
