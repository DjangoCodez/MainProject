import { expect, Page } from "@playwright/test";
import { BasePage } from "../../common/BasePage";
import * as allure from "allure-js-commons";


export class OverviewPage extends BasePage {
    readonly page: Page;
    private newPage!: Page;

    constructor(page: Page) {
        super(page);
        this.page = page;
    }

    async filterBySupplierName(name: string) {
        await allure.step('Create Supplier Invoice', async () => {
            const nameFilterInput = this.page.locator(`//input[@aria-label='Name Filter Input']`);
            await nameFilterInput.fill(name);
            await nameFilterInput.press('Enter');
        });
    }

    async editSupplier(isSelectedSupplier: boolean = true) {
        await allure.step(`Edit Supplier`, async () => {
            const editButton = this.page.locator(`//label[normalize-space()='Supplier']/parent::div//button[contains(@class,'iconEdit')]`);
            await editButton.waitFor({ state: 'visible' });
            await editButton.click();
            if (isSelectedSupplier) {
                await this.waitForDataLoad('/api/Economy/Supplier/Supplier/NextSupplierNr/');
            } else {
                await this.waitForDataLoad('/api/Core/ContactPerson/ContactPersonsByActorId/', 20000);
            }
        });
    }

    async selectSupplier() {
        await allure.step(`Select Supplier`, async () => {
            const select = this.page.locator(`//div[normalize-space(text())='AuttoTest000 Test supplier']`);
            await select.waitFor({ state: 'visible' });
            await select.click();
        });
    }

    async clikOnSupplierName() {
        await allure.step(`Click on Supplier Name`, async () => {
            const supplierNameLink = this.page.locator(`//div[@role='gridcell' and @col-id='supplierName' and normalize-space(.)='AuttoTest000 Test supplier']`);
            await supplierNameLink.nth(0).waitFor({ state: 'visible' });
            [this.newPage] = await Promise.all([
                this.page.context().waitForEvent('page'),
                await supplierNameLink.nth(0).dblclick()
            ]);
            await this.newPage.waitForLoadState();
        });
    }

    async verifyNewBrowserUrl(tabName: string) {
        await allure.step(`Verify New Tab Opened`, async () => {
            await expect(this.newPage).toHaveTitle(new RegExp(tabName));
        });
    }

    async closedOpenedBrowserTab() {
        await allure.step(`closed opened browser tab`, async () => {
            await this.newPage.close();
        });
    }

    async searchSupplier() {
        await allure.step(`Search Supplier`, async () => {
            const searchInput = this.page.getByTestId('economy.supplier.suppliercentral.seeksupplierbutton');
            await searchInput.click();
        });
    }

    async verifySearchSupplierPopUpOpen() {
        await allure.step(`Verify Search Supplier Pop Up Open`, async () => {
            const popUpTitle = this.page.locator(`//span[contains(normalize-space(.),'Search Supplier')]`);
            await expect(popUpTitle, `expect Search Supplier Pop Up to be visible`).toBeVisible();
        });
    }

}