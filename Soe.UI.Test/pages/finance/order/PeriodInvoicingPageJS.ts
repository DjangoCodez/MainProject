import { expect, Page } from "@playwright/test";
import { BasePage } from "../../common/BasePage";
import * as allure from "allure-js-commons";
import { SectionGridPageJS } from "../../common/SectionGridPageJS";
import { ModelGridPageJS } from "../../common/ModelGridPageJS";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";

export class PeriodInvoicingPageJS extends BasePage {

    readonly page: Page;
    readonly registerTimeGrid: SectionGridPageJS;
    readonly periodInvoicingGrid: SingleGridPageJS;
    readonly customerInvoicesGrid: SingleGridPageJS;
    readonly timesGrid: SingleGridPageJS;
    readonly productsGrid: SectionGridPageJS;

    constructor(page: Page) {
        super(page);
        this.page = page;
        this.registerTimeGrid = new ModelGridPageJS(page, 'ctrl.gridHandler.gridAg.options.gridOptions');
        this.periodInvoicingGrid = new SingleGridPageJS(page, 0, 'ctrl.gridAg.options.gridOptions');
        this.customerInvoicesGrid = new SingleGridPageJS(page, 1, 'ctrl.gridAg.options.gridOptions');
        this.timesGrid = new SingleGridPageJS(page, 0, 'directiveCtrl.soeGridOptions.gridOptions');
        this.productsGrid = new SectionGridPageJS(page, 'billing.order.productrows', 'directiveCtrl.soeGridOptions.gridOptions');
    }

    async untickOnlyReadyForInvoice() {
        await allure.step("Untick 'Only Ready for Invoice' checkbox", async () => {
            const checkbox = this.page.locator('#ctrl_onlyValidToTransfer');
            await checkbox.waitFor({ state: 'visible' });
            if (await checkbox.isChecked()) {
                await checkbox.uncheck();
            }
        });
    }

    async search() {
        await allure.step("Click search button", async () => {
            await this.page.getByRole('button', { name: 'Search' }).click();
            await this.waitForDataLoad('api/Billing/Order/HandleBilling/Search/');
        });
    }

    async desOrderNo() {
        await allure.step("Click on 'Order No.'", async () => {
            const orderNo = this.page.locator(`//span[text()='Order No.']`);
            await orderNo.waitFor({ state: 'visible' });
            await orderNo.click();
        });
    }

    async clickEditIconOnOrder(orderNo: string) {
        await allure.step("Click edit icon on order", async () => {
            await this.periodInvoicingGrid.filterByName('Order No.', orderNo);
            const editIcon = this.page.locator(`//div[@col-id='invoiceNr' and .//span[text()='${orderNo}']]//button[contains(@class, 'gridCellIcon') and contains(@class, 'iconEdit') and contains(@class, 'fa-pencil')]`);
            await editIcon.first().waitFor({ state: 'visible' });
            await editIcon.first().click();
        });
    }

    async expandTimes() {
        await allure.step("Expand times section", async () => {
            const expandButton = this.page.locator(`//label[contains(@class, 'control-label') and contains(text(), 'Times')]`);
            await expandButton.waitFor({ state: 'visible' });
            await expandButton.click();
        });
    }

    async loadAllTimes() {
        await allure.step("Load all times", async () => {
            const caretLocator = this.page.locator("//div[@label-key='billing.order.timeproject.searchintervall']//span[@class='caret']")
            await caretLocator.click();
            const loadAllButton = this.page.getByRole('link', { name: 'Load all times' })
            await loadAllButton.waitFor({ state: 'visible' });
            await loadAllButton.click();
        });
    }

    async addTimeRow() {
        await allure.step("Add time row", async () => {
            const addTimeButton = this.page.getByRole('button', { name: 'Add Row' });
            await addTimeButton.waitFor({ state: 'visible' });
            await addTimeButton.click();
        });
    }

