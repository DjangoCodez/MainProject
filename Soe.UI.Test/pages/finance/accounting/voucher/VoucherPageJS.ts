import { expect, Page } from "@playwright/test";
import { BasePage } from "../../../common/BasePage";
import * as allure from "allure-js-commons";
import { getDateUtil } from "../../../../utils/CommonUtil";
import { SingleGridPageJS } from "../../../common/SingleGridPageJS";
import { SectionGridPageJS } from "../../../common/SectionGridPageJS";

export class VoucherPageJS extends BasePage {
    readonly page: Page;
    readonly codingRowsGrid: SectionGridPageJS;
    readonly accruralsGrid: SingleGridPageJS;
    readonly voucherGrid: SingleGridPageJS;
    readonly viewVoucherGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.page = page;
        this.codingRowsGrid = new SectionGridPageJS(page, 'common.accountingrows', 'directiveCtrl.soeGridOptions.gridOptions');
        this.accruralsGrid = new SingleGridPageJS(page);
        this.voucherGrid = new SingleGridPageJS(page);
        this.viewVoucherGrid = new SingleGridPageJS(page, 0, 'directiveCtrl.soeGridOptions.gridOptions');
    }

    async waitForPageLoad() {
        await allure.step(`Wait for Voucher Page Load`, async () => {
            await this.voucherGrid.waitForPageLoad();
            await this.waitForDataLoad('/api/Core/UserGridState/Economy_Accounting_Vouchers');
        });
    }

    async filterbyText(text: string) {
        await allure.step(`Filter Account By Text: ${text}`, async () => {
            const totalRecords = this.page.locator(`//input[@aria-label="Text Filter Input"]`);
            await totalRecords.waitFor({ state: 'visible' });
            await this.voucherGrid.filterByName('Text', text);
        });
    }

    async selectSeries(text: string) {
        await allure.step(`Select Series: ${text}`, async () => {
            await this.page.selectOption('#ctrl_selectedVoucherSeries', { label: text });
            await this.page.waitForTimeout(2000);
        });
    }

    async getVoucherNumber() {
        return await allure.step(`Get Voucher Number`, async () => {
            return await this.page.$eval('#ctrl_voucher_voucherNr', (el) => {
                return (el as HTMLInputElement).value;
            });
        });
    }

    async addText(text: string) {
        await allure.step(`Add Text: ${text}`, async () => {
            await this.page.locator('#ctrl_voucher_text').fill(text);
        });
    }

    async addTodayDate(today: string) {
        await allure.step(`Add Today's Date`, async () => {
            await this.page.locator('#ctrl_selectedDate').fill(today);
        });
    }

    async addCodingRow() {
        await allure.step(`Add Coding Row`, async () => {
            await this.page.getByTitle('Add Row').click();
        });
    }

    async addAccount(accountNumber: string, newAccount: boolean = false) {
        await allure.step(`Add Account: ${accountNumber}`, async () => {
            await this.page.locator('#typeahead-editor').fill(accountNumber);
            if (newAccount) {
                await this.page.keyboard.press('Enter');
            } else {
                await this.page.waitForSelector('.typeahead.dropdown-menu', { state: 'visible' });
                await this.page.locator('.typeahead.dropdown-menu li a', { hasText: accountNumber }).click();
                await this.page.waitForTimeout(100);
            }
        });
    }

    async addDebitAmount(amount: string, rowIndex: number = 0) {
        await allure.step(`Add Debit Amount: ${amount}`, async () => {
            await this.codingRowsGrid.enterGridValueByColumnId('debitAmount', amount, rowIndex);
            await this.page.keyboard.press('Enter');
        });
    }

    async addCreditAmount(amount: string, rowIndex: number = 0) {
        await allure.step(`Add Credit Amount: ${amount}`, async () => {
            await this.codingRowsGrid.enterGridValueByColumnId('creditAmount', amount, rowIndex);
        });
    }

    async verifyName(expectedName: string) {
        await allure.step(`Verify Name: ${expectedName}`, async () => {
            const actualName = await this.page.locator('#ctrl_templateName').inputValue();
            const expectedNameParts = expectedName.split(",").map(part => part.trim());
            const actualNameParts = actualName.split(",").map(part => part.trim());
            expect(expectedNameParts[0], `Expected ${expectedNameParts[0]}, but found ${actualNameParts[0]}`).toBe(actualNameParts[0])
            expect(expectedNameParts[1], `Expected ${expectedNameParts[1]}, but found ${actualNameParts[1]}`).toBe(actualNameParts[1])
            const expectedNamePartsDate = new Date(expectedNameParts[1]);
            const actualNamePartsDate = new Date(actualNameParts[1]);
            expect(expectedNamePartsDate.toString(), `Expected ${expectedNamePartsDate.toString()}, but found ${actualNamePartsDate.toString()}`).toBe(actualNamePartsDate.toString());
        });
    }

    async verifyVoucherSeries(expectedSeries: string) {
        await allure.step(`Verify Voucher Series: ${expectedSeries}`, async () => {
            const selectedOption = this.page.locator('#ctrl_distributionHead_voucherSeriesTypeId >> option[selected]');
            const actualSeries = await selectedOption.textContent();
            expect(actualSeries?.trim(), `Expected "${expectedSeries}", but found "${actualSeries}"`).toBe(expectedSeries);
        });
    }

    async verifyStartDate(expectedDate: string) {
        await allure.step(`Verify Date: ${expectedDate}`, async () => {
            const date = await this.page.locator('#ctrl_startDate').inputValue();
            const expectedDateObj = new Date(expectedDate);
            const actualDate = new Date(date);
            expect(expectedDateObj.toString(), `Expected ${expectedDateObj.toString()}, but found ${actualDate.toString()}`).toBe(actualDate.toString());
        });
    }

    async verifyEndDate() {
        await allure.step(`Verify End Date`, async () => {
            const endDate = await this.page.locator('#ctrl_endDate').inputValue();
            expect(endDate).toBe('')
        });
    }

    async verifyNumberOfTimes() {
        await allure.step(`Verify Number of Times: `, async () => {
            const numberOfTimes = await this.page.locator('#ctrl_nbrOfPeriods').inputValue();
            expect(numberOfTimes).toBe('')
        });
    }

    async verifyCalculationType() {
        await allure.step(`Verify Calculation Type`, async () => {
            const calculationType = await this.page.locator('#ctrl_calculationType').inputValue();
            expect(calculationType, 'Calculation type should not be empty').toBe('Percent');
        });
    }

    async verifyOppositeSign(expectedValue: string) {
        await allure.step(`Verify Opposite Sign: ${expectedValue}`, async () => {
            const oppositeSign = await this.codingRowsGrid.getCellValueFromGrid('rowNbr', '1', 'oppositeBalance');
            expect(oppositeSign, 'Opposite sign value should match').toBe(expectedValue);
        });
    }

    async addNumberOfTimes(times: number) {
        await allure.step(`Add Number of Times: ${times}`, async () => {
            await this.page.fill('#ctrl_nbrOfPeriods', times.toString());
        });
    }

    async verifyEndDateUpdated() {
        await allure.step(`Verify End Date Updated`, async () => {
            const endDate = await this.page.locator('#ctrl_endDate').inputValue();
            await expect(endDate).toMatch(/^(0?[1-9]|1[0-2])\/(0?[1-9]|[12][0-9]|3[01])\/\d{2,4}$/ );
        });
    }

    async addCalculationType(type: string) {
        await allure.step(`Add Calculation Type: ${type}`, async () => {
            await this.page.locator('#ctrl_calculationType').selectOption({ label: type });
        });
    }

    async addAccountInPopup(accountNumber: string) {
        await allure.step(`Add Account in Popup: ${accountNumber}`, async () => {
            await this.codingRowsGrid.enterDropDownValueGrid('dim1Nr', accountNumber, 1);
            await this.page.getByRole('button', { name: 'Coding Rows ' }).click();
        });
    }

    async save() {
        await allure.step(`Save Popup`, async () => {
            const ok = this.page.locator("//div[contains(@class,'modal-footer')]//button[@data-ng-click='ctrl.buttonOkClick()' and normalize-space()='OK' and not(contains(@class,'ng-hide'))]");
            await ok.waitFor({ state: 'visible' });
            await ok.scrollIntoViewIfNeeded();
            await expect(ok).toBeVisible();
            await expect(ok).toBeEnabled();
            await ok.click({ force: true });
        });
    }

    async saveVoucher() {
        await allure.step(`Save Voucher`, async () => {
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.waitForDataLoad('/api/Economy/Accounting/AccountBalance/GetAccountBalances');
            await this.page.waitForTimeout(1000);
        });
    }

    async closeVoucher() {
        await allure.step(`Close Voucher`, async () => {
            await this.page.locator('//i[@title="Close" and contains(@class, "removableTabIcon fal")]').click();
            await this.page.waitForTimeout(1000);
        });
    }

    async selectbyVoucherNumber(voucherNumber: string) {
        await allure.step(`Filter Account By Vowucher Number: ${voucherNumber}`, async () => {
            await this.accruralsGrid.filterByName('Name', voucherNumber);
        });
    }

    async selectbyVoucherByText(text: string) {
        await allure.step(`Filter Account By Voucher Text: ${text}`, async () => {
            await this.voucherGrid.filterByName('Text', text);
            await this.page.getByRole('checkbox', { name: 'Press Space to toggle row' }).click();
        });
    }

    async waitingForCodingRowsLoading() {
        await allure.step(`Waiting For Coding Rows Loading`, async () => {
            await this.page.getByTitle('Add Row').waitFor({ state: 'visible', timeout: 80000 });
        });
    }

    async verifyAccountNumberNotFound() {
        await allure.step(`Verify Account Number Not Found`, async () => {
            const alertText = 'The account number was not found. Add a new account?';
            const alertMessage = this.page.locator('label', { hasText: alertText });
            expect(alertMessage, 'Alert message should indicate account number not found').toHaveText(alertText);
        });
    }

    async addNameForNewAccount(name: string) {
        await allure.step(`Add Name For New Account: ${name}`, async () => {
            await this.page.locator('#ctrl_account_name').waitFor({ state: 'visible' });
            await this.page.fill('#ctrl_account_name', name);
            await this.page.keyboard.press('Enter');
        });
    }

    async addAccountType(type: string) {
        await allure.step(`Add Account Type: ${type}`, async () => {
            await this.page.selectOption('#ctrl_account_accountTypeSysTermId', { label: type });
        });
    }

    async saveNewAccount() {
        await allure.step(`Save New Account`, async () => {
            await this.page.getByRole('button', { name: 'ok' }).click();
            await this.waitForDataLoad('/api/Economy/Accounting/Account/Small/');
        });
    }

    async verifyAccountType(expectedType: string) {
        await allure.step(`Verify Account Type: ${expectedType}`, async () => {
            const accountType = await this.page.locator('#ctrl_accountType').inputValue();
            expect(accountType, `Expected account type to be ${expectedType}, but found ${accountType}`).toBe(expectedType);
        });
    }

    async editVoucher() {
        await allure.step(`Edit Voucher`, async () => {
            await this.page.locator('//button[@title="Edit" and contains(@class, "fa-pencil iconEdit")]').click();
            await this.waitForDataLoad('/api/Core/Currency/Enterprise/');
        });
    }

    async getBalance() {
        return await allure.step(`Get Account Balance`, async () => {
            let balance = await this.codingRowsGrid.getRowColumnValue('balance', 0);
            balance = Number(balance?.replace(/\s/g, '').replace(',', '.')).toFixed(2);
            return balance;
        });
    }

    async verifyDebitAmountandAccount(expected: string, account: string) {
        await allure.step(`Verify Debit Amount: ${expected}`, async () => {
            const accountUi = await this.viewVoucherGrid.getRowColumnValue('dim1Nr', 0);
            expect(accountUi, 'Account should match').toBe(account);
            const debitAmountRaw = await this.viewVoucherGrid.getRowColumnValue('debitAmount', 0);
            const debitAmount = debitAmountRaw?.replace(/\s/g, '').trim();
            const expectedClean = expected.replace(/\s/g, '').trim();
            expect(debitAmount, 'Debit amount value should match').toBe(expectedClean);
        });
    }

    async verifyCreditAmountandAccount(expected: string, account: string) {
        await allure.step(`Verify Credit Amount: ${expected}`, async () => {
            const accountUi = await this.viewVoucherGrid.getRowColumnValue('dim1Nr', 0);
            expect(accountUi, 'Account should match').toBe(account);
            const creditAmountRaw = await this.viewVoucherGrid.getRowColumnValue('creditAmount', 1);
            const creditAmount = creditAmountRaw?.replace(/\s/g, '').trim() || '';
            const expectedClean = expected.replace(/\s/g, '').trim();
            expect(creditAmount, 'Credit amount value should match').toBe(expectedClean);
        });
    }

    async verifyBalanceOfAccountingRows() {
        await allure.step(`Verify Balance of Accounting Rows`, async () => {
            const debitAmountSum = await this.page.locator(`//div[@class='ag-center-cols-clipper']//div[@col-id='debitAmount']//div[@class='pull-right']`).textContent();
            console.log('Debit Amount Sum: ' + debitAmountSum);
            const creditAmountSum = await this.page.locator(`//div[@class='ag-center-cols-clipper']//div[@col-id='creditAmount']//div[@class='pull-right']`).textContent();
            console.log('Credit Amount Sum: ' + creditAmountSum);
            expect(debitAmountSum, 'Credit and Debit Amounts not Same ').toBe(creditAmountSum);
        });
    }

    async verifyDate() {
        await allure.step(`Verify Date`, async () => {
            const date = await this.page.getByRole('textbox', { name: 'Date' }).inputValue();
            console.log('Date: ' + date);
            const today = await getDateUtil(0);
            const formatToday = new Date(today);
            const formatDate = new Date(date);
            expect(formatDate.toString(), `Expected ${formatDate.toString()} row, but found ${formatToday.toString()} rows.`).toBe(formatToday.toString());
        });
    }

    async getVoucherDate() {
        return await allure.step(`Get Voucher Date`, async () => {
            const date = await this.page.getByRole('textbox', { name: 'Date' }).inputValue();
            return date;
        });
    }

    async getInvoiceNumber() {
        return await allure.step(`Get Invoice Number`, async () => {
            const number = await this.page.locator('#ctrl_voucher_text').inputValue();
            const match = number?.match(/Cust\.inv\.\s+(\d+),/);
            const invoiceNumber = match?.[1] ?? '';
            return invoiceNumber;
        });
    }

    async selectReportHuvubok2dim() {
        await allure.step(`Select Report Huvubok 2 dim`, async () => {
            const report = this.page.locator(`//table[contains(@class,'report-menu-hless-table')]//tr[td[normalize-space()='Huvudbok 2dim']]`);
            await report.waitFor({ state: 'visible' });
            await report.click();
        });
    }
}