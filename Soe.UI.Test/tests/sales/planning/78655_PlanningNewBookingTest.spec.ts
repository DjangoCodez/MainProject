import { test } from '../../fixtures/finance-fixture';
import * as allure from 'allure-js-commons';
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { getDateUtil } from '../../../utils/CommonUtil';
import { EmployeeUtil } from 'apis/utils/EmployeeUtil';
import { ScheduleUtil } from 'apis/utils/ScheduleUtil';

let envUrl: string;
let employeeName: string = getEnvironmentValue('default_employee') ?? '';
let employeeUtil: EmployeeUtil;
let employeeId: string;
let scheduleUtil: ScheduleUtil;


test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite('Sales');
  await allure.suite('Orders');
  await allure.subSuite('Planning');
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/78655', 'Test case : 78655');
  employeeUtil = new EmployeeUtil(page, envUrl);
  let employeeResponse = await employeeUtil.getEmployees();
  employeeId = employeeResponse.filter((emp: any) => emp.name === employeeName)[0]?.employeeId;
  scheduleUtil = new ScheduleUtil(page, envUrl);
});


test('Planning New Booking : AP', { tag: ['@Sales', '@Orders', '@Regression'] }, async ({ page, salesBasePage, planningPageJS }) => {
  const date = await getDateUtil(0);
  await salesBasePage.goToMenu('Order', 'Planning');
  await planningPageJS.waitOrderLoading();
  let scheduleResponse: string[] = await planningPageJS.removeAllReadyAssignments(employeeId);
  const shiftIds: number[] = scheduleResponse.map(Number);
  if (shiftIds.length > 0) {
    await scheduleUtil.removeSchedule(shiftIds);
  }
  await planningPageJS.createOrderOrBookingByShiftByDate(employeeId, date);
  await planningPageJS.newBooking();
  await planningPageJS.bookingType();
  await planningPageJS.description(`Test Description`);
  await planningPageJS.saveBooking();
  await planningPageJS.createOrderOrBookingByShiftByDate(employeeId, date);
  await planningPageJS.editBooking();
  await planningPageJS.verifyDescription('Test Description');
  await planningPageJS.verifyLength('4:00');
}
);


