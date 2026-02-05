import { expect, type Page } from '@playwright/test';
import * as allure from "allure-js-commons";
import { GridPage } from '../../common/GridPage';
import { AngVersion } from '../../../enums/AngVersionEnums';
import { BasePage } from 'pages/common/BasePage';
import { SingleGridPageJS } from 'pages/common/SingleGridPageJS';
import { SectionGridPageJS } from 'pages/common/SectionGridPageJS';

export class CustomersPage extends BasePage {

    readonly page: Page;
    readonly customerGrid: GridPage;
    readonly customerGridJS: SingleGridPageJS;
    readonly ang_version: AngVersion;
    readonly staticsticGrid: SectionGridPageJS;
    readonly contactGrid: SectionGridPageJS;
    readonly conntactsPersonGridJS: SectionGridPageJS;
    readonly conntactsPersonGrid: GridPage;

    constructor(page: Page, ang_version: AngVersion = AngVersion.NEW) {
        super(page);
        this.page = page;
        this.customerGrid = new GridPage(page, 'Common.Customer.Customers');
        this.customerGridJS = new SingleGridPageJS(page);
        this.ang_version = ang_version;
        this.staticsticGrid = new SectionGridPageJS(page, 'common.statistics', 'ctrl.statisticsGrid.gridAg.options.gridOptions');
        this.contactGrid = new SectionGridPageJS(page, 'common.customer.customer.customer', 'ctrl.gridAg.options.gridOptions');
        this.conntactsPersonGridJS = new SectionGridPageJS(page, 'common.contactperson.contactpersons', 'ctrl.gridAg.options.gridOptions');
        this.conntactsPersonGrid = new GridPage(page, 'common.contactperson.contactpersons-container');


    }

    async setNumber(code: string) {
        await allure.step("Set customer number: " + code, async () => {
            if (this.ang_version === AngVersion.NEW) {
                const customerNr = this.page.locator("#ctrl_customer_customerNr")
                await expect(customerNr).toBeVisible();
                await customerNr.fill(code);
            } else {
                await this.page.locator('#ctrl_customer_customerNr').fill(code);
            }
        });
    }

