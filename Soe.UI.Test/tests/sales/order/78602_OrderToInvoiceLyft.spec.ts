import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';
import { ProductUtils } from 'apis/utils/ProductUtil';


const testCaseId: string = '78602';
const reportPath = 'test-data/temp-download/78602_invoice.pdf'
let envUrl: string;
const productNumber = `78602`
const productName = 'betalningsprodukt'
const productData = {
    "invoiceProduct.purchasePrice": 120,
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
    await new ProductUtils(page, envUrl).createProduct(productNumber, productName, productData)
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Order to invoice Lyft : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS, invoicePageJS }) => {
    let internalText = `Manual_Test_Order_Copy_${Math.random().toString(36).substring(2, 7)}`
    // Create an order and add products
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Construction service');
    await orderPageJS.saveOrder()
    await orderPageJS.expandProducts();
    await orderPageJS.addNewProduct("999", '500000', '1', 0);
    await orderPageJS.addNewProduct(productName, '250000', '1', 1);
    await orderPageJS.addNewProduct(productName, '300000', '1', 2);
    await orderPageJS.verifyProductRowCount(3)
    await orderPageJS.saveOrder()
    // Set Delfakt1 to Klar and transfer to final invoice
    await orderPageJS.addToKlar(1)
    await orderPageJS.expandProducts();
    await orderPageJS.transferToFinalInvoice(1)
    await orderPageJS.closeOrder()
    // Set Delfakt2 to Klar and transfer to preliminary invoice
    await orderPageJS.filterByInternalText(internalText)
    await orderPageJS.editOrder()
    await orderPageJS.expandProjectOrder()
    await orderPageJS.addInternalText(`${internalText}_2`);
    await orderPageJS.expandProducts();
    await orderPageJS.addToKlar(2)
    await orderPageJS.expandProducts();
    await orderPageJS.tranferToPreliminaryInvoice(2)
    // Set Material to Klar and transfer to final invoice
    await orderPageJS.addInternalText(`${internalText}_3`);
    await orderPageJS.addToKlar(0)
    await orderPageJS.expandProducts();
    await orderPageJS.transferToFinalInvoice(0)
    await orderPageJS.closeOrder()
    // Verify invoice
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.filterByInternalText(`${internalText}_3`)
    await invoicePageJS.verifyInvoice(1)
    await invoicePageJS.editInvoice()
    await invoicePageJS.expandProductRowTab()
    await invoicePageJS.verifyVATexcludedTotalValue("−50 000,00")
    await invoicePageJS.printInvoice(testCaseId)
    await invoicePageJS.verifyMultipleValueInPDF(reportPath, "Summa Momsbelopp Valuta Att erhålla", "-50 000,00 0,00 SEK -50 000,00")
});