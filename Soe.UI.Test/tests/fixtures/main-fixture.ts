import { test as base } from '@playwright/test';
import { LoginPage } from '../../pages/basic/LoginPage';
import { AccontPage } from '../../pages/basic/AccountPage';
import { getAccountValue, getDomainValue } from '../../utils/properties';
import fs from 'fs';
import { Account, AccountExended, defaultAccount, get_storage_state, setDefault } from './accounts';

export * from '@playwright/test';

export const test = base.extend<{
	account: Account;
	accountEx: AccountExended;
	workerStorageState: string;
}>({
	account: defaultAccount,
	accountEx: async ({ account }, use) => {
		use( {
				...account,
				baseUrl: getDomainValue(account, 'url')?.toString() ?? '',
				storageState: get_storage_state(getAccountValue(account, 'username')?.toString() ?? '' + test.info().parallelIndex),
				loginUrl: process.env.URL ?? ''
			})
	},
	storageState: async ({ workerStorageState }, use) =>
		use(workerStorageState),
	async workerStorageState({ account, browser, accountEx }, use) {
		const filename = accountEx.storageState;
		if (fs.existsSync(accountEx.storageState)) {
			console.log('Using already stored auth of ' + filename);
			await use(filename);
			return;
		}
		const page = await browser.newPage({ storageState: undefined });
		let url = accountEx.loginUrl ?? "";
		console.log('Url ' + url)
		await page.goto(url);
		let loginPage = new LoginPage(page);
		await page.waitForTimeout(3000);
		await loginPage.Login(getAccountValue(account, 'username')!, getAccountValue(account, 'password')!);
		await page.waitForTimeout(5000);
		let accountPage = new AccontPage(page);
		await accountPage.selectAccount(getDomainValue(account, 'name')!);
		await page.locator('#information-menu-toggle').isEnabled({ timeout: 800000 });
		await page.context().storageState({ path: filename });
		console.log('Stored auth in ' + filename);
		await page.close();
		await use(filename);
	}
});
