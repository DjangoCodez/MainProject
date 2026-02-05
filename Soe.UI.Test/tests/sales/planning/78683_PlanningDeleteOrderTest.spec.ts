import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { OrderUtil } from '../../../apis/utils/OrderUtil';
import { getDateUtil } from '../../../utils/CommonUtil';

let testCaseId: string = '78683';
let envUrl: string;
let orderNumber: string;
let orderUtils: OrderUtil;
const employeeName = 'Playwright Employee';
const customerName = 'Playwright Test Customer';

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Planning");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    orderUtils = new OrderUtil(page, envUrl);
    orderNumber = await orderUtils.CreateOrder();
});

test(testCaseId + ': Planning delete order : AP', { tag: ['@Sales', '@Orders', '@Regression'] }, async ({ salesBasePage, planningPageJS, orderPageJS }) => {
    const date = await getDateUtil(0);
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.filterAllOrders();
    await orderPageJS.filterByOrderNo(orderNumber.toString());
    await orderPageJS.editOrder();
    await orderPageJS.expandPlanning();
    await orderPageJS.selectAssignmentType();
    await orderPageJS.planStartDate(date);
    await orderPageJS.saveOrder();
    await salesBasePage.goToMenu('Order', 'Planning');
    await planningPageJS.waitOrderLoading();
    await planningPageJS.reloadOrders();
    await planningPageJS.filterOrder(orderNumber);
    await planningPageJS.planOrderDragandDrop(orderNumber, employeeName, customerName);
    await planningPageJS.waitForDialogPopup();
    await planningPageJS.saveNewAssignment();
    await planningPageJS.reloadOrders();
    await planningPageJS.removeAssignment(employeeName, orderNumber);
    await salesBasePage.clickAlertMessage('OK');
    await planningPageJS.reloadOrders();
    await planningPageJS.filterOrder(orderNumber);
    await planningPageJS.verifyTimeInOrder(orderNumber, '4:00/4:00');
});
