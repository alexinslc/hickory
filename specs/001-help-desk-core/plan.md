# Implementation Plan: Hickory Help Desk System

**Branch**: `001-help-desk-core` | **Date**: October 26, 2025 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/001-help-desk-core/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Hickory is a modern, minimal, and fast open-source help desk for IT/DevOps/Software teams built as a full-stack application in an Nx monorepo. The system enables users to submit and track support tickets, allows agents to manage and respond to tickets efficiently, and provides organization features like categories, tags, search, and real-time notifications. The backend uses ASP.NET Core 9 with vertical-slice architecture and MediatR for CQRS patterns, PostgreSQL for data persistence with full-text search, MassTransit for event-driven architecture, and SignalR for real-time updates. The frontend uses Next.js 15 with TypeScript, TailwindCSS, and ShadCN UI for the web interface, with NSwag auto-generating TypeScript types from OpenAPI specs for end-to-end type safety. A Node.js CLI provides command-line access. The entire stack runs in Docker containers with Docker Compose for local development.

## Technical Context

**Language/Version**: 
- Backend: C# 13 / ASP.NET Core 9
- Frontend: TypeScript 5.x / Next.js 15
- CLI: TypeScript 5.x / Node.js 20+

**Primary Dependencies**:
- Backend: MediatR, FluentValidation, Entity Framework Core 9, MassTransit, SignalR, Serilog, OpenTelemetry, Swashbuckle (OpenAPI/Swagger)
- Frontend: React 19, TanStack Query (React Query), TailwindCSS, ShadCN UI, NSwag (client generation)
- CLI: Commander.js, Inquirer.js
- Infrastructure: Docker, Docker Compose, Nx (monorepo tooling)

**Storage**: 
- Database: PostgreSQL 16+ (full-text search, JSONB support)
- File Storage: Local filesystem or object storage (S3-compatible) for attachments
- Cache: Redis (optional, for MassTransit and distributed caching)

**Testing**: 
- Backend: xUnit, FluentAssertions, Testcontainers, NSubstitute
- Frontend: Vitest, React Testing Library, Playwright (E2E)
- CLI: Vitest
- Contract Testing: NSwag-generated clients validated against OpenAPI spec

**Target Platform**: 
- Deployment: Docker containers (Linux), Kubernetes-ready
- Development: Cross-platform (Windows, macOS, Linux) via Docker Compose
- Browser Support: Modern browsers (last 2 versions of Chrome, Firefox, Safari, Edge)

**Project Type**: Full-stack web application (monorepo with backend API, frontend web app, CLI tool)

**Performance Goals**: 
- API: P95 latency <200ms for GET requests, <500ms for POST/PUT/DELETE under 100 concurrent users
- Search: Results within 2 seconds for queries across 100,000+ tickets
- UI: Initial page load <2 seconds on 3G networks, Core Web Vitals meet "Good" thresholds
- Real-time: SignalR updates delivered within 1 second of server event
- Scalability: Support 10,000 concurrent users, 1 million tickets

**Constraints**: 
- Backend containers: <512MB RAM under normal load
- Attachment size: 10MB per file
- Database queries: All queries must use indexes, no N+1 patterns
- Type safety: Zero tolerance for TypeScript `any` types or C# nullable warnings without justification
- Ticket reopening: Limited to 30 days after closure

**Scale/Scope**: 
- User base: 10 to 1,000 users per deployment
- Ticket volume: Up to 1 million tickets with maintained performance
- Concurrent connections: 1,000+ concurrent SignalR connections per instance
- Deployment scenarios: Side-project (single container) to SaaS (multi-tenant, scaled horizontally)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Code Quality Standards ✅

- **Type Safety**: C# nullable reference types enabled, TypeScript strict mode enabled
- **Linting & Formatting**: dotnet format + StyleCop for C#, ESLint + Prettier for TypeScript
- **Code Review**: Required for all PRs with vertical-slice architecture (backend) and component composition (frontend) verification
- **Documentation**: JSDoc/TSDoc for TypeScript, XML comments for C# public APIs
- **Dependency Management**: All specified dependencies (MediatR, FluentValidation, TanStack Query, etc.) are actively maintained and widely adopted

**Status**: PASS - All dependencies justified, type safety enforced, quality tooling specified

### II. Testing Standards (NON-NEGOTIABLE) ✅

