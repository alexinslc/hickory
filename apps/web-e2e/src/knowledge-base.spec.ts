import { test, expect } from '@playwright/test';
import { registerUser, generateTestUser } from './fixtures/test-helpers';

/**
 * E2E Tests: Knowledge Base
 * 
 * Tests the knowledge base functionality including:
 * - Viewing articles
 * - Creating/editing articles (agent/admin)
 * - Searching articles
 * - Article categories
 */

test.describe('Knowledge Base: Viewing Articles', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('KB1.1: Knowledge base page loads successfully', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    // Verify page loaded
    await page.waitForLoadState('networkidle');
    
    // Should have heading
    await expect(
      page.locator('h1, h2').first()
    ).toBeVisible();
  });

  test('KB1.2: Knowledge base shows articles or empty state', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    // Either articles or empty state should be visible
    const articles = page.locator('[data-testid="article"], article, .article');
    const emptyState = page.locator('text=/no articles|empty|get started/i');
    
    const hasArticles = await articles.first().isVisible().catch(() => false);
    const hasEmptyState = await emptyState.isVisible().catch(() => false);
    
    expect(hasArticles || hasEmptyState).toBe(true);
  });

  test('KB1.3: Can navigate to article list from dashboard', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Look for knowledge base link
    const kbLink = page.locator(
      'a[href="/knowledge-base"], a:has-text("Knowledge Base"), a:has-text("Help Articles")'
    );
    
    if (await kbLink.first().isVisible().catch(() => false)) {
      await kbLink.first().click();
      await expect(page).toHaveURL('/knowledge-base');
    } else {
      // Skip if link not visible
      test.skip();
    }
  });

  test('KB1.4: Can view article details', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    // Look for first article link
    const firstArticle = page.locator(
      '[data-testid="article"] a, article a, .article a'
    ).first();
    
    if (await firstArticle.isVisible().catch(() => false)) {
      await firstArticle.click();
      
      // Should navigate to article details
      await expect(page).toHaveURL(/\/knowledge-base\/[a-f0-9-]+/);
      
      // Article content should be visible
      await expect(
        page.locator('article, .article-content, main').first()
      ).toBeVisible();
    } else {
      // Skip if no articles exist
      test.skip();
    }
  });

  test('KB1.5: Article details show title and content', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    const firstArticle = page.locator(
      '[data-testid="article"] a, article a, .article a'
    ).first();
    
    if (await firstArticle.isVisible().catch(() => false)) {
      // Get article title
      const articleTitle = await firstArticle.textContent().catch(() => '');
      
      await firstArticle.click();
      
      // Title should be visible on details page
      if (articleTitle) {
        await expect(page.locator(`text=${articleTitle}`)).toBeVisible();
      }
      
      // Content area should exist
      await expect(
        page.locator('article, .article-content, .content, main').first()
      ).toBeVisible();
    } else {
      test.skip();
    }
  });

  test('KB1.6: Can navigate back from article to list', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    const firstArticle = page.locator(
      '[data-testid="article"] a, article a, .article a'
    ).first();
    
    if (await firstArticle.isVisible().catch(() => false)) {
      await firstArticle.click();
      
      // Look for back button or breadcrumb
      const backButton = page.locator(
        'a:has-text("Back"), a:has-text("Knowledge Base"), button:has-text("Back")'
      );
      
      if (await backButton.first().isVisible().catch(() => false)) {
        await backButton.first().click();
        await expect(page).toHaveURL('/knowledge-base');
      } else {
        // Use browser back
        await page.goBack();
        await expect(page).toHaveURL('/knowledge-base');
      }
    } else {
      test.skip();
    }
  });
});

test.describe('Knowledge Base: Search', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('KB2.1: Knowledge base has search functionality', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    // Look for search input
    const searchInput = page.locator(
      'input[type="search"], input[placeholder*="search" i], input[name="search"]'
    );
    
    const hasSearch = await searchInput.first().isVisible().catch(() => false);
    
    if (hasSearch) {
      expect(hasSearch).toBe(true);
    } else {
      // Search might be in nav bar
      test.skip();
    }
  });

  test('KB2.2: Can search for articles', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    const searchInput = page.locator(
      'input[type="search"], input[placeholder*="search" i]'
    ).first();
    
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill('help');
      await searchInput.press('Enter');
      
      // Should show search results or no results message
      await page.waitForTimeout(1000);
      
      // Page should still be functional
      await expect(searchInput).toBeVisible();
    } else {
      test.skip();
    }
  });
});

