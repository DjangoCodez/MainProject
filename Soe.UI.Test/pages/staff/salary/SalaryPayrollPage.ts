import { StaffBasePage } from "../StaffBasePage";
import { Page, expect } from "@playwright/test";
import * as allure from "allure-js-commons";
import { extractWithPdfReader } from "utils/CommonUtil";
import { GridPageJS } from "pages/common/GridPageJS";

export class SalaryPayrollPage extends StaffBasePage {
    readonly salaryIDGrid: GridPageJS;
    constructor(page: Page) {

        super(page);
        this.salaryIDGrid = new GridPageJS(page, "directiveCtrl.gridHandler.gridAg.options.gridOptions");
    }

    async waitforPageLoad() {
        await allure.step("Wait for load", async () => {
            await this.waitForDataLoad('Time/Payroll/PayrollCalculation/TreeWarnings/');
        });
    }

    async setPeriodSet(periodSet: string) {
        await allure.step(`Set Period Set to ${periodSet}`, async () => {
            await this.page.getByLabel('Period Set').selectOption({ label: periodSet });
        });
    }
    async setPaymentDate() {
        await allure.step("Set Payment Date to current month", async () => {
            const now = new Date();
            const year = now.getFullYear();
            const month = now.getMonth() + 1;
            const day = 21;
            let paymentYear = year;
            let paymentMonth = month + 1;
            if (paymentMonth > 12) {
                paymentMonth = 1;
                paymentYear = year + 1;
            }
            const datePrefix = `${paymentYear}-${String(paymentMonth).padStart(2, '0')}-${day}`;
            const paymentDateCombobox = this.page.getByLabel('Payment Date');
            const options = await paymentDateCombobox.locator('option').all();
            for (const option of options) {
                const text = await option.textContent();
                if (text?.startsWith(datePrefix)) {
                    await paymentDateCombobox.selectOption({ label: text });
                    break;
                }
            }
        });
    }

    async searchEmployee(employeeId: string, name: string) {
        await allure.step(`Search employee: ${employeeId}`, async () => {
            await this.page.getByRole('button', { name: 'Employees' }).waitFor({ state: 'visible', timeout: 15000 });
            await this.page.getByRole('button', { name: 'Employees' }).click();
            await this.page.getByRole('textbox', { name: 'Search' }).click();
            await this.page.getByRole('textbox', { name: 'Search' }).fill(employeeId);
            await this.page.getByRole('checkbox', { name: `(${employeeId}) ${employeeId}` }).check();
            await this.page.getByRole('button', { name: 'Filter' }).click();
            await this.page.waitForTimeout(2000);
            await this.page.getByText(`${employeeId}`).first().click();
            await this.page.waitForTimeout(3000);
        });
    }

    async editEmpTaxYear( employeeId: string = '') {
        await allure.step(`Edit Employee Tax Year to next year`, async () => {
            await this.page.locator('span').filter({ hasText: `${employeeId}` }).click();
            await this.page.getByRole('button', { name: 'Employment data ' }).click();
            await this.page.getByRole('button', { name: 'Taxes and social' }).click();
            const now = new Date();
            const year = now.getFullYear();
            const month = now.getMonth() + 1;
            let taxYear = year;
            let taxMonth = month + 1;
            if (taxMonth > 12) {
                taxYear = year + 1;
            }
            const yearSelect = this.page.locator('#ctrl_year');
            const optionValue = await yearSelect
                .locator('option', { hasText: taxYear.toString() })
                .first()
                .getAttribute('value');
            await yearSelect.selectOption({ value: optionValue! });
            await this.page.waitForTimeout(2000);
            await this.page.getByLabel('Calculation', { exact: true }).selectOption({ label: 'Side income 30% tax' });
            const saveButton = this.page.getByRole('button', { name: 'Save' });
            const isSaveEnabled = await saveButton.isEnabled();
            if (isSaveEnabled) {
                await saveButton.click();
            } else {
                await this.page.getByRole('dialog').getByText('×').click();
            }
        });
    }

    async selectCalculate() {
        await allure.step(`Select Calculate`, async () => {
            await this.page.getByRole('button', { name: 'Calculate' }).click();
            await this.page.waitForTimeout(2000);
            await this.page.getByRole('button', { name: 'OK' }).click();
            await this.page.waitForTimeout(5000);
        });
    }

    async clearCalculation() {
        await allure.step(`clear Calculations`, async () => {
            await this.page.getByRole('button', { name: 'Functions' }).click();
            await this.page.getByRole('link', { name: 'Clear calculation' }).click();
            await this.page.waitForTimeout(2000);
            await this.page.getByRole('button', { name: 'OK' }).click();
            await this.page.waitForTimeout(5000);
        });
    }


    async VerifySalaryIsCalculated(salaryId: string) {
        await allure.step(`Search Salary ID: ${salaryId}`, async () => {
            const salaryIdInput = this.page.getByRole('textbox', { name: 'Salary ID Filter Input' });
            await salaryIdInput.click();
            await salaryIdInput.fill(salaryId);
            await this.page.waitForTimeout(2000);
            const rowCount = await this.salaryIDGrid.getAgGridRowCount();
            expect(rowCount).toBeGreaterThan(0);
        });
    }

