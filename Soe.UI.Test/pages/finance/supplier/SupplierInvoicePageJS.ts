import { expect, Page } from "@playwright/test";
import { BasePage } from "../../common/BasePage";
import * as allure from "allure-js-commons";
import { ModelGridPageJS } from "../../common/ModelGridPageJS";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";
import { SectionGridPageJS } from "../../common/SectionGridPageJS";
import path from 'path/win32';


export class SupplierInvoicePageJS extends BasePage {
    readonly page: Page;
    readonly orderDialogGrid: ModelGridPageJS;
    readonly projectDialogGrid: ModelGridPageJS;
    readonly allocateCostGrid: SectionGridPageJS;
    readonly supplierInvoiceGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.page = page;
        this.orderDialogGrid = new ModelGridPageJS(page, 'ctrl.gridHandler.gridAg.options.gridOptions');
        this.projectDialogGrid = new ModelGridPageJS(page, 'ctrl.soeGridOptions.gridOptions');
        this.allocateCostGrid = new SectionGridPageJS(page, 'economy.supplier.invoice.allocatecosts', 'ctrl.gridAg.options.gridOptions');
        this.supplierInvoiceGrid = new SingleGridPageJS(page, 0, 'ctrl.gridAg.options.gridOptions');
    }

    async waitForPageLoad() {
        await allure.step("Wait for Supplier Invoice page to load", async () => {
            await this.supplierInvoiceGrid.waitForPageLoad();
            await this.waitForDataLoad('/api/Economy/Supplier/Invoices/Grid');
        });
    }

    async createSupplierInvoice() {
        await allure.step('Create Supplier Invoice', async () => {
            await this.page.click('button:has-text("Create Supplier Invoice")');
            await this.page.waitForSelector('.supplier-invoice-form');
        });
    }

    async setSupplier(supplierName: string = '1') {
        await allure.step(`Set Supplier: ${supplierName}`, async () => {
            const supplierInput = this.page.getByRole('textbox', { name: 'Supplier', exact: true })
            await supplierInput.waitFor({ state: 'visible', timeout: 10000 });
            await supplierInput.click();
            await supplierInput.fill(supplierName);
            await this.page.waitForTimeout(1000); // Wait for the options to load
            await this.page.keyboard.press('Tab');
        });
    }

    async setInvoiceDate(date: string) {
        await allure.step(`Set Invoice Date: ${date}`, async () => {
            const invoiceDateInput = this.page.getByRole('textbox', { name: 'Invoice Date' })
            await invoiceDateInput.click();
            await invoiceDateInput.fill(date);
            await this.page.keyboard.press('Enter'); // Assuming the date is in a format that can be directly entered
        });
    }

    async setInvoiceNumber(invoiceNumber: string) {
        await allure.step(`Set Invoice Number: ${invoiceNumber}`, async () => {
            const invoiceNumberInput = this.page.getByRole('textbox', { name: 'Invoice No.' })
            await invoiceNumberInput.click();
            await invoiceNumberInput.fill(invoiceNumber);
        });
    }

    async setTotal(total: string) {
        await allure.step(`Set Total: ${total}`, async () => {
            const totalInput = this.page.getByRole('textbox', { name: 'Total' });
            await totalInput.click();
            await totalInput.fill(total);
        });
    }

    async linkOrder(internalText: string) {
        await allure.step(`Link Order: ${internalText}`, async () => {
            await this.page.getByTitle('Order No.').click();
            await this.orderDialogGrid.waitForPageLoad();
            await this.orderDialogGrid.filterByName('Internal Text', internalText);
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async linkProject(projectName: string) {
        await allure.step(`Link Project: ${projectName}`, async () => {
            await this.page.getByTitle('Projects').click();
            await this.projectDialogGrid.waitForPageLoad();
            await this.projectDialogGrid.filterByName('Name', projectName);
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async chargeCostToProject(isChecked: boolean, rowIndex: number = 0) {
        await allure.step(`Charge Cost To Project: ${isChecked}`, async () => {
            await this.allocateCostGrid.clickCheckBoxByColumnId('chargeCostToProject', isChecked, rowIndex);
        });
    }

    async verifyAllocateCostRowCount(count: number) {
        await allure.step(`Verify project row count is : ${count}`, async () => {
            expect(await this.allocateCostGrid.getAgGridRowCount()).toEqual(count);
        });
    }

    async deleteProjectRow() {
        await allure.step(`Delete project row`, async () => {
            await this.allocateCostGrid.clickButtonByColumnId('delete');
            await this.page.waitForLoadState('networkidle');
        });
    }

    async addRowInAllocateCostGrid() {
        await allure.step('Add Row In Allocate Cost Grid', async () => {
            const allocateCost = this.page.locator("//*[@label-key='economy.supplier.invoice.allocatecosts']")
            await allocateCost.getByRole('button', { name: 'Add Row' }).scrollIntoViewIfNeeded();
            await allocateCost.getByRole('button', { name: 'Add Row' }).click();
            await this.allocateCostGrid.waitForPageLoad();
        });
    }

    async reInvoiceAllocateCostGrid() {
        await allure.step('Add Row In Allocate Cost Grid', async () => {
            const allocateCost = this.page.locator("//*[@label-key='economy.supplier.invoice.allocatecosts']")
            await allocateCost.getByRole('button', { name: 'Re-Bill' }).scrollIntoViewIfNeeded();
            await allocateCost.getByRole('button', { name: 'Re-Bill' }).click();
            await this.allocateCostGrid.waitForPageLoad();
        });
    }

    async connectProjectAllocateCostGrid(currentTextOnButton: string = 'Add Row') {
        await allure.step('Connect project option Allocate Cost Grid', async () => {
            await this.toggleAllocateCostGrid(currentTextOnButton, 'Connect to Project');
            await this.allocateCostGrid.waitForPageLoad();
        });
    }

    async reInvoiceToggleAllocateCostGrid(currentTextOnButton: string = 'Add Row') {
        await allure.step('Connect project option Allocate Cost Grid', async () => {
            await this.toggleAllocateCostGrid(currentTextOnButton, 'Re-Bill');
            await this.allocateCostGrid.waitForPageLoad();
        });
    }

    private async toggleAllocateCostGrid(currentText: string, linkText: string) {
        const allocateCost = this.page.locator("//*[@label-key='economy.supplier.invoice.allocatecosts']")
        const currentButtton = allocateCost.getByRole('button', { name: currentText });
        await expect(currentButtton).toBeVisible();
        const siblingButton = currentButtton.locator('xpath=following-sibling::button[1]');
        await siblingButton.click();
        await allocateCost.getByRole('link', { name: linkText }).click();
    }

    async verifyAllocateCostGridValue(column: string, value: string, rowIndex: number = 0) {
        await allure.step(`Verify grid value : ${column} row ${column}`, async () => {
            let gridCellValue = await this.allocateCostGrid.getCellvalueByColIdandGrid(column, rowIndex);
            if (typeof gridCellValue === 'string') {
                gridCellValue = gridCellValue.replace(/\u00A0/g, ' ').trim();
            }
            expect(gridCellValue).toEqual(value);
        });
    }

    async scrollToAllocateCostGrid() {
        await allure.step('Scroll To Allocate Cost Grid', async () => {
            const allocateCosr = this.page.getByRole('button', { name: 'Cost Allocation' })
            await allocateCosr.scrollIntoViewIfNeeded();
        });
    }

    async expandAllocateCostGrid() {
        await allure.step('Expand Allocate Cost Grid', async () => {
            const allocateCosr = this.page.getByRole('button', { name: 'Cost Allocation' })
            await allocateCosr.click();
            await this.allocateCostGrid.waitForPageLoad();
            await this.waitForDataLoad('/api/Core/UserGridState/common_directives_supplierinvoiceallocatedcosts');
        });
    }

    async saveInvoice() {
        await allure.step('Save Invoice', async () => {
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.page.waitForLoadState('networkidle');
            await this.page.waitForTimeout(3000);
        });
    }

    async openInvoiceByNumber(invoiceNumber: string, isEdit: boolean = true) {
        await allure.step(`Open Invoice By Number: ${invoiceNumber}`, async () => {
            await this.supplierInvoiceGrid.filterByName('Invoice No.', invoiceNumber);
            if (isEdit) {
                await this.page.waitForTimeout(1000);
                await this.supplierInvoiceGrid.clickButtonByColumnId('showCreateInvoiceIcon');
            }
        });
    }

    async selectSupplierinvoiceByInvoiceNo(invoiceNumber: string) {
        await allure.step(`Open Invoice By Invoice No: ${invoiceNumber}`, async () => {
            await this.supplierInvoiceGrid.filterByName('Invoice No.', invoiceNumber);
        });
    }

    async changeAttestationGroup(attestationGroup: string) {
        await allure.step(`Change Attestation Group: ${attestationGroup}`, async () => {
            await this.supplierInvoiceGrid.enterDropDownValueGridRichSelecter('attestGroupName', attestationGroup);
            await this.page.locator("//button[contains(@class, 'ngSoeMainButton') and contains(@class, 'dropdown-toggle')]").click();
            await this.page.getByRole('link', { name: 'Save', exact: true }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async verifyInvoiceAvailable(invoiceNumber: string) {
        await allure.step(`Verify Invoice Available: ${invoiceNumber}`, async () => {
            await this.supplierInvoiceGrid.filterByName('Invoice No.', invoiceNumber);
            await this.page.waitForTimeout(2000);
            const count = await this.supplierInvoiceGrid.getFilteredAgGridRowCount();
            expect(count, `Expected 1 row, but found ${count} rows.`).toBe(1);
        });
    }

    async setInvoiceAmountAllocateCost(amount: string, rowIndex: number = 0) {
        await allure.step(`Set invoice amount on allocate cost`, async () => {
            await this.allocateCostGrid.enterGridValueByColumnId('rowAmountCurrency', amount, rowIndex);
        });
    }

    async setOrderAllocateCost(order: string, rowIndex: number = 0) {
        await allure.step(`Set order for allocate cost`, async () => {
            await this.allocateCostGrid.enterGridValueByColumnId('customerInvoiceNumberName', order, rowIndex, true);
            await this.page.waitForTimeout(2000);
        });
    }

    async verifyAlertMessage(message: string) {
        await allure.step(`Verify Alert Message: ${message}`, async () => {
            const alert = this.page.getByText(message);
            await expect(alert).toBeVisible({ timeout: 15000 });
        });
    }

    async clickAlertMessageOk() {
        await allure.step(`Ok Alert Message`, async () => {
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async editSupplierinvoice() {
        await allure.step(`Edit Supplier Invoice`, async () => {
            const button = this.page.locator('//button[contains(@class, "iconEdit")]');
            await button.click({ timeout: 5000 });
        });
    }

    async startAssetFlow() {
        await allure.step('Start Asset Flow', async () => {
            await this.page.locator("//button[contains(@class, 'ngSoeMainButton') and contains(@class, 'dropdown-toggle')]").click();
            await this.page.getByRole('link', { name: 'Start attest flow' }).click();
            await this.page.getByRole('button', { name: 'Yes' }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async reloadPage() {
        await allure.step('Reload Page', async () => {
            await this.page.getByRole('toolbar').getByTitle('Reload records').click();
            await this.waitForDataLoad('/api/Economy/Supplier/Invoices/Grid');
        });
    }

    async verifyAttest(attest: string) {
        await allure.step('Verify Attest', async () => {
            const attestStatus = await this.supplierInvoiceGrid.getCellvalueByColIdandGrid('attestStateName');
            expect(attestStatus).toBe(attest);
        });
    }

    async openToAttestation() {
        await allure.step('Open To Attestation', async () => {
            const toggleIcon = this.page.locator('//div[contains(@class, "soe-accordion-heading")]//i[contains(@class, "fa-chevron-down") or contains(@class, "fa-chevron-up")]');
            await toggleIcon.nth(3).scrollIntoViewIfNeeded();
            await toggleIcon.nth(3).click({ timeout: 5000 });
            await this.page.getByRole('button', { name: 'To Attestation' }).click();
        });
    }

    async selectAttestationGroup(attestationGroup: string) {
        return await allure.step(`Select Attestation Group: ${attestationGroup}`, async () => {
            await this.page.getByLabel('Attestation Group', { exact: true }).selectOption({ label: attestationGroup });
        });
    }

    async clickOk() {
        await allure.step('Click OK', async () => {
            const okButton = this.page.locator('//button[normalize-space(text())="OK"]');
            await okButton.waitFor({ state: 'visible' });
            await expect(okButton).toBeEnabled();
            await okButton.scrollIntoViewIfNeeded();
            await okButton.click();
            const okButton2 = this.page.locator('//div[@class="modal-content"]//div[@class="modal-footer ng-scope"]//button[@type="button" and text()="OK"]');
            await okButton2.waitFor({ state: 'visible' });
            await expect(okButton2).toBeEnabled();
            await okButton2.scrollIntoViewIfNeeded();
            await okButton2.click();
        });
    }

    async closeTab() {
        await allure.step('Close Tab', async () => {
            await this.page.locator('(//a[@ng-click="select($event)"]//i[contains(@class, "removableTabIcon")])[1]').click();
        });
    }

    async verifyApprovers(approvers: string) {
        await allure.step('Verify Approvers', async () => {
            const currentApprovers = await this.supplierInvoiceGrid.getCellvalueByColIdandGrid('currentAttestUserName');
            expect(currentApprovers).toBe(approvers);
        });
    }

    async verifyAttestationGroup(attestationGroup: string) {
        await allure.step('Verify Attestation Group', async () => {
            const currentAttestationGroup = await this.supplierInvoiceGrid.getCellvalueByColIdandGrid("attestGroupName");
            expect(currentAttestationGroup).toBe(attestationGroup);
        });
    }

    async waitForSupplierInvoiceGridLoaded() {
        await allure.step('Wait for Supplier Invoice Grid to load', async () => {
            await this.waitForDataLoad('e/api/Economy/Supplier/Invoices/Grid?allItemsSelection=1&loadOpen=true&loadClosed=false', 25000);
        });
    }

    async waitForModal() {
        await allure.step('Wait for Modal', async () => {
            const modal = this.page.locator("//div[@uib-modal-window='modal-window' and contains(@class, 'modal')]");
            await modal.waitFor({ state: 'visible' });
            await this.waitForDataLoad('/api/Core/UserGridState/Economy_Supplier_Invoices');
        });
    }

    async saveAsDefinitive() {
        await allure.step("Save as definitive", async () => {
            const functions = this.page.locator("//div[@class='tab-pane ng-scope active']//div[@form='ctrl.edit']//button[contains(text(),'Functions')]/following-sibling::button");
            await functions.waitFor({ state: 'visible' });
            await functions.click();
            const saveAsDefinitive = this.page.getByRole('link', { name: 'Save as definitive' });
            await saveAsDefinitive.waitFor({ state: 'visible' });
            await saveAsDefinitive.click();
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
            await this.page.waitForTimeout(300);
            await expect(this.page.getByText('Invoice saved as definitive')).toBeVisible();
            await this.clickAlertMessage('OK');
        });
    }

    async getInvoiceSequenceNo() {
        return await allure.step("Get invoice sequence number", async () => {
            const sequenceNumber = await this.supplierInvoiceGrid.getCellvalueByColIdandGrid('seqNr');
            return sequenceNumber;
        });
    }

    async selectSupplierinvoice() {
        await allure.step(`select Supplier Invoice`, async () => {
            await this.supplierInvoiceGrid.selectAllCheckBox();
        });
    }

    async editSupplier(isSelectedSupplier: boolean = true) {
        await allure.step(`Edit Supplier`, async () => {
            const editButton = this.page.locator(`//label[normalize-space()='Supplier']/parent::div//button[contains(@class,'iconEdit')]`);
            await editButton.waitFor({ state: 'visible' });
            await editButton.click();
            if (isSelectedSupplier) {
                await this.waitForDataLoad('/api/Economy/Supplier/Supplier/NextSupplierNr/');
            } else {
                await this.waitForDataLoad('/api/Core/ContactPerson/ContactPersonsByActorId/', 20000);
            }
        });
    }

    async enterSupplierName(supplierName: string) {
        await allure.step(`Enter Supplier Name: ${supplierName}`, async () => {
            const supplier = this.page.getByRole('textbox', { name: 'Name', exact: true });
            await supplier.waitFor({ state: 'visible' });
            await supplier.fill(supplierName);
        });
    }

    async saveSupplier(index: number = 0, isEdit: boolean = true) {
        await allure.step('Save Supplier', async () => {
            await this.page.getByRole('button', { name: 'Save' }).nth(index).click();
            if (isEdit) {
                await this.waitForDataLoad('/api/Economy/Supplier/Supplier/?onlyActive=false');
                await this.waitForDataLoad('/api/Core/Currency/Comp/');
            } else {
                await this.waitForDataLoad('/api/Economy/Supplier/Supplier/');
            }
        });
    }

    async enterSupplierNo(supplierNo: string) {
        await allure.step(`Enter Supplier No: ${supplierNo}`, async () => {
            const supplierNoInput = this.page.locator('#ctrl_supplier_supplierNr');
            await supplierNoInput.fill(supplierNo);
            await this.page.keyboard.press('Enter');
        });
    }
    async waitForNewSupplierInvoiceLoaded() {
        await allure.step('Wait for Supplier Invoice to load', async () => {
            await Promise.all([
                this.waitForDataLoad('/api/Core/Currency/Enterprise/'),
            ]);
        });
    }

    async clearSupplier() {
        await allure.step(`Clear Supplier`, async () => {
            const supplier = this.page.locator(`#ctrl_selectedSupplier`);
            await supplier.fill('');
        });
    }

    async verifySupplierName(supplierName: string) {
        await allure.step(`Verify Supplier Name: ${supplierName}`, async () => {
            await this.page.waitForTimeout(1000);
            const supplierNameInput = this.page.locator('#ctrl_supplier_name');
            await supplierNameInput.waitFor({ state: 'visible' });
            const currentSupplierName = await supplierNameInput.inputValue();
            expect(currentSupplierName).toBe(supplierName);
        });
    }

    async verifySupplierNo(supplierNo: string) {
        await allure.step(`Verify Supplier No: ${supplierNo}`, async () => {
            const supplierNoInput = this.page.locator('#ctrl_supplier_supplierNr');
            await supplierNoInput.waitFor({ state: 'visible' });
            const currentSupplierNo = await supplierNoInput.inputValue();
            expect(currentSupplierNo).toBe(supplierNo);
        });
    }

    async verifyPaymentAccount(paymentAccount: string) {
        await allure.step(`Verify Payment Account: ${paymentAccount}`, async () => {
            const paymentAccountInput = this.page.locator('#ctrl_selectedPaymentInfo');
            await paymentAccountInput.waitFor({ state: 'visible' });
            const currentPaymentAccount = await paymentAccountInput.locator('option:checked').textContent();
            expect(currentPaymentAccount).toBe(paymentAccount);
        });
    }

    async deleteSupplier() {
        await allure.step(`Delete Supplier`, async () => {
            const deleteButton = this.page.getByRole('button', { name: 'Remove' });
            await deleteButton.waitFor({ state: 'visible' });
            await deleteButton.click();
            await this.clickAlertMessage('OK');
            await this.waitForDataLoad('/api/Economy/Supplier/Supplier/?onlyActive=true&addEmptyRow=true');
            await this.clickAlertMessage('OK');
        });
    }

    async verifySupplierNotSelected(expectedValue: string) {
        await allure.step(`Verify supplier "${expectedValue}" is NOT selected`, async () => {
            const supplierInput = this.page.getByRole('textbox', { name: 'Supplier', exact: true });
            const currentValue = await supplierInput.inputValue();
            expect(currentValue, `❌ Supplier "${expectedValue}" was selected, but should NOT be!`).not.toBe(expectedValue);
        });
    }

    async verifyFilteredCount(expectedCount: number) {
        await allure.step(`Verify Filtered Count: ${expectedCount}`, async () => {
            const actualCount = await this.supplierInvoiceGrid.getFilteredAgGridRowCount();
            expect(actualCount, `Expected ${expectedCount} row(s), but found ${actualCount} row(s).`).toBe(expectedCount);
        });
    }

    async closeSupplierModal() {
        await allure.step(`Close Add Supplier Modal`, async () => {
            const closeButton = this.page.locator('//button[@class="close" and @data-dismiss="modal"]').nth(0);
            await closeButton.waitFor({ state: 'visible' });
            await closeButton.click();
        });
    }

    async verifyVatAmmount(vatAmount: string) {
        await allure.step(`Verify VAT Amount: ${vatAmount}`, async () => {
            const vatAmountInput = this.page.locator('#ctrl_invoice_vatAmountCurrency');
            let currentVatAmount = await vatAmountInput.inputValue();
            currentVatAmount = currentVatAmount.replace(/\s/g, "").trim();
            expect(currentVatAmount, `❌ VAT Amount "${currentVatAmount}" was selected, but should NOT be!`).toBe(vatAmount);
        });
    }

    async verifyAccountingDate(accountingDate: string) {
        await allure.step(`Verify Accounting Date: ${accountingDate}`, async () => {
            const accountingDateInput = this.page.locator('//input[@id="ctrl_selectedVoucherDate"]');
            await accountingDateInput.waitFor({ state: 'visible' });
            const currentAccountingDate = await accountingDateInput.inputValue();
            const dateExpected = new Date(accountingDate);
            const formatDateExpected = new Date(dateExpected);
            const formatDateRecieved = new Date(currentAccountingDate);
            expect(formatDateExpected.toString(), `Expected ${formatDateExpected.toString()} row, but found ${formatDateRecieved.toString()} rows.`).toBe(formatDateRecieved.toString());
        });
    }

    async verifyDueDateIsNotInvoiceDate(invoiceDate: string) {
        await allure.step(`Verify Due Date is NOT Invoice Date: ${invoiceDate}`, async () => {
            const invoiceDueDateInput = this.page.locator('//input[@id="ctrl_invoice_dueDate"]');
            await invoiceDueDateInput.waitFor({ state: 'visible' });
            const currentDueDate = await invoiceDueDateInput.inputValue();
            const expectedInvoiceDate = new Date(invoiceDate);
            const receivedDueDate = new Date(currentDueDate);
            expectedInvoiceDate.setHours(0, 0, 0, 0);
            receivedDueDate.setHours(0, 0, 0, 0);
            expect(expectedInvoiceDate.getTime(), `❌ Due date should NOT equal invoice date: ${expectedInvoiceDate.toDateString()}`).not.toBe(receivedDueDate.getTime());
            expect(receivedDueDate.getTime(), `❌ Due date should be in the future, but got ${receivedDueDate.toDateString()}`).toBeGreaterThan(expectedInvoiceDate.getTime());
        });
    }

    async uploadPhoto(filePath: string) {
        await allure.step(`Upload Photo: ${filePath}`, async () => {
            const selectFileUpload = this.page.locator('//button[normalize-space(.)="Select File to Upload"]');
            await selectFileUpload.waitFor({ state: 'visible' });
            await selectFileUpload.click();
            const fileInput = this.page.locator('//input[@type="file" and @uploader="ctrl.uploader" and @nv-file-select]');
            const resolvedFilePath = path.resolve(filePath);
            await fileInput.setInputFiles(resolvedFilePath);
            await this.waitForDataLoad('/api/Core/Files/Invoice/');
            const closeButton = this.page.locator("//button[@type='button' and @class='close' and @data-ng-click='ctrl.buttonCancelClick()']");
            if (await closeButton.isVisible()) {
                await closeButton.click();
            }
            await this.page.waitForTimeout(300);
        });
    }

    async deletePhoto() {
        await allure.step(`Delete Photo`, async () => {
            const backButton = this.page.locator("//a[contains(@class, 'fa-arrow-to-left')]");
            await backButton.waitFor({ state: 'visible' });
            await backButton.click();
            await this.page.waitForTimeout(300);
            const deleteButton = this.page.locator("//button[contains(@class,'fa-trash')]");
            await deleteButton.waitFor({ state: 'visible' });
            await deleteButton.click();
            const selectFileUpload = this.page.locator('//button[normalize-space(.)="Select File to Upload"]');
            await selectFileUpload.waitFor({ state: 'visible' });
            await expect(selectFileUpload, `Image delete not successful`).toBeVisible();
        });
    }

    async clickPreliminary(checked: boolean = true) {
        await allure.step(`Click Preliminary`, async () => {
            const preliminaryCheckbox = this.page.locator('//input[@id="ctrl_draft"]');
            await preliminaryCheckbox.waitFor({ state: 'visible' });
            if (checked) {
                await preliminaryCheckbox.check();
                await expect(preliminaryCheckbox).toBeChecked();
            }
            else {
                await preliminaryCheckbox.uncheck();
                await expect(preliminaryCheckbox).not.toBeChecked();
            }
            await this.page.waitForTimeout(300);
        });
    }

    async saveSupplierInvoice(saveAndClose: boolean = false) {
        await allure.step(`Save Supplier Invoice`, async () => {
            const dropdownButton = this.page.locator("//button[@type='button' and contains(@class, 'ngSoeMainButton') and contains(@class, 'dropdown-toggle')]").nth(1);
            await dropdownButton.waitFor({ state: 'visible' });
            await dropdownButton.click();
            const saveAndCloseLink = this.page.getByRole('link', { name: /Save and close/i });
            const saveLink = this.page.getByRole('link', { name: /^Save \(Ctrl\+S\)$/i });
            if (saveAndClose) {
                await saveAndCloseLink.waitFor({ state: 'visible' });
                await saveAndCloseLink.click();
                await this.waitForDataLoad('/api/Economy/Supplier/Invoice/');
            } else {
                await saveLink.waitFor({ state: 'visible' });
                await saveLink.click();
            }
        });
    }

    async verifyInvoiceStatus(status: string) {
        await allure.step(`Verify Invoice Status: ${status}`, async () => {
            const receivedStatus = await this.supplierInvoiceGrid.getRowColumnValue('statusName');
            expect(receivedStatus, `Invoice status mismatch got ${receivedStatus} expect ${status}`).toEqual(status);
        });
    }

    async setInvoiceAmount(amount: string) {
        await allure.step(`Set Invoice Amount: ${amount}`, async () => {
            const amountInput = this.page.getByRole('textbox', { name: 'Total' });
            await amountInput.waitFor({ state: 'visible' });
            await amountInput.click();
            await amountInput.fill(amount);
            await amountInput.click();
            await this.page.evaluate(() => document.activeElement?.dispatchEvent(
                new KeyboardEvent('keypress', { key: 'Enter', bubbles: true })
            ));
            await this.page.waitForTimeout(500);
        });
    }

    async setInternalText(description: string) {
        await allure.step(`Set Internal Text: ${description}`, async () => {
            const descriptionInput = this.page.getByRole('textbox', { name: 'Internal Text' });
            await descriptionInput.waitFor({ state: 'visible' });
            await descriptionInput.click();
            await descriptionInput.fill(description);
        });
    }

    async selectVatType(vatType: string) {
        await allure.step(`Select VAT Type: ${vatType}`, async () => {
            await this.page.locator('#ctrl_invoice_vatType').scrollIntoViewIfNeeded();
            await this.page.locator('#ctrl_invoice_vatType').waitFor({ state: 'visible' });
            await this.page.selectOption('#ctrl_invoice_vatType', { label: vatType });
            await this.page.waitForTimeout(300);
            await this.page.evaluate(() => document.activeElement?.dispatchEvent(
                new KeyboardEvent('keypress', { key: 'Enter', bubbles: true })
            ));
        });
    }

    async selectSupplier(supplierName: string) {
        await allure.step(`Enter Supplier Name: ${supplierName}`, async () => {
            const supplier = this.page.locator('#ctrl_selectedSupplier');
            await supplier.waitFor({ state: 'visible' });
            await supplier.fill(supplierName);
            await this.page.waitForSelector('ul.dropdown-menu li.uib-typeahead-match', { state: 'visible' });
            await this.page.locator(`ul.dropdown-menu li.uib-typeahead-match:has-text("${supplierName}")`).click();
            await this.page.evaluate(() => document.activeElement?.dispatchEvent(
                new KeyboardEvent('keypress', { key: 'Enter', bubbles: true })
            ));
            await this.page.waitForTimeout(200);
        });
    }

    async waitForNewSupplierInvoiceLoad() {
        await allure.step(`Wait for Supplier Invoice Page to load`, async () => {
            await this.waitForDataLoad('e/api/Core/Currency/Enterprise/', 20000);
        });
    }

    async waitForEditInvoice() {
        await allure.step(`Wait for Edit Invoice`, async () => {
            await this.waitForDataLoad('/api/Core/UserGridState/Common_Directives_AccountingRows');
            await this.page.waitForTimeout(3500);
        });
    }

    async verifyAssertVatAmount(expectedAmount: string, locked: boolean = false) {
        await allure.step(`Verify Assert VAT Amount: ${expectedAmount}`, async () => {
            const actualVatAmount = this.page.locator(`#ctrl_invoice_vatAmountCurrency`);
            await actualVatAmount.waitFor({ state: 'visible' });
            const actualAmountText = await actualVatAmount.inputValue();
            const amount = actualAmountText.replace(/\s+/g, "").replace(",", ".");
            expect(amount, `Expected VAT Amount: ${expectedAmount}, but found: ${amount}`).toBe(expectedAmount);
            if (locked) {
                await expect(actualVatAmount).toHaveAttribute('readonly', 'readOnly');
            }
        });
    }

    async expandProductRowsGrid() {
        await allure.step('Expand Product Rows Grid', async () => {
            const expandButton = this.page.locator('label', { hasText: 'Coding Rows' });
            await expandButton.waitFor({ state: 'visible' });
            await expandButton.scrollIntoViewIfNeeded();
            await expandButton.click();
        });
    }

    async verifyCodingRowsCount(expectedCount: number) {
        await allure.step(`Verify Coding Rows Count: ${expectedCount}`, async () => {
            const total = this.page.locator('//span[contains(@class, "soe-ag-grid-totals-all-count")]').nth(1);
            const actualCount = await total.textContent();
            let formatedCount: number | null = null;
            if (actualCount) {
                const match = actualCount.match(/Total\s+(\d+)/);
                if (match) {
                    formatedCount = parseInt(match[1], 10);
                }
            }
            expect(formatedCount, `Expected Coding Rows Count: ${expectedCount}, but found: ${formatedCount}`).toBe(expectedCount);
        });
    }

    async closeSupplierInvoice() {
        await allure.step('Close Supplier Invoice', async () => {
            const tab = this.page.locator('a.nav-link', {
                has: this.page.locator('label', { hasText: /Supplier Invoice Inv-Playwright-/ })
            });
            await tab.waitFor({ state: 'visible' });
            const closeIcon = tab.locator('i[title="Close"]');
            await closeIcon.waitFor({ state: 'visible' });
            await closeIcon.click();
        });
    }

    async checkPreliminaryInvoice() {
        await allure.step('Check Preliminary Invoice', async () => {
            const preliminaryCheckbox = this.page.locator('#ctrl_draft');
            await preliminaryCheckbox.waitFor({ state: 'visible' });
            const isChecked = await preliminaryCheckbox.isChecked();
            if (!isChecked) {
                await preliminaryCheckbox.check();
                await this.page.waitForTimeout(150);
            }
        });
    }

    async showOpenInvoices(isShowOpen: boolean = true) {
        return await allure.step('Show Open Invoices', async () => {
            const openInvoices = this.page.locator('#ctrl_loadOpen');
            await openInvoices.waitFor({ state: 'visible' });
            if (isShowOpen) {
                if (!await openInvoices.isChecked()) {
                    const [response] = await Promise.all([
                        this.page.waitForResponse(resp => resp.url().includes('/api/Economy/Supplier/Invoices/Grid') && resp.status() === 200),
                        openInvoices.check({ force: true })
                    ]);
                    const data = await response.json();
                    return data.length;
                }
            } else {
                if (await openInvoices.isChecked()) {
                    await openInvoices.uncheck({ force: true });
                    await this.waitForDataLoad('/api/Economy/Supplier/Invoices/Grid', 10000);
                }
            }
            await this.page.waitForTimeout(3000);
        });
    }

    async showClosedInvoices(isShowClosed: boolean = true) {
        return await allure.step('Show Closed Invoices', async () => {
            const closedInvoices = this.page.locator('#ctrl_loadClosed');
            await closedInvoices.waitFor({ state: 'visible' });
            if (isShowClosed) {
                const [response] = await Promise.all([
                    this.page.waitForResponse(resp => resp.url().includes('/api/Economy/Supplier/Invoices/Grid') && resp.status() === 200),
                    closedInvoices.check()
                ]);
                const data = await response.json();
                return data.length;
            } else {
                if (await closedInvoices.isChecked()) {
                    await closedInvoices.uncheck();
                    await this.waitForDataLoad('/api/Economy/Supplier/Invoices/Grid', 10000);
                }
            }
            await this.page.waitForTimeout(2000);
        });
    }

    async verifyInvoiceCount(expectedCount: number, isZero: boolean = false) {
        await allure.step(`Verify Invoice Count: ${expectedCount}`, async () => {
            if (isZero) {
                await this.supplierInvoiceGrid.waitForPageLoad();
                const actualCount = await this.supplierInvoiceGrid.getAgGridRowCount();
                console.log('✅ Total count:', actualCount);
                expect(actualCount, `Expected no invoices, but found ${actualCount}`).toBe(0);
            } else {
                const totalText = await this.page.locator("//span[contains(@class, 'soe-ag-grid-totals-all-count') and contains(text(), 'Total')]").innerText();
                const totalMatch = totalText.match(/Total\s+(\d+)/i);
                const actualCount = totalMatch ? Number(totalMatch[1]) : 0;
                console.log('✅ Total count:', actualCount);
                expect(Number(actualCount), `Expected ${actualCount} invoices, greater than ${expectedCount}`).toBeGreaterThan(0);
                expect(Number(actualCount), `Expected ${expectedCount} invoices, but found ${actualCount}`).toBe(expectedCount);
            }
        });
    }

    async reloadSupplierInvoiceGrid() {
        await allure.step('Reload Supplier Invoice Grid', async () => {
            const reloadButton = this.page.locator('//a[@title="Reload records" and contains(@class, "fa-sync")]');
            await reloadButton.waitFor({ state: 'visible' });
            await reloadButton.click();
        });
    }

    async verifyTotalAmount(expectedAmount: string) {
        await allure.step(`Verify Total Amount: ${expectedAmount}`, async () => {
            const actualTotalAmount = this.page.locator(`#ctrl_invoice_totalAmountCurrency`);
            await actualTotalAmount.waitFor({ state: 'visible' });
            const amount = await actualTotalAmount.inputValue();
            const formattedAmount = amount.replace(/\s/g, "").trim();
            expect(formattedAmount, `Expected Total Amount: ${expectedAmount}, but found: ${formattedAmount}`).toBe(expectedAmount);
        });
    }

    async selectInvoicePeriod(invoicePeriod: string) {
        await allure.step(`Select Invoice Period: ${invoicePeriod}`, async () => {
            const invoicePeriodDropdown = this.page.locator('#ctrl_allItemsSelection');
            await invoicePeriodDropdown.waitFor({ state: 'visible' });
            await invoicePeriodDropdown.selectOption({ label: invoicePeriod });
            await this.page.waitForTimeout(3000);
        });
    }

    async createItem(): Promise<void> {
        await allure.step('Create Item', async () => {
            await this.page.waitForLoadState('networkidle');
            const newItemButton = this.page.locator(`//ul/li[@index!="tab.index"]`).nth(0)
            const tabs = this.page.locator(`//li[@index="tab.index"]`)
            const initTabCount = await tabs.count();
            await newItemButton.click({ force: true });
            const finalTabCount = await tabs.count();
            expect(finalTabCount, 'New tab was not created').toBe(initTabCount + 1);
            await expect.poll(async () => {
                const title = await this.page.locator('#ctrl_selectedVoucherDate').inputValue() ?? '';
                return title
            }, { timeout: 30_000 }).not.toBe('');
        });
    }

    async enterInvoiceNumber(invoiceNumber: string): Promise<void> {
        await allure.step(`Enter Invoice Number: ${invoiceNumber}`, async () => {
            const invoiceNumberInput = this.page.locator('#ctrl_invoice_invoiceNr');
            await invoiceNumberInput.waitFor({ state: 'visible' });
            await invoiceNumberInput.fill(invoiceNumber);
        });
    }

    async enterInvoiceDate(invoiceDate: string): Promise<void> {
        await allure.step(`Enter Invoice Date: ${invoiceDate}`, async () => {
            const invoiceDateInput = this.page.locator('#ctrl_selectedInvoiceDate');
            await invoiceDateInput.waitFor({ state: 'visible' });
            await invoiceDateInput.fill(invoiceDate);
            await this.page.keyboard.press('Enter');
            await this.page.waitForTimeout(500);
        });
    }

    async enterTotalAmount(totalAmount: string): Promise<void> {
        await allure.step(`Enter Total Amount: ${totalAmount}`, async () => {
            const totalAmountInput = this.page.locator('#ctrl_invoice_totalAmountCurrency');
            await totalAmountInput.waitFor({ state: 'visible' });
            await totalAmountInput.fill(totalAmount);
            await this.page.waitForTimeout(3000);
        });
    }

    async verifyInvoiceNumberInGrid(expectedInvoiceNumber: number = 1): Promise<void> {
        await allure.step(`Verify Invoice Number in Grid: ${expectedInvoiceNumber}`, async () => {
            const rows = this.page.locator('//div[@ref="eContainer"]/div[@role="row"]')
            await expect.poll(async () => await rows.count(), { timeout: 10000 }).toBe(expectedInvoiceNumber);
        });
    }

    async selectInvoiceInGrid(invoiceNumber: number = 0): Promise<void> {
        await allure.step(`Select Invoice in Grid: ${invoiceNumber}`, async () => {
            const invoiceRow = this.page.locator(`//div[@name="left"]//div[@col-id="soe-row-selection"]`).nth(invoiceNumber);
            await invoiceRow.click();
        });
    }

    async transferInvoiceTo(transferTo: string): Promise<void> {
        await allure.step('Transfer Invoice To', async () => {
            const functionButton = this.page.locator('//div[@label-key="core.functions"]')
            await functionButton.locator('[data-toggle="dropdown"]').click();
            const options = functionButton.locator('ul>li>a').all();
            for (let option of await options) {
                const optionText = await option.innerText();
                if (optionText?.trim() === transferTo) {
                    await option.click();
                    break;
                }
            }
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
            await this.waitForDataLoad('/api/Economy/Accounting/AccountBalance/CalculateAccountBalanceForAccountsFromVoucher/');
        });
    }

    async verifyVoucherStatus() {
        await allure.step('Verify Voucher Status', async () => {
            const voucherStatus = await this.page.locator('//div[@col-id="statusName" and @role="gridcell"]').innerText();
            expect(voucherStatus).toBe('Voucher');
        });
    }
}