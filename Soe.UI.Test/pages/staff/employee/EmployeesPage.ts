import { expect, Page } from "@playwright/test";
import { GridPageJS } from "../../common/GridPageJS";
import * as allure from "allure-js-commons";
import { SectionGridPageJS } from "../../common/SectionGridPageJS";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";
import { getValueFromPDF, generateSocialSecNumber, extractWithPdfReader } from "utils/CommonUtil";
import { StaffBasePage } from "../StaffBasePage";
import { randomUUID } from "crypto";

export class EmployeesPage extends StaffBasePage {
  readonly employeeGrid: GridPageJS;
  readonly codingSettingsGrid: SectionGridPageJS;
  readonly categoriesGrid: SectionGridPageJS;

  constructor(page: Page) {
    super(page);
    this.employeeGrid = new GridPageJS(page, "ctrl.gridAg.options.gridOptions");
    this.codingSettingsGrid = new SingleGridPageJS(page, 3, 'ctrl.gridAg.options.gridOptions');
    this.categoriesGrid = new SectionGridPageJS(page, 'time.employee.employee.categoryattest', 'ctrl.soeGridOptions.gridOptions');
  }

  async waitForPageLoad() {
    await allure.step("Wait for Product Groups page to load", async () => {
      await this.waitForDataLoad('/api/Time/Employee/EmployeeForGrid/?date=');
    });
  }

  async createEmployee() {
    await allure.step("Click on New Employee button", async () => {
      await this.createItem();
      await this.waitForDataLoad('/api/Time/Payroll/PayrollGroup/Reports/', 15000);
    });
  }

  async setStartDate() {
    await allure.step("Set Employee start Date in 2 weeks", async () => {
      const setDate = new Date();
      setDate.setDate(setDate.getDate() + 14);
      const formattedDate = `${String(setDate.getMonth() + 1).padStart(2, '0')}/${String(setDate.getDate()).padStart(2, '0')}/${setDate.getFullYear()}`;
      const dateInput = this.page.getByRole('textbox', { name: 'Start Date' });
      await dateInput.click();
      await dateInput.fill(formattedDate);
      await this.page.keyboard.press('Enter');
    });
  }

  async setDateOneMonthBefore() {
    await allure.step("Set date one month before", async () => {
      const setDate = new Date();
      setDate.setMonth(setDate.getMonth() - 1);
      const formattedDate = `${String(setDate.getMonth() + 1).padStart(2, '0')}/${String(setDate.getDate()).padStart(2, '0')}/${setDate.getFullYear()}`;
      const dateInput = this.page.getByRole('textbox', { name: 'Start Date' });
      await dateInput.click();
      await dateInput.fill(formattedDate);
      await this.page.keyboard.press('Enter');
    });
  }

  async setTodaysDate(date: string) {
    await allure.step(`Set employee start date as today Date: ${date}`, async () => {
      const startDateInput = this.page.getByRole('textbox', { name: 'Start Date' })
      await startDateInput.click();
      await startDateInput.fill(date);
      await this.page.keyboard.press('Enter');
    });
  }

  async setEndDate() {
    await allure.step("Set Employee End Date in 1 month", async () => {
      const setDate = new Date();
      setDate.setMonth(setDate.getMonth() + 1);
      const formattedDate = `${String(setDate.getMonth() + 1).padStart(2, '0')}/${String(setDate.getDate()).padStart(2, '0')}/${setDate.getFullYear()}`;
      const dateInput = this.page.getByRole('textbox', { name: 'End Date' });
      await dateInput.click();
      await dateInput.fill(formattedDate);
      await this.page.keyboard.press('Enter');
    });
  }

  async setEmpStartDate() {
    await allure.step("Set Employee start Date as 1st date of last month", async () => {
      const setDate = new Date();
      setDate.setMonth(setDate.getMonth() - 1);
      setDate.setDate(1);
      const formattedDate = `${String(setDate.getMonth() + 1).padStart(2, '0')}/${String(setDate.getDate()).padStart(2, '0')}/${setDate.getFullYear()}`;
      const dateInput = this.page.getByRole('textbox', { name: 'Start Date' });
      await dateInput.click();
      await dateInput.fill(formattedDate);
      await this.page.keyboard.press('Enter');
    });
  }


