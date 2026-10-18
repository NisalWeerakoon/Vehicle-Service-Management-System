const { test, expect } = require('@playwright/test');

test('Vehicle Service Management - User Journey', async ({ page }) => {
  // 1. Visit Login page on the local Vite dev server
  await page.goto('http://localhost:5173/login');
  await expect(page.locator('#email')).toBeVisible();

  // 2. Register a new customer user journey
  const uniqueId = Date.now();
  const testEmail = `customer_${uniqueId}@service.com`;
  const testPassword = 'Password123!';

  await page.click('text=Create an account');
  await expect(page).toHaveURL(/.*register/);

  // Fill in registration fields
  await page.fill('#fullName', 'Jane Doe');
  await page.fill('#email', testEmail);
  await page.fill('#phone', '0771234567');
  await page.fill('#password', testPassword);
  await page.fill('#confirmPassword', testPassword);
  await page.click('button[type="submit"]');

  // 3. User is automatically logged in and navigated to customer portal
  await expect(page).toHaveURL(/.*profile/, { timeout: 15000 });

  // 4. Navigate to Vehicles section
  await page.click('button:has-text("Vehicles")');
  await expect(page).toHaveURL(/.*vehicles/);

  // 5. Open Add Vehicle page
  await page.click('button:has-text("+ Add Vehicle")');
  await expect(page).toHaveURL(/.*vehicles\/add/);

  // 6. Fill vehicle details
  const regNumber = `CAB-${Math.floor(1000 + Math.random() * 9000)}`;
  await page.fill('#registrationNumber', regNumber);
  await page.fill('#make', 'Toyota');
  await page.fill('#model', 'Prius');
  await page.fill('#year', '2022');
  await page.selectOption('#fuelType', 'Hybrid');
  await page.click('button[type="submit"]');

  // 7. Verify redirected to /vehicles and new vehicle registration number is displayed
  await expect(page).toHaveURL(/.*vehicles/, { timeout: 15000 });
  await expect(page.locator('body')).toContainText(regNumber);
});
