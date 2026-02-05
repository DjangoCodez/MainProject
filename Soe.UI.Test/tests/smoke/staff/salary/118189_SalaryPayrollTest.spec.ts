import { test } from '../../../fixtures/staff-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../../utils/properties';
import { EmployeeUtil } from '../../../../apis/utils/EmployeeUtil';

let testCaseId: string = '118189';
let envUrl: string;
let employeeUtil: EmployeeUtil;
let employeeExists: boolean = false;

test.beforeEach(async ({ page, accountEx, staffBasePage, employeesPage }) => {
    await allure.parentSuite("Staff");
    await allure.suite("Salary");
    await allure.subSuite("Salary calculation");
    envUrl = accountEx.baseUrl;
    await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin')

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

test.describe('Salary Payroll and Report : DK', () => {

    test('118189: Salary Payroll : DK', { tag: ['@Staff', '@Salary', '@Smoke'] }, async ({ staffBasePage, salaryPayrollPage }) => {
        await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/118189', 'Test case : 118189');
        await staffBasePage.goToMenu('Salary', 'Salary calculation');
        await salaryPayrollPage.waitforPageLoad();
        await salaryPayrollPage.setPeriodSet('Tid');
        await salaryPayrollPage.setPaymentDate();
        await salaryPayrollPage.searchEmployee('118189', '118189, Pw_PayrollEmployee');
        await salaryPayrollPage.editEmpTaxYear('118189 118189');
        await salaryPayrollPage.selectCalculate();
        await salaryPayrollPage.VerifySalaryIsCalculated('11100, Månadslön');
        await salaryPayrollPage.markAsAttesterad();
        await salaryPayrollPage.markAsLöneberäknad();
    });

    test('118182: Report PaySlip : DK', { tag: ['@Staff', '@Salary', '@Smoke'] }, async ({ staffBasePage, salaryPayrollPage }) => {
        await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/118182', 'Test case : 118182');
        await salaryPayrollPage.goToReportAnalysis();
        await salaryPayrollPage.goToReportsTab();
        await salaryPayrollPage.searchReport('Lön - lönespecifikation XE');
        await salaryPayrollPage.setPeriod();
        await salaryPayrollPage.filterEmployee('118189', '118189, Pw_PayrollEmployee');
        await salaryPayrollPage.generatePDFReport();
        const report = await salaryPayrollPage.openReport();
        await salaryPayrollPage.verifyValueInPdf(report, '118189 Pw_PayrollEmployee', 'Preliminary payment', '11100Månadslön');
        
        await staffBasePage.goToMenu('Salary', 'Salary calculation');
        await salaryPayrollPage.waitforPageLoad();
        await salaryPayrollPage.setPeriodSet('Tid');
        await salaryPayrollPage.setPaymentDate();
        await salaryPayrollPage.searchEmployee('118189', '118189, Pw_PayrollEmployee');
        await salaryPayrollPage.clearCalculation();
    });
});
