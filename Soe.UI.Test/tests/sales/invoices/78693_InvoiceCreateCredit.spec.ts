import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78693';
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

test(testCaseId + ': Invoice Create Credit : DS', { tag: ['@Sales', '@Invoice', '@Regression'] }, async ({ salesBasePage, invoicePageJS, orderPageJS }) => {
    let internalText = `${testCaseId}_${Math.random().toString(36).substring(2, 7)}`;
    //Create an invocice without a credit invoice
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForPageLoad()
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.addNewProduct('999', '2300', '10');
    await invoicePageJS.makeFinalInvoice()
    const initialPrice = await orderPageJS.getValueFromProductRows("sumAmountCurrency", 0)
    await invoicePageJS.creditInvoice("No")
    const creditPrice = await orderPageJS.getValueFromProductRows("sumAmountCurrency", 0)
    await orderPageJS.closeOrder()
    await invoicePageJS.handleWarningPopup("Yes")
    // Make the credit invoice
    await orderPageJS.filterByInternalText(internalText);
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.editInvoice();
    await invoicePageJS.creditInvoice("Yes")
    await invoicePageJS.handleWarningPopup("No", "Do you want to recalculate the selling price and the purchase price?")
    await invoicePageJS.saveInvoice()
    await orderPageJS.closeOrder(1)
    await invoicePageJS.switchToTab(1);
    const finalCreditPrice = await orderPageJS.getValueFromProductRows("sumAmountCurrency", 0)
    await invoicePageJS.saveInvoice()
    await orderPageJS.closeOrder()
    await orderPageJS.filterByInternalText(internalText);
    await invoicePageJS.verifyInvoice(3)
    expect(initialPrice).toBe("23 000,00");
    expect(creditPrice).toBe("-23 000,00");
    expect(finalCreditPrice).toBe("-23 000,00");
});



