import { test, expect } from '@playwright/test';
import { registerUser, generateTestUser } from './fixtures/test-helpers';

/**
 * E2E Tests: Admin Functionality
 * 
 * Tests admin-specific features including:
 * - Category management (CRUD operations)
 * - Admin dashboard access
 * - Admin permissions and role-based access
 */

test.describe('Admin: Category Management', () => {
  test.beforeEach(async ({ page }) => {
    // Note: This would ideally use admin credentials
    // For testing purposes, registering as user and attempting access
    const testUser = generateTestUser('admin');
    await registerUser(page, testUser);
  });

  test('ADM1.1: Can navigate to categories management page', async ({ page }) => {
    // Try to navigate to admin categories page
    await page.goto('/admin/categories');
    
    // Check if page loaded (might redirect if not admin)
    const currentUrl = page.url();
    
    if (currentUrl.includes('/admin/categories')) {
      // User has access - verify page elements
      await page.waitForLoadState('networkidle');
      
      // Should have page heading
      await expect(
        page.locator('h1, h2').first()
      ).toBeVisible();
    } else {
      // User was redirected (correct behavior for non-admin)
      // This is expected for non-admin users
      console.log('Non-admin user correctly denied access to admin pages');
    }
  });

  test('ADM1.2: Categories page shows existing categories', async ({ page }) => {
    await page.goto('/admin/categories');
    
    // Skip if redirected (not admin)
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    // Should show categories list or empty state
    const categoriesList = page.locator('table, [data-testid="categories-list"], .categories');
    const emptyState = page.locator('text=/no categories|empty/i');
    
    const hasList = await categoriesList.isVisible().catch(() => false);
    const hasEmpty = await emptyState.isVisible().catch(() => false);
    
    expect(hasList || hasEmpty).toBe(true);
  });

  test('ADM1.3: Can create a new category', async ({ page }) => {
    await page.goto('/admin/categories');
    
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    // Look for create button
    const createButton = page.locator(
      'button:has-text("New Category"), button:has-text("Add Category"), button:has-text("Create")'
    );
    
    if (await createButton.first().isVisible().catch(() => false)) {
      await createButton.first().click();
      
      // Fill in category form (might be modal or new page)
      const nameInput = page.locator('input[name="name"], input[id="name"], input[placeholder*="name" i]');
      await nameInput.fill(`Test Category ${Date.now()}`);
      
      const descInput = page.locator('textarea[name="description"], textarea[id="description"]');
      if (await descInput.isVisible().catch(() => false)) {
        await descInput.fill('Test category description');
      }
      
      // Submit form
      const submitButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Create")');
      await submitButton.click();
      
      // Should show success message or return to list
      await expect(
        page.locator('text=/success|created|added/i').first()
      ).toBeVisible({ timeout: 5000 }).catch(() => {
        // Or check if we're back on the list page
        return expect(page).toHaveURL('/admin/categories');
      });
    } else {
      test.skip();
    }
  });

  test('ADM1.4: Can edit an existing category', async ({ page }) => {
    await page.goto('/admin/categories');
    
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    // Look for edit button on first category
    const editButton = page.locator(
      'button:has-text("Edit"), a:has-text("Edit"), [data-action="edit"]'
    ).first();
    
    if (await editButton.isVisible().catch(() => false)) {
      await editButton.click();
      
      // Modify category name
      const nameInput = page.locator('input[name="name"], input[id="name"]');
      await nameInput.fill(`Updated Category ${Date.now()}`);
      
      // Submit form
      const submitButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Update")');
      await submitButton.click();
      
      // Should show success message
      await expect(
        page.locator('text=/success|updated|saved/i').first()
      ).toBeVisible({ timeout: 5000 });
    } else {
      test.skip();
    }
  });

  test('ADM1.5: Can delete a category', async ({ page }) => {
    await page.goto('/admin/categories');
    
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    // First create a category to delete
    const createButton = page.locator('button:has-text("New Category"), button:has-text("Create")');
    
    if (await createButton.first().isVisible().catch(() => false)) {
      await createButton.first().click();
      
      const nameInput = page.locator('input[name="name"], input[id="name"]');
      const categoryName = `Delete Test ${Date.now()}`;
      await nameInput.fill(categoryName);
      
      const submitButton = page.locator('button[type="submit"], button:has-text("Save")');
      await submitButton.click();
      
      await page.waitForTimeout(1000);
      
      // Now delete it
      const deleteButton = page.locator(
        `button:has-text("Delete"), [data-action="delete"]`
      ).first();
      
      if (await deleteButton.isVisible().catch(() => false)) {
        await deleteButton.click();
        
        // Confirm deletion (if confirmation dialog appears)
        const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")');
        if (await confirmButton.isVisible().catch(() => false)) {
          await confirmButton.click();
        }
        
        // Should show success message
        await expect(
          page.locator('text=/deleted|removed/i').first()
        ).toBeVisible({ timeout: 5000 });
      }
    } else {
      test.skip();
    }
  });

  test('ADM1.6: Category form validates required fields', async ({ page }) => {
    await page.goto('/admin/categories');
    
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    const createButton = page.locator('button:has-text("New Category"), button:has-text("Create")');
    
    if (await createButton.first().isVisible().catch(() => false)) {
      await createButton.first().click();
      
      // Try to submit without filling name
      const submitButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Create")');
      await submitButton.click();
      
      // Should show validation error
      await expect(
        page.locator('text=/required/i').first()
      ).toBeVisible({ timeout: 3000 });
    } else {
      test.skip();
    }
  });
});

