import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { InvoiceUtil } from '../../../apis/utils/InvoiceUtil';

let testCaseId: string = '78028';
let envUrl: string;
let invoiceNo_1: string;
let invoiceNo_2: string;
let invoiceUtil: InvoiceUtil;

test.beforeEach(async ({ page, accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Accounting");
  await allure.subSuite("Customer");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  invoiceNo_1 = testCaseId + Math.floor(100000 + Math.random() * 900000);
  invoiceNo_2 = testCaseId + Math.floor(100000 + Math.random() * 900000);
  invoiceUtil = new InvoiceUtil(page, envUrl);
  await invoiceUtil.CreateCustomerInvoice(invoiceNo_1);
  await invoiceUtil.CreateCustomerInvoice(invoiceNo_2);
});

test(testCaseId + ': Create Customer Invoices : AP', { tag: ['@Finance', '@Customer', '@Regression'] }, async ({ financeBasePage, financeInvoicePageJS, voucherPageJS, invoicePageJS }) => {
  await financeBasePage.goToMenu('Customer', 'Invoices');
  await financeInvoicePageJS.waitForPageLoad();
  await financeInvoicePageJS.filterByInvoiceNo(invoiceNo_1);
  await financeInvoicePageJS.selectInvoiceByNumber();
  await financeInvoicePageJS.filterByInvoiceNo(invoiceNo_2);
  await financeInvoicePageJS.selectInvoiceByNumber();
  await financeInvoicePageJS.transferToVoucher();
  const listInvoiceNumbers = [invoiceNo_1, invoiceNo_2];
  for (const invoiceNo of listInvoiceNumbers) {
    await financeBasePage.goToMenu('Accounting', 'Voucher');
    await voucherPageJS.filterbyText(invoiceNo);
    await voucherPageJS.editVoucher();
    await voucherPageJS.waitingForCodingRowsLoading();
    await voucherPageJS.verifyBalanceOfAccountingRows();
    const date = await voucherPageJS.getVoucherDate();
    const invoiceNumber = await voucherPageJS.getInvoiceNumber();
    await financeBasePage.goToMenu('Customer', 'Invoices');
    await invoicePageJS.waitForGridLoaded();
    await invoicePageJS.filterByInvoiceNumber(invoiceNumber.toString());
    await invoicePageJS.editInvoice();
    await invoicePageJS.expandCustomerInvoiceTab();
    await invoicePageJS.verifyInvoiceDate(date);
  }
});


