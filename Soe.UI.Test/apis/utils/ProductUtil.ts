import { type Page, expect } from '@playwright/test';
import { BillingAPI } from '../BillingAPI';
import fs from 'fs'

export class ProductUtils {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly billingAPI: BillingAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.billingAPI = new BillingAPI(page, url);
    }


    setJsonValues(obj: any, key: any, value: any) {
        const keys = key.split(".");
        let current = obj;
        while (keys.length > 1) {
            const part = keys.shift();
            if (!current[part]) current[part] = {};
            current = current[part];
        }
        current[keys[0]] = value;
    }

    async createProduct(productNumber: string, productName: string, productData: any = {}) {
        let keys = Object.keys(productData)
        let products = await this.billingAPI.getAllProducts();
        let productsResponse: any[] = await products.json();
        const filteredProducts = productsResponse.filter((product) => product.number === productNumber);
        if (filteredProducts.length > 0) {
            const existingProduct = filteredProducts[0]; // or return all if you expect multiple
            const { productId, number, name } = existingProduct;
            console.log('Product already exists with name: ' + existingProduct.name + ' and id: ' + existingProduct.number);
            return { productId, number, name };
        }
        const filePath = this.basePathJsons + 'product-invoice.json';
        const rawData = fs.readFileSync(filePath, 'utf-8');
        const jsonData = JSON.parse(rawData);
        jsonData.invoiceProduct.name = productName;
        jsonData.invoiceProduct.number = productNumber;
        for (let key of keys) {
            this.setJsonValues(jsonData, key, productData[key])
        }
        const response = await this.billingAPI.createInvoiceProduct(jsonData);
        let res = await response.json();
        const { integerValue } = res;
        expect(response.ok()).toBeTruthy();
        console.log('Product  created (API) product Number: ' + productNumber + ' product Name: ' + productName);
        return { productId: integerValue };
    }

    async getProducts() {
        let products = await this.billingAPI.getSelectProducts();
        return products;
    }

    async createPricelist(pricelistData: any = { name: "" }) {
        let keys = Object.keys(pricelistData)
        const filePath = this.basePathJsons + 'pricelist.json';
        const rawData = fs.readFileSync(filePath, 'utf-8');
        const jsonData = JSON.parse(rawData);
        for (let key of keys) {
            this.setJsonValues(jsonData, key, pricelistData[key])
        }
        const response = await this.billingAPI.createPricelist(pricelistData["priceListType.name"], jsonData);
        return response;
    }

}