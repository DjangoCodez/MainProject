import { expect, Page } from "@playwright/test";
import { FinanceBasePage } from "../FinanceBasePage";
import * as allure from "allure-js-commons";
import { GridPage } from "../../common/GridPage";
import { getCurrentDateUtilWithFormat } from '../../../utils/CommonUtil';

export class DistributionCodePage extends FinanceBasePage {
  readonly distributionCodeGrid: GridPage;

  constructor(page: Page) {
    super(page);
    this.distributionCodeGrid = new GridPage(page, "Economy.Accounting.DistributionCode");
  }

  async waitForPageLoad() {
    await allure.step("Wait for Product Groups page to load", async () => {
      await this.distributionCodeGrid.waitForPageLoad();
    });
  }

  async setName(name: string) {
    await allure.step("Set product group name ", async () => {
      await this.page.getByTestId("name").fill(name);
      await this.page.waitForTimeout(1000);
    });
  }

  async verifyNumberOfPeriods(periods: string) {
    await allure.step(`Verify Number of Periods: `, async () => {
      const numberOfPeriods = await this.page.getByTestId("noOfPeriods").inputValue();
      expect(numberOfPeriods).toBe(periods)
    });
  }

  async addDistributionCodeValidFrom() {
    await allure.step('Add date valid from ', async () => {
      const distributionCodeValidFromEle = this.page.locator("(//input[@placeholder='mm/dd/yyyy'])[1]");
      await distributionCodeValidFromEle.click();
      const dateInput = await getCurrentDateUtilWithFormat();
      await distributionCodeValidFromEle.fill(dateInput);
      await this.page.keyboard.press('Enter');
    });
  }

  async filterByDistributionCodeName(name: string) {
    await allure.step("Search Contract Group", async () => {
      await this.distributionCodeGrid.filterByColumnNameAndValue("Name", name);
    });
  }

  async save() {
    await allure.step("Save", async () => {
      await this.page.getByTestId('save').click();
      await this.waitForDataLoad('/Economy/Accounting/DistributionCode/Grid');
    })
  }

  async verifyFilteredRowCount(expectedCount: string) {
    await allure.step(`Verify filtered row count: ${expectedCount}`, async () => {
      await this.distributionCodeGrid.verifyFilteredItemCount(expectedCount);
    });
  }

  async verifyShareDistribution(periods: number, sharecellValue: string, lastCellValue: string) {
    await allure.step(`Verify Share Distribution for periods: ${periods}`, async () => {
      await this.page.locator(`//label[contains(text(), 'Total Periods')]`).scrollIntoViewIfNeeded();
      await this.page.waitForTimeout(3000);
      const shareCells = await this.page.locator(`//ag-grid-angular//div[@data-ref='eBody']//div[@role='gridcell' and @col-id='percent']`).all();
      for (let i = 0; i < shareCells.length; i++) {
        const value = await shareCells[i].textContent();

        if (i == (shareCells.length - 1)) {
          expect(value).toBe(String(lastCellValue));
        }
        else {
          expect(value).toBe(String(sharecellValue));
        }
      }
    });
  }
}
