import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';

let testCaseId: string = '78559';
let envUrl: string;
const checklist = "78559_Test_CheckList"

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Order");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await new ManageUtils(page, envUrl).createCheckList({ name: `${testCaseId}_Test_CheckList` });
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Order order checklist : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS }) => {
    let internalText = `Manual_Test_Order_Copy_${Math.random().toString(36).substring(2, 7)}`
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow("999", '1', '100');
    await orderPageJS.expandChecklist()
    await orderPageJS.addChecklist(checklist)
    await orderPageJS.answerChecklistCheckbox("Did the customer pay for delivery?", true)
    await orderPageJS.answerChecklistYesNO("Is the order subject to VAT?", "Yes")
    await orderPageJS.answerChecklistFreeText("Is the payment method for the order cheque?", "CN879-325-778")
    await orderPageJS.saveOrder()
    await orderPageJS.answerChecklistUploadImage("Is a copy of the contract attached?")
    await orderPageJS.saveOrder()
    await orderPageJS.closeOrder()
    await orderPageJS.reloadOrders()
    await orderPageJS.filterByInternalText(internalText)
    await orderPageJS.reloadOrders()
    await orderPageJS.verifyOrder()
});