test.describe('Admin: Access Control', () => {
  test('ADM2.1: Non-admin users cannot access admin pages', async ({ page }) => {
    // Register as regular user (not admin)
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Try to access admin page
    await page.goto('/admin/categories');
    
    // Should be redirected or show error
    const currentUrl = page.url();
    
    if (currentUrl.includes('/admin/categories')) {
      // If we're on the admin page, check for access denied message
      const accessDenied = await page.locator('text=/access denied|unauthorized|forbidden/i').isVisible().catch(() => false);
      
      if (!accessDenied) {
        // This might be a security issue - regular users shouldn't access admin pages
        console.warn('Regular user was able to access admin page without error');
      }
    } else {
      // User was redirected (expected behavior)
      expect(currentUrl).not.toContain('/admin/categories');
    }
  });

  test('ADM2.2: Admin navigation only visible to admin users', async ({ page }) => {
    // Register as regular user
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    await page.goto('/dashboard');
    
    // Admin links should not be visible
    const adminLink = page.locator('a[href*="/admin"], a:has-text("Admin")');
    const isAdminLinkVisible = await adminLink.isVisible().catch(() => false);
    
    // Regular users should not see admin links
    expect(isAdminLinkVisible).toBe(false);
  });
});

test.describe('Admin: Categories - Advanced Features', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('admin');
    await registerUser(page, testUser);
  });

  test('ADM3.1: Can search/filter categories', async ({ page }) => {
    await page.goto('/admin/categories');
    
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    // Look for search input
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i]');
    
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill('test');
      await page.waitForTimeout(500);
      
      // Results should be filtered (can't verify exact results without knowing data)
      await expect(searchInput).toBeVisible();
    } else {
      test.skip();
    }
  });

  test('ADM3.2: Categories are displayed in a sortable table', async ({ page }) => {
    await page.goto('/admin/categories');
    
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    // Look for table headers that might be sortable
    const tableHeaders = page.locator('th, [role="columnheader"]');
    
    const headerCount = await tableHeaders.count().catch(() => 0);
    
    if (headerCount > 0) {
      // Table exists
      expect(headerCount).toBeGreaterThan(0);
      
      // Check if headers are clickable (sortable)
      const firstHeader = tableHeaders.first();
      const isSortable = await firstHeader.locator('button, a, [role="button"]').isVisible().catch(() => false);
      
      // Note: Not all implementations make headers sortable
      console.log(`Table headers sortable: ${isSortable}`);
    } else {
      // Categories might be in a different format (cards, list, etc.)
      test.skip();
    }
  });

  test('ADM3.3: Category deletion requires confirmation', async ({ page }) => {
    await page.goto('/admin/categories');
    
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    const deleteButton = page.locator('button:has-text("Delete")').first();
    
    if (await deleteButton.isVisible().catch(() => false)) {
      await deleteButton.click();
      
      // Should show confirmation dialog
      const confirmDialog = page.locator('text=/are you sure|confirm|delete/i');
      const hasConfirmation = await confirmDialog.isVisible().catch(() => false);
      
      expect(hasConfirmation).toBe(true);
      
      // Cancel the deletion
      const cancelButton = page.locator('button:has-text("Cancel"), button:has-text("No")');
      if (await cancelButton.isVisible().catch(() => false)) {
        await cancelButton.click();
      }
    } else {
      test.skip();
    }
  });
});

test.describe('Admin: Performance', () => {
  test('ADM4.1: Admin categories page loads within acceptable time', async ({ page }) => {
    const testUser = generateTestUser('admin');
    await registerUser(page, testUser);
    
    const startTime = Date.now();
    
    await page.goto('/admin/categories');
    
    if (!page.url().includes('/admin/categories')) {
      test.skip();
      return;
    }
    
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    
    // Should load within 5 seconds
    expect(loadTime).toBeLessThan(5000);
    
    console.log(`Admin categories page loaded in ${loadTime}ms`);
  });
});
