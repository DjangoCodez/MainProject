import { expect, test } from "../../fixtures/staff-fixture";
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { EmployeeUtil } from 'apis/utils/EmployeeUtil';
import { generateSocialSecNumber, getFormattedDateMMDDYY } from "utils/CommonUtil";


const testCaseId: string = '122535';
let employeeUtil: EmployeeUtil

const scheduleStartDate = new Date(Date.UTC(new Date().getUTCFullYear(), new Date().getUTCMonth(), new Date().getUTCDate() + ((8 - new Date().getUTCDay()) % 7 || 7)))
const timeTemplate = {
    startDate: scheduleStartDate.toISOString(),
    endDate: new Date(scheduleStartDate.getTime() + 27 * 86400000).toISOString() // 28 days schedule
};

const timeTemplateData = {
    name: `${testCaseId}_Time_Schedule`,
    startDate: timeTemplate.startDate,
    firstMondayOfCycle: timeTemplate.startDate,
    timeCodeId: 150,
    shiftTypeId: 113,
    noOfDays: 7
}

const ssn = generateSocialSecNumber({ gender: 'M' })
const firstName1 = Array.from({ length: 5 }, () => String.fromCharCode(97 + Math.floor(Math.random() * 26))).join('') + Date.now();
const lastName1 = Array.from({ length: 5 }, () => String.fromCharCode(97 + Math.floor(Math.random() * 26))).join('') + Date.now();
const employeeData = {
    actorCompanyId: 1226,
    salaryAgreement: "HAO Månadslön 18 år",
    employmentTypeName: "Permanent Position",
    employeeGroupName: `Order - Lön (månad)`,
    firstName: firstName1,
    lastName: lastName1,
    fullName: `${firstName1} ${lastName1}`,
    ssn: ssn,
    timeCodeId: timeTemplateData.timeCodeId
}

const ssn2 = generateSocialSecNumber({ gender: 'F' })
const firstName2 = Array.from({ length: 5 }, () => String.fromCharCode(97 + Math.floor(Math.random() * 26))).join('') + Date.now();
const lastName2 = Array.from({ length: 5 }, () => String.fromCharCode(97 + Math.floor(Math.random() * 26))).join('') + Date.now();
const employeeData1 = {
    actorCompanyId: 1226,
    salaryAgreement: "HAO Månadslön 18 år",
    employmentTypeName: "Permanent Position",
    employeeGroupName: `Order - Lön (månad)`,
    firstName: firstName2,
    lastName: lastName2,
    fullName: `${firstName2} ${lastName2}`,
    ssn: ssn2,
    timeCodeId: timeTemplateData.timeCodeId
}

const modifiedDate = new Date(timeTemplate.startDate.substring(0, 10))
const startDate = getFormattedDateMMDDYY(modifiedDate);
const sourceShift1 = [
    {
        date: `${getFormattedDateMMDDYY(modifiedDate)}`,
        shiftTime: '8:00 AM-4:00 PM',
        shiftType: 'Arbetstid'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 1 * 86400000))}`,
        shiftTime: '8:00 AM-12:00 PM',
        shiftType: 'SeleniumOrderType'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 1 * 86400000))}`,
        shiftTime: '12:00 PM-4:00 PM',
        shiftType: 'Playwright BookingType'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 2 * 86400000))}`,
        shiftTime: '8:00 AM-10:30 AM',
        shiftType: 'SeleniumOrderType'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 2 * 86400000))}`,
        shiftTime: '10:30 AM-2:00 PM',
        shiftType: 'Playwright BookingType'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 2 * 86400000))}`,
        shiftTime: '2:00 PM-4:00 PM',
        shiftType: 'Playwright BookingType'
    }
]
const sourceShift2 = [
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 8 * 86400000))}`,
        shiftTime: '8:00 AM-4:00 PM',
        shiftType: 'Arbetstid'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 9 * 86400000))}`,
        shiftTime: '8:00 AM-12:00 PM',
        shiftType: 'SeleniumOrderType'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 9 * 86400000))}`,
        shiftTime: '12:00 PM-4:00 PM',
        shiftType: 'Playwright BookingType'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 10 * 86400000))}`,
        shiftTime: '8:00 AM-10:30 AM',
        shiftType: 'SeleniumOrderType'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 10 * 86400000))}`,
        shiftTime: '10:30 AM-2:00 PM',
        shiftType: 'Playwright BookingType'
    },
    {
        date: `${getFormattedDateMMDDYY(new Date(modifiedDate.getTime() + 10 * 86400000))}`,
        shiftTime: '2:00 PM-4:00 PM',
        shiftType: 'Playwright BookingType'
    }
]
const sourceShift3 = [
    {
        date: '',
        shiftTime: '8:00 AM-4:00 PM',
        shiftType: 'Arbetstid'
    }
]

