import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { CustomerUtil } from 'apis/utils/CustomerUtil';

let testCaseId: string = '78692';
let envUrl: string;
let customer1: any;
let customer2: any;
let customer2Id: number;
let customer1Id: number;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Invoice");
    await allure.subSuite("Invoice");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    const customerUtil = new CustomerUtil(page, envUrl)
    const customer1Res = await customerUtil.createCustomer({ "customer.vatType": 1 })
    customer1 = customer1Res.customer
    customer1Id = customer1Res.customerId
    const customer2Res = await customerUtil.createCustomer({ "customer.vatType": 3 })
    customer2 = customer2Res.customer
    customer2Id = customer2Res.customerId
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Invoice Change Customer : DS', { tag: ['@Sales', '@Invoice', '@Regression'] }, async ({ salesBasePage, customersPage, orderPageJS, invoicePageJS }) => {
    let internalText = `${testCaseId}_${Math.random().toString(36).substring(2, 7)}}`;
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
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForPageLoad()
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(customer1.name);
    await invoicePageJS.waitForCustomerDataReset(customer1Id);
    await invoicePageJS.expandTerms();
    await invoicePageJS.getCustomerInvoiceAddress(customer1.contactAddresses[1].displayAddress);
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.verifyVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.addNewProduct('999', '2300', '10');
    await invoicePageJS.saveInvoice()
    await orderPageJS.closeOrder()
    await invoicePageJS.filterByCustomerName(customer1.name);
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.editInvoice();
    // Verify updated order
    await invoicePageJS.expandDebit()
    await orderPageJS.addCustomer(customer2.name);
    await invoicePageJS.waitForCustomerDataReset(customer2Id);
    await invoicePageJS.expandTerms();
    await invoicePageJS.getCustomerInvoiceAddress(customer2.contactAddresses[1].displayAddress);
    await invoicePageJS.expandInvoice();
    await orderPageJS.verifyVatType('Construction service');
    await invoicePageJS.saveInvoice()
    await orderPageJS.closeOrder()
    await invoicePageJS.filterByCustomerName(customer2.name);
    await invoicePageJS.verifyInvoice(1)
});



