import { Page } from "@playwright/test";
import { FinanceBasePage } from "../FinanceBasePage";
import * as allure from "allure-js-commons";

export class AccountsReceivableSettingsPage extends FinanceBasePage {

    constructor(page: Page) {
        super(page);
    }

    async unselectTransferInvoiceToVoucher() {
        await allure.step("Unselect Transfer Invoice To Voucher", async () => {
            const locator = this.page.locator("#InvoiceTransferToVoucher");
            await locator.waitFor({ state: 'visible' });
            if (await locator.isChecked()) {
                await locator.uncheck();
            }
        });
    }

    async saveAccountReceivableSettings() {
        await allure.step("Save Account Receivable Settings", async () => {
            const save = this.page.locator("//button[@id='submit']");
            await save.waitFor({ state: 'visible' });
            await save.click();
        });
    }

    async moveToRequirementsandBillingTab() {
        await allure.step("Move To Requirements and Billing Tab", async () => {
            const tab = this.page.locator("//div[@class='tabList']//ul//a[contains(text(),'Settings for Requirements and Interest Billing')]");
            await tab.waitFor({ state: 'visible' });
            await tab.click();
        });
    }

    async setInterestOnLatePayments(interest: string) {
        await allure.step("Set Interest On Late Payments", async () => {
            const locator = this.page.locator("#InterestPercent");
            await locator.waitFor({ state: 'visible' });
            await locator.fill(interest);
        });
    }
}