import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { OrderUtil } from '../../../apis/utils/OrderUtil';

let testCaseId: string = '115125';
let envUrl: string;
let orderUtils: OrderUtil;
let orderNumber: string;

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Order");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    orderUtils = new OrderUtil(page, envUrl);
    //price list need to be set for two products with the same prices as in Order.json
    orderNumber = await orderUtils.CreateOrder();
});

test(testCaseId + ': Order Re Calculate Price : AP ', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS }) => {
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.filterAllOrders();
    await orderPageJS.filterByOrderNo(orderNumber.toString());
    await orderPageJS.editOrder();
    await orderPageJS.expandProducts();
    await orderPageJS.addColumnToProductRows('Purchase Price');
    await orderPageJS.verifyPurchasePriceAndSalesPriceDifference();
    await orderPageJS.clickOnFunctions();
    await orderPageJS.verifyRecalculatePriceDisabled();
    await orderPageJS.selectAllProducts();
    let price_1 = await orderPageJS.getValueFromProductRows('amountCurrency', 0);
    let price_2 = await orderPageJS.getValueFromProductRows('amountCurrency', 1);
    await orderPageJS.changeSalesPrice(price_1, 1000, 0);
    await orderPageJS.changeSalesPrice(price_2, 1000, 1);
    await orderPageJS.saveOrder();
    await orderPageJS.closeOrder();
    await orderPageJS.reloadOrders();
    await orderPageJS.filterAllOrders();
    await orderPageJS.filterByOrderNo(orderNumber.toString());
    await orderPageJS.editOrder();
    await orderPageJS.expandProducts();
    await orderPageJS.verifySalesPrice(price_1, 1000, 0);
    await orderPageJS.verifySalesPrice(price_2, 1000, 1);
    await orderPageJS.selectAllProducts();
    await orderPageJS.clickOnFunctions();
    await orderPageJS.selectRecalculatePrice();
    await orderPageJS.addNewLineOfText('Test', 2, false);
    await orderPageJS.saveOrder();
    await orderPageJS.selectTextLine(2);
    await orderPageJS.clickOnFunctions();
    await orderPageJS.verifyRecalculatePriceDisabled();
    await orderPageJS.closeOrder();
    await orderPageJS.reloadOrders();
    await orderPageJS.filterAllOrders();
    await orderPageJS.filterByOrderNo(orderNumber.toString());
    await orderPageJS.editOrder();
    await orderPageJS.expandProducts();
    await orderPageJS.verifySalesPrice(price_1, 0, 0);
    await orderPageJS.verifySalesPrice(price_2, 0, 1);
});


