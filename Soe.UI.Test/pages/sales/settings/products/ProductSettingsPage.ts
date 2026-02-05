import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../../SalesBasePage";
import * as allure from "allure-js-commons";

export class ProductSettingsPage extends SalesBasePage {

    constructor(page: Page) {
        super(page);
    }

    async waitForPageLoad() {
        await allure.step("Wait for product settings page to load", async () => {
            await this.waitForDataLoad('/ajax/getUserCompanySetting');
            await this.page.waitForTimeout(3000);
        });
    }

    async verifyGrossMarginSettings(expectedValue: string) {
        await allure.step("Verify Gross Margin Settings", async () => {
            const grossMarginDropdown = this.page.getByLabel('Gross margin calculation');
            await expect(grossMarginDropdown).toBeVisible();
            const selectedLabel = await grossMarginDropdown.locator('option:checked').textContent();
            expect(selectedLabel?.trim()).toBe(expectedValue);
        });
    }

    async setGrossMarginSettings(value: string) {
        await allure.step("Set Gross Margin Settings to " + value, async () => {
            const grossMarginDropdown = this.page.getByLabel('Gross margin calculation');
            await grossMarginDropdown.selectOption({ label: value });
            await this.page.getByRole('button', { name: 'Save' }).click();;
            await this.waitForDataLoad('/ajax/getUserCompanySetting');
        });
    }

}