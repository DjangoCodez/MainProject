import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';

let testCaseId: string = '78552';
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

test(testCaseId + ': Order Rev Tax : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS }) => {
    let internalText = Math.random().toString(36).substring(2, 7);
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Construction service');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow("999", '2300', '10');
    await orderPageJS.saveOrder();
    await orderPageJS.expandCodingRows()
    await orderPageJS.verifyCodingRowCount(2)
    await orderPageJS.verifyAccount("1510", "dim1Nr", 0)
    await orderPageJS.verifyAccount("3231", "dim1Nr", 1)
    await orderPageJS.verifyAccountBalance("23 000,00","debitAmount",0)
    await orderPageJS.verifyAccountBalance("23 000,00","creditAmount",1)
});



