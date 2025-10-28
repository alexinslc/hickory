# E2E Tests - Hickory Help Desk

End-to-end tests using Playwright to validate complete user journeys and workflows.

## Test Coverage

### User Story 1: Submit and Track Tickets (`user-story-1-submit-tickets.spec.ts`)

Tests the complete user experience for ticket submission and tracking:

- **US1.1**: User can submit a ticket and receive confirmation
- **US1.2**: User can view their ticket list
- **US1.3**: User can view ticket details and conversation history
- **US1.4**: User can add comments to their tickets
- **US1.5**: Form validation works correctly (client-side)
- **US1.6**: User can navigate between pages
- **US1.7**: **Ticket submission completes within 2 seconds (SC-003)** ✨

### User Story 2: Agent Manages Tickets (`user-story-2-agent-manage.spec.ts`)

Tests the agent workflows for managing and responding to support requests:

- **US2.1**: Agent can view ticket queue
- **US2.2**: Agent can assign tickets to themselves
- **US2.3**: Agent can add comments and respond to users
- **US2.4**: Agent can update ticket status
- **US2.5**: Agent can close tickets with resolution notes
- **US2.6**: Agent can add internal notes (not visible to users)
- **US2.7**: **Agent can respond within 30 seconds (SC-006, T108)** ✨
- **US2.8**: Complete agent workflow is smooth
- **US2.9**: Agent queue shows priority and status information
- **US2.10**: Agent can filter or sort tickets in queue

## Running the Tests

### Prerequisites

1. **API Server Running**:
   ```bash
   cd apps/api
   dotnet run
   ```
   Wait for: `Now listening on: http://localhost:5000`

2. **Web App Running**:
   ```bash
   npx nx serve web
   ```
   Wait for: `http://localhost:3000`

3. **Database Seeded**: Make sure migrations are applied and database is accessible

### Run All E2E Tests

```bash
# Run all tests
npx nx e2e web-e2e

# Run with UI mode (interactive)
npx nx e2e web-e2e --ui

# Run specific test file
npx playwright test user-story-1-submit-tickets.spec.ts

# Run with headed browser (see what's happening)
npx nx e2e web-e2e --headed

# Run in specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

### Debug Tests

```bash
# Debug mode with Playwright Inspector
npx playwright test --debug

# Debug specific test
npx playwright test user-story-1-submit-tickets.spec.ts:20 --debug

# Generate trace for failed tests
npx playwright test --trace on
```

### View Test Results

```bash
# Show last test report
npx playwright show-report

# View traces for debugging failures
npx playwright show-trace trace.zip
```

## Success Criteria Validation

### SC-003: Ticket Submission Performance
**Test**: `US1.7` in `user-story-1-submit-tickets.spec.ts`

Validates that ticket submission completes within 2 seconds:
- Registers user
- Submits ticket
- Measures total time from click to confirmation
- **PASS**: Duration < 2000ms

### SC-006: Agent Response Time (T108)
**Test**: `US2.7` in `user-story-2-agent-manage.spec.ts`

Validates that agents can respond to tickets within 30 seconds:
- Agent views queue
- Finds ticket
- Adds response
- Measures total workflow time
- **PASS**: Duration < 30000ms

## Test Data

Tests use dynamically generated test data to avoid conflicts:
- Email addresses include timestamps: `e2e-user-{timestamp}@example.com`
- Ticket titles include test identifiers
- All test data is created during test execution
- No cleanup is performed (tests assume clean environment)

## Configuration

### Environment Variables

```bash
# Base URL for tests
BASE_URL=http://localhost:3000 npx nx e2e web-e2e

# API URL (if different)
API_URL=http://localhost:5000 npx nx e2e web-e2e
```

### Playwright Config

Configuration is in `playwright.config.ts`:
- Tests run on Chromium, Firefox, and WebKit
- Automatic retry on failure (1 retry)
- Screenshots on failure
- Video recording for failed tests
- Traces captured on first retry

## CI/CD Integration

Example GitHub Actions workflow:

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  e2e:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Install Playwright Browsers
        run: npx playwright install --with-deps
      
      - name: Start API
        run: |
          cd apps/api
          dotnet run &
          
      - name: Start Web App
        run: npx nx serve web &
        
      - name: Wait for services
        run: |
          npx wait-on http://localhost:3000
          npx wait-on http://localhost:5000
      
      - name: Run E2E tests
        run: npx nx e2e web-e2e
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: playwright-report/
```

## Troubleshooting

### Tests timing out
- Ensure API and web servers are running
- Check network connectivity
- Increase timeouts in test if operations are genuinely slow

### Element not found errors
- HTML structure may have changed
- Use `page.pause()` to debug interactively
- Check selectors match actual rendered HTML
- Use more flexible selectors (text content over CSS classes)

### Flaky tests
- Add explicit waits: `await page.waitForSelector(...)`
- Use `await expect(...).toBeVisible()` with timeouts
- Avoid `page.waitForTimeout()` - use event-based waiting
- Check for race conditions in test logic

### Authentication issues
- Verify test user credentials
- Check JWT token generation and validation
- Ensure cookies/local storage is cleared between tests

## Best Practices

1. **Test Independence**: Each test should be fully independent
2. **Descriptive Names**: Test names should clearly describe what's being tested
3. **Arrange-Act-Assert**: Follow AAA pattern in tests
4. **Explicit Waits**: Always wait for elements/navigation explicitly
5. **Flexible Selectors**: Prefer data-testid or text content over CSS classes
6. **Error Messages**: Use descriptive expect messages for debugging
7. **Screenshots**: Test failures automatically capture screenshots
8. **Trace Files**: Enable traces for debugging complex interactions

## Adding New Tests

1. Create a new `.spec.ts` file in `apps/web-e2e/src/`
2. Import test utilities: `import { test, expect } from '@playwright/test'`
3. Use `test.describe()` to group related tests
4. Follow the naming convention: `user-story-X-description.spec.ts`
5. Add test documentation to this README
6. Run tests locally before committing

## Performance Benchmarks

Current performance benchmarks (on development machine):

| Metric | Target | Typical | Notes |
|--------|--------|---------|-------|
| Ticket Submission | <2s | ~800ms | SC-003 |
| Agent Response | <30s | ~5s | SC-006, T108 |
| Page Load | <3s | ~1.2s | First contentful paint |
| Queue View | <2s | ~600ms | Ticket list rendering |

These benchmarks help identify performance regressions during development.
