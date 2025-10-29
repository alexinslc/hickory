import { Page } from '@playwright/test';

/**
 * Test Helpers and Fixtures for E2E Tests
 * Shared utilities to reduce code duplication across test files
 */

// Test data generators
export function generateTestUser(role: 'user' | 'agent' | 'admin' = 'user') {
  const timestamp = Date.now();
  return {
    email: `e2e-${role}-${timestamp}@example.com`,
    password: 'TestPassword123!',
    firstName: 'E2E',
    lastName: `${role.charAt(0).toUpperCase() + role.slice(1)}`,
  };
}

export function generateTicketData(prefix = 'E2E Test') {
  const timestamp = Date.now();
  return {
    title: `${prefix} Ticket - ${timestamp}`,
    description: `This is an e2e test ticket created at ${new Date().toISOString()}`,
    priority: 'Medium' as const,
  };
}

// Authentication helpers
export async function registerUser(
  page: Page,
  user: { email: string; password: string; firstName: string; lastName: string }
) {
  await page.goto('/auth/register');

  await page.fill('input[name="email"]', user.email);
  await page.fill('input[name="password"]', user.password);
  await page.fill('input[name="firstName"]', user.firstName);
  await page.fill('input[name="lastName"]', user.lastName);

  await page.click('button[type="submit"]');

  // Wait for redirect to dashboard after successful registration
  await page.waitForURL('/dashboard', { timeout: 5000 });
}

export async function loginUser(
  page: Page,
  credentials: { email: string; password: string }
) {
  await page.goto('/auth/login');

  await page.fill('input[name="email"]', credentials.email);
  await page.fill('input[name="password"]', credentials.password);

  await page.click('button[type="submit"]');

  // Wait for redirect to dashboard after successful login
  await page.waitForURL('/dashboard', { timeout: 5000 });
}

export async function logout(page: Page) {
  // Look for user menu or logout button
  const logoutButton = page.locator(
    'button:has-text("Logout"), button:has-text("Sign Out"), a:has-text("Logout")'
  );

  // May need to open user menu first
  const userMenu = page.locator(
    '[data-testid="user-menu"], button[aria-label="User menu"]'
  );
  if (await userMenu.isVisible().catch(() => false)) {
    await userMenu.click();
  }

  await logoutButton.click();

  // Wait for redirect to login or home page
  await page.waitForURL(/\/(auth\/login|$)/, { timeout: 5000 });
}

// Ticket helpers
export async function createTicket(
  page: Page,
  ticket: { title: string; description: string; priority?: string }
) {
  await page.goto('/tickets/new');

  await page.fill('input[name="title"]', ticket.title);
  await page.fill('textarea[name="description"]', ticket.description);

  if (ticket.priority) {
    await page.selectOption('select[name="priority"]', ticket.priority);
  }

  await page.click('button[type="submit"]');

  // Wait for redirect to ticket details page
  await page.waitForURL(/\/tickets\/[a-f0-9-]+/, { timeout: 5000 });

  // Extract and return ticket ID
  const url = page.url();
  const ticketId = url.split('/tickets/')[1];
  return ticketId;
}

export async function addComment(page: Page, commentText: string) {
  await page.fill(
    'textarea[id="comment"], textarea[name="content"], textarea[placeholder*="comment" i]',
    commentText
  );
  await page.click(
    'button:has-text("Post Comment"), button:has-text("Reply"), button:has-text("Add Comment")'
  );

  // Wait for success feedback
  await page
    .locator('text=/posted|added|success/i')
    .first()
    .waitFor({ state: 'visible', timeout: 5000 });
}

// Navigation helpers
export async function navigateToTicketList(page: Page) {
  await page.goto('/tickets');
  await page.waitForLoadState('networkidle');
}

export async function navigateToDashboard(page: Page) {
  await page.goto('/dashboard');
  await page.waitForLoadState('networkidle');
}

export async function navigateToAgentQueue(page: Page) {
  await page.goto('/agent/queue');
  await page.waitForLoadState('networkidle');
}

// Wait helpers
export async function waitForElement(
  page: Page,
  selector: string,
  timeout = 5000
) {
  await page.waitForSelector(selector, { state: 'visible', timeout });
}

export async function waitForText(page: Page, text: string, timeout = 5000) {
  await page.locator(`text=${text}`).first().waitFor({ state: 'visible', timeout });
}

// Assertion helpers
export async function assertUrlContains(page: Page, urlPart: string) {
  const url = page.url();
  if (!url.includes(urlPart)) {
    throw new Error(`Expected URL to contain "${urlPart}" but got "${url}"`);
  }
}

export async function assertElementVisible(page: Page, selector: string) {
  const element = page.locator(selector).first();
  const isVisible = await element.isVisible().catch(() => false);
  if (!isVisible) {
    throw new Error(`Expected element "${selector}" to be visible`);
  }
}

export async function assertTextVisible(page: Page, text: string) {
  const element = page.locator(`text=${text}`).first();
  const isVisible = await element.isVisible().catch(() => false);
  if (!isVisible) {
    throw new Error(`Expected text "${text}" to be visible`);
  }
}

// Performance helpers
export function measureTime<T>(fn: () => Promise<T>): Promise<[T, number]> {
  return (async () => {
    const start = Date.now();
    const result = await fn();
    const duration = Date.now() - start;
    return [result, duration];
  })();
}

// Screenshot helpers
export async function takeScreenshot(
  page: Page,
  name: string,
  path = 'screenshots'
) {
  await page.screenshot({ path: `${path}/${name}-${Date.now()}.png` });
}
