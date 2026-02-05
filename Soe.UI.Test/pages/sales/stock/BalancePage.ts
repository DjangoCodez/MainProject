import { expect, type Locator, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";
import { GridPage } from '../../common/GridPage';
import { ok } from 'assert';

export class BalancePage extends SalesBasePage {

    readonly balanceMainGrid: GridPage;
    readonly transactionGrid: GridPage;


    constructor(page: Page) {
        super(page);
        this.balanceMainGrid = new GridPage(page, 'Billing.Stock.StockSaldo');
        this.transactionGrid = new GridPage(page, 'Billing.Stock.StockSaldo.StockTransactions');
    }

    async waitforPageLoad() {
        await allure.step("Wait for Balance page to load", async () => {
            await this.balanceMainGrid.waitForPageLoad();
        })
    }

    async releadRecords() {
        await allure.step("Reload grid records", async () => {
            await this.page.getByTestId('core.reload_data').click();
        })
    }

    async clearGridFilter() {
        await allure.step("Clear grid filter", async () => {
            await this.page.getByTestId('core.uigrid.gridmenu.clear_all_filters').click();
        })
    }

    async addQuantity(quantity: string) {
        await allure.step("Add quantity for balance", async () => {
            await this.page.getByTestId('quantity').fill(quantity);
            await this.page.keyboard.press('Tab');
        })
    }

    async addPrice(price: string) {
        await allure.step("Add quantity for balance", async () => {
            await this.page.getByTestId('price').fill(price);
        })
    }

    async updatePrice(price: string) {
        await allure.step("Update price for balance", async () => {
            const priceInput = this.page.getByTestId('price');
            await priceInput.fill('');
            await priceInput.fill(price);
        })
    }

    async addNote(note: string) {
        await allure.step("Add note for balance", async () => {
            await this.page.getByTestId('note').fill(note);
        })
    }

    async saveBalance() {
        await allure.step("Save the balance changes", async () => {
            await this.page.getByTestId('save').click();
            await this.waitForDataLoad('Billing/Stock/StockProduct/Transactions');
        })
    }

    async clickBalanceTab() {
        await allure.step("Go to balance tab", async () => {
            await this.clickTabByTestId('tab-0');
        })
    }

    async filterByProductNumber(productNumber: string) {
        await allure.step(`Filter by product number: ${productNumber}`, async () => {
            await this.balanceMainGrid.filterByColumnNameAndValue('Product Number', productNumber);
        })
    }

    async edit(rowIndex: number = 0) {
        await allure.step("Edit row", async () => {
            const rowId = await this.page.locator('.ag-center-cols-container [role="row"]').nth(rowIndex).getAttribute('row-id');
            if (!rowId) throw new Error('Row-id not found');
            await this.page.locator(`.ag-pinned-right-cols-container [role="row"][row-id="${rowId}"] [title="Edit"]`).click({ force: true });
            await this.waitForDataLoad('/api/V2/Core/UserGridState/Billing_Stock_StockSaldo_StockTransactions');
            await this.page.getByTestId('editTab-0').waitFor({ state: 'visible' });
        });
    }

    async selectType(type: string) {
        await allure.step(`Select type: ${type}`, async () => {
            await this.page.getByTestId('actionType').selectOption({ label: type });
        });
    }

    async verifyInStock(expectedQuantity: string) {
        await allure.step(`Verify in stock quantity: ${expectedQuantity}`, async () => {
            try {
                await this.balanceMainGrid.verifyCellValueFromGrid('In Stock', expectedQuantity, 0);
            } catch (error) {
                await this.balanceMainGrid.verifyCellValueFromGrid('In stock', expectedQuantity, 0);
            }
        });
    }

    async verifyAveragePriceInGrid(expectedPrice: string) {
        await allure.step(`Verify average price in grid: ${expectedPrice}`, async () => {
            try {
                await this.balanceMainGrid.verifyCellValueFromGrid('Average Price', expectedPrice, 0);
            } catch (error) {
                await this.balanceMainGrid.verifyCellValueFromGrid('Average price', expectedPrice, 0);
            }
        });
    }

    async setProductNumber(productNumber: string) {
        await allure.step(`Set product number: ${productNumber}`, async () => {
            await this.page.waitForTimeout(1000);
            const input = this.page.locator(
                'soe-autocomplete[formcontrolname="invoiceProductId"] input'
            );
            await input.waitFor({ state: 'visible' });
            await input.click();
            await input.fill(productNumber);
            const option = this.page.locator(
                'mat-option span',
                { hasText: productNumber }
            );
            await option.waitFor({ state: 'visible' });
            await option.click();
            console.log("Set product number:", productNumber);
        });
    }

    async handleWarningPopupIfPresent() {
        await allure.step('Handle warning popup if present', async () => {
            try {
                const dialog = this.page.locator('soe-messagebox');
                await dialog.waitFor({ state: 'attached', timeout: 2000 });
                const okButton = this.page.getByTestId('primary');
                await okButton.click();
                await dialog.waitFor({ state: 'detached', timeout: 3000 });
                await this.page
                    .locator('.cdk-overlay-backdrop')
                    .waitFor({ state: 'detached', timeout: 3000 });
            } catch (error) {
                console.log('Warning popup not present or already closed');
            }
        });
    }

    async setWarehouseLocation(warehouseLocation: string) {
        await allure.step(`Set warehouse location: ${warehouseLocation}`, async () => {
            const input = this.page.locator('soe-autocomplete[formcontrolname="stockProductId"] input');
            await input.waitFor({ state: 'visible' });
            await input.click();
            await input.fill(warehouseLocation);
            const option = this.page.locator('mat-option span', { hasText: warehouseLocation });
            await option.waitFor({ state: 'visible' });
            await option.click();
            console.log("Set warehouse location:", warehouseLocation);
        });
    }

    async verifySetPriceInBalance(expectedPrice: string) {
        await allure.step(`Verify set price in balance: ${expectedPrice}`, async () => {
            const priceInput = this.page.getByTestId('price');
            const actualPrice = await priceInput.inputValue();
            console.log(`Expected Price: ${expectedPrice} | Actual Price: ${actualPrice}`);
            const expected = Number(expectedPrice);
            const actual = Number(actualPrice);
            expect(actual).toBe(expected);
        });
    }

    async clickAddButtonBalance() {
        await allure.step("Add balance", async () => {
            await this.page.locator('.cdk-overlay-backdrop').waitFor({ state: 'detached' });
            const addButton = this.page.getByTestId('common.general-container').getByTestId('save');
            await addButton.click();
        });
    }

    async clickRecalculateBalance() {
        await allure.step("Click Recalculate Balance", async () => {
            const recalculateButton = this.page.getByTestId('billing.stock.stocksaldo.recalculatebalance');
            await recalculateButton.click();
            await this.waitForDataLoad('/api/V2/Billing/Stock/StockProducts');
        });
    }

    async verifyInOrderInBalanceGrid(expectedQuantity: string) {
        await allure.step(`Verify In Order quantity in balance grid: ${expectedQuantity}`, async () => {
            await this.balanceMainGrid.verifyCellValueFromGrid('In Order', expectedQuantity, 0);
        });
    }

    async verifyReservedInBalanceGrid(expectedQuantity: string) {
        await allure.step(`Verify Reserved quantity in balance grid: ${expectedQuantity}`, async () => {
            await this.balanceMainGrid.verifyCellValueFromGrid('Reserved', expectedQuantity, 0);
        });
    }

    async verifyInStockInBalanceGrid(expectedQuantity: string) {
        await allure.step(`Verify In Stock quantity in balance grid: ${expectedQuantity}`, async () => {
            await this.balanceMainGrid.verifyCellValueFromGrid('In Stock', expectedQuantity, 0);
        });
    }

}