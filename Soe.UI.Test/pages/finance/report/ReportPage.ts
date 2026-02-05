import { expect, Page } from "@playwright/test";
import { BasePage } from "../../common/BasePage";
import * as allure from "allure-js-commons";
import { getValueFromPDF } from "../../../utils/CommonUtil";
import * as fs from 'fs';


export class ReportPage extends BasePage {
    readonly page: Page;

    constructor(page: Page) {
        super(page);
        this.page = page;
    }

    async selectReport(reportName: string) {
        await allure.step(`Select Report Huvubok`, async () => {
            const report = this.page.locator(`//table[contains(@class,'report-menu-hless-table')]//tr[td[normalize-space()='${reportName}']]`);
            await report.waitFor({ state: 'visible' });
            await report.click();
        });
    }

    async selectAccount(accountNumber: string, rowIndex: number = 0) {
        await allure.step(`Select Account: ${accountNumber}`, async () => {
            const accountInput = this.page.locator('#filterRange_accountFrom').nth(rowIndex);
            await accountInput.fill(accountNumber);
            const firstVisibleOption = this.page.locator('ul[role="listbox"] li', { hasText: accountNumber }).filter({ has: this.page.locator(':visible') }).first();
            await firstVisibleOption.waitFor({ state: 'visible' });
            await firstVisibleOption.click();
        });
    }

    async createReport() {
        await allure.step(`Create Report`, async () => {
            const printButton = this.page.locator("//button[@type='button' and contains(@class,'fa-print') and @title='Create']");
            await printButton.waitFor({ state: 'visible' });
            await printButton.click();
            await this.waitForDataLoad(`/api/Report/Menu/Queue/`);
        });
    }

    async printedQueueReportReload() {
        await allure.step(`Reload Printed Queue`, async () => {
            const reloadIcon = this.page.locator("//table[contains(@class,'table-condensed')]//th//i[contains(@class,'fa-sync')]");
            await reloadIcon.waitFor({ state: 'visible' });
            await reloadIcon.click();
            await this.waitForDataLoad(`/api/Report/Menu/Queue`, 50000);
        });
    }

    async rightClickReportPdf() {
        await allure.step(`Right Click Report PDF`, async () => {
            const pdfIcon = this.page.locator("//table[contains(@class,'table-condensed')]//tr[td[normalize-space()='Huvudbok 2dim']]//i[contains(@class,'fa-file-pdf')]");
            await pdfIcon.nth(0).waitFor({ state: 'visible' });
            await pdfIcon.nth(0).click({ button: 'right' });
        });
    }

    async openReport() {
        return await allure.step(`Open Report`, async () => {
            const openReportOption = this.page.locator("//ul[contains(@class,'dropdown-menu') and @role='menu']//a[.//span[contains(.,'Open report')]]");
            await openReportOption.waitFor({ state: 'visible' });
            const [download] = await Promise.all([this.page.waitForEvent('download'), openReportOption.click()]);
            const filename = download.suggestedFilename();
            const localPath = `./test-data/temp-download/${filename}`;
            console.log('Downloaded file path:', localPath);
            await download.saveAs(localPath);
            console.log('File saved as:', filename);
            return localPath;
        });
    }

    async removeReport() {
        return await allure.step(`Remove Report`, async () => {
            const removeReportOption = this.page.locator("//ul[contains(@class,'dropdown-menu') and @role='menu']//a[.//span[contains(.,'Remove')]]");
            await removeReportOption.waitFor({ state: 'visible' });
            await removeReportOption.click();
        });
    }

    async verifyTotalValueInPdf(reportPath: string, totalValue: string) {
        await allure.step(`Read Report PDF`, async () => {
            let total = await getValueFromPDF(reportPath, /(?<![A-Za-z])Total(?![A-Za-z])[^\S\r\n]*(-?\d{1,3}(?:[ .]\d{3})*|\d+)(?:,(\d{2}))?/i);
            console.log('Extracted Total Value from PDF:', total);
            total = Number(total?.replace(/\s/g, '').replace(',', '.')).toFixed(2);
            expect(total, `expected ${totalValue} but found ${total}`).toBe(totalValue);
        });
    }

    async deleteDownloadedFile(reportPath: string) {
        await allure.step(`Delete Downloaded File`, async () => {
            fs.unlinkSync(reportPath);
        });
    }
}


