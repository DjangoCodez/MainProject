import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../SalesBasePage";
import * as allure from "allure-js-commons";
import { GridPage } from "../../common/GridPage";

export class ProductUnitsPage extends SalesBasePage {
  readonly productUnitsGrid: GridPage;

  constructor(page: Page) {
    super(page);
    this.productUnitsGrid = new GridPage(
      page,
      "billing.product.productunit.productunits"
    );
  }

  async setCode(code: string) {
    await allure.step("Set product unit ", async () => {
      await this.page.getByTestId("code").fill(code);
    });
  }

  async setName(name: string) {
    await allure.step("Set product unit name ", async () => {
      await this.page.getByTestId("name").fill(name);
      await this.page.keyboard.press("Enter");
    });
  }

  async filterByProductUnitName(name: string) {
    await allure.step("Search Contract Unit", async () => {
      await this.productUnitsGrid.filterByColumnNameAndValue("Name", name);
    });
  }

  async VerifyRowCount(count: string) {
    await allure.step(
      "Verify Product unit filtered count is " + count,
      async () => {
        await expect(this.page.getByTestId("filtered")).toHaveText(
          "(Filtered " + count + ")");
      }
    );
  }

  async editProductUnit() {
    await allure.step("Edit Product unit", async () => {
      await this.page
        .locator('span[title="Edit"]')
        .nth(0)
        .click({ timeout: 30000 });
    });
  }

  async updateProductUnit(name: string) {
    await allure.step("Update Product unit Name", async () => {
      await this.page.getByTestId("name").fill(name);
    });
  }

  async deleteProductUnit() {
    await allure.step("Delete Product unit", async () => {
      await this.page.getByRole("button", { name: " Remove" }).click();
      await this.page.getByRole("button", { name: "OK" }).click();
    });
  }

  async verifyRowCount(expectedCount: number) {
    await allure.step("Verify Row Count", async () => {
      const count = await this.productUnitsGrid.getRowCount("Name");
      expect(
        count,
        `Mismatch: UI shows "${count}", expected "${expectedCount}"`
      ).toBe(expectedCount);
    });
  }

  async waitForSave() {
    await allure.step("Wait for save", async () => {
       await this.page.waitForResponse((resp) => resp.url().includes('/api/V2/Billing/Product/ProductUnit') && resp.status() === 200, { timeout: 10000 });
    });
  }

}
