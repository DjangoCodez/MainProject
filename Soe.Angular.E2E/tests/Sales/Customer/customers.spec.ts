// import { test, expect } from '@playwright/test';

// test.beforeEach( async ( { page })=> {
//   await page.goto('https://development.softone.se/soe/billing/customer/customers/default.aspx?c=7#!/');
// });

// test('go to the custmers view', async ({ page }) => {
//   await expect(page.getByText('Testkund android', { exact: true })).toBeVisible();
// });

// test('add new customer', async ({ page }) => {
//   await page.getByTitle('New customer').click();
//   await expect(page.getByText('Address / Number', { exact: true })).toBeVisible();
//   await page.getByLabel('Customer number').type('13377331');

//   await page.locator('#ctrl_customer_name').type('Playwright Test Customer 1');
//   await page.getByRole('button', { name: 'Save' }).click();
//   await expect(page.getByText('Customer 13377331', { exact: true })).toBeVisible();
//   await page.goto('https://development.softone.se/soe/billing/customer/customers/default.aspx?c=7#!/');
//   await expect(page.getByText('Testkund android', { exact: true })).toBeVisible();
//   await page.locator('#ag-132-input').type('13377331');
//   await expect(page.getByText('Playwright')).toBeVisible();
//   await page.getByText('Playwright').dblclick();
//   await expect(page.getByRole('button', {name: 'Delete'})).toBeVisible();
//   await page.getByRole('button', {name: 'Delete'}).click();
//   await expect(page.getByText('You are about to delete the record')).toBeVisible();
//   await page.getByRole('button', {name: 'OK'}).click();
//   await expect(page.getByText('Removed')).toBeVisible();
//   await page.getByRole('button', {name: 'OK'}).click();
//   await page.getByRole('link', { name: 'Customers' }).click();
//   await expect(page.getByText('Playwright')).not.toBeVisible();
// });
