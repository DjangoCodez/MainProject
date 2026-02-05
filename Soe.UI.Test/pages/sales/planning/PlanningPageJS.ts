import { expect, type Locator, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";

export class PlanningPageJS extends SalesBasePage {

    constructor(page: Page) {
        super(page);
    }


    async filterOrder(invoiceNumber: string) {
        await allure.step("Select order by order number", async () => {
            const selection = await this.page.locator(`//button[@title='Selection' and .//i[contains(@class, 'fa-filter')]]`);
            const textInput = await this.page.locator('#ctrl_orderListFreeTextFilter');
            await selection.waitFor({ state: 'visible' });
            if (!await textInput.isVisible()) {
                await selection.click();
            }
            await textInput.waitFor({ state: 'visible' });
            await textInput.fill(invoiceNumber.toString());
        });
    }

    async planOrderDragandDrop(orderNumber: string, employeeName: string, customerName: string) {
        await allure.step(`Plan order Drag and drop : ${orderNumber}`, async () => {
            await this.page.waitForTimeout(500);
            const xpath = `//li[@drag-object-type='order' and contains(@title, 'Assignment type: SeleniumOrderType')]//ng-include//div//span[@class='ng-binding' and text()='${orderNumber}']`;
            const source = this.page.locator(xpath);
            const target = this.page.locator(`//td[contains(@title, "${employeeName}")]/following-sibling::td[contains(@class, "planning-day-today")]//div[contains(@class, "shift-drop-zone")]`);
            await source.waitFor({ state: 'visible' });
            await target.waitFor({ state: 'visible' });
            await source.scrollIntoViewIfNeeded();
            await target.scrollIntoViewIfNeeded();
            await source.hover();
            await this.page.waitForTimeout(100);
            await target.hover();
            await source.dragTo(target);
        });
    }

    async reloadOrders() {
        await allure.step("Reload orders", async () => {
            const reloadIcon = this.page.locator('//i[@class="fal fa-sync pull-right link" and @data-ng-click="ctrl.loadAllUnscheduledOrders()"]');
            await reloadIcon.waitFor({ state: 'visible' });
            await reloadIcon.click();
            await this.waitForDataLoad('/api/Time/Schedule/Order/Unscheduled/');
        });
    }

    async verifyPlanningTime(plannedTime: string) {
        await allure.step(`Verify planning time for order: `, async () => {
            await this.page.waitForTimeout(1500);
            const plannedTimeHH = await this.page.locator(`//div[@model="ctrl.plannedTimeDate"]//input[@placeholder="HH"]`).inputValue();
            const plannedTimeMM = await this.page.locator(`//div[@model="ctrl.plannedTimeDate"]//input[@placeholder="MM"]`).inputValue();
            const plannedTimeValue = `${plannedTimeHH}:${plannedTimeMM}`;
            expect(plannedTimeValue, `expect : ${plannedTime} got ${plannedTimeValue.trim()}`).toBe(plannedTime);
        });
    }

    async saveNewAssignment() {
        await allure.step("Save new assignment", async () => {
            const saveButton = this.page.getByRole('button', { name: 'Save' });
            await saveButton.click();
            await this.waitForDataLoad('/api/Time/Schedule/Shift/Search/');
        });
    }

    async waitForDialogPopup() {
        await allure.step('Wait for employee input to be visible', async () => {
            await expect(this.page.locator('#ctrl_selectedEmployee')).toBeVisible();
            await this.waitForDataLoad('/api/Time/Schedule/Shift')
        });
    }

    async waitOrderLoading() {
        await allure.step('Wait for order loading', async () => {
            await this.waitForDataLoad('/api/Time/Schedule/Shift/Search/');
        });
    }

    async editAssignment(employeeName: string) {
        await allure.step('Edit assignment', async () => {
            const shiftLocators = this.page.locator(`//tr[td[contains(., "${employeeName}")]]//td[contains(@class, "planning-day-today")]//div[contains(@class, "planning-shift")]`);
            await shiftLocators.first().waitFor({ state: 'visible' });
            await shiftLocators.first().scrollIntoViewIfNeeded();
            await shiftLocators.first().dblclick();
        });
    }

    async createOrderOrBookingByShiftByDate(empId: string, date: string) {
        await allure.step(`Right click shift slot for date ${date}`, async () => {
            const newDate = new Date(date);
            const year = newDate.getFullYear();
            const month = String(newDate.getMonth() + 1).padStart(2, '0');
            const day = String(newDate.getDate()).padStart(2, '0');
            const formatDate = `'${year}|${month}|${day}|00|00'`;
            const shiftSlot = this.page.locator(`//tr[@id='empId${empId}']//div[contains(@class,'shift-drop-zone') and @date="${formatDate}"]`).nth(0);
            await shiftSlot.waitFor({ state: 'visible' });
            await shiftSlot.scrollIntoViewIfNeeded();
            await shiftSlot.click({ button: 'right' });
        });
    }


    async editEndTime(newEndTime: string) {
        await allure.step(`Edit end time to: ${newEndTime}`, async () => {
            const endTimeHHLocator = this.page.locator('//input[@type="text" and @placeholder="HH" and @ng-model="hours"]').nth(1);
            await endTimeHHLocator.fill('');
            await this.page.keyboard.press('Enter');
            await this.page.waitForTimeout(3000);
            await endTimeHHLocator.fill(newEndTime);
            await this.page.keyboard.press('Enter');
            const endTimeMMLocator = this.page.locator('//input[@type="text" and @placeholder="MM" and @ng-model="minutes"]').nth(1);
            await endTimeMMLocator.fill('');
            await endTimeMMLocator.fill('00');
            await this.page.keyboard.press('Enter');
            const inputSelector = '//input[@ng-model="hours"][2]';
            await this.page.waitForFunction(
                async (selector) => {
                    const el = document.evaluate(selector, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    if (el instanceof HTMLInputElement) {
                        return el.value === '01';
                    }
                    return false;
                },
                inputSelector,
            );
        });
    }

    async removeAssignment(employeeName: string, orderNumber: string) {
        await allure.step('Remove assignment', async () => {
            await this.page.waitForTimeout(5000);
            const shiftLocators = this.page.locator(`//tr[td[contains(., "${employeeName}")]]//td[contains(@class, "planning-day-today")]//div[contains(@class, "planning-shift")]`);
            await shiftLocators.first().waitFor({ state: 'visible' });
            const shiftCount = await shiftLocators.count();
            console.log(` Found ${shiftCount} shifts for employee "${employeeName}" (Order: ${orderNumber})`);
            let targetShiftId: string | null = null;

            const getShiftTitleWithRetries = async (shift: Locator, shiftIndex: number) => {
                const maxRetries = 5;
                const retryDelay = 100;
                for (let attempt = 1; attempt <= maxRetries; attempt++) {
                    try {
                        let target: Element | null;
                        const title = await shift.evaluate(el => {
                            target = el.querySelector('[title]') || el;
                            return target.getAttribute('title');
                        });

                        if (title) {
                            return title;
                        } else {
                            console.log(`Attempt ${attempt} (Shift #${shiftIndex + 1}): title is null`);
                        }
                    } catch (err) {
                        console.warn(` Attempt ${attempt} failed on shift #${shiftIndex + 1}:`, err);
                    }
                    await new Promise(r => setTimeout(r, retryDelay));
                }
                return null;
            };

            for (let i = 0; i < shiftCount; i++) {
                const shift = shiftLocators.nth(i);
                await shift.hover();
                await shift.waitFor({ state: 'visible' });
                const title = await getShiftTitleWithRetries(shift, i);
                if (title) {
                    const mainSection = title.split("Today's mission")[0];
                    if (mainSection.includes(orderNumber)) {
                        targetShiftId = await shift.evaluate(el => el.getAttribute('id'));
                        break;
                    }
                }
            }

            if (targetShiftId) {
                const targetShift = this.page.locator(`//tr[td[contains(., "${employeeName}")]]//div[@id='${targetShiftId}']`);
                await targetShift.waitFor({ state: 'visible' });
                await targetShift.scrollIntoViewIfNeeded();
                await targetShift.click({ button: 'right' });
                await this.page.getByRole('link', { name: ' Remove assignments' }).click();
            } else {
                throw new Error(`No shift found for employee "${employeeName}" with orderNumber "${orderNumber}".`);
            }
        });
    }

    async verifyTimeInOrder(orderNumber: string, time: string) {
        await allure.step(`Verify time in order: ${time}`, async () => {
            const timeText = await this.page.locator(`//li[contains(., "${orderNumber}")]//span[contains(@class, "times")]`).innerText();
            expect(timeText, `Time : ${timeText} got ${timeText}`).toBe(time);
        });
    }

    async removeAllReadyAssignments(employeeId: string) {
        return await allure.step('Remove allready assignments', async () => {
            const shiftLocators = this.page.locator(`//tr[@id='empId${employeeId}']//td[contains(@class, "planning-day-today")]//div[contains(@class,'planning-shift')and @id]`);
            try {
                await shiftLocators.first().waitFor({ state: 'visible' });
            } catch (e) {
                console.warn('Shift not visible after waiting');
                return [];
            }
            const shiftIds: string[] = [];
            const count = await shiftLocators.count();
            for (let i = 0; i < count; i++) {
                const id = await shiftLocators.nth(i).evaluate(el => el.id);
                if (id) shiftIds.push(id);
            }
            return shiftIds;
        });
    }

    async newBooking() {
        await allure.step('Create new booking', async () => {
            const newBooking = this.page.locator(`//a[normalize-space()='New booking']`);
            await newBooking.waitFor({ state: 'visible' });
            await newBooking.click();
            await this.waitForDataLoad('/Time/Schedule/Shift/853/20260109T000000/1,2/false/false/null/false/false/false/true/0');
        });
    }

    async bookingType(bookingTypeLabel: string = 'Playwright BookingType') {
        await allure.step('Create new booking', async () => {
            const shiftType = this.page.locator('#ctrl_shift_shiftTypeId');
            await shiftType.waitFor({ state: 'visible' });
            await shiftType.selectOption({ label: bookingTypeLabel });
            await this.page.waitForTimeout(50);
        });
    }

    async description(descriptionText: string = '') {
        await allure.step('Create new booking', async () => {
            const description = this.page.locator('#ctrl_shift_description');
            await description.waitFor({ state: 'visible' });
            await expect(description).toBeEditable();
            await description.fill(descriptionText);
        });
    }

    async saveBooking() {
        return allure.step('Save new booking', async () => {
            const save = this.page.getByRole('button', { name: 'Save' });
            await save.waitFor({ state: 'visible' });
            await save.click();
            await this.waitForDataLoad('/api/Time/Schedule/Shift/Search/', 25_000);
        });
    }

    async editBooking() {
        return allure.step('Edit new booking', async () => {
            const edit = this.page.locator(`//a[contains(@class,'dropdown-item') and contains(normalize-space(),'Edit booking')]`);
            await edit.click();
        });
    }

    async verifyDescription(descriptionText: string ) {
        return allure.step('Verify description', async () => {
            const description = await this.page.locator(`#ctrl_shift_description`)
            await description.waitFor({ state: 'visible' });
            const descriptionText = await description.textContent();
            expect(descriptionText?.trim(), 'Verify description text').toBe(descriptionText);
        });
    }

    async verifyLength(lengthText: string) {
        return allure.step('Verify description', async () => {
            const length = this.page.locator(`//span[@data-ng-class="{'errorColor': ctrl.plannedTime <= 0}"]`);
            length.waitFor({ state: 'visible' });
            const lengthTextContent = await length.innerText();
            expect(lengthTextContent, 'Verify length text').toBe(lengthText);
        });
    }
}