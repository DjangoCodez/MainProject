import { Page } from "@playwright/test";
import { FinanceBasePage } from "../FinanceBasePage";
import * as allure from "allure-js-commons";

export class AccountPayableSettingsPage extends FinanceBasePage {

    constructor(page: Page) {
        super(page);
    }

    async waitForPageLoad() {
        await allure.step("Wait for Account Payable Settings page to load", async () => {
            await this.page.getByRole('link', { name: 'Account Payable Settings' }).waitFor({ state: 'visible', timeout: 10000 });
        });
    }

    async transferInvoiceToVoucher(check: boolean = false) {
        await allure.step("Transfer Invoice To Voucher", async () => {
            const locator = this.page.locator("#InvoiceTransferToVoucher");
            await locator.waitFor({ state: 'visible' });
            if (check) {
                if (!await locator.isChecked()) {
                    await locator.check();
                }
            } else {
                if (await locator.isChecked()) {
                    await locator.uncheck();
                }
            }
        });
    }

    async unselectTransferPaymentToVoucher() {
        await allure.step("Unselect Transfer Payment To Voucher", async () => {
            const locator = this.page.locator("#PaymentManualTransferToVoucher");
            await locator.waitFor({ state: 'visible' });
            if (await locator.isChecked()) {
                await locator.uncheck();
            }
        });
    }

    async saveAccountPayableSettings() {
        await allure.step("Save Account Payable Settings", async () => {
            const save = this.page.locator("//button[@id='submit']");
            await save.waitFor({ state: 'visible' });
            await save.click();
            await this.waitForDataLoad('/ajax/getUserCompanySetting.aspx?date');
        });
    }
}