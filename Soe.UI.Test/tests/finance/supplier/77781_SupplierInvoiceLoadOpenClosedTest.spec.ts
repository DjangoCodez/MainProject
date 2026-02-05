import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';

let testCaseId: string = '77781';
let envUrl: string;

test.beforeEach(async ({ accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Supplier");
  await allure.subSuite("Supplier");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Supplier Invoice Load Open Closed : AP', { tag: ['@Finance', '@Supplier', '@Regression'] }, async ({ financeBasePage, supplierInvoicePageJS }) => {
  await financeBasePage.goToMenu('Supplier', 'Invoices');
  await supplierInvoicePageJS.showOpenInvoices(false);
  await supplierInvoicePageJS.showClosedInvoices(false);
  await supplierInvoicePageJS.verifyInvoiceCount(0, true);
  const closedInvoices = await supplierInvoicePageJS.showClosedInvoices();
  await supplierInvoicePageJS.verifyInvoiceCount(Number(closedInvoices));
  await supplierInvoicePageJS.showClosedInvoices(false);
  const openInvoices = await supplierInvoicePageJS.showOpenInvoices();
  await supplierInvoicePageJS.verifyInvoiceCount(Number(openInvoices));
});

