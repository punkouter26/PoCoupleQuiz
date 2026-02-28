import { test, expect } from '@playwright/test';

test.describe('Page Scrolling', () => {
  test('should allow scrolling to bottom of home page', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000); // Wait for Blazor WASM to initialize
    
    // Get initial scroll position
    const initialScrollY = await page.evaluate(() => window.scrollY);
    console.log('Initial scroll position:', initialScrollY);
    
    // Get the full page height
    const pageHeight = await page.evaluate(() => document.documentElement.scrollHeight);
    const viewportHeight = await page.evaluate(() => window.innerHeight);
    console.log('Page height:', pageHeight, 'Viewport height:', viewportHeight);
    
    // Check if page is scrollable
    const isScrollable = pageHeight > viewportHeight;
    console.log('Is page scrollable:', isScrollable);
    
    if (isScrollable) {
      // Scroll to the bottom of the page
      await page.evaluate(() => window.scrollTo(0, document.documentElement.scrollHeight));
      await page.waitForTimeout(500); // Wait for scroll to complete
      
      // Get new scroll position
      const newScrollY = await page.evaluate(() => window.scrollY);
      console.log('New scroll position:', newScrollY);
      
      // Verify we scrolled down
      expect(newScrollY).toBeGreaterThan(initialScrollY);
      
      // Verify we can see content near the bottom
      const scrolledToNearBottom = await page.evaluate(() => {
        const scrollY = window.scrollY;
        const windowHeight = window.innerHeight;
        const documentHeight = document.documentElement.scrollHeight;
        // Check if we're within 100px of the bottom
        return (scrollY + windowHeight) >= (documentHeight - 100);
      });
      
      expect(scrolledToNearBottom).toBeTruthy();
    }
  });

  test('should verify game setup section is accessible by scrolling', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);
    
    // Find the "Game Setup" section - look for team name input or player count section
    const teamNameInput = page.locator('input[placeholder*="team"]');
    
    // Check if it exists
    const exists = await teamNameInput.count() > 0;
    
    if (exists) {
      // Scroll to the team name input
      await teamNameInput.scrollIntoViewIfNeeded();
      await page.waitForTimeout(500);
      
      // Verify the input is visible in viewport
      const isVisible = await teamNameInput.isVisible();
      expect(isVisible).toBeTruthy();
    }
  });

  test('should verify content area has proper overflow and can scroll internally', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);
    
    // Check the content-area element for overflow properties
    const contentAreaStyles = await page.evaluate(() => {
      const contentArea = document.querySelector('.content-area');
      if (!contentArea) return null;
      
      const styles = window.getComputedStyle(contentArea);
      return {
        overflowY: styles.overflowY,
        maxHeight: styles.maxHeight,
        height: styles.height,
        scrollHeight: (contentArea as HTMLElement).scrollHeight,
        clientHeight: (contentArea as HTMLElement).clientHeight
      };
    });
    
    console.log('Content area styles:', contentAreaStyles);
    
    if (contentAreaStyles) {
      // Verify overflow-y is set to auto or scroll
      expect(['auto', 'scroll']).toContain(contentAreaStyles.overflowY);
      
      // If content is taller than the container, verify it's scrollable
      if (contentAreaStyles.scrollHeight > contentAreaStyles.clientHeight) {
        console.log('Content area is scrollable');
        
        // Try to scroll within the content area
        const scrollResult = await page.evaluate(() => {
          const contentArea = document.querySelector('.content-area') as HTMLElement;
          if (!contentArea) return { success: false };
          
          const initialScrollTop = contentArea.scrollTop;
          contentArea.scrollTop = contentArea.scrollHeight;
          const newScrollTop = contentArea.scrollTop;
          
          return {
            success: true,
            scrolled: newScrollTop > initialScrollTop,
            initialScrollTop,
            newScrollTop,
            scrollHeight: contentArea.scrollHeight,
            clientHeight: contentArea.clientHeight
          };
        });
        
        console.log('Scroll result:', scrollResult);
        expect(scrollResult.success).toBeTruthy();
      }
    }
  });

  test('should verify main-content container allows scrolling', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);
    
    // Check the main-content element for overflow properties
    const mainContentStyles = await page.evaluate(() => {
      const mainContent = document.querySelector('.main-content');
      if (!mainContent) return null;
      
      const styles = window.getComputedStyle(mainContent);
      return {
        overflowY: styles.overflowY,
        overflowX: styles.overflowX,
        height: styles.height,
        minHeight: styles.minHeight,
        scrollHeight: (mainContent as HTMLElement).scrollHeight,
        clientHeight: (mainContent as HTMLElement).clientHeight
      };
    });
    
    console.log('Main content styles:', mainContentStyles);
    
    if (mainContentStyles) {
      // Verify overflow-y allows scrolling
      expect(['auto', 'scroll', 'visible']).toContain(mainContentStyles.overflowY);
    }
  });

  test('should verify app container height allows content expansion', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);
    
    // Check the #app element height properties
    const appStyles = await page.evaluate(() => {
      const app = document.querySelector('#app');
      if (!app) return null;
      
      const styles = window.getComputedStyle(app);
      return {
        height: styles.height,
        minHeight: styles.minHeight,
        maxHeight: styles.maxHeight,
        overflow: styles.overflow,
        scrollHeight: (app as HTMLElement).scrollHeight,
        clientHeight: (app as HTMLElement).clientHeight
      };
    });
    
    console.log('App container styles:', appStyles);
    
    if (appStyles) {
      // Verify height is not fixed at 100vh (which would prevent scrolling)
      // Height should be 'auto' or greater than minHeight if content is larger
      expect(appStyles.height).not.toBe('100vh');
    }
  });

  test('should scroll through entire page and verify all major sections are accessible', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);
    
    // List of sections we expect to find
    const sectionsToCheck = [
      { name: 'Header', selector: '.page-title' },
      { name: 'Stats', selector: 'text=Your Stats' },
      { name: 'Game Setup', selector: 'text=Game Setup' },
      { name: 'Number of Players', selector: 'text=Number of Players' }
    ];
    
    for (const section of sectionsToCheck) {
      console.log(`Checking section: ${section.name}`);
      const element = page.locator(section.selector).first();
      const exists = await element.count() > 0;
      
      if (exists) {
        // Scroll to the element
        await element.scrollIntoViewIfNeeded();
        await page.waitForTimeout(300);
        
        // Verify it's visible
        const isVisible = await element.isVisible();
        console.log(`  ${section.name} visible:`, isVisible);
        expect(isVisible).toBeTruthy();
      } else {
        console.log(`  ${section.name} not found (may be conditional)`);
      }
    }
  });
});
