import { expect, test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { StockUtils } from '../../../apis/utils/StockUtils';
import { EconomyAPI } from '../../../apis/EconomyAPI';
import { BillingAPI } from '../../../apis/BillingAPI';
import { getAccountExValue } from '../../../utils/properties';
import { SupplierUtil } from '../../../apis/utils/SupplierUtil';
import { AngVersion } from '../../../enums/AngVersionEnums';


let testCaseId: string = '118900';
let warehouseLocationName: string;
let stockId: number;
let envUrl: string;
let stockUtils: StockUtils;
let economyAPI: EconomyAPI;
let billingAPI: BillingAPI;
let supplierUtil: SupplierUtil

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Stock");
    await allure.subSuite("Purchase Proposal");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    stockUtils = new StockUtils(page, envUrl);
    economyAPI = new EconomyAPI(page, envUrl);
    billingAPI = new BillingAPI(page, envUrl);
    supplierUtil = new SupplierUtil(page, envUrl);
    warehouseLocationName = 'Auto WHL' + testCaseId;
    stockId = await stockUtils.createWareHouseLocation(warehouseLocationName);
});

test(testCaseId + ': Create purchase proposal : MG', { tag: ['@Sales', '@Stock', '@Regression'] }, async ({ page, salesBasePage, purchaseProposalPage, purchasePage }) => {
    await salesBasePage.goToMenu('Stock', 'Purchase Proposal');
    await purchaseProposalPage.selectBaseSuggestion('Order quantity if stock balance <= order point');
    await purchaseProposalPage.selectWarehouseLocation(warehouseLocationName);
    await purchaseProposalPage.uncheckedWithoutOrderPoints();
    const responsePromise = page.waitForResponse('**/GenerateSuggestion');
    await purchaseProposalPage.createSuggestion();
    const res = await responsePromise;
    let resSuggestion = await res.json();
    let supplierName: string;
    if (resSuggestion.length == 0) {
        const productId = await stockUtils.addProducts(stockId);
        console.log('Product id : ' + productId);
        await page.waitForTimeout(3000);
        const suppliers = await economyAPI.getActiveSupliers();
        const supplierProductNr = Math.floor(100000 + Math.random() * 900000) + '_' + testCaseId;
        await supplierUtil.CreateSupplierProduct(suppliers[0].id, productId, supplierProductNr)
        const responsePromiseSecond = page.waitForResponse('**/GenerateSuggestion');
        await purchaseProposalPage.createSuggestion();
        const resTwo = await responsePromiseSecond;
        resSuggestion = await resTwo.json();
        supplierName = suppliers[0].name;
    } else {
        supplierName = resSuggestion[0].supplierName;
    }
    await purchaseProposalPage.selectAllRows();
    await purchaseProposalPage.clickCreatePurchase();
    const responsePromisePurchase = page.waitForResponse('**/CreatePurchaseFromStockSuggestion');
    await purchaseProposalPage.clickYesInformationAlert();
    const resPurchase = await responsePromisePurchase;
    const resPurchaseJ = await resPurchase.json();
    let purchaseNumber: string = resPurchaseJ[0].name;
    await purchasePage.goToMenu('Purchase', 'Purchase');
    await purchasePage.goToPageVersion(purchasePage.getPageVersion());
    await purchasePage.waitForPageLoad();
    await purchasePage.filterByPurchaseNumber(purchaseNumber);
    await purchasePage.verifyFilteredItemCount('1');
    await purchasePage.selectPurchase(purchaseNumber);
    await purchasePage.verifySupplier(supplierName);
    await purchasePage.expandPurchaserows();
    if (purchasePage.getPageVersion() === AngVersion.NEW) {
        await purchasePage.getPurchaseRowsGrid().verifyCellValueFromGrid('Product no.', resSuggestion[0].productNr);
        await purchasePage.getPurchaseRowsGrid().verifyCellValueFromGrid('Text', resSuggestion[0].productName);
        await purchasePage.getPurchaseRowsGrid().verifyCellValueFromGrid('Stock', 'Code' + warehouseLocationName);
    } else {
        expect(await purchasePage.getPurchaseRowsGridJS().getCellValueFromGrid('rowNr', '1', 'productNr')).toBe(resSuggestion[0].productNr);
        expect(await purchasePage.getPurchaseRowsGridJS().getCellValueFromGrid('rowNr', '1', 'text')).toBe(resSuggestion[0].productName);
        expect(await purchasePage.getPurchaseRowsGridJS().getCellValueFromGrid('rowNr', '1', 'stockId')).toBe('Code' + warehouseLocationName);
    }
    //clean up purchase created
    await billingAPI.deletePurchase(page, resPurchaseJ[0].id)
});