    private async selectFromList(item: string) {
        await expect(this.page.locator('//ul[@class="typeahead dropdown-menu"]')).toBeVisible({ timeout: 5000 });
        const fprducts = await this.page.locator('//ul[@class="typeahead dropdown-menu"]/li/a').all()
        for (let prd of fprducts) {
            const name = await prd.innerText()
            if (name.includes(item)) {
                await prd.click()
                break;
            }
        }
    }

    async setEmployee(employeeName: string) {
        const input = this.page.locator('#typeahead-editor');
        await input.fill('');
        await input.fill(employeeName);
        await this.selectFromList(employeeName);
        await this.page.waitForTimeout(1000);
    }

    async setChargingType(chargingType: string, rowIndex: number = 0) {
        const ele = this.page.locator(`//div[@class="modal-body"]//div[@row-index="${rowIndex}"]//div[@col-id="timeCodeName" and @role="gridcell"]`)
        await ele.click();
        await this.page.locator('#typeahead-editor').fill(chargingType);
        await this.selectFromList(chargingType);
        await this.page.waitForTimeout(1000);
    }

    async setDateInRegisterTimeGrid(date: string, rowIndex: number = 0) {
        const datePicker = this.page.locator(`//div[@class="modal-body"]//div[@row-index="${rowIndex}"]//div[@col-id="date" and @role="gridcell"]`)
        const calendar = this.page.locator('//ul[contains(@class,"uib-datepicker-popup")]');
        await calendar.waitFor({ state: 'visible' });
        await datePicker.locator('input').fill(date);
        await this.page.click('body'); // Click outside to close the date picker
    }

    async addRegisteredTime(employeeName: string, timeWorked: string, billableTime: string, date: string, rowIndex: number = 0, addnewRow: boolean = false, submit: boolean = false) {
        await allure.step("Add registered time", async () => {
            if (addnewRow) {
                const newRow = this.page.locator(`//button[@type='button' and contains(@class, 'btn-default') and @title='Ctrl+R' and text()='Add Row']`);
                await newRow.waitFor({ state: 'visible' });
                await newRow.click();
            }
            await this.setEmployee(employeeName);
            await this.setDateInRegisterTimeGrid(date, rowIndex);
            await this.setChargingType('Övertid kl 16-18', rowIndex);
            await this.page.waitForTimeout(2500);
            await this.registerTimeGrid.enterGridValueByColumnId('timePayrollQuantityFormattedEdit', timeWorked, rowIndex);
            await this.page.waitForTimeout(200);
            await this.registerTimeGrid.enterGridValueByColumnId('invoiceQuantityFormatted', billableTime, rowIndex);
            await this.page.waitForTimeout(200);
            if (submit) {
                const saveButton = this.page.locator(`//button[@type='button' and contains(@class, 'btn-primary') and @title='Ctrl+S' and text()='Save']`);
                await saveButton.waitFor({ state: 'visible' });
                await saveButton.click();
                await this.page.waitForTimeout(1000);
                const okButton = this.page.getByRole('button', { name: 'OK' });
                if (await okButton.isVisible()) {
                    await okButton.click();
                }
                await this.waitForDataLoad('/api/Core/Project/ProjectTimeBlockSaveDTO/');
                await this.page.waitForTimeout(1000);
            }
        });
    }

    async verifyGridValueUpdated(timeWorked: string, billableTime: string) {
        await allure.step("Verify grid value updated", async () => {
            const timePayrollQuantityFormatted = this.page.locator(`//div[@row-index=2]//div[@role="gridcell" and @col-id="timePayrollQuantityFormatted"]`);
            const invoiceQuantityFormatted = this.page.locator(`//div[@row-index=2]//div[@role="gridcell" and @col-id="invoiceQuantityFormatted"]`);
            await expect.poll(async () => await timePayrollQuantityFormatted.innerText(), { timeout: 15000 }).toBe(timeWorked); // Wait until the value is updated to 3
            await expect.poll(async () => await invoiceQuantityFormatted.innerText(), { timeout: 15000 }).toBe(billableTime); // Wait until the value is updated to 3

            // Below lines are alternative way to verify the values using grid methods
            const timeWorkedUI = await this.timesGrid.getRowColumnValue('timePayrollQuantityFormatted', 2);
            const billableTimeUI = await this.timesGrid.getRowColumnValue('invoiceQuantityFormatted', 2);
            expect(timeWorkedUI, `Expected time worked to be ${timeWorkedUI} but found ${timeWorked}`).toBe(timeWorked);
            expect(billableTimeUI, `Expected billable time to be ${billableTimeUI} but found ${billableTime}`).toBe(billableTime);
        });
    }

