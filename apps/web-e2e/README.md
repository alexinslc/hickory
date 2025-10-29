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

### Dashboard Functionality (`dashboard.spec.ts`)

Tests the main dashboard features:

- **D1.1-D1.8**: User dashboard view (layout, stats, recent tickets, quick actions, updates)
- **D2.1-D2.3**: Navigation (menu accessibility, section navigation, dashboard links)
- **D3.1-D3.2**: Responsive design (mobile and tablet viewports)
- **D4.1**: Performance (dashboard load time)

### Authentication Flows (`authentication.spec.ts`)

Tests all authentication functionality:

- **A1.1-A1.8**: Login (page load, valid/invalid credentials, validation, links, redirects)
- **A2.1-A2.7**: Registration (page load, valid data, validation, duplicate prevention, links)
- **A3.1-A3.3**: Logout (successful logout, session clearing, access control)
- **A4.1-A4.2**: Session persistence (page reloads, navigation)
- **A5.1-A5.2**: Security (password masking, form protection)

### Search Functionality (`search.spec.ts`)

Tests the search features:

- **S1.1-S1.7**: Basic search (page load, title search, no results, clickable results, nav search, special chars, case-insensitive)
- **S2.1-S2.3**: Filters (priority filter, status filter, clear filters)
- **S3.1**: Performance (search results load time)
- **S4.1-S4.3**: Edge cases (long queries, empty queries, SQL injection protection)

### Knowledge Base (`knowledge-base.spec.ts`)

Tests the knowledge base functionality:

- **KB1.1-KB1.6**: Viewing articles (page load, article list, navigation, details, content, back navigation)
- **KB2.1-KB2.2**: Search (search functionality, article search)
- **KB3.1-KB3.2**: Categories (organization, filtering)
- **KB4.1-KB4.3**: Creating/editing (create page, form fields, edit page)
- **KB5.1**: Mobile responsive
- **KB6.1**: Performance (page load time)

### Admin Functionality (`admin.spec.ts`)

Tests admin-specific features:

- **ADM1.1-ADM1.6**: Category management (navigation, list view, create, edit, delete, validation)
- **ADM2.1-ADM2.2**: Access control (non-admin restrictions, navigation visibility)
- **ADM3.1-ADM3.3**: Advanced features (search/filter, sortable table, deletion confirmation)
- **ADM4.1**: Performance (admin page load time)

### Settings and Notifications (`settings.spec.ts`)

Tests user settings and preferences:

- **SET1.1-SET1.2**: Navigation (settings access, page load)
- **SET2.1-SET2.6**: Notification preferences (page load, options, email toggle, ticket notifications, comment notifications, persistence)
- **SET3.1-SET3.2**: Profile settings (view info, current data display)
- **SET4.1-SET4.3**: UI/UX (section navigation, mobile responsive)
- **SET5.1-SET5.2**: Form validation (error display, invalid data prevention)
- **SET6.1**: Performance (page load time)

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
# Run all tests (starts web server automatically via playwright config)
npx nx e2e web-e2e

# Run with UI mode (interactive)
npx nx e2e web-e2e --ui

# Run specific test file
npx playwright test user-story-1-submit-tickets.spec.ts
npx playwright test dashboard.spec.ts
npx playwright test authentication.spec.ts

# Run with headed browser (see what's happening)
npx nx e2e web-e2e --headed

# Run in specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit

# Run tests matching pattern
npx playwright test --grep "dashboard"
npx playwright test --grep "authentication"
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
