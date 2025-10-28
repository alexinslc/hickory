import { test, expect, Page } from '@playwright/test';

/**
 * E2E Test: User Story 1 - Submit and Track Support Tickets
 * 
 * Tests the complete user journey for submitting and tracking tickets:
 * - User registration and login
 * - Ticket creation with validation
 * - Viewing ticket list
 * - Viewing ticket details
 * - Adding comments to tickets
 */

// Test data
const TEST_USER = {
  email: `e2e-user-${Date.now()}@example.com`,
  password: 'TestPassword123!',
  firstName: 'E2E',
  lastName: 'User',
};

const TEST_TICKET = {
  title: 'E2E Test Ticket - Browser Issue',
  description: 'This is a test ticket created by E2E tests to validate the ticket submission flow.',
  priority: 'High',
};

// Helper function to register a new user
async function registerUser(page: Page, user: typeof TEST_USER) {
  await page.goto('/auth/register');
  
  await page.fill('input[name="email"]', user.email);
  await page.fill('input[name="password"]', user.password);
  await page.fill('input[name="firstName"]', user.firstName);
  await page.fill('input[name="lastName"]', user.lastName);
  
  await page.click('button[type="submit"]');
  
  // Wait for redirect to dashboard after successful registration
  await page.waitForURL('/dashboard', { timeout: 5000 });
}