    async closeOrder() {
        await allure.step("Close order", async () => {
            const closeButton = this.page.locator(`//i[@title='Close' and contains(@class, 'fa-times')]`);
            await closeButton.waitFor({ state: 'visible' });
            await closeButton.scrollIntoViewIfNeeded();
            await closeButton.click();
        });
    }

    async reloadAllTimes() {
        await allure.step("Reload all times", async () => {
            await this.page.waitForTimeout(1500);
            const reloadButton = this.page.locator(`//button[contains(@class, 'ngSoeMainButton') and normalize-space(text())='Load all times']`);
            await reloadButton.waitFor({ state: 'visible' });
            await reloadButton.scrollIntoViewIfNeeded();
            await this.page.waitForTimeout(500);
            await reloadButton.click();
            await this.waitForDataLoad('/api/Core/Project/TimeBlock', 35000);
        });
    }

    async filterByOrderNo(orderNo: string) {
        await allure.step("Filter order by order number", async () => {
            await this.periodInvoicingGrid.filterByName('Order No.', orderNo);
        });
    }

    async selectedAllTimeEntries() {

        return await allure.step("Select all items in the grid with clock icon", async () => {
            await this.periodInvoicingGrid.selectAllCheckBox()
            return await this.page.locator('//div[contains(@class,"active")]//div[@name="left"]/div[@row-id!="rowGroupFooter_ROOT_NODE_ID"]').count();
        });
    }

    async reloadPeriodInvoicingGrid() {
        await allure.step("Reload Period Invoicing Grid", async () => {
            const reloadGrid = this.page.locator(`//a[@title="Reload records"]`);
            await reloadGrid.waitFor({ state: 'visible' });
            await reloadGrid.click({ force: true });
            await this.waitForDataLoad('api/Billing/Order/HandleBilling/Search/');
        });
    }

    async changeKlar() {
        await allure.step("Change Klar", async () => {
            await this.page.waitForSelector('#ctrl_selectedAttestState', { state: 'visible' });
            await this.page.selectOption('#ctrl_selectedAttestState', { label: 'Klar' });
        });
    }

    async clickRun() {
        await allure.step("Click Run", async () => {
            const runButton = this.page.getByTitle('Change row status');
            await runButton.waitFor({ state: 'visible' });
            await runButton.click();
            await this.clickAlertMessage('OK');
            await this.waitForDataLoad('api/Billing/Order/HandleBilling/ChangeAttestState/');
        });
    }

    async verifychangeToKlar(entries: number) {
        await allure.step("Verify change to Klar", async () => {
            const circles = this.page.locator('div[role="gridcell"][col-id="attestStateNames"] svg circle');
            await circles.first().waitFor({ state: 'visible' });
            const total = await circles.count();
            const styles = await circles.evaluateAll(nodes =>
                nodes.map(n => ({
                    inlineStyle: n.getAttribute('style'),
                    computedFill: getComputedStyle(n).fill
                }))
            );
            const BLUE_RGB = 'rgb(77, 71, 237)';
            const blueCount = styles.filter(s => s.computedFill === BLUE_RGB || (s.inlineStyle || '').replace(/\s/g, '').toLowerCase().includes('fill:#4d47ed')).length;
            expect(entries, `Expected ${entries} entries to be in 'Klar' status but found ${blueCount}`).toBe(blueCount);
        });
    }

