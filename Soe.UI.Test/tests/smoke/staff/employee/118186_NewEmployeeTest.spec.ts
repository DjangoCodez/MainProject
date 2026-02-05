import { test } from '../../../fixtures/staff-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../../utils/properties';

let testCaseId: string = '118186';
let envUrl: string;

test.beforeEach(async ({ accountEx, staffBasePage }) => {
  await allure.parentSuite("Staff");
  await allure.suite("Employee");
  await allure.subSuite("Employee");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': New Employee : DK', { tag: ['@Staff', '@Planning', '@Smoke'] }, async ({ employeesPage }) => {
  {
    await employeesPage.goToMenu('Employee', 'Employees');
    await employeesPage.waitForPageLoad();
    await employeesPage.createEmployee();
    await employeesPage.setEmpStartDate();
    await employeesPage.clickOk();
    await employeesPage.setEmploymentPosition(getEnvironmentValue('default_smoke_employmentPosition')?.toString() ?? '');
    await employeesPage.setSalaryAgreement(getEnvironmentValue('default_smoke_salaryAgreement')?.toString() ?? '');
    await employeesPage.setTimeAgreement(getEnvironmentValue('default_smoke_timeAgreement')?.toString() ?? '');
    await employeesPage.enterWeeklyHours('38:15');
    await employeesPage.verifyEmploymentRate('100,00');
    await employeesPage.clickOk();
    let smoke: string = 'First' + Math.random().toString(36).substring(2, 7);
    await employeesPage.setFirstName(smoke);
    let employee: string = 'Last' + Math.random().toString(36).substring(2, 7);
    await employeesPage.setLastName(employee);
    await employeesPage.setSocialSecurityNumber('');
    await employeesPage.expandContactDetails();
    await employeesPage.addContactDetails(
      {
        email: testCaseId + '_' + Math.random().toString(36).substring(2, 7) + '@gmail.com',
      });
    await employeesPage.expandBankAccounts();
    await employeesPage.setPaymentMethod(getEnvironmentValue('default_smoke_paymentMethod')?.toString() ?? '');
    await employeesPage.expandEmploymentData();
    const smokeEmpId = 'smk' + Math.floor(10000 + Math.random() * 90000).toString();
    const employeeId = `${smokeEmpId}`;
    await employeesPage.setSmokeEmployeeId(employeeId);
    await employeesPage.expandUserInfo();
    await employeesPage.verifyUserNameIsSameAsEmployeeId();
    await employeesPage.expandCategoriesTab();
    await employeesPage.filterfirstTwoCategories();
    await employeesPage.expandSalaryAndCoding();
    await employeesPage.salaryAddRow();
    await employeesPage.setEmployeeFromDate();
    await employeesPage.setSalaryAmount('50000');
    await employeesPage.clickOk();
    await employeesPage.expandTaxAndSocialContributions();
    await employeesPage.selectTaxCalculation('According to the tax table');
    await employeesPage.setTaxTable('30');
    await employeesPage.saveEmployee();
    await employeesPage.closeAllTabs();
    await employeesPage.filterByEmpName(smoke + ' ' + employee, 0);
    await employeesPage.verifyRowCount();
  }
});