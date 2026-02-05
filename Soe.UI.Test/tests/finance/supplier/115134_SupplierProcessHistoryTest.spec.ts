import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { getFirstDateOfCurrentMonth, getLastDateOfCurrentMonth } from 'utils/CommonUtil';

let testCaseId: string = '115134';
let envUrl: string;
let supplierNumber: string = testCaseId + Math.floor(100 + Math.random() * 900);


test.beforeEach(async ({ accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Supplier");
  await allure.subSuite("Supplier");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Supplier Processing History : AP', { tag: ['@Finance', '@Supplier', '@Regression'] }, async ({ page, financeBasePage, processingHistoryPage, suppliersPage }) => {
  const supplierName = 'Supplier ' + supplierNumber;
  const supplierCompanyRegNumber = 'Reg-No-' + supplierNumber;
  await financeBasePage.goToMenu('Supplier', 'Processing History', true, 'Index');
  await processingHistoryPage.waitForProcessingHistoryPageLoad();
  await processingHistoryPage.verifyFromDate(await getFirstDateOfCurrentMonth());
  await processingHistoryPage.verifyToDate(await getLastDateOfCurrentMonth());
  await financeBasePage.goToMenu('Supplier', 'Suppliers', true, 'Index');
  await suppliersPage.createItem();
  await suppliersPage.waitForNewSupplierLoad();
  await suppliersPage.enterSupplierNumber(supplierNumber);
  await suppliersPage.enterSupplierName(supplierName);
  await suppliersPage.enterCompanyRegistrationNumber(supplierCompanyRegNumber);
  await suppliersPage.save();
  await suppliersPage.waitForSaveComplete();
  await page.waitForTimeout(1500);
  await financeBasePage.goToMenu('Supplier', 'Processing History', true, 'Index');
  await processingHistoryPage.waitForProcessingHistoryPageLoad();
  await processingHistoryPage.search();
  await processingHistoryPage.filterBySupplierName(supplierName);
  await processingHistoryPage.confirmFieldCreated(3);
  await processingHistoryPage.verifyType('New', 0);
  await processingHistoryPage.verifyType('New', 1);
  await processingHistoryPage.verifyType('New', 2);
  await processingHistoryPage.editProcessingHistory(Math.floor(Math.random() * 3));
  await processingHistoryPage.verifySupplierNumberNotNull();
});

