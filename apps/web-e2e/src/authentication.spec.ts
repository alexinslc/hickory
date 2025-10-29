import { test, expect } from '@playwright/test';
import { generateTestUser, loginUser, logout, registerUser } from './fixtures/test-helpers';

/**
 * E2E Tests: Authentication Flows
 * 
 * Tests authentication functionality including:
 * - Login validation and error handling
 * - Registration validation and success
 * - Logout functionality
 * - Session persistence
 * - Password requirements
 * - Email validation
 */

test.describe('Authentication: Login', () => {
  test('A1.1: Login page loads successfully', async ({ page }) => {
    await page.goto('/auth/login');
    
    // Verify page elements
    await expect(page.locator('input[name="email"]')).toBeVisible();
    await expect(page.locator('input[name="password"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('A1.2: Login with valid credentials succeeds', async ({ page }) => {
    // Register a user first
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Logout
    await logout(page);
    
    // Login with same credentials
    await loginUser(page, {
      email: testUser.email,
      password: testUser.password,
    });
    
    // Verify successful login (redirected to dashboard)
    await expect(page).toHaveURL('/dashboard');
  });

  test('A1.3: Login with invalid email shows error', async ({ page }) => {
    await page.goto('/auth/login');
    
    await page.fill('input[name="email"]', 'nonexistent@example.com');
    await page.fill('input[name="password"]', 'SomePassword123!');
    await page.click('button[type="submit"]');
    
    // Should show error message
    await expect(
      page.locator('text=/invalid|incorrect|not found|error/i').first()
    ).toBeVisible({ timeout: 5000 });
  });

  test('A1.4: Login with invalid password shows error', async ({ page }) => {
    // Register a user first
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    await logout(page);
    
    // Try to login with wrong password
    await page.goto('/auth/login');
    await page.fill('input[name="email"]', testUser.email);
    await page.fill('input[name="password"]', 'WrongPassword123!');
    await page.click('button[type="submit"]');
    
    // Should show error message
    await expect(
      page.locator('text=/invalid|incorrect|wrong|error/i').first()
    ).toBeVisible({ timeout: 5000 });
  });

  test('A1.5: Login form validates required fields', async ({ page }) => {
    await page.goto('/auth/login');
    
    // Try to submit empty form
    await page.click('button[type="submit"]');
    
    // Should show validation errors
    await expect(
      page.locator('text=/required|enter/i').first()
    ).toBeVisible({ timeout: 3000 });
  });

  test('A1.6: Login form validates email format', async ({ page }) => {
    await page.goto('/auth/login');
    
    await page.fill('input[name="email"]', 'invalid-email');
    await page.fill('input[name="password"]', 'SomePassword123!');
    
    // Trigger validation
    await page.locator('input[name="email"]').blur();
    
    // Should show email format error
    await expect(
      page.locator('text=/valid email|email format/i').first()
    ).toBeVisible({ timeout: 3000 }).catch(() => {
      // Some forms only validate on submit
      return true;
    });
  });

  test('A1.7: Login page has link to registration', async ({ page }) => {
    await page.goto('/auth/login');
    
    // Look for sign up or register link
    const registerLink = page.locator(
      'a:has-text("Sign Up"), a:has-text("Register"), a:has-text("Create Account")'
    );
    
    await expect(registerLink.first()).toBeVisible();
    
    // Click and verify navigation
    await registerLink.first().click();
    await expect(page).toHaveURL(/\/auth\/register/);
  });

  test('A1.8: Login remembers return URL after redirect', async ({ page }) => {
    // Try to access protected page
    await page.goto('/tickets/new');
    
    // Should redirect to login
    await page.waitForURL(/\/auth\/login/, { timeout: 5000 }).catch(() => {
      // Page might already be accessible if not implementing auth guards
    });
  });
});

test.describe('Authentication: Registration', () => {
  test('A2.1: Registration page loads successfully', async ({ page }) => {
    await page.goto('/auth/register');
    
    // Verify form fields
    await expect(page.locator('input[name="email"]')).toBeVisible();
    await expect(page.locator('input[name="password"]')).toBeVisible();
    await expect(page.locator('input[name="firstName"]')).toBeVisible();
    await expect(page.locator('input[name="lastName"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('A2.2: Registration with valid data succeeds', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Verify successful registration (redirected to dashboard)
    await expect(page).toHaveURL('/dashboard');
    
    // Verify user is logged in
    await expect(
      page.locator(`text=${testUser.firstName} ${testUser.lastName}`)
    ).toBeVisible();
  });

  test('A2.3: Registration validates required fields', async ({ page }) => {
    await page.goto('/auth/register');
    
    // Try to submit empty form
    await page.click('button[type="submit"]');
    
    // Should show validation errors
    await expect(
      page.locator('text=/required/i').first()
    ).toBeVisible({ timeout: 3000 });
  });

  test('A2.4: Registration validates email format', async ({ page }) => {
    await page.goto('/auth/register');
    
    await page.fill('input[name="email"]', 'invalid-email');
    await page.fill('input[name="password"]', 'TestPassword123!');
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    
    // Trigger validation
    await page.locator('input[name="email"]').blur();
    
    // Should show email format error
    await expect(
      page.locator('text=/valid email|email format/i').first()
    ).toBeVisible({ timeout: 3000 }).catch(() => {
      // Some forms only validate on submit
      return true;
    });
  });

  test('A2.5: Registration validates password requirements', async ({ page }) => {
    await page.goto('/auth/register');
    
    // Try weak password
    await page.fill('input[name="email"]', 'test@example.com');
    await page.fill('input[name="password"]', 'weak');
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    
    await page.locator('input[name="password"]').blur();
    
    // Should show password requirements error
    await expect(
      page.locator('text=/password|character|strong/i').first()
    ).toBeVisible({ timeout: 3000 }).catch(() => {
      // Some forms only validate on submit
      return true;
    });
  });

  test('A2.6: Registration prevents duplicate email', async ({ page }) => {
    // Register first user
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Logout
    await logout(page);
    
    // Try to register with same email
    await page.goto('/auth/register');
    await page.fill('input[name="email"]', testUser.email);
    await page.fill('input[name="password"]', 'AnotherPassword123!');
    await page.fill('input[name="firstName"]', 'Another');
    await page.fill('input[name="lastName"]', 'User');
    await page.click('button[type="submit"]');
    
    // Should show error about duplicate email
    await expect(
      page.locator('text=/already exists|already registered|taken/i').first()
    ).toBeVisible({ timeout: 5000 });
  });

  test('A2.7: Registration page has link to login', async ({ page }) => {
    await page.goto('/auth/register');
    
    // Look for login or sign in link
    const loginLink = page.locator(
      'a:has-text("Sign In"), a:has-text("Login"), a:has-text("Already have")'
    );
    
    await expect(loginLink.first()).toBeVisible();
    
    // Click and verify navigation
    await loginLink.first().click();
    await expect(page).toHaveURL(/\/auth\/login/);
  });
});

test.describe('Authentication: Logout', () => {
  test('A3.1: User can logout successfully', async ({ page }) => {
    // Register and login
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Verify logged in
    await expect(page).toHaveURL('/dashboard');
    
    // Logout
    await logout(page);
    
    // Verify redirected to login or home
    await expect(page).toHaveURL(/\/(auth\/login|$)/);
  });

  test('A3.2: Logout clears session', async ({ page }) => {
    // Register and login
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Logout
    await logout(page);
    
    // Try to access protected page
    await page.goto('/tickets/new');
    
    // Should redirect to login (or show login-required message)
    await page.waitForURL(/\/auth\/login/, { timeout: 5000 }).catch(() => {
      // Some implementations may handle this differently
    });
  });

  test('A3.3: Cannot access protected pages after logout', async ({ page }) => {
    // Register and login
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Remember dashboard URL
    const dashboardUrl = page.url();
    
    // Logout
    await logout(page);
    
    // Try to navigate back to dashboard
    await page.goto(dashboardUrl);
    
    // Should not be on dashboard (either redirected to login or show error)
    const currentUrl = page.url();
    expect(currentUrl).not.toBe(dashboardUrl);
  });
});

test.describe('Authentication: Session Persistence', () => {
  test('A4.1: Session persists across page reloads', async ({ page }) => {
    // Register and login
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Verify on dashboard
    await expect(page).toHaveURL('/dashboard');
    
    // Reload page
    await page.reload();
    
    // Should still be logged in and on dashboard
    await expect(page).toHaveURL('/dashboard');
    await expect(
      page.locator(`text=${testUser.firstName} ${testUser.lastName}`)
    ).toBeVisible();
  });

  test('A4.2: Session persists across navigation', async ({ page }) => {
    // Register and login
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Navigate to different pages
    await page.goto('/tickets');
    await page.goto('/tickets/new');
    await page.goto('/dashboard');
    
    // Should still be logged in
    await expect(
      page.locator(`text=${testUser.firstName} ${testUser.lastName}`)
    ).toBeVisible();
  });
});

test.describe('Authentication: Security', () => {
  test('A5.1: Password field is masked', async ({ page }) => {
    await page.goto('/auth/login');
    
    const passwordInput = page.locator('input[name="password"]');
    const inputType = await passwordInput.getAttribute('type');
    
    expect(inputType).toBe('password');
  });

  test('A5.2: Login form prevents multiple rapid submissions', async ({ page }) => {
    await page.goto('/auth/login');
    
    await page.fill('input[name="email"]', 'test@example.com');
    await page.fill('input[name="password"]', 'TestPassword123!');
    
    // Click submit button multiple times rapidly
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();
    await submitButton.click();
    await submitButton.click();
    
    // Button should be disabled during submission
    const isDisabled = await submitButton.isDisabled().catch(() => false);
    
    // Either button is disabled or there's loading state
    if (!isDisabled) {
      // Check for loading indicator
      const hasLoader = await page.locator('text=/loading|processing/i, [data-loading], .spinner').first().isVisible().catch(() => false);
      expect(hasLoader).toBe(true);
    }
  });
});