    async markAsAttesterad() {
        await allure.step(`Mark all salary Ids as Attesterad`, async () => {
            const selectAllCheckbox = this.page.getByRole('checkbox', { name: 'Press Space to toggle all' });
            while (!(await selectAllCheckbox.nth(0).isChecked())) {
                await selectAllCheckbox.nth(0).waitFor({ state: 'attached' });
                await selectAllCheckbox.nth(0).click({ force: true });
            }
            await this.page.waitForTimeout(1000);
            await this.page.locator('.ng-scope div[label-key="time.atteststate.state"] button[data-toggle="dropdown"]').click();
            await this.page.getByRole('link', { name: 'Attesterad' }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async markAsLöneberäknad() {
        await allure.step(`Mark all as Löneberäknad`, async () => {
            const selectAllCheckbox = this.page.getByRole('checkbox', { name: 'Press Space to toggle all' });
            while (!(await selectAllCheckbox.nth(0).isChecked())) {
                await selectAllCheckbox.nth(0).waitFor({ state: 'attached' });
                await selectAllCheckbox.nth(0).click({ force: true });
            }
            await this.page.waitForTimeout(1000);
            await this.page.locator('.ng-scope div[label-key="time.atteststate.state"] button[data-toggle="dropdown"]').click();
            await this.page.getByRole('link', { name: 'Löneberäknad' }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async goToReportAnalysis() {
        await allure.step(`Go to Report Analysis`, async () => {
            await this.page.locator('#report-menu-toggle > a').click();
            await this.page.waitForTimeout(2000);
        });
    }

    async goToReportsTab() {
        await allure.step(`Go to Reports Tab`, async () => {
            await this.page.locator('#reportMenuPanel').getByText('Reports', { exact: true }).click();
            await this.page.waitForTimeout(2000);
        });
    }

    async searchReport(reportName: string) {
        await allure.step(`Search Report: ${reportName}`, async () => {
            await this.page.getByPlaceholder('Search').click();
            await this.page.getByPlaceholder('Search').fill(reportName);
            await this.page.waitForTimeout(2000);

            await this.page.getByText('Lön - lönespecifikation XE').click();
            await this.page.waitForTimeout(2000);
        });
    }

    async setPeriod() {
        await allure.step("Set Period to current month", async () => {
            const now = new Date();
            const year = now.getFullYear();
            const month = now.getMonth() + 1;
            const day = 21;
            let periodYear = year;
            let periodMonth = month + 1;
            if (periodMonth > 12) {
                periodMonth = 1;
                periodYear = year + 1;
            }
            const datePrefix = `(${periodMonth}/${day}/${periodYear})`;
            await this.page.getByRole('button', { name: 'Select' }).click();
            await this.page.getByRole('menuitem', { name: new RegExp(`^${datePrefix.replace(/[()]/g, '\\$&')}`) }).click();
            await this.page.getByRole('button', { name: 'Filter' }).click();
            await this.page.waitForTimeout(2000);
        });
    }

    async filterEmployee(employeeId: string, name: string) {
        await allure.step(`Search employee: ${employeeId}`, async () => {
            await this.page.getByRole('button', { name: 'Employees' }).waitFor({ state: 'visible', timeout: 15000 });
            await this.page.getByRole('button', { name: 'Employees' }).click();
            await this.page.getByRole('textbox', { name: 'Search' }).click();
            await this.page.getByRole('textbox', { name: 'Search' }).fill(employeeId);
            await this.page.getByRole('checkbox', { name: `(${employeeId}) ${employeeId}` }).check();
            await this.page.getByRole('button', { name: 'Filter' }).click();
            await this.page.waitForTimeout(2000);
        });
    }

    async generatePDFReport() {
        await allure.step(`Generate PDF Report`, async () => {
            await this.page.locator('#ctrl_selectedExportType').selectOption({ label: 'PDF' });
            await this.page.waitForTimeout(1000);
            await this.page.getByRole('button', { name: '' }).click();
            await this.page.waitForTimeout(10000);
        });
    }
    async openReport() {
        return await allure.step(`Open Report`, async () => {
            const pdfIcon = this.page.getByText('Lön - lönespecifikation XE').first();
            await pdfIcon.waitFor({ state: 'visible' });
            await pdfIcon.click({ button: 'right' });
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

    async verifyValueInPdf(reportPath: string, employee: string = '', title: string = 'Preliminary payment', salaryId: string = '11100Månadslön') {
        await allure.step(`Read Report PDF`, async () => {
            const fulltext = await extractWithPdfReader(reportPath);
            console.log('Extracted Full Text from PDF:', fulltext);
            expect(fulltext.includes(title), `expected value ${title} not found in PDF`).toBe(true);
            expect(fulltext.includes(employee), `expected value ${employee} not found in PDF`).toBe(true);
            expect(fulltext.includes(salaryId), `expected value ${salaryId} not found in PDF`).toBe(true);
        });
    }
}