import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78691';
let envUrl: string;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Invoice");
    await allure.subSuite("Invoice");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Invoice Accounting : DS', { tag: ['@Sales', '@Invoice', '@Regression'] }, async ({ salesBasePage, invoicePageJS, orderPageJS }) => {
    let internalText = `${testCaseId}_${Math.random().toString(36).substring(2, 7)}`;
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForPageLoad()
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    //Add the first product row
    await orderPageJS.addNewProduct('999', '2300', '10');
    await invoicePageJS.saveInvoice()
    await orderPageJS.expandCodingRows()
    await orderPageJS.verifyCodingRowCount(3)
    await orderPageJS.verifyAccount("1510", "dim1Nr", 0)
    await orderPageJS.verifyAccount("3010", "dim1Nr", 1)
    await orderPageJS.verifyAccount("2620", "dim1Nr", 2)
    await orderPageJS.verifyAccountBalance("25 760,00", "debitAmount", 0)
    await orderPageJS.verifyAccountBalance("23 000,00", "creditAmount", 1)
    await orderPageJS.verifyAccountBalance("2 760,00", "creditAmount", 2)
    //Add the second product row
    await orderPageJS.addNewProduct('999', '2000', '10', 1);
    await invoicePageJS.saveInvoice()
    await orderPageJS.expandCodingRows()
    await orderPageJS.verifyCodingRowCount(3)
    await orderPageJS.verifyAccount("1510", "dim1Nr", 0)
    await orderPageJS.verifyAccount("3010", "dim1Nr", 1)
    await orderPageJS.verifyAccount("2620", "dim1Nr", 2)
    await orderPageJS.verifyAccountBalance("48 160,00", "debitAmount", 0)
    await orderPageJS.verifyAccountBalance("43 000,00", "creditAmount", 1)
    await orderPageJS.verifyAccountBalance("5 160,00", "creditAmount", 2)
    //Update the second product row
    await orderPageJS.addNewProduct('999', '1300', '10', 1);
    await invoicePageJS.saveInvoice()
    await orderPageJS.expandCodingRows()
    await orderPageJS.verifyCodingRowCount(3)
    await orderPageJS.verifyAccount("1510", "dim1Nr", 0)
    await orderPageJS.verifyAccount("3010", "dim1Nr", 1)
    await orderPageJS.verifyAccount("2620", "dim1Nr", 2)
    await orderPageJS.verifyAccountBalance("40 320,00", "debitAmount", 0)
    await orderPageJS.verifyAccountBalance("36 000,00", "creditAmount", 1)
    await orderPageJS.verifyAccountBalance("4 320,00", "creditAmount", 2)
});



