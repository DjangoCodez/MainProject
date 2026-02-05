import { expect, Page } from "@playwright/test";
import * as allure from "allure-js-commons";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";
import fs from 'fs';
import path from 'path';
import { FinanceBasePage } from "../FinanceBasePage";

export class PaymentsPage extends FinanceBasePage {
    readonly paymentsGrid: SingleGridPageJS;
    readonly demandGrid: SingleGridPageJS;
    readonly customerPayementsGrid: SingleGridPageJS;
    readonly checkingGrid: SingleGridPageJS;
    readonly interestGrid: SingleGridPageJS;


    constructor(page: Page) {
        super(page);
        this.paymentsGrid = new SingleGridPageJS(page, 0, 'ctrl.gridAg.options.gridOptions');
        this.demandGrid = new SingleGridPageJS(page, 1, 'ctrl.gridAg.options.gridOptions');
        this.customerPayementsGrid = new SingleGridPageJS(page);
        this.checkingGrid = new SingleGridPageJS(page, 3);
        this.interestGrid = new SingleGridPageJS(page, 2);
    }

    async filterByInvoiceNo(invoiceNumber: string) {
        await allure.step("Create Payment", async () => {
            await this.paymentsGrid.filterByName('Invoice No.', invoiceNumber);
            await this.page.waitForTimeout(2000);
        });
    }

    async waitForGridLoad() {
        await allure.step("Wait for Payments page to load", async () => {
            await this.waitForDataLoad('/api/Core/CustomerInvoices/');
        });
    }

    async editInvoice() {
        await allure.step("Edit Invoice", async () => {
            const button = this.page.locator('.gridCellIcon.fal.fa-pencil.iconEdit');
            await button.click();
            await this.waitForDataLoad('/api/Core/Currency/Enterprise/');
        });
    }

    async changeDueDate(dueDate: string) {
        await allure.step("Change Due Date", async () => {
            const dueDateInput = this.page.locator(`//input[@id='ctrl_invoice_dueDate' and @name="ctrl_invoice_dueDate"]`);
            await dueDateInput.waitFor({ state: 'visible' });
            await dueDateInput.fill('');
            await dueDateInput.fill(dueDate);
            await this.page.waitForTimeout(500);
            await dueDateInput.press('Enter');
            console.log("Due Date changed to: " + dueDate);
        });
    }

    async saveCustomerInvoice() {
        await allure.step("Save Customer Invoice", async () => {
            const saveButton = this.page.getByRole('button', { name: 'Save' });
            await saveButton.waitFor({ state: 'visible' });
            await saveButton.click();
            await this.waitForDataLoad('/api/Economy/Common/CustomerLedger/');
            await this.clickAlertMessage('OK');
            await this.page.waitForTimeout(500);
        });
    }

    async close() {
        await allure.step("Close Payments Page", async () => {
            const closeButton = this.page.locator(`//li[contains(@class,'uib-tab')][.//i[contains(@class,'removeAllTabIcon')]]//a`);
            await closeButton.waitFor({ state: 'visible' });
            await closeButton.click();
        });
    }

    async moveToDemandTab() {
        await allure.step("Move to Demand Tab", async () => {
            const demandTab = this.page.locator(`//li[contains(@class,'uib-tab')][.//label[normalize-space()='Demand']]//a`);
            await demandTab.waitFor({ state: 'visible' });
            await demandTab.click();
            await this.waitForDataLoad('/api/Core/CustomerInvoices/');
        });
    }

    async filterInvoiceNumberDemand(invoiceNumber: string) {
        await allure.step("Filter by Invoice Number in Demand Tab", async () => {
            await this.demandGrid.filterByName('Invoice No.', invoiceNumber);
            await this.page.waitForTimeout(1000);
        });
    }

    async getRequirementLevel(invoiceNumber: string) {
        return await allure.step("Get Requirement Level", async () => {
            const reqLevel = this.demandGrid.getCellValueFromGrid('invoiceNr', invoiceNumber, 'noOfRemindersText');
            return reqLevel;
        });
    }

    async selectInvoice(rowIndex: number = 0) {
        await allure.step("Select Invoice", async () => {
            await this.demandGrid.selectCheckBox(rowIndex);
        });
    }

