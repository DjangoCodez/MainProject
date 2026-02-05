import { expect, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";
import { GridPage } from '../../common/GridPage';



export class InventoryPage extends SalesBasePage {

    readonly stockInvnetoryRowsGrid: GridPage;
    readonly stockInvnetoryMainGrid: GridPage;

    constructor(page: Page) {
        super(page);
        this.stockInvnetoryMainGrid = new GridPage(page, 'Billing.Stock.StockInventory'); 
        this.stockInvnetoryRowsGrid = new GridPage(page, 'Billing.Stock.StockInventory.Rows');      
    }

    async waitForPageLoad() {
        await allure.step("Wait for Inventory page to load", async () => {
            await this.stockInvnetoryMainGrid.waitForPageLoad();
        });
    }

    async waitForInventoryRowsPageLoad() {
        await allure.step("Wait for Inventory Rows page to load", async () => {
            await this.stockInvnetoryRowsGrid.waitForPageLoad();
            await this.page.getByTestId('standard').waitFor({ state: 'visible' });
        });
    }

    async enterName(name: string) {
        await allure.step("Enter inventory name : " + name, async () => {
            await this.page.getByTestId('headerText').click();
            await this.page.getByTestId('headerText').fill(name);
        });
    }

    async selectWarehouseLocation(wareHouseLocation: string) {
        await allure.step("Select warehouse location", async () => {
            await this.page.getByTestId('stockId').click();
            await this.page.getByTestId('stockId').selectOption(wareHouseLocation);
            await this.waitForDataLoad('Billing/Stock/StockProducts/Products');
            await this.page.waitForLoadState('domcontentloaded');
        })
    }

    async enterProductNumberFromTo(productNumberFrom: string, productNUmberTo: string) {
        await allure.step("Enter product No. from & Product No to : " + productNumberFrom + "," + productNUmberTo, async () => {
            await this.page.getByTestId('productNrFromId').fill(productNumberFrom);
            await this.page.keyboard.press('Enter');
            await this.page.getByTestId('productNrToId').fill(productNUmberTo);
            await this.page.keyboard.press('Enter');
        })
    }

    async generateDocumentation() {
        await allure.step("Generate documentation", async () => {
            await this.page.getByTestId('standard').click();
            await this.waitForDataLoad('Billing/Stock/GenerateRows');
        })
    }

    async acceptItemNotFoundAlert() {
        await allure.step("Accept item not found alert", async () => {
            await this.page.getByTestId('primary').click();
        })
    }

    async saveInventory(checkForDataLoad: boolean = true) {
        await allure.step("Save Inventory", async () => {
            await this.page.getByTestId('save').click();
            if (checkForDataLoad) {
                await this.waitForDataLoad('/Billing/Stock/SaveInventory');
            }
            await this.page.waitForTimeout(1000); // Wait for the save operation to complete
        })
    }
    
    async acceptInventory() {
        await allure.step("Accept Inventory", async () => {
            await this.page.getByTestId('standard').click();
        })
    }

    async selectInventoryByWarehouse(){
        await allure.step("Select inventory by warehouse", async () => {
            await this.stockInvnetoryMainGrid.clickGridRow('Warehouse Location', 1);
            await this.waitForDataLoad('Billing/Stock/StockInventory');
        })
    }

    async acceptAcceptInventoryAlert() {
        await allure.step("Click yes for accept inventory alert", async () => {
            await this.page.getByTestId('primary').click();
        })
    }

    async goToInventoryTab() {
        await allure.step("Go Inventory tab", async () => {
            await this.page.getByTestId('tab-0').click();
        })
    }

    async searchInventoryByName(name: string) {
        await allure.step("Search inventory by name", async () => {
            await this.stockInvnetoryMainGrid.filterByColumnNameAndValue('Name', name);
        })
    }

    async searchInventoryByWarehouseLocation(name: string) {
        await allure.step("Search inventory by warehouse location", async () => {
            await this.page.getByRole('textbox', { name: 'Warehouse location Filter' }).fill(name);
        })
    }

    async verifyFilteredInventory(name: string, exactMatch: boolean = true) {
        await allure.step("Verify inventory", async () => {
            await this.stockInvnetoryMainGrid.verifyFilteredItem(name, exactMatch);
        })
    }

    async clearGridFilter() {
        await allure.step("Clear grid filter", async () => {
            await this.page.getByTestId('clearFilters').click();
        })
    }

    async verifyFilteredInventoryCount(count: string) {
        await allure.step("Verify inventory filtered count is " + count, async () => {
            await expect(this.page.getByTestId('filtered')).toHaveText('(Filtered ' + count + ') ');
        })
    }

    async verifyFilterIsCleared() {
        await allure.step("Verify inventory filtered is cleared ", async () => {
                await expect(this.page.getByTestId('filtered')).toHaveCount(0);
        })
    }

    async releadRecords() {
        await allure.step("Reload grid records", async () => {
            await this.page.getByTestId('reload').click();
        })
    }

    async closeInventoryTab() {
        await allure.step("Close Tab 1", async () => {
            await this.page.getByTestId('tab-1-close').click();
        })
    }


}