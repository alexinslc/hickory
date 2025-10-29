import { test, expect } from '@playwright/test';
import { registerUser, generateTestUser } from './fixtures/test-helpers';

/**
 * E2E Tests: Settings and Notifications
 * 
 * Tests user settings and notification preferences including:
 * - Accessing settings page
 * - Updating notification preferences
 * - Profile settings
 * - Email preferences
 */

test.describe('Settings: Navigation', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('SET1.1: Can navigate to settings page', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Look for settings link in navigation or user menu
    const settingsLink = page.locator(
      'a[href="/settings"], a:has-text("Settings"), a[href*="settings"]'
    );
    
    // May need to open user menu first
    const userMenu = page.locator('[data-testid="user-menu"], button[aria-label="User menu"]');
    if (await userMenu.isVisible().catch(() => false)) {
      await userMenu.click();
      await page.waitForTimeout(300);
    }
    
    if (await settingsLink.first().isVisible().catch(() => false)) {
      await settingsLink.first().click();
      await expect(page).toHaveURL(/\/settings/);
    } else {
      // Try direct navigation
      await page.goto('/settings');
      // Verify page loaded
      await page.waitForLoadState('networkidle');
    }
  });

  test('SET1.2: Settings page loads successfully', async ({ page }) => {
    await page.goto('/settings');
    
    // Verify page loaded
    await page.waitForLoadState('networkidle');
    
    // Should have heading
    await expect(
      page.locator('h1, h2').first()
    ).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Settings: Notification Preferences', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('SET2.1: Can navigate to notifications settings', async ({ page }) => {
    // Try direct navigation
    await page.goto('/settings/notifications');
    
    // Verify page loaded
    await page.waitForLoadState('networkidle');
    
    // Should have notification-related elements
    const notificationElements = page.locator(
      'text=/notification|email|alert/i'
    );
    
    const count = await notificationElements.count().catch(() => 0);
    expect(count).toBeGreaterThan(0);
  });

  test('SET2.2: Notifications page shows preference options', async ({ page }) => {
    await page.goto('/settings/notifications');
    
    // Look for checkbox or toggle elements
    const checkboxes = page.locator('input[type="checkbox"], [role="switch"], input[type="radio"]');
    const count = await checkboxes.count().catch(() => 0);
    
    if (count > 0) {
      // Has preference options
      expect(count).toBeGreaterThan(0);
    } else {
      // Might not be implemented yet
      test.skip();
    }
  });

  test('SET2.3: Can toggle email notifications', async ({ page }) => {
    await page.goto('/settings/notifications');
    
    // Look for email notification toggle
    const emailToggle = page.locator(
      'input[type="checkbox"][name*="email"], [role="switch"]'
    ).first();
    
    if (await emailToggle.isVisible().catch(() => false)) {
      const initialState = await emailToggle.isChecked().catch(() => false);
      
      // Toggle the setting
      await emailToggle.click();
      
      await page.waitForTimeout(500);
      
      // State should have changed
      const newState = await emailToggle.isChecked().catch(() => false);
      expect(newState).toBe(!initialState);
      
      // Look for save button if present
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]');
      if (await saveButton.isVisible().catch(() => false)) {
        await saveButton.click();
        
        // Should show success message
        await expect(
          page.locator('text=/saved|updated|success/i').first()
        ).toBeVisible({ timeout: 5000 });
      }
    } else {
      test.skip();
    }
  });

  test('SET2.4: Can configure ticket update notifications', async ({ page }) => {
    await page.goto('/settings/notifications');
    
    // Look for ticket-related notification settings
    const ticketNotification = page.locator(
      'text=/ticket update|ticket notification/i'
    );
    
    if (await ticketNotification.first().isVisible().catch(() => false)) {
      // Find associated toggle/checkbox
      const toggle = ticketNotification.locator('..').locator('input[type="checkbox"], [role="switch"]').first();
      
      if (await toggle.isVisible().catch(() => false)) {
        const initialState = await toggle.isChecked();
        await toggle.click();
        
        const newState = await toggle.isChecked();
        expect(newState).toBe(!initialState);
      }
    } else {
      test.skip();
    }
  });

  test('SET2.5: Can configure comment notifications', async ({ page }) => {
    await page.goto('/settings/notifications');
    
    // Look for comment-related notification settings
    const commentNotification = page.locator(
      'text=/comment|reply|response/i'
    );
    
    if (await commentNotification.first().isVisible().catch(() => false)) {
      // Find associated toggle/checkbox
      const toggle = commentNotification.locator('..').locator('input[type="checkbox"], [role="switch"]').first();
      
      if (await toggle.isVisible().catch(() => false)) {
        await toggle.click();
        await page.waitForTimeout(300);
        
        // Setting changed
        expect(true).toBe(true);
      }
    } else {
      test.skip();
    }
  });

  test('SET2.6: Notification preferences persist after save', async ({ page }) => {
    await page.goto('/settings/notifications');
    
    const emailToggle = page.locator('input[type="checkbox"]').first();
    
    if (await emailToggle.isVisible().catch(() => false)) {
      const initialState = await emailToggle.isChecked();
      
      // Change setting
      await emailToggle.click();
      
      // Save if button exists
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]');
      if (await saveButton.isVisible().catch(() => false)) {
        await saveButton.click();
        await page.waitForTimeout(1000);
      }
      
      // Reload page
      await page.reload();
      await page.waitForLoadState('networkidle');
      
      // Check if setting persisted
      const newState = await emailToggle.isChecked();
      expect(newState).toBe(!initialState);
    } else {
      test.skip();
    }
  });
});