    async printDemandLetter() {
        await allure.step("Print Demand Letter", async () => {
            const caretButton = this.page.locator(`//div[@form='ctrl.edit']//button[@data-toggle='dropdown' and contains(@class,'dropdown-toggle')]`);
            await caretButton.nth(1).waitFor({ state: 'visible' });
            await caretButton.nth(1).click();
            const printDemandLetterButton = this.page.locator(`//a[contains(normalize-space(.), 'Print demand letter')]`);
            await printDemandLetterButton.waitFor({ state: 'visible' });
            await this.downloadFile('test-data/temp-download/demand-letter.pdf', printDemandLetterButton, 'OK');
        });
    }

    async filterAllPayments() {
        await allure.step("Filter all payments", async () => {
            const select = this.page.locator('#ctrl_allItemsSelection');
            await select.waitFor({ state: 'visible' });
            await select.selectOption({ label: 'All' });
        });
    }

    async filterByInvoiceNumber(invoiceNumber: string) {
        await allure.step(`Filter by invoice number: ${invoiceNumber}`, async () => {
            await this.customerPayementsGrid.filterByName('Invoice No.', invoiceNumber);
        });
    }

    async selectInvoiceByNumber() {
        await allure.step(`Select invoice `, async () => {
            await this.page.getByRole('checkbox', { name: 'Press Space to toggle row' }).check();
        });
    }

    async editPayment() {
        await allure.step(`Edit payment`, async () => {
            const edit = this.page.locator("//button[@class='gridCellIcon fal fa-plus iconEdit']");
            await edit.waitFor({ state: 'visible' });
            await edit.click();
            await this.waitForDataLoad('/api/Economy/Common/PaymentInfor');
        });
    }

    async closePayement() {
        await allure.step(`Close payment`, async () => {
            const close = this.page.locator("//i[@title='Close']");
            await close.waitFor({ state: 'visible' });
            await close.click();
            await this.clickAlertMessage('Yes');
        });
    }

    async openChecking(isWaitForLoad: boolean = false) {
        await allure.step(`Open checking`, async () => {
            const checking = this.page.locator(`//li//label[normalize-space()="Checking"]`);
            await checking.waitFor({ state: 'visible' });
            await checking.click();
            if (isWaitForLoad) {
                await this.page.waitForTimeout(2000);
            }
        });
    }

    async filterByInvoiceNumberChecking(invoiceNumber: string, rowIndex: number = 0) {
        await allure.step("Filter by invoice number ", async () => {
            await this.checkingGrid.filterByName('Invoice No.', invoiceNumber, 15000, rowIndex);
        });
    }

    async filterByInvoiceNumberInterest(invoiceNumber: string, rowIndex: number = 0) {
        await allure.step("Filter by invoice number ", async () => {
            await this.interestGrid.filterByName('Invoice No.', invoiceNumber, 15000, rowIndex);
        });
    }

    async edit(index: number = 0) {
        await allure.step("Edit payment", async () => {
            const editButton = this.page.locator("//div[@ag-grid='ctrl.gridAg.options.gridOptions']//button[contains(@class, 'gridCellIcon fal fa-pencil iconEdit')]");
            await editButton.nth(index).waitFor({ state: 'visible' });
            await editButton.nth(index).click();
        });
    }

    async customerPaymentLoading() {
        await allure.step("Customer payment loading", async () => {
            await this.page.waitForTimeout(2000);
        });
    }

    async enterPayementDate(paymentDueDate: string, index: number = 2) {
        await allure.step(`Enter payement date: ${paymentDueDate}`, async () => {
            const paymentDueDateInput = this.page.locator("//input[@id='ctrl_selectedPayDate']");
            await paymentDueDateInput.nth(index).waitFor({ state: 'visible' });
            await paymentDueDateInput.nth(index).click();
            await paymentDueDateInput.nth(index).fill(paymentDueDate);
            await this.page.keyboard.press('Tab');
        });
    }

    async selectPaymentInGrid(invoiceNumber: number = 0): Promise<void> {
        await allure.step(`Select Invoice in Grid: ${invoiceNumber}`, async () => {
            const invoiceRow = this.page.locator(`//div[@name="left"]//div[@col-id="soe-row-selection"]`).nth(invoiceNumber);
            await invoiceRow.click();
        });
    }