  async clickOk() {
    await allure.step("Click ok button", async () => {
      const okButton = this.page.getByRole('button', { name: 'OK' });
      await okButton.waitFor({ state: 'visible' });
      await okButton.click();
      await this.page.waitForTimeout(500);
    });
  }

  async setEmploymentPosition(empPosition: string) {
    await allure.step("set employment position " + empPosition, async () => {
      await this.page.locator('#ctrl_selectedEmploymentType')
        .selectOption({ label: empPosition });
    });
  }

  async setSalaryAgreement(salaryAgreement: string) {
    await allure.step("set salary agreement " + salaryAgreement, async () => {
      await this.page.getByLabel('Salary agreement', { exact: true })
        .selectOption({ label: salaryAgreement });
    });
  }

  async setTimeAgreement(timeAgreement: string) {
    await allure.step("set time agreement " + timeAgreement, async () => {
      await this.page.getByLabel('Time agreement', { exact: true })
        .selectOption({ label: timeAgreement });
    });
  }

  async enterWeeklyHours(weeklyHours: string) {
    await allure.step(`Enter weekly hours: ${weeklyHours}`, async () => {
      const weeklyHoursInput = this.page.getByRole('textbox', { name: 'Weekly working hours', exact: true });
      await weeklyHoursInput.click();
      await weeklyHoursInput.fill('');
      await weeklyHoursInput.fill(weeklyHours);
      await this.page.keyboard.press('Tab');
      await this.page.waitForTimeout(500);
    });
  }

  async verifyEmploymentRate(expectedRate: string) {
    await allure.step(`Verify Employment Rate: `, async () => {
      const employmentRateInput = this.page.getByRole('textbox', { name: 'Employment Rate' });
      const actualRate = await employmentRateInput.inputValue();
      expect(actualRate).toBe(expectedRate);
    });
  }

  async setIndustryExperience(industryExperience: string) {
    await allure.step("set industry experience " + industryExperience, async () => {
      await this.page.getByRole('textbox', { name: 'Industry experience (months)' }).fill(industryExperience);
    });
  }

  async enterWorkPlace(workPlace: string) {
    await allure.step("Enter work place " + workPlace, async () => {
      const workPlaceInput = this.page.getByRole('textbox', { name: 'Workplace' });
      await workPlaceInput.fill(workPlace);
    });
  }

  async enterTaskDescription(taskDescription: string) {
    await allure.step("Enter task description " + taskDescription, async () => {
      const taskDescriptionInput = this.page.getByRole('textbox', { name: 'Tasks' });
      await taskDescriptionInput.fill(taskDescription);
    });
  }

  async enterStandingInFor(standingInFor: string) {
    await allure.step("Enter Standing In For " + standingInFor, async () => {
      const standingInForInput = this.page.getByRole('textbox', { name: 'Standing in for' })
      await standingInForInput.fill(standingInFor);
    });
  }

  async enterStandingInDueTo(standingInDueTo: string) {
    await allure.step("Enter Standing In Due To " + standingInDueTo, async () => {
      const standingInDueToInput = this.page.getByRole('textbox', { name: 'Standing in due to' })
      await standingInDueToInput.fill(standingInDueTo);
    });
  }

  async setFirstName(firstName: string) {
    await allure.step("Enter first name : " + firstName, async () => {
      await this.page.getByRole('textbox', { name: 'First Name' }).fill(firstName);
    });
  }

  async setLastName(lastName: string) {
    await allure.step("Enter last name : " + lastName, async () => {
      await this.page.getByRole('textbox', { name: 'Last Name' }).fill(lastName);
    });
  }

  async setSocialSecurityNumber(socialSecurityNumber: string) {
    await allure.step("Enter social security number : " + socialSecurityNumber, async () => {
      const socialSeqNo = generateSocialSecNumber();
      await this.page.getByRole('textbox', { name: 'Social Security Number' }).fill(socialSeqNo);
    });
  }

