import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78689';
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

test(testCaseId + ': Edit Invoice : DS', { tag: ['@Sales', '@Invoice', '@Regression'] }, async ({ salesBasePage, invoicePageJS, orderPageJS }) => {
    // Create invoice
    let internalText = `${testCaseId}_${Math.random().toString(36).substring(2, 7)}`;
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForPageLoad()
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.setVatType('Subject to VAT');
    await invoicePageJS.expandProductRowTab();
    await orderPageJS.addNewProduct('999', '2300', '10', 0, true);
    await orderPageJS.addNewProduct('999', '2300', '20');
    await invoicePageJS.saveInvoice();
    await orderPageJS.closeOrder()
    // Edit invoice
    await orderPageJS.filterByInternalText(internalText);
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.editInvoice();
    await invoicePageJS.expandProductRowTab();
    await orderPageJS.addNewLineOfText('Text Row', 1);
    await orderPageJS.addSubTotal()
    await orderPageJS.addPageBreak()
    await invoicePageJS.expandDebit();
    await orderPageJS.addInternalText(`${internalText}_Edited`);
    await invoicePageJS.saveInvoice();
    await orderPageJS.closeOrder()
    //Verify edited invoice
    await orderPageJS.filterByInternalText(`${internalText}_Edited`);
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.editInvoice();
    await invoicePageJS.expandProductRowTab();
    const subtotalValue = await invoicePageJS.getSubTotalValue();
    const { invoiceNumber } = await invoicePageJS.makeFinalInvoice()
    expect(subtotalValue).toBe("46 000,00")
    expect(invoiceNumber).not.toBe(0);
});



