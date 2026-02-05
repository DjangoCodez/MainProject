import { Page, expect } from "@playwright/test";
import { SalesBasePage } from "../SalesBasePage";
import * as allure from "allure-js-commons";
import { extractWithPdfReader, getDateUtil, getFormattedDateMMDDYY } from '../../../utils/CommonUtil';
import { SingleGridPageJS } from "../../common/SingleGridPageJS";


export class InvoicePageJS extends SalesBasePage {

    readonly invoiceGrid: SingleGridPageJS;
    readonly fileGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.invoiceGrid = new SingleGridPageJS(page);
        this.fileGrid = new SingleGridPageJS(page, 0, 'directiveCtrl.gridAg.options.gridOptions');
    }

    async filterByInternalText(internalText: string, rowIndex: number = 0) {
        await allure.step("Filter by internal  text ", async () => {
            await this.invoiceGrid.filterByName('Internal Text', internalText, 15000, rowIndex);
        });
    }


    async closeInvoice(index: number = 0) {
        await allure.step("Close Invoice", async () => {
            const orderCloseButton = this.page.locator("//i[contains(@class, 'fa-times') and @title='Close']").nth(index)
            await orderCloseButton.click();
        });
    }

    async verifyInvoice(rowCount: number = 1) {
        await allure.step("Verify row count ", async () => {
            const rows = this.page.locator('//div[@ref="eContainer"]/div[@role="row"]')
            await expect.poll(async () => await rows.count(), { timeout: 10000 }).toBe(rowCount);
            const count = await this.invoiceGrid.getFilteredAgGridRowCount();
            expect(count, `Expected ${rowCount} row, but found ${count} rows.`).toBe(rowCount);
        });
    }

    async switchToTab(tabIndex: number = 0) {
        await allure.step(`Switch to ${tabIndex} tab`, async () => {
            const tab = this.page.locator(`//li[@index="tab.index"]`).nth(tabIndex);
            await tab.click();
        });
    }

    async Reload() {
        await allure.step("Reload", async () => {
            await this.page.locator("//a[contains(@class, 'fa-sync') and @title='Reload records']").click();
        });
    }

    async verifyInvoiceInfor(expectedValue: string, colId: string, rowId: number = 0) {
        await allure.step(`Verify ${colId}`, async () => {
            const columnValue = await this.invoiceGrid.getCellvalueByColIdandGrid(colId, rowId)
            expect(columnValue, `${expectedValue} is not equal to ${columnValue}`).toBe(expectedValue)
        })
    }

    async verifyVATexcludedTotalValue(totalVatExcludeValue: string) {
        await allure.step("Verify VAT excluded total value", async () => {
            const total = await this.page.locator("#directiveCtrl_sumAmountCurrency").inputValue()
            expect(total, `Expected total VAT exclude amount ${totalVatExcludeValue} is not equal to ${total}`).toBe(totalVatExcludeValue)
        })
    }

    async verifyInvoiceFee(feeAmount: string) {
        await allure.step("Verify Invoice Fee", async () => {
            const fee = await this.page.locator("#directiveCtrl_feeAmountCurrency").inputValue()
            expect(fee, `Expected invoice fee amount ${feeAmount} is not equal to ${fee}`).toBe(feeAmount)
        })
    }

    async verifyInvoiceNo() {
        await allure.step("Verify invoice no", async () => {
            const invoiceNo = await this.invoiceGrid.getCellvalueByColIdandGrid('invoiceNr')
            expect(invoiceNo).toBeTruthy();
        });
    }

    async waitForGridLoaded() {
        await allure.step("Reload", async () => {
            await this.page.waitForResponse(response =>
                response.url().includes('api/Core/CustomerInvoices/') && response.status() === 200
            );
        });
    }

    async verifyInvoiceExsit() {
        await allure.step("Verify row count ", async () => {
            const count = await this.invoiceGrid.getFilteredAgGridRowCount();
            expect(count, `Expected 1 row, but found ${count} rows.`).toBe(1);
        });
    }

    async verifyStatus() {
        await allure.step("Verify status documentation ", async () => {
            const status = await this.invoiceGrid.getCellvalueByColIdandGrid('statusName');
            expect(status, `Expected Documentation row, but found ${status} rows.`).toBe('Documentation');
        });
    }

    async getInvoiceTotalAmount() {
        return await allure.step("Verify total amount", async () => {
            return await this.page.locator('#directiveCtrl_totalAmountCurrency').inputValue();
        });
    }


    async verifyInvoiceDate(date: string) {
        await allure.step("Verify invoice date", async () => {
            const dateInput = this.page.getByRole('tabpanel', { name: /Customer Invoice/i }).locator('input#ctrl_selectedInvoiceDate[readonly]');
            await dateInput.waitFor({ state: 'visible' });
            const value = await dateInput.inputValue();
            const dates = [date, value];
            const dateArray: Date[] = [];
            for (const d of dates) {
                const [month, day, year] = d.split('/').map(Number);
                const fullYear = year < 100 ? 2000 + year : year;
                const formatDate = new Date(fullYear, month - 1, day);
                dateArray.push(formatDate);
            }
            expect(dateArray[0].getTime(), `Expected ${date}, but found ${dateInput}`).toBe(dateArray[1].getTime());
        });
    }

    async verifyInvoiceDateInCustomerInvoiceGrid() {
        await allure.step("Verify invoice date today ", async () => {
            const date = await this.invoiceGrid.getCellvalueByColIdandGrid('invoiceDate');
            const today = await getDateUtil(0);
            const formatToday = new Date(today);
            const formatDate = new Date(date);
            expect(formatDate.toString(), `Expected ${formatToday.toString()} row, but found ${formatDate.toString()} rows.`).toBe(formatToday.toString());
        });
    }

    async filterAllInvoices() {
        await allure.step("Filter all invoices", async () => {
            const dropDown = this.page.locator("#ctrl_allItemsSelection");
            await dropDown.waitFor({ state: 'visible' });
            const isSelected = await dropDown.locator('option[label="All"]').getAttribute('selected');
            if (!isSelected) {
                await dropDown.selectOption('All');
                await this.waitForDataLoad('/api/Core/CustomerInvoices/');
            }
        });
    }

    async filterByInvoiceNumber(invoiceNumber: string, rowIndex: number = 0) {
        await allure.step(`Filter by invoice number ${invoiceNumber} `, async () => {
            await this.invoiceGrid.filterByName('Invoice No.', invoiceNumber, 15000, rowIndex);
        });
    }

    async filterByCustomerName(customerName: string, rowIndex: number = 0) {
        await allure.step("Filter by customer name ", async () => {
            await this.invoiceGrid.filterByName('Customer Name', customerName, 15000, rowIndex);
        });
    }

    async filterByOrderNumber(orderNumber: string, rowIndex: number = 0) {
        await allure.step("Filter by order number", async () => {
            await this.invoiceGrid.filterByName('Order No.', orderNumber, 15000, rowIndex);
        });
    }

    async expandDocumentTab() {
        await allure.step("Expand document tab", async () => {
            const documentTab = this.page.locator("//label[@class='control-label' and text()='Document']");
            await documentTab.waitFor({ state: 'visible' });
            await documentTab.click();
        });
    }

    async expandProductRowTab() {
        await allure.step("Expand product row tab", async () => {
            const productRowTab = this.page.locator('[label-key="billing.order.productrows"]');
            await productRowTab.waitFor({ state: 'visible' });
            await productRowTab.click();
            await this.page.locator(`#article-rows-grid`).waitFor({ state: 'visible' });

        });
    }


    async expandTracking() {
        await allure.step("Expand tracking row tab", async () => {
            const trackingRowTab = this.page.locator('[label-key="common.tracing"]');
            await trackingRowTab.waitFor({ state: 'visible' });
            await trackingRowTab.click();
            await this.page.locator(`[label-key="common.tracing"] [name="center"]`).waitFor({ state: 'visible' });
        });
    }

    async verifyVoucher() {
        return await allure.step("Verify voucher", async () => {
            await this.page.locator(`//div[@col-id="originTypeName"]//span[text()="Voucher"]/../..`).dblclick();
            const tabs = this.page.locator(`//li[@index="tab.index"]`);
            await expect.poll(async () => await tabs.count(), { timeout: 5000 }).toBe(3);
            const activeTab = this.page.locator(`//li[contains(@class,"active") and @index="tab.index"]`);
            const actieTabHeading = await activeTab.locator(`label`).innerText();
            if (!actieTabHeading.includes('Voucher')) {
                await tabs.nth(1).click();
            }
            const accountRecord = this.page.locator('//div[@col-id="dim1Nr" and contains(normalize-space(.), "1510- Kundfordringar")]');
            await accountRecord.waitFor({ state: 'visible', timeout: 5000 });
            const debitAmount = await this.page.locator('//div[@col-id="dim1Nr" and contains(normalize-space(.), "1510- Kundfordringar")]/../div[@col-id="debitAmount"]').innerText();
            return {
                debitAmount
            }
        });

    }
    async verifyFileExist(fileName: string) {
        await allure.step("Verify file exist", async () => {
            const fileNameUI = await this.fileGrid.getCellvalueByColIdandGrid('fileName', 0);
            expect(fileNameUI, `Expected file name to be '${fileName}', but found '${fileNameUI}'`).toBe(fileName);
        });
    }

    async editInvoice(expectedInvocesNumber: number = 1, editInvoiceButtonIndex: number = 0) {
        await allure.step("Edit invoice", async () => {
            const editInvoiceButton = this.page.locator("//div[@col-id='icon']//button[contains(@class, 'iconEdit')]");
            await expect(editInvoiceButton).toHaveCount(expectedInvocesNumber);
            await editInvoiceButton.nth(editInvoiceButtonIndex).waitFor({ state: 'visible' });
            await editInvoiceButton.nth(editInvoiceButtonIndex).click();
        });
    }

    async expandCustomerInvoiceTab() {
        await allure.step("Expand customer invoice tab ", async () => {
            const expand = this.page.locator("//div[@class='soe-accordion-heading ng-scope']//label[text()='Customer Invoice']");
            await expand.waitFor({ state: 'visible' });
            await expand.click();
            await this.waitForDataLoad('/api/Core/Currency/Enterprise/');
        });
    }

    async verifyTotalAmount(expectedAmount: number) {
        await allure.step("Verify total amount", async () => {
            const amountValue = await this.invoiceGrid.getCellvalueByColIdandGrid('totalAmountExVat');
            const cleaned = amountValue.replace(/\s/g, '').replace(',', '.');
            const amountNumber = parseFloat(cleaned);
            console.log("UI amount :", amountValue);
            expect(amountNumber, `Expected total amount to be '${expectedAmount}', but found '${amountNumber}'`).toBe(expectedAmount);
        });
    }

    async printInvoice(testCaseId: string) {
        await allure.step("Print the invoice", async () => {
            const print = this.page.getByRole('button', { name: 'Print' });
            await this.downloadFile(`test-data/temp-download/${testCaseId}_invoice.pdf`, print);
        })
    }

    async InvoiceJournal(testCaseId: string) {
        await allure.step("Print the invoice", async () => {
            await this.page.getByTitle('Invoice Journal').click();
            const report = this.page.locator('//span[text()="Fakturajournal Kund"]/../following-sibling::td/span[@title="Open report"]').nth(0);
            await report.waitFor({ state: 'visible' });
            await this.downloadFile(`test-data/temp-download/${testCaseId}_invoice.pdf`, report);
        })
    }

    async verifyMultipleValueInPDF(reportPath: string, ...values: string[]) {
        await allure.step(`Verify invoice values`, async () => {
            const fulltext = await extractWithPdfReader(reportPath);
            await this.deleteFile(reportPath);
            for (let val of values) {
                expect(fulltext.includes(val), `expected value ${val} not found in PDF`).toBe(true);
            }
        });
    }

    async createPayment(paymentMethod: string, rowIndex: number = 0) {
        await allure.step(" Create a payment", async () => {
            await this.invoiceGrid.selectCheckBox(rowIndex)
            const paymentMethods = this.page.locator('#ctrl_selectedPaymentMethod')
            await paymentMethods.click()
            await paymentMethods.selectOption({ label: `${paymentMethod}` })
            await this.page.locator("#ctrl_selectedPayDate").fill(getFormattedDateMMDDYY())
            await this.page.locator('//button[@data-toggle="dropdown"]').click({ delay: 500 })
            await this.page.locator('//a[contains(text(),"Create payment")]').click()
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
        })
    }

    async createApplication(rowIndex: number = 0) {
        await allure.step("Create a payment", async () => {
            await this.invoiceGrid.selectCheckBox(rowIndex)
            await this.page.getByRole('button', { name: 'Create application' }).click();
            await this.waitForDataLoad("**/Billing/Invoice/HouseholdTaxDeduction/SaveApplied/")
            await this.clickAlertMessage('OK');
        })
    }

    async navigateToTab(tabName: string) {
        await allure.step(`Navigate to ${tabName} tab`, async () => {
            await this.page.getByRole('link', { name: tabName, exact: true }).click()
        })
    }

    async selectFilteredInvoiceItem(rowIndex: number = 0) {
        await allure.step(`Select filtered invoice item`, async () => {
            const activeTab = this.page.locator('//div[contains(@class,"active") and contains(@class,"tab-pane") and not(@id)]');
            const checkbox = activeTab.locator('div[ref="leftContainer"] input')
            await checkbox.nth(rowIndex).click()
            expect(checkbox).toBeChecked()
        });
    }

    async filterByInvoiceNumberInTabs(invoiceNumber: string) {
        await allure.step(`Filter by invoice number ${invoiceNumber} in tab`, async () => {
            const activeTab = this.page.locator('//div[contains(@class,"active") and contains(@class,"tab-pane") and not(@id)]');
            await activeTab.locator('input[aria-label="Invoice No. Filter Input"]').fill(invoiceNumber);
            const row = activeTab.locator('div[ref="centerContainer"] div[role="row"]');
            await expect(row).toHaveCount(1);
        });
    }

    async processApplication(step: string, rowIndex: number = 0) {
        await allure.step(`Tax appliation ${allure.step}`, async () => {
            await this.page.locator("#ctrl_bulkDate").fill(getFormattedDateMMDDYY())
            await this.selectFilteredInvoiceItem(rowIndex)
            await this.page.locator('//button[@data-toggle="dropdown"]').nth(1).click({ delay: 500 })
            await this.page.locator(`//a[contains(text(),'${step}')]`).click()
            await this.clickAlertMessage('No');
        })
    }

    async expandInvoice() {
        await allure.step("Expand Invoice", async () => {
            const projectTab = this.page.getByRole('button', { name: 'Invoice' });
            await projectTab.scrollIntoViewIfNeeded();
            await projectTab.click();
        });
    }

    async expandTerms() {
        await allure.step("Terms Invoice", async () => {
            const projectTab = this.page.getByRole('button', { name: 'Terms' });
            await projectTab.scrollIntoViewIfNeeded();
            await projectTab.click();
        });
    }

    async waitForCustomerDataReset(customerId: number) {
        await allure.step("Wait for customer data reset", async () => {
            const url = `**/Core/Customer/Customer/${customerId}/false/true/false/false/false/false`;
            await this.page.waitForResponse(url);
        });
    }

    async getCustomerInvoiceAddress(customerAddress: string) {
        return await allure.step("Get Customer Invoice Address", async () => {
            await this.page.waitForTimeout(2000); //wait for address to load
            await this.page.locator("#ctrl_invoice_billingAddressId").click();
            const invoiceAddress = await this.page.locator('#ctrl_invoice_billingAddressId > option[selected="selected"]').innerText()
            expect(invoiceAddress).toContain(customerAddress);
        });
    }

    async expandDebit() {
        await allure.step("Expand debit", async () => {
            const projectTab = this.page.getByRole('button', { name: 'Debit' });
            await projectTab.scrollIntoViewIfNeeded();
            await projectTab.click();
        });
    }

    async expandCredit() {
        await allure.step("Expand credit", async () => {
            const projectTab = this.page.getByRole('button', { name: 'Credit' });
            await projectTab.scrollIntoViewIfNeeded();
            await projectTab.click();
        });
    }

    async getInvoiceNumber() {
        return await allure.step("Get Invoice Number", async () => {
            const invoiceNumber = await this.page.locator('#ctrl_invoice_invoiceNr').inputValue();
            return invoiceNumber;
        });
    }

    async createItem() {
        await allure.step("Create Item", async () => {
            await this.page.waitForLoadState('networkidle');
            const newItemButton = this.page.locator(`//ul/li[@index!="tab.index"]`).nth(0)
            const tabs = this.page.locator(`//li[@index="tab.index"]`)
            const initTabCount = await tabs.count();
            await newItemButton.click({ force: true });
            const finalTabCount = await tabs.count();
            expect(finalTabCount, 'New tab was not created').toBe(initTabCount + 1);
        })
    }

    async waitForPageLoad() {
        await allure.step("Wait for page load", async () => {
            await this.waitForDataLoad("/api/Core/CustomerInvoices/")
        });
    }

    async saveInvoice() {
        return await allure.step("Save Invoice", async () => {
            await this.page.getByRole('button', { name: 'Save' }).click();
            const responsePromise = await this.page.waitForResponse("**/Billing/Invoice/")
            const response = await responsePromise.json();
            const { integerValue, value } = response;
            return { integerValue, invoiceNumber: value };
        });
    }

    async makeFinalInvoice() {
        return await allure.step("Make final invoice", async () => {
            await this.page.locator("#ctrl_definitive").click();
            return await this.saveInvoice();
        });
    }

    async getSubTotalValue() {
        return await allure.step("Get Sub Total Value", async () => {
            const subTotal = await this.page.locator('//div[@ref="eContainer"]//span[@class="pull-right"]').innerText();
            return subTotal;
        });
    }

    async creditInvoice(isCredit: "Yes" | "No") {
        await allure.step("Credit Invoice", async () => {
            await this.page.getByRole('button', { name: 'Credit' }).click();
            await expect(this.page.locator('.modal-body')).toBeVisible();
            await this.page.getByRole('button', { name: isCredit }).click();
            await this.page.locator('.modal-content').
                waitFor({ state: 'visible', timeout: 3000 })
                .catch(() => { console.log(`The ${process.env.ENV} UI is a bit vary`); });
        });
    }

    async handleWarningPopup(option: "Yes" | "No" | "OK", message: string = "") {
        await allure.step("Handle warning popup", async () => {
            const popup = await this.page.locator('.modal-content').
                waitFor({ state: 'visible', timeout: 3000 })
                .then(() => true)
                .catch(() => false);
            if (message && popup) {
                const alertMessage = await this.page.locator('.modal-body').innerText();
                expect(alertMessage).toContain(message);
            }
            if (popup) {
                await this.page.getByRole('button', { name: option }).click();
            }
        });
    }

    async addInvoiceFee(feeAmount: string) {
        await allure.step("Add Invoice Fee", async () => {
            await this.page.locator('#ctrl_invoice_invoiceFeeCurrency').fill(feeAmount)
            await this.page.keyboard.press('Enter');
        });
    }

    async verifyInvoiceFeeInTermsTab(feeAmount: string, invoiceIndex: number = 0) {
        await allure.step("Verify Invoice Fee in Terms Tab", async () => {
            const invoice = this.page.locator("#ctrl_invoice_invoiceFeeCurrency").nth(invoiceIndex)
            const fee = await invoice.inputValue()
            expect(fee, `Expected invoice fee amount ${feeAmount} is not equal to ${fee}`).toBe(feeAmount)
        })
    }

    async selectAllInvoicesOrderRowsGrid() {
        await allure.step("Select all invoices from order grid", async () => {
            await this.invoiceGrid.selectAllCheckBox()
        })
    }

    async transferInvoice(state: string = 'Save as definitive') {
        await allure.step("Tranfer to definitive invoice", async () => {
            await this.page.locator('//button[@data-toggle="dropdown"]').click({ delay: 500 })
            await this.page.locator(`//a[contains(text(),'${state}')]`).click()
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
        })
    }

    async verifyInvoiceStatus(status: string) {
        await allure.step(`Verify invoice status is ${status}`, async () => {
            const status = await this.page.locator('#ctrl_invoice_originStatusName').inputValue();
            expect(status, `Expected status to be 'Definitive', but found '${status}'`).toBe(status)
        });
    }

    async selectInvoiceRowFromTheGrid(rowIndex: number = 0) {
        await allure.step(`Select invoice row at index ${rowIndex}`, async () => {
            const rows = this.page.locator(`//div[@class="ag-pinned-left-cols-container"]/div/div[@col-id="soe-row-selection"]`);
            if (rowIndex === 0) {
                await expect.poll(async () => await rows.count(), { timeout: 10000 }).toBe(1);
            }
            await rows.nth(rowIndex).click();
        });
    }
}