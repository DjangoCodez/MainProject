import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ProductUtils } from '../../../apis/utils/ProductUtil';
import { ManageUtils } from '../../../apis/utils/ManageUtils';

let testCaseId: string = '78547';
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
    const productUtils = new ProductUtils(page, envUrl);
    await productUtils.createProduct(testCaseId, productName);
    await new ManageUtils(page, envUrl).verifyOrderCheckLists();
});

test(testCaseId + ': Order Edit : MG', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS }) => {
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    let internalText = Math.random().toString(36).substring(2, 7);
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.expandProducts();
    await orderPageJS.addNewProduct(productName, '2300', '10');
    await orderPageJS.saveOrder();
    await orderPageJS.closeOrder();
    await orderPageJS.filterByInternalText(internalText);
    await orderPageJS.editOrder();
    await orderPageJS.expandProjectOrder();
    let internalText_2 = Math.random().toString(36).substring(2, 7);
    await orderPageJS.addInternalText(internalText_2);
    let firstAccountProject: string = await orderPageJS.addFirstAccountProject();
    await orderPageJS.expandProducts();
    await orderPageJS.selectProductRow(0);
    await orderPageJS.clickOnFunctions();
    await orderPageJS.deleteSelectedRow();
    await orderPageJS.newProductRow(productName, '2300', '10');
    await orderPageJS.saveOrder();
    await orderPageJS.expandTimes();
    await orderPageJS.addWork();
    await orderPageJS.closeOrder();
    await orderPageJS.filterByInternalText(internalText_2);
    await orderPageJS.editOrder();
    await orderPageJS.expandProjectOrder();
    await orderPageJS.verifyInternalText(internalText_2);
    await orderPageJS.verifyAccountProject(firstAccountProject.split(' ')[1].trim());
    await orderPageJS.expandProducts();
    await orderPageJS.verifyProductSumAmount('23020,00');
    await orderPageJS.verifyProductRowCount(2);
});



