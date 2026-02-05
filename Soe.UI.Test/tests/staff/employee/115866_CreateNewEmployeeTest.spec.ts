import { test } from "../../fixtures/staff-fixture";
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let envUrl: string;
let empFirstName: string;
let empLastName: string;

test.beforeEach(async ({ accountEx, staffBasePage }) => {
  await allure.parentSuite("Staff");
  await allure.suite("Employee");
  await allure.subSuite("Employee");
  envUrl = accountEx.baseUrl;
  await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test.describe('Create Employee and set Attest Role', () => {

  test('115866 : New Employee Create : DK', { tag: ['@Staff', '@Employee', '@Regression'] }, async ({ employeesPage, page }) => {
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/115866', 'Test case : 115866');
    await employeesPage.goToMenu('Employee', 'Employees');
    await employeesPage.waitForPageLoad();
    await employeesPage.createEmployee();
    await employeesPage.setStartDate();
    await employeesPage.clickOk();
    await employeesPage.setEmploymentPosition(getEnvironmentValue('default_employmentPosition')?.toString() ?? '');
    await employeesPage.setSalaryAgreement(getEnvironmentValue('default_salaryAgreement')?.toString() ?? '');
    await employeesPage.setTimeAgreement(getEnvironmentValue('default_timeAgreement')?.toString() ?? '');
    await employeesPage.enterWeeklyHours('20:00');
    await employeesPage.verifyEmploymentRate('52,29');
    await employeesPage.setIndustryExperience('12');
    await employeesPage.enterWorkPlace('Pw_RegTest AB');
    await employeesPage.enterTaskDescription('RegTest Task Description');
    await employeesPage.clickOk();
    empFirstName = 'First' + Math.random().toString(36).substring(2, 7);
    empLastName = 'Last' + Math.random().toString(36).substring(2, 7);
    await employeesPage.setFirstName(empFirstName);
    await employeesPage.setLastName(empLastName);
    await employeesPage.setSocialSecurityNumber('');
    await employeesPage.expandContactDetails();
    await employeesPage.addContactDetails(
      {
        email: 115866 + '_' + Math.random().toString(36).substring(2, 7) + '@gmail.com',
        phone: '0701234567',
        nextOfKin: { address: '123456789', name: 'PlwReg Kin', relationship: 'Sibling' }
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
    await employeesPage.expandHolidayNotherAbsence();
    await employeesPage.clickHolidayEdit();
    await employeesPage.setEarnedDays('25');
    await employeesPage.setRemainingDays('25');
    await employeesPage.clickParentalLeaveAddRow();
    let childFirstName: string = 'ChildFirst' + Math.random().toString(36).substring(2,);
    await employeesPage.addChildFirstName(childFirstName);
    let childLastName: string = 'ChildLast' + Math.random().toString(36).substring(2, 7);
    await employeesPage.addChildLastName(childLastName);
    await employeesPage.setChildDOB();
    await employeesPage.clickOk();
    await employeesPage.expandHrTab();
    await employeesPage.expandSkillsTab();
    await employeesPage.selectSkills();
    await employeesPage.saveEmployee();
    await employeesPage.editEmployment();
    await employeesPage.clickOk();
    await employeesPage.clickOk();
    await employeesPage.salaryAddRow();
    await employeesPage.setEmployeeFromDate();
    await employeesPage.setSalaryAmount('150');
    await employeesPage.clickOk();
    await employeesPage.saveEmployee();
    await employeesPage.closeAllTabs();
    await employeesPage.filterByEmpName(empFirstName + ' ' + empLastName, 0);
    await employeesPage.verifyRowCount();
    await employeesPage.clickEdit();
    await employeesPage.expandEmploymentData();
    await employeesPage.printEmploymentCertificate();
    const reportName = 'Anställningsbevis Anställd - Anställningsbevis detaljhandel';
    const reportPath = await employeesPage.openReport(reportName);
    await employeesPage.verifyValueInPdf(reportPath, empFirstName, empLastName);
    }
  );

  test('115869 : Set Attest Role : DK', { tag: ['@Staff', '@Employee', '@Regression'] }, async ({ employeesPage }) => {
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/115869', 'Test case : 115869');
    await employeesPage.goToMenu('Employee', 'Employees');
    await employeesPage.waitForPageLoad();
    await employeesPage.filterByEmpName(`${empFirstName} ${empLastName}`, 0);
    await employeesPage.verifyRowCount();
    await employeesPage.clickEdit();
    await employeesPage.expandPersonalData();
    await employeesPage.expandUserInfo();
    await employeesPage.clickAttestRoleEdit();
    await employeesPage.verifyAttesRole('Montör');
    }
  );
});