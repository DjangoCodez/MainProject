import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { ProductUtils } from '../../../apis/utils/ProductUtil';
import { getAccountExValue } from '../../../utils/properties';
import { SupplierUtil } from '../../../apis/utils/SupplierUtil';

let testCaseId: string = '116559';
let envUrl: string;
let productUtils: ProductUtils;
let supplierUtil: SupplierUtil;
let productIdOne: string;
let productIdTwo: string;
let supplier: any;
let products: any;
let supplierProductNrOne: string;
let supplierProductNrTwo: string;

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Purchase");
  await allure.subSuite("Purchase pricelists");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  productUtils = new ProductUtils(page, envUrl);
  productIdOne = await (await productUtils.createProduct(testCaseId + "-1", "Auto Purchase-1")).toString();
  productIdTwo = await (await productUtils.createProduct(testCaseId + "-2", "Auto Purchase-2")).toString();
  supplierUtil = new SupplierUtil(page, envUrl);
  supplier = await supplierUtil.getSupplier();
  products = await productUtils.getProducts();
  supplierProductNrOne = Math.floor(100000 + Math.random() * 900000) + '_' + testCaseId;
  supplierProductNrTwo = Math.floor(100000 + Math.random() * 900000) + '_' + testCaseId;
  await supplierUtil.CreateSupplierProduct(supplier[0].id, productIdOne, supplierProductNrOne);
  await supplierUtil.CreateSupplierProduct(supplier[0].id, productIdTwo, supplierProductNrTwo);
});

test(testCaseId + ': Create purchase price list : AP ', { tag: ['@Sales', '@Purchase', '@Regression'] }, async ({ page, salesBasePage, purchasePriceListPage }) => {
  // Temp fix
  if (process.env.ENV === 'stage') {
    await salesBasePage.goToMenu('Purchase', 'Purchase pricelists');
  } else {
    await salesBasePage.goToMenu('Purchase', 'Purchase Price Lists');
  }
  await salesBasePage.goToPageVersion(purchasePriceListPage.ang_version);
  await salesBasePage.createItem();
  await purchasePriceListPage.addSupplier(supplier[0].name.slice(2));
  await purchasePriceListPage.addPriceListValidFrom();
  await purchasePriceListPage.addPriceListValidTo();
  await purchasePriceListPage.addCurrency("Swedish Krona");
  await purchasePriceListPage.addPurchasePriceRow();
  await purchasePriceListPage.addSupplierProductNo(supplierProductNrOne);
  await page.waitForTimeout(1000);
  await page.keyboard.press('Enter');
  await purchasePriceListPage.addNewFromQuantity(100);
  await purchasePriceListPage.addNewPurchasePrice(10.20);
  await purchasePriceListPage.addPurchasePriceRow();
  await purchasePriceListPage.addSupplierProductNo(supplierProductNrTwo, 1);
  await page.waitForTimeout(1000);
  await page.keyboard.press('Enter');
  await purchasePriceListPage.addNewFromQuantity(200, 1);
  await purchasePriceListPage.addNewPurchasePrice(10.10, 1);
  await purchasePriceListPage.save();
  await page.waitForTimeout(1000);
  await purchasePriceListPage.verifyTabNameChanged(supplier[0].name.slice(2));
  await purchasePriceListPage.closeTab();
  await purchasePriceListPage.reloadPage();
  await purchasePriceListPage.filterBySupplierName(supplier[0].name.slice(2));
  await purchasePriceListPage.editLastRecord();
  await purchasePriceListPage.verifySupplierProductNo(supplierProductNrOne);
  await purchasePriceListPage.verifySupplierProductNo(supplierProductNrTwo);
});


