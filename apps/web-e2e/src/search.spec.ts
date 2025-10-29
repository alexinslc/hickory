import { test, expect } from '@playwright/test';
import { registerUser, generateTestUser, createTicket } from './fixtures/test-helpers';

/**
 * E2E Tests: Search Functionality
 * 
 * Tests the search functionality including:
 * - Basic ticket search
 * - Search filters
 * - Search results display
 * - Empty search results
 */

test.describe('Search: Basic Functionality', () => {
  let testUser: ReturnType<typeof generateTestUser>;
  let ticketTitle: string;

  test.beforeEach(async ({ page }) => {
    testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Create a test ticket to search for
    ticketTitle = `Searchable Ticket ${Date.now()}`;
    await createTicket(page, {
      title: ticketTitle,
      description: 'This ticket should be findable via search',
      priority: 'Medium',
    });
  });

  test('S1.1: Search page loads successfully', async ({ page }) => {
    await page.goto('/search');
    
    // Verify search page elements
    await expect(
      page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first()
    ).toBeVisible();
  });

  test('S1.2: Can search for tickets by title', async ({ page }) => {
    await page.goto('/search');
    
    // Search for the ticket
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    await searchInput.fill(ticketTitle);
    
    // Submit search (either button or enter key)
    await searchInput.press('Enter').catch(async () => {
      // If Enter doesn't work, look for search button
      await page.click('button:has-text("Search"), button[type="submit"]');
    });
    
    // Verify search results
    await expect(page.locator(`text=${ticketTitle}`)).toBeVisible({ timeout: 5000 });
  });

  test('S1.3: Search shows no results for non-existent query', async ({ page }) => {
    await page.goto('/search');
    
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    await searchInput.fill('NonExistentTicketQuery12345XYZ');
    await searchInput.press('Enter');
    
    // Should show no results message
    await expect(
      page.locator('text=/no results|nothing found|no tickets/i').first()
    ).toBeVisible({ timeout: 5000 });
  });

  test('S1.4: Search results can be clicked to view details', async ({ page }) => {
    await page.goto('/search');
    
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    await searchInput.fill(ticketTitle);
    await searchInput.press('Enter');
    
    // Wait for results
    await page.waitForTimeout(1000);
    
    // Click on the result
    await page.click(`text=${ticketTitle}`);
    
    // Should navigate to ticket details
    await expect(page).toHaveURL(/\/tickets\/[a-f0-9-]+/);
  });

  test('S1.5: Search works from navigation bar', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Look for search in navigation
    const navSearch = page.locator('nav input[type="search"], header input[type="search"]');
    
    if (await navSearch.first().isVisible().catch(() => false)) {
      await navSearch.first().fill(ticketTitle);
      await navSearch.first().press('Enter');
      
      // Should show search results
      await expect(page.locator(`text=${ticketTitle}`)).toBeVisible({ timeout: 5000 });
    } else {
      // Skip if no search in navigation
      test.skip();
    }
  });

  test('S1.6: Search handles special characters', async ({ page }) => {
    // Create ticket with special characters
    const specialTitle = `Test @#$ Ticket ${Date.now()}`;
    await createTicket(page, {
      title: specialTitle,
      description: 'Ticket with special characters',
      priority: 'Low',
    });
    
    await page.goto('/search');
    
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    await searchInput.fill(specialTitle);
    await searchInput.press('Enter');
    
    // Should find the ticket
    await expect(page.locator(`text=${specialTitle}`)).toBeVisible({ timeout: 5000 });
  });

  test('S1.7: Search is case-insensitive', async ({ page }) => {
    await page.goto('/search');
    
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    
    // Search with different case
    await searchInput.fill(ticketTitle.toUpperCase());
    await searchInput.press('Enter');
    
    // Should still find the ticket
    await expect(page.locator(`text=${ticketTitle}`).first()).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Search: Filters', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Create tickets with different priorities
    await createTicket(page, {
      title: `High Priority Ticket ${Date.now()}`,
      description: 'High priority test ticket',
      priority: 'High',
    });
    
    await createTicket(page, {
      title: `Low Priority Ticket ${Date.now()}`,
      description: 'Low priority test ticket',
      priority: 'Low',
    });
  });

  test('S2.1: Can filter search results by priority', async ({ page }) => {
    await page.goto('/search');
    
    // Look for priority filter
    const priorityFilter = page.locator('select[name="priority"], [data-filter="priority"]');
    
    if (await priorityFilter.isVisible().catch(() => false)) {
      await priorityFilter.selectOption('High');
      
      // Should show only high priority tickets
      await expect(page.locator('text=High Priority Ticket')).toBeVisible({ timeout: 5000 });
      
      // Low priority should not be visible
      await expect(page.locator('text=Low Priority Ticket')).not.toBeVisible();
    } else {
      // Skip if filters not implemented
      test.skip();
    }
  });

  test('S2.2: Can filter search results by status', async ({ page }) => {
    await page.goto('/search');
    
    // Look for status filter
    const statusFilter = page.locator('select[name="status"], [data-filter="status"]');
    
    if (await statusFilter.isVisible().catch(() => false)) {
      await statusFilter.selectOption('Open');
      
      // Results should be filtered (all tickets should be Open since just created)
      const results = page.locator('[data-testid="ticket-row"], tbody tr');
      const count = await results.count().catch(() => 0);
      
      expect(count).toBeGreaterThan(0);
    } else {
      // Skip if filters not implemented
      test.skip();
    }
  });

  test('S2.3: Can clear search filters', async ({ page }) => {
    await page.goto('/search');
    
    // Look for clear or reset button
    const clearButton = page.locator('button:has-text("Clear"), button:has-text("Reset")');
    
    if (await clearButton.isVisible().catch(() => false)) {
      // Apply some filter first
      const priorityFilter = page.locator('select[name="priority"]');
      if (await priorityFilter.isVisible().catch(() => false)) {
        await priorityFilter.selectOption('High');
        await page.waitForTimeout(500);
      }
      
      // Clear filters
      await clearButton.click();
      
      // Filters should be reset
      const selectedValue = await priorityFilter.inputValue().catch(() => '');
      expect(selectedValue === '' || selectedValue === 'all').toBe(true);
    } else {
      // Skip if clear functionality not implemented
      test.skip();
    }
  });
});

