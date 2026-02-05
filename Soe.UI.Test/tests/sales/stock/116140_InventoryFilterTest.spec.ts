import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { StockUtils } from '../../../apis/utils/StockUtils';
import { getAccountExValue } from '../../../utils/properties';


let testCaseId: string = '116140';
let envUrl: string;
let stockUtils: StockUtils;
let wareHouseLocationOne = 'Auto WH' + testCaseId + '-1';
let wareHouseLocationTwo = 'Auto WH' + testCaseId + '-2';
let inventoryNameOne = 'Auto ' + testCaseId + '-1';
let inventoryNameTwo = 'Auto ' + testCaseId + '-2';
let stockIdTwo: number;

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Stock");
  await allure.subSuite("Inventory");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  stockUtils = new StockUtils(page, envUrl);
  const stockIdOne: number = await stockUtils.createWareHouseLocation(wareHouseLocationOne);
  await stockUtils.createInventory(inventoryNameOne, stockIdOne);
  stockIdTwo = await stockUtils.createWareHouseLocation(wareHouseLocationTwo);
  await stockUtils.createInventory(inventoryNameTwo, stockIdTwo);
});

test(testCaseId + ': Filter test in inventory main grid : MG', { tag: ['@Sales', '@Stock', '@Regression'] }, async ({ page, salesBasePage, inventoryPage }) => {
  await salesBasePage.goToMenu('Stock', 'Inventory');
  await inventoryPage.searchInventoryByName(inventoryNameOne);
  await inventoryPage.verifyFilteredInventory(inventoryNameOne);
  await inventoryPage.verifyFilteredInventoryCount('1');
  await inventoryPage.clearGridFilter();
  await inventoryPage.verifyFilterIsCleared();
  await inventoryPage.searchInventoryByWarehouseLocation(wareHouseLocationOne);
  await inventoryPage.verifyFilteredInventory(inventoryNameOne);
  await inventoryPage.verifyFilteredInventoryCount('1');
  await inventoryPage.clearGridFilter();
  await inventoryPage.verifyFilterIsCleared();
  let inventoryNameThree = 'Auto ' + Math.random().toString(36).substring(2, 7);
  await stockUtils.createInventory(inventoryNameThree, stockIdTwo);
  await inventoryPage.releadRecords();
  await inventoryPage.searchInventoryByName(inventoryNameThree);
  await inventoryPage.verifyFilteredInventory(inventoryNameThree);
  await inventoryPage.clearGridFilter();
  await inventoryPage.verifyFilterIsCleared();
  await stockUtils.acceptInventory(inventoryNameThree);
  await inventoryPage.releadRecords();
  await inventoryPage.searchInventoryByName(inventoryNameThree);
  await inventoryPage.verifyFilteredInventoryCount('0');
});


