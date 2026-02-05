import * as allure from 'allure-js-commons';
import { test } from 'tests/fixtures/sales-fixture';
import { getFormattedDateMMDDYY } from 'utils/CommonUtil';
import { getAccountExValue } from 'utils/properties';

let testCaseId: string = '125972';
let envUrl: string;
const supplierName: string = "ABC Smoke Test Supplier";
const invoiceNumber: string = Number(Math.floor(10000 + Math.random() * 900000)).toString();
let voucherNumber: string;
const rewportName: string = "Huvudbok 2dim";



test.use({ account: { domain: "mtest", user: 'admin' } });

test.beforeEach(async ({ page, accountEx, financeBasePage, salesBasePage }) => {
    await allure.parentSuite("Finance");
    await allure.suite("Supplier");
    await allure.subSuite("Supplier");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Create supplier invoice and payment : DS', { tag: ['@Finance', '@Supplier', '@Smoke'] }, async ({ supplierInvoicePageJS, financeBasePage, paymentsPage, accountPage }) => {
    await financeBasePage.goToMenu('Supplier', 'Invoices');
    await supplierInvoicePageJS.waitForNewSupplierInvoiceLoaded();
    await supplierInvoicePageJS.createItem();
    await supplierInvoicePageJS.selectSupplier(supplierName);
    await supplierInvoicePageJS.enterInvoiceNumber(invoiceNumber);
    await supplierInvoicePageJS.enterInvoiceDate(getFormattedDateMMDDYY());
    await supplierInvoicePageJS.enterTotalAmount("100")
    await supplierInvoicePageJS.saveSupplierInvoice(true);
    await supplierInvoicePageJS.selectSupplierinvoiceByInvoiceNo(invoiceNumber)
    await supplierInvoicePageJS.verifyInvoiceNumberInGrid(1);
    await supplierInvoicePageJS.selectInvoiceInGrid()
    await supplierInvoicePageJS.transferInvoiceTo("Transfer to voucher")
    await supplierInvoicePageJS.selectSupplierinvoiceByInvoiceNo(invoiceNumber)
    await supplierInvoicePageJS.verifyInvoiceNumberInGrid(1);
    await supplierInvoicePageJS.verifyVoucherStatus()
    await financeBasePage.goToMenu('Supplier', 'Payments');
    await paymentsPage.filterInvoiceByNumber(invoiceNumber);
    await paymentsPage.verifyFilteredRowCount(1);
    await paymentsPage.selectPaymentInGrid(0);
    await paymentsPage.selectFunction(getFormattedDateMMDDYY(), "Create payment suggestion");
    await paymentsPage.switchTo("Payment Proposal")
    await paymentsPage.filterInvoiceByNumber(invoiceNumber);
    await paymentsPage.verifyFilteredRowCount(1);
    await paymentsPage.selectPaymentInGrid(0);
    await paymentsPage.selectFunction(getFormattedDateMMDDYY(), "Create payment file");
    await paymentsPage.switchTo("Checking")
    await paymentsPage.filterInvoiceByNumber(invoiceNumber);
    await paymentsPage.verifyFilteredRowCount(1);
    await paymentsPage.selectPaymentInGrid(0);
    await paymentsPage.selectFunction(getFormattedDateMMDDYY(), "Save changes and transfer to voucher", true);
    await financeBasePage.goToMenu('Accounting', 'Voucher');
    await accountPage.waitForNetworkIdle();
    await accountPage.createItem();
    await accountPage.addCodingRow('100', '0', 0);
    await accountPage.addCodingRow('0', '100', 1);
    await accountPage.addText(invoiceNumber);
    voucherNumber = (await accountPage.saveVoucher()).voucherNumber;
    await accountPage.searchReport(rewportName);
    await financeBasePage.goToMenu('Accounting', 'Account Analysis', true, 'Analysis');
    await accountPage.setVoucherSearchPeriod("Today")
    await accountPage.enterVoucherText(invoiceNumber);
    await accountPage.searchVoucher()
    await accountPage.verifyVoucherInGrid(voucherNumber);
})
