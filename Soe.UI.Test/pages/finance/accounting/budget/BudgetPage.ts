import { expect, Page } from "@playwright/test";
import { FinanceBasePage } from "../../FinanceBasePage";
import { GridPage } from "../../../common/GridPage";
import * as allure from "allure-js-commons";
import { AngVersion } from "../../../../enums/AngVersionEnums";

export class BudgetPage extends FinanceBasePage {
  readonly budgetGrid: GridPage;
  readonly ang_version: AngVersion;

  constructor(page: Page, ang_version: AngVersion = AngVersion.NEW) {
    super(page);
    this.budgetGrid = new GridPage(page, "Economy.Accounting.Budget.Budgets");
    this.ang_version = ang_version;
  }

  async setBudgetName(name: string) {
    await allure.step("Enter name : " + name, async () => {
      await this.page.getByTestId('name').fill(name);
    });
  }

  async verifyNumberOfPeriods(periods: string) {
    await allure.step(`Verify Number of Periods: `, async () => {
      const numberOfPeriods = await this.page.getByTestId("noOfPeriods").inputValue();
      expect(numberOfPeriods).toBe(periods)
    });
  }

  async clickYesLoadResults() {
    await allure.step('Click yes for load results pop up', async () => {
      await this.page.getByRole('button', { name: 'Yes' }).click();
    });
  }

  async clickClear() {
    await allure.step("Click clear button", async () => {
      const clearButton = this.page.getByRole('button', { name: 'Clear' });
      await clearButton.waitFor({ state: 'visible' });
      if (await clearButton.isVisible() && await clearButton.isEnabled()) {
        await clearButton.click();
      } else {
        console.log('Clear button is not ready to be clicked.');
      }
    });
  }

  async checkCostCenterCheckbox() {
    await allure.step(`check the Kostnadsställe checkbox`, async () => {
      const checkbox = await this.page.getByTestId('useDim2');
      await checkbox.waitFor({ state: 'visible' });
      await checkbox.click();
      await expect(this.page.getByTestId('useDim2')).toBeChecked();
    });
  }

  async addCostCenter(Kostnadsställe: string) {
    await allure.step('Add standard kostnadsställe cost center', async () => {
      await this.page.getByTestId('dim2Id').locator('select, :scope')
        .selectOption({ label: Kostnadsställe });
    });
  }

  async addStandardDistributionCode(distributionCode: string) {
    await allure.step("Add standard distribution Code " + distributionCode, async () => {
      await this.page.getByTestId('distributionCodeHeadId').locator('select, :scope')
        .selectOption({ label: distributionCode });
    });
  }

  async AddRow() {
    await allure.step("Add a row", async () => {
      await this.page.getByTestId('common.newrow').click();
    });
  }

  async addAccount(accountNumber: string) {
    await allure.step('Add Account', async () => {
      const combo = this.page.getByTestId('Economy.Accounting.Budget.Rows').getByRole('combobox');
      await combo.click();
      await combo.fill(accountNumber);
      await this.page.getByRole('listbox')
        .getByRole('option', { name: new RegExp(`^\\s*${accountNumber}\\b`) })
        .first()
        .click();
    });
  }

  async filterByBudgetName(name: string) {
    await allure.step("Search budget record", async () => {
      await this.budgetGrid.filterByColumnNameAndValue("Name", name);
    });
  }

  async goToEditBudget() {
    await allure.step("go to edit budget", async () => {
      await this.page
        .locator('span[title="Edit"]')
        .nth(0)
        .click({ timeout: 3000 });
    });
  }

  async removeBudget() {
    await allure.step("remove the created Budget", async () => {
      await this.page.getByRole("button", { name: " Remove" }).click();
      await this.page.getByRole("button", { name: "OK" }).click();
    });
  }

  async VerifyRowCount(count: string) {
    await allure.step("Verify Budget filtered count is " + count, async () => {
      await expect(this.page.getByTestId("filtered")).toHaveText("(Filtered " + count + ")");
    });
  }
}