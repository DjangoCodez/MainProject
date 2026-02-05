import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { ProductUtils } from '../../../apis/utils/ProductUtil';

let testCaseId: string = '117930';
let envUrl: string;
let customerNumber: string = `${Date.now().toString().slice(-5)}${Math.floor(1000 + Math.random() * 9000)}`;
let customerName = 'AutoCus ' + Math.random().toString(36).substring(2, 7) + ' ' + testCaseId;
let productName: string = 'Auto Product' + testCaseId;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Customer");
  await allure.subSuite("Customer");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  let productUtils = new ProductUtils(page, envUrl);
  await productUtils.createProduct(testCaseId, productName);
});

test(testCaseId + ': Customer Integration Order : SR', { tag: ['@Sales', '@Customer', '@Regression'] }, async ({ salesBasePage, customersPage, orderPageJS }) => {
  allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/124602', "124602");
  // Create Customer
  await salesBasePage.goToMenu('Customer_Sale', 'Customers');
  await salesBasePage.goToPageVersion(customersPage.ang_version);
  await customersPage.createItem();
  await customersPage.waitForPageLoad();
  await customersPage.setNumber(customerNumber);
  await customersPage.addCustomerName(customerName);
  await customersPage.expandProductTab();
  await customersPage.addNewProductRow();
  const selectedProduct = await customersPage.addProductWithPrice(productName, '280');
  await customersPage.save();
  // Create first order
  await salesBasePage.goToMenu('Order', 'Order');
  await orderPageJS.createItem();
  await orderPageJS.selectCustomerByNumber(customerNumber, customerName);
  await orderPageJS.expandProducts();
  await orderPageJS.clickNewProductRow();
  await orderPageJS.addProductForCustomer(selectedProduct);
  await orderPageJS.updateQuantity('1', 0);
  await orderPageJS.verifyProductPrice([{ expectedPrice: '280,00' }]);
  await orderPageJS.saveOrder();
  const orderNumber1 = await orderPageJS.getOrderNumber();
  // Edit customer and remove product
  await salesBasePage.goToMenu('Customer_Sale', 'Customers');
  await salesBasePage.goToPageVersion(customersPage.ang_version);
  await customersPage.verifyFilterByNumber(customerNumber);
  await customersPage.editFirstRow();
  await customersPage.waitForPageLoad(true);
  await customersPage.expandProductTab();
  await customersPage.removeProduct();
  await customersPage.save();
  // Create second order
  await salesBasePage.goToMenu('Order', 'Order');
  await orderPageJS.createItem();
  await orderPageJS.selectCustomerByNumber(customerNumber, customerName);
  await orderPageJS.expandProducts();
  await orderPageJS.clickNewProductRow();
  await orderPageJS.addProductForCustomer(selectedProduct);
  await orderPageJS.updateQuantity('1', 0);
  await orderPageJS.verifyProductPrice([{ expectedPrice: '1 000,00' }]);
  await orderPageJS.saveOrder();
  const orderNumber2 = await orderPageJS.getOrderNumber();
  // Verify in statistics
  await salesBasePage.goToMenu('Customer_Sale', 'Customers');
  await salesBasePage.goToPageVersion(customersPage.ang_version);
  await customersPage.verifyFilterByNumber(customerNumber);
  await customersPage.editFirstRow();
  await customersPage.expandStatisticsTab();
  await customersPage.searchOrdersInStatistics();
  await customersPage.verifyCreatedOrderRows([
    { orderNumber: orderNumber1, expectedPrice: '280,00' },
    { orderNumber: orderNumber2, expectedPrice: '1 000,00' },
  ]);
});
