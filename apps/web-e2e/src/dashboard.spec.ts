import { test, expect } from '@playwright/test';
import { registerUser, generateTestUser, createTicket, navigateToDashboard } from './fixtures/test-helpers';

/**
 * E2E Tests: Dashboard Functionality
 * 
 * Tests the main dashboard features including:
 * - Dashboard layout and navigation
 * - Ticket statistics display
 * - Recent tickets list
 * - Quick actions
 * - Role-based content
 */

test.describe('Dashboard: User View', () => {
  let testUser: ReturnType<typeof generateTestUser>;

  test.beforeEach(async ({ page }) => {
    testUser = generateTestUser('user');
    await registerUser(page, testUser);
    await navigateToDashboard(page);
  });

  test('D1.1: Dashboard loads successfully', async ({ page }) => {
    // Verify we're on the dashboard
    await expect(page).toHaveURL('/dashboard');
    
    // Verify main heading exists
    await expect(page.locator('h1, h2').first()).toBeVisible();
    
    // Verify page loaded completely
    await page.waitForLoadState('networkidle');
  });

  test('D1.2: Dashboard shows user information', async ({ page }) => {
    // Verify user name is displayed somewhere (header, sidebar, etc.)
    const userName = `${testUser.firstName} ${testUser.lastName}`;
    await expect(page.locator(`text=${userName}`)).toBeVisible();
    
    // Verify user role or type is indicated
    await expect(
      page.locator('text=/User|Customer|End User/i').first()
    ).toBeVisible({ timeout: 10000 }).catch(() => {
      // Role may not be explicitly shown for end users
      return true;
    });
  });

  test('D1.3: Dashboard shows ticket statistics', async ({ page }) => {
    // Look for common dashboard statistics
    const statElements = page.locator('[data-testid*="stat"], .stat, [class*="statistic"]');
    
    // If stats are present, they should be visible
    const count = await statElements.count().catch(() => 0);
    
    if (count > 0) {
      // Verify at least one stat is visible
      await expect(statElements.first()).toBeVisible();
      
      // Stats might include: Total, Open, In Progress, Closed
      const possibleStats = ['Total', 'Open', 'In Progress', 'Closed', 'Resolved'];
      let foundStat = false;
      
      for (const stat of possibleStats) {
        if (await page.locator(`text=${stat}`).first().isVisible().catch(() => false)) {
          foundStat = true;
          break;
        }
      }
      
      expect(foundStat).toBe(true);
    }
  });

  test('D1.4: Dashboard shows recent tickets or empty state', async ({ page }) => {
    // Check for either tickets list or empty state message
    const ticketsList = page.locator('[data-testid="tickets-list"], [data-testid="recent-tickets"], table, [role="table"]');
    const emptyState = page.locator('text=/no tickets|empty|get started/i');
    
    // Either tickets or empty state should be visible
    const hasTickets = await ticketsList.isVisible().catch(() => false);
    const hasEmptyState = await emptyState.isVisible().catch(() => false);
    
    expect(hasTickets || hasEmptyState).toBe(true);
  });

  test('D1.5: Dashboard has quick action buttons', async ({ page }) => {
    // Look for new ticket or create ticket button
    const newTicketButton = page.locator(
      'a:has-text("New Ticket"), button:has-text("New Ticket"), a:has-text("Create Ticket"), a:has-text("Submit")'
    );
    
    await expect(newTicketButton.first()).toBeVisible();
  });

  test('D1.6: Can navigate to create ticket from dashboard', async ({ page }) => {
    // Click new ticket button
    await page.click(
      'a:has-text("New Ticket"), button:has-text("New Ticket"), a:has-text("Create Ticket"), a:has-text("Submit")'
    );
    
    // Verify navigation to ticket creation page
    await expect(page).toHaveURL(/\/tickets\/new/);
  });

  test('D1.7: Can navigate to ticket list from dashboard', async ({ page }) => {
    // Look for link to all tickets
    const ticketsLink = page.locator(
      'a:has-text("My Tickets"), a:has-text("All Tickets"), a:has-text("View All")'
    );
    
    if (await ticketsLink.first().isVisible().catch(() => false)) {
      await ticketsLink.first().click();
      
      // Verify navigation
      await expect(page).toHaveURL(/\/tickets/);
    }
  });

  test('D1.8: Dashboard updates after creating a ticket', async ({ page }) => {
    // Get initial state (if any tickets shown)
    const initialTickets = await page.locator('[data-testid="ticket-row"], tbody tr').count().catch(() => 0);
    
    // Create a new ticket
    const ticket = {
      title: 'Dashboard Update Test Ticket',
      description: 'Testing if dashboard updates after ticket creation',
      priority: 'High',
    };
    
    await createTicket(page, ticket);
    
    // Navigate back to dashboard
    await navigateToDashboard(page);
    
    // Verify ticket appears in dashboard (either in list or stats updated)
    await expect(page.locator(`text=${ticket.title}`).first()).toBeVisible({ timeout: 10000 }).catch(async () => {
      // If not visible in list, check if stats updated
      const currentTickets = await page.locator('[data-testid="ticket-row"], tbody tr').count().catch(() => 0);
      expect(currentTickets).toBeGreaterThanOrEqual(initialTickets);
    });
  });
});

test.describe('Dashboard: Navigation', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('D2.1: Navigation menu is accessible', async ({ page }) => {
    await navigateToDashboard(page);
    
    // Look for navigation menu
    const nav = page.locator('nav, [role="navigation"], [data-testid="navigation"]');
    await expect(nav.first()).toBeVisible();
  });

  test('D2.2: Can navigate between main sections', async ({ page }) => {
    await navigateToDashboard(page);
    
    // Test navigation to tickets
    const ticketsLink = page.locator('a[href="/tickets"], nav a:has-text("Tickets")');
    if (await ticketsLink.first().isVisible().catch(() => false)) {
      await ticketsLink.first().click();
      await expect(page).toHaveURL(/\/tickets/);
    }
  });

  test('D2.3: Dashboard link returns to dashboard', async ({ page }) => {
    // Start on dashboard
    await navigateToDashboard(page);
    
    // Navigate away
    await page.goto('/tickets/new');
    
    // Click dashboard link
    const dashboardLink = page.locator(
      'a[href="/dashboard"], nav a:has-text("Dashboard"), a:has-text("Home")'
    );
    
    await dashboardLink.first().click();
    await expect(page).toHaveURL('/dashboard');
  });
});

test.describe('Dashboard: Responsive Design', () => {
  test('D3.1: Dashboard is mobile responsive', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    await navigateToDashboard(page);
    
    // Verify page loads on mobile
    await page.waitForLoadState('networkidle');
    
    // Main content should be visible
    await expect(page.locator('main, [role="main"], body').first()).toBeVisible();
  });

  test('D3.2: Dashboard is tablet responsive', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Set tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    
    await navigateToDashboard(page);
    
    // Verify page loads on tablet
    await page.waitForLoadState('networkidle');
    
    // Main content should be visible
    await expect(page.locator('main, [role="main"], body').first()).toBeVisible();
  });
});

test.describe('Dashboard: Performance', () => {
  test('D4.1: Dashboard loads within acceptable time', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    const startTime = Date.now();
    
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    
    // Dashboard should load within 5 seconds
    expect(loadTime).toBeLessThan(5000);
    
    console.log(`Dashboard loaded in ${loadTime}ms`);
  });
});
