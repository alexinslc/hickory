# E2E Test Guide - Hickory Help Desk

## Overview

This document provides comprehensive guidance on writing, running, and maintaining end-to-end tests for the Hickory Help Desk application using Playwright.

## Test Structure

### Test Organization

Tests are organized by feature/functionality:

```
apps/web-e2e/src/
├── fixtures/
│   └── test-helpers.ts          # Reusable utilities
├── user-story-1-submit-tickets.spec.ts    # User ticket workflows
├── user-story-2-agent-manage.spec.ts      # Agent workflows
├── dashboard.spec.ts             # Dashboard functionality
├── authentication.spec.ts        # Auth flows
├── search.spec.ts               # Search features
├── knowledge-base.spec.ts       # Knowledge base
├── admin.spec.ts                # Admin features
└── settings.spec.ts             # Settings & notifications
```

### Test Naming Convention

Tests follow a hierarchical naming structure:

- **Feature-ID**: `FEATURE#.#` (e.g., `D1.1`, `A2.3`)
- **Description**: Clear, action-oriented description
- **Example**: `D1.1: Dashboard loads successfully`

## Writing Tests

### Basic Test Structure

```typescript
import { test, expect } from '@playwright/test';
import { registerUser, generateTestUser } from './fixtures/test-helpers';

test.describe('Feature: Description', () => {
  test.beforeEach(async ({ page }) => {
    // Setup: Create user, login, navigate, etc.
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('F1.1: Test description', async ({ page }) => {
    // Arrange: Set up test data and state
    await page.goto('/feature');
    
    // Act: Perform the action being tested
    await page.click('button:has-text("Action")');
    
    // Assert: Verify the expected outcome
    await expect(page.locator('text=Success')).toBeVisible();
  });
});
```

### Using Test Helpers

The `test-helpers.ts` file provides reusable utilities:

#### Data Generators

```typescript
// Generate test user
const testUser = generateTestUser('user'); // or 'agent' or 'admin'
// Returns: { email, password, firstName, lastName }

// Generate ticket data
const ticket = generateTicketData('My Prefix');
// Returns: { title, description, priority }
```

#### Authentication Helpers

```typescript
// Register a new user
await registerUser(page, testUser);

// Login existing user
await loginUser(page, { email: 'user@test.com', password: 'pass123' });

// Logout
await logout(page);
```

#### Ticket Helpers

```typescript
// Create a ticket (returns ticket ID)
const ticketId = await createTicket(page, {
  title: 'Test Ticket',
  description: 'Description',
  priority: 'High'
});

// Add comment to ticket
await addComment(page, 'This is my comment');
```

#### Navigation Helpers

```typescript
await navigateToDashboard(page);
await navigateToTicketList(page);
await navigateToAgentQueue(page);
```

### Best Practices

#### 1. Use Descriptive Selectors

```typescript
// ❌ Bad: CSS class selectors (brittle)
await page.click('.btn-primary');

// ✅ Good: Text content (resilient)
await page.click('button:has-text("Submit")');

// ✅ Better: Data attributes (explicit)
await page.click('[data-testid="submit-button"]');
```

#### 2. Wait for Elements Properly

```typescript
// ❌ Bad: Fixed timeouts
await page.waitForTimeout(3000);

// ✅ Good: Wait for specific element
await page.waitForSelector('text=Success', { state: 'visible' });

// ✅ Better: Use Playwright's auto-waiting
await expect(page.locator('text=Success')).toBeVisible();
```

#### 3. Handle Optional Elements

```typescript
// Check if element exists before interacting
if (await element.isVisible().catch(() => false)) {
  await element.click();
} else {
  test.skip(); // Or handle alternative flow
}
```

#### 4. Test Independence

Each test should:
- Create its own test data
- Not depend on other tests
- Clean up after itself (if necessary)
- Be runnable in isolation

```typescript
test('Independent test', async ({ page }) => {
  // Create fresh user for this test
  const testUser = generateTestUser('user');
  await registerUser(page, testUser);
  
  // Test logic here
});
```

#### 5. Performance Testing

```typescript
test('Performance test', async ({ page }) => {
  const startTime = Date.now();
  
  // Perform action
  await page.click('button:has-text("Submit")');
  await page.waitForURL(/\/success/);
  
  const duration = Date.now() - startTime;
  
  // Assert performance requirement
  expect(duration).toBeLessThan(2000);
  
  console.log(`Action completed in ${duration}ms`);
});
```

