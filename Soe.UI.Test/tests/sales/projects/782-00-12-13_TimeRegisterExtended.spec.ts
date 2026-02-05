import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';
import { generateSocialSecNumber, getDateUtil } from '../../../utils/CommonUtil';
import { EmployeeUtil } from '../../../apis/utils/EmployeeUtil';

let envUrl: string;
let customer: string = getEnvironmentValue('default_customer') ?? '';
let employeeUtil: EmployeeUtil;
let internalText: string = `Auto ${Math.random().toString(36).substring(2, 7)}_78200 `;

const firstName2 = Array.from({ length: 5 }, () => String.fromCharCode(97 + Math.floor(Math.random() * 26))).join('') + Date.now();
const lastName2 = Array.from({ length: 5 }, () => String.fromCharCode(97 + Math.floor(Math.random() * 26))).join('') + Date.now();

const timeTemplate = {
    startDate: new Date(Date.UTC(new Date().getUTCFullYear(), new Date().getUTCMonth(), new Date().getUTCDate() - ((new Date().getUTCDay() + 6) % 7))).toISOString(),
    endDate: new Date(Date.UTC(new Date().getUTCFullYear() + 1, new Date().getUTCMonth(), new Date().getUTCDate() + ((8 - new Date(Date.UTC(new Date().getUTCFullYear() + 1, new Date().getUTCMonth(), new Date().getUTCDate())).getUTCDay()) % 7 || 7))).toISOString()
}

const timeTemplateData = {
    name: `TT_78200_${Math.random().toString(36).substring(2, 7)}`,
    startDate: timeTemplate.startDate,
    firstMondayOfCycle: timeTemplate.endDate,
    timeCodeId: 150,
    shiftTypeId: 111,
    noOfDays: 7,
}

const ssn = generateSocialSecNumber();

const employeeData = {
    actorCompanyId: 1226,
    salaryAgreement: "HAO Månadslön 18 år",
    employmentTypeName: "Permanent Position",
    employeeGroupName: `Order - Lön (månad)`,
    firstName: firstName2,
    lastName: lastName2,
    fullName: `${firstName2} ${lastName2}`,
    ssn: ssn,
    timeCodeId: timeTemplateData.timeCodeId
}

test.use({ account: { domain: process.env.defualt_domain!, user: 'admin' } });

test.beforeEach(async ({ accountEx, salesBasePage, page }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Projects");
    await allure.subSuite("Time Report");
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    await new ManageUtils(page, envUrl).verifyOrderCheckLists();
    employeeUtil = new EmployeeUtil(page, envUrl);
});

