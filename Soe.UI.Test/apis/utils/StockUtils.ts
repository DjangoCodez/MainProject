import { type Page, expect } from '@playwright/test';
import { BillingAPI } from '../BillingAPI';
import * as StockModels from '../models/StockDefault';

export class StockUtils {

  readonly page: Page;
  readonly dominaUrl: string;
  readonly billingAPI: BillingAPI;
  readonly basePathJsons: string = './apis/jsons/';

  constructor(page: Page, url: string) {
    this.page = page;
    this.dominaUrl = url;
    this.billingAPI = new BillingAPI(page, url);
  }


  async createWareHouseLocation(testCaseId: string) {
    const warehouse = await this.billingAPI.getWarehouses()
    const warehouseList = await warehouse.json();
    let isWarehouseExist: boolean = false;
    let stockId: number = 0;
    let whareHouseName: string = testCaseId;
    for (let index = 0; index < warehouseList.length; index++) {
      const element = warehouseList[index];
      if (element.name === whareHouseName) {
        isWarehouseExist = true;
        stockId = element.id;
        break;
      }
    }

    if (!isWarehouseExist) {
      const jsonData = StockModels.defaultStockDTO;
      jsonData.name = whareHouseName;
      jsonData.code = 'Code' + whareHouseName;
      jsonData.stockShelves[0].code = 'CodeL' + testCaseId;
      jsonData.stockShelves[0].name = 'NameL' + testCaseId;
      jsonData.stockShelves[0].stockName = whareHouseName;
      console.log('Warehouse creation ');
      const response = await this.billingAPI.createWarehouses(jsonData);
      const wareHouseCreated = await response.json();
      stockId = wareHouseCreated.integerValue;
      expect(response.ok()).toBeTruthy();
      console.log('Warehouse location created (API)' + whareHouseName)
    }
    return stockId;
  }


  async addProducts(stockId: number) {
    const products = await this.billingAPI.getStockProducts(stockId);
    const productList = await products.json();

    const maxProduct = productList.reduce((max: { productId: number; }, item: { productId: number; }) => {
      return item.productId > max.productId ? item : max;
    }, productList[0]);

    let lastNum: number = 0;
    if (maxProduct === undefined) {
      //let lastNumber: string = maxProduct.number;
      let lastNumberStr = '000';
      lastNum = +lastNumberStr;
    } else {
      let lastNumber: string = maxProduct.number;
      let lastNumberStr = lastNumber.split(" ", 1)[0];
      lastNum = +lastNumberStr;
    }

    const stockResponse = await this.billingAPI.getStock(stockId);
    const stockDetails = await stockResponse.json();

    const fs = require('fs');
    const filePath = this.basePathJsons + 'product-inventory.json';
    const rawData = fs.readFileSync(filePath);
    const jsonData = JSON.parse(rawData);

    for (let index = 0; index < 1; index++) {
      let randomName = Math.random().toString(36).substring(2, 7);
      jsonData.invoiceProduct.number = '00' + (lastNum + index + 1) + ' Auto' + randomName;
      jsonData.invoiceProduct.name = '00' + (lastNum + index + 1) + ' Auto' + randomName;

      jsonData.stocks[0].stockId = stockId;
      jsonData.stocks[0].name = stockDetails.name;
      jsonData.stocks[0].code = stockDetails.code;;
      jsonData.stocks[0].stockShelfId = stockDetails.stockShelves[0].stockShelfId;
      jsonData.stocks[0].stockShelfName = stockDetails.stockShelves[0].name;

      console.log('Creating product');
      const response = await this.billingAPI.createInvoiceProduct(jsonData);
      expect(response.ok()).toBeTruthy();
      console.log('Product added(API)');
      await this.page.waitForTimeout(1000);
      let resj = await response.json();
      return resj.integerValue;
    }
  }

  async addProductForStatistics(productNumber: string, productName: string) {
    const fs = require('fs');
    const filePath = this.basePathJsons + 'product-statistics.json';
    const rawData = fs.readFileSync(filePath, 'utf-8');
    const jsonData = JSON.parse(rawData);

    jsonData.invoiceProduct.number = productNumber;
    jsonData.invoiceProduct.name = productName;

    console.log('Creating statistics product:', productNumber);
    const response = await this.billingAPI.createInvoiceProduct(jsonData);
    expect(response.ok()).toBeTruthy();
    console.log('Statistics product created (API):', productNumber);

    const responseJson = await response.json();
    return responseJson.integerValue;
  }

  async createInventory(inventoryName: string, stockId: number) {

    let inventoryId: number = 0;
    const invnetories = await this.billingAPI.getInventories();
    const inventoryList = await invnetories.json();

    const stockInventoryHeadId = getStockInventoryHeadId(inventoryList, inventoryName);

    if (stockInventoryHeadId !== null) {
      console.log(`Stock Inventory Head ID Already Exist: ${stockInventoryHeadId}` + ' ' + inventoryName);
      inventoryId = stockInventoryHeadId;
    } else {
      console.log(`Inventyory "${inventoryName}" not found.`);
      console.log(`Creating "${inventoryName}" inventory.`);
      const jsonData = StockModels.defaultStockInventoryFilterDTO;
      jsonData.stockId = stockId;

      let response = await this.billingAPI.generateInventoryRows(jsonData);
      let resj = await response.json();
      if (resj.length < 1) {
        await this.addProducts(stockId);
        response = await this.billingAPI.generateInventoryRows(jsonData);
        resj = await response.json();
      }

      const jsonDataInventory = StockModels.defaultStockInventoryHeadDTO;

      jsonDataInventory.headerText = inventoryName;
      jsonDataInventory.stockId = stockId;
      jsonDataInventory.stockInventoryRows = resj;

      const inventoryResponse = await this.billingAPI.saveInventory(jsonDataInventory);
      expect(inventoryResponse.ok()).toBeTruthy();
      console.log('Inventory created(API) :' + inventoryName);
    }

  }

  async acceptInventory(inventoryName: string) {
    const invnetories = await this.billingAPI.getInventories();
    const inventoryList = await invnetories.json();
    const stockInventoryHeadId: number = getStockInventoryHeadId(inventoryList, inventoryName) ?? 0;
    const inventoryResponse = await this.billingAPI.acceptInventory(stockInventoryHeadId);
    expect(inventoryResponse.ok()).toBeTruthy();
    console.log('Inventory ' + inventoryName + ' accepted(API), will remove from grid record');
  }

  async deleteWarehouseLocations(stockId: number) {
    const deleteWarehouseResponse = await this.billingAPI.deleteWarehouse(stockId);
    expect(deleteWarehouseResponse.ok()).toBeTruthy();
    console.log('Warehouse location ' + stockId + ' is deleted (API)');
  }

  // async changeStockBalance(stockId: number) {
  //   const jsonData = StockModels.defaultStockTransaction();
  //   jsonData.
  //   const deleteWarehouseResponse = await this.billingAPI.saveTransaction();
  //   expect(deleteWarehouseResponse.ok()).toBeTruthy();
  //   console.log('Warehouse location ' + stockId + ' is deleted (API)');
  // }

};



type InventoryItem = {
  stockInventoryHeadId: number;
  headerText: string;
  inventoryStart: string;
  stockName: string;
  createdBy: string;
};

function getStockInventoryHeadId(
  data: InventoryItem[],
  headerTextToCheck: string
): number | null {
  // Find the item with the matching headerText
  const foundItem = data.find(item => item.headerText === headerTextToCheck);
  // If the item is found, return the stockInventoryHeadId, otherwise return null
  return foundItem ? foundItem.stockInventoryHeadId : null;
}