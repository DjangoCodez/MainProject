import { test } from '../../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { StockUtils } from '../../../../apis/utils/StockUtils';
import { getAccountExValue } from '../../../../utils/properties';


let testCaseId: string = '118904';
let envUrl: string;
let stockUtils: StockUtils;

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Stock");
  await allure.subSuite("Warehouse Location");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  stockUtils = new StockUtils(page, envUrl);
});

test(testCaseId + ': Create new warehouse location : MG', { tag: ['@Sales', '@Stock', '@Regression'] }, async ({ page, salesBasePage, wareHousePage, inventoryPage }) => {
  await salesBasePage.goToMenu('Stock', 'Warehouse location', true);
  await wareHousePage.createItem();
  let warehouseLocationCode: string = 'AutoWLCode' + Math.random().toString(36).substring(2, 7);
  let warehouseLocationName: string = 'AutoWLName' + Math.random().toString(36).substring(2, 7);
  await wareHousePage.enterCode(warehouseLocationCode);
  await wareHousePage.enterName(warehouseLocationName);
  await wareHousePage.expandStockLocation();
  await wareHousePage.clickAddStockLocation();
  let stockLocationCode: string = 'AutoRackCode' + Math.random().toString(36).substring(2, 7);
  let stockLocationName: string = 'AutoRackName' + Math.random().toString(36).substring(2, 7);
  await wareHousePage.enterStockLocationFoColumn('Code', stockLocationCode);
  await wareHousePage.enterStockLocationFoColumn('Name', stockLocationName);
  const responsePromise = page.waitForResponse('**/Billing/Stock/Stock');
  await wareHousePage.saveWarehouseLocation();
  const res = await responsePromise;
  const resj = await res.json();
  const stockLocationId: number = resj.integerValue
  console.log('Created stock location id : ' + stockLocationId);
  await wareHousePage.goToWarehouseLocationTab();
  await wareHousePage.filterWarehouseByColumn('Code', warehouseLocationCode);
  await wareHousePage.verifyFilteredWarehouse(warehouseLocationCode);
  await salesBasePage.goToMenu('Stock', 'Inventory');
  await inventoryPage.createItem();
  await inventoryPage.selectWarehouseLocation(warehouseLocationName);
  await stockUtils.deleteWarehouseLocations(stockLocationId);
});


