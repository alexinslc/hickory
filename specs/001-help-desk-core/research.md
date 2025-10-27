# Research: Hickory Help Desk System

**Feature**: 001-help-desk-core  
**Date**: October 26, 2025  
**Purpose**: Technology selection rationale and architectural decisions

## Executive Summary

This document captures the research and decision-making process for the Hickory Help Desk technical architecture. All technology choices prioritize simplicity, production-readiness, strong community support, and alignment with the constitutional principles of code quality, testing, user experience, and performance.

## Technology Stack Decisions

### 1. Monorepo Management: Nx

**Decision**: Use Nx for monorepo management

**Rationale**:
- **Code Sharing**: Enables sharing of TypeScript types generated from OpenAPI specs across web and CLI applications
- **Build Optimization**: Intelligent caching and incremental builds reduce CI/CD time
- **Developer Experience**: Integrated tooling for code generation, testing, and linting across all projects
- **Scalability**: Proven at enterprise scale with clear patterns for managing multiple applications
- **TypeScript First-Class**: Excellent TypeScript support aligns with type safety requirements

**Alternatives Considered**:
- **Turborepo**: Simpler but less feature-rich; lacks built-in code generators and scaffolding
- **Lerna**: Older tooling with less active development; Nx provides better modern DX
- **PNPM Workspaces**: Lower-level solution requiring more custom tooling setup

**References**:
- Nx Documentation: https://nx.dev
- Nx with ASP.NET Core: https://nx.dev/recipes/other/dotnet-core

---

### 2. Backend: ASP.NET Core 9 with Vertical Slice Architecture

