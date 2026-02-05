import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { getDateUtil } from '../../../utils/CommonUtil';

let testCaseId: string = '78167';
let envUrl: string;
let internalText: string = `Auto ${Math.random().toString(36).substring(2, 7)} ` + ' ' + testCaseId;
let customer: string = getEnvironmentValue('default_customer') ?? '';
let employee_1: string = getEnvironmentValue('default_timeReportEmployee') ?? '';
let employee_2: string = getEnvironmentValue('default_timeReportEmployee2') ?? '';

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Projects");
    await allure.subSuite("Time Report");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Verify time delete : SR', { tag: ['@Sales', '@Projects', '@TimeReport', '@Regression'] }, async ({ timeReportPage, orderPageJS }) => {
    await orderPageJS.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(customer);
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.saveOrder();
    const capturedOrderNumber = await orderPageJS.getOrderNumber();
    await orderPageJS.expandTimes();
    await orderPageJS.addWork({ employeeName: employee_1 });
    await orderPageJS.addWork({ employeeName: employee_2 });
    await orderPageJS.verifyRowCountTimeGrid(3); // 2 time entries + 1 footer row
    await timeReportPage.goToMenu('Projects', 'Time Report');
    const today = await getDateUtil(0, false);
    await timeReportPage.setDateFilter(today, today);
    await timeReportPage.clickSearchButton();
    await timeReportPage.applyFilters({ "Order": capturedOrderNumber });
    await timeReportPage.VerifyRowCount(2);
    await timeReportPage.selectFirstRow();
    await timeReportPage.deleteSelectedTimeEntries();
    await timeReportPage.VerifyRowCount(1);
    await timeReportPage.clickEditButton();
    await timeReportPage.deleteTimeEntryInEditMode(capturedOrderNumber);
    await timeReportPage.page.reload();
    await timeReportPage.setDateFilter(today, today);
    await timeReportPage.clickSearchButton();
    await timeReportPage.applyFilters({ "Order": capturedOrderNumber });
    await timeReportPage.VerifyRowCount(0);
    await orderPageJS.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.filterByOrderNo(capturedOrderNumber);
    await orderPageJS.editOrder();
    await orderPageJS.expandProducts();
    await orderPageJS.verifyProductRowCount(0);
});