    async transferToPreliminaryInvoice() {
        await allure.step("Transfer to Preliminary Invoice", async () => {
            const transferButton = this.page.locator(`//div[@form='ctrl.edit']//button[contains(@class, 'dropdown-toggle')]//span[@class='caret']`).nth(5);
            await transferButton.waitFor({ state: 'visible' });
            await transferButton.click();
            await this.page.getByRole('link', { name: 'Transfer to preliminary invoice' }).click();
            await this.clickAlertMessage('OK');
            await this.waitForDataLoad('api/Billing/Order/HandleBilling/TransferOrdersToInvoice/');
            await this.page.waitForTimeout(500);
        });
    }

    async moveToCustomerInvoicesTab() {
        await allure.step("Move to Customer Invoices Tab", async () => {
            const customerInvoicesTab = this.page.getByRole('link', { name: 'Customer Invoices' });
            await customerInvoicesTab.waitFor({ state: 'visible' });
            await customerInvoicesTab.click();
            await this.waitForDataLoad('api/Core/CustomerInvoices/');
        });
    }

    async filterByOrderNoInvoice(orderNo: string) {
        await allure.step("Filter by Order No in Customer Invoices", async () => {
            await this.customerInvoicesGrid.filterByName('Order No.', orderNo);
        });
    }

    async reloadCustomerInvoicesGrid() {
        await allure.step("Reload Customer Invoices Grid", async () => {
            const reloadGrid = this.page.locator(`//a[@title="Reload records"]`);
            await reloadGrid.nth(1).waitFor({ state: 'visible' });
            await reloadGrid.nth(1).click();
            await this.waitForDataLoad('api/Core/CustomerInvoices/');
        });
    }

    async editSelectedInvoice() {
        await allure.step("Edit Selected Invoice", async () => {
            const editButton = this.page.locator(`//button[@class="gridCellIcon fal fa-pencil iconEdit"]`);
            await editButton.waitFor({ state: 'visible' });
            await editButton.click();
        });
    }

    async transferToTimeRowsToProductRows() {
        await allure.step("Transfer time rows to product rows", async () => {
            await this.page.waitForSelector(`.soe-ag-totals-row-part.soe-ag-grid-totals-all-count`);
            const selectAllCheckboxes = this.page.locator(`//div[contains(@class, 'ag-header-row') and contains(@class, 'ag-header-row-column')]//input[@type='checkbox' and contains(@class, 'ag-checkbox-input')]`);
            await selectAllCheckboxes.nth(1).scrollIntoViewIfNeeded();
            await selectAllCheckboxes.nth(1).waitFor({ state: 'visible' });
            await selectAllCheckboxes.nth(1).check({ force: true });
            const transferButton = this.page.locator(`//div[@form='ctrl.edit']//button[contains(@class, 'dropdown-toggle')]//span[@class='caret']`).nth(5);
            await transferButton.waitFor({ state: 'visible' });
            await transferButton.click();
            await this.page.getByRole('link', { name: 'Move time rows to new product rows' }).click();
            await this.clickAlertMessage('OK');
            await this.waitForDataLoad('/api/Billing/Order/HandleBilling/Search/');
        });
    }

    async expandProductRows() {
        await allure.step("Expand products tab", async () => {
            const productsTab = this.page.getByRole('button', { name: 'Product rows' });
            await productsTab.scrollIntoViewIfNeeded();
            await productsTab.click();
            await this.waitForDataLoad('/api/Billing/Product/ProductRows/List/');
        });
    }

    async verifyProductQuantityEqualsTimeQuantity(timeQuantity: string) {
        await allure.step("Verify product quantity equals time quantity", async () => {
            const productQuantity = await this.productsGrid.getCellvalueByColIdandGrid('quantity', 2);
            expect(productQuantity, `Expected product quantity to be ${productQuantity} but found ${timeQuantity}`).toBe(timeQuantity);
        });
    }
}