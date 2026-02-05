import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';

let testCaseId: string = '117907';
let envUrl: string;
let customerName = `AutoCus_${testCaseId}_` + Math.random().toString(36).substring(2, 7);
let customerNumber: string = `${Date.now().toString().slice(-5)}_${testCaseId}`;
let email: string = `auto${Date.now().toString().slice(-5)}${Math.floor(1000 + Math.random() * 9000)}@email.com`;
let homePhone: string = `0${Math.floor(270000000 + Math.random() * 8999999)}`;
let mobilePhone: string = `07${Math.floor(40000000 + Math.random() * 9999999)}`;
let firstname = 'Auto_' + testCaseId;
let lastname = Math.random().toString(36).substring(2, 7)

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Customer");
  await allure.subSuite("Customer");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Create new customer : SR', { tag: ['@Sales', '@Customer', '@Regression'] }, async ({ customersPage }) => {
  await customersPage.goToMenu('Customer_Sale', 'Customers');
  await customersPage.goToPageVersion(customersPage.ang_version);
  await customersPage.createItem();
  await customersPage.waitForPageLoad();
  await customersPage.verifySaveButtonDisabled();
  await customersPage.setNumber(customerNumber);
  await customersPage.addCustomerName(customerName);
  await customersPage.verifySaveButtonEnabled();
  await customersPage.addAllContacts(email, homePhone, mobilePhone);
  await customersPage.expandContacts();
  await customersPage.clickCreateContact();
  await customersPage.verifyContactPopupDisplayed();
  await customersPage.createContactDetails(firstname, lastname, email, mobilePhone);
  await customersPage.verifyContactDetailsInGrid();
  await customersPage.addNote();
  await customersPage.save();
  await customersPage.verifyDefaultStatusCheckbox();
  await customersPage.verifyTabChanged(customerName, customerNumber);
  await customersPage.closeTab();
  await customersPage.reloadPage();
  await customersPage.verifyFilterByNumber(customerNumber);
});