    private async getTabIndex() {
        const currentTab = await this.page.locator('//li[@index="tab.index" and contains(@class,"active")]/a').innerText();
        const tabToGridIndex: Record<string, number> = {
            'Unpaid': 0,
            'Payment Proposal': 1,
            'Checking': 2,
            'Checked Off': 3
        };
        const currentGridIndex = tabToGridIndex[currentTab.trim()] ?? 0;
        return currentGridIndex;
    }

    async filterInvoiceByNumber(invoiceNumber: string) {
        await allure.step(`Filter by invoice number: ${invoiceNumber}`, async () => {
            const currentGridIndex = await this.getTabIndex();
            await this.page.locator('//input[@aria-label="Invoice No. Filter Input"]').nth(currentGridIndex).fill(invoiceNumber);
            await this.page.waitForTimeout(2000);
        });
    }

    async selectFunction(paymentDate: string, paymentSuggestion: string, isUncheckPayment: boolean = false) {
        await allure.step("Create Payment Suggestion", async () => {
            const currentGridIndex = await this.getTabIndex();
            await this.page.locator('#ctrl_selectedPayDate').nth(currentGridIndex).fill(paymentDate);
            const functionButton = this.page.locator('//div[@main-button="true"]').nth(currentGridIndex);
            await functionButton.locator('[data-toggle="dropdown"]').click();
            const options = functionButton.locator('ul>li>a').all();
            for (let option of await options) {
                const optionText = await option.innerText();
                if (optionText?.trim() === paymentSuggestion) {
                    await option.click();
                    break;
                }
            }
            await this.clickAlertMessage('OK');
            if (isUncheckPayment) {
                await this.clickAlertMessage('OK');
            }

        });
    }

    async saveAndCloseRegisterPayment(caretIndex: number = 0, saveButtonIndex: number = 0) {
        await allure.step("Save and close register payment", async () => {
            await this.page.evaluate(() => window.scrollBy(0, window.innerHeight));
            const caret = this.page.locator("//button[@data-toggle='dropdown']/span[@class='caret']");
            await caret.nth(caretIndex).waitFor({ state: 'visible' });
            await caret.nth(caretIndex).scrollIntoViewIfNeeded();
            await caret.nth(caretIndex).click();
            const saveButton = this.page.locator(`//a[contains(normalize-space(.), 'Save and close')]`);
            await saveButton.nth(saveButtonIndex).waitFor({ state: 'visible' });
            await saveButton.nth(saveButtonIndex).click();
            await this.waitForDataLoad('/api/Economy/Supplier/PaymentRow/');
            await this.clickAlertMessage('OK');
        });
    }

    async registerNewPayment() {
        await allure.step("Register new payment", async () => {
            const registerPaymentButton = this.page.locator("//button[contains(@class, 'gridCellIcon') and contains(@class, 'fal') and contains(@class, 'fa-plus') and contains(@class, 'iconEdit')]");
            await registerPaymentButton.waitFor({ state: 'visible' });
            await registerPaymentButton.click();
            await this.waitForDataLoad('/api/Economy/Common/PaymentInformation/');
        });
    }

    async selectPaymentMethod(paymentMethod: string) {
        await allure.step("Select payment method", async () => {
            const dropdown = this.page.locator('#ctrl_selectedPaymentMethod');
            await expect(dropdown.nth(1)).toBeEnabled();
            await dropdown.nth(1).selectOption({ label: paymentMethod }, { force: true });
        });
    }

    async saveNewPayment() {
        await allure.step("Save new payment", async () => {
            const saveButton = this.page.getByRole('button', { name: 'Save' });
            await saveButton.waitFor({ state: 'visible' });
            await saveButton.click();
            await this.waitForDataLoad('/api/Billing/Invoice/Payment/GetPaymentTraceViews/');
            await this.page.waitForTimeout(1000);
            await this.clickAlertMessage('OK');
        });
    }

    async saveandCloseNewPayment() {
        await allure.step("Save new payment", async () => {
            const carret = this.page.locator(`//button[@type='button' and contains(@class, 'ngSoeMainButton') and contains(@class, 'dropdown-toggle')]`).nth(1);
            await carret.waitFor({ state: 'visible' });
            await carret.click();
            const saveLink = this.page.getByRole('link', { name: 'Save and close', exact: true });
            await saveLink.waitFor({ state: 'visible' });
            await saveLink.click();
            await this.waitForDataLoad('/api/Core/PaymentInformation/PaymentInformationForPaymentMethod/', 25000);
            await this.page.waitForTimeout(1000);
            await this.clickAlertMessage('OK');
        });
    }

