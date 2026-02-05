import { test as baseTest, expect } from '@playwright/test';
import { LoginPage } from '../../pages/basic/LoginPage';
import { AccontPage } from '../../pages/basic/AccountPage';
import { WelcomePage } from '../../pages/basic/WelcomePage';
import fs from 'fs';
import path from 'path';

export * from '@playwright/test';
export const test = baseTest.extend<{}, { workerStorageState: string }>({
  // Use the same storage state for all tests in this worker.
  storageState: ({ workerStorageState }, use) => use(workerStorageState),

  // Authenticate once per worker with a worker-scoped fixture.
  workerStorageState: [async ({ browser }, use) => {
    // Use parallelIndex as a unique identifier for each worker.
    const id = test.info().parallelIndex;
    //const fileName = path.resolve(test.info().project.outputDir, `.auth/${id}.json`);
    const fileName = '.auth/admin${id}.json';

    if (fs.existsSync(fileName)) {
      // Reuse existing authentication state if any.
      await use(fileName);
      return;
    }

    // Important: make sure we authenticate in a clean environment by unsetting storage state.
    const page = await browser.newPage({ storageState: undefined });

    // End of authentication steps.
    test.setTimeout(150000);
    let url = process.env.URL ?? "";
    await page.goto(url);
    let loginPage = new LoginPage(page);
    await loginPage.Login(process.env.admin_username!, process.env.admin_password!);
    let accountPage = new AccontPage(page);
    await accountPage.selectAccount(process.env.domain_name!);

    const element = await page.locator('#ctl00_ctl00_baseMasterBody_soeTopMenu_UserSelector_Container');
    await element.locator('xpath=//a').first().click();
    // const switchLanguage = await element.locator('xpath=//ul//li/a').filter({ hasText: 'language' } );
    await element.locator('xpath=/ul//li[4]/a').first().textContent().then(option => {
      if (!option?.includes('language')) {
        element.locator('xpath=/ul//li[4]/a').first().click();
        page.getByRole('link', { name: 'En' }).or(page.getByRole('link', { name: 'en' })).click();
      }
    });



    let currentUrl: string = page.url();
    if (currentUrl.includes('soe/default.asp')) {
      let welcomePage = new WelcomePage(page);
      await welcomePage.selectModule('Sales');
    }


    // let welcomePage = new WelcomePage(page);
    // await welcomePage.selectModule('Sales');

    await page.waitForTimeout(3000);

    await page.locator('#ActiveHeader').textContent().then(name => {
      if (!name?.includes('Sales')) {
        page.locator('#ActiveHeader').click();
        page.locator('#billing').click();
        page.getByRole('link', { name: 'Sales' })
      }
    });
    await page.waitForTimeout(5000);

    await page.context().storageState({ path: fileName });
    await page.close();
    await use(fileName);
  }, { scope: 'worker' }],
});


