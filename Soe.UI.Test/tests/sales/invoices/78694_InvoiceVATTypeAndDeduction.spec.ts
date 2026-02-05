import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { CustomerUtil } from 'apis/utils/CustomerUtil';

let testCaseId: string = '78694';
let envUrl: string;
let customer: any;
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
    "customer.name": `${testCaseId}_Rot_Customer`,
    "customer.customerNr": testCaseId,
}

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Invoice");
    await allure.subSuite("Invoice");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    const customerUtil = new CustomerUtil(page, envUrl)
    const customer1Res = await customerUtil.createCustomer(customerData)
    customer = customer1Res.customer
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Invoice VAT Type And Deduction : DS', { tag: ['@Sales', '@Invoice', '@Regression'] }, async ({ salesBasePage, invoicePageJS, orderPageJS }) => {
    let internalText = `${testCaseId}_${Math.random().toString(36).substring(2, 7)}}`;
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForPageLoad()
    await invoicePageJS.createItem()
    await orderPageJS.addCustomer(customer.name);
    await orderPageJS.addInternalText(internalText);
    await invoicePageJS.expandInvoice();
    await orderPageJS.setVatType('Construction service');
    await orderPageJS.expandProducts();
    await orderPageJS.addNewProduct('999', '2300', '10', 0);
    // Construction service
    await orderPageJS.clickNewProductRow()
    await orderPageJS.addProductForCustomer("930 Rot-avdrag inkl moms")
    await invoicePageJS.handleWarningPopup("OK", "ROT deductions cannot be added to the selected VAT type");
    //VAT exempt
    await orderPageJS.setVatType('VAT exempt');
    await orderPageJS.clickNewProductRow()
    await orderPageJS.addProductForCustomer("930 Rot-avdrag inkl moms")
    await invoicePageJS.handleWarningPopup("OK", "ROT deductions cannot be added to the selected VAT type");
    await invoicePageJS.saveInvoice();
    await orderPageJS.expandCodingRows()
    await orderPageJS.verifyCodingRowCount(2)
    await orderPageJS.verifyAccount("1510", "dim1Nr", 0)
    await orderPageJS.verifyAccount("3014", "dim1Nr", 1)
    await orderPageJS.verifyAccountBalance("23 000,00", "debitAmount", 0)
    await orderPageJS.verifyAccountBalance("23 000,00", "creditAmount", 1)
    //Subject to VAT
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.clickNewProductRow()
    await orderPageJS.addProductForCustomer("930 Rot-avdrag inkl moms")
    await invoicePageJS.handleWarningPopup("OK");
    await orderPageJS.verifyTaxROTDetails(rot[0].property, rot[0].socialSecNr, rot[0].name, "0")
    await invoicePageJS.saveInvoice();
    await orderPageJS.expandCodingRows()
    await orderPageJS.verifyCodingRowCount(3)
    await orderPageJS.verifyAccount("1510", "dim1Nr", 0)
    await orderPageJS.verifyAccount("3010", "dim1Nr", 1)
    await orderPageJS.verifyAccount("2620", "dim1Nr", 2)
    await orderPageJS.verifyAccountBalance("25 760,00", "debitAmount", 0)
    await orderPageJS.verifyAccountBalance("23 000,00", "creditAmount", 1)
    await orderPageJS.verifyAccountBalance("2 760,00", "creditAmount", 2)
});



