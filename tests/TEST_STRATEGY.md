# Hickory Help Desk - Comprehensive Test Strategy

## Overview
This document outlines the testing approach for the Hickory Help Desk monorepo, which consists of 3 applications:
- **Web App** (Next.js 15 + React 19)
- **API** (.NET 9.0 ASP.NET Core)
- **CLI** (Node.js + TypeScript)

## Goals
- **Target Coverage**: 60% (Solid Foundation)
- **Priorities**: Fast CI/CD pipeline, High confidence, Easy maintenance
- **Integration Testing**: Full database, API, and SignalR integration tests

---

## Testing Pyramid Distribution

```
                 /\
                /  \
              /  E2E \          5% - Full user journeys (Playwright)
             /--------\
            /          \
          /  Integration \     25% - API endpoints, DB, SignalR
         /----------------\
        /                  \
      /    Unit Tests       \  70% - Components, handlers, services
     /----------------------\
```

### Target Test Distribution
| Test Type | Percentage | Purpose | Speed |
|-----------|-----------|---------|-------|
| Unit Tests | 70% | Fast feedback, component/function isolation | <1s |
| Integration Tests | 25% | Real dependencies, confidence in connections | 1-5s |
| E2E Tests | 5% | Critical user journeys, UI workflows | 10-30s |

---

## Application-Specific Strategies

### 1. API (.NET 9.0)

#### Test Projects Structure
```
apps/
  api/
    Hickory.Api.csproj
    Hickory.Api.Tests/              # Unit tests (xUnit)
      Hickory.Api.Tests.csproj
      Features/
        Auth/
          LoginCommandHandlerTests.cs
          RegisterCommandHandlerTests.cs
        Tickets/
          CreateTicketCommandHandlerTests.cs
          UpdateTicketCommandHandlerTests.cs
        Comments/
        KnowledgeBase/
      Infrastructure/
        Auth/
          JwtTokenServiceTests.cs
          PasswordHasherTests.cs
        Data/
          TicketNumberGeneratorTests.cs
      Validators/
        LoginCommandValidatorTests.cs
        CreateTicketCommandValidatorTests.cs

    Hickory.Api.IntegrationTests/   # Integration tests
      Hickory.Api.IntegrationTests.csproj
      Controllers/
        AuthControllerTests.cs
        TicketsControllerTests.cs
        CommentsControllerTests.cs
      Database/
        TicketRepositoryTests.cs
        EfCoreTransactionTests.cs
        MigrationTests.cs
      RealTime/
        NotificationHubTests.cs
      TestFixtures/
        WebApplicationFactory.cs
        DatabaseFixture.cs
```

#### Test Categories
1. **Unit Tests** (Handler Logic, Validators, Services)
   - MediatR command/query handlers
   - FluentValidation validators
   - JWT token generation/validation
   - Password hashing
   - Ticket number generation
   - Business logic

2. **Integration Tests** (Database, Controllers, SignalR)
   - Full HTTP request/response cycle
   - EF Core database operations
   - Transaction handling
   - Optimistic concurrency conflicts
   - SignalR hub connections and notifications
   - Authentication middleware

3. **Test Utilities**
   - Custom WebApplicationFactory for TestServer
   - In-memory PostgreSQL (via Testcontainers)
   - Fixture for seeding test data
   - Mock services for email/webhooks

#### Key Frameworks & Libraries
```xml
<PackageReference Include="xunit" Version="2.9.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageReference Include="Moq" Version="4.20.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
<PackageReference Include="Respawn" Version="6.2.1" /> <!-- DB cleanup -->
```

#### Coverage Targets by Feature
| Feature | Unit | Integration | Priority |
|---------|------|-------------|----------|
| Authentication | 80% | 90% | Critical |
| Ticket CRUD | 70% | 80% | Critical |
| Comments | 60% | 70% | High |
| Knowledge Base | 50% | 60% | Medium |
| Optimistic Locking | 80% | 90% | High |
| SignalR Notifications | 60% | 80% | High |
| Validation | 90% | N/A | Critical |

---

### 2. Web App (Next.js + React)

