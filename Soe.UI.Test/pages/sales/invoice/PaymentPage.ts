import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../SalesBasePage";
import * as allure from "allure-js-commons";
import { SingleGridPageJS } from "pages/common/SingleGridPageJS";

export class PaymentPage extends SalesBasePage {

    readonly paymentsGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.paymentsGrid = new SingleGridPageJS(page);
    }

    async waitForPageLoad() {
        await allure.step("Wait for payments page to load", async () => {
            await this.waitForDataLoad('/api/Core/CustomerInvoices');
            await this.page.waitForTimeout(3000);
        });
    }

    async filterByInvoiceNumber(invoiceNumber: string) {
        await allure.step(`Filter payments grid by invoice number: ${invoiceNumber}`, async () => {
            await this.paymentsGrid.filterByName('Invoice No.', invoiceNumber, 5000);
        });
    }

    async selectFirstGridRow() {
        await allure.step("Select the first row in payments grid", async () => {
            await this.paymentsGrid.selectCheckBox(0);
        });
    }

    async fillPaymentDate() {
        await allure.step("Fill payment date with today's date", async () => {
            const today = new Date();
            const month = today.getMonth() + 1;
            const day = today.getDate();
            const year = String(today.getFullYear()).slice(-2);
            const formattedDate = `${month}/${day}/${year}`;
            await this.page.locator('#ctrl_selectedPayDate').fill(formattedDate);
        });
    }

    async clickCreatePayment() {
        await allure.step("Click on Create Payment button", async () => {
            const button = this.page.locator('button.btn.btn-sm.btn-default.ngSoeSplitButton.dropdown-toggle.ngSoeMainButton');
            await button.click();
            await this.page.getByRole('link', { name: 'Create payment' }).click();
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
        });
    }

}