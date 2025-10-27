<!--
Sync Impact Report:
- Version: [TEMPLATE] → 1.0.0 (Initial constitution ratification)
- Principles Created:
  * I. Code Quality Standards
  * II. Testing Standards (NON-NEGOTIABLE)
  * III. User Experience Consistency
  * IV. Performance Requirements
- Sections Added:
  * Core Principles (4 principles)
  * Development Workflow
  * Governance
- Templates requiring updates:
  ✅ plan-template.md (Constitution Check gates align with principles)
  ✅ spec-template.md (Requirements structure supports principles)
  ✅ tasks-template.md (Task organization supports test-first principle)
- Follow-up TODOs: None
-->

# Hickory Help Desk Constitution

## Core Principles

### I. Code Quality Standards

All code contributed to Hickory MUST meet the following quality requirements:

- **Type Safety**: Full TypeScript strict mode for frontend/CLI, C# nullable reference
  types enabled for backend. Zero tolerance for `any` types or nullable warnings without
  explicit justification.
- **Linting & Formatting**: All code MUST pass ESLint/Prettier (frontend/CLI) and
  dotnet format/StyleCop (backend) with zero warnings before commit.
- **Code Review**: Every pull request MUST be reviewed by at least one maintainer,
  verifying adherence to architectural patterns (vertical slices for backend, component
  composition for frontend).
- **Documentation**: Public APIs, complex business logic, and architectural decisions
  MUST include inline documentation (JSDoc/TSDoc for TS, XML comments for C#).
- **Dependency Management**: New dependencies require justification demonstrating they are
  actively maintained, widely adopted, and solve a problem that cannot be reasonably
  solved with existing stack.

**Rationale**: Hickory targets production deployments from side-projects to SaaS scale.
Type safety and consistent quality prevent categories of bugs, reduce cognitive load, and
enable confident refactoring as the codebase grows.

### II. Testing Standards (NON-NEGOTIABLE)

Hickory follows a strict test-first development discipline:

- **Red-Green-Refactor**: For every user story, acceptance tests MUST be written first,
  verified to fail, then implementation proceeds until tests pass.
- **Test Coverage Gates**: All new features MUST include:
  - **Unit tests** for business logic (MediatR handlers, validation rules, utility
    functions, React hooks)
  - **Integration tests** for API contracts (OpenAPI spec compliance, end-to-end request
    flows through backend)
  - **Contract tests** when introducing or modifying API endpoints (ensuring NSwag-
    generated client types remain compatible)
- **Test Execution**: All tests MUST pass before merge. CI pipeline enforces this gate.
- **Test Independence**: Each test MUST be independently runnable and not depend on
  execution order or shared mutable state.

**Rationale**: Test-first development catches regressions early, documents expected
behavior, and enables safe refactoring. For a help desk system handling production
incidents, reliability is non-negotiable.

### III. User Experience Consistency

Hickory MUST deliver a cohesive, intuitive, and accessible user experience across all
interfaces (web, CLI, real-time notifications):

- **Design System**: All UI components MUST use ShadCN UI primitives with TailwindCSS
  utilities. Custom components require design review to ensure consistency.
- **Accessibility**: WCAG 2.1 AA compliance is REQUIRED. All interactive elements MUST be
  keyboard navigable, screen-reader friendly, and provide appropriate ARIA labels.
- **Responsive Design**: Web interface MUST be fully functional on mobile (portrait/
  landscape), tablet, and desktop viewports.
- **Performance Budget**: Initial page load MUST complete in <2 seconds on 3G networks.
  Core Web Vitals (LCP, FID, CLS) MUST meet "Good" thresholds.
- **Error Handling**: User-facing errors MUST provide actionable guidance. Never expose
  stack traces or internal error codes to end users.
- **Real-time Feedback**: When backend operations take >500ms, UI MUST show loading
  states. SignalR updates MUST appear within 1 second of server event.

**Rationale**: Help desk users are often under time pressure resolving incidents. A
confusing or sluggish UI increases resolution time and user frustration. Consistency and
speed are competitive advantages.

### IV. Performance Requirements

Hickory MUST maintain production-grade performance at scale:

- **API Response Times**: P95 latency for GET requests MUST be <200ms, POST/PUT/DELETE
  <500ms under normal load (100 concurrent users).
- **Database Queries**: All queries MUST use indexes. N+1 query patterns are forbidden.
  EF Core query analysis MUST be enabled in development to detect inefficiencies.
- **Caching Strategy**: TanStack Query MUST cache GET requests with appropriate stale
  times. Backend MUST implement response caching for read-heavy endpoints.
- **Real-time Scalability**: SignalR hubs MUST support at least 1,000 concurrent
  connections per instance. Connection backpressure and reconnection logic MUST be
  implemented.
- **Resource Limits**: Backend containers MUST operate within 512MB RAM under normal
  load. Memory leaks discovered in production are P0 bugs.
- **Observability**: Serilog structured logging, OpenTelemetry tracing, and health checks
  MUST be instrumented for all critical paths. Performance regressions MUST be detectable
  via metrics before user impact.

**Rationale**: Performance directly impacts user productivity and hosting costs.
Proactive performance discipline prevents expensive rewrites and ensures Hickory remains
viable for self-hosted and SaaS deployments at scale.

## Development Workflow

### Feature Implementation

1. **Specification Phase**: Features MUST begin with a user-focused spec in
   `/specs/[###-feature-name]/spec.md` defining user stories, acceptance criteria, and
   testable scenarios.
2. **Planning Phase**: Technical implementation plan in `plan.md` MUST pass Constitution
   Check gates before research begins.
3. **Test-First Development**: Acceptance tests written and approved BEFORE
   implementation (see Principle II).
4. **Incremental Delivery**: User stories MUST be prioritized (P1, P2, P3...) to enable
   independent implementation, testing, and deployment of MVP slices.
5. **Code Review**: PRs MUST reference the spec, demonstrate passing tests, and include
   migration steps if schema/API changes are involved.

### Quality Gates

- **Pre-commit**: Linting, formatting, and unit tests MUST pass locally.
- **Pre-merge**: All tests (unit, integration, contract) MUST pass in CI. Code review
  approved by maintainer.
- **Pre-deploy**: Health checks and smoke tests MUST pass in staging environment.

## Governance

### Constitutional Authority

This constitution supersedes all other practices, guidelines, or tribal knowledge. When
conflicts arise between this document and other project artifacts, the constitution takes
precedence.

### Amendment Process

1. Proposed amendments MUST be documented in a PR against this file.
2. Amendment rationale and impact analysis (affected templates, workflows, existing code)
   MUST be included.
3. Amendments require approval from project maintainers.
4. Version number MUST be updated per semantic versioning:
   - **MAJOR**: Backward-incompatible governance changes or principle removals.
   - **MINOR**: New principles added or material expansions of existing guidance.
   - **PATCH**: Clarifications, wording improvements, or non-semantic refinements.
5. After amendment, dependent artifacts (plan-template.md, spec-template.md,
   tasks-template.md) MUST be updated to maintain consistency.

### Compliance Review

- All pull requests MUST verify compliance with applicable principles.
- Deviations from constitutional requirements MUST be explicitly justified in PR
  description and approved by maintainers.
- Repeated violations without justification may result in PR rejection.

**Version**: 1.0.0 | **Ratified**: 2025-10-26 | **Last Amended**: 2025-10-26
