import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';

let testCaseId: string = '78152';
let envUrl: string;
let customer: string = getEnvironmentValue('default_customer') ?? '';
let employee: string = getEnvironmentValue('default_employee') ?? '';
let internalText: string = `Auto ${Math.random().toString(36).substring(2, 7)} ` + ' ' + testCaseId;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ accountEx, salesBasePage, page }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Projects");
    await allure.subSuite("Time Report");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    await new ManageUtils(page, envUrl).verifyOrderCheckLists();
});

test(testCaseId + ': Verify time register : SR', { tag: ['@Sales', '@Projects', '@TimeReport', '@Regression'] }, async ({ timeReportPage, orderPageJS }) => {
    await orderPageJS.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(customer);
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.saveOrder();
    const capturedOrderNumber = await orderPageJS.getOrderNumber();
    const capturedProjectNumber = await orderPageJS.getProjectNumber();
    const orderDropdownValue = `${capturedOrderNumber} - ${customer}`;
    const projectDropdownValue = `${capturedProjectNumber} ${customer}`;
    await timeReportPage.goToMenu('Projects', 'Time Report');
    await timeReportPage.clickAddRow();
    await timeReportPage.setRegisterTimeDetails({employeeName: employee, orderNumber: orderDropdownValue, projectNumber: projectDropdownValue, chargingType: 'Arbetad tid', timeWorked: '4', billableTime: '4', rowIndex: 0 });
    await timeReportPage.saveRegisteredTime();
    await timeReportPage.applyFilters({ "Order": capturedOrderNumber });
    await timeReportPage.VerifyRowCount(1);
    await orderPageJS.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.filterByOrderNo(capturedOrderNumber);
    await orderPageJS.editOrder();
    await orderPageJS.expandTimes();
    await orderPageJS.page.waitForTimeout(3000);
    await orderPageJS.verifyTotalHoursInTimesSection('4:00');
    await orderPageJS.expandProducts();
    await orderPageJS.verifyProductRowCount(1);
    await orderPageJS.verifyToInvoiceGreaterThanZero();
});