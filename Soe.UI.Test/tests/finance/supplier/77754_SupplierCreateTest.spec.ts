import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '77754';
let envUrl: string;
let supplierNumber: string = testCaseId + Math.floor(100 + Math.random() * 900);
let paymentType: string = getEnvironmentValue('default_paymentType') ?? '';

test.beforeEach(async ({ accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Supplier");
  await allure.subSuite("Supplier");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Supplier Create : AP', { tag: ['@Finance', '@Supplier', '@Regression'] }, async ({ financeBasePage, suppliersPage }) => {
  await financeBasePage.goToMenu('Supplier', 'Suppliers', true, 'Index');
  await financeBasePage.createItem();
  await suppliersPage.waitForNewSupplierLoad();
  await suppliersPage.enterSupplierNumber(supplierNumber);
  await suppliersPage.enterSupplierName("Test Supplier_" + supplierNumber);
  await suppliersPage.expandSettings();
  await suppliersPage.expandPaymentDetails();
  await suppliersPage.addRowInPaymentDetails(0);
  await suppliersPage.addPaymentType(paymentType);
  await suppliersPage.addAccountOrIBAN(supplierNumber);
  await suppliersPage.save();
  await suppliersPage.verifySupplierCreatedSuccessfully();
  await suppliersPage.closeTab();
  await suppliersPage.filterBySupplierName("Test Supplier_" + supplierNumber);
  await suppliersPage.editSupplier();
  await suppliersPage.enterSupplierName("Updated Test Supplier_" + supplierNumber);
  await suppliersPage.save();
  await suppliersPage.verifySupplierCreatedSuccessfully();
  await suppliersPage.closeTab();
  await suppliersPage.filterBySupplierName("Updated Test Supplier_" + supplierNumber);
  await suppliersPage.editSupplier();
  await suppliersPage.deleteSupplier();
  await suppliersPage.clearAllFilters();
  await suppliersPage.reload();
  await suppliersPage.filterBySupplierName("Updated Test Supplier_" + supplierNumber);
  await suppliersPage.verifyDeletedSupplierSuccessfully();
});

