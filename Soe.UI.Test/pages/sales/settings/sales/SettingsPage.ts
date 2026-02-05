import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../../SalesBasePage";
import * as allure from "allure-js-commons";

export class SettingsPage extends SalesBasePage {

    constructor(page: Page) {
        super(page);
    }

    async waitForPageLoad() {
        await allure.step("Wait for product settings page to load", async () => {
            await this.waitForDataLoad('/ajax/getUserCompanySetting');
            await this.page.waitForTimeout(3000);
        });
    }

    async setEInvoiceFormat(format: string) {
        await allure.step("Set E-Invoice Format to " + format, async () => {
            const eInvoiceDropdown = this.page.getByLabel('E-Invoice format');
            await expect(eInvoiceDropdown).toBeVisible();
            await eInvoiceDropdown.selectOption({ label: format });
        });
    }

    async saveSettings() {
        await allure.step("Save Settings", async () => {
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.waitForDataLoad('/ajax/getUserCompanySetting');
        });
    }

}