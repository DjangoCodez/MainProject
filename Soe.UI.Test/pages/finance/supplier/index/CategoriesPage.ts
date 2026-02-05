import { expect, Page } from "@playwright/test";
import { BasePage } from "pages/common/BasePage";
import * as allure from "allure-js-commons";
import { GridPage } from "pages/common/GridPage";


export class CategoriesPage extends BasePage {
    readonly page: Page;
    readonly categoriesGrid: GridPage;

    constructor(page: Page) {
        super(page);
        this.page = page;
        this.categoriesGrid = new GridPage(page, 'common.categories.categories');
    }

    async addCode(code: string) {
        await allure.step(`add code ${code}`, async () => {
            const addCode = await this.page.getByTestId("code");
            await addCode.waitFor({ state: 'visible' });
            await addCode.fill(code);
        });
    }

    async addName(name: string) {
        await allure.step(`add name ${name}`, async () => {
            const addName = await this.page.getByTestId("name");
            await addName.waitFor({ state: 'visible' });
            await addName.fill(name);
        });
    }

    async verifyCodeNameAdreadyExistsMessage(code: string) {
        await allure.step(`verify code name already exists message for code ${code}`, async () => {
            await expect(this.page.locator('span.soe-dialog__text-content', { hasText: code })).toBeVisible();
        });
    }

    async filterbyCode(code: string) {
        await allure.step(`filter by code ${code}`, async () => {
            await this.categoriesGrid.filterByColumnNameAndValue('Code', code);
        });
    }

    async edit() {
        await allure.step(`edit the category`, async () => {
            const edit = await this.page.locator('//span[@title=\'Edit\']');
            await edit.waitFor({ state: 'visible' });
            await edit.click();
        });
    }

    async remove() {
        await allure.step(`edit the category`, async () => {
            const remove = await this.page.getByTestId('delete');
            await remove.waitFor({ state: 'visible' });
            await remove.click();
        });
    }

    async reload(){
        await allure.step(`Reload the page`, async () => {
            const reload = await this.page.getByTestId('reload');
            await reload.waitFor({ state: 'visible' });
            await reload.click();
        });
    }

    async verifyNoRecords() {
        await allure.step(`verify no records found`, async () => {
            const noRecords = await this.page.getByTestId('filtered');
            await noRecords.waitFor({ state: 'attached' });
            await noRecords.waitFor({ state: 'visible' });
            await expect(noRecords, `expected no records found but found some records`).toHaveText(' (Filtered 0) ');
        });
    }



}