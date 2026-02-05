import { expect, Page } from "@playwright/test";
import * as allure from "allure-js-commons";
import { BasePage } from "pages/common/BasePage";
import { GridPageJS } from "pages/common/GridPageJS";

export class ProcessingHistoryPage extends BasePage {

    readonly page: Page;
    readonly processingHistoryGrid: GridPageJS;

    constructor(page: Page) {
        super(page);
        this.page = page;
        this.processingHistoryGrid = new GridPageJS(page, "ctrl.gridAg.options.gridOptions");
    }

    async waitForProcessingHistoryPageLoad() {
        await allure.step("Wait For Page Load", async () => {
            await this.page.locator("//label[contains(@class,'ng-binding') and text()='Processing History']").waitFor({ state: 'visible' });
        });
    }

    async verifyFromDate(expectedDate: string) {
        await allure.step("Verify From date", async () => {
            const fromDate = this.page.locator("#ctrl_fromDate");
            await fromDate.waitFor({ state: 'visible' });
            const fromDateValue = await fromDate.inputValue();
            const formattedFromDate = new Date(fromDateValue);
            const formattedExpectedDate = new Date(expectedDate);
            expect(formattedFromDate, `expected from date ${formattedExpectedDate} got ${formattedFromDate}`).toStrictEqual(formattedExpectedDate);
        });
    }

    async verifyToDate(expectedDate: string) {
        await allure.step("Verify To date", async () => {
            const ctrl_toDate = this.page.locator("#ctrl_toDate");
            await ctrl_toDate.waitFor({ state: 'visible' });
            const toDateValue = await ctrl_toDate.inputValue();
            const formattedToDate = new Date(toDateValue);
            const formattedExpectedDate = new Date(expectedDate);
            expect(formattedToDate, `expected to date ${formattedExpectedDate} got ${formattedToDate}`).toStrictEqual(formattedExpectedDate);
        });
    }

    async search() {
        await allure.step('click on search ', async () => {
            const search = this.page.locator("//button[@title='Search' and @type='button']");
            await search.waitFor({ state: 'visible' });
            await search.click();
        })
    }

    async filterBySupplierName(supplierName: string) {
        await allure.step('Enter Supplier ', async () => {
            await this.processingHistoryGrid.filterByName("Supplier", supplierName);
        });
    }

    async confirmFieldCreated(expectedRows: number) {
        await allure.step("Confirm Field Created: " + expectedRows, async () => {
            const rowCount = await this.processingHistoryGrid.getAgGridRowCount();
            expect(rowCount, `Expected rows: ${expectedRows}, but got: ${rowCount}`).toBe(expectedRows);
        });
    }

    async verifyType(expectedType: string, index: number = 0) {
        await allure.step("Verify Type: " + expectedType, async () => {
            const type = await this.processingHistoryGrid.getRowColumnValue("actionText", index);
            expect(type, `Expected Type: ${expectedType}, but got: ${type}`).toBe(expectedType);
        });
    }

    async editProcessingHistory(rowIndex: number = 0) {
        await allure.step("Edit Processing History at row index: " + rowIndex, async () => {
            const editButton = this.page.locator(`//button[contains(@class,'gridCellIcon') and contains(@class,'iconEdit')]`);
            await editButton.nth(rowIndex).waitFor({ state: 'visible' });
            await editButton.nth(rowIndex).click();
            await this.waitForDataLoad('/api/Core/ContactPerson/ContactPersonsByActorId/');
        });
    }

    async verifySupplierNumberNotNull() {
        await allure.step("Verify Supplier Number Not Null: ", async () => {
            const supplierNumber = this.page.locator("#ctrl_supplier_supplierNr");
            await supplierNumber.waitFor({ state: 'visible' });
            const supplierNumberValue = await supplierNumber.inputValue();
            expect(supplierNumberValue, `Expected Supplier Number to be not null or empty`).not.toBeNull();
        });
    }
}