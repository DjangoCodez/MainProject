import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';

let testCaseId: string = '117913';
let envUrl: string;
let customerName = `AutoCus ${Date.now().toString().slice(-5)}${Math.floor(1000 + Math.random() * 9000)}`;
let customerNumber: string = `${Math.random().toString(36).substring(2, 7)}_${testCaseId}`;
let customerName_2 = `AutoCus ${Date.now().toString().slice(-5)}${Math.floor(1000 + Math.random() * 9000)}`;
let customerNumber_2: string = `${Math.random().toString(36).substring(2, 7)}_${testCaseId}`;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Customer");
    await allure.subSuite("Customer");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Create Customer with E-Invoice Validation : SR', { tag: ['@Sales', '@Customer', '@Regression'] }, async ({ customersPage, settingsPage }) => {
    await settingsPage.goToMenu('Settings_Sales', 'Settings', true, 'Sales');
    await settingsPage.waitForPageLoad();
    await settingsPage.setEInvoiceFormat('Finvoice 3.0');
    await settingsPage.saveSettings();
    await customersPage.goToMenu('Customer_Sale', 'Customers');
    await customersPage.createItem();
    await customersPage.waitForPageLoad();
    await customersPage.setNumber(customerNumber);
    await customersPage.addCustomerName(customerName);
    await customersPage.expandSettings();
    await customersPage.setInvoiceMethod('E-Invoice');
    await customersPage.save();
    await customersPage.verifyErrorMessageShown('An error occurred, the customer could not be saved: Invoice method is set to "E-invoice". There is a requirement for organization number, VAT number, Finvoice address and operator.');
    await customersPage.checkPrivatePersonCheckbox();
    await customersPage.save();
    await customersPage.closeTab();
    await customersPage.reloadPage();
    await customersPage.verifyFilterByNumber(customerNumber);
    await settingsPage.goToMenu('Settings_Sales', 'Settings', true, 'Sales');
    await settingsPage.waitForPageLoad();
    await settingsPage.setEInvoiceFormat('Intrum');
    await settingsPage.saveSettings();
    await customersPage.goToMenu('Customer_Sale', 'Customers');
    await customersPage.createItem();
    await customersPage.waitForPageLoad();
    await customersPage.setNumber(customerNumber_2);
    await customersPage.addCustomerName(customerName_2);
    await customersPage.expandSettings();
    await customersPage.setInvoiceMethod('E-Invoice');
    await customersPage.save();
    await customersPage.closeTab();
    await customersPage.reloadPage();
    await customersPage.verifyFilterByNumber(customerNumber_2);
    // set back to default
    await settingsPage.goToMenu('Settings_Sales', 'Settings', true, 'Sales');
    await settingsPage.waitForPageLoad();
    await settingsPage.setEInvoiceFormat('');
    await settingsPage.saveSettings();
});