#### Test Structure
```
apps/
  web/
    src/
      __tests__/
        components/
          ui/
            Button.test.tsx
            Input.test.tsx
          tickets/
            TicketForm.test.tsx
            TicketList.test.tsx
            TicketDetail.test.tsx
          agent/
            AgentQueue.test.tsx
        hooks/
          use-auth.test.ts
          use-tickets.test.ts
          use-comments.test.ts
          use-agent.test.ts
        lib/
          api-client.test.ts
          ticket-utils.test.ts
          utils.test.ts (already exists)
        store/
          auth-store.test.ts
      lib/
        utils.spec.ts (already exists)
      specs/
        index.spec.tsx (already exists)
```

#### Test Categories
1. **Component Tests** (React Testing Library)
   - Form components (validation, submission)
   - List/detail views (rendering, interactions)
   - Modal dialogs (open, close, submit)
   - UI primitives (buttons, inputs, badges)
   - Layout components

2. **Hook Tests** (renderHook utility)
   - `use-auth`: login, logout, token refresh
   - `use-tickets`: CRUD operations, optimistic updates
   - `use-comments`: add comment, fetch comments
   - `use-agent`: queue management, assignment
   - `use-optimistic-locking`: conflict handling

3. **Service Tests**
   - API client: request/response, error handling, interceptors
   - Auth store: persistence, logout, token management
   - Ticket utilities: formatting, validation
   - SignalR connection handling

4. **E2E Tests** (Already exist - enhance if needed)
   - User Story 1: Submit tickets (7 tests)
   - User Story 2: Agent management (10 tests)

#### Key Frameworks & Libraries (Already installed)
```json
{
  "jest": "^30.0.2",
  "jest-environment-jsdom": "^30.0.2",
  "@testing-library/react": "16.3.0",
  "@testing-library/dom": "10.4.0",
  "@testing-library/user-event": "^14.5.2" (to be added),
  "@playwright/test": "^1.36.0"
}
```

#### Coverage Targets
| Category | Target | Priority |
|----------|--------|----------|
| Custom Hooks | 80% | Critical |
| Form Components | 75% | Critical |
| API Client | 70% | Critical |
| UI Components | 60% | High |
| Layout Components | 50% | Medium |

---

### 3. CLI (Node.js + TypeScript)

#### Test Structure
```
apps/
  cli/
    src/
      __tests__/
        commands/
          auth.test.ts
          ticket.test.ts
          agent.test.ts
        utils/
          config.test.ts
          formatting.test.ts
      commands/
        auth.ts
        ticket.ts
        agent.ts
  cli-e2e/
    src/
      cli/
        cli.spec.ts (already exists - enhance)
        auth-flow.spec.ts
        ticket-flow.spec.ts
```

#### Test Categories
1. **Unit Tests** (Command Logic)
   - Command argument parsing
   - API client integration
   - Config file operations
   - Output formatting
   - Error handling

2. **Integration Tests** (CLI E2E)
   - Full command execution
   - Login/logout flow
   - Ticket creation and viewing
   - Agent operations

#### Coverage Targets
| Category | Target | Priority |
|----------|--------|----------|
| Command Handlers | 70% | High |
| API Integration | 60% | High |
| Config Management | 80% | Medium |

---

## Test Data Management

### Approach
1. **Unit Tests**: Use mocks and stubs (Moq for .NET, Jest mocks for Node)
2. **Integration Tests**:
   - Use Testcontainers for PostgreSQL
   - Seed data via fixtures
   - Clean database between tests (Respawn library)
3. **E2E Tests**: Use dedicated test user accounts

### Test Data Fixtures
```typescript
// Shared test data
export const testUsers = {
  regularUser: { email: 'user@test.com', password: 'Test123!' },
  agent: { email: 'agent@test.com', password: 'Test123!' },
  admin: { email: 'admin@test.com', password: 'Test123!' }
};

export const testTickets = {
  open: { id: 1, title: 'Test Ticket', status: 'Open' },
  inProgress: { id: 2, title: 'In Progress', status: 'InProgress' }
};
```

---

## CI/CD Optimization

### Parallel Execution
- Enable Jest parallel workers (default)
- Run test projects independently
- Matrix strategy for different environments