## Running Tests

### Basic Commands

```bash
# Run all tests
npx nx e2e web-e2e

# Run specific file
npx playwright test dashboard.spec.ts

# Run specific test
npx playwright test dashboard.spec.ts:42

# Run tests matching pattern
npx playwright test --grep "dashboard"

# Run in headed mode (see browser)
npx nx e2e web-e2e --headed

# Run in UI mode (interactive)
npx nx e2e web-e2e --ui

# Run in debug mode
npx playwright test --debug
```

### Browser Selection

```bash
# Run on specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit

# Run on all browsers
npx playwright test
```

### Parallel Execution

```bash
# Run with specific number of workers
npx playwright test --workers=4

# Run tests in serial (one at a time)
npx playwright test --workers=1
```

## Debugging Tests

### Debug Mode

```bash
# Launch Playwright Inspector
npx playwright test --debug

# Debug specific test
npx playwright test dashboard.spec.ts:42 --debug
```

### Screenshots and Videos

Tests automatically capture:
- Screenshots on failure
- Videos for failed tests
- Traces on first retry

View artifacts:
```bash
# Show last test report
npx playwright show-report

# View trace
npx playwright show-trace trace.zip
```

### Console Logging

```typescript
test('Debug test', async ({ page }) => {
  // Log to console
  console.log('Current URL:', page.url());
  
  // Log element text
  const text = await page.locator('h1').textContent();
  console.log('Heading:', text);
  
  // Pause execution
  await page.pause();
});
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  e2e:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Install Playwright browsers
        run: npx playwright install --with-deps
      
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

### Common Issues

#### 1. Element Not Found

**Problem**: `TimeoutError: page.locator(...) exceeded timeout`

**Solutions**:
- Increase timeout: `{ timeout: 10000 }`
- Wait for navigation: `await page.waitForLoadState('networkidle')`
- Check selector accuracy
- Verify element is actually rendered

#### 2. Flaky Tests

**Problem**: Tests pass/fail inconsistently

**Solutions**:
- Avoid `waitForTimeout()` - use event-based waiting
- Add explicit waits for dynamic content
- Use `.toBeVisible()` with timeout
- Check for race conditions

#### 3. Authentication Issues

**Problem**: Tests fail with auth errors

**Solutions**:
- Verify test user credentials
- Check session persistence
- Clear cookies/storage if needed
- Ensure auth token is valid

#### 4. Browser Installation Issues

**Problem**: Playwright browsers not installed

**Solutions**:
```bash
# Install all browsers
npx playwright install

# Install with dependencies
npx playwright install --with-deps

# Install specific browser
npx playwright install chromium
```

## Test Maintenance

### When to Update Tests

Update tests when:
1. UI structure changes significantly
2. Feature behavior changes
3. New features are added
4. Bugs are fixed (add regression test)
5. Performance requirements change

### Keeping Tests DRY

Use test helpers for common operations:

```typescript
// ❌ Bad: Repetitive code
test('Test 1', async ({ page }) => {
  await page.goto('/auth/register');
  await page.fill('input[name="email"]', 'user@test.com');
  // ... more repetitive code
});

// ✅ Good: Use helper
test('Test 1', async ({ page }) => {
  const user = generateTestUser('user');
  await registerUser(page, user);
});
```

### Test Data Management

- Generate unique data per test using timestamps
- Use realistic test data
- Clean up test data if necessary
- Don't rely on existing data

## Performance Benchmarks

Current performance targets:

| Operation | Target | Test |
|-----------|--------|------|
| Ticket submission | <2s | US1.7 |
| Agent response | <30s | US2.7 |
| Dashboard load | <5s | D4.1 |
| Search results | <3s | S3.1 |
| Page load | <5s | Various |

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Test Selectors Guide](https://playwright.dev/docs/selectors)
- [Debugging Guide](https://playwright.dev/docs/debug)
- [CI/CD Integration](https://playwright.dev/docs/ci)

## Contributing

When adding new tests:

1. Follow existing patterns and naming conventions
2. Use test helpers for common operations
3. Add descriptive test names
4. Include AAA pattern (Arrange, Act, Assert)
5. Test both happy path and edge cases
6. Update documentation
7. Ensure tests are independent
8. Run tests locally before committing

## Support

For issues or questions:
- Check this guide first
- Review existing tests for examples
- Check Playwright documentation
- Ask in team chat or create an issue
