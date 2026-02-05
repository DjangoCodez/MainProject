import { expect, test } from '../../fixtures/sales-fixture';

import * as allure from "allure-js-commons";
import { StockUtils } from '../../../apis/utils/StockUtils';
import { getAccountExValue } from '../../../utils/properties';


let testCaseId: string = '116142';
let warehouseLocationName: string;
let stockId: number;
let envUrl: string;
let stockUtils: StockUtils;
let inventoryName = 'Auto ' + Math.random().toString(36).substring(2, 7);

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Stock");
  await allure.subSuite("Inventory");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  stockUtils = new StockUtils(page, envUrl);
  warehouseLocationName = 'Auto WHL' + testCaseId;
  stockId = await stockUtils.createWareHouseLocation(warehouseLocationName);
});

test.afterEach(async () => {
  await stockUtils.acceptInventory(inventoryName);
});

test(testCaseId + ': Add new inventory document : MG', { tag: ['@Sales', '@Stock', '@Regression'] }, async ({ page, salesBasePage, inventoryPage, balancePage }) => {
  await salesBasePage.goToMenu('Stock', 'Balance');
  await balancePage.waitforPageLoad();
  //check product balance is available for stock warehouse
  await balancePage.balanceMainGrid.filterByColumnNameAndValue('Warehouse Location', warehouseLocationName);
  const rowCount = await balancePage.balanceMainGrid.getRowCount('Warehouse Location');
  // If product not available for warehouse create product for stock
  if (rowCount == 0) {
    await balancePage.clearGridFilter();
    await stockUtils.addProducts(stockId);
    await balancePage.releadRecords();
    await balancePage.balanceMainGrid.filterByColumnNameAndValue('Warehouse Location', warehouseLocationName);
  }
  let productNumber: string = await balancePage.balanceMainGrid.getRowColumnValue('Product Number', 1) ?? '';
  // Update the balance for product for given row 
  await balancePage.balanceMainGrid.clickGridRow('Warehouse Location', 1);
  await balancePage.addQuantity('10');
  let note = 'Auto ' + Math.random().toString(36).substring(2, 7);
  await balancePage.addNote(note);
  await balancePage.saveBalance();
  await balancePage.clickBalanceTab();
  await balancePage.clearGridFilter();
  await balancePage.balanceMainGrid.verifyFilteredIsCleared();
  await balancePage.releadRecords();
  await balancePage.balanceMainGrid.filterByColumnNameAndValue('Warehouse Location', warehouseLocationName);
  let inStock = await balancePage.balanceMainGrid.getCellValueFromGrid('Product Number', productNumber, 'In Stock') ?? '0';
  await salesBasePage.goToMenu('Stock', 'Inventory');
  await inventoryPage.waitForPageLoad();
  await inventoryPage.createItem();
  console.log('Inventory name :' + inventoryName);
  await inventoryPage.enterName(inventoryName);
  await inventoryPage.selectWarehouseLocation(warehouseLocationName);
  await inventoryPage.generateDocumentation();
  expect(await inventoryPage.stockInvnetoryRowsGrid.getCellValueFromGrid('Product Number', productNumber, 'Stock Amount')).toEqual(inStock);
  await inventoryPage.saveInventory();
  await inventoryPage.closeInventoryTab();
  await inventoryPage.goToInventoryTab();
  await inventoryPage.releadRecords();
  await inventoryPage.searchInventoryByName(inventoryName);
  await inventoryPage.verifyFilteredInventory(inventoryName);
  await inventoryPage.selectInventoryByWarehouse();
  await inventoryPage.waitForInventoryRowsPageLoad();
  //let currentStockCount:number = +inStock;
  let newStockCount: number = +inStock + 5;
  await inventoryPage.stockInvnetoryRowsGrid.enterCellValuetoGrid('Product Number', productNumber, 'Inventoried Amount', String(newStockCount));
  await inventoryPage.saveInventory(false);
  expect(await inventoryPage.stockInvnetoryRowsGrid.getCellValueFromGrid('Product Number', productNumber, 'Difference')).toEqual('5.00');
  await inventoryPage.acceptInventory();
  await inventoryPage.acceptAcceptInventoryAlert();
  await page.waitForTimeout(2000);
  await salesBasePage.goToMenu('Stock', 'Balance');
  await balancePage.waitforPageLoad();
  await balancePage.balanceMainGrid.filterByColumnNameAndValue('Warehouse Location', warehouseLocationName);
  expect(await balancePage.balanceMainGrid.getCellValueFromGrid('Product Number', productNumber, 'In Stock')).toEqual(String(newStockCount) + '.00');
  //check the balance
  //voucher check for accounts, and values
  //seperate test cases for account priority product, warehouselocation, default
});