    async addCustomerName(name: string) {
        await allure.step("Enter Customer name: " + name, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByTestId('name').click();
                await this.page.getByTestId('name').fill(name);
            } else {
                await this.page.getByRole('textbox', { name: 'Name', exact: true }).fill(name);
            }
        });
    }

    async addEmailContact(email: string) {
        await allure.step("Add Email Contact: " + email, async () => {
            await this.page.getByRole('button', { name: 'Add Contact' }).click();
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByText('Email address').click();
                await this.page.getByTestId('eComText').fill(email);
                await this.page.getByTestId('Contact').getByTestId('save').click();
            } else {
                await this.page.getByRole('link', { name: '   Email address' }).click();
                await this.page.getByRole('textbox', { name: 'Email Address' }).fill(email);
                await this.page.getByRole('button', { name: 'Select' }).click();
            }
        });
    }

    async addHomePhoneContact(homePhone: string) {
        await allure.step("Add Home Phone Contact: " + homePhone, async () => {
            await this.page.getByRole('button', { name: 'Add Contact' }).click();
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByText('Home phone').click();
                await this.page.getByTestId('eComText').fill(homePhone);
                await this.page.getByTestId('Contact').getByTestId('save').click();
            } else {
                await this.page.getByRole('link', { name: '   Home phone' }).click();
                await this.page.getByRole('textbox', { name: 'Address / Number' }).fill(homePhone);
                await this.page.getByRole('button', { name: 'Select' }).click();
            }
        });
    }

    async addMobilePhoneContact(mobilePhone: string) {
        await allure.step("Add Mobile Phone Contact: " + mobilePhone, async () => {
            await this.page.getByRole('button', { name: 'Add Contact' }).click();
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByTestId('common.customer.customer.customer-container').getByText('Mobile phone').click();
                await this.page.getByTestId('eComText').fill(mobilePhone);
                await this.page.getByTestId('Contact').getByTestId('save').click();
            } else {
                await this.page.getByRole('link', { name: '   Mobile phone' }).click();
                await this.page.getByRole('textbox', { name: 'Address / Number' }).fill(mobilePhone);
                await this.page.getByRole('button', { name: 'Select' }).click();
            }
        });
    }

    async addDeliveryAddress(address: string) {
        await allure.step("Add Delivery Address: " + address, async () => {
            await this.page.locator('//div[@options="ctrl.addressTypes"]/button').click();
            const addresses = this.page.locator('//div[@options="ctrl.addressTypes"]//a');
            await expect.poll(async () => { return addresses.count() }, { timeout: 10000 }).toBeGreaterThan(0);
            for (let i = 0; i < await addresses.count(); i++) {
                const addrText = await addresses.nth(i).innerText();
                if (addrText.includes('Delivery address')) {
                    await addresses.nth(i).click();
                    break;
                }
            }
            await this.page.locator('.modal-content').waitFor({ state: 'visible' });
            await this.page.locator('#ctrl_selectedAddress_address').fill(address);
            await this.page.getByRole('button', { name: 'Select' }).click();
        }
        );
    }

    async addAllContacts(email: string, homePhone: string, mobilePhone: string) {
        await allure.step("Add all contacts", async () => {
            await this.addEmailContact(email);
            await this.addHomePhoneContact(homePhone);
            await this.addMobilePhoneContact(mobilePhone);
        });
    }

    async addNote() {
        const noteText = 'This is a test note';
        await allure.step("Add Note: " + noteText, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByTestId('common.note').click();
                await this.page.getByTestId('note').fill(noteText);
            } else {
                await this.page.getByRole('button', { name: 'Note ' }).click();
                const noteTab = this.page.getByRole('tabpanel', { name: 'Note ' }).locator('#ctrl_customer_note');
                await noteTab.fill(noteText);
            }
        });
    }

    async save() {
        await allure.step('Save', async () => {
            if (this.ang_version === AngVersion.NEW) {
                const saveBtn = this.page.getByTestId('edit-footer').getByTestId('save');
                await saveBtn.scrollIntoViewIfNeeded();
                await saveBtn.click({ timeout: 5000 });
                await this.waitForDataLoad("/api/V2/Core/ContactPerson/");
            } else {
                const saveBtn = this.page.getByRole('button', { name: 'Save' });
                await saveBtn.scrollIntoViewIfNeeded();
                await saveBtn.click();
            }
        });
    }

    async verifyTabChanged(customerName: string, customerNumber?: string) {
        await allure.step('Verify Tab name changed after save', async () => {
            let tabLocator;
            if (this.ang_version === AngVersion.NEW) {
                tabLocator = this.page.locator(`//span[contains(normalize-space(text()), 'Customer ${customerName}')]`);
            } else {
                tabLocator = this.page.locator(`//label[contains(normalize-space(text()), 'Customer ${customerNumber}')]`);
            }
            await expect(tabLocator).toBeVisible({ timeout: 20000 });
        });
    }

    async closeTab() {
        await allure.step("Close tab", async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByTestId('tab-1-close').click();
            } else {
                await this.page.locator('i.removableTabIcon[title="Close"]').click();
            }
        });
    }

    async reloadPage() {
        await allure.step("Reload page", async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByTestId('reload').click();
            } else {
                await this.page.getByRole('toolbar').getByTitle('Reload records').click();
            }
        });
    }

    async verifyFilterByNumber(number: string) {
        await allure.step("Filter by customer number : " + number, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.customerGrid.filterByColumnNameAndValue('Number', number);
                await this.customerGrid.verifyFilteredItem(number);
            } else {
                await this.customerGridJS.filterByName('Number', number);
                const filteredCount = await this.customerGridJS.getFilteredAgGridRowCount();
                expect(filteredCount).toBeGreaterThan(0);
            }
        });
    }

    async verifyFilterByName(name: string) {
        await allure.step("Filter by customer name : " + name, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.customerGrid.filterByColumnNameAndValue('Name', name);
                await this.customerGrid.verifyFilteredItem(name);
            } else {
                await this.customerGridJS.filterByName('Name', name);
                const filteredCount = await this.customerGridJS.getFilteredAgGridRowCount();
                expect(filteredCount).toBeGreaterThan(0);
            }
        });
    }

    async waitForPageLoad(isEditMode: boolean = false) {
        await allure.step("Wait for Customers page load", async () => {
            if (this.ang_version === AngVersion.NEW) {
                if (isEditMode) {
                    await this.waitForDataLoad("/api/V2/Shared/Customer");
                } else {
                    await this.waitForDataLoad("/api/V2/Core/SysLanguage");
                }
            } else {
                await this.waitForDataLoad("/api/Core/Customer/Customer/");
            }
        });
    }

    async expandProductTab() {
        await allure.step("Expand purchase rows", async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByTestId('common.customer.customer.products').scrollIntoViewIfNeeded();
                await this.page.getByTestId('common.customer.customer.products').click();
            } else {
                await this.page.getByRole('button', { name: 'Product' }).scrollIntoViewIfNeeded();
                await this.page.getByRole('button', { name: 'Product' }).click();
            }
        });
    }

    async addNewProductRow() {
        await allure.step("Click add new product row", async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByTestId('plus').scrollIntoViewIfNeeded();
                await this.page.getByTestId('plus').click();
            } else {
                await this.page.getByTitle('New Product Row').scrollIntoViewIfNeeded();
                await this.page.getByTitle('New Product Row').click();
            }
        });
    }

    async addProductWithPrice(productName: string, productPrice: string): Promise<string> {
        if (this.ang_version === AngVersion.NEW) {
            await this.page.getByRole('option', { name: productName }).click();
            await this.page.getByRole('gridcell', { name: '0.00' }).click();
            const priceBox = this.page.getByTestId('Common.Customer.Customers.Directives.CustomerProducts').getByRole('textbox');
            await priceBox.fill(productPrice);
            await this.page.keyboard.press('Enter');
        } else {
            const productOption = this.page.getByRole('option', { name: productName });
            await productOption.waitFor({ state: 'visible', timeout: 10000 });
            await productOption.click();
            const priceCell = this.page.getByRole('gridcell', { name: '0,00' });
            const priceInput = priceCell.getByRole('textbox');
            await priceInput.fill(productPrice);
            await this.page.keyboard.press('Enter');
        }
        console.log(`Added product: ${productName} with price: ${productPrice}`);
        return productName;
    }

    async editFirstRow() {
        await allure.step("Edit first row in the grid", async () => {
            let firstRow;
            if (this.ang_version === AngVersion.NEW) {
                firstRow = this.page.locator('.ag-center-cols-container [role="row"]').first();
            } else {
                firstRow = this.page.locator('//div[@class="ag-center-cols-container"]//div[@role="row"]').first();
            }
            await firstRow.scrollIntoViewIfNeeded();
            await firstRow.waitFor({ state: 'visible' });
            await firstRow.dblclick();
        });
    }

    async removeProduct() {
        await allure.step("Remove product row", async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.locator("//fa-icon[@class='ng-fa-icon icon-delete link']//*[name()='svg']").scrollIntoViewIfNeeded();
                await this.page.locator("//fa-icon[@class='ng-fa-icon icon-delete link']//*[name()='svg']").click();
            } else {
                await this.page.locator("//button[@class='gridCellIcon fal fa-times iconDelete']").scrollIntoViewIfNeeded();
                await this.page.locator("//button[@class='gridCellIcon fal fa-times iconDelete']").click();
            }
        });
    }

    async expandStatisticsTab() {
        await allure.step("Expand Statistics tab", async () => {
            let statisticsBtn;
            if (this.ang_version === AngVersion.NEW) {
                statisticsBtn = this.page.getByTestId('common.statistics');
            } else {
                statisticsBtn = this.page.getByRole('button', { name: 'Statistics' });
            }
            await statisticsBtn.scrollIntoViewIfNeeded();
            await statisticsBtn.click();
        });
    }

    async searchOrdersInStatistics() {
        await allure.step("Search Orders in Statistics tab", async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.locator('#selection_').selectOption('1');
                await this.page.getByTestId('standard').click();
            } else {
                await this.page.getByRole('tabpanel', { name: 'Statistics' }).getByLabel('', { exact: true }).selectOption('number:1');
                await this.page.getByTitle('Search').click();
                await this.waitForDataLoad('/api/Core/Customer/Customer/Statistics/');
            }
        });
    }

    async verifyCreatedOrderRows(rows: { orderNumber: string; expectedPrice: string }[]) {
        await allure.step(`Verify created order rows`, async () => {
            const filteredCount = await this.staticsticGrid.getAgGridRowCount();
            expect(filteredCount).toBe(3);
            await this.staticsticGrid.filterByName('Invoice Number', rows[0].orderNumber);
            const invoiceNumber_1 = await this.staticsticGrid.getRowColumnValue('invoiceNr', 0);
            expect(invoiceNumber_1).toBe(rows[0].orderNumber);
            const type_1 = await this.staticsticGrid.getRowColumnValue('originType', 0);
            expect(type_1).toBe('Order');
            const price_1 = await this.staticsticGrid.getRowColumnValue('productPrice', 0);
            expect(price_1).toBe(rows[0].expectedPrice);

            await this.staticsticGrid.filterByName('Invoice Number', rows[1].orderNumber);
            const invoiceNumber_2 = await this.staticsticGrid.getRowColumnValue('invoiceNr', 0);
            expect(invoiceNumber_2).toBe(rows[1].orderNumber);
            const type_2 = await this.staticsticGrid.getRowColumnValue('originType', 0);
            expect(type_2).toBe('Order');
            const price_2 = await this.staticsticGrid.getRowColumnValue('productPrice', 0);
            const normalize = (val: string) => val.replace(/\s/g, '').trim();
            expect(normalize(price_2 ?? '')).toBe(normalize(rows[1].expectedPrice));
        });
    }

    async verifyInvoiceAddress(labelValue: string, address: string) {
        await allure.step("Verify Invoice Address", async () => {
            await this.page.getByRole('button', { name: 'Remove' }).isVisible({ timeout: 20000 })
            const lable = await this.contactGrid.getCellvalueByColIdandGrid("name", 1)
            const value = await this.contactGrid.getCellvalueByColIdandGrid("displayAddress", 1)
            expect(lable?.trim(), { message: `Customer invoice address not equals to ${labelValue}` }).toEqual(labelValue)
            expect(value?.trim(), { message: `Customer invoice address not equals to ${address}` }).toEqual(address)

        })
    }

    async verifyDeliveryAddress(labelValue: string, address: string) {
        await allure.step("Verify Delivery Address", async () => {
            await this.page.getByRole('button', { name: 'Remove' }).isVisible({ timeout: 20000 })
            const lable = await this.contactGrid.getCellvalueByColIdandGrid("name", 0)
            const value = await this.contactGrid.getCellvalueByColIdandGrid("displayAddress", 0)
            expect(lable?.trim(), { message: `Customer delivery address not equals to ${labelValue}` }).toEqual(labelValue)
            expect(value?.trim(), { message: `Customer delivery address not equals to ${address}` }).toEqual(address)
        })
    }

    async verifyDefaultStatusCheckbox() {
        await allure.step("Verify default status is Active", async () => {
            const statusCheckbox = this.page.getByRole('checkbox', { name: 'Active' });
            const isChecked = await statusCheckbox.isChecked();
            console.log(`Default status checkbox is checked: ${isChecked}`);
            expect(isChecked, 'Default status is not Active').toBeTruthy();
        });
    }

    async verifySaveButtonDisabled() {
        await allure.step("Verify Save button is disabled", async () => {
            if (this.ang_version === AngVersion.NEW) {
                const saveButton = this.page.getByTestId('edit-footer').getByTestId('save');
                const hasDisabledClass = await saveButton.evaluate((el) => el.classList.contains('is-disabled'));
                console.log(`Save button has is-disabled class: ${hasDisabledClass}`);
                expect(hasDisabledClass, 'Save button does not have is-disabled class').toBeTruthy();
            } else {
                const saveButton = this.page.getByRole('button', { name: 'Save' });
                const isDisabled = await saveButton.isDisabled();
                console.log(`Save button is disabled: ${isDisabled}`);
                expect(isDisabled, 'Save button is not disabled').toBeTruthy();
            }
        });
    }

    async verifySaveButtonEnabled() {
        await allure.step("Verify Save button is enabled", async () => {
            if (this.ang_version === AngVersion.NEW) {
                const saveButton = this.page.getByTestId('edit-footer').getByTestId('save');
                const hasEnabledClass = await saveButton.isEnabled();
                console.log(`Save button is enabled: ${hasEnabledClass}`);
                expect(hasEnabledClass, 'Save button is not enabled').toBeTruthy();
            } else {
                const saveButton = this.page.getByRole('button', { name: 'Save' });
                const isEnabled = await saveButton.isEnabled();
                console.log(`Save button is enabled: ${isEnabled}`);
                expect(isEnabled, 'Save button is not enabled').toBeTruthy();
            }
        });
    }

    async expandContacts() {
        await allure.step("Expand Contacts section", async () => {
            const contactsBtn = this.page.getByRole('button', { name: /^Contacts/i });
            await contactsBtn.scrollIntoViewIfNeeded();
            await contactsBtn.click();
        });
    }

    async clickCreateContact() {
        await allure.step("Click Create Contact button", async () => {
            const createContactBtn = this.page.getByRole('button', { name: 'Create Contact' });
            await createContactBtn.scrollIntoViewIfNeeded();
            await createContactBtn.click();
        });
    }

    async verifyContactPopupDisplayed() {
        await allure.step("Verify Contact popup is displayed", async () => {
            if (this.ang_version === AngVersion.NEW) {
                const contactPopup = this.page.getByTestId('Add Contact');
                await expect(contactPopup).toBeVisible({ timeout: 3000 });
            } else {
                const contactPopup = this.page.locator('div.modal-content');
                await expect(contactPopup).toBeVisible({ timeout: 3000 });
            }
        });
    }

    async setFirstName(firstName: string) {
        await allure.step("Set First Name: " + firstName, async () => {
            if (this.ang_version === AngVersion.NEW) {
                const firstNameBox = this.page.getByTestId('firstName');
                await firstNameBox.fill(firstName);
            } else {
                const firstNameBox = this.page.getByRole('textbox', { name: 'First Name' });
                await firstNameBox.fill(firstName);
            }
        });
    }

    async setLastName(lastName: string) {
        await allure.step("Set Last Name: " + lastName, async () => {
            if (this.ang_version === AngVersion.NEW) {
                const lastNameBox = this.page.getByTestId('lastName');
                await lastNameBox.fill(lastName);
            } else {
                const lastNameBox = this.page.getByRole('textbox', { name: 'Last Name' });
                await lastNameBox.fill(lastName);
            }
        });
    }

    async setEmail(email: string) {
        await allure.step("Set Email: " + email, async () => {
            if (this.ang_version === AngVersion.NEW) {
                const emailBox = this.page.getByTestId('email');
                await emailBox.fill(email);
            } else {
                const emailBox = this.page.getByRole('textbox', { name: 'Email' });
                await emailBox.fill(email);
            }
        });
    }

    async setPhoneNumber(phoneNumber: string) {
        await allure.step("Set Phone Number: " + phoneNumber, async () => {
            if (this.ang_version === AngVersion.NEW) {
                const phoneBox = this.page.getByTestId('phoneNumber');
                await phoneBox.fill(phoneNumber);
            } else {
                const phoneBox = this.page.getByRole('textbox', { name: 'Phone' });
                await phoneBox.fill(phoneNumber);
            }
        });
    }

    async selectPosition(position: string = 'Consultant') {
        await allure.step("Select Position: " + position, async () => {
            if (this.ang_version === AngVersion.NEW) {
                const positionDropdown = this.page.getByTestId('position');
                await positionDropdown.selectOption({ label: position });
            } else {
                const positionDropdown = this.page.getByLabel('Position');
                await positionDropdown.selectOption({ label: position });
            }
        });
    }

    async clickOk() {
        await allure.step("Click OK button on Contact popup", async () => {
            if (this.ang_version === AngVersion.NEW) {
                const okButton = this.page.getByTestId('Add Contact').getByTestId('save');
                await okButton.click();
            } else {
                const okButton = this.page.getByRole('button', { name: 'OK' });
                await okButton.click();
            }
        });
    }

    async createContactDetails(firstName: string, lastName: string, email: string, phoneNumber: string) {
        await allure.step("Create Contact details", async () => {
            await this.setFirstName(firstName);
            await this.setLastName(lastName);
            await this.setEmail(email);
            await this.setPhoneNumber(phoneNumber);
            await this.selectPosition();
            await this.clickOk();
            await this.waitForDataLoad('/api/Core/ContactPerson/');
        });
    }

    async verifyContactDetailsInGrid() {
        await allure.step("Verify contact details in Contacts grid", async () => {
            if (this.ang_version === AngVersion.NEW) {
                const rowCount = await this.conntactsPersonGrid.getRowCount('First Name');
                expect(rowCount).toEqual(1);
                console.log(`Contact grid row count: ${rowCount}`);
            } else {
                const rowCount = await this.conntactsPersonGridJS.getAgGridRowCount();
                expect(rowCount).toEqual(1);
                console.log(`Contact grid row count: ${rowCount}`);
            }
        });
    }

    async createNewCustomer() {
        await allure.step("Create new customer", async () => {
            const addButton = this.page.locator('//ul[@class="nav nav-tabs"]/li[@index!="tab.index"]').nth(0)
            await addButton.click({ force: true });
            await this.page.waitForTimeout(3000)
        });
    }
    
    async expandSettings() {
        await allure.step("Expand Settings section", async () => {
            const settingsButton = this.page.getByRole('button', { name: /^Settings/i });
            await expect(settingsButton).toBeVisible({ timeout: 3000 });
            await settingsButton.scrollIntoViewIfNeeded();
            await settingsButton.click();
        });
    }

    async setInvoiceMethod(method: string) {
        await allure.step("Set Invoice Method to " + method, async () => {
            const invoiceMethodDropdown = this.page.getByLabel('Invoice Method');
            await expect(invoiceMethodDropdown).toBeVisible();
            await invoiceMethodDropdown.selectOption({ label: method });
        });
    }

    async verifyErrorMessageShown(expectedMessage: string) {
        await allure.step(`Verify error modal is shown`, async () => {
            const modal = this.page.locator('div.modal-dialog');
            const message = modal.locator('.messagebox');
            await expect(modal).toBeVisible();
            await expect(message).toContainText(expectedMessage, { ignoreCase: true });
            await this.clickAlertMessage('OK');
        });
    }

    async checkPrivatePersonCheckbox() {
        await allure.step("Check Private Person checkbox", async () => {
            const privatePersonCheckbox = this.page.getByRole('checkbox', { name: 'Private person' });
            await expect(privatePersonCheckbox).toBeVisible();
            const isChecked = await privatePersonCheckbox.isChecked();
            if (!isChecked) {
                await privatePersonCheckbox.check();
            }
        });
    }
}