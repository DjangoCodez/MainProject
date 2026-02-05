import { expect, Page } from "@playwright/test";
import * as allure from "allure-js-commons";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";
import { FinanceBasePage } from "../FinanceBasePage";

export class InvoicePageJS extends FinanceBasePage {
    readonly page: Page;
    readonly customerInvoiceGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.page = page;
        this.customerInvoiceGrid = new SingleGridPageJS(page);
    }

    async waitForPageLoad() {
        await allure.step("Wait for Invoices page to load", async () => {
            await this.customerInvoiceGrid.waitForPageLoad();
        });
    }

    async filterByInvoiceNo(invoiceNo: string) {
        await allure.step(`Filter by invoice number: ${invoiceNo}`, async () => {
            await this.customerInvoiceGrid.filterByName('Invoice No.', invoiceNo);
        });
    }

    async selectInvoiceByNumber() {
        await allure.step(`Select invoice `, async () => {
            await this.page.getByRole('checkbox', { name: 'Press Space to toggle row' }).check();
        });
    }

    async transferToVoucher() {
        await allure.step("Transfer to Voucher", async () => {
            await this.page.locator(`//button[@type='button' and contains(@class, 'ngSoeMainButton') and contains(@class, 'dropdown-toggle')]`).click();
            await this.page.getByRole('link', { name: 'Transfer to voucher' }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
            await expect(this.page.getByText('Saved')).toBeVisible();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async editInvoice() {
        await allure.step("Edit invoice", async () => {
            const editInvoiceButton = this.page.locator("//button[contains(@class, 'iconEdit')]");
            await editInvoiceButton.waitFor({ state: 'visible' });
            await editInvoiceButton.click();
        });
    }

    async expandCustomerInvoiceTab() {
        await allure.step("Expand customer invoice tab ", async () => {
            const expand = await this.page.locator("//div[@class='soe-accordion-heading ng-scope']//label[text()='Customer Invoice']");
            await expand.waitFor({ state: 'visible' });
            await expand.click();
            await this.waitForDataLoad('/api/Core/Currency/Enterprise/');
        });
    }

    async changeDueDate(dueDate: string) {
        await allure.step(`Change due date to: ${dueDate}`, async () => {
            const dueDateInput = this.page.locator("//input[@id='ctrl_invoice_dueDate']");
            await dueDateInput.waitFor({ state: 'visible' });
            await dueDateInput.fill(dueDate);
            await this.page.keyboard.press('Tab');
        });
    }

    async saveInvoice() {
        await allure.step("Save invoice", async () => {
            const save = this.page.getByRole('button', { name: 'Save' });
            await save.waitFor({ state: 'visible' });
            await save.click();
            await expect(this.page.getByText('Saved')).toBeVisible();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async showClosed() {
        await allure.step("Show closed invoices", async () => {
            const showClosed = this.page.locator("//input[@type='checkbox' and @id='ctrl_loadClosed']");
            await showClosed.waitFor({ state: 'visible' });
            if (!(await showClosed.isChecked())) {
                await showClosed.check();
                await this.waitForDataLoad('/api/Core/CustomerInvoices/');
            }
        });
    }

    async addColumnToGrid(column: string) {
        await allure.step("Add column to grid", async () => {
            await this.customerInvoiceGrid.setGridColumnHeaders(column);
        });
    }

    async filterByInternalText(internalText: string) {
        await allure.step(`Filter by internal text: ${internalText}`, async () => {
            await this.customerInvoiceGrid.filterByName('Internal Text', internalText);
        });
    }

    async expandAttest() {
        await allure.step("Expand Attest", async () => {
            const expand = await this.page.locator("//div[@class='soe-accordion-heading ng-scope']//label[text()='Attest']");
            await expand.waitFor({ state: 'visible' });
            await expand.click();
        });
    }

    async clickToAttestation() {
        await allure.step("Click to Attestation", async () => {
            const attestButton = this.page.locator("//button[@title='To Attestation']");
            await attestButton.waitFor({ state: 'visible' });
            await attestButton.click();
            await this.waitForDataLoad('/api/Core/UserGridState/Economy_Supplier_Invoices');
            await this.page.waitForTimeout(1000);
            const okButton = await this.page.getByRole('button', { name: 'OK' });
            await okButton.scrollIntoViewIfNeeded();
            await okButton.click();
            await this.page.waitForTimeout(1000);
            await this.clickAlertMessage('OK');
            const approveButton = await this.page.getByRole('button', { name: 'Approve', exact: true });
            await approveButton.waitFor({ state: 'visible' });
            await approveButton.click();
        });
    }
}
