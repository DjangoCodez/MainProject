import { expect, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";
import { GridPage } from '../../common/GridPage';

export class WarehousePage extends SalesBasePage {

    readonly stockLocationGrid: GridPage;
    readonly warehouseLocationGrid: GridPage;

    constructor(page: Page) {
        super(page);
        this.stockLocationGrid = new GridPage(page, 'Soe.Billing.Stock.Stocks.Shelves');
        this.warehouseLocationGrid = new GridPage(page, 'Soe.Billing.Stock.Stocks');
    }

    async enterCode(code: string) {
        await allure.step("Enter code : " + code, async () => {
            await this.page.getByTestId('code').fill(code);
        });
    }

    async enterName(name: string) {
        await allure.step("Enter name : " + name, async () => {
            await this.page.getByTestId('name').fill(name);
        });
    }

    async expandStockLocation() {
        await allure.step("Expand stock location : ", async () => {
            await this.page.getByTestId('billing.stock.stockplaces.stockplace').click();
        });
    }

    async clickAddStockLocation() {
        await allure.step("Click on Add stock location : ", async () => {
            await this.page.getByTestId('core.add').click();
        });
    }

    async enterStockLocationFoColumn(columnName: string, value: string) {
        await allure.step("Enter stock location : " + columnName + ":" + value, async () => {
            await this.stockLocationGrid.enterValueToGrid(columnName, value);
        });
    }

    async saveWarehouseLocation() {
        await allure.step("Save warehouse location ", async () => {
            await this.page.getByTestId('save').click();
        });
    }

    async goToWarehouseLocationTab() {
        await allure.step("Go Warehosue location tab", async () => {
            await this.page.getByTestId('tab-0').click();
        })
    }

    async filterWarehouseByColumn(columnName: string, value: string) {
        await allure.step("Filter warehosue " + columnName + "> " + value, async () => {
            await this.warehouseLocationGrid.filterByColumnNameAndValue(columnName, value);
        })
    }

    async verifyFilteredWarehouse(value: string) {
        await allure.step("Verify filtered correctly with one entry and filtered count 1", async () => {
            await this.warehouseLocationGrid.verifyFilteredItem(value);
            await this.warehouseLocationGrid.verifyFilteredItemCount('1');
        })
    }

    async verifyFilteredWarehouseCount(count: string) {
        await allure.step("Verify filtered count " + count, async () => {
            await this.warehouseLocationGrid.verifyFilteredItemCount(count);
        })
    }

    async verifyFiltereIsCleared() {
        await allure.step("Verify filtered is cleared ", async () => {
            await this.warehouseLocationGrid.verifyFilteredIsCleared();
        })
    }

    async clearGridFilter() {
        await allure.step("Clear grid filter", async () => {
            await this.page.getByTestId('clearFilters').click(); 
        })
    }

    async releadRecords() {
        await allure.step("Reload grid records", async () => {
            await this.page.getByTestId('reload').click();
        })
    }


}