import { type Locator, type Page } from '@playwright/test';
import { BasePage } from '../base.page';
import { AgGridComponent } from '../../components/ag-grid.component';
import { ListAndFormPage } from '../list-and-form.page';
import { test, expect } from '../../../utils/main-fixture.ts';

export class UserPage extends ListAndFormPage {
	readonly firstNameField: Locator;
	readonly lastNameField: Locator;
	readonly contactDetailsParent: Locator;
	readonly contactDetailsAccordion: Locator;
	readonly addAddressButton: Locator;
	readonly deliveryAddressChoice: Locator;
	readonly addressField: Locator;
	readonly coField: Locator;
	readonly zipcodeField: Locator;
	readonly cityField: Locator;
	readonly countryField: Locator;
	readonly contactPopUp: Locator;
	readonly contactSelectButton: Locator;
	readonly contactCancelButton: Locator;


	constructor(page: Page) {
		super(page, 'soe/manage/users');

		this.firstNameField = this.page.locator('#ctrl_user_firstName');
		this.lastNameField = this.page.locator('#ctrl_user_lastName');
		this.contactDetailsParent = this.page.locator('[label-key="manage.user.user.contactinfo"].ng-scope.ng-isolate-scope');
		this.contactDetailsAccordion = this.contactDetailsParent.locator('.panel-heading');
		this.addAddressButton = this.contactDetailsParent.locator('[options="ctrl.addressTypes"]');
		this.deliveryAddressChoice = this.addAddressButton.locator('.fa-fw.fal.fa-fw.fa-mailbox');
		this.addressField = this.page.locator('#ctrl_selectedAddress_address');
		this.coField = this.page.locator('#ctrl_selectedAddress_addressCO');
		this.zipcodeField = this.page.locator('#ctrl_selectedAddress_postalCode');
		this.cityField = this.page.locator('#ctrl_selectedAddress_postalAddress');
		this.countryField = this.page.locator('#ctrl_selectedAddress_country');
		this.contactPopUp = this.page.locator('#ctrl_selectedAddress_name');
		this.contactSelectButton = this.page.locator('.btn.btn-primary');
		this.contactCancelButton = this.page.locator('.btn.btn-default');



	}

	async setFirstName(input: string) {
		await this.setInput(this.firstNameField, input);
	}

	async setLastName(input: string) {
		await this.setInput(this.lastNameField, input);
	}

	async openUserInformation() {
		await this.openAccordion(this.contactDetailsAccordion);

	}
	async closeUserInformation() {
		await this.closeAccordion(this.contactDetailsAccordion);
	}
	async addDeliveryAddress(address?: string, coAddress?: string, zipCode?: string, city?: string, country?: string) {
		await expect(this.addAddressButton).toBeEnabled();
		await this.addAddressButton.click();
		await expect(this.deliveryAddressChoice).toBeVisible();
		await this.deliveryAddressChoice.click();
		await expect(this.contactPopUp).toBeVisible();
		await this.setInput(this.addressField, address || '');
		await this.setInput(this.coField, coAddress || '');
		await this.setInput(this.zipcodeField, zipCode || '');
		await this.setInput(this.cityField, city || '');
		await this.setInput(this.countryField, country || '');
		await this.contactSelectButton.click();
	}
}