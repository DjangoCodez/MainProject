import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';
import { ProductUtils } from 'apis/utils/ProductUtil';


const testCaseId: string = '78590';
let envUrl: string;
const paymentProductName = 'paymentproduct'
const paymentProductData = {
    "invoiceProduct.vatType": 0,
    "invoiceProduct.calculationType": 3,
    "invoiceProduct.accountingPrio": "1=0,2=0,3=0",
}

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Order");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await new ManageUtils(page, envUrl).verifyOrderCheckLists()
    await new ProductUtils(page, envUrl).createProduct(testCaseId, paymentProductName, paymentProductData)
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Order to fixed invoice Lyft : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS }) => {
    let internalText = `Manual_Test_Order_Copy_${Math.random().toString(36).substring(2, 7)}`
    // Create an order and add products
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Construction service');
    await orderPageJS.setAgreementType("Fixed price")
    await orderPageJS.verifyProductRowCount(1)
    // Add fixed price product
    await orderPageJS.addFixedPriceProduct(`Fastpris`, '1', '500001', 0);
    // Add other products
    await orderPageJS.addNewProduct("Arbete", '490', '5', 1);
    await orderPageJS.addNewProduct("999", '2300', '20', 2);
    const item2Details = await orderPageJS.getProductValuesFromRows(1)
    const item3Details = await orderPageJS.getProductValuesFromRows(2)
    await orderPageJS.saveOrder()
    // Add work item and another product
    await orderPageJS.expandTimes()
    await orderPageJS.addWork();
    await orderPageJS.verifyTimeRowCount(1);
    await orderPageJS.addNewProduct("999", '5000', '10', 3);
    const item4Details = await orderPageJS.getProductValuesFromRows(4)
    await orderPageJS.saveOrder()
    await orderPageJS.verifyProductRowCount(5)
    await orderPageJS.verifyBalanceLeftToInvoice("500 001,00")
    // Add payment products
    await orderPageJS.addNewProduct(paymentProductName, '250000', '1', 4);
    await orderPageJS.addNewProduct(paymentProductName, '250001', '1', 5);
    await orderPageJS.saveOrder()
    await orderPageJS.verifyProductRowCount(7)
    // Transfer payment products to preliminary invoice
    await orderPageJS.addToKlar(4)
    await orderPageJS.expandProducts();
    await orderPageJS.tranferToPreliminaryInvoice(4)
    await orderPageJS.closeOrder()
    await orderPageJS.reloadOrders()
    await orderPageJS.filterByInternalText(internalText)
    await orderPageJS.editOrder()
    await orderPageJS.expandProducts();
    await orderPageJS.verifyBalanceLeftToInvoice("250 001,00")
    await orderPageJS.addToKlar(5)
    await orderPageJS.expandProducts();
    await orderPageJS.tranferToPreliminaryInvoice(5)
    await orderPageJS.closeOrder()
    await orderPageJS.reloadOrders()
    await orderPageJS.filterByInternalText(internalText)
    await orderPageJS.editOrder()
    await orderPageJS.expandProducts();
    await orderPageJS.verifyBalanceLeftToInvoice("0,00")
    expect(item2Details.itemPrice).toBe('0,00')
    expect(item3Details.itemPrice).toBe('0,00')
    expect(item4Details.itemPrice).toBe('0,00')
});