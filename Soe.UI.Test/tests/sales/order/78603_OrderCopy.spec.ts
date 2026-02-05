import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';

let testCaseId: string = '78603';
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

test(testCaseId + ': Order copy : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS }) => {
    let internalText = `Manual_Test_Order_Copy_${Math.random().toString(36).substring(2, 7)}`
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow("999", '1', '100');
    const createdOrderNumber = await (await orderPageJS.saveOrder()).orderId;
    await orderPageJS.copyOrder()
    await orderPageJS.handlePopUp('Yes');
    const copiedOrderNumber = await (await orderPageJS.saveOrder()).orderId;
    await orderPageJS.verifyInternalText(internalText)
    await orderPageJS.verifyProductRowCount()
    await orderPageJS.closeOrder();
    await orderPageJS.filterByOrderNo(copiedOrderNumber)
    await orderPageJS.verifyOrder()
    expect(Number.parseInt(copiedOrderNumber)).toBeGreaterThanOrEqual(Number.parseInt(createdOrderNumber) + 1);
});