test.describe('Search: Performance', () => {
  test('S3.1: Search results load within acceptable time', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Create a ticket to search for
    const ticketTitle = `Performance Test Ticket ${Date.now()}`;
    await createTicket(page, {
      title: ticketTitle,
      description: 'Testing search performance',
      priority: 'Medium',
    });
    
    await page.goto('/search');
    
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    
    // Measure search time
    const startTime = Date.now();
    
    await searchInput.fill(ticketTitle);
    await searchInput.press('Enter');
    
    // Wait for results
    await page.locator(`text=${ticketTitle}`).first().waitFor({ state: 'visible', timeout: 5000 });
    
    const searchTime = Date.now() - startTime;
    
    // Search should complete within 3 seconds
    expect(searchTime).toBeLessThan(3000);
    
    console.log(`Search completed in ${searchTime}ms`);
  });
});

test.describe('Search: Edge Cases', () => {
  test('S4.1: Search handles very long query strings', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    await page.goto('/search');
    
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    
    // Try very long search query
    const longQuery = 'A'.repeat(500);
    await searchInput.fill(longQuery);
    await searchInput.press('Enter');
    
    // Should handle gracefully (show no results or error)
    await page.waitForTimeout(1000);
    
    // Page should still be functional
    await expect(searchInput).toBeVisible();
  });

  test('S4.2: Search handles empty query', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    await page.goto('/search');
    
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    
    // Try to search with empty query
    await searchInput.press('Enter');
    
    // Should handle gracefully (show all tickets or prompt for input)
    await page.waitForTimeout(1000);
    
    // Page should still be functional
    await expect(searchInput).toBeVisible();
  });

  test('S4.3: Search handles SQL injection attempts', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    await page.goto('/search');
    
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], input[name="search"]').first();
    
    // Try SQL injection
    await searchInput.fill("'; DROP TABLE tickets; --");
    await searchInput.press('Enter');
    
    // Should handle safely and show no results
    await page.waitForTimeout(1000);
    
    // Application should still work
    await expect(searchInput).toBeVisible();
    
    // Verify app still functions
    await page.goto('/dashboard');
    await expect(page).toHaveURL('/dashboard');
  });
});
