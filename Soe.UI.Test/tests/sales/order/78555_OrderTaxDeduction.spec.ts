import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';
import { CustomerUtil } from 'apis/utils/CustomerUtil';
import { ProductUtils } from 'apis/utils/ProductUtil';

let testCaseId: string = '78555';
let envUrl: string;
let customer: any;
const name = `${testCaseId}_Customer`
const productNumber = testCaseId
const productName = `Rot_Product`
const productData = {
    "invoiceProduct.vatType": 2,
    "invoiceProduct.productUnitId": 1,
    "invoiceProduct.vatCodeId": null,
    "invoiceProduct.householdDeductionType": 2,
    "invoiceProduct.accountingPrio": "1=0,2=0,3=0",
    "invoiceProduct.state": 0,
    "invoiceProduct.accountingSettings": [],
}
const rot = [
    {
        "new": true,
        "property": "Family House",
        "name": "Drottningholm Palace",
        "socialSecNr": "19801011-7102",
        "ag_node_id": "0"
    }
]
const customerData = {
    "customer.isPrivatePerson": true,
    "houseHoldTaxApplicants": rot,
    "customer.name": name,
    "customer.customerNr": testCaseId,
    "customer.customerProducts": [
        {
            "productId": 643,
            "price": 120,
            "ag_node_id": "0",
            "number": `${testCaseId}`,
            "isModified": true,
            "name": `Rot_Product`
        }
    ]
}

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Order");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await new ManageUtils(page, envUrl).verifyOrderCheckLists();
    const { productId } = await new ProductUtils(page, envUrl).createProduct(productNumber, productName, productData)
    customerData['customer.customerProducts'][0]['productId'] = productId
    const customerUtil = new CustomerUtil(page, envUrl)
    const customer1Res = await customerUtil.createCustomer(customerData)
    customer = customer1Res.customer
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Order tax deduction : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS, invoicePageJS }) => {
    let internalText = `Manual_Test_Order_Copy_${Math.random().toString(36).substring(2, 7)}`
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(customer.name);
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    // Add first item
    await orderPageJS.newProductRow(productName, '5', '490', ``, 0);
    // Add second item
    await orderPageJS.clickNewProductRow()
    await orderPageJS.addProductForCustomer("930 Rot-avdrag inkl moms")
    await orderPageJS.verifyTaxROTDetails(rot[0].property, rot[0].socialSecNr, rot[0].name, "918")
    await orderPageJS.verifyVATexcludedLeftValue("1 532,00")
    await orderPageJS.verifyVATexcludedTotalValue("2 450,00")
    await orderPageJS.saveOrder();
    await orderPageJS.addToKlar()
    await orderPageJS.expandProducts()
    const { invoiceNumber } = await orderPageJS.transferToFinalInvoice()
    await salesBasePage.goToMenu('Invoice', 'Payments');
    await invoicePageJS.filterByInvoiceNumber(invoiceNumber)
    await invoicePageJS.filterByCustomerName(customer.name)
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.createPayment("PaymentMethod1")
    await salesBasePage.goToMenu('Invoice', 'Tax Deduction');
    await invoicePageJS.filterByInvoiceNumber(invoiceNumber)
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.createApplication()
    await invoicePageJS.navigateToTab("Applied")
    await invoicePageJS.filterByInvoiceNumberInTabs(invoiceNumber)
    await invoicePageJS.processApplication("Approve deduction")
    await invoicePageJS.navigateToTab("Approved")
    await invoicePageJS.filterByInvoiceNumberInTabs(invoiceNumber)
});