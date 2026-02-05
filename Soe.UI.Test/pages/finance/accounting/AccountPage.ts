import { expect, Page } from "@playwright/test";
import { BasePage } from "../../common/BasePage";
import { GridPage } from "../../common/GridPage";
import * as allure from "allure-js-commons";
import { AngVersion } from "enums/AngVersionEnums";
import { SingleGridPageJS } from "pages/common/SingleGridPageJS";


export class AccountPage extends BasePage {
    readonly page: Page;
    readonly accountsGrid: GridPage;
    readonly accountsGridJS: SingleGridPageJS;
    readonly ang_version: AngVersion;

    constructor(page: Page, ang_version: AngVersion = AngVersion.NEW) {
        super(page);
        this.page = page;
        this.ang_version = ang_version;
        this.accountsGrid = new GridPage(page, 'economy.accounting.accounts');
        this.accountsGridJS = new SingleGridPageJS(page);
    }

    async waitForPageLoad() {
        await allure.step(`Wait for Account Page Load`, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.accountsGrid.waitForPageLoad();
            } else {
                await this.accountsGridJS.waitForPageLoad();
            }
            // await this.page.getByTitle('Account Plan', { exact: true }).waitFor({ state: 'visible', timeout: 10000 });
        });
    }

    async waitForNetworkIdle(timeout: number = 5000) {
        await allure.step(`Wait for Network Idle`, async () => {
            await this.waitForDataLoad('**/api/Economy/Accounting/Voucher/BySeries/**', timeout);
        });
    }

    async filterAccount(accountNumber: string) {
        await allure.step(`Filter Account By Account Number: ${accountNumber}`, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.accountsGrid.filterByColumnNameAndValue('Number', accountNumber);
            } else {
                await this.accountsGridJS.filterByName('Number', accountNumber);
            }
        });
    }

    async openAccount() {
        await allure.step(`Open Account`, async () => {
            await this.page.getByTitle('Edit').nth(0).click({ timeout: 5000 });
        });
    }

    async verifyIfAccrualAccount() {
        await allure.step(`Verify If Accrual Account`, async () => {
            let checkbox = this.page.getByRole('checkbox', { name: 'Accrual account' });
            if (this.ang_version === AngVersion.NEW) {
                checkbox = this.page.getByTestId('isAccrualAccount');
            }
            await checkbox.waitFor({ state: 'visible', timeout: 5000 });
            if (!(await checkbox.isChecked())) {
                await checkbox.check();
            }
        });
    }

    async createItem() {
        await allure.step("Create Item", async () => {
            const newItemButton = this.page.locator(`//ul/li[@index!="tab.index"]`).nth(0)
            const tabs = this.page.locator(`//li[@index="tab.index"]`)
            const initTabCount = await tabs.count();
            await expect.poll(async () => await newItemButton.isEnabled(), { timeout: 5000 }).toBe(true);
            await newItemButton.click({ force: true });
            const finalTabCount = await tabs.count();
            expect(finalTabCount, 'New tab was not created').toBe(initTabCount + 1);
        })
    }

    async addCodingRow(debitAmount: string, creditAmount: string, rowIndex: number = 0) {
        await allure.step("Add Coding Row", async () => {
            const debitRow = this.page.locator(`//div[@id="accounting-rows-grid"]//div[@row-index="${rowIndex}" ]//div[@col-id="debitAmount"]`)
            const creditRow = this.page.locator(`//div[@id="accounting-rows-grid"]//div[@row-index="${rowIndex}" ]//div[@col-id="creditAmount"]`)
            const balance = this.page.locator(`//div[@id="accounting-rows-grid"]//div[@row-index="${rowIndex}" ]//div[@col-id="balance"]`)
            await this.page.locator('//a[@title="Add Row"]').click();
            const accounts = this.page.locator('//div[@name="root"]/ul')
            await accounts.waitFor({ state: 'visible', timeout: 5000 });
            await accounts.locator('li').nth(rowIndex + 1).click();
            await this.page.waitForTimeout(1000);
            await debitRow.click();
            await debitRow.locator('input').fill(debitAmount);
            await balance.click();
            await this.page.waitForTimeout(1000);
            await creditRow.click();
            await creditRow.locator('input').fill(creditAmount);
            await balance.click();
        });
    }

    async addText(text: string) {
        await allure.step(`Add Text: ${text}`, async () => {
            const textArea = this.page.locator('#ctrl_voucher_text');
            await textArea.fill(text);
        });
    }

    async saveVoucher() {
        return await allure.step("Save Voucher", async () => {
            const mainButton = this.page.locator('//div[@main-button="true"]')
            await mainButton.nth(0).click();
            const request = this.page.waitForResponse('**/api/Economy/Accounting/Voucher/**');
            const response = await request;
            const { value, integerValue } = await response.json();
            return { voucherId: integerValue, voucherNumber: value };
        });
    }

    async searchReport(report: string) {
        await allure.step(`Search ${report}`, async () => {
            await this.page.locator("#report-menu-toggle").click()
            await this.page.locator("#reportMenuPanel").waitFor({ state: "visible", timeout: 2000 })
            await this.page.locator('//li[@title="Reports"]').click()
            await this.page.locator('#ctrl_freeTextFilter').fill(report)
            const reportItem = this.page.locator(`//tr/td[2]/span[text()="${report}"]`)
            await reportItem.nth(0).click()
            const reportPanel = this.page.locator('#report-menu-overview-container')
            await reportPanel.waitFor({ state: "visible", timeout: 2000 })
            const reportGenerateRequest = this.page.waitForResponse(`**/api/Report/Menu/Queue/**`);
            await this.page.locator('//button[@title="Create"]').click()
            const response = await reportGenerateRequest;
            const { reportName, created } = await response.json();
            expect(response.ok(), 'Report generation failed').toBeTruthy();
            expect(reportName, 'Generated report name mismatch').toBe(report);
            expect(created, 'Report creation timestamp missing').not.toBeNull();
        })
    }

    async setVoucherSearchPeriod(period: string) {
        await allure.step(`Set Voucher Search Period: ${period}`, async () => {
            const dropDown = this.page.locator('//soe-select//select[@id="selection_economy.accounting.vouchersearch.voucherperiod"]');
            await dropDown.click();
            await dropDown.selectOption({ label: period });
        });
    }

    async enterVoucherText(voucherText: string) {
        await allure.step(`Enter Voucher Text: ${voucherText}`, async () => {
            const textBox = this.page.locator('//input[@data-testid="voucherText"]');
            await textBox.fill(voucherText);
        });
    }

    async searchVoucher() {
        await allure.step(`Search Voucher`, async () => {
            await this.page.locator('//button[@data-testid="save"]').click();
        });
    }

    async verifyVoucherInGrid(voucherId: string) {
        await allure.step(`Verify Voucher In Grid: ${voucherId}`, async () => {
            const voucherInGrid = this.page.locator(`//div[@data-ref="eViewport"]/div/div[@row-id!="rowGroupFooter_ROOT_NODE_ID" and @role="row"]`);
            await expect.poll(async () => await voucherInGrid.count(), { timeout: 5000 }).toBe(2);
        });
    }

}