  async expandContactDetails() {
    await allure.step("Expand Contact Details tab", async () => {
      await this.page.getByRole('button', { name: 'Contact Details' }).click();
    });
  }

  async addContactDetails(details: { email?: string; phone?: string; nextOfKin?: { address: string; name: string; relationship: string } }, testCaseId?: string) {
    await allure.step("Add contact details", async () => {

      if (details.email || testCaseId) {
        await this.page.getByRole('button', { name: 'Add Contact' }).click();
        const addEmailAddress = this.page.getByRole('link', { name: 'Email address' });
        await addEmailAddress.waitFor({ state: 'visible' });
        await this.page.waitForTimeout(1000);
        await addEmailAddress.click();
        await this.page.getByRole('textbox', { name: 'Email address' }).click();
        await this.page.waitForTimeout(1000);

        const randomId = Math.floor(Math.random() * 10000);
        const email = details.email || `${randomId}_${testCaseId || ''}@gmail.com`;
        await this.page.getByRole('textbox', { name: 'Email address' }).fill(email);
        await this.page.getByRole('button', { name: 'Select' }).click();
      }

      if (details.phone) {
        await this.page.getByRole('button', { name: 'Add Contact' }).click();
        const addMobileNumber = this.page.getByRole('link', { name: 'Mobile phone' });
        await addMobileNumber.waitFor({ state: 'visible', timeout: 10000 });
        await addMobileNumber.click();
        await this.page.getByRole('textbox', { name: 'Address / Number' }).fill(details.phone);
        await this.page.getByRole('button', { name: 'Select' }).click();
      }

      if (details.nextOfKin) {
        await this.page.getByRole('button', { name: 'Add Contact' }).click();
        await this.page.getByRole('link', { name: 'Next of kin' }).click();
        await this.page.getByRole('textbox', { name: 'Address / Number' }).fill(details.nextOfKin.address);
        await this.page.getByRole('textbox', { name: 'Name' }).fill(details.nextOfKin.name);
        await this.page.getByRole('textbox', { name: 'Relationship' }).fill(details.nextOfKin.relationship);
        await this.page.getByRole('button', { name: 'Select' }).click();
      }
    });
  }

  async expandBankAccounts() {
    await allure.step("Expand Bank Accounts tab", async () => {
      await this.page.getByRole('button', { name: 'Bank Accounts' }).click();
    });
  }

  async setPaymentMethod(paymentMethod?: string) {
    await allure.step("Set Payment Method", async () => {
      const paymentMethodSelect = this.page.getByLabel('Payment Method');
      await paymentMethodSelect.selectOption({ label: paymentMethod });
    });
  }

  async expandEmploymentData() {
    await allure.step("Expand Employment Data tab", async () => {
      await this.page.getByRole('button', { name: 'Employment Data' }).click();
    });
  }

  async verifyEmployeeIdsGenerated() {
    await allure.step("Verify Employee Id is generated automatically", async () => {
      const employeeIdInput = this.page.getByRole('textbox', { name: 'Employee ID' });
      const employeeId = await employeeIdInput.inputValue();
      expect(employeeId).not.toBe('');
    });
  }

  async setSmokeEmployeeId(empId: string) {
    await allure.step("Set Employee Id : " + empId, async () => {
      const employeeIdInput = this.page.getByRole('textbox', { name: 'Employee ID' });
      await employeeIdInput.fill(empId);
    });
  }
  async setEmployeeId(testCaseId: string) {
    await allure.step(`Set Employee ID: ${testCaseId}`, async () => {
      const employeeIdInput = this.page.getByRole('textbox', { name: 'Employee ID' });
      await employeeIdInput.click();
      await employeeIdInput.fill(testCaseId);
      await this.page.keyboard.press('Enter');
    });
  }

  async expandUserInfo() {
    await allure.step("Expand User Info tab", async () => {
      await this.page.getByRole('button', { name: 'User Information' }).click();
      await this.page.waitForTimeout(1000);
    });
  }

