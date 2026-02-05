import { expect, type Locator, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";
import { GridPage } from '../../common/GridPage';
import { AngVersion } from '../../../enums/AngVersionEnums';
import { SingleGridPageJS } from '../../common/SingleGridPageJS';
import { SectionGridPageJS } from '../../common/SectionGridPageJS';

export class PurchacePage extends SalesBasePage {

    readonly purchaseGrid: GridPage;
    readonly purchaseGridJS: SingleGridPageJS;
    readonly purchaseRowGrid: GridPage;
    readonly purchaseRowGridJS: SectionGridPageJS;
    readonly ang_version: AngVersion;

    constructor(page: Page, ang_version: AngVersion = AngVersion.NEW) {
        super(page);
        this.purchaseGrid = new GridPage(page, 'Billing.Purchase.Purchase');
        this.purchaseGridJS = new SingleGridPageJS(page);
        this.purchaseRowGrid = new GridPage(page, 'billing.purchase.rows');
        this.purchaseRowGridJS = new SectionGridPageJS(page, 'billing.purchase.rows', 'directiveCtrl.gridAg.options.gridOptions');
        this.ang_version = ang_version;
    }

    getPageVersion() {
        return this.ang_version;
    }

    getPurchaseMainGrid() {
        return this.purchaseGrid;
    }

    getPurchaseRowsGrid() {
        return this.purchaseRowGrid;
    }

    getPurchaseRowsGridJS() {
        return this.purchaseRowGridJS;
    }

    async goToPurchasePage() {
        super.goToMenu('Purchase', 'Purchase');
    }

    async waitForPageLoad() {
        await allure.step("Wait for Purchase page to load", async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.purchaseGrid.waitForPageLoad();
            } else {
                await this.purchaseGridJS.waitForPageLoad();
            }
        });
    }

    async filterByPurchaseNumber(purchaseNumber: string) {
        await allure.step("Filter by purchase number " + purchaseNumber, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.purchaseGrid.filterByColumnNameAndValue('Purchase No.', purchaseNumber);
            } else {
                await this.purchaseGridJS.filterByName('Purchase No.', purchaseNumber);
            }
        });
    }

    async verifyFilteredItemCount(count: string) {
        await allure.step("Verify filtered item count " + count, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.purchaseGrid.verifyFilteredItemCount(count);
            } else {
                expect(await this.purchaseGridJS.getFilteredAgGridRowCount()).toBe(parseInt(count));
            }
        });
    }

    async selectPurchase(purchaseNumber: string) {
        await allure.step("Select purchase " + purchaseNumber, async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.purchaseGrid.doubleClickGridCellItem(purchaseNumber);
            } else {
                await this.purchaseGridJS.clickButtonByColumnId('edit');
            }
            await this.waitForDataLoad('/api/Billing/Purchase/Order/');
        });
    }

    async addSupplier(supplier: string) {
        await allure.step("Add supplier " + supplier, async () => {
            //const supplierEle = await this.page.getByTestId('supplierId');
            const supplierEle = this.page.locator("//input[@role='combobox' and contains(@class, 'mat-mdc-autocomplete-trigger')]").first();
            await supplierEle.click();
            await supplierEle.fill(supplier);
            await this.page.getByText(supplier).first().click();
        });
    }


    async addInternalText(internalText: string) {
        await allure.step("Add internal text " + internalText, async () => {
            await this.page.getByTestId('origindescription').fill(internalText);
        });
    }

    async expandPurchaserows() {
        await allure.step("Expand purchase rows", async () => {
            if (this.ang_version === AngVersion.NEW) {
                await this.page.getByTestId('billing.purchase.rows-container').scrollIntoViewIfNeeded();
                await this.page.getByTestId('billing.purchase.rows-container').click();
                await this.page.waitForTimeout(1000);
            } else {
                const allocateCost = this.page.getByRole('button', { name: 'Purchase rows' });
                await allocateCost.scrollIntoViewIfNeeded();
                await allocateCost.click();
                await this.page.waitForTimeout(1000);
            }
            await this.page.waitForTimeout(3000);
        });
    }

    async addRowInPurchase() {
        await allure.step("Add rows", async () => {
            await this.page.getByRole('button', { name: 'Add row' }).scrollIntoViewIfNeeded();
            await this.page.getByRole('button', { name: 'Add row' }).click();
        });
    }

    async addProduct(productName: string) {
        await allure.step("Add product " + productName, async () => {
            const product = this.page.locator("xpath=//div[@row-id='0']//div[@col-id='productId']").nth(0);
            await product.click();
            await await this.page.locator("xpath=//div[@row-id='0']//div[@col-id='productId']").nth(0).locator("xpath=//input").fill(productName);
            await this.page.getByRole('option', { name: productName, exact: true }).click();
            await this.page.keyboard.press('Enter');
        });

    }

    async addQuantity(quantity: string) {
        await allure.step("Add quantity " + quantity, async () => {
            await this.page.locator("xpath=//div[@row-id='0']//div[@col-id='quantity']").nth(0).click();
            await this.page.locator("xpath=//div[@row-id='0']//div[@col-id='quantity']").nth(0).locator("xpath=//input").fill(quantity);
            await this.page.keyboard.press('Enter');
        });
    }

    async addPurchasePrice(price: string) {
        await allure.step("Add price " + price, async () => {
            await this.page.locator("xpath=//div[@row-id='0']//div[@col-id='purchasePriceCurrency']").nth(0).click();
            await this.page.locator("xpath=//div[@row-id='0']//div[@col-id='purchasePriceCurrency']").nth(0).locator("xpath=//input").fill(price);
            await this.page.keyboard.press('Enter');
        });
    }

    async savePurchase() {
        const responsePromise = this.page.waitForResponse('**/Billing/PurchaseOrders');
        await allure.step("Save purchase ", async () => {
            await this.page.getByRole('button', { name: 'Save' }).click();
        })
        const response = await responsePromise;
        return response.json();
        ;
    }

    async checkPurchaseNumber(value: string) {
        await allure.step("Verify purchase number " + value, async () => {
            await expect(this.page.getByTestId('purchaseNr')).toHaveValue(value);
        });
    }

    async filterPurchasesByColumn(columnName: string, value: string) {
        await allure.step("Filter purchases " + columnName + "> " + value, async () => {
            await this.purchaseGrid.filterByColumnNameAndValue(columnName, value);
        })
    }

    async verifySupplier(supplier: string) {
        await allure.step("Verify supplier " + supplier, async () => {
            if (this.ang_version === AngVersion.NEW) {
                const supplierEle = this.page.locator("//input[@role='combobox' and contains(@class, 'mat-mdc-autocomplete-trigger')]").first();
                await supplierEle.click();
                await expect(this.page.getByRole('option', { name: new RegExp(supplier, 'i') })).toBeVisible();
            } else {
                await expect(this.page.getByRole('textbox', { name: 'Supplier' })).toHaveValue(new RegExp(supplier, 'i'));
            }
        });
    }
}