    async reload(index: number = 0) {
        await allure.step("Reload Grid", async () => {
            const reloadButton = `//a[@title='Reload records']`;
            const reloadButtonLocator = this.page.locator(`xpath=${reloadButton}`).nth(index);
            await reloadButtonLocator.waitFor({ state: 'visible' });
            await reloadButtonLocator.click();
            await this.page.waitForTimeout(3000);
        });
    }

    async verifyIncreasedRequirementLevel(requirementLevel: string, updatedRequirementLevel: string) {
        await allure.step("Verify Increased Requirement Level", async () => {
            expect(requirementLevel, `expect ${requirementLevel} got ${requirementLevel}`).not.toBe(updatedRequirementLevel);
            const requirementLevelvalue = Number(requirementLevel);
            const updatedRequirementLevelValue = Number(updatedRequirementLevel);
            const level = Number(requirementLevelvalue + 1);
            expect(level, `expect ${level} got ${requirementLevelvalue}`).toBe(updatedRequirementLevelValue);
        });
    }

    async expandCustomerInvoiceSection() {
        await allure.step("Expand Customer Invoice Section", async () => {
            const sectionHeader = this.page.locator(`//label[normalize-space()="Customer Invoice"]`);
            await sectionHeader.waitFor({ state: 'visible' });
            sectionHeader.click();
        });
    }

    async expandCodingRows() {
        await allure.step("Expand Coding Rows", async () => {
            const sectionHeader = this.page.locator(`//label[normalize-space()="Coding Rows"]`);
            await sectionHeader.waitFor({ state: 'visible' });
            sectionHeader.scrollIntoViewIfNeeded();
            sectionHeader.click();
        });
    }

    async verifyPaymentDate(expectedDate: string) {
        await allure.step("Verify payment due date", async () => {
            const dateUI = this.page.locator("//input[@id='ctrl_selectedPayDate']");
            await dateUI.nth(2).waitFor({ state: 'visible' });
            const dateRecieved = await dateUI.nth(2).evaluate(el => (el as HTMLInputElement).value);
            const dateExpected = new Date(expectedDate);
            const formatDateExpected = new Date(dateExpected);
            const formatDateRecieved = new Date(dateRecieved);
            expect(formatDateExpected.toString(), `Expected ${formatDateExpected.toString()} row, but found ${formatDateRecieved.toString()} rows.`).toBe(formatDateRecieved.toString());
        });
    }

    async moveToInterestTab() {
        await allure.step("Move to interest tab", async () => {
            const interestTab = this.page.locator("//a[contains(@class, 'nav-link') and contains(normalize-space(.), 'Interest')]");
            await interestTab.waitFor({ state: 'visible' });
            await interestTab.click();
            await this.waitForDataLoad('/api/Core/CustomerInvoices/');
        });
    }

    async filterInterestByInvoiceNNumber(invoiceNumber: string) {
        await allure.step("Filter interest by invoice number", async () => {
            await this.interestGrid.filterByName('Invoice No.', invoiceNumber);
        });
    }

    async selectInterestByRow(rowIndex: number = 0) {
        await allure.step("Select interest by row", async () => {
            await this.interestGrid.selectCheckBox(rowIndex);
        });
    }

    async clickSelect(buttonText: string = 'Select', caretIndex: number = 0, saveButtonIndex: number = 0) {
        await allure.step("Click select button", async () => {
            const caret = this.page.locator("//button[@data-toggle='dropdown']/span[@class='caret']");
            await caret.nth(caretIndex).waitFor({ state: 'visible' });
            await caret.nth(caretIndex).click();
            const saveButton = this.page.locator(`//a[contains(normalize-space(.), '${buttonText}')]`);
            await saveButton.nth(saveButtonIndex).waitFor({ state: 'visible' });
            await saveButton.nth(saveButtonIndex).click();
            await this.clickAlertMessage('OK');
            await this.waitForDataLoad('/api/Core/CustomerInvoices/');
            await this.clickAlertMessage('OK');
        });
    }

    async verifyPaymentAmountGreaterThan() {
        await allure.step("Verify payment amount is greater than expected", async () => {
            const paymentAmount = this.page.locator("//input[@id='ctrl_invoice_totalAmountCurrency']")
            paymentAmount.waitFor({ state: 'visible' });
            const paymentAmountText = await paymentAmount.inputValue();
            console.log('Payment amount text: ' + paymentAmountText);
            expect(parseFloat(paymentAmountText)).toBeGreaterThan(0);
        });
    }