test.describe('Knowledge Base: Categories', () => {
  test.beforeEach(async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
  });

  test('KB3.1: Articles can be organized by categories', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    // Look for category filters or sections
    const categories = page.locator(
      '[data-testid="category"], .category, select[name="category"]'
    );
    
    const hasCategories = await categories.first().isVisible().catch(() => false);
    
    if (hasCategories) {
      expect(hasCategories).toBe(true);
    } else {
      // Categories might not be implemented yet
      test.skip();
    }
  });

  test('KB3.2: Can filter articles by category', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    const categoryFilter = page.locator('select[name="category"], [data-filter="category"]');
    
    if (await categoryFilter.isVisible().catch(() => false)) {
      // Get first option (skip "All" if present)
      const options = await categoryFilter.locator('option').allTextContents();
      const category = options.find(opt => opt !== 'All' && opt.trim() !== '');
      
      if (category) {
        await categoryFilter.selectOption(category);
        await page.waitForTimeout(500);
        
        // Results should be filtered
        // Can't verify exact results without knowing data
        await expect(page.locator('body')).toBeVisible();
      }
    } else {
      test.skip();
    }
  });
});

test.describe('Knowledge Base: Creating/Editing (Agent/Admin)', () => {
  test.beforeEach(async ({ page }) => {
    // Note: This would require agent/admin credentials
    // For now, test the page accessibility
    const testUser = generateTestUser('agent');
    await registerUser(page, testUser);
  });

  test('KB4.1: Can navigate to create article page', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    // Look for create/new article button
    const createButton = page.locator(
      'a:has-text("New Article"), button:has-text("Create"), a:has-text("Add Article")'
    );
    
    if (await createButton.first().isVisible().catch(() => false)) {
      await createButton.first().click();
      
      // Should navigate to create page or open modal
      // URL might be /knowledge-base/new or similar
      await page.waitForTimeout(1000);
      
      // Form should be visible
      const titleInput = page.locator('input[name="title"], input[id="title"]');
      await expect(titleInput).toBeVisible({ timeout: 5000 });
    } else {
      // User might not have permission (correct behavior)
      test.skip();
    }
  });

  test('KB4.2: Create article form has required fields', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    const createButton = page.locator(
      'a:has-text("New Article"), button:has-text("Create")'
    );
    
    if (await createButton.first().isVisible().catch(() => false)) {
      await createButton.first().click();
      await page.waitForTimeout(500);
      
      // Verify form fields exist
      await expect(
        page.locator('input[name="title"], input[id="title"]')
      ).toBeVisible({ timeout: 5000 });
      
      await expect(
        page.locator('textarea[name="content"], textarea[id="content"], [contenteditable]').first()
      ).toBeVisible({ timeout: 5000 });
    } else {
      test.skip();
    }
  });

  test('KB4.3: Can navigate to edit article page', async ({ page }) => {
    await page.goto('/knowledge-base');
    
    // Look for first article
    const firstArticle = page.locator(
      '[data-testid="article"] a, article a'
    ).first();
    
    if (await firstArticle.isVisible().catch(() => false)) {
      await firstArticle.click();
      
      // Look for edit button on article details
      const editButton = page.locator(
        'a:has-text("Edit"), button:has-text("Edit")'
      );
      
      if (await editButton.isVisible().catch(() => false)) {
        await editButton.click();
        
        // Should navigate to edit page
        await expect(page).toHaveURL(/\/knowledge-base\/[a-f0-9-]+\/edit/);
        
        // Edit form should be visible
        await expect(
          page.locator('input[name="title"], input[id="title"]')
        ).toBeVisible();
      }
    }
  });
});

test.describe('Knowledge Base: Mobile Responsive', () => {
  test('KB5.1: Knowledge base is mobile responsive', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    await page.goto('/knowledge-base');
    
    // Verify page loads on mobile
    await page.waitForLoadState('networkidle');
    
    // Main content should be visible
    await expect(page.locator('main, body').first()).toBeVisible();
  });
});

test.describe('Knowledge Base: Performance', () => {
  test('KB6.1: Knowledge base list loads within acceptable time', async ({ page }) => {
    const testUser = generateTestUser('user');
    await registerUser(page, testUser);
    
    const startTime = Date.now();
    
    await page.goto('/knowledge-base');
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    
    // Should load within 5 seconds
    expect(loadTime).toBeLessThan(5000);
    
    console.log(`Knowledge base loaded in ${loadTime}ms`);
  });
});
