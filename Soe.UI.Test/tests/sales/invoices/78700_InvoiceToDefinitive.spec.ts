import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78700';
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

test(testCaseId + ': Invoice to definitive : DS', { tag: ['@Sales', '@Invoice', '@Regression'] }, async ({ salesBasePage, invoicePageJS, orderPageJS }) => {
    let internalText = `${testCaseId}_${Math.random().toString(36).substring(2, 7)}}`;
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForPageLoad()
    // First invoice
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.addNewProduct('999', '2300', '10');
    await invoicePageJS.saveInvoice()
    await orderPageJS.closeOrder()
    await invoicePageJS.filterByInternalText(internalText);
    await invoicePageJS.verifyInvoice(1)
    // Second invoice
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.addNewProduct('999', '2300', '10');
    await invoicePageJS.saveInvoice()
    await orderPageJS.closeOrder()
    await invoicePageJS.switchToTab()
    await orderPageJS.filterByInternalText(internalText);
    await invoicePageJS.verifyInvoice(2)
    await invoicePageJS.selectAllInvoicesOrderRowsGrid();
    await invoicePageJS.transferInvoice();
    await invoicePageJS.editInvoice(2, 0);
    await invoicePageJS.switchToTab(1)
    await invoicePageJS.expandDebit()
    await invoicePageJS.verifyInvoiceStatus("Documentation")
    await orderPageJS.closeOrder()
    await orderPageJS.filterByInternalText(internalText);
    await invoicePageJS.editInvoice(2, 1);
    await invoicePageJS.switchToTab(1)
    await invoicePageJS.expandDebit()
    await invoicePageJS.verifyInvoiceStatus("Documentation")
});