    async verifyDowloadPDF() {
        await allure.step("Verify dowload PDF", async () => {
            const filePath = path.resolve('test-data/temp-download/demand-letter.pdf');
            const buffer = fs.readFileSync(filePath);
            const header = buffer.toString('utf-8', 0, 5);
            expect(header, `PDF header mismatch`).toBe('%PDF-');
            await this.deleteFile(filePath);
        });
    }

    async getPaymentAmount() {
        return await allure.step("Get payment amount", async () => {
            const paymentAmount = this.page.locator("#ctrl_payment_amount");
            await paymentAmount.waitFor({ state: 'visible' });
            return await paymentAmount.inputValue();
        });
    }

    async enterPayAmount(amount: string) {
        await allure.step(`Enter pay amount: ${amount}`, async () => {
            const payAmount = this.page.locator("#ctrl_payment_amount");
            await payAmount.waitFor({ state: 'visible' });
            await payAmount.fill(amount);
            await this.page.waitForTimeout(1500);
            await this.page.keyboard.press('Tab');
            await this.page.waitForTimeout(1000);
            await expect(payAmount).toHaveValue(amount);
        });
    }

    async verifyAmountToPay(expectedAmount: string) {
        await allure.step(`Verify amount to pay: ${expectedAmount}`, async () => {
            const amountToPay = await this.paymentsGrid.getCellvalueByColId('payAmount', false);
            expect(amountToPay, `Expected ${expectedAmount} got ${amountToPay}`).toBe(expectedAmount);
        });
    }

    async verifyBalanceOfAccountingRows() {
        await allure.step(`Verify Balance of Accounting Rows`, async () => {
            const debitAmountSum = await this.page.locator(`//div[@class='ag-center-cols-clipper']//div[@col-id='debitAmount']//div[@class='pull-right']`).textContent();
            const creditAmountSum = await this.page.locator(`//div[@class='ag-center-cols-clipper']//div[@col-id='creditAmount']//div[@class='pull-right']`).textContent();
            expect(debitAmountSum, 'Credit and Debit Amounts not Same ').toBe(creditAmountSum);
        });
    }

    async verifyPaymentNotExists() {
        await allure.step(`Verify payment not exists: `, async () => {
            const noRows = await this.paymentsGrid.getAgGridRowCount();
            expect(noRows, `Payment row found`).toBe(0);
        });
    }

    async fullyPaid(checked: boolean = false) {
        await allure.step(`Fully paid the invoice`, async () => {
            const fullyPaidButton = this.page.locator("//input[@id='ctrl_payment_fullyPaid']");
            await fullyPaidButton.waitFor({ state: 'visible' });
            if (checked) {
                await fullyPaidButton.check();
            } else {
                await fullyPaidButton.uncheck();
            }
        });
    }

    async verifyFilteredRowCount(count: number) {
        await allure.step(`Verify row count: ${count}`, async () => {
            const currentTab = await this.page.locator('//li[@index="tab.index" and contains(@class,"active")]/a').innerText();
            const tabToGridIndex: Record<string, number> = {
                'Unpaid': 0,
                'Payment Proposal': 1,
                'Checking': 2,
                'Checked Off': 3
            };
            const currentGridIndex = tabToGridIndex[currentTab.trim()] ?? 0;
            const rows = this.page.locator(`//div[@ag-grid="ctrl.gridAg.options.gridOptions"]`).nth(currentGridIndex).locator(`//div[@ref="eContainer"]/div[@role="row"]`);
            const rowCount = await rows.count();
            expect(rowCount, `Expected ${count} got ${rowCount}`).toBe(count);
            await this.page.waitForTimeout(2000);
        });
    }

    async switchTo(tabName: "Payment Proposal" | "Unpaid" | "Checking" | "Checked Off") {
        await allure.step(`Switch to ${tabName} `, async () => {
            const tabs = this.page.locator(`//li[@index="tab.index"]/a`).all();
            for (let tab of await tabs) {
                const tabNameInner = await tab.innerText();
                if (tabName.trim() === tabNameInner.trim()) {
                    await tab.click();
                    break;
                }
            }
        });
    }
}