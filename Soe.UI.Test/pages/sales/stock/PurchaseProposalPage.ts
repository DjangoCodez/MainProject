import { expect, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";
import { GridPage } from '../../common/GridPage'


export class PurchaseProposalPage extends SalesBasePage {

    constructor(page: Page) {
        super(page);
    }

    async selectBaseSuggestion(baseSuggetion: string) {
        await allure.step("Select base suggetion on", async () => {
            await this.page.getByTestId('purchaseGenerationType').selectOption(baseSuggetion);
        });
    }

    async selectWarehouseLocation(warehouseLocation: string) {
        await allure.step("Select wharehouse location", async () => {
            await this.page.locator('[id="selection_billing\\.stock\\.stocks\\.stocks"]').click();
            await this.page.getByPlaceholder('Search...').fill(warehouseLocation);
            await this.page.getByRole('listitem', { name: warehouseLocation, exact: true }).getByRole('checkbox').click();
            await this.page.getByTestId('common.selection-container').getByRole('button', { name: 'Filter' }).click();
        });
    }

    async createSuggestion() {
        await allure.step("Create suggestion", async () => {
            await this.page.getByTestId('common.selection-container').getByTestId('primary').click();
        });
    }

    async uncheckedWithoutOrderPoints() {
        await allure.step("Unchecked exclude items without order points", async () => {
            await this.page.getByTestId('excludeMissingTriggerQuantity').click();
        });
    }

    async uncheckedWithoutPurchaseQuantity() {
        await allure.step("Unchecked exclude items without purchase quantity", async () => {
            await this.page.getByTestId('excludeMissingPurchaseQuantity').click();
        });
    }

    async clickCreatePurchase() {
        await allure.step("Click create purchase button", async () => {
            await this.page.getByRole('button', { name: 'Create purchase' }).click();
        });
    }

    async selectAllRows() {
        await allure.step("Select all rows checkbox", async () => {
            await this.page.getByRole('checkbox', { name: 'Column with Header Selection' }).click();
        });
    }

    async clickYesInformationAlert() {
        await allure.step("Click yes for information alert", async () => {
            await this.page.getByTestId('core.info').getByTestId('primary').click();
        });
    }

}