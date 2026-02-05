import {test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78695';
let envUrl: string;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Invoice");
    await allure.subSuite("Invoice");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Create Invoice : DS', { tag: ['@Sales', '@Invoice', '@Regression'] }, async ({ salesBasePage, invoicePageJS, orderPageJS }) => {
    let internalText = `${testCaseId}_${Math.random().toString(36).substring(2, 7)}}`;
    // Create an invoice
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForPageLoad()
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.setVatType('Subject to VAT');
    await invoicePageJS.expandTerms()
    await invoicePageJS.addInvoiceFee('30');
    await orderPageJS.expandProducts();
    await orderPageJS.addNewProduct('999', '2300', '10');
    await invoicePageJS.saveInvoice()
    await invoicePageJS.verifyInvoiceFee('30,00');
    await orderPageJS.verifyProductRowCount(1);
    // Credit the invoice
    await invoicePageJS.makeFinalInvoice()
    await invoicePageJS.creditInvoice("Yes")
    await invoicePageJS.handleWarningPopup("No")
    // Verify the new invoice
    await invoicePageJS.expandDebit()
    await invoicePageJS.saveInvoice()
    await invoicePageJS.expandTerms()
    await invoicePageJS.verifyInvoiceFeeInTermsTab('30,00', 1);
    await orderPageJS.closeOrder(1)
    await invoicePageJS.switchToTab(1);
    await invoicePageJS.verifyInvoiceFee("−30,00");
    await invoicePageJS.verifyInvoiceFeeInTermsTab("−30,00");
});



