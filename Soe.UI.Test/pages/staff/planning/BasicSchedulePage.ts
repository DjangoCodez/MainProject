import { StaffBasePage } from "../StaffBasePage";
import { expect, Locator, Page } from "@playwright/test";
import * as allure from "allure-js-commons";
import { extractWithPdfReader } from "utils/CommonUtil";


export class BasicSchedulePage extends StaffBasePage {

    constructor(page: Page) {
        super(page);
    }

    async waitforPageLoad() {
        await allure.step("Wait for load", async () => {
            await this.waitForDataLoad('/api/Time/Schedule/TemplateShift/Search/');
        });
    }

    async chooseFilterAndEmployee(employeeId: string) {
        await allure.step("Choose filter & employee", async () => {
            await this.page.getByTitle('Selection', { exact: true }).waitFor({ state: 'visible', timeout: 15_000 });
            await this.page.getByTitle('Selection', { exact: true }).click();
            await this.page.getByRole('button', { name: 'Employees' }).click();
            await this.page.getByRole('textbox', { name: 'Search ...' }).click();
            await this.page.getByRole('textbox', { name: 'Search ...' }).fill(employeeId);
            await this.page.getByRole('checkbox', { name: `(${employeeId})` }).check();
            await this.page.getByRole('button', { name: 'Filter' }).click();
        });
    }

    async filterMultiEmployees(employeeIds: string[]) {
        await allure.step("Filter multiple employees", async () => {
            await this.page.getByTitle('Selection', { exact: true }).waitFor({ state: 'visible', timeout: 15_000 });
            await this.page.getByTitle('Selection', { exact: true }).click({ force: true, delay: 100 });
            const employeeFilterButton = this.page.getByRole('button', { name: 'Employees' })
            await employeeFilterButton.waitFor({ state: 'visible', timeout: 5000 });
            await employeeFilterButton.click();
            const checkboxAll = await this.page.locator('//ul/li/a[@role="menuitem"]').all();
            const count = checkboxAll.length;
            await expect.poll(async () => count, { timeout: 10000 }).toBeGreaterThan(0);
            for (const empId of employeeIds) {
                for (const checkbox of checkboxAll) {
                    const checkboxText = await checkbox.textContent();
                    if (checkboxText && checkboxText.includes(empId)) {
                        const checkboxLocator = checkbox.locator('input[type="checkbox"]');
                        await checkboxLocator.check();
                        break;
                    }
                }
            }
            await this.page.getByRole('button', { name: 'Filter' }).click();
            const emps = this.page.locator(`//tbody[@id="rowsTarget"]/tr`);
            await expect.poll(async () => await emps.count(), { timeout: 10000 }).toBe(0);
        });
    }
    async clickViewAll() {
        await allure.step("Click view all", async () => {
            await this.page.getByRole('checkbox', { name: 'View all' }).click();
        });
    }

    async addNewScheduleMonday() {
        await allure.step("Add new schedule for Monday", async () => {
            await this.page.locator('.shift-drop-zone').first().click({ button: 'right' });
            await this.page.getByRole('link', { name: '+ New basic schedule' }).click();
            const model = this.page.locator('.modal-content');
            await model.waitFor({ state: 'visible', timeout: 5000 });
            await model.getByRole('textbox', { name: 'Weeks' }).fill('2');
            await this.page.waitForTimeout(2000);
            await model.locator('button[title="Save"]').click();
            await this.page.waitForTimeout(5000);
        });
    }

    async addNewBasicSchedule() {
        await allure.step("Add new basic schedule", async () => {
            await this.page.locator('.shift-drop-zone').first().click({ button: 'right' });
            await this.page.getByRole('link', { name: '+ New basic schedule' }).click();
            const model = this.page.locator('.modal-content');
            await model.waitFor({ state: 'visible', timeout: 5000 });
            await model.getByRole('textbox', { name: 'Copy From' }).click();
            await model.getByRole('textbox', { name: 'Copy From' }).clear();
            await model.getByRole('textbox', { name: 'Copy From' }).fill('130');
            await model.getByRole('textbox', { name: 'Copy From' }).press('Enter');
            await this.page.waitForTimeout(1000);
            await this.page.getByLabel('Basic schedule').selectOption({ index: 1 });
            await this.page.waitForTimeout(2000);
            await model.locator('button[title="Save"]').click();
            await this.page.waitForTimeout(5000);
        });
    }
    async addBasicSchedule({ weeks = 1, startDate = '', weekInCycle = 1, copyFromEmployee = '', basicSchedule = '' }: { weeks?: number, startDate?: string, weekInCycle?: number, copyFromEmployee?: string, basicSchedule?: string } = {}) {
        await allure.step("Add new basic schedule", async () => {
            await this.page.locator('[label-key="time.schedule.planning.templateschedule.headtitle"]').waitFor({ state: 'visible', timeout: 5000 });
            if (weeks > 1) {
                const week = this.page.locator('#ctrl_nbrOfWeeks');
                await week.fill(weeks.toString());
                await week.press('Enter');
                await this.page.waitForTimeout(1000);
            } if (startDate) {
                const startDateInput = this.page.locator('#ctrl_templateHead_startDate');
                await startDateInput.fill(startDate);
                await startDateInput.press('Enter');
            }
            if (weekInCycle > 1) {
                await this.page.locator('#ctrl_weekInCycle').selectOption(weekInCycle.toString());
            }
            if (copyFromEmployee) {
                const copyFromInput = this.page.locator('#ctrl_copyFromEmployeeId');
                await copyFromInput.fill(copyFromEmployee);
                await copyFromInput.press('Enter');
            }
            if (basicSchedule) {
                await this.page.locator('#ctrl_copyFromTemplateHeadId').selectOption({ label: basicSchedule });
            }
            await this.page.locator('//div[@class="modal-footer"]/button[text()="Save"]').click()
            await this.waitForDataLoad('/api/Time/Schedule/TimeScheduleTemplateHead/SaveTimeScheduleTemplateAndPlacement/')
            await this.page.locator('.planning-day-first-day-of-template div div i').first().waitFor({ state: 'visible', timeout: 10000 });
        });
    }

