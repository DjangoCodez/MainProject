import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../SalesBasePage";
import * as allure from "allure-js-commons";
import { getDateUtil } from "../../../utils/CommonUtil";
import { SectionGridPageJS } from "../../common/SectionGridPageJS";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";


export class AgreementsPageJS extends SalesBasePage {

    readonly productGrid: SectionGridPageJS;
    readonly agreementGridTabCurrent: SingleGridPageJS;
    readonly agreementGridTabAgreement: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.productGrid = new SectionGridPageJS(page, 'billing.order.productrows');
        this.agreementGridTabCurrent = new SingleGridPageJS(page);
        this.agreementGridTabAgreement = new SingleGridPageJS(page, 1);
    }

    async waitForPageLoad() {
        await allure.step("Wait for page load", async () => {
            await this.waitForDataLoad('/api/Core/CustomerInvoices/');
            const pageLocator = this.page.getByRole('checkbox', { name: 'Show summaries incl. VAT' });
            await expect(pageLocator).toBeVisible({ timeout: 30_000 });
        });
    }

    async creatAgreement() {
        await allure.step("Create new agreement", async () => {
            await this.createItem();
            await this.waitForDataLoad('/api/Billing/Product/Prices/');
        });
    }

    async editCustomer() {
        await allure.step("Go to edit Customer ", async () => {
            const editInput = this.page.locator('(//button[@data-l10n-bind-title="editTooltipKey"])[1]');
            await editInput.waitFor({ state: 'visible' });
            await editInput.click();
            await this.waitForDataLoad('/api/Core/Customer/Customer/GLN');
        });
    }

    async createCustomer(customerNumber: string, CustomerName: string) {
        await allure.step("Create customer ", async () => {
            await this.page.locator('#ctrl_customer_customerNr').waitFor({ state: 'visible' });
            const customerInput = this.page.locator('//input[@id="ctrl_customer_customerNr"]');
            await customerInput.fill(customerNumber);
            const customerNameInput = this.page.locator('(//div[@form="ctrl.edit"]//textarea)[1]');
            await customerNameInput.fill(CustomerName);
            const save = this.page.locator('(//button[text()="Save"])[1]');
            await save.click();
            await this.waitForDataLoad('/api/Core/Customer/Customer/GLN/');
        });
    }

    async setInternalText(InternalText: string) {
        await allure.step("Set internal text ", async () => {
            await this.page.locator('#ctrl_invoice_originDescription').fill(InternalText);
        });
    }

    async setContractGroup(group: string) {
        await allure.step("Set agreement group ", async () => {
            await this.page.selectOption('#ctrl_invoice_contractGroupId', { label: group });
            await this.page.keyboard.press('Enter');
        });
    }

    async nextInvoiceDate() {
        await allure.step("Set invoice date ", async () => {
            const invoiceDate = this.page.locator('#ctrl_selectedNextInvoiceDate');
            const date = await getDateUtil(0, true);
            await invoiceDate.fill(date);
        });
    }

    async expandProductRows() {
        await allure.step("Expand product rows", async () => {
            const productRow = this.page.locator('[label-key="billing.order.productrows"]');
            await productRow.waitFor({ state: 'visible' });
            await productRow.click({ timeout: 20000 });
        });
    }


    async addProductRow() {
        await allure.step("Add product rows", async () => {
            const totalCount = this.page.locator(`//div[contains(@class, 'ag-cell') and @col-id='sumAmountCurrency']`);
            await totalCount.waitFor({ state: 'visible' });
            await expect(totalCount).toBeVisible();
            const productRow = this.page.locator("//button[contains(text(), 'New product row')]");
            await expect(productRow).toBeVisible();
            await productRow.scrollIntoViewIfNeeded();
            await productRow.click();
        });
    }

    async clickNewProductRow() {
        await allure.step("Add product rows", async () => {
            const productRow = this.page.getByRole('button', { name: 'New product row' });
            await expect(productRow).toBeVisible();
            await productRow.scrollIntoViewIfNeeded();
            await productRow.click();
        });
    }

    async addProductNo(name: any) {
        await allure.step("Select product name", async () => {
            await this.productGrid.enterDropDownValue(name);
            await this.page.waitForTimeout(1_000);
        });
    }

    async clickOk() {
        await allure.step("Click ok button", async () => {
            const okButton = this.page.locator('button.btn.btn-primary', { hasText: 'OK' });
            await okButton.waitFor({ state: 'visible' });
            if (await okButton.isVisible() && await okButton.isEnabled()) {
                await okButton.click();
            } else {
                console.log('OK button is not ready to be clicked.');
            }
        });
    }

    async addProductAmount(amount: number) {
        await allure.step("Add product amount", async () => {
            await this.productGrid.enterGridValueByColumnId('quantity', amount.toString());
        });
    }

    async addProductPrice(price: number) {
        await allure.step("Add product price amount", async () => {
            await this.productGrid.enterGridValueByColumnId('amountCurrency', price.toString());
        });
    }

    async save() {
        await allure.step("Save", async () => {
            await this.page.locator("//button[normalize-space(text())='Save']").click();
            await this.waitForDataLoad('/api/Billing/Contract/');
        });
    }

    async close() {
        await allure.step("Add product price amount", async () => {
            await this.page.locator("//i[@title='Close']").click();
        });
    }

    async VerifyRowCount(actualRowCount: number = 1) {
        await allure.step("Verify row count", async () => {
            const rowCount = await this.productGrid.getFilteredAgGridRowCount();
            expect(rowCount).toBe(actualRowCount);
        });
    }

    async VerifyAgreementGroup(group: string) {
        await allure.step("Verify agreement group", async () => {
            const contractGroupName = await this.agreementGridTabAgreement.getCellvalueByColIdandGrid('contractGroupName');
            expect(contractGroupName).toBe(group);
        });
    }

    async VerifyTotalVatAmount(amount: string) {
        await allure.step("Verify total vat amount", async () => {
            const vatTotalRaw = await this.agreementGridTabAgreement.getCellvalueByColIdandGrid('totalAmountExVat');
            const vatTotal = vatTotalRaw.replace(/\s/g, '');
            expect(amount, `Mismatch: UI shows "${vatTotal}", expected "${amount}"`).toBe(vatTotal);
        });
    }

    async VerifyTotalVatAmountCurrent(amount: string) {
        await allure.step("Verify total vat amount", async () => {
            const vatTotalRaw = await this.agreementGridTabCurrent.getCellvalueByColIdandGrid('totalAmountExVat');
            const vatTotal = vatTotalRaw.replace(/\s/g, '');
            await this.page.waitForTimeout(1_000);
            expect(vatTotal, `Mismatch: UI shows "${vatTotal}", expected "${amount}"`).toBe(amount);
        });
    }

    async moveToCurrentAgreementTab() {
        await allure.step("Move to the current tab", async () => {
            await this.page.locator("//label[text()='Current']").click();
        });
    }

    async filterByInternalTextCurrent(internalText: string, rowindex: number = 0) {
        await allure.step("Search by internal text", async () => {
            await this.agreementGridTabCurrent.filterByName('Internal Text', internalText, 15000, rowindex);
        });
    }

    async filterByInternalTextAgreements(internalText: string, rowindex: number = 0) {
        await allure.step("Search by internal text", async () => {
            const totalSpan = this.page.locator("//div[contains(@class, 'soe-ag-totals-row-part') and contains(@class, 'pull-right')]//span[contains(@class, 'soe-ag-grid-totals-all-count')]").nth(1);
            await expect(totalSpan).toBeVisible();
            await this.page.locator(`//a[contains(@class, 'fa-filter-slash') and @title='Clear all filters']`).nth(1).click();
            await this.agreementGridTabAgreement.filterByName('Internal Text', internalText, 15000, rowindex);
        });
    }

    async selectAgreement() {
        await allure.step("Select agreement", async () => {
            await this.page.locator("(//input[@ref='eInput' and @type='checkbox'])[2]").click({ timeout: 30000 });
        });
    }

    async createPreliminaryInvoice() {
        await allure.step("Create preliminary invoice", async () => {
            await this.page.locator("//button[@type='button' and contains(@class, 'ngSoeMainButton') and contains(@class, 'dropdown-toggle')]").click();
            await this.page.locator("//ul[@class='dropdown-menu ngSoeMainDropdownMenu']//li[3]//a").click();
            await this.page.locator("//button[@type='button' and text()='OK' and contains(@class, 'btn-primary')]").click();
            await this.page.locator("//button[@type='button' and normalize-space(text())='OK' and contains(@class, 'btn-default')]").click();
        });
    }

    async createPreliminaryInvoicefromAgreement() {
        await allure.step("Create preliminary invoice through agreement", async () => {
            const toglleButton = this.page.getByRole('button').filter({ hasText: /^$/ }).nth(1);
            await toglleButton.waitFor({ state: 'visible', timeout: 7000 });
            await toglleButton.click();
            await this.page.getByRole('link', { name: 'Transfer to preliminary invoice' }).click();
            const okButton = this.page.getByRole('button', { name: 'OK' })
            await okButton.click();
        });
    }

    async createPreliminaryMergedInvoicefromAgreement() {
        await allure.step("Create preliminary invoice through agreement", async () => {
            await this.page.locator('//div[@label-key="core.functions"]/button[@data-toggle="dropdown"]').click();
            await this.page.getByRole('link', { name: 'Transfer to preliminary merged invoice' }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
            await this.page.waitForTimeout(1000);
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async moveToAgreementTab() {
        await allure.step("Move to agreement tab", async () => {
            const agreementTab = this.page.locator("//label[@class='ng-binding' and text()='Agreements']");
            await agreementTab.waitFor();
            await agreementTab.click();
        });
    }

    async editAgreement() {
        await allure.step("Edit agreement", async () => {
            await this.page.locator("(//div[@ag-grid='ctrl.gridAg.options.gridOptions'])[2]//div[@role='row'][1]//button[@title='Open agreement']").click();
        });
    }

    async transferToFinalInvoice() {
        await allure.step("Transfer final agreement", async () => {
            const toglleButton = this.page.getByRole('button').filter({ hasText: /^$/ }).nth(1);
            await toglleButton.waitFor({ state: 'visible', timeout: 7000 });
            await toglleButton.click();
            await this.page.getByRole('link', { name: ' Transfer to the final invoice' }).click({ timeout: 5000 });
            const okButton = this.page.getByRole('button', { name: 'OK' })
            await okButton.click({ timeout: 3000 });
            await this.page.waitForResponse(response =>
                response.url().includes('api/Core/CustomerInvoices/Transfer/') && response.status() === 200
            );
            const okButton_2 = this.page.getByRole('button', { name: 'OK' })
            await okButton_2.click({ timeout: 3000 });
        });
    }

    async trasnferToOrderfromAgreement() {
        await allure.step("Transfer to order through agreement", async () => {
            await this.page.getByRole('button', { name: 'Transfer to Order' }).click({ timeout: 3000 });
            const okButton = this.page.getByRole('button', { name: 'OK' })
            await okButton.click({ timeout: 3000 });
            await this.page.waitForResponse(response =>
                response.url().includes('api/Core/CustomerInvoices/Transfer/') && response.status() === 200
            );
            const okButtonTwo = this.page.getByRole('button', { name: 'OK' })
            await okButtonTwo.click({ timeout: 3000 });
        });
    }

    async trasnferToOrder() {
        await allure.step("Transfer to order", async () => {
            await this.page.getByRole('button').filter({ hasText: /^$/ }).nth(1).click({ timeout: 7000 });
            await this.page.getByRole('link', { name: 'Transfer to order' }).click({ timeout: 5000 });
            await this.page.waitForResponse(response =>
                response.url().includes('api/Core/CustomerInvoices/Transfer/') && response.status() === 200
            );
            await this.page.getByRole('button', { name: 'OK' }).click({ timeout: 3000 });
        });
    }

    async selectAgreementTwo() {
        await allure.step("Select agreement", async () => {
            await this.page.locator("(//input[@ref='eInput' and @type='checkbox'])[3]").click({ timeout: 30000 });
        });
    }

    async clickUnlockButton() {
        await allure.step("Click unlock button", async () => {
            await this.page.getByTitle('Unlock agreement details').click();
        });
    }

    async expandHeaderSection() {
        await allure.step("Expand header section", async () => {
            await this.page.locator("div.soe-accordion-heading.ng-scope")
                .filter({ hasText: "Customer" })
                .filter({ hasText: "Status" })
                .first().click();
        });
    }

    async verifyAddedProductInAgreement(productName: string) {
        await allure.step("Verify added product in agreement", async () => {
            await this.page.waitForTimeout(2_000);
            const filteredCount = await this.productGrid.getAgGridRowCount();
            expect(filteredCount).toBe(2);
            const type_2 = await this.productGrid.getRowColumnValue('text', 1);
            expect(type_2).toBe(productName);
        });
    }

    async getAgreementNumber(): Promise<string> {
        return await allure.step("Get created agreement number", async () => {
            const agreementNumberField = this.page.getByRole('textbox', { name: 'Agreement number' });
            await agreementNumberField.waitFor({ state: 'visible', timeout: 10000 });
            await this.page.waitForFunction((el) => (el as HTMLInputElement).value && (el as HTMLInputElement).value.trim() !== "", await agreementNumberField.elementHandle(), { timeout: 10000 });
            const agreementNumber = await agreementNumberField.inputValue();
            console.log("Captured Agreement Number:", agreementNumber);
            return agreementNumber;
        });
    }

    async clickCopyAgreementIcon() {
        await allure.step("Click Copy Contract Icon", async () => {
            const copyIcon = this.page.getByTitle('Copy');
            await copyIcon.waitFor({ state: 'visible' });
            await copyIcon.click();
            await this.page.waitForTimeout(1_000);
        });
    }

    async verifyAgreementNumberNotSame(previousAgreementNumber: string) {
        await allure.step("Verify Agreement Number Not Same", async () => {
            const newAgreementNumber = await this.getAgreementNumber();
            expect(newAgreementNumber).not.toBe(previousAgreementNumber);
            console.log(`Previous Agreement Number: ${previousAgreementNumber}, New Agreement Number: ${newAgreementNumber}`);
        });
    }

    async verifyInternalTextCopiedAgreement(originalInternalText: string) {
        await allure.step("Verify Internal Text Copied Agreement", async () => {
            const internalTextField = this.page.getByRole('textbox', { name: 'Internal text' });
            await internalTextField.waitFor({ state: 'visible' });
            const copiedInternalText = await internalTextField.inputValue();
            console.log("Copied Internal Text:", copiedInternalText);
            expect(copiedInternalText).toBe(originalInternalText);
        });
    }

    async getTotalAmount(): Promise<number> {
        return await allure.step("Get Total Amount (Ex VAT) from grid", async () => {
            const totalAmountField = await this.agreementGridTabCurrent.getCellvalueByColIdandGrid('totalAmountExVat');
            console.log("Captured Total Amount :", totalAmountField);
            const cleaned = totalAmountField.replace(/\s/g, '').replace(',', '.');
            const amountNumber = parseFloat(cleaned);
            return amountNumber;
        });
    }

    async clickUpdatePrices() {
        await allure.step("Update prices through agreement", async () => {
            await this.page.locator('//div[@label-key="core.functions"]/button[@data-toggle="dropdown"]').click();
            await this.page.getByRole('link', { name: 'Update prices' }).click();
        });
    }

    async editPrices(amount: string) {
        await allure.step("Edit prices through agreement", async () => {
            await this.page.locator('#ctrl_amount').fill(amount);
            await this.page.getByRole('button', { name: 'OK' }).click();
            await this.page.waitForTimeout(1_000);
        });
    }

    async clickFinishButton() {
        await allure.step("Click Finish Button", async () => {
            await this.page.getByRole('button', { name: 'Finish' }).click();
            await this.page.getByRole('button', { name: 'Yes' }).click();
            await this.page.waitForTimeout(2_000);
        });
    }
}