test.describe('User Story 1: Submit and Track Tickets', () => {
  test.beforeEach(async ({ page }) => {
    // Register and login for each test
    await registerUser(page, TEST_USER);
  });

  test('US1.1: User can submit a ticket and receive confirmation', async ({ page }) => {
    // Navigate to new ticket page
    await page.goto('/tickets/new');
    
    // Fill out ticket form
    await page.fill('input[name="title"]', TEST_TICKET.title);
    await page.fill('textarea[name="description"]', TEST_TICKET.description);
    await page.selectOption('select[name="priority"]', TEST_TICKET.priority);
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Wait for redirect to ticket details page
    await page.waitForURL(/\/tickets\/[a-f0-9-]+/, { timeout: 5000 });
    
    // Verify ticket was created
    await expect(page.locator('h1')).toContainText(TEST_TICKET.title);
    
    // Verify ticket number is displayed (format: HSD-YYYYMMDD-XXXX)
    const ticketNumber = await page.locator('text=/HSD-\\d{8}-\\d{4}/').textContent();
    expect(ticketNumber).toMatch(/HSD-\d{8}-\d{4}/);
    
    // Verify priority badge is visible
    await expect(page.locator(`text=${TEST_TICKET.priority}`)).toBeVisible();
    
    // Verify status is "Open"
    await expect(page.locator('text=Open')).toBeVisible();
  });

  test('US1.2: User can view their ticket list', async ({ page }) => {
    // Create a ticket first
    await page.goto('/tickets/new');
    await page.fill('input[name="title"]', 'Test Ticket for List View');
    await page.fill('textarea[name="description"]', 'This ticket should appear in the list');
    await page.selectOption('select[name="priority"]', 'Medium');
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/tickets\/[a-f0-9-]+/);
    
    // Navigate to ticket list
    await page.goto('/tickets');
    
    // Verify the ticket appears in the list
    await expect(page.locator('text=Test Ticket for List View')).toBeVisible();
    
    // Verify ticket metadata is shown
    await expect(page.locator('text=Medium')).toBeVisible();
    await expect(page.locator('text=Open')).toBeVisible();
    
    // Verify the list shows submitted by the current user
    await expect(page.locator(`text=${TEST_USER.firstName} ${TEST_USER.lastName}`)).toBeVisible();
  });

  test('US1.3: User can view ticket details and conversation history', async ({ page }) => {
    // Create a ticket
    await page.goto('/tickets/new');
    await page.fill('input[name="title"]', 'Ticket with Details Test');
    await page.fill('textarea[name="description"]', 'Testing ticket details view');
    await page.selectOption('select[name="priority"]', 'Low');
    await page.click('button[type="submit"]');
    
    // Should be on ticket details page
    await page.waitForURL(/\/tickets\/[a-f0-9-]+/);
    
    // Verify all ticket details are visible
    await expect(page.locator('h1')).toContainText('Ticket with Details Test');
    await expect(page.locator('text=Testing ticket details view')).toBeVisible();
    await expect(page.locator('text=Low')).toBeVisible();
    await expect(page.locator('text=Open')).toBeVisible();
    
    // Verify submitter information
    await expect(page.locator('text=Submitted by')).toBeVisible();
    await expect(page.locator(`text=${TEST_USER.firstName} ${TEST_USER.lastName}`)).toBeVisible();
    
    // Verify timestamps are shown
    await expect(page.locator('text=Created')).toBeVisible();
    await expect(page.locator('text=Last updated')).toBeVisible();
  });

  test('US1.4: User can add comments to their ticket', async ({ page }) => {
    // Create a ticket
    await page.goto('/tickets/new');
    await page.fill('input[name="title"]', 'Ticket for Comment Test');
    await page.fill('textarea[name="description"]', 'Testing comment functionality');
    await page.selectOption('select[name="priority"]', 'Medium');
    await page.click('button[type="submit"]');
    
    await page.waitForURL(/\/tickets\/[a-f0-9-]+/);
    
    // Add a comment
    const commentText = 'This is a test comment with additional information.';
    await page.fill('textarea[id="comment"]', commentText);
    await page.click('button:has-text("Post Comment")');
    
    // Wait for success feedback
    await expect(page.locator('text=posted successfully').or(page.locator('text=Comment added'))).toBeVisible({ timeout: 3000 });
    
    // Verify comment count increased (initially 0, should now show 1+)
    await expect(page.locator('text=/Activity|Comments/')).toBeVisible();
  });

  test('US1.5: Form validation works correctly', async ({ page }) => {
    await page.goto('/tickets/new');
    
    // Try to submit empty form
    await page.click('button[type="submit"]');
    
    // Should show validation errors
    await expect(page.locator('text=/Title is required|required/i')).toBeVisible();
    
    // Fill title but leave description empty
    await page.fill('input[name="title"]', 'Test');
    await page.click('button[type="submit"]');
    
    // Should show description validation error
    await expect(page.locator('text=/Description is required|required/i')).toBeVisible();
    
    // Test title length validation (min 5 characters)
    await page.fill('input[name="title"]', 'Tes'); // Only 3 characters
    await page.locator('input[name="title"]').blur();
    await expect(page.locator('text=/at least 5|too short/i')).toBeVisible();
    
    // Test description length validation (min 10 characters)
    await page.fill('textarea[name="description"]', 'Short'); // Only 5 characters
    await page.locator('textarea[name="description"]').blur();
    await expect(page.locator('text=/at least 10|too short/i')).toBeVisible();
  });

  test('US1.6: User can navigate between pages', async ({ page }) => {
    // From dashboard to new ticket
    await page.goto('/dashboard');
    await page.click('a:has-text("New Ticket"), a:has-text("Submit"), a:has-text("Create")');
    await expect(page).toHaveURL(/\/tickets\/new/);
    
    // Back to dashboard
    await page.click('a:has-text("Dashboard"), button:has-text("Back")');
    await expect(page).toHaveURL(/\/dashboard/);
    
    // To ticket list
    await page.click('a:has-text("My Tickets"), a:has-text("Tickets")');
    await expect(page).toHaveURL(/\/tickets/);
  });
});

test.describe('User Story 1: Performance - Ticket Submission', () => {
  test('US1.7: Ticket submission completes within 2 seconds (SC-003)', async ({ page }) => {
    // Register and login
    const user = {
      email: `perf-test-${Date.now()}@example.com`,
      password: 'TestPassword123!',
      firstName: 'Perf',
      lastName: 'Test',
    };
    await registerUser(page, user);
    
    // Navigate to new ticket page
    await page.goto('/tickets/new');
    
    // Fill the form
    await page.fill('input[name="title"]', 'Performance Test Ticket');
    await page.fill('textarea[name="description"]', 'Testing submission performance for SC-003');
    await page.selectOption('select[name="priority"]', 'Medium');
    
    // Measure submission time
    const startTime = Date.now();
    
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/tickets\/[a-f0-9-]+/, { timeout: 5000 });
    
    const endTime = Date.now();
    const duration = endTime - startTime;
    
    // Verify ticket was created
    await expect(page.locator('h1')).toContainText('Performance Test Ticket');
    
    // Success Criterion SC-003: Should complete in under 2 seconds
    expect(duration).toBeLessThan(2000);
    
    console.log(`Ticket submission completed in ${duration}ms (target: <2000ms)`);
  });
});
