const { test, expect } = require('@playwright/test');

test('Vehicle Service Management - User Journey', async ({ page }) => {
  const uniqueId = Date.now();
  const testEmail = `customer_${uniqueId}@service.com`;
  const testPassword = 'Password123!';
  const vehicles = [];

  await page.route('**/api/**', async (route) => {
    const request = route.request();
    const method = request.method();
    const url = request.url();

    if (url.includes('/api/auth/register') && method === 'POST') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          token: 'mock-token',
          userId: 'mock-user-id',
          email: testEmail,
          role: 'Customer',
        }),
      });
      return;
    }

    if (url.includes('/api/customers/me')) {
      await route.fulfill({
        status: method === 'POST' ? 201 : 200,
        contentType: 'application/json',
        body: JSON.stringify({
          fullName: 'Jane Doe',
          email: testEmail,
          phone: '0771234567',
        }),
      });
      return;
    }

    if (url.includes('/api/vehicles/me') && method === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(vehicles),
      });
      return;
    }

    if (url.includes('/api/vehicles/me') && method === 'POST') {
      const payload = request.postDataJSON();
      const createdVehicle = { id: String(vehicles.length + 1), ...payload };
      vehicles.push(createdVehicle);
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify(createdVehicle),
      });
      return;
    }

    if (url.includes('/api/notifications')) {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(url.includes('/unread-count') ? 0 : []),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([]),
    });
  });

  // 1. Visit Login page on the local Vite dev server
  await page.goto('http://localhost:5173/login');
  await expect(page.locator('#email')).toBeVisible();

  // 2. Register a new customer user journey

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