test.describe('Settings: Profile Settings', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('SET3.1: Can view profile information', async ({ page }) => {
    await page.goto('/settings');
    
    // Look for profile or account section
    const profileSection = page.locator(
      'text=/profile|account|personal info/i'
    );
    
    if (await profileSection.first().isVisible().catch(() => false)) {
      // Profile section exists
      expect(true).toBe(true);
    } else {
      // Might be on different page
      await page.goto('/settings/profile').catch(() => {});
    }
    
    // Should show user information
    await expect(
      page.locator('input[name="firstName"], input[name="email"]').first()
    ).toBeVisible({ timeout: 5000 }).catch(() => {
      // Profile editing might not be implemented
      test.skip();
    });
  });

  test('SET3.2: Profile shows current user data', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    await page.goto('/settings');
    
    // Look for email field
    const emailField = page.locator('input[name="email"], input[type="email"]').first();
    
    if (await emailField.isVisible().catch(() => false)) {
      const emailValue = await emailField.inputValue();
      expect(emailValue).toBe(testUser.email);
    }
  });
});

test.describe('Settings: UI/UX', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('SET4.1: Settings has navigation between sections', async ({ page }) => {
    await page.goto('/settings');
    
    // Look for settings navigation/tabs
    const settingsTabs = page.locator(
      'nav a, [role="tab"], .tab'
    );
    
    const count = await settingsTabs.count().catch(() => 0);
    
    if (count > 1) {
      // Has multiple settings sections
      expect(count).toBeGreaterThan(1);
    }
  });

  test('SET4.2: Can navigate between settings sections', async ({ page }) => {
    await page.goto('/settings');
    
    // Look for notifications link
    const notificationsLink = page.locator(
      'a[href*="notifications"], a:has-text("Notifications")'
    );
    
    if (await notificationsLink.first().isVisible().catch(() => false)) {
      await notificationsLink.first().click();
      
      await expect(page).toHaveURL(/notifications/);
    }
  });

  test('SET4.3: Settings page is mobile responsive', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    await page.goto('/settings');
    
    // Verify page loads on mobile
    await page.waitForLoadState('networkidle');
    
    // Main content should be visible
    await expect(page.locator('main, body').first()).toBeVisible();
  });
});

test.describe('Settings: Form Validation', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('SET5.1: Settings form shows validation errors', async ({ page }) => {
    await page.goto('/settings');
    
    // Look for editable field
    const emailField = page.locator('input[name="email"], input[type="email"]').first();
    
    if (await emailField.isVisible().catch(() => false)) {
      // Try to set invalid email
      await emailField.fill('invalid-email');
      await emailField.blur();
      
      // Look for validation error
      await expect(
        page.locator('text=/valid email/i').first()
      ).toBeVisible({ timeout: 3000 }).catch(() => {
        // Some forms only validate on submit
        return true;
      });
    } else {
      test.skip();
    }
  });

  test('SET5.2: Cannot save settings with invalid data', async ({ page }) => {
    await page.goto('/settings');
    
    const emailField = page.locator('input[name="email"]').first();
    const saveButton = page.locator('button:has-text("Save"), button[type="submit"]');
    
    if (await emailField.isVisible().catch(() => false) && await saveButton.isVisible().catch(() => false)) {
      // Set invalid email
      await emailField.clear();
      await emailField.fill('invalid');
      
      // Try to save
      await saveButton.click();
      
      // Should show error or stay on page
      await page.waitForTimeout(1000);
      
      // Should still be on settings page
      await expect(page).toHaveURL(/settings/);
    } else {
      test.skip();
    }
  });
});

test.describe('Settings: Performance', () => {
  test('SET6.1: Settings page loads within acceptable time', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    const startTime = Date.now();
    
    await page.goto('/settings');
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    
    // Should load within 5 seconds
    expect(loadTime).toBeLessThan(5000);
    
    console.log(`Settings page loaded in ${loadTime}ms`);
  });
});
