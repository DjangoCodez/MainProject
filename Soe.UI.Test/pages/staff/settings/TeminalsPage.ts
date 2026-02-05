import { StaffBasePage } from "../StaffBasePage";
import { expect, Page } from "@playwright/test";
import * as allure from "allure-js-commons";
import { time } from "console";
import { SingleGridPageJS } from "pages/common/SingleGridPageJS";


export class TerminalsPage extends StaffBasePage {

    readonly terminalGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.terminalGrid = new SingleGridPageJS(page);
    }

    async waitforPageLoad() {
        await allure.step("Wait for load", async () => {
            await this.waitForDataLoad('/api/Core/UserGridState/Time_Time_TimeTerminals');
        });
    }

    async createTerminalIfNotExists(testCaseId: string, terminalName: string, heading: string) {
        await allure.step("Create new terminal if not exists", async () => {
            await this.terminalGrid.filterByName('Name', testCaseId);
            let count: number = await this.terminalGrid.getFilteredAgGridRowCount();
            if (count === 0) {
                await this.createItem();
                await this.waitForDataLoad('/Time/Time/TimeTerminal/GroupName/');
                await this.page.waitForTimeout(3000);
                await this.addTerminalName(terminalName);
                await this.expandHomePageSection();
                await this.addHeading(heading);
                await this.page.waitForTimeout(1000);
                await this.save();
                await this.page.waitForTimeout(5000);
            }
        });
    }

    async addTerminalName(terminalName: string) {
        await allure.step(`Add terminal name: ${terminalName}`, async () => {
            const nameTextbox = this.page.getByRole('textbox', { name: 'Name' });
            await nameTextbox.waitFor({ state: 'visible', timeout: 5000 });
            await nameTextbox.fill(terminalName);
            await this.page.waitForTimeout(2000);
        });
    }

    async expandHomePageSection() {
        await allure.step("Expand home page section", async () => {
            await this.page.getByRole('button', { name: 'Home page ' }).click();
        });
    }

    async addHeading(heading: string) {
        await allure.step("Add heading", async () => {
            await this.page.getByRole('textbox', { name: 'Heading' }).fill(heading);
        });
    }

    async save() {
        await allure.step("Save terminal", async () => {
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.page.waitForTimeout(1000);
        });
    }

    async openTerminalDetails(terminalName: string) {
        await allure.step(`Open terminal details: ${terminalName}`, async () => {
            const link = this.page.getByRole('link', { name: terminalName + ' ' });
            if (!(await link.isVisible())) {
                await this.page.locator('#ng-app-bootstrap-element').getByRole('link', { name: 'Terminals' }).click();
                await this.terminalGrid.filterByName('Name', terminalName);
                await this.terminalGrid.edit();
            } else {
                await link.click();
            }
        });
    }

    async openTerminalPunchEmployee() {
        return await allure.step(`Open terminal:`, async () => {
            const [newPage] = await Promise.all([
                this.page.context().waitForEvent('page'),
                this.page.locator('button[title="Open"]').click()
            ]);
            await newPage.waitForLoadState();
            await newPage.waitForTimeout(5000);
            if (await newPage.getByRole('button', { name: 'Cancel' }).isVisible({ timeout: 10000 })) {
                await newPage.getByRole('button', { name: 'Cancel' }).click();
            }
            await newPage.getByRole('button', { name: '1' }).click();
            await newPage.getByRole('button', { name: '6' }).click();
            await newPage.getByRole('button', { name: '9' }).click();
            const pucnchTime = await newPage.locator('//clock//div[@class=\'time\']/span').textContent();
            await newPage.getByRole('button').filter({ hasText: /^$/ }).nth(1).click();
            await newPage.getByText('Deviation').click();
            await newPage.waitForTimeout(2000);
            await newPage.getByText('Standard').click();
            await newPage.waitForTimeout(2000);
            await newPage.close();
            return pucnchTime;
        });
    }
}