### Caching Strategy
```yaml
# Example GitHub Actions cache
- uses: actions/cache@v3
  with:
    path: |
      ~/.nuget/packages
      node_modules
      ~/.cache/Cypress
    key: ${{ runner.os }}-test-${{ hashFiles('**/package-lock.json', '**/*.csproj') }}
```

### Test Execution Order (Fast → Slow)
1. Unit tests (1-2 minutes)
2. Integration tests (3-5 minutes)
3. E2E tests (5-10 minutes)

### Fail Fast
- Run unit tests first
- Only run integration/E2E if unit tests pass
- Use `--bail` flag to stop on first failure

---

## Naming Conventions

### .NET (xUnit)
```csharp
public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken() { }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedException() { }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    public async Task Handle_InvalidEmail_ThrowsValidationException(string email) { }
}
```

### JavaScript/TypeScript (Jest)
```typescript
describe('useAuth hook', () => {
  it('should login successfully with valid credentials', async () => {});

  it('should throw error on invalid password', async () => {});

  it.each([
    ['', 'empty email'],
    ['invalid', 'invalid format']
  ])('should fail validation for %s', async (email, _) => {});
});
```

---

## Coverage Reporting

### Tools
- **.NET**: Coverlet + ReportGenerator
- **Node**: Jest built-in coverage (Istanbul)
- **Overall**: Codecov.io or similar

### Coverage Thresholds
```json
{
  "jest": {
    "coverageThreshold": {
      "global": {
        "branches": 60,
        "functions": 60,
        "lines": 60,
        "statements": 60
      }
    }
  }
}
```

### .NET Coverage (in .csproj)
```xml
<PropertyGroup>
  <CoverletOutput>./coverage/</CoverletOutput>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
  <Threshold>60</Threshold>
  <ThresholdType>line,branch,method</ThresholdType>
</PropertyGroup>
```

---

## Running Tests

### Local Development
```bash
# Run all tests
npm test

# Per application
npx nx test web           # Web unit tests
npx nx test cli           # CLI unit tests
dotnet test apps/api      # API unit + integration tests

# E2E tests
npx nx e2e web-e2e        # Web E2E (Playwright)
npx nx e2e cli-e2e        # CLI E2E

# With coverage
npx nx test web --coverage
dotnet test apps/api --collect:"XPlat Code Coverage"

# Watch mode
npx nx test web --watch
```

### CI/CD Pipeline
```bash
# Fast feedback loop
npm run test:ci           # All unit tests in parallel
npm run test:integration  # Integration tests
npm run test:e2e          # E2E tests (last)

# Generate coverage report
npm run test:coverage
```

---

## Test Maintenance Guidelines

### Do's
- Keep tests simple and focused
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)
- Mock external dependencies
- Clean up test data after integration tests
- Use fixtures for common setup
- Write tests for bug fixes

### Don'ts
- Don't test implementation details
- Don't share mutable state between tests
- Don't mock what you don't own (test real dependencies)
- Don't write tests that depend on execution order
- Don't use random data (use deterministic fixtures)

---

## Next Steps

### Phase 1: Infrastructure (This PR)
- [ ] Set up .NET test projects (xUnit)
- [ ] Configure Testcontainers for PostgreSQL
- [ ] Add test utilities and fixtures
- [ ] Configure coverage reporting

### Phase 2: Critical Path Tests
- [ ] Authentication (API + Web)
- [ ] Ticket CRUD (API + Web + CLI)
- [ ] Core validation logic

### Phase 3: Extended Coverage
- [ ] Comments, Knowledge Base
- [ ] Agent operations
- [ ] Real-time SignalR features
- [ ] Optimistic locking conflicts

### Phase 4: Optimization
- [ ] Performance testing
- [ ] Load testing
- [ ] CI/CD pipeline tuning

---

## Success Metrics

- [ ] 60%+ code coverage across all projects
- [ ] All critical paths tested (auth, ticket CRUD)
- [ ] Zero failing tests on main branch
- [ ] Test suite runs in <10 minutes
- [ ] Easy to add new tests (good examples + docs)

---

**Last Updated**: 2025-10-28
**Author**: Claude Code
**Version**: 1.0