    async viewAllEmployees(expectedCount: number = 1) {
        await allure.step("View all employees", async () => {
            const isChecked = await this.page.locator('#ctrl_showAllEmployees').isChecked();
            if (!isChecked) {
                await this.page.locator('#ctrl_showAllEmployees').check();
            }
            const rows = this.page.locator('//tbody[@id="rowsTarget"]/tr')
            await expect.poll(async () => await rows.count(), { timeout: 10000 }).toBe(expectedCount);
        });
    }

    async getEmpoyeeIds(employeeName: string) {
        const rows = await this.page.locator('//tbody[@id="rowsTarget"]/tr').all();
        for (const row of rows) {
            let empName = await row.locator('td span[class=name]').innerText();
            const rowEmpId = await row.getAttribute('id')
            console.log(`Employee Name: ${empName}, Employee ID: ${rowEmpId}`);
            if (empName.trim().includes(employeeName)) {
                return rowEmpId;
            }
        }
    }

    async addNewShift(...shifts: { shiftFrom: string, shiftTo: string, shiftType: string, breakFrom?: string, breakTo?: string, isNewShift?: boolean, shiftIndex?: number, fillGapsWithBreaks?: boolean }[]) {
        for (const shift of shifts) {
            await this.addShift(shift);
        }
        await this.page.locator('//div[@main-button="true"]/button').nth(0).click();
        await this.page.locator('//div[contains(@class, "modal-footer")]/button[text()="Yes"]').click()
        const isSaveAndActiveViewVisible = await this.page.locator('//div[@class="table-outer-bordered"]').
            waitFor({ state: 'visible', timeout: 5000 }).
            then(() => true).
            catch(() => false);
        if (isSaveAndActiveViewVisible) {
            const checkbox = this.page.locator('//tr/td/i[contains(@class,"fa-square")]');
            await checkbox.waitFor({ state: 'visible', timeout: 5000 });
            await checkbox.click();
            await this.page.locator('//div[@class="modal-footer"]/button[text()="Save"]').click()
        }
    }

    private async addShift({ shiftFrom, shiftTo, shiftType, breakFrom, breakTo, isNewShift = false, shiftIndex = 0, fillGapsWithBreaks = false }: { shiftFrom: string, shiftTo: string, shiftType: string, breakFrom?: string, breakTo?: string, isNewShift?: boolean, shiftIndex?: number, fillGapsWithBreaks?: boolean }) {
        await allure.step("Add new shift", async () => {
            const breakButton = this.page.getByRole('button', { name: 'New break' });
            await breakButton.waitFor({ state: 'visible', timeout: 5000 });
            let shifts = await this.page.locator('[label-key="time.schedule.planning.shifts"] #shift_actualStartTime').count();
            const breaks = await this.page.locator('[label-key="time.schedule.planning.breaks"] #brk_actualStartTime').count();
            // Add new shift
            if (isNewShift) {
                await this.page.getByRole('button', { name: 'New shift' }).click();
            } else {
                shifts = shiftIndex;
            }
            const shiftStartTime = this.page.locator("#shift_actualStartTime").nth(shifts);
            await shiftStartTime.waitFor({ state: 'visible', timeout: 5000 });
            await shiftStartTime.fill(shiftFrom);
            await this.page.waitForTimeout(1000);
            await shiftStartTime.press('Tab');
            const shiftEndTime = this.page.locator("#shift_actualStopTime").nth(shifts);
            await shiftEndTime.fill(shiftTo);
            await this.page.waitForTimeout(1000);
            await shiftEndTime.press('Tab');
            const shiftTypeDropdown = this.page.locator('#shift_shiftTypeId');
            await shiftTypeDropdown.nth(shifts).selectOption({ label: shiftType });
            // Add break if provided
            if (breakFrom && breakTo && !fillGapsWithBreaks) {
                await breakButton.click();
                const breakStartTime = this.page.locator('#brk_actualStartTime').nth(breaks);
                await breakStartTime.waitFor({ state: 'visible', timeout: 5000 });
                await breakStartTime.fill(breakFrom);
                await this.page.waitForTimeout(1000);
                await breakStartTime.press('Tab');
                const breakStopTime = this.page.locator('#brk_actualStopTime').nth(breaks)
                await breakStopTime.fill(breakTo);
                await this.page.waitForTimeout(1000);
                await breakStopTime.press('Tab');
            }
            // Fill gaps with breaks if not specified
            if (fillGapsWithBreaks) {
                await this.fillGapsWithBreaks();
                const breakStartTime = this.page.locator('#brk_actualStartTime');
                const breakStopTime = this.page.locator('#brk_actualStopTime');
                const breakStarts = await breakStartTime.nth(0).inputValue();
                const breakEnds = await breakStopTime.nth(0).inputValue();
                await expect.poll(async () => await breakStartTime.count(), { timeout: 10000 }).toBe(1);
                await expect.poll(async () => await breakStopTime.count(), { timeout: 10000 }).toBe(1);
                expect(breakStarts).toBe(breakFrom);
                expect(breakEnds).toBe(breakTo);
            }
        });
    }

