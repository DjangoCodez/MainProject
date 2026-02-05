import { StaffBasePage } from "../StaffBasePage";
import { Page } from "@playwright/test";
import * as allure from "allure-js-commons";
import { SingleGridPageJS } from "pages/common/SingleGridPageJS";


export class PlanningPage extends StaffBasePage {

    readonly terminalGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.terminalGrid = new SingleGridPageJS(page);
    }

    async waitforPageLoad() {
        await allure.step("Wait for load", async () => {
            await this.page.waitForSelector('li > a[href="#Form1_2"]', { state: 'visible' });
        });
    }

    async goToPlanningTab() {
        await allure.step("Go to Planning Terminals Tab", async () => {
            const tabLocator = this.page.locator(`//a[@href='#Form1_2' and normalize-space()='Planning settings']`);
            await tabLocator.click();
        });
    }

    async toggleIgnoreBreaksWhenPlanning(enable: boolean = true) {
        await allure.step("Toggle Ignore breaks when planning", async () => {
            const checkbox = this.page.locator("//input[@id='OrderPlanningIgnoreScheduledBreaksOnAssignment']");
            await checkbox.waitFor({ state: 'visible' });
            if (enable) {
                await checkbox.check();
            } else {
                await checkbox.uncheck();
            }
        });
    }

    async savePlanningSettings() {
        await allure.step("Click Save button", async () => {
            const saveButton = this.page.locator(  "//button[@id='submit' and @title='Save']");
            await saveButton.waitFor({ state: 'visible' });
            await saveButton.click();
        });
    }




}