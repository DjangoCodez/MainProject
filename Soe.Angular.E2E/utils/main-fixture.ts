import { test as base } from '@playwright/test';

import { FileUtils } from './file-utils';

import fs from 'fs';
import { Account, defaultAccount, setDefault } from './accounts';

export * from '@playwright/test';

export const test = base.extend<{
	account: Account;
	fileUtils: FileUtils;
	workerStorageState: string;
}>({
	account: defaultAccount,
	storageState: async ({ workerStorageState }, use) =>
		use(workerStorageState),
	async workerStorageState({ account, browser }, use) {
		const accountEx = setDefault(account);
		const filename = accountEx.storageState;

		if (fs.existsSync(accountEx.storageState)) {
			// context.storageState({ path: filename });
			await use(filename);
			return;
		}

		const baseURL = accountEx.baseUrl;

		const page = await browser.newPage({ storageState: undefined });
		await page.goto(accountEx.loginUrl);
		await page.locator('#login-username').fill(accountEx.userName);
		await page.locator('#login-password').fill(accountEx.password);
		await page.locator('#btn-login').click();
		//await page.locator('.input-group-icon-addon-left.fal.fa-fw fa-house').first().isVisible();
		await page.goto(
			accountEx.loginUrl + '/account/MultipleUsersRedirect?url=' + baseURL
		);
		await page.locator('#information-menu-toggle').isEnabled();
		await page.context().storageState({ path: filename });
		await page.close();
		await use(filename);
		console.log('Stored auth for ' + filename);
	}
});