    private async fillGapsWithBreaks() {
        await allure.step("Fill gaps with breaks", async () => {
            await this.page.locator('.modal-footer [label-key="core.functions"] button').click();
            await this.page.locator('//a[contains(text(),"Fill gaps with breaks")]').click();
        });
    }


    async selectShifts({ empId, from, to }: { empId: string, from: { week: number, date: string }, to: { week: number, date: string } }) {
        return await allure.step("Select shifts", async () => {
            const firstDay = 7 * (from.week - 1) + this.gettDayIndex(from.date);
            const lastDay = 7 * (to.week - 1) + this.gettDayIndex(to.date);

            console.log(`Selecting shifts for employee ID: ${empId} from day index ${firstDay} to ${lastDay}`);


            const modifier = process.platform === 'darwin' ? 'Meta' : 'Control';
            const scheduleDays = await this.page.locator(`//tr[@id="${empId}"]/td/div[@class="shift-day"]`).all();
            const shifts = []
            for (let i = firstDay; i <= lastDay; i++) {
                const count = await scheduleDays[i].locator('.planning-shift').count();
                if (count > 0) {
                    shifts.push(scheduleDays[i]);
                }
            }
            await this.page.keyboard.down(modifier);
            for (let i = shifts.length - 1; i >= 0; i--) {
                await shifts[i].locator('.planning-shift').first().click();
                await this.page.waitForTimeout(500);
            }
            await this.page.keyboard.up(modifier);
            console.log(`Selected ${shifts.length} shifts for employee ID: ${empId}`);
            return shifts
        });
    }

    async dragAndDropShift(shift: Locator, to: { week: number, date: string }) {
        return await allure.step("Drag and drop shift", async () => {
            const dates = this.page.locator('//td[contains(@class,"planning-day-has-employee-schedule")]/div');
            const date = 7 * (to.week - 1) + this.gettDayIndex(to.date);
            const target = dates.nth(date);
            await this.dragAndDrop(shift, target);
        });
    }

    private gettDayIndex(day: string) {
        const dayIndex = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"]
        return dayIndex.indexOf(day);

    }

    async copyMoveEmployeeShiftsByDragDrop({ empId, source, target, action = 'copy', expectedCount }:
        {
            empId: string,
            source: { from: { week: 1 | 2 | 3 | 4, date: string }, to: { week: 1 | 2 | 3 | 4, date: string } },
            target: { to: { week: 1 | 2 | 3 | 4 | 5, date: string } },
            action?: 'copy' | 'move',
            expectedCount?: number
        }) {
        return await allure.step("Copy/Move shifts", async () => {
            const shift = await this.selectShifts({ empId, from: source.from, to: source.to });
            await this.dragAndDropShift(shift[0], target.to);
            await this.page.locator('.modal-content').waitFor({ state: 'visible', timeout: 5000 });
            const sourceShifts = await this.getSourceShifts();
            await this.copyOrMove(action);
            const shifts = this.page.locator('.planning-shift')
            await expect.poll(async () => await shifts.count(), { timeout: 10000 }).toBe(expectedCount || shift.length * 2);
            await this.page.waitForTimeout(3000);
            return sourceShifts;
        });
    }

    private async getSourceShifts() {
        const shifts = this.page.locator('//div[@class="modal-body"]//tr');
        const shiftIds: Array<{ date: string; shiftTime: string; shiftType: string }> = [];
        const record: Record<number, keyof typeof shiftIds[number]> = {
            0: 'date',
            1: 'shiftTime',
            2: 'shiftType',
        };
        const count = await shifts.count();
        for (let i = 0; i < count; i++) {
            const shiftData = { date: '', shiftTime: '', shiftType: '' };
            const spans = await shifts.nth(i).locator('td>span').count()
            for (let j = 0; j < spans; j++) {
                const cellText = await shifts
                    .nth(i)
                    .locator('td>span')
                    .nth(j)
                    .innerText();
                const field = (spans < 3)
                    ? record[j + 1]
                    : record[j];
                shiftData[field] = cellText.trim();
            }
            shiftIds.push(shiftData);
        }
        return shiftIds;
    }


    private async copyOrMove(action: 'copy' | 'move') {
        await allure.step("Copy or Move shifts", async () => {
            if (action === 'copy') {
                await this.page.locator('//label[text()="Copy"]').click();
            } else {
                await this.page.locator('//label[text()="Move"]').click();
            }
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.waitForDataLoad('/api/Time/Schedule/TemplateShift/Search/')
        });
    }

