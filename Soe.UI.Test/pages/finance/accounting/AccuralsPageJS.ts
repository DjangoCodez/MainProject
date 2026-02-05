import { expect, Page } from "@playwright/test";
import { BasePage } from "../../common/BasePage";
import { GridPageJS } from "../../common/GridPageJS";
import * as allure from "allure-js-commons";


export class AccuralsPageJS extends BasePage {
    readonly page: Page;
    readonly accruralsGrid: GridPageJS;
    readonly voucherGrid: GridPageJS;

    constructor(page: Page) {
        super(page);
        this.page = page;
        this.accruralsGrid = new GridPageJS(page, 'ctrl.gridAg.options.gridOptions');
        this.voucherGrid = new GridPageJS(page, 'directiveCtrl.soeGridOptions.gridOptions', false, 'common.accountingrows');
    }

    async filterbyVoucherNumber(voucherNumber: string) {
        await allure.step(`Filter Account By Vowucher Number: ${voucherNumber}`, async () => {
            await this.accruralsGrid.filterByName('Name', voucherNumber, 10000);
        });
    }

    async selectAccrualsGridRow() {
        await allure.step(`Select Accruals Grid Row`, async () => {
            this.page.getByRole('checkbox', { name: 'Press Space to toggle row' }).check({ timeout: 3000 });
        });
    }

    async clickAccurals() {
        await allure.step(`Click Accurals`, async () => {
            await this.page.getByTitle('Accrual').click({ timeout: 3000 });
        });
    }

    async clickVoucherNumberEdit() {
        await allure.step(`Click Voucher Number Edit`, async () => {
            await this.page.getByRole('button', { name: '' }).nth(2).click({ timeout: 3000 });
        });
    }

    async verifyVoucherSeries(expectedSeries: string) {
        await allure.step(`Verify Voucher Series: ${expectedSeries}`, async () => {
            const select = this.page.locator('select#ctrl_selectedVoucherSeries >> option:checked');
            await select.waitFor({ state: 'attached' });
            let actualSeries: string | null = null;
            const maxRetries = 5;
            const delayMs = 3300;
            for (let i = 0; i < maxRetries; i++) {
                actualSeries = await select.textContent();
                if (actualSeries !== "?") {
                    break;
                }
                await this.page.waitForTimeout(delayMs);
                console.log(`Retry ${i + 1}/${maxRetries}: Waiting for series to be set. Current value: ${actualSeries}`);
            }
            expect(actualSeries, `Expected ${expectedSeries}, but found ${actualSeries}`).toBe(expectedSeries);
        });
    }

    async verifyVoucherDate(expectedDate: string) {
        await allure.step(`Verify Voucher Date: ${expectedDate}`, async () => {
            const date = await this.page.locator('#ctrl_selectedDate').inputValue();
            const actual = new Date(date);
            const expected = new Date(expectedDate);
            expect(actual.toString(), `Expected ${expected.toString()}, but found ${actual.toString()}`).toBe(expected.toString());
        });
    }

    async verifyVoucherText(expected: string) {
        await allure.step(`Verify Voucher Text: ${expected}`, async () => {
            const actualName = await this.page.locator('#ctrl_voucher_text').inputValue();
            const expectedNameParts = expected.split(",").map(part => part.trim());
            const actualNameParts = actualName.split(",").map(part => part.trim());
            expect(expectedNameParts[0], `Expected ${expectedNameParts[0]}, but found ${actualNameParts[0]}`).toBe(actualNameParts[0])
            expect(expectedNameParts[1], `Expected ${expectedNameParts[1]}, but found ${actualNameParts[1]}`).toBe(actualNameParts[1])
            const expectedNamePartsDate = new Date(expectedNameParts[1]);
            const actualNamePartsDate = new Date(actualNameParts[1]);
            expect(expectedNamePartsDate.toString(), `Expected ${expectedNamePartsDate.toString()}, but found ${actualNamePartsDate.toString()}`).toBe(actualNamePartsDate.toString());
        });
    }

    async verifyDebitAmountandAccount(expected: string, account: string) {
        await allure.step(`Verify Debit Amount: ${expected}`, async () => {
            const debitAmountRaw = await this.voucherGrid.getCellValueFromGrid('dim1Nr', account, 'debitAmount');
            const debitAmount = debitAmountRaw?.replace(/\s/g, '').trim();
            const expectedClean = expected.replace(/\s/g, '').trim();
            expect(debitAmount, 'Debit amount value should match').toBe(expectedClean);
        });
    }

    async verifyCreditAmountandAccount(expected: string, account: string) {
        await allure.step(`Verify Credit Amount: ${expected}`, async () => {
            const creditAmountRaw = await this.voucherGrid.getCellValueFromGrid('dim1Nr', account, 'creditAmount');
            const creditAmount = creditAmountRaw?.replace(/\s/g, '').trim() || '';
            expect(creditAmount, 'Credit amount value should match').toBe(expected.replace(/\s/g, '').trim());
        });
    }

    async verifyMessage(expected: string) {
        await allure.step(`Verify Message: ${expected}`, async () => {
            const messageLocator = this.page.locator("//div[contains(@class,'modal-content')]//span[contains(@class,'ng-binding')]");
            await expect(messageLocator).toHaveText(expected, { timeout: 5000 });
        });
    }

    async closeVoucher() {
        await allure.step(`Close Voucher`, async () => {
            await this.page.locator(`//i[@title='Close' and contains(@class, 'removableTabIcon')]`).click({ timeout: 5000 });
        });
    }

}