test.beforeEach(async ({ accountEx, staffBasePage }) => {
    await allure.parentSuite("Staff");
    await allure.suite("Planning");
    await allure.subSuite("Planning");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test_Case : " + testCaseId);
    let envUrl = accountEx.baseUrl;
    employeeUtil = new EmployeeUtil(staffBasePage.page, envUrl);
    await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');

    await employeeUtil.createEmployee(employeeData)
    await employeeUtil.createEmployee(employeeData1)
});

test(testCaseId + ': Plannign Basic Schedule : DS', { tag: ['@Staff', '@Planning', '@Regression'] }, async ({ employeesPage, basicSchedulePage }) => {
    await employeesPage.goToMenu('Planning', 'Basic Schedule');
    await basicSchedulePage.waitforPageLoad();
    // Set the schedule start date
    await basicSchedulePage.setScheduleStartDate(startDate);
    // Select the created employees
    await basicSchedulePage.filterMultiEmployees([employeeData.fullName, employeeData1.fullName]);
    await basicSchedulePage.viewAllEmployees(2);
    const employeeNr1 = await basicSchedulePage.getEmpoyeeIds(employeeData.fullName) ?? '';
    const employeeNr2 = await basicSchedulePage.getEmpoyeeIds(employeeData1.fullName) ?? '';
    // Edit and select skills for the first employee
    await basicSchedulePage.editEmployee({ index: 0 });
    await basicSchedulePage.selectSkills({ name: employeeData.fullName, selectAll: true });
    // Edit and select skills for the second employee
    await basicSchedulePage.editEmployee({ index: 1 });
    await basicSchedulePage.selectSkills({ name: employeeData1.fullName, selectAll: true });
    // Setup basic schedule for the  first employee
    await basicSchedulePage.setupEmployeeSchedule(employeeNr1, 1, "Monday", "New basic schedule");
    await basicSchedulePage.addBasicSchedule({ weeks: 4, startDate: getFormattedDateMMDDYY(new Date(timeTemplate.startDate.substring(0, 10))) });
    // Setup and activate schedule for the first employee
    await basicSchedulePage.setupEmployeeSchedule(employeeNr1, 1, "Monday", "Activate");
    await basicSchedulePage.activateSchedule({ endDate: getFormattedDateMMDDYY(new Date(timeTemplate.endDate.substring(0, 10))) }) // need to complete
    // Add new shift with break for the first employee
    await basicSchedulePage.setupEmployeeSchedule(employeeNr1, 1, "Monday", "New shift");
    await basicSchedulePage.addNewShift({ shiftFrom: "08:00", shiftTo: "16:00", shiftType: "Arbetstid", breakFrom: "11:30", breakTo: "12:30" });
    // Add another new shift for the first employee on Tuesday
    await basicSchedulePage.setupEmployeeSchedule(employeeNr1, 1, "Tuesday", "New shift");
    await basicSchedulePage.addNewShift(
        { shiftFrom: "08:00", shiftTo: "12:00", shiftType: "SeleniumOrderType", breakFrom: "12:00", breakTo: "13:00" },
        { shiftFrom: "12:00", shiftTo: "16:00", shiftType: "Playwright BookingType", isNewShift: true });
    await basicSchedulePage.setupEmployeeSchedule(employeeNr1, 1, "Wednesday", "New shift");
    await basicSchedulePage.addNewShift(
        { shiftFrom: "08:00", shiftTo: "10:30", shiftType: "SeleniumOrderType" },
        { shiftFrom: "10:30", shiftTo: "13:00", shiftType: "Playwright BookingType", isNewShift: true },
        { shiftFrom: "14:00", shiftTo: "16:00", shiftType: "Playwright BookingType", isNewShift: true, breakFrom: "13:00", breakTo: "14:00", fillGapsWithBreaks: true });
    await basicSchedulePage.changeVisiblePeriod('Four weeks');
    // Copy shifts of first employee by drag and drop
    const copyShift = await basicSchedulePage.copyMoveEmployeeShiftsByDragDrop({
        empId: employeeNr1,
        source: { from: { week: 1, date: "Monday" }, to: { week: 1, date: "Wednesday" } },
        target: { to: { week: 2, date: "Tuesday" } },
        action: 'copy',
        expectedCount: 12
    });
    // Copy shifts of first employee by drag and drop
    const copyShift2 = await basicSchedulePage.copyMoveEmployeeShiftsByDragDrop({
        empId: employeeNr1,
        source: { from: { week: 2, date: "Tuesday" }, to: { week: 3, date: "Thursday" } },
        target: { to: { week: 3, date: "Wednesday" } },
        action: 'copy',
        expectedCount: 18
    });
    // Copy shifts of first employee by drag and drop
    const copyShift3 = await basicSchedulePage.copyMoveEmployeeShiftsByDragDrop({
        empId: employeeNr1,
        source: { from: { week: 3, date: "Wednesday" }, to: { week: 3, date: "Wednesday" } },
        target: { to: { week: 4, date: "Saturday" } },
        action: 'copy',
        expectedCount: 19
    });
    expect(copyShift).toEqual(sourceShift1);
    expect(copyShift2).toEqual(sourceShift2);
    expect(copyShift3).toEqual(sourceShift3);
}); 