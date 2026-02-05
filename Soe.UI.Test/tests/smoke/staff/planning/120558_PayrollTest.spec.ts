import { test } from '../../../fixtures/staff-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../../utils/properties';
import { EmployeeUtil } from '../../../../apis/utils/EmployeeUtil';

let testCaseId = '120558';
let envUrl: string;
let employeeUtil: EmployeeUtil;
let employeeExists: boolean = false;

test.beforeEach(async ({ page, accountEx, staffBasePage, employeesPage }) => {
    await allure.parentSuite("Staff");
    await allure.suite("Planning");
    await allure.subSuite("Planning");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    employeeUtil = new EmployeeUtil(page, envUrl);
    const employees = await employeeUtil.getEmployees();
    employeeExists = employees.some((emp: any) => emp.employeeNr === testCaseId);

    if (employeeExists) {
        console.log(`Employee with number ${testCaseId} already exists`);
    } else {
        console.log(`Employee with number ${testCaseId} does not exist`);
        await employeesPage.goToMenu('Employee', 'Employees');
        await employeesPage.waitForPageLoad();
        await employeesPage.createEmployee();
        await employeesPage.setDateOneMonthBefore();
        await employeesPage.setEndDate();
        await employeesPage.clickOk();
        await employeesPage.setEmploymentPosition(getEnvironmentValue('default_smoke_employmentPosition')?.toString() ?? '');
        await employeesPage.setSalaryAgreement(getEnvironmentValue('default_smoke_salaryAgreement')?.toString() ?? '');
        await employeesPage.setTimeAgreement(getEnvironmentValue('default_smoke_timeAgreement')?.toString() ?? '');
        await employeesPage.enterWeeklyHours('38:15');
        await employeesPage.verifyEmploymentRate('100,00');
        await employeesPage.setIndustryExperience('12');
        await employeesPage.clickOk();
        await employeesPage.setFirstName(testCaseId);
        await employeesPage.setLastName('Pw_PayrollEmployee');
        await employeesPage.setSocialSecurityNumber('');
        await employeesPage.expandContactDetails();
        await employeesPage.addContactDetails({
            email: testCaseId + '_' + Math.random().toString(36).substring(2, 7) + '@gmail.com',
        });
        await employeesPage.expandBankAccounts();
        await employeesPage.setPaymentMethod(getEnvironmentValue('default_smoke_paymentMethod')?.toString() ?? '');
        await employeesPage.expandEmploymentData();
        await employeesPage.setEmployeeId(testCaseId);
        await employeesPage.expandCategoriesTab();
        await employeesPage.filterfirstTwoCategories();
        await employeesPage.saveEmployee();
    }
});

test(testCaseId + 'Planning Payroll : DK', { tag: ['@Staff', '@Planning', '@Smoke'] }, async ({ staffBasePage, basicSchedulePage }) => {
    await staffBasePage.goToMenu('Planning', 'Basic schedule');
    await basicSchedulePage.waitforPageLoad();
    await basicSchedulePage.chooseFilterAndEmployee(testCaseId);
    await basicSchedulePage.clickViewAll();
    await basicSchedulePage.page.waitForTimeout(2000);
    let attempts = 0;
    while (await basicSchedulePage.isScheduleActivated() && attempts < 3) {
        await basicSchedulePage.deactivateSchedule();
        attempts++;
        await basicSchedulePage.page.waitForTimeout(3000);
    }
    await basicSchedulePage.removeBasicScheduleIfPresent();
    await basicSchedulePage.addNewBasicSchedule();
    await basicSchedulePage.activateBasicSchedulewithEndDate();
    await basicSchedulePage.verifyScheduleActivated();
});