- **Red-Green-Refactor**: Test-first development required per constitution
- **Unit Tests**: xUnit (backend), Vitest (frontend/CLI) for business logic
- **Integration Tests**: Testcontainers for database integration, API contract tests
- **Contract Tests**: NSwag-generated clients validated against OpenAPI spec ensures type safety
- **CI Gates**: All tests must pass before merge

**Status**: PASS - Comprehensive testing strategy with contract testing for API type safety

### III. User Experience Consistency ✅

- **Design System**: ShadCN UI with TailwindCSS specified
- **Accessibility**: WCAG 2.1 AA compliance required (will be verified in implementation)
- **Responsive Design**: Modern browser support specified
- **Performance Budget**: <2 second load target aligns with constitution
- **Error Handling**: User-focused error messages required
- **Real-time Feedback**: SignalR for <1 second notification delivery

**Status**: PASS - Design system and performance requirements align with constitution

### IV. Performance Requirements ✅

- **API Response Times**: P95 <200ms (GET), <500ms (POST/PUT/DELETE) - meets constitution
- **Database Queries**: Index requirements specified, N+1 prevention required
- **Caching Strategy**: TanStack Query for frontend caching, response caching for backend
- **Real-time Scalability**: 1,000+ concurrent SignalR connections specified
- **Resource Limits**: <512MB RAM per container aligns with constitution
- **Observability**: Serilog, OpenTelemetry, health checks specified

**Status**: PASS - All performance requirements meet or exceed constitutional standards

### Gate Result: ✅ PASS

All constitutional principles satisfied. Proceed to Phase 0 research.

## Project Structure

### Documentation (this feature)

