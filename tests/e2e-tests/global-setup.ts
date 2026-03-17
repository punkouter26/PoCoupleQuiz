/**
 * Playwright global setup — acquires a dev_user cookie from the server's
 * /dev-login endpoint and saves browser state to storageState.json.
 *
 * This replaces Microsoft OAuth for E2E tests.  The cookie is accepted by the
 * server's DevAuthHandler when  ASPNETCORE_ENVIRONMENT=Development.
 */
import { chromium, FullConfig } from '@playwright/test';
import path from 'path';

const BASE_URL  = process.env.TEST_BASE_URL ?? 'http://127.0.0.1:5000';
const TEST_USER = process.env.TEST_USER_NAME ?? 'PlaywrightUser';
const STATE_FILE = path.join(__dirname, 'storageState.json');

export default async function globalSetup(_config: FullConfig): Promise<void> {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ ignoreHTTPSErrors: true });
  const page    = await context.newPage();

  // Navigate to /?user=X — TestAuthStateProvider reads the URL param,
  // POSTs to /api/dev/set-identity which sets the dev_user cookie, then notifies auth state.
  // This mirrors how a real tester opens a second browser window with a different user.
  await page.goto(`${BASE_URL}/?user=${encodeURIComponent(TEST_USER)}`, {
    waitUntil: 'networkidle',
  });

  // Give the WASM auth provider a moment to POST /api/dev/set-identity and set the cookie
  await page.waitForTimeout(1500);

  const title = await page.title().catch(() => '');
  console.log(`[global-setup] Page title after identity setup: "${title}"`);

  // Persist cookies (including dev_user) + localStorage for all tests
  await context.storageState({ path: STATE_FILE });
  console.log(`[global-setup] Auth state saved to ${STATE_FILE}`);

  await browser.close();
}
