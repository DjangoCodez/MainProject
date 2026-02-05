import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78172';
let envUrl: string;
let internalText: string = `Auto ${Math.random().toString(36).substring(2, 7)} ` + ' ' + testCaseId;
let customer: string = getEnvironmentValue('default_customer') ?? '';

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Projects");
    await allure.subSuite("Time Report");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Verify time order : SR', { tag: ['@Sales', '@Order', '@TimeReport', '@Regression'] }, async ({ orderPageJS }) => {
    await orderPageJS.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(customer);
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.saveOrder();
    await orderPageJS.expandTimes();
    await orderPageJS.addWorkTime();
    await orderPageJS.expandProducts();
    await orderPageJS.verifyProductRowCount(1);
    const itemDetails = await orderPageJS.getProductValuesFromRows(0);
    expect(itemDetails.itemQuantity).toBe('2');
    await orderPageJS.clickEditButton();
    await orderPageJS.updateWorkTime({ billableTime: '3' });
    const itemDetails1 = await orderPageJS.getProductValuesFromRows(0);
    expect(itemDetails1.itemQuantity).toBe('3');
    await orderPageJS.clickEditButton();
    await orderPageJS.updateWorkTime({ billableTime: '0' });
    await orderPageJS.verifyProductRowCount(0);
    await orderPageJS.clickEditButton();
    await orderPageJS.updateWorkTime({ billableTime: '1' });
    const itemDetails2 = await orderPageJS.getProductValuesFromRows(0);
    expect(itemDetails2.itemQuantity).toBe('1');
    await orderPageJS.clickEditButton();
    await orderPageJS.deleteTimeEntryInEditMode();
    await orderPageJS.verifyProductRowCount(0);
    await orderPageJS.addWork();
    await orderPageJS.verifyProductRowCount(1);
    const itemDetails3 = await orderPageJS.getProductValuesFromRows(0);
    expect(itemDetails3.itemQuantity).toBe('2');
});