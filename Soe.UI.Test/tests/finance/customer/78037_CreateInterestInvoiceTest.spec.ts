import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { InvoiceUtil } from '../../../apis/utils/InvoiceUtil';
import { getDateUtil } from '../../../utils/CommonUtil';

let testCaseId: string = '78037';
let envUrl: string;
let invoiceNo_1: string;
let invoiceUtil: InvoiceUtil;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, financeBasePage }) => {
    await allure.parentSuite("Finance");
    await allure.suite("Customer");
    await allure.subSuite("Customer Payments");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    invoiceNo_1 = testCaseId + Math.floor(100000 + Math.random() * 900000);
    invoiceUtil = new InvoiceUtil(page, envUrl);
    await invoiceUtil.CreateCustomerInvoice(invoiceNo_1);
});

test(testCaseId + ': Create Customer Payment Create Interest Invoice : AP', { tag: ['@Finance', '@Customer', '@Regression'] }, async ({ financeBasePage, accountsReceivableSettingsPage, financeInvoicePageJS, paymentsPage }) => {
    await financeBasePage.goToMenu('Settings_Finance', 'Settings', true, 'Accounts Receivable');
    await accountsReceivableSettingsPage.unselectTransferInvoiceToVoucher();
    await accountsReceivableSettingsPage.saveAccountReceivableSettings();
    await accountsReceivableSettingsPage.moveToRequirementsandBillingTab();
    await accountsReceivableSettingsPage.setInterestOnLatePayments('10.00');
    await accountsReceivableSettingsPage.saveAccountReceivableSettings();
    await financeBasePage.goToMenu('Customer', 'Invoices');
    await financeInvoicePageJS.filterByInvoiceNo(invoiceNo_1);
    await financeInvoicePageJS.editInvoice();
    await financeInvoicePageJS.expandCustomerInvoiceTab();
    await financeInvoicePageJS.changeDueDate(await getDateUtil(30));
    await financeInvoicePageJS.saveInvoice();
    await financeBasePage.goToMenu('Customer', 'Payments');
    await paymentsPage.filterAllPayments();
    await paymentsPage.filterByInvoiceNumber(invoiceNo_1);
    await paymentsPage.registerNewPayment();
    await paymentsPage.saveAndCloseRegisterPayment(4, 0);
    await paymentsPage.moveToInterestTab();
    await paymentsPage.filterInterestByInvoiceNNumber(invoiceNo_1);
    await paymentsPage.selectInterestByRow(0);
    await paymentsPage.clickSelect('Create interest invoice', 2, 0);
    await financeBasePage.goToMenu('Customer', 'Invoices');
    await financeInvoicePageJS.showClosed();
    await financeInvoicePageJS.addColumnToGrid("Internal Text");
    await financeInvoicePageJS.filterByInternalText(invoiceNo_1);
    await financeInvoicePageJS.editInvoice();
    await financeInvoicePageJS.expandCustomerInvoiceTab();
    await paymentsPage.verifyPaymentAmountGreaterThan();
});