  async verifyUserNameIsSameAsEmployeeId() {
    await allure.step("Verify User Name is same as Employee ID", async () => {
      const employeeIdInput = this.page.getByRole('textbox', { name: 'Employee ID' });
      const employeeId = await employeeIdInput.inputValue();
      await this.page.getByRole('columnheader', { name: '' }).locator('i').click();
      const userNameInput = this.page.getByRole('textbox', { name: 'Username' });
      await expect(userNameInput).toHaveValue(/.+/);
      const userName = await userNameInput.inputValue();
      await this.page.getByRole('button', { name: 'Cancel' }).click();
      expect(userName).toBe(employeeId);
    });
  }

  async expandEmpAccountingCode() {
    await allure.step("Expand Employee Accounting Code tab", async () => {
      await this.page.getByRole('button', { name: 'Employment, salary and' }).click();
    });
  }

  async setCostAccount(accountNumber: string) {
    await allure.step(`Add Cost Account: ${accountNumber}`, async () => {
      await this.codingSettingsGrid.enterDropDownValueGrid('account1Nr', accountNumber, 0);
      await this.page.waitForTimeout(1000);
      await this.page.keyboard.press('Tab');
    });
  }

  async setCostCenter(costCenter: string) {
    await allure.step(`Add Cost Center: ${costCenter}`, async () => {
      await this.codingSettingsGrid.enterDropDownValueGrid('account2Nr', costCenter, 0);
    });
  }

  async expandCategoriesTab() {
    await allure.step("Expand Categories tab", async () => {
      await this.page.getByRole('button', { name: 'Categories and attest' }).click();
    });
  }

  async filterfirstTwoCategories() {
    await allure.step(`Filter and select first two categories:`, async () => {
      await this.categoriesGrid.selectSubGridCheckBox(0);
      await this.categoriesGrid.selectSubGridCheckBox(2);
    });
  }

  async expandHolidayNotherAbsence() {
    await allure.step("Expand Holiday and other absence tab", async () => {
      await this.page.getByRole('button', { name: 'Holiday and other absence' }).click();
    });
  }

  async clickHolidayEdit() {
    await allure.step("Click Holiday Edit button", async () => {
      await this.page.getByRole('button', { name: 'Edit' }).click();
    });
  }

  async setEarnedDays(earnedDays: string) {
    await allure.step(`Set Earned Days`, async () => {
      const earnedDaysInput = this.page.locator('#ctrl_employeeVacation_earnedDaysPaid');
      await earnedDaysInput.fill(earnedDays);
    });
  }

  async setRemainingDays(remainingDays: string) {
    await allure.step(`Set Remaining Days`, async () => {
      const remainingDaysInput = this.page.locator('#ctrl_employeeVacation_remainingDaysPaid');
      await remainingDaysInput.fill(remainingDays);
    });
  }

  async clickParentalLeaveAddRow() {
    await allure.step("Click Add Row button", async () => {
      await this.page.getByLabel('Holiday and other absence').getByText('Add Row').nth(1).click();
      await this.page.waitForTimeout(500);
    });
  }

  async addChildFirstName(childFirstName: string) {
    await allure.step(`Add Child First Name `, async () => {
      await this.page.getByRole('textbox', { name: 'First Name' }).fill(childFirstName);
    });
  }

  async addChildLastName(childLastName: string) {
    await allure.step(`Add Child Last Name `, async () => {
      await this.page.getByRole('textbox', { name: 'Last Name' }).fill(childLastName);
    });
  }

  async setChildDOB() {
    await allure.step("Set Child Date of birth 3 months before", async () => {
      const setDate = new Date();
      setDate.setDate(setDate.getDate() - 90);
      const formattedDate = `${String(setDate.getMonth() + 1).padStart(2, '0')}/${String(setDate.getDate()).padStart(2, '0')}/${setDate.getFullYear()}`;
      const dateInput = this.page.getByRole('textbox', { name: 'Date Of Birth' });
      await dateInput.click();
      await dateInput.fill(formattedDate);
      await this.page.keyboard.press('Enter');
    });
  }

  async expandHrTab() {
    await allure.step("Expand HR tab", async () => {
      const hrTab = this.page.getByRole('button', { name: /^HR(?!\s*follow-up)/i });
      await hrTab.click();

    });
  }

