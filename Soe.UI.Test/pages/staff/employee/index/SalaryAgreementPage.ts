import { expect, Page } from "@playwright/test";
import { StaffBasePage } from "pages/staff/StaffBasePage";
import * as allure from "allure-js-commons";
import { GridPageJS } from "pages/common/GridPageJS";

export class SalaryAgreementPage extends StaffBasePage {
  readonly salaryAgreementGrid: GridPageJS;

  constructor(page: Page) {
    super(page);
    this.salaryAgreementGrid = new GridPageJS(page, "ctrl.gridAg.options.gridOptions");
  }

  async waitForPageLoad() {
    await allure.step("Wait for Product Groups page to load", async () => {
      await this.waitForDataLoad('api/Time/Payroll/PayrollGroup', 5000);
    });
  }

  async setName(name: string) {
    await allure.step(`Set salary agreement name`, async () => {
      await this.page.getByRole('textbox', { name: 'Name' }).fill(name);
    });
  }

  async setPeriodSet(periodSet: string) {
    await allure.step("set employment position " + periodSet, async () => {
      await this.page.getByLabel('Period set', { exact: true }).selectOption({ label: periodSet });
    });
  }

  async setSalaryType(salaryType: string) {
    await allure.step(`Set Salary Type : ${salaryType}`, async () => {
      await this.page.getByText('New salary type').click();
      await this.page.waitForTimeout(1000);
      await this.page.getByRole('heading', { name: 'ADD' }).waitFor();
      await this.page.getByLabel('Salary type', { exact: true }).selectOption({ label: salaryType });
      await this.page.getByRole('checkbox', { name: 'Show for employee' }).check();
      await this.page.getByRole('button', { name: 'OK' }).click();
    });
  }

  async setSalaryFormulas(salaryFormulas: string) {
    await allure.step(`Set Salary Formulas : ${salaryFormulas}`, async () => {
      await this.page.getByText('New formula').click();
      await this.page.getByRole('heading', { name: 'Salary formulas' }).waitFor();
      await this.page.getByLabel('Salary formula', { exact: true }).selectOption({ label: salaryFormulas });
      await this.page.getByRole('checkbox', { name: 'Show for employee' }).click();
      await this.page.getByRole('button', { name: 'OK' }).click();
    });
  }

  async setCalculationOneTimeTax(calculationTax: string) {
    await allure.step(`Set Calculation Tax : ${calculationTax}`, async () => {
      await this.page.getByLabel('Calculation of one-time tax').selectOption({ label: calculationTax });
    });
  }

  async setCalculationValidMonths(validMonths: string) {
    await allure.step(`Set Calculation Valid Months : ${validMonths}`, async () => {
      await this.page.getByLabel('Calculation of valid month').selectOption({ label: validMonths });
    });
  }

  async expandSettingsTab() {
    await allure.step("Expand Settings section", async () => {
      await this.page.getByRole('button', { name: 'Settings' }).click();
    });
  }

  async setNewHolidayAgreement(holidayAgreement: string) {
    await allure.step(`Set New Holiday Agreement : ${holidayAgreement}`, async () => {
      await this.page.getByTitle('New holiday agreement').click();
      await this.page.getByRole('heading', { name: 'add' }).waitFor();
      await this.page.waitForTimeout(1000);
      await this.page.getByLabel('Holiday agreement', { exact: true }).selectOption({ label: holidayAgreement });
      await this.page.getByRole('checkbox', { name: 'Standard' }).check();
      await this.page.getByRole('button', { name: 'OK' }).click();
    });
  }

  async expandReportsTab() {
    await allure.step("Expand Reports Tab", async () => {
      await this.page.getByRole('tab', { name: 'Reports' }).click();
    });
  }

  async checkFirstReport(reportName: string) {
    await allure.step(`Check First Report : ${reportName}`, async () => {
      await this.page.locator('.fal.margin-small-left').first().click();
    });
  }

  async expandStatisticsTab() {
    await allure.step("Expand Statistics Tab", async () => {
      await this.page.getByRole('button', { name: 'Statistics' }).click();
    });
  }

  async setSCBInfo(staffCategory: string, workTimeType: string, salType: string, currentSalary: string, AssociationNumber: string, AgreementCode: string) {
    await allure.step(`Set SCB Information`, async () => {
      await this.page.getByLabel('SCB Staff category', { exact: true }).selectOption({ label: staffCategory });
      await this.page.getByLabel('SCB Work time type', { exact: true }).selectOption({ label: workTimeType });
      await this.page.getByLabel('SCB Salary type', { exact: true }).selectOption({ label: salType });
      await this.page.getByLabel('SCB Current salary').selectOption({ label: currentSalary });
      await this.page.getByRole('textbox', { name: 'SCB Association number' }).fill(AssociationNumber);
      await this.page.getByRole('textbox', { name: 'SCB Agreement code' }).fill(AgreementCode);
    });
  }

  async closeAllTabs() {
    await allure.step('Close Tab', async () => {
      await this.page.locator('(//a[@ng-click="select($event)"]//i[contains(@class, "removableTabIcon")])[1]').click();
      await this.page.waitForTimeout(1000);
    });
  }

  async saveSalaryAgreement() {
    await allure.step("Save the  agreement", async () => {
      const save = this.page.getByRole('button', { name: 'Save' });
      await save.waitFor({ state: 'visible' });
      await save.click();
      await this.waitForDataLoad('api/Time/Payroll/PayrollGroup/');
    });
  }

  async filterBySalaryAgreementName(salaryAgreementName: string) {
    await allure.step(`Filter by Salary Agreement Name: ${salaryAgreementName}`, async () => {
      await this.salaryAgreementGrid.filterByName('Name', salaryAgreementName);
    });
  }

  async verifyRowCount() {
    await allure.step("Verify row count", async () => {
      const rowCount = await this.salaryAgreementGrid.getAgGridRowCount();
      expect(rowCount).toBe(1);
    });
  }
}