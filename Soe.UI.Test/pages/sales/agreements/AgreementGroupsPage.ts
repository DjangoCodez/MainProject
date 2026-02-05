import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../SalesBasePage";
import * as allure from "allure-js-commons";
import { GridPage } from "../../common/GridPage";
import exp from "constants";

export class AgreementsGroupsPage extends SalesBasePage {
    readonly contractGroupsGrid: GridPage;

    constructor(page: Page) {
        super(page);
        this.contractGroupsGrid = new GridPage(page, 'Billing.Contracts.ContractGroups');
    }

    async setName(name: string) {
        await allure.step("Set agreement group name ", async () => {
            await this.page.getByTestId('name').fill(name);
        });
    }

    async setPeriod(period: string) {
        await allure.step("Set agreement group period ", async () => {
            const select = await this.page.getByTestId('period');
            await select.waitFor();
            await expect(select).toBeVisible();
            await this.page.selectOption(`[data-testid="period"]`, period);
        });
    }

    async setPriceManagement(priceMangment: string) {
        await allure.step("Set price Management ", async () => {
            await this.page.selectOption(`[data-testid="priceManagement"]`, priceMangment);
        });
    }

    async setDayOfMonth(day: number) {
        await allure.step("Set Day In Month", async () => {
            await this.page.getByTestId('dayInMonth').fill(day.toString());
        });
    }

    async setInterval(interval: number) {
        await allure.step("Set Interval", async () => {
            await this.page.getByTestId('interval').fill(interval.toString());
        });
    }

    async searchContractGroupByName(name: string) {
        await allure.step("Search Contract Group", async () => {
            await this.contractGroupsGrid.filterByColumnNameAndValue('Name', name);
        });
    }

    async editContractGroup() {
        await allure.step("Edit Contract Group", async () => {
            const edit = await this.page.locator('span[title="Edit"]').nth(0);
            await edit.waitFor({ state: 'visible' });
            await edit.click();
        });
    }

    async updateContractGroupName(name: string) {
        await allure.step("Update Contract Group Name", async () => {
            await this.page.getByTestId('name').fill(name);
        });
    }

    async updateContractGroupDescription(description: string) {
        await allure.step("Update Contract Group Description", async () => {
            await this.page.getByTestId('description').fill(description);
        });
    }

    async deleteContractGroup() {
        await allure.step("Delete Contract Group", async () => {
            await this.page.getByRole('button', { name: ' Remove' }).click();
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async verifyRowCount(expectedCount: number) {
        await allure.step("Verify Row Count", async () => {
           const count = await this.contractGroupsGrid.getRowCount('Name');
           expect(count, `Mismatch: UI shows "${count}", expected "${expectedCount}"`).toBe(expectedCount);
        });
    }

}