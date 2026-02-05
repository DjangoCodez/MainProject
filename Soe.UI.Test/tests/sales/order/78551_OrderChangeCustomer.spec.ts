import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { CustomerUtil } from 'apis/utils/CustomerUtil';
import { OrderUtil } from 'apis/utils/OrderUtil';

let testCaseId: string = '78551';
let envUrl: string;
let customer1: any;
let customer2: any;
let order: number;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Order");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    const customerUtil = new CustomerUtil(page, envUrl)
    const orderUtil = new OrderUtil(page, envUrl)
    const customer1Res = await customerUtil.createCustomer({ "customer.vatType": 1 })
    customer1 = customer1Res.customer
    order = await orderUtil.CreateOrder(customer1Res.customerId)
    const customer2Res = await customerUtil.createCustomer({ "customer.vatType": 3 })
    customer2 = customer2Res.customer
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Order Change Customer : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, customersPage, orderPageJS }) => {
    // Verify customer's details
    await salesBasePage.goToMenu('Customer_Sale', 'Customers');
    await customersPage.verifyFilterByName(customer1.name)
    await customersPage.editFirstRow()
    await customersPage.verifyDeliveryAddress("Delivery address", customer1.contactAddresses[0].displayAddress)
    await customersPage.verifyInvoiceAddress("Invoice address", customer1.contactAddresses[1].displayAddress)
    await customersPage.closeTab()
    await customersPage.verifyFilterByName(customer2.name)
    await customersPage.editFirstRow()
    await customersPage.verifyDeliveryAddress("Delivery address", customer2.contactAddresses[0].displayAddress)
    await customersPage.verifyInvoiceAddress("Invoice address", customer2.contactAddresses[1].displayAddress)
    await customersPage.closeTab()
    // Change customer
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.filterByOrderNo(order.toString())
    await orderPageJS.editOrder();
    await orderPageJS.expandProjectOrder();
    await orderPageJS.addCustomer(customer2.name);
    await orderPageJS.clickAlertMessage("OK")
    await orderPageJS.saveOrder();
    // Verify updated order
    await orderPageJS.closeOrder()
    await orderPageJS.reloadOrders()
    await orderPageJS.filterByOrderNo(order.toString())
    await orderPageJS.getOrderDetails(customer2.customerNr.toString(), "actorCustomerNr", 0)
    await orderPageJS.getOrderDetails(customer2.name, "actorCustomerName", 0)
});



