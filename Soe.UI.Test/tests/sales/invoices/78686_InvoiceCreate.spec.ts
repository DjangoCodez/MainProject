import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '79686';
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
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForPageLoad()
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow('999', '2300', '10');
    await orderPageJS.addNewLineOfText('Text Row', 1);
    await orderPageJS.addSubTotal()
    await orderPageJS.addPageBreak()
    const subtotalValue = await invoicePageJS.getSubTotalValue();
    const initialInvoiceNumber = (await invoicePageJS.saveInvoice()).invoiceNumber;
    await orderPageJS.closeOrder()
    await orderPageJS.filterByInternalText(internalText);
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.editInvoice();
    await invoicePageJS.makeFinalInvoice()
    await invoicePageJS.expandDebit();
    const invoiceNumber = await invoicePageJS.getInvoiceNumber();
    expect(initialInvoiceNumber).toBe(0)
    expect(subtotalValue).toBe("23 000,00")
    expect(invoiceNumber).not.toBe(initialInvoiceNumber);
});



