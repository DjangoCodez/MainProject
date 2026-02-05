import { expect, type Page } from '@playwright/test';
import * as allure from "allure-js-commons";
import { getFirstDateOfCurrentMonth, getLastDateOfCurrentMonth } from '../../../utils/CommonUtil';
import { GridPage } from '../../common/GridPage';
import { SalesBasePage } from '../SalesBasePage';
import { AngVersion } from '../../../enums/AngVersionEnums';

export class PurchasePriceListPage extends SalesBasePage {
    readonly page: Page;
    readonly purchasePriceGrid: GridPage;
    readonly purchasePriceListGrid: GridPage;
    readonly ang_version: AngVersion;

    constructor(page: Page, ang_version: AngVersion = AngVersion.NEW) {
        super(page);
        this.page = page;
        this.purchasePriceGrid = new GridPage(page, 'billing.purchase.pricelist.prices');
        this.purchasePriceListGrid = new GridPage(page, 'Billing.Purchase.Pricelists');
        this.ang_version = ang_version;
    }

    async addSupplier(supplier: string) {
        await allure.step("Add supplier " + supplier, async () => {
            const supplierEle = this.page.locator("//input[@role='combobox' and contains(@class, 'mat-mdc-autocomplete-trigger')]").nth(1);
            await supplierEle.click();
            await supplierEle.fill(supplier);
            const option = this.page.locator(`mat-option >> text=${supplier}`);
            await option.waitFor({ state: 'visible' });
            await option.click();
            await this.page.keyboard.press('Enter');
        });
    }

    async addPriceListValidFrom() {
        await allure.step('Add price list valid from ', async () => {
            const priceListValidFromEle = this.page.locator("(//input[@placeholder='mm/dd/yyyy'])[1]");
            await priceListValidFromEle.click();
            const dateInput = await getFirstDateOfCurrentMonth();
            await priceListValidFromEle.fill(dateInput);
            await this.page.keyboard.press('Enter');
        });
    }

    async addPriceListValidTo() {
        await allure.step('Add price list valid to ', async () => {
            const priceListValidToEle = this.page.locator("(//input[@placeholder='mm/dd/yyyy'])[2]");
            await priceListValidToEle.click();
            const dateInput = await getLastDateOfCurrentMonth();
            await priceListValidToEle.fill(dateInput);
            await this.page.keyboard.press('Enter');
        }
        );
    }

    async addCurrency(currency: string) {
        await allure.step('Add currency', async () => {
            const currencyEle = this.page.getByTestId('currencyId');
            await currencyEle.click();
            await currencyEle.selectOption({ label: currency });
            await this.page.keyboard.press('Enter');
        });
    }

    async addPurchasePriceRow() {
        await allure.step('Add purchase price row ', async () => {
            const addRowEle = this.page.locator("//button[.//span[contains(text(), 'Add Row')]]");
            await addRowEle.click();
        })
    }

    async addSupplierProductNo(SupplierProductNo: string, rowIndex: number = 0) {
        await allure.step('Add Supplier product no', async () => {
            await this.purchasePriceGrid.selectDropdownValueFromGrid('Supplier Product No.', SupplierProductNo, rowIndex);
        })
    }

    async addNewFromQuantity(quantity: number, rowIndex: number = 0) {
        await allure.step('Add Supplier product quantity', async () => {
            await this.purchasePriceGrid.enterValueToGridByColumnId('quantity', quantity.toString(), rowIndex);
        })
    }

    async addNewPurchasePrice(price: number, rowIndex: number = 0) {
        await allure.step('Add Supplier product purchase price', async () => {
            await this.purchasePriceGrid.enterValueToGridByColumnId('price', price.toString(), rowIndex);
        })
    }

    async save() {
        await allure.step('Save', async () => {
            await this.page.getByTestId('save').click();
        })
    }

    async verifyTabNameChanged(SupplierName: string) {
        await allure.step('Verify Tab name changed after save', async () => {
            const tabName = await this.page.locator(`//span[contains(normalize-space(text()), 'Purchase Price List ${SupplierName}')]`);
            await expect(tabName).toBeVisible();
        })
    }

    async closeTab() {
        await allure.step("Close tab", async () => {
            await this.page.getByTestId('tab-1-close').click();
        })
    }

    async reloadPage() {
        await allure.step("Reload page", async () => {
            await this.page.getByTestId('reload').click();
        })
    }

    async filterBySupplierName(supplier: string) {
        await allure.step("Filter by supplier name ", async () => {
            await this.purchasePriceListGrid.filterByColumnNameAndValue('Supplier Name', supplier);
        });
    }

    async editLastRecord() {
        await allure.step("View edit on last record", async () => {
            await this.page.locator('//soe-icon-cell-renderer//fa-icon').last().click();
        });
    }

    async verifySupplierProductNo(supplierProductNo: string) {
        await allure.step("verify Supplier product no", async () => {
            await this.purchasePriceGrid.filterByColumnNameAndValue('Supplier Product No.', '');
            await this.purchasePriceGrid.filterByColumnNameAndValue('Supplier Product No.', supplierProductNo);
            await this.purchasePriceGrid.verifyFilteredItemCount('1');
            await this.page.waitForTimeout(3500);
            let productNo = await this.purchasePriceGrid.getRowColumnValue('Supplier Product No.', 1);
            console.log('Expected Supplier Product No: ' + supplierProductNo + ' , Actual Supplier Product No: ' + productNo);
            expect(productNo?.toString().replace(/^\s+|\s+$/g, ''), `expect ${supplierProductNo} to be ${productNo}`).toBe(supplierProductNo?.toString().replace(/^\s+|\s+$/g, ''));
        });
    }

}

