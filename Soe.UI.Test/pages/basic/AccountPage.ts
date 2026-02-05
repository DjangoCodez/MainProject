import { expect, type Locator, type Page } from '@playwright/test';
import * as allure from "allure-js-commons";

export class AccontPage {
    readonly page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    async selectAccount(accountName: string) {
        await allure.step("Select environment " + accountName, async () => {
            if (accountName.includes("NA") || accountName.includes("na")) {
                return
            }
            const locator = this.page.getByPlaceholder(accountName).first();
            await locator.waitFor({ state: 'visible', timeout: 25_000 });
            await locator.click();
        });
    }


}