    private async dragAndDrop(source: Locator, target: Locator) {
        await allure.step("Drag and drop element", async () => {
            await expect(source).toBeVisible();
            await expect(target).toBeVisible();
            await source.scrollIntoViewIfNeeded();
            await target.scrollIntoViewIfNeeded();
            const sourceBox = await source.boundingBox();
            const targetBox = await target.boundingBox();
            if (!sourceBox || !targetBox) {
                throw new Error('Source or target element not visible for drag and drop');
            }
            const sourceX = sourceBox.x + sourceBox.width / 5;
            const sourceY = sourceBox.y + sourceBox.height / 5;
            const targetX = targetBox.x + targetBox.width / 2;
            const targetY = targetBox.y + targetBox.height / 2;
            await this.page.mouse.move(sourceX, sourceY);
            await this.page.waitForTimeout(500);
            await this.page.mouse.down();
            await this.page.waitForTimeout(500);
            await this.page.mouse.move(sourceX, sourceY);
            await this.page.waitForTimeout(500);
            await this.page.mouse.move(targetX, targetY, { steps: 30 });
            await this.page.waitForTimeout(500);
            await this.page.mouse.up();
            await this.page.waitForTimeout(1000);
        });
    }

    async changeVisiblePeriod(weeks: string) {
        await allure.step("Change visible period", async () => {
            const dropdown = this.page.locator('#ctrl_selectedVisibleDays');
            await dropdown.selectOption(weeks);
            await this.page.waitForTimeout(3000);
        });
    }

    async newShiftToday(shiftStartTime: string = '08:00', shiftEndTime: string = '16:00', breakStartTime: string = '12:00', breakEndTime: string = '13:00') {
        await allure.step("Add new shift for today", async () => {
            const today = new Date();
            const dayOfWeek = today.getDay();
            const year = today.getFullYear();
            const month = String(today.getMonth() + 1).padStart(2, '0');
            const day = String(today.getDate()).padStart(2, '0');
            const formattedDate = `'${year}|${month}|${day}|00|00'`;
            await this.page.locator(`//td//div[@date="${formattedDate}"]`).click({ button: 'right' });
            await this.page.getByRole('link', { name: '+ New shift' }).click();
            const model = this.page.locator('.modal-content');
            await model.waitFor({ state: 'visible', timeout: 5000 });
            await model.locator('#shift_actualStartTime').fill(shiftStartTime);
            await model.waitFor({ state: 'visible', timeout: 500 });
            await model.locator('#shift_actualStartTime').press('Tab');
            await model.locator('#shift_actualStopTime').fill(shiftEndTime);
            await model.locator('#shift_actualStopTime').press('Tab');
            await model.locator('#shift_shiftTypeId').selectOption({ index: 1 });
            await this.page.waitForTimeout(2000);
            await model.getByRole('button', { name: 'New break' }).click();
            await model.locator('#brk_actualStartTime').fill(breakStartTime);
            await model.locator('#brk_actualStartTime').press('Tab');
            await model.locator('#brk_actualStopTime').fill(breakEndTime);
            await model.locator('#brk_actualStopTime').press('Tab');
            await this.page.waitForTimeout(3000);
            await model.getByRole('button', { name: 'Save' }).click();
            await this.page.waitForTimeout(1000);
            await model.getByRole('button', { name: 'Ok' }).click();
            await this.page.waitForTimeout(3000);
        });
    }

    async selectWeeks(weeks: string) {
        await allure.step("Select weeks", async () => {
            const dropdown = this.page.locator('#ctrl_selectedVisibleDays');
            await dropdown.selectOption(weeks);
            await this.page.waitForTimeout(3000);
        });
    }

    async verifyRepeatingShift(numberOfWeeks: number) {
        await allure.step("Verify repeating shift", async () => {
            const today = new Date();
            const dayOfWeek = today.getDay();
            const year = today.getFullYear();
            const month = String(today.getMonth() + 1).padStart(2, '0');
            const day = String(today.getDate()).padStart(2, '0');
            const futureDate = new Date();
            futureDate.setDate(today.getDate() + numberOfWeeks * 7);
            const futureYear = futureDate.getFullYear();
            const futureMonth = String(futureDate.getMonth() + 1).padStart(2, '0');
            const futureDay = String(futureDate.getDate()).padStart(2, '0');
            const formattedDate2Weeks = `'${futureYear}|${futureMonth}|${futureDay}|00|00'`;
            await expect(
                this.page.locator(`//td//div[@date="${formattedDate2Weeks}"]/i[@title="Repeating day"]`)
            ).toBeVisible();
        });
    }

    async verifyActiveScheduleRepeatingShift(numberOfWeeks: number, type: string) {
        await allure.step("Verify repeating shift", async () => {
            const today = new Date();
            const dayOfWeek = today.getDay();
            const year = today.getFullYear();
            const month = String(today.getMonth() + 1).padStart(2, '0');
            const day = String(today.getDate()).padStart(2, '0');
            const futureDate = new Date();
            futureDate.setDate(today.getDate() + numberOfWeeks * 7);
            const futureYear = futureDate.getFullYear();
            const futureMonth = String(futureDate.getMonth() + 1).padStart(2, '0');
            const futureDay = String(futureDate.getDate()).padStart(2, '0');
            const formattedDate2Weeks = `'${futureYear}|${futureMonth}|${futureDay}|00|00'`;
            await expect(
                this.page.locator(`//td//div[@date="${formattedDate2Weeks}"]/div`)
            ).toContainText(type);
        });
    }

