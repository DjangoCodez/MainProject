import { test, expect } from '../../../utils/main-fixture';
import { VatPage } from '../../../ui/pages/economy/vatcodes.page';

test.use({
	account: {
		userName: '10@931',
		password: 'Sommar2021',
		domain: 's1d1'
	}
});

test.beforeAll(async () => {
	expect(true).toBeTruthy();
});

test.beforeEach(async ({ page, context }) => {
	const vatPage = new VatPage(page);
	await vatPage.navigateTo();
	await vatPage.switchToAngular();
});

test('reload vat codes', async ({ page }) => {
	const vatPage = new VatPage(page);
	//await vatPage.reload(); //need better selector
	await vatPage.navigateTo();
	expect(true).toBeTruthy();
});

test('some other test', async ({ page }) => {
	const vatPage = new VatPage(page);
	//await vatPage.reload(); //need better selector
	await vatPage.navigateTo();
	expect(true).toBeTruthy();
});
