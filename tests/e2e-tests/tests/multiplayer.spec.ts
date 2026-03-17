/**
 * 2-Player Multiplayer E2E Tests
 *
 * These tests verify the full real-time multiplayer flow using two separate
 * browser contexts (simulating two distinct browser windows / incognito sessions).
 *
 * Identity is established via the ?user= URL parameter (TestAuthStateProvider),
 * which silently POSTs to /api/dev/set-identity on the server.  Using separate
 * browser contexts ensures each player has an isolated cookie jar — exactly like
 * two real users on different machines.
 *
 * Test coverage:
 *   1. Host can create a lobby and a 4–6-char game code is displayed
 *   2. Second player can join using that code (real-time SignalR update visible in P1's view)
 *   3. Start button is gated on ≥ 2 players
 *   4. Full happy-path: lobby → join → start → answer round → results
 *   5. Error shown when joining with an invalid code
 */

import { test, expect, Page } from '@playwright/test';

const BASE_URL = process.env.TEST_BASE_URL ?? 'http://127.0.0.1:5000';

// ─── Shared helpers ────────────────────────────────────────────────────────

/**
 * Wait for Blazor WASM to finish bootstrapping.
 * Polls until the loading indicator disappears.
 */
async function waitForBlazorReady(page: Page, timeout = 60_000): Promise<void> {
  await page.waitForLoadState('networkidle', { timeout }).catch(() => {});
  const loader = page.locator('.loading-progress, .loading-progress-text');
  await loader.first().waitFor({ state: 'hidden', timeout }).catch(() => {});
  // Extra render tick for Blazor state subscribers to settle
  await page.waitForTimeout(500);
}

/**
 * Navigate to /?user=<name> so TestAuthStateProvider:
 *   1. Reads the query param client-side
 *   2. POSTs /api/dev/set-identity?name=<name>  →  server sets dev_user cookie
 *   3. Updates Blazor auth state (identity is immediately available)
 *
 * Using incognito / a new BrowserContext means the cookie is isolated to that
 * session — two contexts = two independent player identities.
 */
async function authenticateAs(page: Page, userName: string): Promise<void> {
  await page.goto(`${BASE_URL}/?user=${encodeURIComponent(userName)}`, {
    waitUntil: 'networkidle',
  });
  await waitForBlazorReady(page);
  // Allow WASM auth provider to complete the /api/dev/set-identity POST
  await page.waitForTimeout(1500);
}

/**
 * Click the "2 Players" mode card on the home page and wait for the
 * name / difficulty / action form to appear.
 */
async function selectTwoPlayerMode(page: Page): Promise<void> {
  await page.locator('.mode-card', { hasText: '2 Players' }).click();
  await page
    .locator('input.form-control[placeholder="Enter your name"]')
    .waitFor({ state: 'visible', timeout: 10_000 });
}

// ─── Override storageState so every test starts with a clean, anonymous session.
// (The global storageState.json comes from global-setup and has dev_user=PlaywrightUser;
// multiplayer tests must each set their own distinct user identity.)
test.use({ storageState: { cookies: [], origins: [] } });

// ─── Test suite ────────────────────────────────────────────────────────────

