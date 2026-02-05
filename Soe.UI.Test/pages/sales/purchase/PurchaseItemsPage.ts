import { expect, type Locator, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";
import { GridPage } from '../../common/GridPage';

export class PurchaseItemsPage extends SalesBasePage {

    readonly purchasePriceGrid: GridPage;
    readonly purchaseItems: GridPage;

    constructor(page: Page) {
        super(page);
        this.purchasePriceGrid = new GridPage(page, 'billing.purchase.rows');
        this.purchaseItems = new GridPage(page, 'Billing.Purchase.Products');
    }

    async addSupplier(supplier: string) {
        await allure.step("Add supplier " + supplier, async () => {
            const supplierEle = this.page.locator("//input[@role='combobox' and contains(@class, 'mat-mdc-autocomplete-trigger')]").first();
            await supplierEle.click();
            await supplierEle.fill(supplier);
            const option = this.page.locator(`mat-option >> text=${supplier}`);
            await option.waitFor({ state: 'visible' });
            await option.click();
            console.log(`Selected supplier: ${supplier}`);
        });
    }

    async addSupplierProductNo(supplierProductNo: string) {
        await allure.step("Add supplier product no " + supplierProductNo, async () => {
            const productNoEle = await this.page.getByTestId('supplierProductNr');
            await productNoEle.fill(supplierProductNo);
            await this.page.keyboard.press('Enter');
            console.log("Supplier Product No: " + supplierProductNo);
        });
    }

    async addSuplierProductName(supplierProductName: string) {
        await allure.step("Add supplier product name " + supplierProductName, async () => {
            const productNameEle = await this.page.getByTestId('supplierProductName').nth(1);
            await productNameEle.click();
            await productNameEle.fill(supplierProductName);
        });
    }

    async addSuplierProductUnit(supplierProductUnit: string) {
        await allure.step("Add supplier product unit " + supplierProductUnit, async () => {
            const productUnitEle = await this.page.getByTestId('supplierProductUnitId');
            await productUnitEle.selectOption({ label: supplierProductUnit });
            await this.page.keyboard.press('Enter');
        });
    }

    async addProductNo(productNo: string) {
        await allure.step("Add product no " + productNo, async () => {
            const productIdEle = await  this.page.locator("//input[@role='combobox' and contains(@class, 'mat-mdc-autocomplete-trigger')]").last();
            await productIdEle.click();
            await productIdEle.fill(productNo);
            await this.page.waitForTimeout(2000); // Wait for options to load
            await this.page.keyboard.press('Enter');
        });
    }

    async expandPurchasePricerows() {
        await allure.step("Expand purchase rows", async () => {
            await this.page.getByTestId('billing.product.purchaseprice-container').scrollIntoViewIfNeeded();
            await this.page.getByTestId('billing.product.purchaseprice-container').click();
        });
    }

    async addRowInPurchasePrice() {
        await allure.step("Add Purchase Price rows", async () => {
            await this.page.getByRole('button', { name: 'Add row' }).scrollIntoViewIfNeeded();
            await this.page.getByRole('button', { name: 'Add row' }).click();
        });
    }

    async addFromQuantity(fromQuantity: string) {
        await allure.step("Add from quantity " + fromQuantity, async () => {
            await this.purchasePriceGrid.enterValueToGrid('From Quantity', fromQuantity);
        });
    }

    async addPurchasePrice(purchasePrice: string) {
        await allure.step("Add purchase price " + purchasePrice, async () => {
            await this.purchasePriceGrid.enterValueToGrid('Purchase Price', purchasePrice);
        });
    }

    async addCurrency(currnecy: string, rowIndex: number = 0) {
        await allure.step("Add currnecy " + currnecy, async () => {
            await this.purchasePriceGrid.selectDropdownValueFromGrid_1('Currency', currnecy, rowIndex);
        });
    }

    async addStartDate(date: string, rowIndex: number = 0) {
        await allure.step("Add start date " + date, async () => {
            await this.purchasePriceGrid.clickGridRow('Start Date', rowIndex);
            await this.page.getByRole('textbox', { name: 'mm/dd/yyyy' }).clear();
            await this.page.getByRole('textbox', { name: 'mm/dd/yyyy' }).fill(date);
            await this.page.waitForTimeout(2000);
        });
    }

    async addEndDate(date: string, rowIndex: number = 0) {
        await allure.step("Add end date " + date, async () => {
            await this.page.getByRole('textbox', { name: 'mm/dd/yyyy' }).clear();
            await this.page.getByRole('textbox', { name: 'mm/dd/yyyy' }).fill(date);
            await this.page.waitForTimeout(500);
        });
    }

    async filterSupplierProductNo(supplierProductNo: string) {
        await allure.step("Enter supplier product no " + supplierProductNo, async () => {
            await this.purchaseItems.filterByColumnNameAndValue('Supplier Product No.', supplierProductNo);
        });
    }

    async verifyRecordCount(count: string) {
        await allure.step("Verify Purchase Item row ", async () => {
            await this.purchaseItems.verifyFilteredItemCount(count);
        });
    }

    async verifyProductNo(productNo: string) {
        await allure.step("Verify product no ", async () => {
            let value = await this.purchaseItems.getRowColumnValue("Product No.", 1);
            expect(String(value).trim()).toEqual(String(productNo).trim());
        });
    }

    async closeUnsavedChangesButton() {
        await allure.step("Close unsaved changes button", async () => {
            const closeUnsavedChangesButton = this.page.getByTestId('primary');
            if (await closeUnsavedChangesButton.isEnabled()) {
                await closeUnsavedChangesButton.click();
            }
        });
    }

    async waitForSave() {
        await allure.step("Wait for save", async () => {
            await this.waitForDataLoad('/api/V2/Billing/Supplier/Product/Products/');
        });
    }

}