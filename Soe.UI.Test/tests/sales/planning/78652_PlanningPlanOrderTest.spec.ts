import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { OrderUtil } from '../../../apis/utils/OrderUtil';
import { ScheduleUtil } from '../../../apis/utils/ScheduleUtil';
import { getDateUtil } from '../../../utils/CommonUtil';
import { EmployeeUtil } from '../../../apis/utils/EmployeeUtil';

let testCaseId: string = '78652';
let envUrl: string;
let orderNumber: string;
let orderUtils: OrderUtil;
let scheduleUtil: ScheduleUtil;
let employeeUtil: EmployeeUtil;
const employeeName = 'Playwright Employee';
const customerName = 'Playwright Test Customer';
let employeeId: string;

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Orders");
  await allure.subSuite("Planning");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  orderUtils = new OrderUtil(page, envUrl);
  scheduleUtil = new ScheduleUtil(page, envUrl);
  orderNumber = await orderUtils.CreateOrder();
  employeeUtil = new EmployeeUtil(page, envUrl);
  let employeeResponse = await employeeUtil.getEmployees();
  employeeId = employeeResponse.filter((emp: any) => emp.name === employeeName)[0]?.employeeId;
});

test(testCaseId + ': Planning plan order : AP', { tag: ['@Sales', '@Orders', '@Regression'] }, async ({ salesBasePage, planningPageJS, orderPageJS }) => {
  const date = await getDateUtil(0);
  await salesBasePage.goToMenu('Order', 'Order');
  await orderPageJS.waitForOrderLoading();
  await orderPageJS.filterAllOrders();
  await orderPageJS.filterByOrderNo(orderNumber.toString());
  await orderPageJS.editOrder();
  await orderPageJS.expandPlanning();
  await orderPageJS.selectAssignmentType();
  await orderPageJS.planStartDate(date);
  await orderPageJS.saveOrder();
  await salesBasePage.goToMenu('Order', 'Planning');
  await planningPageJS.waitOrderLoading();
  let scheduleResponse: string[] = await planningPageJS.removeAllReadyAssignments(employeeId);
  const shiftIds: number[] = scheduleResponse.map(Number);
  if (shiftIds.length > 0) {
    await scheduleUtil.removeSchedule(shiftIds);
  }
  await planningPageJS.reloadOrders();
  await planningPageJS.filterOrder(orderNumber);
  await planningPageJS.planOrderDragandDrop(orderNumber, employeeName, customerName);
  await planningPageJS.waitForDialogPopup();
  await planningPageJS.verifyPlanningTime('04:00');
  await planningPageJS.saveNewAssignment();
  await planningPageJS.reloadOrders();
  await planningPageJS.editAssignment(employeeName);
  await planningPageJS.editEndTime('08');
  await planningPageJS.saveNewAssignment();
  await planningPageJS.reloadOrders();
  await planningPageJS.verifyTimeInOrder(orderNumber, '3:00/4:00');
});


