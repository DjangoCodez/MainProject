import { Page } from "@playwright/test";
import { StaffBasePage } from "pages/staff/StaffBasePage";
import * as allure from "allure-js-commons";
import { GridPage } from "pages/common/GridPage";

export class AgreementGroupsPage extends StaffBasePage {
  readonly agreementGroupsGrid: GridPage;

  constructor(page: Page) {
    super(page);
    this.agreementGroupsGrid = new GridPage(
      page, "Time.Employee.EmployeeCollectiveAgreement"
    );
  }
  async waitForPageLoad() {
    await allure.step("Wait for Agreement Groups page to load", async () => {
      await this.agreementGroupsGrid.waitForPageLoad();
    });
  }

  async setCode(code: string) {
    await allure.step("Set agreement group code ", async () => {
      await this.page.getByTestId("code").fill(code);
    });
  }

  async setName(name: string) {
    await allure.step("Set agreement group name ", async () => {
      await this.page.getByTestId("name").fill(name);
    });
  }

  async setDescription(description: string) {
    await allure.step("Set agreement group description ", async () => {
      await this.page.getByTestId("description").fill(description);
    });
  }

  async selectTimeAgreement(timeAgreement: string) {
    await allure.step("Select time agreement from the dropdown ", async () => {
      const timeAgSelection = this.page.getByTestId("employeeGroupId");
      await timeAgSelection.selectOption({ label: timeAgreement });
    });
  }

  async selectSalaryAgreement(salaryAgreement: string) {
    await allure.step("Select salary agreement from the dropdown ", async () => {
      const salaryAgSelection = this.page.getByTestId("payrollGroupId");
      await salaryAgSelection.selectOption({ label: salaryAgreement });
    });
  }

  async selectHolidayAgreement(holidayAgreement: string) {
    await allure.step("Select holiday agreement from the dropdown ", async () => {
      const holidayAgSelection = this.page.getByTestId("vacationGroupId");
      await holidayAgSelection.selectOption({ label: holidayAgreement });
    });
  }

  async saveAgreementGroup() {
    await allure.step("Save", async () => {
      const [responses] = await Promise.all([
        Promise.all([
          this.page.waitForResponse(response =>
            response.url().includes("/api/V2/Time/Employee/EmployeeCollectiveAgreement/Grid") &&
            response.status() === 200
          ),
        ]),
        this.page.getByTestId('save').click(),
      ]);
    });
  }

  async filterByAgreementGroupName(name: string) {
    await allure.step("filter the created agreement group", async () => {
      await this.agreementGroupsGrid.filterByColumnNameAndValue("Name", name);
      await this.page.keyboard.press("Enter");
    });
  }

  async verifyFilteredRowCount(expectedCount: string) {
    await allure.step(`Verify filtered row count: ${expectedCount}`, async () => {
      await this.agreementGroupsGrid.verifyFilteredItemCount(expectedCount);
    });
  }

  async editAgreementGroup() {
    await allure.step("Edit Agreement Group", async () => {
      await this.page.locator('span[title="Edit"]').nth(0).click();
      await this.page.waitForResponse(response =>
        response.url().includes("/api/V2/Time/Employee/EmployeeCollectiveAgreement") &&
        response.status() === 200
      );
      const codeInput = this.page.getByTestId("code");
      await codeInput.waitFor({ state: "visible", timeout: 5000 });
    });
  }

  async setActiveStatus(isActive: boolean) {
    await allure.step(`Mark Agreement Group as ${isActive ? "Active" : "Inactive"}`, async () => {
      const checkbox = this.page.getByTestId("isActive");
      if (isActive) {
        await checkbox.check({ timeout: 1000 });
      } else {
        await checkbox.uncheck({ timeout: 1000 });
      }
      await this.page.waitForTimeout(1000);
    });
  }

  async clickGridActiveCheckbox() {
    await allure.step("Click Active/Inactive checkbox in the grid", async () => {
      await this.agreementGroupsGrid.clickGridActiveInactiveCheckbox();
      await this.page.waitForTimeout(500);
    });
  }

  async deleteAgreementGroup() {
    await allure.step("Delete Agreement Group", async () => {
      await this.page.getByRole("button", { name: " Remove" }).click();
      await this.page.getByRole("button", { name: "OK" }).click();
    });
  }
}