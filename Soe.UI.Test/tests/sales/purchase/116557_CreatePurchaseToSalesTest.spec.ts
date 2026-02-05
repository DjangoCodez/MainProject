import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { ProductUtils } from '../../../apis/utils/ProductUtil';
import { getAccountExValue } from '../../../utils/properties';
import { getDateUtil } from '../../../utils/CommonUtil';

let testCaseId: string = '116557';
let envUrl: string;
let productUtils: ProductUtils;
let productNumber: string;
let supplierProductName: string = 'Auto ' + testCaseId + " Product Name";
let supplierProductUnit: string = "Meters";
let supplierProductNo: string = Math.floor(100000 + Math.random() * 900000) + '_' + testCaseId;
let productName: string;


test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Purchase");
  await allure.subSuite("Purchase");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  productUtils = new ProductUtils(page, envUrl);
  const prodId = (await productUtils.createProduct(testCaseId, "Auto Purchase-")).number;
  productNumber = prodId.toString();
  productName = productNumber + ' Auto Purchase-';
});

test(testCaseId + ': Create purchase item link to sales item : AP', { tag: ['@Sales', '@Purchase', '@Regression'] }, async ({ page, salesBasePage, purchaseItemsPage, productsPageJS }) => {
  await salesBasePage.goToMenu('Purchase', 'Purchase Items');
  await purchaseItemsPage.createItem();
  await purchaseItemsPage.addSupplier("AuttoTest000 Test supplier");
  await purchaseItemsPage.addSupplierProductNo(supplierProductNo);
  await purchaseItemsPage.addSuplierProductName(supplierProductName);
  await purchaseItemsPage.addSuplierProductUnit(supplierProductUnit);
  await purchaseItemsPage.addProductNo(productNumber);
  await purchaseItemsPage.expandPurchasePricerows();
  await purchaseItemsPage.addRowInPurchasePrice();
  await purchaseItemsPage.addFromQuantity("100");
  await purchaseItemsPage.addPurchasePrice("10.50");
  await purchaseItemsPage.addCurrency("SEK");
  const startDate = await getDateUtil(2);
  const endDate = await getDateUtil(2, true);
  await purchaseItemsPage.addStartDate(startDate);
  await page.keyboard.press('Enter');
  await purchaseItemsPage.addEndDate(endDate);
  await purchaseItemsPage.addRowInPurchasePrice();
  await purchaseItemsPage.addFromQuantity("200");
  await purchaseItemsPage.addPurchasePrice("10.40");
  await purchaseItemsPage.addCurrency("SEK", 1);
  await purchaseItemsPage.addStartDate(startDate, 2);
  await page.keyboard.press('Enter');
  await purchaseItemsPage.addEndDate(endDate, 2);
  await page.keyboard.press('Enter');
  await purchaseItemsPage.save();
  await purchaseItemsPage.waitForSave();
  await purchaseItemsPage.closeTab();
  await purchaseItemsPage.closeUnsavedChangesButton();
  await purchaseItemsPage.reloadPage();
  await purchaseItemsPage.filterSupplierProductNo(supplierProductNo);
  await purchaseItemsPage.verifyRecordCount("1");
  await purchaseItemsPage.verifyProductNo(productNumber);
  await page.waitForTimeout(2000);
  await salesBasePage.goToMenu('Product', 'Products');
  await productsPageJS.waitForPageLoad();
  await productsPageJS.filterProductNo(productNumber);
  await productsPageJS.edit();
  await productsPageJS.expandPurchaseItems();
  await productsPageJS.verifyPurchaseItems(supplierProductNo);
});


