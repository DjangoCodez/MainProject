import { BasePage } from '../../ui/pages/base.page.ts';
import { test, expect } from '../../utils/main-fixture.ts';
import { UserPage } from '../../ui/pages/manage/user.page.ts';

test.use({
	account: {
		userName: 'seleniumsys237',
		password: 'Summer2022',
		domain: 's1d1'
	}
});
test('go to the employees view', async ({ page }) => {
	const userPage = new BasePage(page, 'soe/manage/users');
	await userPage.navigateTo();
	
});
test('go to employees view and create new', async ({ page }) => {
	const firstName: string = 'Markus';
	const lastName: string = 'Lord';

	const address: string = 'Drottninggatan 33';
	const coAddress: string = '1121';
	const zipCode: string = '121234';
	const city: string = 'Stockholm'
	const country: string = 'Sweden'


	const userPage = new UserPage(page);
	await userPage.navigateTo();
	await userPage.addNew();
	await userPage.setFirstName(firstName);
	await userPage.setLastName(lastName);
	await userPage.openUserInformation();
	await userPage.addDeliveryAddress(address, coAddress, zipCode, city, country);

});
