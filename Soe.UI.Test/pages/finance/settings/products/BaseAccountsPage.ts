import { Page } from "@playwright/test";
import * as allure from "allure-js-commons";

export class BaseAccountsPage {

    readonly page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    async waitForLoad() {
        await allure.step("Wait for Base Accounts Page to Load", async () => {
            await this.page.waitForLoadState('networkidle');
        });
    }

    async selectOutputVAT2Account(accountName: string) {
        await allure.step(`Select Output VAT 2 Account : ${accountName}`, async () => {
            const account = await this.page.locator('#AccountCommonVatPayable2');
            await account.waitFor({ state: 'visible' });
            await account.fill(accountName);
            await this.page.keyboard.press('Enter');
     });
    }

    async selectOutputVAT3Account(accountNumber: string) {
        await allure.step(`Select Output VAT 3 Account : ${accountNumber}`, async () => {
            const account = await this.page.locator('#AccountCommonVatPayable3');
            await account.waitFor({ state: 'visible' });
            await account.fill(accountNumber);
            await this.page.keyboard.press('Enter');
        });
    }

    async save() {
        await allure.step("Save Base Accounts Settings", async () => {
            const saveButton = await this.page.getByRole('button', { name: 'Save' });
            await saveButton.waitFor({ state: 'visible' });
            await saveButton.click();
        });
    }

}