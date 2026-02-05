import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';

let testCaseId: string = '78606';
let envUrl: string;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Order");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await new ManageUtils(page, envUrl).verifyOrderCheckLists();
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Order Invoice Merge : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS, invoicePageJS }) => {
    let internalText = `Manual_Test_Order_Copy_${Math.random().toString(36).substring(2, 7)}`
    // Create first order
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow("999", '2300', '10');
    await orderPageJS.saveOrder();
    await orderPageJS.addToKlar()
    await orderPageJS.closeOrder()
    await orderPageJS.filterByInternalText(internalText)
    await orderPageJS.verifyOrder(1)
    // Create second order
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow("999", '250000', '1');
    await orderPageJS.saveOrder();
    await orderPageJS.addToKlar()
    await orderPageJS.closeOrder()

    // Orders Merge
    await orderPageJS.filterByInternalText(internalText)
    await orderPageJS.selectAllProductsOrderRowsGrid()
    await orderPageJS.tranferToPreliminaryMergedInvoice()
    await orderPageJS.showClosedOrders()
    await orderPageJS.verifyOrder(2)
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.filterByInternalText(internalText)
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.verifyInvoiceInfor("273 000,00", "totalAmountExVat")
});



