import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { SupplierInvoiceUtil } from '../../../apis/utils/SupplierInvoiceUtil';

let testCaseId: string = '77986';
let envUrl: string;
let invoiceNumber: string;
let SupplierInvoiceUtils: SupplierInvoiceUtil;
const attesttationGroup = 'Test Attest Group';
const Approvers = 'Admin Admin, Playwright Test';

test.beforeEach(async ({ page, accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Supplier");
  await allure.subSuite("Attest");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  invoiceNumber = testCaseId + Math.floor(100000 + Math.random() * 900000) + '_1';
  SupplierInvoiceUtils = new SupplierInvoiceUtil(page, envUrl);
  await SupplierInvoiceUtils.CreateSupplierInvoice(invoiceNumber);
});

test(testCaseId + ': Supplier Attest Flow From Invoice : AP', { tag: ['@Finance', '@Supplier', '@Regression'] }, async ({ page, financeBasePage, supplierInvoicePageJS }) => {
  await financeBasePage.goToMenu('Supplier', 'Invoices');
  await supplierInvoicePageJS.waitForSupplierInvoiceGridLoaded();
  await supplierInvoicePageJS.selectSupplierinvoiceByInvoiceNo(invoiceNumber);
  await supplierInvoicePageJS.editSupplierinvoice();
  await supplierInvoicePageJS.openToAttestation();
  await supplierInvoicePageJS.waitForModal();
  await supplierInvoicePageJS.selectAttestationGroup(attesttationGroup);
  await page.waitForTimeout(500);
  await supplierInvoicePageJS.clickOk();
  await supplierInvoicePageJS.closeTab();
  await supplierInvoicePageJS.reloadPage();
  await supplierInvoicePageJS.verifyAttest('Ankommen');
  await supplierInvoicePageJS.verifyApprovers(Approvers);
  await supplierInvoicePageJS.verifyAttestationGroup(attesttationGroup);
});


