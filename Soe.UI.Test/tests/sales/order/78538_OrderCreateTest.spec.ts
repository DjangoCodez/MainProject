import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ProductUtils } from '../../../apis/utils/ProductUtil';
import { ManageUtils } from '../../../apis/utils/ManageUtils';

let testCaseId: string = '78538';
let envUrl: string;
let productName: string = 'Order product' + testCaseId;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Order");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    let productUtils = new ProductUtils(page, envUrl);
    await productUtils.createProduct(testCaseId, productName);
    await new ManageUtils(page, envUrl).verifyOrderCheckLists();
});

test(testCaseId + ': Order Create : MG', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS }) => {
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    let internalText = Math.random().toString(36).substring(2, 7);
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow(productName, '2300', '10');
    await orderPageJS.addNewLineOfText('This is auto test order ' + testCaseId, 1);
    await orderPageJS.addSubTotal();
    await orderPageJS.verifyProductSubTotal('23 000,00', 2);
    await orderPageJS.addPageBreak();
    await orderPageJS.saveOrder();
    await orderPageJS.expandTimes();
    await orderPageJS.addWork();
    await orderPageJS.verifyTimeRow("2", 4);
    await orderPageJS.verifyTimeRowCount(1);
    await orderPageJS.verifyProductPageBreak(3);
    await orderPageJS.verifyTextRow('This is auto test order ' + testCaseId, 1);
    await orderPageJS.verifyVatType('Subject to VAT');
    await orderPageJS.verifyInternalText(internalText);
});