    async activateBasicSchedule() {
        await allure.step("Activate basic schedule", async () => {
            await this.page.getByTitle('First day of basic schedule \'').click({
                button: 'right'
            });
            await this.page.getByRole('link', { name: ' Activate' }).click();
            await this.page.getByRole('button', { name: '' }).nth(1);
            const now = new Date();
            const day = now.getDay();
            const diff = (day === 0 ? -6 : 1 - day); // Monday is 1, Sunday is 0
            const monday = new Date(now);
            monday.setDate(now.getDate() + diff);
            const mm = String(monday.getMonth() + 1).padStart(2, '0');
            const dd = String(monday.getDate()).padStart(2, '0');
            const yy = String(monday.getFullYear()).slice(-2);
            const mondayFormatted = `${mm}/${dd}/${yy}`;
            await this.page.getByRole('tabpanel', { name: 'Activate basic schedule ' }).getByLabel('Start date').fill(mondayFormatted);
            const endDate = new Date(monday);
            endDate.setDate(monday.getDate() + 8 * 7);
            const endMm = String(endDate.getMonth() + 1).padStart(2, '0');
            const endDd = String(endDate.getDate()).padStart(2, '0');
            const endYy = String(endDate.getFullYear()).slice(-2);
            const endMondayFormatted = `${endMm}/${endDd}/${endYy}`;
            await this.page.getByRole('textbox', { name: 'End date' }).fill(endMondayFormatted);
            // mondayFormatted contains the current week's Monday in mm/dd/yy format
            await this.page.getByRole('checkbox', { name: 'Preliminary' }).uncheck();
            await this.page.getByRole('button', { name: 'Activate', exact: true }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async getEmployeeEndDate(): Promise<string> {
        const now = new Date();
        const oneMonthAfter = new Date(now);
        oneMonthAfter.setMonth(now.getMonth() + 1);
        const mm = String(oneMonthAfter.getMonth() + 1).padStart(2, '0');
        const dd = String(oneMonthAfter.getDate()).padStart(2, '0');
        const yy = String(oneMonthAfter.getFullYear()).slice(-2);
        return `${mm}/${dd}/${yy}`;
    }

    async activateBasicSchedulewithEndDate() {
        await allure.step("Activate basic schedule", async () => {
            await this.page.getByTitle('First day of basic schedule \'').click({
                button: 'right'
            });
            await this.page.getByRole('link', { name: ' Activate' }).click();
            await this.page.getByRole('button', { name: '' }).nth(1);
            const now = new Date();
            const day = now.getDay();
            const diff = (day === 0 ? -6 : 1 - day); // Monday is 1, Sunday is 0
            const monday = new Date(now);
            monday.setDate(now.getDate() + diff);
            const mm = String(monday.getMonth() + 1).padStart(2, '0');
            const dd = String(monday.getDate()).padStart(2, '0');
            const yy = String(monday.getFullYear()).slice(-2);
            const mondayFormatted = `${mm}/${dd}/${yy}`;

            await this.page.getByRole('tabpanel', { name: 'Activate basic schedule ' }).getByLabel('Start date').fill(mondayFormatted);
            const endDate = await this.getEmployeeEndDate();
            await this.page.getByRole('textbox', { name: 'End date' }).clear();
            await this.page.getByRole('textbox', { name: 'End date' }).fill(endDate);
            await this.page.getByRole('checkbox', { name: 'Preliminary' }).uncheck();
            await this.page.getByRole('button', { name: 'Activate', exact: true }).click();
            const okButton = this.page.getByRole('button', { name: 'OK' });
            if (await okButton.isVisible({ timeout: 5000 }).catch(() => false)) {
                await okButton.click();
            }
        });
    }

    async verifyScheduleActivated() {
        await allure.step("Verify schedule activated", async () => {
            const today = new Date();
            const tomorrow = new Date(today);
            tomorrow.setDate(today.getDate() + 1);
            const year = tomorrow.getFullYear();
            const month = String(tomorrow.getMonth() + 1).padStart(2, '0');
            const day = String(tomorrow.getDate()).padStart(2, '0');
            const formattedDate = `'${year}|${month}|${day}|00|00'`;
            const dateRow = this.page.locator(`//td//div[@date="${formattedDate}"]`);
            const parentRow = dateRow.locator('..').locator('..');
            console.log(`Verifying schedule activation for date: ${formattedDate}`);
            console.log(`Parent row title attribute: ${await parentRow.getAttribute('title')}`);
            await this.page.waitForTimeout(2000);
            const popupButton = this.page.getByRole('button', { name: 'OK' });
            if (await popupButton.isVisible({ timeout: 2000 }).catch(() => false)) {
                await popupButton.click();
            }
            await expect(parentRow).toHaveAttribute('title', 'The day is activated');
        });
    }

    async selectActiveSchedule() {
        await allure.step("Select active schedule", async () => {
            await this.page.locator('.fal.fa-chevron-down.margin-small-left').click();
            await this.page.locator('#ng-app-bootstrap-element').getByText('Active schedule', { exact: true }).click();
            await this.waitForDataLoad('api/Time/Schedule/StaffingNeeds/UnscheduledTaskDates/');
        });
    }

    async isScheduleActivated() {
        return await allure.step("Verify schedule activated", async () => {
            const today = new Date();
            const dayOfWeek = today.getDay();
            const year = today.getFullYear();
            const month = String(today.getMonth() + 1).padStart(2, '0');
            const day = String(today.getDate()).padStart(2, '0');
            const formattedDate = `'${year}|${month}|${day}|00|00'`;
            const dateRow = this.page.locator(`//td//div[@date="${formattedDate}"]`);
            const parentRow = dateRow.locator('..').locator('..');
            const isActivated = await parentRow.getAttribute('title') === 'The day is activated';
            return isActivated;
        });
    }

    async isBasicSchedulePresent() {
        return await allure.step("Verify basic schedule present", async () => {
            const basicScheduleElement = this.page.getByTitle('First day of basic schedule \'');
            return await basicScheduleElement.count() > 0;
        });
    }

    async removeBasicScheduleIfPresent() {
        await allure.step("Remove basic schedule if present", async () => {
            if (await this.isBasicSchedulePresent()) {
                await this.page.getByTitle('First day of basic schedule \'').click({
                    button: 'right'
                });
                await this.page.getByRole('link', { name: ' Edit basic schedule' }).click();
                await this.page.getByRole('button', { name: 'Remove' }).click();
                await this.page.getByRole('button', { name: 'OK' }).click();
                await this.page.waitForTimeout(3000);
            }
        });
    }

    async deactivateSchedule() {
        await allure.step("Deactivate schedule", async () => {
            await this.page.getByRole('button', { name: 'Functions' }).click();
            await this.page.waitForTimeout(1000);
            await this.page.getByRole('link', { name: '   Activate' }).click();
            await this.page.waitForTimeout(3000);
            const currentYear = new Date().getFullYear().toString();
            const model = this.page.locator('.modal-content');
            const gridCells = await model.getByRole('gridcell').all();
            for (const cell of gridCells) {
                const cellName = await cell.textContent();
                if (cellName && cellName.includes(currentYear)) {
                    await model.getByRole('button', { name: '' }).click();
                    await this.page.waitForTimeout(1000);
                    await model.getByRole('button', { name: 'OK' }).click();
                    await this.page.waitForTimeout(3000);
                    if (await model.getByRole('button', { name: 'Reverse activation' }).isVisible()) {
                        await model.getByRole('button', { name: 'Reverse activation' }).click();
                        await model.getByRole('button', { name: 'OK' }).click();
                    }
                    await model.locator('//button[@class="close"]').click();
                    await this.page.waitForTimeout(3000);
                    break;
                }
            }

        });
    }

    async dragDropShiftToNextDay() {
        await allure.step("Drag and drop shift", async () => {
            // Implement drag and drop logic here
            const today = new Date();
            const dayOfWeek = today.getDay();
            const year = today.getFullYear();
            const month = String(today.getMonth() + 1).padStart(2, '0');
            const day = String(today.getDate()).padStart(2, '0');
            const formattedDate = `'${year}|${month}|${day}|00|00'`;
            const nextDay = new Date(today);
            nextDay.setDate(today.getDate() + 1);
            const nextYear = nextDay.getFullYear();
            const nextMonth = String(nextDay.getMonth() + 1).padStart(2, '0');
            const nextDate = String(nextDay.getDate()).padStart(2, '0');
            const formattedNextDate = `'${nextYear}|${nextMonth}|${nextDate}|00|00'`;
            await this.page.locator(`//td//div[@date="${formattedDate}"]/div`).click();
            await this.page.waitForTimeout(1000);
            await this.page.mouse.down();
            await this.page.waitForTimeout(1000);
            await this.page.locator(`//td//div[@date="${formattedNextDate}"]`).hover();
            await this.page.waitForTimeout(1000);
            await this.page.mouse.up();
            await this.page.getByRole('button', { name: 'Save' }).click();
        });
    }

    async openNextDayShiftDetails() {
        await allure.step("Open shift details", async () => {
            const today = new Date();
            const nextDay = new Date(today);
            nextDay.setDate(today.getDate() + 1);
            const nextYear = nextDay.getFullYear();
            const nextMonth = String(nextDay.getMonth() + 1).padStart(2, '0');
            const nextDate = String(nextDay.getDate()).padStart(2, '0');
            const formattedNextDate = `'${nextYear}|${nextMonth}|${nextDate}|00|00'`;
            const dateRow = this.page.locator(`//td//div[@date="${formattedNextDate}"]/div`);
            await dateRow.dblclick();
        });
    }


    async verifyLengthAndBreaks(shiftStartTime: string, shiftEndTime: string, shiftLength: string, breakStartTime: string, breakEndTime: string) {
        await allure.step("Verify length & breaks", async () => {
            const model = this.page.locator('.modal-content');
            await model.locator('#shift_actualStartTime');
            await expect(model.locator('#shift_actualStartTime')).toHaveValue(shiftStartTime);
            await expect(model.locator('#shift_actualStopTime')).toHaveValue(shiftEndTime);
            await expect(model.locator('[id="shift_duration | minutesToTimeSpan"]')).toHaveValue(shiftLength);
            await expect(model.locator('#brk_actualStartTime')).toHaveValue(breakStartTime);
            await expect(model.locator('#brk_actualStopTime')).toHaveValue(breakEndTime);
        });
    }

    async setShiftEndTime(shiftEndTime: string) {
        await allure.step("Set shift end time", async () => {
            const model = this.page.locator('.modal-content');
            await model.locator('#shift_actualStopTime').fill(shiftEndTime);
            await model.locator('#shift_actualStopTime').press('Tab');
            await this.page.waitForTimeout(2000);
        });
    }

    async setShiftType(shiftTypeIndex: number) {
        await allure.step("Set shift type", async () => {
            const model = this.page.locator('.modal-content');
            await model.locator('#shift_shiftTypeId').selectOption({ index: shiftTypeIndex });
        });
    }

    async saveShiftDetails() {
        await allure.step("Save shift details", async () => {
            const model = this.page.locator('.modal-content');
            await model.getByRole('button', { name: 'Save' }).click();
            await this.page.waitForTimeout(3000);
        });
    }

    async printActiveSchedule(reportName: string) {
        await allure.step("Print active schedule", async () => {
            await this.page.getByRole('button', { name: 'Functions' }).click();
            await this.page.waitForTimeout(1000);
            await this.page.getByRole('link', { name: '   Print active schedule for' }).click();
            await this.page.waitForTimeout(1000);
            const model = this.page.locator('.modal-content');
            await model.waitFor({ state: 'visible', timeout: 5000 });
            const rows = await model.locator('tbody tr').all();
            for (const row of rows) {
                const tdWithReportName = await row.locator('td', { hasText: reportName });
                if (await tdWithReportName.count() > 0) {
                    await row.locator('td span.fa-print').click();
                    await this.page.waitForTimeout(10000);
                    break;
                }
            }
        })
    }

    async openReport() {
        return await allure.step(`Open Report`, async () => {
            const pdfIcon = this.page.getByText('Schema - Veckoschema med färg').first();
            //const pdfIcon = this.page.locator("//table[contains(@class,'table-condensed')]//tr[td[normalize-space()='Huvudbok 2dim']]//i[contains(@class,'fa-file-pdf')]");
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

    async verifyValueInPdf(reportPath: string, typeOne: string, typeTwo: string, title: string = '640 Schema - Veckoschema med färg', changedTime: string = '08:00-18:00') {
        await allure.step(`Read Report PDF`, async () => {
            const fulltext = await extractWithPdfReader(reportPath);
            expect(fulltext.includes(typeOne), `expected value ${typeOne} not found in PDF`).toBe(true);
            expect(fulltext.includes(typeTwo), `expected value ${typeTwo} not found in PDF`).toBe(true);
            expect(fulltext.includes(title), `expected value ${title} not found in PDF`).toBe(true);
            expect(fulltext.includes(changedTime), `expected value ${changedTime} not found in PDF`).toBe(true);
        });
    }

    private async getEmployeeRow({ empId, empName, index }: { empId: string, empName: string, index: number }) {
        const emps = this.page.locator(`//tbody[@id="rowsTarget"]/tr`);
        if (index >= 0) {
            const emp = emps.nth(index);
            return emp.locator('td').nth(0)
        }
        else {
            const rows = emps.all()
            for (const row of await rows) {
                const empDetails = await row.locator('td').nth(0).innerText();
                if (empDetails.includes(empId) && empDetails.includes(empName)) {
                    return row.locator('td').nth(0);
                }
            }
        }
    }

    async editEmployee({ empId = '', empName = '', index = 0 }: { empId?: string, empName?: string, index?: number }) {
        await allure.step(`Edit Employee ${empId} - ${empName}`, async () => {
            const empRow = await this.getEmployeeRow({ empId, empName, index });
            if (!empRow) {
                throw new Error(`Employee row not found for empId: ${empId}, empName: ${empName}, index: ${index}`);
            }
            await empRow.dblclick();
            await this.page.locator('#ctrl_employee_active').waitFor({ state: 'visible', timeout: 5000 });
        });
    }

    private formatWeekRange(date: string) {
        const d = new Date(new Date(date).getUTCFullYear(), new Date(date).getUTCMonth(), new Date(date).getUTCDate());
        // Move to Monday of the same ISO week
        const day = d.getUTCDay() || 7; // Sunday = 7
        d.setUTCDate(d.getUTCDate() + 2);

        const start = new Date(d);
        const end = new Date(d);
        end.setUTCDate(start.getUTCDate() + 6);

        const dayNames = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
        const monthNames = [
            'January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'
        ];

        const getISOWeek = (date: Date) => {
            const temp = new Date(Date.UTC(
                date.getUTCFullYear(),
                date.getUTCMonth(),
                date.getUTCDate()
            ));
            temp.setUTCDate(temp.getUTCDate() + 4 - (temp.getUTCDay() || 7));
            const yearStart = new Date(Date.UTC(temp.getUTCFullYear(), 0, 1));
            return Math.ceil((((temp.getTime() - yearStart.getTime()) / 86400000) + 1) / 7);
        };

        return `${dayNames[0]} ${start.getUTCDate()} ${monthNames[start.getUTCMonth()]} - ` +
            `${dayNames[6]} ${end.getUTCDate()} ${monthNames[end.getUTCMonth()]}, ` +
            `week ${getISOWeek(start)}`;
    }

    async setScheduleStartDate(date: string) {
        await allure.step(`Set Schedule Start Date to ${date}`, async () => {
            const expectedWeekRange = this.formatWeekRange(date);
            const startDateInput = this.page.locator('#ctrl_dateFrom');
            await startDateInput.waitFor({ state: 'visible', timeout: 5000 });
            await startDateInput.fill(date);
            await startDateInput.press('Enter');
            await this.waitForDataLoad('/api/Time/Schedule/TemplateShift/Search/');
            await this.page.waitForTimeout(2000);
            const emp = await this.page.locator(`//td[contains(@class,"planning-daterange")]`).innerText();
            await expect.poll(async () => emp, { timeout: 10000 }).toBe(expectedWeekRange);
        });
    }

    async setupEmployeeSchedule(empId: string, week: number = 1, weekDay: "Monday" | "Tuesday" | "Wednesday" | "Thursday" | "Friday" | "Saturday" | "Sunday", action: "Remove" | "Activate" | "New basic schedule" | "New shift") {
        await allure.step(`Setup Schedule for Employee ${empId}`, async () => {
            const dayIndex = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"].indexOf(weekDay) + 1;
            const day = 7 * (week - 1) + dayIndex;
            const emp = this.page.locator(`//tbody[@id="rowsTarget"]/tr[@id="${empId}"]`);
            await emp.waitFor({ state: 'visible', timeout: 5000 });
            console.log(`Calculated day index: ${day} for week ${week} and weekday ${weekDay}`);
            const firstMonday = emp.locator('td').nth(day);
            await firstMonday.scrollIntoViewIfNeeded();
            await firstMonday.click();
            await this.page.waitForTimeout(500);
            await firstMonday.click({ button: 'right', force: true });
            await this.page.waitForTimeout(2000);
            const dropdown = this.page.locator('//ul[@class="dropdown-menu" and @role="menu"]');
            await dropdown.waitFor({ state: 'visible', timeout: 5000 });
            const listItems = await dropdown.locator('li a').all();
            for (const item of listItems) {
                const itemText = await item.textContent();
                if (itemText && itemText.includes(action)) {
                    await item.click();
                    break;
                }
            }
            await this.page.locator('.modal-content').waitFor({ state: 'visible', timeout: 5000 });
        });
    }

    async activateSchedule({ weeks = 1, startDate = '', endDate = '', weekInCycle = 1, fn = 'Change end date', isPreliminary = false }:
        { weeks?: number, startDate?: string, endDate?: string, weekInCycle?: number, fn?: string, isPreliminary?: boolean }) {
        if (weeks > 1) {
            await this.page.locator('#ctrl_weeksInCycle').fill(weekInCycle.toString());
        } if (startDate) {
            await this.page.locator('#ctrl_templateHead_startDate').fill(startDate);
        } if (weekInCycle > 1) {
            await this.page.locator('#ctrl_weekInCycle').fill(weekInCycle.toString());
        }
        if (endDate) {
            console.log(`Filling end date: ${endDate}`);
            const endDateInput = this.page.locator('#ctrl_placementStopDate');
            await endDateInput.fill(endDate);
            await endDateInput.press('Enter');
        }
        if (fn !== 'Change end date') {
            const changeEndDateLink = this.page.locator('#ctrl_placementFunction');
            await changeEndDateLink.click();
            await changeEndDateLink.selectOption({ label: fn });
        }
        if (isPreliminary) {
            const preliminaryCheckbox = this.page.locator('#ctrl_placementPreliminary');
            const isChecked = await preliminaryCheckbox.isChecked();
            if (!isChecked) {
                await preliminaryCheckbox.check();
            }
        }
        await this.page.locator('//div[@class="modal-footer"]/button[text()="Activate"]').click();
    }

    async selectSkills({ name, skills = [], selectAll = false }: { name: string, skills?: string[], selectAll?: boolean }) {
        await allure.step(`Select skills`, async () => {
            await this.editEmployeeHRDetails();
            const saveButton = this.page.locator('button[title="Save"]');
            if (selectAll) {
                await this.page.locator('#ctrl_selectAllSkills').check();
            }
            else {
                for (const skill of skills) {
                    const skillCheckbox = this.page.locator(`//div[@title="${skill}"]/../preceding-sibling::div/div/input[@type="checkbox"]`);
                    await skillCheckbox.check();
                }
            }
            await saveButton.click();
            await this.waitForDataLoad('/api/Time/Employee/')
            const emp = this.page.locator(`//tbody[@id="rowsTarget"]/tr//span[contains(text(),"${name}")]`);
            await expect.poll(async () => await emp.count(), { timeout: 10000 }).toBe(1);
        });
    }

    private async editEmployeeHRDetails() {
        await this.page.locator('[label-key="time.employee.employee.hr"]').click();
        const skillTab = this.page.locator('[label-key="time.employee.employee.skill"]');
        await skillTab.waitFor({ state: 'visible', timeout: 2000 });
        await skillTab.click();
    }
}


