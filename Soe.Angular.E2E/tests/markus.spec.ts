import { BasePage } from '../ui/pages/base.page';
import { test, expect } from '../utils/main-fixture';

test.use({
	account: {
		userName: 'seleniumsys564',
		password: 'Summer2022',
		domain: 's1d1'
	}
});
/*
test("Open homepage and verify title", async ({page}) => {
    await page.goto('https://gotest.softone.se/account/login');

    await expect(page).toHaveTitle('SoftOne - Log in');
})


test("Verify SoftOne logo and log-in text is visible", async ({page}) => {
    await page.goto('https://gotest.softone.se/account/login');

    const logo = page.locator('[src="/images/logo_go_dark_red.svg"]')
    const logintext = page.locator('[class="header-text"]')
    await expect(logintext).toBeVisible();
    await expect(logo).toBeVisible();
    await expect(logintext).toContainText('Login')
})


*/