  async expandSkillsTab() {
    await allure.step("Expand Skills tab", async () => {
      await this.page.getByRole('button', { name: 'Skill' }).click();
    });
  }

  async selectSkills() {
    await allure.step("Select Skills", async () => {
      await this.page.getByRole('tabpanel', { name: 'Select skills ' }).getByLabel('Select All').check();
    });
  }

  async saveEmployee() {
    await allure.step("Save Account Payable Settings", async () => {
      const save = this.page.getByRole('button', { name: 'Save' });
      await save.waitFor({ state: 'visible' });
      await save.click();
      await this.waitForDataLoad('/api/Time/Employee/EmployeeForGrid/?showInactive=false');
      await this.page.waitForTimeout(2000);
    });
  }

  async editEmployment() {
    await allure.step("Edit Employment", async () => {
      const editButton = this.page.locator("//span[@title='Change employment']//i[contains(@class, 'fa-pencil')]");
      editButton.waitFor({ state: 'visible' });
      editButton.click();
      await this.page.waitForTimeout(1500);
    });
  }

  async setEmployeeFromDate() {
    await allure.step("Set Employee start Date in 2 weeks", async () => {
      const setDate = new Date();
      setDate.setDate(setDate.getDate() + 14);
      const formattedDate = `${String(setDate.getMonth() + 1).padStart(2, '0')}/${String(setDate.getDate()).padStart(2, '0')}/${setDate.getFullYear()}`;
      const dateInput = this.page.getByRole('textbox', { name: 'From' });
      await dateInput.waitFor({ state: 'visible', timeout: 20000 });
      await dateInput.fill(formattedDate);
      await this.page.keyboard.press('Enter');
    });
  }

  async expandSalaryAndCoding() {
    await allure.step("Expand Salary and coding tab", async () => {
      await this.page.getByRole('button', { name: 'Employment, salary and' }).click();
    });
  }

  async salaryAddRow() {
    await allure.step("Click Add Row button in Salary and coding", async () => {
      await this.page.getByLabel('Employment, salary and').getByText('Add Row').first().click();
      await this.page.waitForTimeout(500);
    });
  }

  async setSalaryAmount(salaryAmount: string) {
    await allure.step(`Set Salary Amount`, async () => {
      const salaryAmountInput = this.page.getByRole('textbox', { name: 'Amount' })
      await salaryAmountInput.fill(salaryAmount);
    });
  }

  async expandTaxAndSocialContributions() {
    await allure.step("Expand Tax and Social Contributions tab", async () => {
      await this.page.getByRole('button', { name: 'Taxes and social' }).click();
    });
  }

  async selectTaxCalculation(taxCalculation: string) {
    await allure.step(`Select Tax Calculation Method: ${taxCalculation}`, async () => {
      const taxCalculationSelect = this.page.getByLabel('Calculation', { exact: true });
      await taxCalculationSelect.selectOption({ label: taxCalculation });
    });
  }

  async setTaxTable(taxAmount: string) {
    await allure.step(`Set Tax Amount`, async () => {
      const taxTableInput = this.page.getByRole('textbox', { name: 'Tax Table' })
      await taxTableInput.fill(taxAmount);
    });
  }

  async closeAllTabs() {
    await allure.step('Close Tab', async () => {
      await this.page.locator('(//a[@ng-click="select($event)"]//i[contains(@class, "removableTabIcon")])[1]').click();
      await this.page.waitForTimeout(1000);
    });
  }

  async filterByEmpName(empName: string, colIndex: number) {
    await allure.step(`Filter by Employee name`, async () => {
      await this.employeeGrid.filterByName('Name', empName, colIndex);
      await this.page.waitForTimeout(1000);
    });
  }

  async verifyRowCount() {
    await allure.step("Verify row count", async () => {
      const rowCount = await this.employeeGrid.getFilteredAgGridRowCount();
      expect(rowCount).toBe(1);
    });
  }

