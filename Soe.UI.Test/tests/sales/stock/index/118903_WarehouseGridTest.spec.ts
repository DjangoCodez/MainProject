import { test } from '../../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { StockUtils } from '../../../../apis/utils/StockUtils';
import { getAccountExValue } from '../../../../utils/properties';


let testCaseId: string = '118903';
let envUrl: string;
let stockUtils: StockUtils;
let wareHouseLocationOne = 'Auto WH' + testCaseId + '-1';
let wareHouseLocationTwo = 'Auto WH' + testCaseId + '-2';

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Stock");
  await allure.subSuite("Warehouse Location");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  stockUtils = new StockUtils(page, envUrl);
  await stockUtils.createWareHouseLocation(wareHouseLocationOne);
  await stockUtils.createWareHouseLocation(wareHouseLocationTwo);
});

test(testCaseId + ': Filter warehouse location grid : MG', { tag: ['@Sales', '@Stock', '@Regression'] }, async ({ salesBasePage, wareHousePage }) => {
  await salesBasePage.goToMenu('Stock', 'Warehouse Location', true);
  await wareHousePage.filterWarehouseByColumn('Name', wareHouseLocationOne);
  await wareHousePage.verifyFilteredWarehouse(wareHouseLocationOne);
  await wareHousePage.verifyFilteredWarehouseCount('1');
  await wareHousePage.clearGridFilter();
  await wareHousePage.verifyFiltereIsCleared();
  await wareHousePage.filterWarehouseByColumn('Code', 'Code' + wareHouseLocationOne);
  await wareHousePage.verifyFilteredWarehouse('Code' + wareHouseLocationOne);
  await wareHousePage.verifyFilteredWarehouseCount('1');
  await wareHousePage.clearGridFilter();
  await wareHousePage.verifyFiltereIsCleared();
  let wareHouseLocationThree = 'Auto WH' + Math.random().toString(36).substring(2, 7);
  const stockIdThree: number = await stockUtils.createWareHouseLocation(wareHouseLocationThree);
  await wareHousePage.releadRecords();
  await wareHousePage.filterWarehouseByColumn('Name', wareHouseLocationOne);
  await wareHousePage.verifyFilteredWarehouse(wareHouseLocationOne);
  await wareHousePage.verifyFilteredWarehouseCount('1');
  await wareHousePage.clearGridFilter();
  await wareHousePage.verifyFiltereIsCleared();
  await stockUtils.deleteWarehouseLocations(stockIdThree);
});