```text
specs/001-help-desk-core/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (to be generated)
├── data-model.md        # Phase 1 output (to be generated)
├── quickstart.md        # Phase 1 output (to be generated)
├── contracts/           # Phase 1 output (to be generated)
│   └── openapi.yaml     # OpenAPI specification for API contracts
├── checklists/          # Quality validation checklists
│   └── requirements.md  # Specification quality checklist (completed)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root - Nx Monorepo)

```text
hickory/
├── apps/
│   ├── api/                           # ASP.NET Core 9 API
│   │   ├── src/
│   │   │   ├── Features/              # Vertical slices (MediatR)
│   │   │   │   ├── Tickets/           # Ticket CRUD operations
│   │   │   │   ├── Users/             # User management
│   │   │   │   ├── Categories/        # Category management
│   │   │   │   ├── Tags/              # Tag management
│   │   │   │   ├── Comments/          # Comment operations
│   │   │   │   ├── Attachments/       # File upload/download
│   │   │   │   ├── Search/            # Full-text search
│   │   │   │   ├── Notifications/     # Notification management
│   │   │   │   └── Reports/           # Analytics & reporting
│   │   │   ├── Infrastructure/        # Cross-cutting concerns
│   │   │   │   ├── Data/              # EF Core DbContext, migrations
│   │   │   │   ├── Auth/              # OAuth 2.0/OIDC + local auth
│   │   │   │   ├── Messaging/         # MassTransit configuration
│   │   │   │   ├── RealTime/          # SignalR hubs
│   │   │   │   ├── Storage/           # File storage abstraction
│   │   │   │   └── Observability/     # Serilog, OpenTelemetry, health checks
│   │   │   ├── Common/                # Shared models, validators, behaviors
│   │   │   └── Program.cs             # Application entry point
│   │   └── tests/
│   │       ├── Unit/                  # Unit tests for handlers, validators
│   │       ├── Integration/           # Testcontainers, DB integration tests
│   │       └── Contract/              # OpenAPI contract validation
│   │
│   ├── web/                           # Next.js 15 Frontend
│   │   ├── src/
│   │   │   ├── app/                   # Next.js App Router pages
│   │   │   │   ├── (auth)/            # Authentication pages
│   │   │   │   ├── tickets/           # Ticket list and details
│   │   │   │   ├── agent/             # Agent queue and management
│   │   │   │   ├── admin/             # Administration pages
│   │   │   │   └── knowledge/         # Knowledge base (future)
│   │   │   ├── components/            # React components
│   │   │   │   ├── ui/                # ShadCN UI primitives
│   │   │   │   ├── tickets/           # Ticket-specific components
│   │   │   │   ├── forms/             # Form components
│   │   │   │   └── layout/            # Layout components
│   │   │   ├── lib/                   # Utilities and API client
│   │   │   │   ├── api/               # NSwag-generated client
│   │   │   │   ├── hooks/             # React hooks
│   │   │   │   ├── queries/           # TanStack Query queries/mutations
│   │   │   │   └── signalr/           # SignalR connection management
│   │   │   └── styles/                # Global styles, Tailwind config
│   │   └── tests/
│   │       ├── unit/                  # Component unit tests (Vitest)
│   │       └── e2e/                   # Playwright E2E tests
│   │
│   └── cli/                           # Node.js CLI Tool
│       ├── src/
│       │   ├── commands/              # CLI commands (tickets, search, etc.)
│       │   ├── lib/                   # API client (NSwag-generated)
│       │   └── index.ts               # Commander.js entry point
│       └── tests/
│           └── unit/                  # Command tests (Vitest)
│
├── packages/                          # Shared packages (if needed)
│   └── contracts/                     # Shared TypeScript types from OpenAPI
│
├── tools/                             # Build and development tools
│   └── generators/                    # Nx generators for feature scaffolding
│
├── docker/                            # Docker configurations
│   ├── api.Dockerfile
│   ├── web.Dockerfile
│   └── docker-compose.yml             # Local development environment
│
├── .github/
│   ├── workflows/                     # CI/CD pipelines
│   └── prompts/                       # Speckit prompts
│
├── .specify/                          # Speckit configuration
│   ├── memory/
│   │   └── constitution.md            # Project constitution
│   ├── scripts/                       # Speckit automation scripts
│   └── templates/                     # Feature templates
│
├── specs/                             # Feature specifications
│   └── 001-help-desk-core/            # This feature
│
├── nx.json                            # Nx workspace configuration
├── package.json                       # Root package.json
└── README.md                          # Project documentation
```

**Structure Decision**: Nx monorepo with three applications (API, Web, CLI) to maximize code sharing and maintain end-to-end type safety through NSwag-generated clients. The backend uses vertical-slice architecture where each feature is self-contained with its own handlers, validators, and models. The frontend uses Next.js App Router with component-based architecture and TanStack Query for server state management. All three applications consume the same OpenAPI contract, ensuring consistent API interactions.

## Complexity Tracking

> **No violations - This section intentionally left empty**

The architecture design complies with all constitutional principles without requiring justification for additional complexity. The three-application structure (API, Web, CLI) is explicitly supported by the monorepo approach and aligns with the requirement for multiple client types (web UI and command-line interface) consuming the same API. Vertical-slice architecture in the backend eliminates the need for repository patterns or additional abstraction layers, keeping the implementation simple and maintainable.

---

## Phase Completion Status

### ✅ Phase 0: Research & Planning (Complete)

**Artifacts Created**:
- [research.md](./research.md) - Technology selection rationale and architectural patterns
- Constitution Check completed - all principles satisfied

**Key Decisions**:
- Nx monorepo for code sharing and build optimization
- ASP.NET Core 9 with vertical-slice architecture
- PostgreSQL 16+ with full-text search
- Next.js 15 with TailwindCSS and ShadCN UI
- NSwag for end-to-end type safety
- MassTransit + SignalR for event-driven + real-time architecture
- Comprehensive observability with Serilog + OpenTelemetry

### ✅ Phase 1: Design & Contracts (Complete)

**Artifacts Created**:
- [data-model.md](./data-model.md) - Complete database schema with Entity Framework Core configuration
- [contracts/openapi.yaml](./contracts/openapi.yaml) - OpenAPI 3.0 specification for all API endpoints
- [quickstart.md](./quickstart.md) - Developer setup guide for local development
- `.github/copilot-instructions.md` - Updated agent context with technology stack

**Data Model Highlights**:
- 7 core entities (User, Ticket, Comment, Category, Tag, Attachment, TicketTag)
- Optimistic concurrency control via row versioning
- Full-text search with PostgreSQL `tsvector`
- Comprehensive indexing strategy for query performance
- EF Core migrations for schema evolution

**API Contract Highlights**:
- 18 endpoints covering authentication, tickets, comments, attachments, categories, tags, search
- JWT bearer authentication with refresh token support
- Pagination, filtering, and sorting on list endpoints
- Optimistic concurrency handling with 409 Conflict responses
- Multipart form-data for file uploads (10MB limit)
- ProblemDetails format for consistent error responses

**Agent Context Updated**:
- GitHub Copilot instructions file created with technology stack details
- Enables AI assistant to provide context-aware code suggestions

### ✅ Phase 2: Task Breakdown (Complete)

**Artifact Created**:
- [tasks.md](./tasks.md) - Complete implementation task list with 275 tasks organized by user story

**Task Organization Highlights**:
- **11 Phases**: Setup → Foundational → 7 User Stories → Attachments → Polish
- **275 Total Tasks**: Granular, actionable tasks with specific file paths
- **150+ Parallel Tasks**: Marked with [P] for concurrent execution
- **User Story Mapping**: All tasks labeled with [US#] for traceability
- **MVP Scope**: Phases 1-4 (108 tasks) deliver core help desk functionality
- **Independent Stories**: Each user story can be implemented and tested independently after Foundational phase

**Task Breakdown by Phase**:
- Phase 1 (Setup): 10 tasks - Project initialization and monorepo structure
- Phase 2 (Foundational): 30 tasks - Authentication, infrastructure, client setup (BLOCKS all user stories)
- Phase 3 (US1 - Submit Tickets): 35 tasks - Ticket creation, viewing, commenting (P1 - MVP)
- Phase 4 (US2 - Agent Response): 33 tasks - Agent queue, assignment, resolution (P1 - MVP)
- Phase 5 (US3 - Categories/Tags): 38 tasks - Organization and filtering (P2)
- Phase 6 (US4 - Search): 17 tasks - Full-text search with PostgreSQL (P2)
- Phase 7 (US5 - Notifications): 23 tasks - Email, in-app, webhooks, SignalR (P2)
- Phase 8 (US6 - Knowledge Base): 24 tasks - Self-service articles (P3)
- Phase 9 (US7 - Reports): 22 tasks - Analytics and SLA tracking (P3)
- Phase 10 (Attachments): 15 tasks - File upload/download across all interfaces
- Phase 11 (Polish): 28 tasks - Accessibility, security, performance, documentation

**Implementation Strategy**:
- **MVP First**: Complete Phases 1-4 (108 tasks) for working help desk
- **Incremental Delivery**: Each phase delivers a deployable increment
- **Type Safety**: NSwag-generated clients ensure end-to-end type safety
- **Contract Testing**: OpenAPI spec validation built into development workflow
- **Parallel Execution**: With 4 developers, MVP achievable in 5 weeks

---

## Next Steps for Development Team

1. **Review Generated Artifacts**:
   - Read [research.md](./research.md) for technology rationale
   - Study [data-model.md](./data-model.md) for database design
   - Review [contracts/openapi.yaml](./contracts/openapi.yaml) for API contracts
   - Review [tasks.md](./tasks.md) for implementation task breakdown
   - Follow [quickstart.md](./quickstart.md) to set up local environment

2. **Environment Setup**:
   - Clone repository and install dependencies
   - Run `docker-compose up -d` to start infrastructure
   - Apply database migrations
   - Verify all services are healthy

3. **Generate TypeScript Clients**:
   - Run NSwag code generation from OpenAPI spec
   - Verify type safety across frontend and CLI

4. **Prioritize MVP Development**:
   - Focus on Phases 1-4 from tasks.md (108 tasks)
   - User Story 1: Submit and Track Support Tickets (P1)
   - User Story 2: Manage and Respond to Support Tickets (P1)
   - Deliver working help desk before adding enhancements

5. **Begin Development**:
   - Start with Phase 1 (Setup) - 10 tasks to initialize project structure
   - Move to Phase 2 (Foundational) - 30 tasks to build core infrastructure
   - Once foundation complete, implement US1 and US2 in parallel if staffed
   - Follow task dependencies and parallel markers [P] for efficient execution

6. **Continuous Integration**:
   - All tests must pass before merge
   - Code review required per constitutional principles
   - Automated checks for linting, formatting, type safety
   - NSwag contract validation ensures API compatibility

7. **Quality Gates**:
   - Verify success criteria from spec.md at each checkpoint
   - Monitor performance targets (P95 latency <200ms for GET, <500ms for POST/PUT/DELETE)
   - Ensure accessibility (WCAG 2.1 AA) and security standards
   - Validate against constitution principles at each phase

---

**Plan Status**: All Phases Complete ✅  
**Last Updated**: October 26, 2025  
**Ready for**: Implementation via tasks.md (275 tasks identified)  
**Next Command**: `/speckit.analyze` to validate consistency across spec, plan, and tasks
