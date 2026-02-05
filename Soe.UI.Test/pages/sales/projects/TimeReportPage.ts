import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../SalesBasePage";
import * as allure from "allure-js-commons";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";
import { ModelGridPageJS } from '../../common/ModelGridPageJS';

export class TimeReportPage extends SalesBasePage {
    readonly timeReportGrid: SingleGridPageJS;
    readonly registerTimeGrid: ModelGridPageJS;

    constructor(page: Page) {
        super(page);
        this.timeReportGrid = new SingleGridPageJS(page, 0, 'directiveCtrl.soeGridOptions.gridOptions');
        this.registerTimeGrid = new ModelGridPageJS(page, 'ctrl.gridHandler.gridAg.options.gridOptions');
    }

    async clickAddRow() {
        await allure.step("Click Add row button", async () => {
            await this.page.getByRole('button', { name: 'Add Row' }).click();
            await this.page.waitForTimeout(1_000);
        });
    }

    async waitForPageLoad() {
        await allure.step("Wait for Order page to load", async () => {
            await this.waitForDataLoad('/api/Billing/Project/Employees/');
            await this.page.waitForTimeout(3000);
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
        const employeeInput = this.page.locator('#typeahead-editor');
        await employeeInput.waitFor({ state: 'visible' });
        await employeeInput.fill(employeeName);
        await this.selectFromList(employeeName);
        await this.page.waitForTimeout(1000);
    }

    async setOrder(orderNumber: string) {
        await this.page.locator('#typeahead-editor').fill(orderNumber);
        await this.selectFromList(orderNumber);
        await this.page.waitForTimeout(1000);
    }

    async setProject(projectNumber: string) {
        await this.page.locator('#typeahead-editor').fill(projectNumber);
        await this.selectFromList(projectNumber);
        await this.page.waitForTimeout(1000);
    }

    async setChargingType(chargingType: string) {
        await this.page.locator('//div[@class="modal-body"]//div[@col-id="timeCodeName" and @role="gridcell"]').click();
        await this.selectFromList(chargingType);
        await this.page.waitForTimeout(1000);
    }
    async setTimeWorked(time: string) {
        const ele = this.page.locator('//div[@class="modal-body"]//div[@col-id="timePayrollQuantityFormattedEdit" and @role="gridcell"]')
        await ele.click();
        await ele.locator('input').fill(time);
        await this.page.waitForTimeout(1000);
    }
    async setBillableTime(billableTime: string) {
        const ele = this.page.locator('//div[@class="modal-body"]//div[@col-id="invoiceQuantityFormatted" and @role="gridcell"]')
        await ele.click();
        await ele.locator('input').fill(billableTime);
        await this.page.waitForTimeout(1000);
    }
    async setExternalNote(externalNote: string) {
        const ele = this.page.locator('//div[@class="modal-body"]//div[@col-id="externalNote" and @role="gridcell"]')
        await ele.click();
        await ele.locator('input').fill(externalNote);
        await this.page.waitForTimeout(1000);
    }

    async setInternalNote(internalNote: string) {
        const ele = this.page.locator('//div[@class="modal-body"]//div[@col-id="internalNote" and @role="gridcell"]')
        await ele.click();
        await ele.locator('input').fill(internalNote);
        await this.page.waitForTimeout(1000);
    }

    async setRegisterTimeDetails({ externalNote = 'External note', internalNote = 'Internal note', employeeName = '', orderNumber = '', projectNumber = '', chargingType = '', timeWorked = '', billableTime = '', rowIndex = 0 } = {}, deleteEmptyRow: boolean = false) {
        await allure.step("Add work time", async () => {
            await this.registerTimeGrid.waitForPageLoad();
            const modalContent = this.page.locator("//div[@class='modal-content']");
            expect(modalContent.getByRole('heading', { name: 'Register time' })).toBeVisible();
            await this.page.waitForTimeout(1000);
            await this.setEmployee(employeeName);
            await this.setOrder(orderNumber);
            await this.setProject(projectNumber);
            await this.setChargingType(chargingType);
            await this.setTimeWorked(timeWorked);
            await this.setBillableTime(billableTime);
            await this.setExternalNote(externalNote);
            await this.setInternalNote(internalNote);
            if (deleteEmptyRow) {
                await this.registerTimeGrid.clickButtonByColumnId('delete', rowIndex + 1);
            }
        })

    }

    // Temporary method to handle OK button in popup
    async clickOkButton() {
        await allure.step("Click OK button in popup if visible", async () => {
            await this.page.waitForTimeout(500);
            const popup = this.page.locator('//div[@class="modal-header"]/h6[text()="Warning"]')
            const isOkButtonVisible = await Promise.race([
                await popup.evaluate(el => {
                    const style = window.getComputedStyle(el);
                    return (
                        style.display !== 'none' &&
                        style.visibility !== 'hidden' &&
                        style.opacity !== '0' &&
                        el.getBoundingClientRect().width > 0 &&
                        el.getBoundingClientRect().height > 0
                    );
                }).catch(() => false),
                await this.page.waitForTimeout(1500).then(() => false)
            ])
            if (isOkButtonVisible) {
                const okButton = this.page.getByRole('button', { name: 'OK' });
                await okButton.click();
            }
        });
    }

    async saveRegisteredTime() {
        await allure.step("Save registered time", async () => {
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.clickOkButton();
            await this.page.waitForTimeout(3000);
        });
    }

    async setDateFilter(fromDate: string, toDate: string) {
        await allure.step(`Set date filter: ${fromDate} → ${toDate}`, async () => {
            await this.page.locator('#directiveCtrl_fromDate').nth(0).fill(fromDate);
            await this.page.keyboard.press('Tab');
            await this.page.locator('#directiveCtrl_toDate').nth(0).fill(toDate);
            await this.page.getByRole('button', { name: 'Search' }).click();
            await this.page.waitForTimeout(2000);
        });
    }

    async applyFilters(filters: Record<string, string>) {
        await allure.step("Apply filters", async () => {
            await this.clearAllFilters();
            const activeTab = this.page.locator('//div[contains(@class,"active") and contains(@class,"tab-pane") and not(@id)]');
            const row = activeTab.locator('//div[@ref="eContainer"]/div[@role="row" and @row-id!="rowGroupFooter_ROOT_NODE_ID"]');
            const initialRowCount = await row.count();
            console.log(`Initial row count: ${initialRowCount}`);
            for (const [columnName, value] of Object.entries(filters)) {
                const normalizedColumn = columnName.toLowerCase();
                const selector = `//div[@id="time-rows-grid"]//input[@type='text'and contains(translate(@aria-label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'${normalizedColumn}')]`;
                const input = this.page.locator(selector).first();
                await input.waitFor({ state: "visible", timeout: 5000 });
                console.log(`Applying filter - ${columnName}: ${value}`);
                await input.fill("");
                await this.page.waitForTimeout(200);
                await input.fill(value);
                await this.page.waitForTimeout(800);
            }
            await expect.poll(async () => await row.count(), { timeout: 10000 }).toBeLessThanOrEqual(initialRowCount);
            await this.page.waitForTimeout(1000);
        });
    }

    async VerifyRowCount(actualRowCount: number = 1) {
        await allure.step("Verify row count", async () => {
            const activeTab = this.page.locator('//div[contains(@class,"active") and contains(@class,"tab-pane") and not(@id)]')
            const row = activeTab.locator('//div[@ref="eContainer"]/div[@role="row" and @row-id!="rowGroupFooter_ROOT_NODE_ID"]')
            await expect.poll(async () => await row.count(), { timeout: 20000 }).toBe(actualRowCount);
            const rowCount = await this.timeReportGrid.getFilteredAgGridRowCount();
            expect(rowCount).toBe(actualRowCount);
        });
    }

    async setOrderNumberTopFilter(orderNumber: string) {
        await allure.step(`Set order number top filter: ${orderNumber}`, async () => {
            await this.page.getByRole('button', { name: 'Order' }).click();
            const searchBox = this.page.getByRole('textbox', { name: 'Search' });
            await searchBox.fill(orderNumber);
            await this.page.getByRole('checkbox', {
                name: new RegExp(`^${orderNumber} - Playwright Test Customer$`)
            }).check();
            await this.page.getByRole('button', { name: 'Filter', exact: true }).click();
            await this.page.waitForTimeout(1000);
        });
    }

    async clickSearchButton() {
        await allure.step("Click Search button", async () => {
            await this.page.getByRole('button', { name: 'Search' }).click();
            await this.page.waitForTimeout(2_000);
        });
    }

    async setProjectNumberTopFilter(projectNumber: string) {
        await this.page.getByRole('button', { name: 'Projects' }).click();
        const searchBox = this.page.getByRole('textbox', { name: 'Search' });
        await searchBox.fill(projectNumber);
        await this.page.getByRole('checkbox', {
            name: new RegExp(`^${projectNumber}\\s*-?\\s*Playwright Test Customer$`)
        }).check();
        await this.page.getByRole('button', { name: 'Filter', exact: true }).click();
        await this.page.waitForTimeout(1000);
    }

    async setEmployeeTopFilter(employeeName: string) {
        await this.page.getByRole('button', { name: 'Employe' }).click();
        const searchBox = this.page.getByRole('textbox', { name: 'Search' });
        await searchBox.fill(employeeName);
        await this.page.getByRole('checkbox', {
            name: new RegExp(`^${employeeName}`)
        }).check();
        await this.page.getByRole('button', { name: 'Filter', exact: true }).click();
        await this.page.waitForTimeout(1000);
    }

    async selectFirstRow() {
        await allure.step("Select first row in time report grid", async () => {
            await this.timeReportGrid.selectCheckBox(0);
            await this.page.waitForTimeout(1000);
        });
    }

    async deleteSelectedTimeEntries() {
        await allure.step("Delete selected time entries", async () => {
            const deleteSplitButton = this.page.locator('div.btn-group').filter({ has: this.page.locator('ul.dropdown-menu a:has-text("Delete row"):not(.disabled-link)') });
            await deleteSplitButton.locator('button.dropdown-toggle').click();
            await deleteSplitButton.locator('a:has-text("Delete row")').click();
            await this.page.getByRole('button', { name: 'OK' }).click();
            await this.waitForDataLoad('api/Core/Project/TimeBlocksForTimeSheetFiltered/');
        });
    }

    async clickEditButton() {
        await allure.step("Click Edit button", async () => {
            await this.timeReportGrid.clickButtonByColumnId('edit', 0);
            await this.page.waitForTimeout(3000);
        });
    }

    async deleteTimeEntryInEditMode(orderNumber: string) {
        await allure.step("Delete time entry in edit mode", async () => {
            const result = await this.registerTimeGrid.getCellValueFromGrid('invoiceNr', orderNumber, 'invoiceNr', false, true);
            if (result && typeof result !== 'string') {
                const { rowIndex } = result;
                await this.registerTimeGrid.clickButtonByColumnId('delete', rowIndex);
            }
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.waitForDataLoad('api/Core/Project/TimeBlocksForTimeSheetFiltered/');
        });
    }

    async setAttestationLevel(attestationLevel: string) {
        await allure.step(`Set attestation level: ${attestationLevel}`, async () => {
            await this.page.getByRole('button', { name: 'Attestation level' }).click();
            await this.page.waitForTimeout(500);
            await this.page.getByRole('link', { name: attestationLevel }).click();
            await this.clickAlertMessage('OK');
            await this.waitForDataLoad('/api/Time/Time/Attest/Transactions/');
        });
    }

    async verifyAttestationLevel(expectedStatus: 'Attest' | 'Registrerad', rowIndex: number = 0, timeout: number = 10000) {
        const expectedColor = expectedStatus === 'Attest' ? '#16C402' : '#FF030B';
        const cellLocator = this.timeReportGrid.rootLocator.locator(`//div[@row-index='${rowIndex}']//div[@col-id="timePayrollAttestStateName"]`);
        await cellLocator.waitFor({ state: 'visible', timeout: 5000 });
        const circleLocator = cellLocator.locator('svg circle');
        await expect(async () => {
            const style = await circleLocator.first().getAttribute('style') || '';
            console.log(`Current attestation status: "${style.includes('#16C402') ? 'Attest (Green)' : style.includes('#FF030B') ? 'Registrerad (Red)' : 'Unknown'}", Expected: "${expectedStatus}"`);
            expect(style.toLowerCase()).toContain(expectedColor.toLowerCase());
        }).toPass({ timeout, intervals: [500] });
    }

    async verifyCellEditability(colId: string, shouldBeEditable: boolean, rowIndex: number = 0) {
        const cellLocator = this.registerTimeGrid.rootLocator.locator(`//div[@row-index='${rowIndex}']//div[@role="gridcell" and @col-id="${colId}"]`);
        await cellLocator.waitFor({ state: 'attached', timeout: 3000 });
        await cellLocator.scrollIntoViewIfNeeded();
        await cellLocator.click({ force: true });
        const isInEditMode = await cellLocator.evaluate(cell =>
            cell.classList.contains('ag-cell-inline-editing') ||
            cell.classList.contains('ag-cell-popup-editing') ||
            cell.querySelector('input, select') !== null
        );
        expect(isInEditMode).toBe(shouldBeEditable);
        console.log(`${colId} - Editable: ${isInEditMode}, Expected: ${shouldBeEditable}`);
    }

    async verifyRowCellsEditability(editabilityMap: Record<string, boolean>, rowIndex: number = 0) {
        for (const [colId, shouldBeEditable] of Object.entries(editabilityMap)) {
            await this.verifyCellEditability(colId, shouldBeEditable, rowIndex);
        }
    }

    async closeRegisterTimeModal() {
        await allure.step("Close Register Time modal", async () => {
            const closeButton = await this.page.locator('button.close:visible');
            await closeButton.click();
            await this.page.waitForTimeout(1000);
        });
    }


}