**Decision**: ASP.NET Core 9 (C#) with vertical-slice architecture

**Rationale**:
- **Performance**: Top-tier performance in TechEmpower benchmarks, meets <200ms P95 latency requirement
- **Cross-Platform**: Runs on Linux containers, enables Docker-based deployment
- **Mature Ecosystem**: Rich libraries for authentication (OAuth2/OIDC), database access (EF Core), real-time (SignalR)
- **Vertical Slices**: Each feature is self-contained, improving maintainability and reducing coupling
- **Strong Typing**: C# nullable reference types prevent entire categories of null reference bugs

**Alternatives Considered**:
- **Node.js (Express/Fastify)**: Good TypeScript support but slower performance, less mature enterprise patterns
- **Spring Boot (Java)**: Excellent enterprise support but heavier runtime, more verbose than C#
- **Go**: Great performance but less mature ORM ecosystem, steeper learning curve for typical help desk features

**References**:
- ASP.NET Core Performance: https://www.techempower.com/benchmarks/
- Vertical Slice Architecture: https://www.jimmybogard.com/vertical-slice-architecture/

---

### 3. CQRS/Mediator: MediatR

**Decision**: MediatR for command/query handling

**Rationale**:
- **Simplicity**: Lightweight library, no infrastructure overhead like message queues for in-process patterns
- **Testability**: Easy to unit test handlers in isolation
- **Pipeline Behaviors**: Cross-cutting concerns (validation, logging, transactions) via pipeline behaviors
- **Vertical Slices**: Natural fit with vertical-slice architecture, each slice has its own handlers
- **Community**: De facto standard in .NET community with extensive examples

**Alternatives Considered**:
- **Direct Controller Methods**: Simpler but leads to fat controllers and poor separation of concerns
- **Service Layer Pattern**: More boilerplate, harder to test, less aligned with vertical slices
- **NServiceBus/Brighter**: Over-engineered for in-process use cases, designed for distributed systems

**References**:
- MediatR: https://github.com/jbogard/MediatR
- CQRS in ASP.NET Core: https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/apply-simplified-microservice-cqrs-ddd-patterns

---

### 4. Validation: FluentValidation

**Decision**: FluentValidation for input validation

**Rationale**:
- **Expressive**: Fluent API makes validation rules readable and maintainable
- **Separation**: Keeps validation logic out of domain models and controllers
- **Testable**: Validators are easily unit tested
- **Integration**: Works seamlessly with MediatR pipeline behaviors for automatic validation
- **Community**: Industry standard in .NET with excellent documentation

**Alternatives Considered**:
- **Data Annotations**: Clutters models, limited expressiveness for complex rules
- **Manual Validation**: Error-prone, inconsistent, hard to test
- **Custom Validators**: Reinventing the wheel, FluentValidation is battle-tested

**References**:
- FluentValidation: https://fluentvalidation.net
- MediatR + FluentValidation: https://github.com/FluentValidation/FluentValidation/wiki

---

### 5. Database: PostgreSQL 16+

**Decision**: PostgreSQL 16+ as primary database

**Rationale**:
- **Full-Text Search**: Built-in FTS with excellent performance for ticket search requirements
- **JSONB**: Native JSON support for flexible data storage (e.g., custom fields, metadata)
- **Performance**: Handles 1M+ records with proper indexing, meets <2 second search requirement
- **ACID**: Strong consistency guarantees for ticket operations
- **Open Source**: No licensing costs, mature ecosystem, excellent Docker support
- **EF Core Support**: First-class support in Entity Framework Core

**Alternatives Considered**:
- **SQL Server**: Excellent but proprietary licensing, overkill for open-source project
- **MySQL**: Lacks advanced FTS, weaker JSON support than PostgreSQL
- **MongoDB**: Document model doesn't fit relational ticket data, complicates transactions
- **SQLite**: Great for development but limited concurrent write support for production

**References**:
- PostgreSQL Full-Text Search: https://www.postgresql.org/docs/current/textsearch.html
- EF Core with PostgreSQL: https://www.npgsql.org/efcore/

---

### 6. ORM: Entity Framework Core 9

**Decision**: Entity Framework Core 9 for data access

**Rationale**:
- **Migrations**: Code-first migrations enable version-controlled schema evolution
- **LINQ**: Type-safe queries with compile-time checking
- **Performance**: Recent versions have addressed performance concerns with compiled queries
- **Change Tracking**: Optimistic concurrency support for conflict detection (constitutional requirement)
- **Interceptors**: Useful for auditing, soft deletes, and observability hooks

**Alternatives Considered**:
- **Dapper**: Faster but micro-ORM requires more manual SQL, loses type safety benefits
- **Raw SQL**: Maximum control but error-prone, loses LINQ expressiveness
- **Npgsql**: Lower-level but too much boilerplate for CRUD operations

**References**:
- EF Core Documentation: https://docs.microsoft.com/en-us/ef/core/
- EF Core Performance: https://docs.microsoft.com/en-us/ef/core/performance/

---

### 7. Messaging: MassTransit with RabbitMQ/Redis

**Decision**: MassTransit for event-driven architecture

**Rationale**:
- **Abstraction**: Transport-agnostic (RabbitMQ for production, Redis for simpler deployments)
- **Patterns**: Built-in support for sagas, outbox pattern, retry policies
- **Integration**: Works with ASP.NET Core, SignalR, and EF Core
- **Observability**: OpenTelemetry integration for distributed tracing
- **Community**: Active development, extensive documentation

**Alternatives Considered**:
- **Direct RabbitMQ**: More control but requires boilerplate for common patterns
- **Azure Service Bus**: Proprietary, not suitable for self-hosted deployments
- **Kafka**: Over-engineered for help desk scale, more complex operations

**References**:
- MassTransit: https://masstransit-project.com
- Outbox Pattern: https://masstransit-project.com/usage/transactional-outbox.html

---

### 8. Real-Time: SignalR

**Decision**: SignalR for real-time notifications

**Rationale**:
- **Native Integration**: Built into ASP.NET Core, zero additional infrastructure
- **Fallback**: Gracefully degrades from WebSockets to SSE to long-polling
- **Scale-Out**: Redis backplane enables horizontal scaling with 1,000+ connections per instance
- **Type Safety**: Strongly-typed hubs with client-side type generation
- **Performance**: Meets <1 second notification delivery requirement

**Alternatives Considered**:
- **WebSockets (raw)**: More control but requires handling reconnection, fallback logic
- **SSE (Server-Sent Events)**: Simpler but one-way only, no bidirectional communication
- **Polling**: Simple but inefficient, doesn't meet real-time requirements

**References**:
- SignalR Documentation: https://docs.microsoft.com/en-us/aspnet/core/signalr/
- SignalR Scale-Out: https://docs.microsoft.com/en-us/aspnet/core/signalr/scale

---

### 9. Authentication: OAuth 2.0/OIDC + Local Email/Password

**Decision**: Dual authentication modes (OAuth 2.0/OIDC for SSO, local email/password as fallback)

**Rationale**:
- **Flexibility**: Enterprises can integrate SSO, small teams can use local auth
- **Standards**: OAuth 2.0/OIDC are industry standards with mature libraries
- **ASP.NET Identity**: Built-in support for local auth with password hashing, 2FA support
- **Constitutional Alignment**: Enables "side-project to SaaS" scalability requirement

**Alternatives Considered**:
- **SSO Only**: Blocks small teams without identity provider
- **Local Only**: Enterprise users demand SSO integration
- **Custom Auth**: High risk, easy to get wrong, FluentValidation handles basics

**References**:
- ASP.NET Core Authentication: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/
- OIDC Documentation: https://openid.net/connect/

---

### 10. Observability: Serilog + OpenTelemetry + Health Checks

**Decision**: Serilog for structured logging, OpenTelemetry for tracing, built-in health checks

**Rationale**:
- **Structured Logging**: Serilog provides rich structured logs for debugging and auditing
- **Distributed Tracing**: OpenTelemetry enables request tracing across services
- **Health Checks**: ASP.NET Core built-in health checks for dependency monitoring
- **Standardization**: OpenTelemetry is vendor-neutral, supports multiple backends (Jaeger, Zipkin, etc.)
- **Constitutional Compliance**: Meets observability requirements for production-worthy architecture

**Alternatives Considered**:
- **Application Insights**: Proprietary, not suitable for self-hosted
- **ELK Stack**: Heavy infrastructure, overkill for initial deployments
- **Prometheus + Grafana**: Great for metrics but requires more setup

**References**:
- Serilog: https://serilog.net
- OpenTelemetry .NET: https://opentelemetry.io/docs/instrumentation/net/
- ASP.NET Health Checks: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks

---

### 11. Frontend: Next.js 15 with TypeScript

**Decision**: Next.js 15 (App Router) with TypeScript

**Rationale**:
- **Full-Stack**: Server components + API routes enable optimal performance
- **App Router**: File-based routing with layouts, loading states, error boundaries
- **Performance**: Automatic code splitting, image optimization, font optimization
- **SEO**: Server-side rendering for public-facing pages (knowledge base)
- **Developer Experience**: Fast Refresh, TypeScript support, excellent tooling

**Alternatives Considered**:
- **Create React App**: Deprecated, no SSR, slower build times
- **Vite + React Router**: Great DX but lacks Next.js App Router conveniences
- **Remix**: Excellent but smaller ecosystem than Next.js

**References**:
- Next.js Documentation: https://nextjs.org/docs
- App Router: https://nextjs.org/docs/app

---

### 12. UI Framework: TailwindCSS + ShadCN UI

**Decision**: TailwindCSS for styling, ShadCN UI for component primitives

**Rationale**:
- **Utility-First**: TailwindCSS enables rapid UI development without CSS files
- **Consistency**: ShadCN UI provides accessible, customizable component primitives
- **Type Safety**: Components are TypeScript-native with full prop typing
- **No Bundle Bloat**: ShadCN copies components into your codebase (no runtime library)
- **Accessibility**: Built on Radix UI primitives with ARIA compliance
- **Constitutional Compliance**: Meets accessibility and design system requirements

**Alternatives Considered**:
- **Material UI**: Heavy bundle, opinionated design harder to customize
- **Chakra UI**: Good accessibility but larger bundle than TailwindCSS
- **Ant Design**: Enterprise-focused, design doesn't fit "modern, minimal" goal

**References**:
- TailwindCSS: https://tailwindcss.com
- ShadCN UI: https://ui.shadcn.com
- Radix UI: https://www.radix-ui.com

---

### 13. State Management: TanStack Query (React Query)

**Decision**: TanStack Query for server state management

**Rationale**:
- **Caching**: Automatic request deduplication and intelligent caching
- **Optimistic Updates**: Seamless optimistic UI updates with rollback
- **Real-Time**: Integrates with SignalR for real-time data invalidation
- **DevTools**: Excellent debugging tools for inspecting cache state
- **Type Safety**: Full TypeScript support with generics for requests/responses
- **Constitutional Compliance**: Meets caching strategy requirements

**Alternatives Considered**:
- **Redux**: Over-engineered for server state, more boilerplate
- **Zustand**: Great for client state but lacks server state features
- **SWR**: Similar to React Query but smaller ecosystem and less feature-rich

**References**:
- TanStack Query: https://tanstack.com/query/latest
- TanStack Query with Next.js: https://tanstack.com/query/latest/docs/framework/react/guides/advanced-ssr

---

### 14. Type Safety: NSwag for OpenAPI Client Generation

**Decision**: NSwag to auto-generate TypeScript clients from OpenAPI specs

**Rationale**:
- **End-to-End Type Safety**: API changes automatically update TypeScript types
- **Single Source of Truth**: OpenAPI spec defines contracts for all clients
- **Zero Drift**: Impossible for frontend/CLI to use stale or incorrect types
- **CI Integration**: Client generation runs in build pipeline, catches breaking changes early
- **Constitutional Compliance**: Enforces type safety across full stack

**Alternatives Considered**:
- **Axios + Manual Types**: Error-prone, types drift from API reality
- **tRPC**: Requires Node.js backend, doesn't work with ASP.NET Core
- **GraphQL Code Generator**: Requires GraphQL, adds complexity vs REST

**References**:
- NSwag: https://github.com/RicoSuter/NSwag
- OpenAPI Specification: https://swagger.io/specification/

---

### 15. CLI: Node.js with Commander.js + Inquirer.js

**Decision**: Node.js CLI using Commander.js (command parsing) and Inquirer.js (interactive prompts)

**Rationale**:
- **Type Safety**: TypeScript CLI shares types with web app via NSwag-generated client
- **Cross-Platform**: Runs on Windows, macOS, Linux via Node.js
- **API Reuse**: Consumes same REST API as web app (consistency)
- **Rich UX**: Inquirer.js enables interactive prompts for better developer experience
- **Community**: Commander.js and Inquirer.js are industry standards

**Alternatives Considered**:
- **dotnet CLI Tool**: Would require duplicating API client logic
- **Python CLI**: Loses type safety, requires separate language ecosystem
- **Go CLI**: Fast but no type sharing with TypeScript frontend

**References**:
- Commander.js: https://github.com/tj/commander.js
- Inquirer.js: https://github.com/SBoudrias/Inquirer.js

---

### 16. Containerization: Docker + Docker Compose

**Decision**: Docker for containerization, Docker Compose for local development

**Rationale**:
- **Consistency**: Dev/prod parity ensures "works on my machine" issues are minimized
- **Isolation**: Each service (API, web, PostgreSQL, Redis) runs in its own container
- **Portability**: Deploy to any container-friendly platform (Kubernetes, Docker Swarm, cloud providers)
- **Developer Experience**: `docker-compose up` starts entire stack with zero manual configuration
- **Constitutional Compliance**: Meets <30 minute deployment requirement

**Alternatives Considered**:
- **Kubernetes Only**: Over-engineered for local development
- **Podman**: Docker-compatible but less ubiquitous
- **Native Deployment**: Complex setup, loses dev/prod parity

**References**:
- Docker Documentation: https://docs.docker.com
- Docker Compose: https://docs.docker.com/compose/

---

## Architecture Patterns

### Vertical Slice Architecture (Backend)

**Pattern**: Each feature is a self-contained vertical slice with its own models, validation, handlers, and tests.

**Benefits**:
- **High Cohesion**: Related code lives together, easier to understand and modify
- **Low Coupling**: Features are independent, reducing ripple effects of changes
- **Testability**: Each slice can be tested in isolation
- **Scalability**: Easy to split features into separate services if needed

**Structure Example**:
```
Features/
└── Tickets/
    ├── Create/
    │   ├── CreateTicketCommand.cs
    │   ├── CreateTicketValidator.cs
    │   ├── CreateTicketHandler.cs
    │   └── CreateTicketTests.cs
    ├── Update/
    ├── Delete/
    └── GetById/
```

**References**:
- Jimmy Bogard on Vertical Slices: https://www.jimmybogard.com/vertical-slice-architecture/

---

### Optimistic Concurrency (Constitutional Requirement)

**Pattern**: Use EF Core's row versioning to detect concurrent edits and notify users.

**Implementation**:
```csharp
public class Ticket
{
    public Guid Id { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; }
    // Other properties...
}
```

When a concurrent edit is detected, return a 409 Conflict status with a message prompting the user to refresh and retry.

**References**:
- EF Core Concurrency Tokens: https://docs.microsoft.com/en-us/ef/core/saving/concurrency

---

### Outbox Pattern (Reliable Messaging)

**Pattern**: Use MassTransit's outbox pattern to ensure events are published reliably.

**Benefits**:
- **Atomicity**: Database writes and message publishing happen in same transaction
- **Reliability**: Messages are guaranteed to be published even if initial send fails
- **Idempotency**: Prevents duplicate events on retries

**References**:
- MassTransit Outbox: https://masstransit-project.com/usage/transactional-outbox.html

---

## Risk Mitigation

### Risk 1: Performance Degradation Under Load

**Mitigation**:
- **Indexing Strategy**: All database queries analyzed during development, indexes added proactively
- **Caching**: TanStack Query (frontend), response caching (backend), Redis (distributed cache)
- **Load Testing**: Implement k6 or Artillery tests in CI to catch performance regressions
- **Monitoring**: OpenTelemetry traces identify slow endpoints before production impact

---

### Risk 2: Type Drift Between API and Clients

**Mitigation**:
- **NSwag Code Generation**: Automated in CI pipeline, fails build if types are out of sync
- **Contract Tests**: Backend tests validate OpenAPI spec matches actual endpoint behavior
- **Versioning**: API versioning strategy (URL-based: `/api/v1/`) enables non-breaking changes

---

### Risk 3: Real-Time Scalability

**Mitigation**:
- **Redis Backplane**: SignalR scale-out via Redis enables horizontal scaling
- **Connection Limits**: Monitor and alert on connection counts approaching instance limits
- **Graceful Degradation**: If real-time fails, fall back to polling with exponential backoff

---

### Risk 4: Security Vulnerabilities

**Mitigation**:
- **Dependency Scanning**: Dependabot for automated vulnerability alerts
- **Authentication**: Use proven libraries (ASP.NET Identity, OAuth 2.0)
- **Input Validation**: FluentValidation catches malicious input before reaching handlers
- **OWASP Top 10**: Regular security reviews against OWASP guidelines

---

## Open Questions (Resolved in spec.md)

All technical unknowns have been resolved through the clarification phase:
- ✅ SLA targets: Admin-configurable per deployment
- ✅ Attachment limits: 10MB per file
- ✅ Authentication: OAuth 2.0/OIDC + local email/password
- ✅ Observability: Structured logging + health checks + basic metrics
- ✅ Concurrency: Optimistic locking with conflict detection

---

## Next Steps

1. ✅ Phase 0 Complete: Research documented
2. 🔄 Phase 1: Generate data-model.md
3. 🔄 Phase 1: Generate API contracts (OpenAPI spec)
4. 🔄 Phase 1: Generate quickstart.md
5. 🔄 Phase 1: Update agent context with technology stack
6. ⏸️ Phase 2: Generate tasks.md (via `/speckit.tasks` command)

---

**Research Complete**: October 26, 2025  
**Next Command**: Continue Phase 1 (data model, contracts, quickstart)