  async clickEdit() {
    await allure.step("go to employment edit page", async () => {
      const editOrderButton = this.page.locator("//button[contains(@class, 'gridCellIcon') and contains(@class, 'fa-pencil') and @title='Edit']");
      await editOrderButton.waitFor({ state: 'visible' });
      await editOrderButton.click();
    });
  }

  async printEmploymentCertificate() {
    await allure.step("Print Employment certificate Employee - Employment certificate retail", async () => {
      await this.page.getByLabel('Employment, salary and').getByRole('cell', { name: '' }).click();
      const printButton = this.page.locator(`(//tr[@data-ng-repeat="report in ctrl.reports | orderBy:['default','reportName']"]//span[contains(@class, 'fa-print')])[1]`);
      printButton.waitFor({ state: 'visible' });
      printButton.click();
      await this.waitForDataLoad('/api/Report/Menu/Queue/[empty]/false', 25000);
      await this.waitForDataLoad(/\/api\/Report\/Menu\/Queue\/\d+\/false$/);
    });
  }

  async printTempEmploymentCertificate() {
    await allure.step("Print Employment certificate Employee - Employment certificate retail", async () => {
      await this.page.getByLabel('Employment, salary and').getByRole('cell', { name: '' }).click();
      const printButton = this.page.locator(`(//tr[@data-ng-repeat="report in ctrl.reports | orderBy:['default','reportName']"]//span[contains(@class, 'fa-print')])[3]`);
      printButton.waitFor({ state: 'visible' });
      printButton.click();
      await this.waitForDataLoad('/api/Report/Menu/Queue/[empty]/false', 25000);
      await this.waitForDataLoad(/\/api\/Report\/Menu\/Queue\/\d+\/false$/);
    });
  }

  async openReport(reportName: string) {
    return await allure.step(`Open Report: ${reportName}`, async () => {
      const pdfIcon = this.page.getByText(reportName).first();
      await pdfIcon.waitFor({ state: 'visible' });
      await pdfIcon.click({ button: 'right' });
      const openReportOption = this.page.locator("//ul[contains(@class,'dropdown-menu') and @role='menu']//a[.//span[contains(.,'Open report')]]");
      await openReportOption.first().waitFor({ state: 'visible' });
      const localPath = `./test-data/temp-download/employee_${randomUUID()}.pdf`
      await this.downloadFile(localPath, openReportOption);
      return localPath;
    });
  }

  async verifyValueInPdf(reportPath: string, empFirstName: string, empLastName: string) {
    await allure.step(`Read Report PDF`, async () => {
      const fulltext = await extractWithPdfReader(reportPath);
      const employeeIdInput = this.page.getByRole('textbox', { name: 'Employee ID' });
      const employeeId = await employeeIdInput.inputValue();
      expect(fulltext.includes(employeeId), `expected value ${employeeId} not found in PDF`).toBe(true);
      expect(fulltext.includes(empFirstName), `expected value ${empFirstName} not found in PDF`).toBe(true);
      expect(fulltext.includes(empLastName), `expected value ${empLastName} not found in PDF`).toBe(true);
      await this.deleteFile(reportPath);
    });
  }

  async expandPersonalData() {
    await allure.step("Expand Personal Data tab", async () => {
      await this.page.getByRole('button', { name: 'Personal data' }).click();
    });
  }

  async clickAttestRoleEdit() {
    await allure.step("Click Attest Role Edit button", async () => {
      await this.page.locator("//thead//i[@data-ng-click='ctrl.openUserRolesDialog();']").click();
      await this.page.waitForTimeout(1500);
      await expect(this.page.getByRole('heading', { name: 'Edit' })).toBeVisible();
    });
  }

  async verifyAttesRole(roleName: string) {
    await allure.step(`Verify ${roleName} role is checked`, async () => {
      const roleCheckbox = this.page.locator(`//table[contains(@class,'table-hover')]//td/span[contains(text(), '${roleName}')]/../../td[2]//span[contains(@class,'fa-check')]`);
      const isChecked = await roleCheckbox.getAttribute('class');
      expect(isChecked).toContain('fa-check-square');
      await this.page.getByRole('button', { name: 'OK' }).click();
    });
  }
}
