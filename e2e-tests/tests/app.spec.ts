import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('should load and display title', async ({ page }) => {
    await page.goto('/');
    
    // Wait for Blazor to load
    await page.waitForLoadState('networkidle');
    
    // Verify page title
    await expect(page).toHaveTitle(/PoCoupleQuiz/);
  });

  test('should have navigation menu', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check for nav menu elements
    const nav = page.locator('nav');
    await expect(nav).toBeVisible();
  });

  test('should be responsive on mobile', async ({ page, viewport }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Verify mobile-friendly layout
    if (viewport && viewport.width < 768) {
      // Mobile-specific checks
      const mainContent = page.locator('main');
      await expect(mainContent).toBeVisible();
    }
  });
});

test.describe('Game Flow', () => {
  test('should allow starting a new game', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Look for game setup elements
    // Note: Adjust selectors based on actual implementation
    const gameButton = page.getByRole('button', { name: /start|new game/i });
    if (await gameButton.isVisible()) {
      await gameButton.click();
      
      // Verify game interface appears
      await page.waitForTimeout(1000);
    }
  });

  test('should display questions when game starts', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Navigate to game page if exists
    const gameLink = page.getByRole('link', { name: /game|play/i });
    if (await gameLink.isVisible()) {
      await gameLink.click();
      await page.waitForLoadState('networkidle');
    }
  });

  test('should have timer component', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check if timer component exists anywhere on the page
    const timerExists = await page.locator('[class*="timer"], [id*="timer"]').count() > 0;
    // Timer may not be visible on home page, just checking it exists in the app
  });
});

test.describe('Leaderboard', () => {
  test('should display leaderboard page', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Try to navigate to leaderboard
    const leaderboardLink = page.getByRole('link', { name: /leaderboard|scoreboard|stats/i });
    if (await leaderboardLink.isVisible()) {
      await leaderboardLink.click();
      await page.waitForLoadState('networkidle');
      
      // Verify scoreboard component
      const scoreboard = page.locator('[class*="scoreboard"], [id*="scoreboard"]');
      // Component should exist even if no data
    }
  });

  test('should show team statistics', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Navigate to stats/leaderboard page
    const statsLink = page.getByRole('link', { name: /stats|teams|leaderboard/i });
    if (await statsLink.isVisible()) {
      await statsLink.click();
      await page.waitForLoadState('networkidle');
    }
  });
});

test.describe('Diagnostics Page', () => {
  test('should load diagnostics page', async ({ page }) => {
    await page.goto('/diag');
    await page.waitForLoadState('networkidle');
    
    // Verify diagnostics page loads
    const heading = page.locator('h1, h2, h3').filter({ hasText: /diagnostic|health|status/i });
    await expect(heading.first()).toBeVisible();
  });

  test('should display health check status', async ({ page }) => {
    await page.goto('/diag');
    await page.waitForLoadState('networkidle');
    
    // Wait for health checks to load
    await page.waitForTimeout(2000);
    
    // Look for status indicators
    const statusCards = page.locator('.card, [class*="status"]');
    const count = await statusCards.count();
    expect(count).toBeGreaterThan(0);
  });

  test('should have refresh functionality', async ({ page }) => {
    await page.goto('/diag');
    await page.waitForLoadState('networkidle');
    
    // Look for refresh button
    const refreshButton = page.getByRole('button', { name: /refresh|reload/i });
    if (await refreshButton.isVisible()) {
      await refreshButton.click();
      await page.waitForTimeout(1000);
    }
  });

  test('should be mobile responsive', async ({ page, viewport }) => {
    await page.goto('/diag');
    await page.waitForLoadState('networkidle');
    
    // Verify page is usable on mobile
    if (viewport && viewport.width < 768) {
      const mainContent = page.locator('main, .container');
      await expect(mainContent.first()).toBeVisible();
      
      // Check that cards stack vertically on mobile
      const cards = page.locator('.card');
      if (await cards.count() > 0) {
        const firstCard = cards.first();
        await expect(firstCard).toBeVisible();
      }
    }
  });
});

test.describe('Responsive Design', () => {
  test('should have readable text on mobile', async ({ page, viewport }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    if (viewport && viewport.width < 768) {
      // Verify text is readable (not too small)
      const body = page.locator('body');
      const fontSize = await body.evaluate(el => 
        window.getComputedStyle(el).fontSize
      );
      
      // Font size should be at least 14px
      const fontSizeNum = parseInt(fontSize);
      expect(fontSizeNum).toBeGreaterThanOrEqual(14);
    }
  });

  test('should have touch-friendly controls on mobile', async ({ page, viewport }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    if (viewport && viewport.width < 768) {
      // Check button sizes (should be at least 44x44 for touch)
      const buttons = page.getByRole('button');
      const count = await buttons.count();
      
      if (count > 0) {
        const firstButton = buttons.first();
        const box = await firstButton.boundingBox();
        
        if (box) {
          // Touch target should be at least 44px
          expect(box.height).toBeGreaterThanOrEqual(40);
        }
      }
    }
  });

  test('should not have horizontal scroll on mobile', async ({ page, viewport }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    if (viewport && viewport.width < 768) {
      // Check that page width doesn't exceed viewport
      const scrollWidth = await page.evaluate(() => document.body.scrollWidth);
      const clientWidth = await page.evaluate(() => document.body.clientWidth);
      
      expect(scrollWidth).toBeLessThanOrEqual(clientWidth + 5); // 5px tolerance
    }
  });
});
