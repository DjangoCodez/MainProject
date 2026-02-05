import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78198';
let envUrl: string;
let internalText: string = `Auto ${Math.random().toString(36).substring(2, 7)} ` + ' ' + testCaseId;
let customer: string = getEnvironmentValue('default_customer') ?? '';
let employee_1: string = getEnvironmentValue('default_timeMoveEmployee') ?? '';
let employee_2: string = getEnvironmentValue('default_timeMoveEmployee2') ?? '';
let employee_3: string = getEnvironmentValue('default_timeMoveEmployee3') ?? '';

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Projects");
    await allure.subSuite("Time Report");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Verify time move rows : SR', { tag: ['@Sales', '@Order', '@TimeReport', '@Regression'] }, async ({ orderPageJS }) => {
    await orderPageJS.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(customer);
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.saveOrder();
    await orderPageJS.expandTimes();
    await orderPageJS.addWork({ timeWorked: '1', billableTime: '1' });
    await orderPageJS.addWork({ employeeName: employee_1, timeWorked: '2', billableTime: '2' });
    await orderPageJS.addWork({ employeeName: employee_2, timeWorked: '3', billableTime: '3' });
    await orderPageJS.addWork({ employeeName: employee_3, timeWorked: '4', billableTime: '4' });
    await orderPageJS.expandProducts();
    await orderPageJS.selectProductRow(0);
    await orderPageJS.rightClickSelectedProductRow();
    await orderPageJS.clickShowLinkedTimeRows();
    await orderPageJS.verifyDialogPopupVisible();
    await orderPageJS.verifyWorkingTimeInPopupGrid([{ expectedTime: '10:00' }], 4);
    await orderPageJS.verifyInvoicedTimeInPopupGrid([{ expectedTime: "10:00" }], 4);
    await orderPageJS.selectTimeRowInPopupGrid(0);
    await orderPageJS.clickMoveTimeRowsToNewButton();
    await orderPageJS.verifyRowCountShowLinkedPopup(4); // 3 time entries + 1 footer row
    await orderPageJS.verifyWorkingTimeInPopupGrid([{ expectedTime: '09:00' }], 3);
    await orderPageJS.verifyInvoicedTimeInPopupGrid([{ expectedTime: "09:00" }], 3);
    await orderPageJS.closeDialogPopup();
    await orderPageJS.verifyProductRowCount(2);
    await orderPageJS.unselectProductRow(0);
    await orderPageJS.selectProductRow(1);
    await orderPageJS.rightClickSelectedProductRow();
    await orderPageJS.clickShowLinkedTimeRows();
    await orderPageJS.verifyDialogPopupVisible();
    await orderPageJS.verifyRowCountShowLinkedPopup(2); // 1 time entry + 1 footer row
    await orderPageJS.verifyWorkingTimeInPopupGrid([{ expectedTime: "01:00" }], 1);
    await orderPageJS.verifyInvoicedTimeInPopupGrid([{ expectedTime: "01:00" }], 1);
    await orderPageJS.clickCancelButtonInPopup();
    await orderPageJS.unselectProductRow(1);
    await orderPageJS.selectProductRow(0);
    await orderPageJS.rightClickSelectedProductRow();
    await orderPageJS.clickShowLinkedTimeRows();
    await orderPageJS.selectTimeRowInPopupGrid(0);
    await orderPageJS.clickMoveTimeRowsToExistingButton();
    await orderPageJS.selectRowToMoveToPopupGrid();
    await orderPageJS.clickOkButtonInMoveToExistingPopup();
    await orderPageJS.verifyRowCountShowLinkedPopup(3); // 1 time entry + 1 footer row
    await orderPageJS.verifyWorkingTimeInPopupGrid([{ expectedTime: "07:00" }], 2);
    await orderPageJS.clickOkButtonInPopup();
    await orderPageJS.verifyProductRowCount(2);
    const itemDetails = await orderPageJS.getProductValuesFromRows(0);
    expect(itemDetails.itemQuantity).toBe('7');
    const itemDetails1 = await orderPageJS.getProductValuesFromRows(1);
    expect(itemDetails1.itemQuantity).toBe('3');
    await orderPageJS.selectProductRow(1);
    await orderPageJS.rightClickSelectedProductRow();
    await orderPageJS.clickShowLinkedTimeRows();
    await orderPageJS.verifyRowCountShowLinkedPopup(3); // 2 time entries + 1 footer row
    await orderPageJS.verifyWorkingTimeInPopupGrid([{ expectedTime: '03:00' }], 2);
});