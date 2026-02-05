import { expect, type Locator, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";
import { SingleGridPageJS } from '../../common/SingleGridPageJS';
import { SectionGridPageJS } from '../../common/SectionGridPageJS';

export class ProductsPageJS extends SalesBasePage {
    readonly productsGrid: SingleGridPageJS;
    readonly purchaseItemsGrid: SectionGridPageJS;
    readonly stockGrid: SectionGridPageJS;
    readonly priceGrid: SectionGridPageJS;
    readonly statisticsGrid: SectionGridPageJS;

    constructor(page: Page) {
        super(page);
        this.productsGrid = new SingleGridPageJS(page);
        this.purchaseItemsGrid = new SectionGridPageJS(page, 'billing.purchase.product.products', 'ctrl.gridAg.options.gridOptions',);
        this.stockGrid = new SectionGridPageJS(page, 'billing.products.product.stocks', 'ctrl.gridAg.options.gridOptions');
        this.priceGrid = new SectionGridPageJS(page, 'billing.product.prices', 'ctrl.gridAg.options.gridOptions');
        this.statisticsGrid = new SectionGridPageJS(page, 'common.statistics', 'ctrl.gridHandler.gridAg.options.gridOptions');
    }

    async waitForPageLoad() {
        await allure.step("Wait for page load", async () => {
            const pageLoctator = this.page.locator("//div[@role='columnheader']//span[@ref='eText' and text()='Number']");
            await expect(pageLoctator).toBeVisible({ timeout: 10000 });
        });
    }

    async filterProductNo(productNo: string) {
        await allure.step("Filter product no " + productNo, async () => {
            await this.productsGrid.filterByName('Number', productNo);
        });
    }

    async edit() {
        await allure.step("Click Edit button ", async () => {
            await this.productsGrid.edit();
        });
    }

    async expandPurchaseItems() {
        await allure.step("Expand purchase items tab ", async () => {
            let purchaseItemTab = "//div[@class='soe-accordion-heading ng-scope'][.//label[text()='Purchase Items']]//i[contains(@class,'fa-chevron')]"
            await this.page.locator(purchaseItemTab).scrollIntoViewIfNeeded();
            await this.page.waitForSelector('.modal', { state: 'hidden' });
            await this.page.locator(purchaseItemTab).click();
        });
    }

    async verifyPurchaseItems(SupplierProductNo: string) {
        await allure.step("Verify purchase items", async () => {
            await this.purchaseItemsGrid.filterByName('Supplier Product No.', SupplierProductNo);
            await this.page.waitForTimeout(500);
            const count = await this.purchaseItemsGrid.getAgGridRowCount();
            expect(count).toBeGreaterThan(0);
            const productNo = await this.purchaseItemsGrid.getCellvalueByColId('supplierProductNr');
            console.log("Except Product No:" + SupplierProductNo + " Got Product No: " + productNo);
            expect(SupplierProductNo).toContain(productNo);
        });
    }

    async waitforCreateProductPageLoad() {
        await allure.step("Wait for create product page load", async () => {
            await this.waitForDataLoad('/api/Core/UserGridState/common_statistics');
        });
    }

    async setProductNumber(productNumber: string) {
        await allure.step("Set product number: " + productNumber, async () => {
            const productNumberLocator: Locator = this.page.locator('#ctrl_product_number');
            await productNumberLocator.fill(productNumber);
            console.log("Set product number: " + productNumber);
        });
    }

    async setProductName(productName: string) {
        await allure.step("Set product name: " + productName, async () => {
            const productNameLocator: Locator = this.page.locator('#ctrl_product_name');
            await productNameLocator.fill(productName);
            console.log("Set product name: " + productName);
        });
    }

    async saveProduct() {
        await allure.step("Save product", async () => {
            const saveButtonLocator: Locator = this.page.getByRole('button', { name: 'Save' });
            await saveButtonLocator.click();
            await this.waitForDataLoad('/api/Billing/Product/Products/');
        });
    }

    async expandPrice() {
        await allure.step("Expand Price section", async () => {
            const priceSectionLocator: Locator = this.page.locator("//div[@class='soe-accordion-heading ng-scope'][.//label[text()='Price']]//i[contains(@class,'fa-chevron')]");
            await priceSectionLocator.scrollIntoViewIfNeeded();
            await this.page.waitForSelector('.modal', { state: 'hidden' });
            await priceSectionLocator.click();
        });
    }

    async setPurchasePrice(purchasePrice: string) {
        await allure.step("Set purchase price: " + purchasePrice, async () => {
            const purchasePriceLocator: Locator = this.page.getByRole('tabpanel', { name: 'Price' }).locator('#ctrl_product_purchasePrice');
            await purchasePriceLocator.fill(purchasePrice);
        });
    }

    async verifyPurchasePrice(actualPrice: string) {
        await allure.step("Verify purchase price", async () => {
            const purchasePriceLocator: Locator = this.page.getByRole('tabpanel', { name: 'Price' }).locator('#ctrl_product_purchasePrice');
            const purchasePrice = await purchasePriceLocator.inputValue();
            console.log("Purchase Price: " + purchasePrice);
            expect(purchasePrice).toBe(actualPrice);
        });
    }

    async expandStock() {
        await allure.step("Expand Stock section", async () => {
            const stockSectionLocator: Locator = this.page.locator("//div[@class='soe-accordion-heading ng-scope'][.//label[text()='Stock']]//i[contains(@class,'fa-chevron')]");
            await stockSectionLocator.scrollIntoViewIfNeeded();
            await this.page.waitForSelector('.modal', { state: 'hidden' });
            await stockSectionLocator.click();
        });
    }

    async checkMarkAsStocked() {
        await allure.step("Check Mark as Stock product checkbox", async () => {
            const markAsStockedLocator: Locator = this.page.getByRole('checkbox', { name: 'Mark as stock product' });
            const isChecked = await markAsStockedLocator.isChecked();
            if (!isChecked) {
                await markAsStockedLocator.check();
            }
        });
    }

    async clickAddRowInStock() {
        await allure.step("Click add row in stock", async () => {
            const stockPanel = this.page.locator('soe-panel[label-key="billing.products.product.stocks"]');
            const addRowButton = stockPanel.locator('span.panel-button:has-text("Add Row")');
            await this.page.waitForTimeout(1000);
            await addRowButton.click({ force: true });
        });
    }

    async clickAddRowInPrice() {
        await allure.step("Click add row in price", async () => {
            const pricePanel = this.page.locator('soe-panel[button-label-key="common.newrow"]');
            const addRowButton = pricePanel.locator('span.panel-button');
            await addRowButton.waitFor({ state: 'visible' });
            await addRowButton.click();
        });
    }

    async setAveragePrice(averagePrice: string) {
        await allure.step("Set average price: " + averagePrice, async () => {
            await this.stockGrid.enterGridValueByColumnId('avgPrice', averagePrice, 0);
        });
    }

    async verifyStockGridRowCount(expectedCount: number) {
        await allure.step("Verify stock grid row count", async () => {
            const actualCount = await this.stockGrid.getAgGridRowCount();
            console.log("Expected Stock Grid Row Count: " + expectedCount + " Actual Stock Grid Row Count: " + actualCount);
            expect(actualCount).toBe(expectedCount);
        });
    }

    async verifyAveragePriceInStockGrid(expectedPrice: string) {
        await allure.step("Verify average price in stock grid", async () => {
            const actualPrice = await this.stockGrid.getCellvalueByColId('avgPrice');
            console.log("Expected Average Price: " + expectedPrice + " Actual Average Price: " + actualPrice);
            expect(actualPrice).toBe(expectedPrice);
        });
    }

    async verifyGrossMarginMethod(expectedLabel: string) {
        await allure.step(`Verify Gross Margin method: "${expectedLabel || 'empty'}"`, async () => {
            const dropdown = this.page.getByLabel('Calculation Type for Gross Margin');
            await expect(dropdown).toBeVisible();
            const selectedOption = dropdown.locator('option:checked');
            if (expectedLabel === "") {
                const selectedText = (await selectedOption.textContent())?.trim();
                expect(selectedText).toBe('');
            } else {
                const selectedText = (await selectedOption.textContent())?.trim();
                expect(selectedText).toBe(expectedLabel);
            }
        });
    }

    async setGrossMarginMethod(value: string) {
        await allure.step(`Set Gross Margin method to "${value}"`, async () => {
            await this.page.waitForTimeout(1000);
            const dropdown = this.page.getByLabel('Calculation Type for Gross Margin');
            await dropdown.selectOption({ label: value });
        });
    }

    async closeTab() {
        await allure.step("Close product tab", async () => {
            const closeTabButton: Locator = this.page.locator('i.removableTabIcon.fal.fa-times.ng-scope');
            await closeTabButton.click();
            await this.page.waitForTimeout(1000);
        });
    }

    async verifyProductIsActive() {
        await allure.step("Verify product is active", async () => {
            const activeCheckbox: Locator = this.page.getByRole('checkbox', { name: 'Active' });
            const isChecked = await activeCheckbox.isChecked();
            expect(isChecked).toBeTruthy();
        });
    }

    async setOrderpoint(orderpoint: string) {
        await allure.step("Set orderpoint: " + orderpoint, async () => {
            await this.stockGrid.enterGridValueByColumnId('purchaseTriggerQuantity', orderpoint, 0);
        });
    }

    async setPriceList(priceListName: string) {
        await allure.step(`Set price list: ${priceListName}`, async () => {
            await this.priceGrid.enterDropDownValueGridRichSelecter('priceListTypeId', priceListName, 0);
            const option = this.page.locator('div.ag-rich-select-virtual-list-container div.ag-rich-select-row', { hasText: priceListName });
            await option.scrollIntoViewIfNeeded();
            await option.click({ force: true });
        });
    }

    async setStockLocation(stockLocationName: string) {
        await allure.step(`Set stock location: ${stockLocationName}`, async () => {
            await this.stockGrid.enterDropDownValueGridRichSelecter('stockShelfId', stockLocationName, 0);
        });
    }

    async setPriceInPriceGrid(price: string, rowIndex: number) {
        await allure.step(`Set price: ${price} in row index: ${rowIndex}`, async () => {
            await this.priceGrid.enterGridValueByColumnId('price', price, rowIndex);
        });
    }

    async setPurchaseQuantity(purchaseQuantity: string) {
        await allure.step("Set purchase quantity: " + purchaseQuantity, async () => {
            await this.stockGrid.enterGridValueByColumnId('purchaseQuantity', purchaseQuantity, 0);
        });
    }

    async setLeadTime(leadTime: string) {
        await allure.step("Set lead time: " + leadTime, async () => {
            await this.stockGrid.enterGridValueByColumnId('deliveryLeadTimeDays', leadTime, 0);
        });
    }

    async expandStatistics() {
        await allure.step("Expand Statistics section", async () => {
            const statisticsSectionLocator: Locator = this.page.locator("//div[@class='soe-accordion-heading ng-scope'][.//label[text()='Statistics']]//i[contains(@class,'fa-chevron')]");
            await statisticsSectionLocator.scrollIntoViewIfNeeded();
            await this.page.waitForSelector('.modal', { state: 'hidden' });
            await statisticsSectionLocator.click();
        });
    }

    async selectTypeInStatistics(type: string) {
        await allure.step(`Select type: ${type}`, async () => {
            await this.page.locator('#ctrl_selectedOriginType').selectOption({ label: type });
        });
    }

    async clickSearchInStatistics() {
        await allure.step("Click Search in Statistics", async () => {
            const searchButtonLocator: Locator = this.page.getByTitle('Search', { exact: true });
            await searchButtonLocator.click();
            await this.waitForDataLoad('/api/Billing/Product/Statistics/');
        });
    }

    async verifyInvoiceNumberInStatistics(invoiceNumber: string,rowIndex: number =0) {
        await allure.step(`Verify invoice number in statistics: ${invoiceNumber}`, async () => {
            const cellValue = await this.statisticsGrid.getCellvalueByColIdandGrid('invoiceNr', rowIndex);
            if (cellValue === invoiceNumber) {
                expect(cellValue).toBe(invoiceNumber);
                return;
            }
            throw new Error(`Invoice number ${invoiceNumber} not found in statistics grid`);
        });
    }
}