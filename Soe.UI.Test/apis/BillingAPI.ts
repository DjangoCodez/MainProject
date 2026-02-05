import { type Page } from '@playwright/test';
import { deleteStock, getStock, getStocksDict, saveStock } from '../models/webapi/generated-service-endpoint/billing/StockV2.endpoints';
import { getStockProductProducts, saveStockTransaction } from '../models/webapi/generated-service-endpoint/billing/StockProduct.endpoints';
import { closeStockInventory, generateStockInventoryRows, getStockInventories, saveStockInventoryRows } from '../models/webapi/generated-service-endpoint/billing/StockInventory.endpoints';

export class BillingAPI {
    readonly page: Page;
    readonly dominaUrl: string;

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url + "/api/";
    }

    async getProducts() {
        const products = await this.page.request.get(this.dominaUrl + "V2/Billing/Product/Products/Small");
        return products;
    }

    async deletePurchase(page: Page, integerValue: string) {
        const deletePurchase = await page.request.delete(this.dominaUrl + "V2/Billing/PurchaseOrders/" + integerValue);
        return deletePurchase;
    }

    async getWarehouses() {
        const warehouse = await this.page.request.get(this.dominaUrl + getStocksDict(false, undefined));
        return warehouse;
    }

    async deleteWarehouse(stockId: number) {
        const warehouseDelete = await this.page.request.delete(this.dominaUrl + deleteStock(stockId));
        return warehouseDelete;
    }

    async createWarehouses(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + saveStock(), { data: jsonData });
        return response;
    }

    async getStockProducts(stockId: number) {
        const products = await this.page.request.get(this.dominaUrl + getStockProductProducts(stockId));
        return products;
    }

    async getStock(stockId: number) {
        const products = await this.page.request.get(this.dominaUrl + getStock(stockId, true));
        return products;
    }

    async createInvoiceProduct(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + "Billing/Product/SaveInvoiceProduct/", { data: jsonData });
        return response;
    }


    async generateInventoryRows(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + generateStockInventoryRows(), { data: jsonData });
        return response;
    }

    async saveInventory(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + saveStockInventoryRows(), { data: jsonData });
        return response;
    }

    async getInventories() {
        const inventories = await this.page.request.get(this.dominaUrl + getStockInventories(false));
        return inventories;
    }

    async acceptInventory(headerId: number) {
        const inventories = await this.page.request.get(this.dominaUrl + closeStockInventory(headerId));
        return inventories;
    }

    async createSupplierProduct(jsonData: Object, ang_version: string = 'old') {
        let response;
        if (ang_version == 'new') {
            response = await this.page.request.post(this.dominaUrl + "V2/Billing/Supplier/Product", { data: jsonData });
        } else {
            response = await this.page.request.post(this.dominaUrl + "Billing/Supplier/Product/", { data: jsonData });
        }
        if (!response.ok) {
            throw new Error('Failed to add supplier product');
        }
        return response;
    }

    async saveTransaction(jsonData: Object) {
        const transactions = await this.page.request.post(this.dominaUrl + saveStockTransaction(), { data: jsonData });
    }

    async getAllProducts() {
        const response = await this.page.request.get(this.dominaUrl + "Billing/Product/Products/false/true/false/true/true");
        return response;
    }

    async getSelectProducts() {
        const response = await this.page.request.get(this.dominaUrl + "Billing/Product/Products/ForSelect");
        let products = await response.json();
        if (products.length == 0) {
            throw new Error('Data Issue : There should be atleast one active product available in environment.');
        }
        return products;
    }

    async getProjects() {
        const response = await this.page.request.get(this.dominaUrl + "Billing/Invoice/Project/Small/true/true/true");
        return response;
    }

    async createProject(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + "/Billing/Project/", { data: jsonData });
        return response;
    }
    async getAgreementsGroups() {
        const response = await this.page.request.get(this.dominaUrl + 'V2/Billing/Contract/ContractGroup/?id=undefined')
        let groups = await response.json();
        return groups;
    }

    async createAgreement(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + 'Billing/Contract/', { data: jsonData });
        if (!response.ok) {
            throw new Error('Failed to add agreement: ' + response.status() + ' ' + response.statusText());
        }
        let agreement = await response.json();
        return agreement;
    }

    async createAgreementGroup(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + 'V2/Billing/Contract/ContractGroup/', { data: jsonData });
        if (!response.ok) {
            throw new Error('Failed to add agreement group: ' + response.status() + ' ' + response.statusText());
        }
        let agreementGroup = await response.json();
        return agreementGroup;
    }

    async getAgreementsGroupsPriceManagement() {
        const response = await this.page.request.get(this.dominaUrl + 'V2/Core/SysTermGroup/128/false/false/false')
        let priceManagements = await response.json();
        return priceManagements;
    }

    async getAgreementsGroupsPeriod() {
        const response = await this.page.request.get(this.dominaUrl + 'V2/Core/SysTermGroup/127/false/false/false')
        let periods = await response.json();
        return periods;
    }

    async createProductGroup(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + 'V2/Billing/ProductGroups/ProductGroup', { data: jsonData });
        if (!response.ok) {
            throw new Error('Failed to add product group: ' + response.status() + ' ' + response.statusText());
        }
        let productGroup = await response.json();
        return productGroup;
    }

    async getProductGroups() {
        const response = await this.page.request.get(this.dominaUrl + 'V2/Billing/ProductGroups')
        let groups = await response.json();
        return groups;
    }

    async createProductUnit(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + 'V2/Billing/Product/ProductUnit', { data: jsonData });
        if (!response.ok) {
            throw new Error('Failed to add product unit: ' + response.status() + ' ' + response.statusText());
        }
        let productUnit = await response.json();
        return productUnit;
    }

    async getProductUnits() {
        const response = await this.page.request.get(this.dominaUrl + 'V2/Billing/Product/ProductUnit/Grid')
        let unit = await response.json();
        return unit;
    }

    async createSupplierInvoice(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + '/Economy/Supplier/Invoice/', { data: jsonData });
        if (!response.ok) {
            throw new Error('Failed to create supplier invoice: ' + response.status() + ' ' + response.statusText());
        }
        let supplierInvoice = await response.json();
        return supplierInvoice;
    }

    async createOrder(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + 'Billing/Order/', { data: jsonData });
        if (!response.ok()) {
            throw new Error('Failed to add order: ' + response.status() + ' ' + response.statusText());
        }
        let order = await response.json();
        return order;
    }

    async getPricelist(pricelist: string) {
        const response = await this.page.request.get(this.dominaUrl + 'V2/Billing/PriceList/PriceListTypes/Grid?priceListTypeId=undefined')
        let priceLists = await response.json() as [{ name: string, priceListTypeId: number }]
        return priceLists.find(p => p.name === pricelist)
    }

    async createPricelist(pricelistName: string, jsonData: {}) {
        const pricelist = await this.getPricelist(pricelistName)
        if (!pricelist) {
            console.log(jsonData)
            const response = await this.page.request.post(this.dominaUrl + 'V2/Billing/PriceList/PriceListTypes/', { data: jsonData });
            if (!response.ok) {
                throw new Error('Failed to add agreement group: ' + response.status() + ' ' + response.statusText());
            }
            const { integerValue } = await response.json();
            return { priceListTypeId: integerValue, name: pricelistName };
        }
        return pricelist
    }
}