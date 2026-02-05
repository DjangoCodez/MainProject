import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../../SalesBasePage";
import * as allure from "allure-js-commons";
import { AngVersion } from "enums/AngVersionEnums";
import { SingleGridPageJS } from "pages/common/SingleGridPageJS";


export class AssignmentTypesPage extends SalesBasePage {
  readonly ang_version: AngVersion;
  readonly assignmentTypeGrid: SingleGridPageJS;

  constructor(page: Page, ang_version: AngVersion = AngVersion.NEW) {
    super(page);
    this.ang_version = ang_version;
    this.assignmentTypeGrid = new SingleGridPageJS(page);
  }

  async waitForPageLoad() {
    await allure.step("Wait for Product Groups page to load", async () => {
      await this.page.locator("//label[normalize-space()='Shift type']").waitFor({ state: 'visible' });
      await this.waitForDataLoad('/api/Economy/Accounting/AccountDim/ShiftType/true');
    });
  }

  async setType(type: string) {
    await allure.step("Set product group code ", async () => {
      const shiftType = this.page.locator('#ctrl_shiftType_timeScheduleTemplateBlockType');
      await shiftType.waitFor({ state: 'visible' });
      await shiftType.selectOption({ label: type });
    });
  }

  async name(name: string) {
    await allure.step("Set Name ", async () => {
      const shiftTypeName = this.page.locator('#ctrl_shiftType_name');
      await shiftTypeName.waitFor({ state: 'visible' });
      await shiftTypeName.fill(name);
    });
  }

  async color(color: string) {
    await allure.step("Set Color ", async () => {
      const shiftTypeColor = this.page.locator('#ctrl_shiftType_color');
      await shiftTypeColor.waitFor({ state: 'visible' });
      await shiftTypeColor.fill(color);
      await shiftTypeColor.press('Enter');
    });
  }

  async length(length: string) {
    await allure.step("Set Length ", async () => {
      const shiftTypeLength = this.page.locator('#ctrl_shiftType_defaultLengthFormatted');
      await shiftTypeLength.waitFor({ state: 'visible' });
      await shiftTypeLength.fill(length);
    });
  }

  async saveAssignmentType() {
    await allure.step("Save Assignment Type ", async () => {
      const saveButton = this.page.locator('//button[@type=\'button\' and @data-ng-if=\'ctrl.modifyPermission\' and normalize-space()=\'Save\']');
      await saveButton.waitFor({ state: 'visible' });
      await saveButton.click();
      await this.page.waitForTimeout(500);
    });
  }

  async close() {
    await allure.step("Close Assignment Type page", async () => {
      const close = this.page.locator("//i[@class='removableTabIcon fal fa-times ng-scope' and @title='Close']");
      await close.waitFor({ state: 'visible' });
      await close.click();
      await this.page.waitForTimeout(500);
    });
  }

  async filterByName(name: string) {
    await allure.step("Filter by Name", async () => {
      await this.assignmentTypeGrid.filterByName('Name', name);
    });
  }

  async edit() {
    await allure.step("Edit Assignment Type", async () => {
      await this.assignmentTypeGrid.edit();
    });
  }

  async verifyType(expectedType: string) {
    await allure.step("Verify Type", async () => {
      const type = await this.page.locator('#ctrl_shiftType_timeScheduleTemplateBlockType option:checked').textContent();
      const actualType = await type;
      expect(actualType, `actual: ${actualType} expected: ${expectedType}`).toBe(expectedType);
    });
  }

  async verifyName(expectedName: string) {
    await allure.step("Verify Name", async () => {
      const name = this.page.locator('#ctrl_shiftType_name');
      const actualName = await name.inputValue();
      expect(actualName, `actual: ${actualName} expected: ${expectedName}`).toBe(expectedName);
    });
  }

  async verifyColor(expectedColor: string) {
    await allure.step("Verify Color", async () => {
      const color = this.page.locator('#ctrl_shiftType_color');
      const actualColor = await color.inputValue();
      expect(actualColor, `actual: ${actualColor} expected: ${expectedColor}`).toBe(expectedColor);
    });
  }

  async verifyLength(expectedLength: string) {
    await allure.step("Verify Length", async () => {
      const actualLength = await this.page.$eval('#ctrl_shiftType_defaultLengthFormatted', el => (el as HTMLInputElement).value);
      expect(actualLength, `actual: ${actualLength} expected: ${expectedLength}`).toBe(expectedLength);
    });
  }
}