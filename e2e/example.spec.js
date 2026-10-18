// @ts-check
import { test, expect } from '@playwright/test';

test('login page loads successfully', async ({ page }) => {
  await page.goto('http://localhost:5173/login');
  await expect(page.locator('#email')).toBeVisible();
  await expect(page.locator('#password')).toBeVisible();
});