test.describe('2-Player Multiplayer', () => {

  // ── Test 1 ──────────────────────────────────────────────────────────────
  test('host creates lobby — game code shown and start button is disabled', async ({ page }) => {
    await authenticateAs(page, 'HostOnly');
    await selectTwoPlayerMode(page);

    const nameInput = page.locator('input.form-control[placeholder="Enter your name"]');
    await nameInput.fill('HostOnly');
    await page.locator('.difficulty-option', { hasText: 'Easy' }).click();
    await page.locator('button.action-tab', { hasText: 'New Game' }).click();
    await page.locator('button.btn-primary').click();

    // Should navigate to /lobby/{code}
    await page.waitForURL('**/lobby/**', { timeout: 15_000 });

    // Game code is a 4–6 char alphanumeric string
    const codeDisplay = page.locator('.code-value');
    await expect(codeDisplay).toBeVisible({ timeout: 15_000 });
    const code = (await codeDisplay.textContent())?.trim() ?? '';
    expect(code, 'Game code should be 4–6 uppercase characters').toMatch(/^[A-Z0-9]{4,6}$/);

    // Host's name appears in the player list
    await expect(page.locator('.player-item', { hasText: 'HostOnly' })).toBeVisible();

    // Start button exists but is disabled — only 1 player so far
    await expect(page.locator('button.btn-start')).toBeDisabled();
  });

  // ── Test 2 ──────────────────────────────────────────────────────────────
  test('start button is disabled until a second player joins', async ({ page }) => {
    await authenticateAs(page, 'LonelyHost');
    await selectTwoPlayerMode(page);
    await page.locator('input.form-control[placeholder="Enter your name"]').fill('LonelyHost');
    await page.locator('.difficulty-option', { hasText: 'Easy' }).click();
    await page.locator('button.action-tab', { hasText: 'New Game' }).click();
    await page.locator('button.btn-primary').click();

    await page.waitForURL('**/lobby/**', { timeout: 15_000 });

    // Button stays disabled with a single player
    await expect(page.locator('button.btn-start')).toBeDisabled();

    // Waiting hint tells the host to invite more players
    await expect(page.locator('.waiting-hint')).toBeVisible();
    await expect(page.locator('.waiting-hint')).toContainText('Waiting', { ignoreCase: true });
  });

  // ── Test 3 ──────────────────────────────────────────────────────────────
  test('second player joins lobby — both players visible in real-time, start enabled', async ({ page, browser }) => {
    // -- Player 1: create lobby --
    await authenticateAs(page, 'P1Host');
    await selectTwoPlayerMode(page);
    await page.locator('input.form-control[placeholder="Enter your name"]').fill('P1Host');
    await page.locator('.difficulty-option', { hasText: 'Easy' }).click();
    await page.locator('button.action-tab', { hasText: 'New Game' }).click();
    await page.locator('button.btn-primary').click();
    await page.waitForURL('**/lobby/**', { timeout: 15_000 });

    const codeDisplay = page.locator('.code-value');
    await codeDisplay.waitFor({ state: 'visible', timeout: 15_000 });
    const gameCode = (await codeDisplay.textContent())?.trim() ?? '';

    // -- Player 2: join in a separate browser context (isolated cookie jar) --
    const ctx2 = await browser.newContext({ ignoreHTTPSErrors: true });
    const page2 = await ctx2.newPage();

    try {
      await authenticateAs(page2, 'P2Joiner');
      await selectTwoPlayerMode(page2);
      await page2.locator('input.form-control[placeholder="Enter your name"]').fill('P2Joiner');
      await page2.locator('button.action-tab', { hasText: 'Join Game' }).click();

      const codeInput = page2.locator('input.form-control.code-input');
      await codeInput.waitFor({ state: 'visible' });
      await codeInput.fill(gameCode);
      await page2.locator('button.btn-primary').click();

      // Player 2 lands in the correct lobby
      await page2.waitForURL(`**/${gameCode}**`, { timeout: 15_000 });
      await expect(page2.locator('.player-item', { hasText: 'P2Joiner' })).toBeVisible({ timeout: 15_000 });

      // Player 1's lobby updates via SignalR — both names visible
      await expect(page.locator('.player-item', { hasText: 'P1Host' })).toBeVisible({ timeout: 10_000 });
      await expect(page.locator('.player-item', { hasText: 'P2Joiner' })).toBeVisible({ timeout: 15_000 });

      // Player count badge should show 2
      await expect(page.locator('.section-title')).toContainText('2', { timeout: 10_000 });

      // Start button is now enabled for the host
      await expect(page.locator('button.btn-start')).toBeEnabled({ timeout: 10_000 });
      await expect(page.locator('button.btn-start')).toContainText('2 players');

      // Joiner's view: start button is NOT shown (only host sees it)
      await expect(page2.locator('button.btn-start')).not.toBeVisible();
    } finally {
      await ctx2.close();
    }
  });

  // ── Test 4 ──────────────────────────────────────────────────────────────
  test('error shown when joining with a non-existent game code', async ({ page }) => {
    await authenticateAs(page, 'BadCodePlayer');
    await selectTwoPlayerMode(page);
    await page.locator('input.form-control[placeholder="Enter your name"]').fill('BadCodePlayer');
    await page.locator('button.action-tab', { hasText: 'Join Game' }).click();

    const codeInput = page.locator('input.form-control.code-input');
    await codeInput.waitFor({ state: 'visible' });
    await codeInput.fill('ZZZZ'); // Definitely does not exist
    await page.locator('button.btn-primary').click();

    // An error message should appear on the same page
    await expect(page.locator('.alert-error')).toBeVisible({ timeout: 15_000 });

    // Navigation to a lobby must NOT have occurred
    await expect(page).not.toHaveURL('**/lobby/**');
  });

  // ── Test 5 ──────────────────────────────────────────────────────────────
  test('full 2-player game: create lobby → join → start → round 1 → results', async ({ page, browser }) => {
    // -- Player 1: create lobby --
    await authenticateAs(page, 'Gamer1');
    await selectTwoPlayerMode(page);
    await page.locator('input.form-control[placeholder="Enter your name"]').fill('Gamer1');
    await page.locator('.difficulty-option', { hasText: 'Easy' }).click();
    await page.locator('button.action-tab', { hasText: 'New Game' }).click();
    await page.locator('button.btn-primary').click();
    await page.waitForURL('**/lobby/**', { timeout: 15_000 });

    const codeDisplay = page.locator('.code-value');
    await codeDisplay.waitFor({ state: 'visible', timeout: 15_000 });
    const gameCode = (await codeDisplay.textContent())?.trim() ?? '';
    expect(gameCode).toBeTruthy();

    // -- Player 2: join --
    const ctx2 = await browser.newContext({ ignoreHTTPSErrors: true });
    const page2 = await ctx2.newPage();

    try {
      await authenticateAs(page2, 'Gamer2');
      await selectTwoPlayerMode(page2);
      await page2.locator('input.form-control[placeholder="Enter your name"]').fill('Gamer2');
      await page2.locator('button.action-tab', { hasText: 'Join Game' }).click();
      const codeInput = page2.locator('input.form-control.code-input');
      await codeInput.waitFor({ state: 'visible' });
      await codeInput.fill(gameCode);
      await page2.locator('button.btn-primary').click();
      await page2.waitForURL(`**/${gameCode}**`, { timeout: 15_000 });

      // Both players visible in P1's lobby; start button enabled
      await expect(page.locator('.player-item', { hasText: 'Gamer2' })).toBeVisible({ timeout: 15_000 });
      await expect(page.locator('button.btn-start')).toBeEnabled({ timeout: 10_000 });

      // -- Host starts the game --
      await page.locator('button.btn-start').click();

      // Both navigate to /game/{code}
      await page.waitForURL(`**/game/${gameCode}**`, { timeout: 20_000 });
      await page2.waitForURL(`**/game/${gameCode}**`, { timeout: 20_000 });

      // Both pages show Round 1
      await expect(page.locator('.round-badge')).toContainText('Round 1', { timeout: 20_000 });
      await expect(page2.locator('.round-badge')).toContainText('Round 1', { timeout: 20_000 });

      // Both see the question text
      await expect(page.locator('.question-text')).toBeVisible({ timeout: 20_000 });
      await expect(page2.locator('.question-text')).toBeVisible({ timeout: 20_000 });

      // Role pills are visible for both players
      await expect(page.locator('.role-pill')).toBeVisible({ timeout: 10_000 });
      await expect(page2.locator('.role-pill')).toBeVisible({ timeout: 10_000 });

      // Exactly one player should be the King; the other should be the Guesser
      const p1Role = (await page.locator('.role-pill').textContent()) ?? '';
      const p2Role = (await page2.locator('.role-pill').textContent()) ?? '';
      const p1IsKing = p1Role.includes('King');
      const p2IsKing = p2Role.includes('King');
      expect(
        p1IsKing !== p2IsKing,
        `Expected one King and one Guesser; got P1="${p1Role.trim()}" P2="${p2Role.trim()}"`
      ).toBe(true);

      // Answer textarea is visible (not yet submitted)
      await expect(page.locator('textarea.answer-input')).toBeVisible({ timeout: 10_000 });
      await expect(page2.locator('textarea.answer-input')).toBeVisible({ timeout: 10_000 });

      // -- Both players submit an answer --
      await page.locator('textarea.answer-input').fill('Gamer1 round-1 answer');
      await page.locator('button.btn-submit').click();

      // P1's waiting box appears immediately after submission
      await expect(page.locator('.waiting-box')).toBeVisible({ timeout: 10_000 });

      await page2.locator('textarea.answer-input').fill('Gamer2 round-1 answer');
      await page2.locator('button.btn-submit').click();

      // P2's waiting box appears
      await expect(page2.locator('.waiting-box')).toBeVisible({ timeout: 10_000 });

      // -- Results phase renders for both after all answers are in --
      // Waiting box disappears and .results-container with round results replaces it
      await expect(page.locator('.results-container')).toBeVisible({ timeout: 20_000 });
      await expect(page2.locator('.results-container')).toBeVisible({ timeout: 20_000 });

      // Results header confirms we're in the results phase
      await expect(page.locator('.results-header')).toContainText('Round Results', { timeout: 10_000 });
      await expect(page2.locator('.results-header')).toContainText('Round Results', { timeout: 10_000 });

      // The King's answer card is shown to both players
      await expect(page.locator('.king-answer-card')).toBeVisible();
      await expect(page2.locator('.king-answer-card')).toBeVisible();

      // "Next Round" button is present in results
      await expect(page.locator('.next-round-btn')).toBeVisible();
      await expect(page2.locator('.next-round-btn')).toBeVisible();

    } finally {
      await ctx2.close();
    }
  });

});
