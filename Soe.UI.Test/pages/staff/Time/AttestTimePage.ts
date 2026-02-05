import { StaffBasePage } from "../StaffBasePage";
import { Page, expect } from "@playwright/test";
import * as allure from "allure-js-commons";
import { log } from "console";
import { SingleGridPageJS } from "pages/common/SingleGridPageJS";

export class AttestTimePage extends StaffBasePage {

    readonly attestTimeGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.attestTimeGrid = new SingleGridPageJS(page, 0, 'directiveCtrl.soeGridOptions.gridOptions');
    }

    async waitforPageLoad() {
        await allure.step("Wait for load", async () => {
            await this.waitForDataLoad('/Time/Time/Attest/Tree/Warnings/');
        });
    }

    async moveToCurrentMonth() {
        await allure.step("Move to current month", async () => {
            const currentMonth = new Date().toLocaleString('default', { month: 'long' });
            const monthLocator = this.page.locator('form[name="dateForm"]').getByText(currentMonth);
            const isCurrentMonthVisible = await monthLocator.isVisible().catch(() => false);
            if (!isCurrentMonthVisible) {
                await this.page.getByTitle('Next', { exact: true }).click();
                await this.page.waitForTimeout(2000);
            }
        });
    }

    async searchEmployee(employeeId: string, name: string) {
        await allure.step(`Search employee: ${employeeId}`, async () => {
            await this.page.getByRole('button', { name: 'Employees' }).waitFor({ state: 'visible', timeout: 15000 });
            await this.page.getByRole('button', { name: 'Employees' }).click();
            await this.page.getByRole('textbox', { name: 'Search' }).click();
            await this.page.getByRole('textbox', { name: 'Search' }).fill(employeeId);
            await this.page.getByRole('checkbox', { name: `(${employeeId})` }).check();
            await this.page.getByRole('button', { name: 'Filter' }).click();
            await this.page.waitForTimeout(2000);
            await this.page.getByText(name).click();
            await this.page.waitForTimeout(3000);
        });
    }
    async checkPunchTime(punchTime: string) {
        await allure.step("Check punch time", async () => {
            await this.page.locator('span.input-group-btn.datepicker-button').click();
            await this.page.getByRole('button', { name: 'Today' }).click();
            const today = new Date();
            const dd = String(today.getDate()).padStart(2, '0');
            const mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
            const yyyy = today.getFullYear();
            const day = today.getDate();
            //const formattedDay = day < 10 ? String(day) : dd;
            const todayDate = `${Number(mm)}/${Number(day)}/${yyyy}`;
            await this.page.locator(`[col-id="date"]:has(.ag-group-value:text-is("${todayDate}")) .ag-group-contracted`).click();
            await this.page.getByText('Punches', { exact: true }).click();
            await this.page.waitForTimeout(3000);
            let swedenPunchTime = '';
            const [time, period] = punchTime.split(' ');
            let [hours, minutes] = time.split(':').map(Number);
            if (period === 'PM' && hours < 12) hours += 12;
            if (period === 'AM' && hours === 12) hours = 0;
            // Create a Date object for today with the punch time in local time zone
            const localDate = new Date(today.getFullYear(), today.getMonth(), today.getDate(), hours, minutes);
            // Convert local time to Sweden time (Europe/Stockholm)
            const swedenTimeString = localDate.toLocaleString('en-US', { timeZone: 'Europe/Stockholm', hour12: false });
            const [datePart, timePart] = swedenTimeString.split(', ');
            let [swedenHours, swedenMinutes] = timePart.split(':');
            swedenHours = String(Number(swedenHours) - 1);
            swedenPunchTime = `${swedenHours.padStart(2, '0')}:${swedenMinutes.padStart(2, '0')}`;
            expect(await this.page.getByRole('textbox', { name: 'HH: MM' }).last().inputValue()).toBe(swedenPunchTime);
            const selectedOption = await this.page.locator('#timeStamp_timeDeviationCauseId option[selected]').last().textContent();
            expect(selectedOption?.trim()).toBe('Standard');
        });
    }

    async clickEditIconByRowIndex(rowIndex: number) {
        await allure.step(`Click edit icon on row ${rowIndex}`, async () => {
            const editButton = this.page.locator(`div[role="row"][row-id="${rowIndex}"] div[col-id='edit'] button[title='Edit']`);
            await editButton.waitFor({ state: 'visible', timeout: 10000 });
            await editButton.click();
            await this.page.waitForTimeout(1000);
        });
    }

    async selectRowByIndex(rowIndex: number) {
        await allure.step(`Select row ${rowIndex}`, async () => {
            const checkbox = this.page.locator(`.ag-pinned-left-cols-container div[role="row"][row-id="${rowIndex}"] input[type="checkbox"]`);
            await checkbox.waitFor({ state: 'visible', timeout: 10000 });
            await checkbox.check({ force: true });
            await this.page.waitForTimeout(500);
        });
    }

    async createPunchesAccordingToSchdule() {
        await allure.step("Create punches according to schedule", async () => {
            const createPunchesButton = this.page.locator('button[data-ng-click="$event.stopPropagation();ctrl.fromSchedule();"]');
            await createPunchesButton.waitFor({ state: 'visible', timeout: 30000 });
            await createPunchesButton.waitFor({ state: 'attached', timeout: 30000 });
            await this.page.waitForTimeout(2000);

            const isEnabled = await createPunchesButton.isEnabled();

            if (isEnabled) {
                await createPunchesButton.click({ force: true });
                await this.page.waitForTimeout(2000);
                await this.page.getByRole('button', { name: 'Save' }).click();
                await this.page.waitForTimeout(2000);
                const salaryIdExpander = this.page.locator("//table[@data-ng-if='ctrl.showTimePayrollTransactionsPermission']//thead//i[@data-ng-click='ctrl.toggleExpanded();']");
                await salaryIdExpander.waitFor({ state: 'visible', timeout: 5000 });
                const salaryRows = this.page.locator("//table[@data-ng-if='ctrl.showTimePayrollTransactionsPermission']//tr[@data-ng-click='ctrl.transactionSelected(trans)']");
                await salaryRows.last().waitFor({ state: 'visible', timeout: 2000 });
                const rowCount = await salaryRows.count();
                expect(rowCount).toBeGreaterThan(0);
                await this.page.getByRole('toolbar').getByTitle('Close').click();
                await this.page.waitForTimeout(2000);
            } else {
                await this.page.getByRole('toolbar').getByTitle('Close').click();
                await this.page.waitForTimeout(2000);
            }
        });
    }

    async clickFunctionButton() {
        await allure.step(`Click function button`, async () => {
            await this.page.getByRole('button', { name: 'Functions' }).click();
            await this.page.waitForTimeout(2000);
        });
    }

    async setAbsence(CauseType: string) {
        await allure.step("Select Absence", async () => {
            await this.page.getByRole('link', { name: 'Absence' }).click();
            await this.page.waitForTimeout(1000);
            await this.page.getByLabel('Cause').click();
            await this.page.getByLabel('Cause').fill(CauseType);
            await this.page.getByLabel('Cause').press('Enter');
            await this.page.waitForTimeout(1000);
            await this.page.getByRole('radio', { name: 'Full days' }).check();
            await this.page.getByRole('button', { name: 'Load/Update Affected Shifts' }).click();
            await this.page.waitForTimeout(2000);
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.page.waitForTimeout(2000);
        });
    }

    async setToAttestationLevel(attestLevel: string = '') {
        await allure.step(`Mark as ${attestLevel}`, async () => {
            await this.page.waitForTimeout(1000);
            const firstRow = this.page.locator(`//div[@role='row' and @row-id='0']//input[@type='checkbox' and contains(@class,'ag-checkbox-input')]`);
            await firstRow.waitFor({ state: 'visible' });
            await firstRow.check();
            const secondRow = this.page.locator(`//div[@role='row' and @row-id='1']//input[@type='checkbox' and contains(@class,'ag-checkbox-input')]`);
            await secondRow.waitFor({ state: 'visible' });
            await secondRow.check();
            await this.page.locator('.col > .ng-isolate-scope > .btn.btn-sm.btn-default.ngSoeSplitButton.dropdown-toggle').click();
            await this.page.waitForTimeout(500);
            await this.page.getByRole('link', { name: attestLevel }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
            await this.page.waitForTimeout(2000);
        });
    }

    async verifyAttestation(attestLevel: string = '') {
        await allure.step(`Verify attestation level changed to ${attestLevel}`, async () => {
            await this.page.waitForTimeout(1000);
            const row1Status = this.page.locator(`//div[@role='row' and @row-id='0'] //div[@col-id="attestStateName"] //span[normalize-space(text())='${attestLevel}']`);
            await row1Status.waitFor({ state: 'visible', timeout: 10000 });
            await expect(row1Status).toBeVisible();

            const row2Status = this.page.locator(`//div[@role='row' and @row-id='1'] //div[@col-id="attestStateName"] //span[normalize-space(text())='${attestLevel}']`);
            await row2Status.waitFor({ state: 'visible', timeout: 10000 });
            await expect(row2Status).toBeVisible();
        });
    }

    async selectRestoreToActiveSchedule() {
        await allure.step("Select Restore to Active Schedule", async () => {
            await this.page.getByRole('link', { name: 'Restore to active schedule' }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async deletePunches() {
        await allure.step("Delete punches", async () => {
            await this.page.getByRole('button', { name: '' }).first().click({ timeout: 2000 });
            await this.page.getByRole('button', { name: '' }).first().click({ timeout: 2000 });
            await this.page.getByRole('button', { name: '' }).first().click({ timeout: 2000 });
            await this.page.getByRole('button', { name: '' }).first().click({ timeout: 2000 });
            await this.page.waitForTimeout(2000);
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.page.waitForTimeout(2000);
            await this.page.getByRole('toolbar').getByTitle('Close').click();
        });
    }

    async setMonthToAttesterad() {
        await allure.step("Select all checkboxes and mark as Attesterad", async () => {
            const selectAllCheckbox = this.page.getByRole('checkbox', { name: 'Press Space to toggle all' });
            await selectAllCheckbox.check();
            await this.page.waitForTimeout(1000);
            await this.page.locator('.col > .ng-isolate-scope > .btn.btn-sm.btn-default.ngSoeSplitButton.dropdown-toggle').click();
            await this.page.getByRole('link', { name: 'Registrerad' }).click();
            await expect(this.page.getByRole('heading', { name: 'Nothing to authorize' })).toBeVisible();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async selectRowByDate(todayDate: string) {
        await allure.step(`Select date: ${todayDate}`, async () => {
            await this.page.waitForTimeout(2000);
            await this.attestTimeGrid.rootLocator.locator('//div[@role="gridcell"]').first().waitFor({ state: 'visible', timeout: 10000 });
            const rowCount = await this.attestTimeGrid.getAgGridRowCount();
            console.log(`Grid loaded with ${rowCount} rows`);
            if (rowCount === 0) {
                throw new Error('Grid has no rows. The attestation data may not have loaded.');
            }
            let foundRowIndex = -1;
            const searchDate = todayDate.trim();
            for (let i = 0; i < rowCount; i++) {
                const dateCell = this.attestTimeGrid.rootLocator.locator(`//div[@row-index='${i}']//div[@col-id='date']`);
                const dateText = (await dateCell.textContent())?.trim() || '';
                if (dateText === searchDate) {
                    foundRowIndex = i;
                    console.log(`Found date "${searchDate}" at rowIndex: ${i}`);
                    break;
                }
            }
            if (foundRowIndex === -1) {
                throw new Error(`Row with date "${todayDate}" not found in ${rowCount} rows`);
            }
            await this.attestTimeGrid.selectCheckBox(foundRowIndex);
            console.log(`Selected checkbox for rowIndex: ${foundRowIndex}`);
        });
    }

    async verifyAttestLevelForDate(todayDate: string, expectedLevel: string) {
        await allure.step(`Verify attest level for date ${todayDate}: ${expectedLevel}`, async () => {
            const rowCount = await this.attestTimeGrid.getAgGridRowCount();
            for (let i = 0; i < rowCount; i++) {
                const dateText = (await this.attestTimeGrid.rootLocator.locator(`//div[@row-index='${i}']//div[@col-id='date']`).textContent())?.trim();
                if (dateText === todayDate.trim()) {
                    const attestLevel = (await this.attestTimeGrid.rootLocator.locator(`//div[@row-index='${i}']//div[@col-id='attestStateName']`).textContent())?.trim();
                    if (attestLevel !== expectedLevel) {
                        throw new Error(`Expected "${expectedLevel}" but found "${attestLevel}" for date ${todayDate} at row ${i}`);
                    }
                    console.log(`Verified row ${i} (${todayDate}): ${expectedLevel}`);
                    return;
                }
            }
            throw new Error(`Row with date ${todayDate} not found`);
        });
    }

    async setToAttestationLevelSelectedRow(attestLevel: string) {
        await allure.step(`Mark selected row as ${attestLevel}`, async () => {
            await this.page.locator('.col > .ng-isolate-scope > .btn.btn-sm.btn-default.ngSoeSplitButton.dropdown-toggle').click();
            await this.page.waitForTimeout(500);
            await this.page.getByRole('link', { name: attestLevel, exact: true }).click();
            await this.clickAlertMessage('OK');
            await this.page.waitForTimeout(2000);
        });
    }
}