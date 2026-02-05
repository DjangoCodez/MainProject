import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { SupplierInvoiceUtil } from 'apis/utils/SupplierInvoiceUtil';

let testCaseId: string = '127352';
let envUrl: string;
const supplierName: string = getEnvironmentValue('default_supplier') ?? '';
let invoiceNo_1: string;
let invoiceUtil: SupplierInvoiceUtil;

test.beforeEach(async ({ page, accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Supplier");
  await allure.subSuite("Supplier");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  invoiceNo_1 = testCaseId + Math.floor(100000 + Math.random() * 900000);
  invoiceUtil = new SupplierInvoiceUtil(page, envUrl);
  await invoiceUtil.CreateSupplierInvoice(invoiceNo_1);
});

test(testCaseId + ': Supplier Overview : AP', { tag: ['@Finance', '@Supplier', '@Regression'] }, async ({ page, financeBasePage, overviewPage }) => {
  await financeBasePage.goToMenu('Supplier', 'Overview');
  await overviewPage.filterBySupplierName(supplierName);
  await overviewPage.selectSupplier();
  await overviewPage.clickAlertMessage('OK');
  await overviewPage.clikOnSupplierName();
  await page.waitForTimeout(3000);
  await overviewPage.verifyNewBrowserUrl('Supplier Invoice List');
  await overviewPage.closedOpenedBrowserTab();
  await overviewPage.searchSupplier();
  await overviewPage.verifySearchSupplierPopUpOpen();
});

