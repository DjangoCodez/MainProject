import { test } from "../../fixtures/staff-fixture";
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { getCurrentDateUtilWithFormat } from '../../../utils/CommonUtil';

let testCaseId: string = '115867';
let envUrl: string;

test.beforeEach(async ({ accountEx, staffBasePage }) => {
  await allure.parentSuite("Staff");
  await allure.suite("Employee");
  await allure.subSuite("Employee");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Create_SubstituteEmployee : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Substitute Employee Create : DK', { tag: ['@Staff', '@Employee', '@Regression'] }, async ({ employeesPage, page }) => {
  {
    await employeesPage.goToMenu('Employee', 'Employees');
    await employeesPage.waitForPageLoad();
    await employeesPage.createEmployee();
    const currentDate = await getCurrentDateUtilWithFormat();
    await employeesPage.setTodaysDate(currentDate);
    await employeesPage.setEndDate();
    await employeesPage.clickOk();
    await employeesPage.setEmploymentPosition(getEnvironmentValue('default_employmentPosition_Substitute')?.toString() ?? '');
    await employeesPage.setSalaryAgreement(getEnvironmentValue('default_salaryAgreement')?.toString() ?? '');
    await employeesPage.setTimeAgreement(getEnvironmentValue('default_timeAgreement')?.toString() ?? '');
    await employeesPage.enterWeeklyHours('5:00');
    await employeesPage.verifyEmploymentRate('13,07');
    await employeesPage.setIndustryExperience('12');
    await employeesPage.enterWorkPlace('Pw_RegTest AB');
    await employeesPage.enterTaskDescription('RegTest Task Description');
    await employeesPage.enterStandingInFor('Pw_Reg employee');
    await employeesPage.enterStandingInDueTo('Illness');
    await employeesPage.clickOk();
    let empFirstName: string = 'SubFirst' + Math.random().toString(36).substring(2, 7);
    await employeesPage.setFirstName(empFirstName);
    let empLastName: string = 'SubLast' + Math.random().toString(36).substring(2, 7);
    await employeesPage.setLastName(empLastName);
    await employeesPage.setSocialSecurityNumber('');
    await employeesPage.expandContactDetails();
    await employeesPage.addContactDetails(
      {
        email: testCaseId + '_' + Math.random().toString(36).substring(2, 7) + '@gmail.com',
        phone: '0707700770',
        nextOfKin: { address: '0123456789', name: 'PlwReg Kin', relationship: 'Brother' }
      });
    await employeesPage.expandBankAccounts();
    await employeesPage.setPaymentMethod(getEnvironmentValue('default_paymentMethod')?.toString() ?? '');
    await employeesPage.expandEmploymentData();
    await employeesPage.verifyEmployeeIdsGenerated();
    await employeesPage.expandUserInfo();
    await employeesPage.verifyUserNameIsSameAsEmployeeId();
    await employeesPage.expandEmpAccountingCode();
    await employeesPage.setCostAccount('1010');
    await page.keyboard.press('Tab');
    await employeesPage.setCostCenter('1');
    await employeesPage.expandCategoriesTab();
    await employeesPage.filterfirstTwoCategories();
    await employeesPage.saveEmployee();
    await employeesPage.editEmployment();
    await employeesPage.clickOk();
    await employeesPage.clickOk();
    await employeesPage.salaryAddRow();
    await employeesPage.setEmployeeFromDate();
    await employeesPage.setSalaryAmount('115,75');
    await employeesPage.clickOk();
    await employeesPage.expandHolidayNotherAbsence();
    await employeesPage.clickHolidayEdit();
    await employeesPage.setEarnedDays('25');
    await employeesPage.setRemainingDays('25');
    await employeesPage.saveEmployee();
    await employeesPage.closeAllTabs();
    await employeesPage.filterByEmpName(empFirstName + ' ' + empLastName, 0);
    await employeesPage.verifyRowCount();
    await employeesPage.clickEdit();
    await employeesPage.expandEmploymentData();
    await employeesPage.printTempEmploymentCertificate();
    const reportName = 'kortare vikariat';
    const reportPath = await employeesPage.openReport(reportName);
    await employeesPage.verifyValueInPdf(reportPath, empFirstName, empLastName);
  }
});