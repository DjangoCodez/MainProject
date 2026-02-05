import { type Page, expect } from '@playwright/test';
import { BillingAPI } from '../BillingAPI';
import { EconomyAPI } from '../EconomyAPI';
import { defaultSupplierProductDTO } from '../models/productDefault';
import { getDateUtil, getVersion } from '../../utils/CommonUtil';
import { AngVersion } from '../../enums/AngVersionEnums';
import fs from 'fs';

export class SupplierUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly billingAPI: BillingAPI;
    readonly economyAPI: EconomyAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.billingAPI = new BillingAPI(page, url);
        this.economyAPI = new EconomyAPI(page, url);
    }

    async getSupplier() {
        let suppliers = await this.economyAPI.getActiveSupliers();
        return suppliers;
    }

    async CreateSupplierProduct(supplierId: string, productId: string, supplierProductNr: string) {

        const ang_version: AngVersion = await getVersion('ProductPage');
        if (ang_version === AngVersion.NEW) {
            const newProduct = { ...defaultSupplierProductDTO };
            newProduct.supplierId = Number(supplierId);
            newProduct.productId = Number(productId);
            newProduct.supplierProductNr = supplierProductNr;
            newProduct.supplierProductName = supplierProductNr + " Product Name";
            newProduct.priceRows[0].quantity = 200;
            newProduct.priceRows[0].price = 500;
            newProduct.priceRows[0].startDate = new Date(await getDateUtil(35));
            newProduct.priceRows[0].endDate = new Date(await getDateUtil(30));
            const supplierProductResponse = await this.billingAPI.createSupplierProduct(newProduct, ang_version);
            expect(supplierProductResponse.ok()).toBeTruthy();
            console.log('Supplier product is created new (API)');
        } else {
            const filePath = this.basePathJsons + 'product-supplier.json';
            const rawData = fs.readFileSync(filePath, 'utf-8');
            const jsonData = JSON.parse(rawData);
            jsonData.product.supplierId = Number(supplierId);
            jsonData.product.productId = Number(productId);
            jsonData.product.supplierProductName = supplierProductNr + " Product Name";
            jsonData.product.supplierProductNr = supplierProductNr;
            const supplier = await this.billingAPI.createSupplierProduct(jsonData);
            expect(supplier.ok()).toBeTruthy();
            console.log('Supplier product is created old (API)');
        }
    }

    async getSuppliers() {
        interface Supplier { supplierNr: string; name: string; }
        const suppliers = await this.economyAPI.getSuppliers();
        return suppliers as Supplier[];
    }

    async getNextSupplierNumber() {
        const nextSupplierNumber = await this.economyAPI.getNextSupplierNumber();
        return nextSupplierNumber;
    }
}