test.describe.serial('Time Register Extended', () => {
    let createdEmployeeId: number;
    let capturedOrderNumber: string;

    test("Setup Time Agreement and Employee", { tag: ['@Sales', '@Projects', '@TimeReport', '@Regression'] }, async ({ }) => {
        const createdEmployee = await employeeUtil.createEmployee(employeeData);
        expect(createdEmployee, 'Employee creation failed').toBeTruthy();
        expect(createdEmployee.employeeNr, 'Employee number missing').toBeTruthy();
        createdEmployeeId = createdEmployee.employeeNr;
        const { timeScheduleId } = await employeeUtil.saveTimeSchedule(ssn, timeTemplateData);
        await employeeUtil.page.waitForTimeout(3000);
        await employeeUtil.activateEmployeeSchedule(timeScheduleId, ssn, timeTemplate.startDate, timeTemplate.endDate);
    });

    test('78200 : Verify time register : SR', { tag: ['@Sales', '@Projects', '@TimeReport', '@Regression'] }, async ({ timeReportPage, orderPageJS }) => {
        await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/78200', "Test case : 78200");
        await orderPageJS.goToMenu('Order', 'Order');
        await orderPageJS.waitForPageLoad();
        await orderPageJS.createItem();
        await orderPageJS.addCustomer(customer);
        await orderPageJS.addInternalText(internalText);
        await orderPageJS.saveOrder();
        capturedOrderNumber = await orderPageJS.getOrderNumber();
        const capturedProjectNumber = await orderPageJS.getProjectNumber();
        const orderDropdownValue = `${capturedOrderNumber} - ${customer}`;
        const projectDropdownValue = `${capturedProjectNumber} ${customer}`;
        await timeReportPage.goToMenu('Projects', 'Time Report');
        await timeReportPage.clickAddRow();
        await timeReportPage.setRegisterTimeDetails({ employeeName: employeeData.fullName, orderNumber: orderDropdownValue, projectNumber: projectDropdownValue, chargingType: 'Arbetad tid', timeWorked: '4', billableTime: '4', rowIndex: 0 });
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

    test('78213 : Time attest page [register extended] : SR', { tag: ['@Sales', '@Projects', '@TimeReport', '@Regression'] }, async ({ attestTimePage }) => {
        await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/78213', "Test case : 78213");
        await attestTimePage.switchMenu('Staff');
        await attestTimePage.goToMenu('Time', 'Attest Time');
        await attestTimePage.waitforPageLoad();
        await attestTimePage.searchEmployee(createdEmployeeId.toString(), employeeData.fullName);
        const today = new Date();
        const todayDate = `${today.getMonth() + 1}/${today.getDate()}/${today.getFullYear()}`;
        await attestTimePage.selectRowByDate(todayDate);
        await attestTimePage.verifyAttestLevelForDate(todayDate, "Registrerad");
        await attestTimePage.setToAttestationLevelSelectedRow("Attest");
        await attestTimePage.selectRowByDate(todayDate);
        await attestTimePage.verifyAttestLevelForDate(todayDate, "Attest");
        await attestTimePage.setToAttestationLevelSelectedRow("Registrerad");
        await attestTimePage.verifyAttestLevelForDate(todayDate, "Registrerad");
    });

    test('78212 : Time attest [register extended] : SR', { tag: ['@Sales', '@Projects', '@TimeReport', '@Regression'] }, async ({ timeReportPage }) => {
        await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/78212', "Test case : 78212");
        await timeReportPage.goToMenu('Projects', 'Time Report');
        await timeReportPage.waitForPageLoad();
        const today = await getDateUtil(0, false);
        await timeReportPage.setDateFilter(today, today);
        await timeReportPage.applyFilters({ "Order": capturedOrderNumber });
        await timeReportPage.VerifyRowCount(1);
        await timeReportPage.selectFirstRow();
        await timeReportPage.setAttestationLevel("Attest");
        await timeReportPage.verifyAttestationLevel("Attest",);
        await timeReportPage.clickEditButton();
        await timeReportPage.verifyRowCellsEditability({
            'employeeName': false,
            'invoiceNr': false,
            'projectNr': false,
            'date': false,
            'weekNo': false,
            'timeDeviationCauseName': false,
            'timeCodeName': true,
            'timePayrollQuantityFormattedEdit': false,
            'invoiceQuantityFormatted': true,
            'externalNote': true,
            'internalNote': true
        }, 0);
        await timeReportPage.closeRegisterTimeModal();
        await timeReportPage.selectFirstRow();
        await timeReportPage.setAttestationLevel("Registrerad");
        await timeReportPage.verifyAttestationLevel("Registrerad",);
        await timeReportPage.clickEditButton();
        await timeReportPage.verifyRowCellsEditability({
            'employeeName': false,
            'invoiceNr': false,
            'projectNr': false,
            'date': false,
            'weekNo': false,
            'timeDeviationCauseName': true,
            'timeCodeName': true,
            'timePayrollQuantityFormattedEdit': true,
            'invoiceQuantityFormatted': true,
            'externalNote': true,
            'internalNote': true
        }, 0);
        await timeReportPage.closeRegisterTimeModal();
        await timeReportPage.selectFirstRow();
        await timeReportPage.deleteSelectedTimeEntries();
        await timeReportPage.applyFilters({ "Order": capturedOrderNumber });
        await timeReportPage.VerifyRowCount(0);
    });
});