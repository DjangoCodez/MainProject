import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../SalesBasePage";
import * as allure from "allure-js-commons";
import { GridPage } from "../../common/GridPage";

export class ProductGroupsPage extends SalesBasePage {
  readonly productGroupGrid: GridPage;

  constructor(page: Page) {
    super(page);
    this.productGroupGrid = new GridPage(
      page,
      "Billing.Invoices.Productgroups"
    );
  }

  async waitForPageLoad() {
    await allure.step("Wait for Product Groups page to load", async () => {
      await this.productGroupGrid.waitForPageLoad();
    });
  }

  async setCode(code: string) {
    await allure.step("Set product group code ", async () => {
      await this.page.getByTestId("code").fill(code);
    });
  }

  async setName(name: string) {
    await allure.step("Set product group name ", async () => {
      await this.page.getByTestId("name").fill(name);
      await this.page.waitForTimeout(1000);
    });
  }

  async saveProductGroup() {
    await allure.step("Save Product Group", async () => {
      await this.page.getByTestId('save').click();
      await this.waitForDataLoad('Billing/ProductGroups/Grid');
    });
  }

  async filterByProductGroupName(name: string) {
    await allure.step("Search Contract Group", async () => {
      await this.productGroupGrid.filterByColumnNameAndValue("Name", name);
    });
  }

  async VerifyRowCount(count: string) {
    await allure.step(
      "Verify Product group filtered count is " + count,
      async () => {
        await expect(this.page.getByTestId("filtered")).toHaveText(
          "(Filtered " + count + ") "
        );
      }
    );
  }

  async editProductGroup() {
    await allure.step("Edit Product Group", async () => {
      await this.page
        .locator('span[title="Edit"]')
        .nth(0)
        .click({ timeout: 30000 });
    });
  }

  async updateProductGroupName(name: string) {
    await allure.step("Update Product Group Name", async () => {
      await this.page.getByTestId("name").fill(name);
    });
  }

  async deleteProductGroup() {
    await allure.step("Delete Product Group", async () => {
      await this.page.getByRole("button", { name: " Remove" }).click();
      await this.page.getByRole("button", { name: "OK" }).click();
    });
  }

  async verifyRowCount(expectedCount: number) {
    await allure.step("Verify Row Count", async () => {
      const count = await this.productGroupGrid.getRowCount("Name");
      expect(
        count,
        `Mismatch: UI shows "${count}", expected "${expectedCount}"`
      ).toBe(expectedCount);
    });
  }
}
