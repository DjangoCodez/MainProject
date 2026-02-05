import { type Page, expect } from '@playwright/test';
import { BillingAPI } from '../BillingAPI';
import { EconomyAPI } from '../EconomyAPI';

export class SupplierInvoiceUtil {

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

    async CreateSupplierInvoice(invoiceNumber: string, isPreliminary: boolean = false) {
        const fs = require('fs');
        const filePath = this.basePathJsons + 'supplier-invoice.json';
        const rawData = fs.readFileSync(filePath);
        const jsonData = JSON.parse(rawData);
        jsonData.invoice.invoiceNr = invoiceNumber;
        jsonData.invoice.invoiceDate = new Date().toISOString();
        jsonData.invoice.dueDate = new Date().toISOString();
        jsonData.invoice.currencyDate = new Date().toISOString();
        jsonData.invoice.originStatus = isPreliminary ? 1 : 2;
        const today = new Date();
        jsonData.invoice.voucherDate = new Date(today.getTime() + 3 * 24 * 60 * 60 * 1000).toISOString();
        if (Array.isArray(jsonData.invoice.accountingRows)) {
            jsonData.invoice.accountingRows = jsonData.invoice.accountingRows.map((row: any) => ({
            ...row,
            date: new Date().toISOString()
            }));
        }
        if (Array.isArray(jsonData.accountingRows)) {
            jsonData.accountingRows = jsonData.accountingRows.map((row: any) => ({
            ...row,
            date: new Date().toISOString()
            }));
        }
        const supplierInvoice = await this.billingAPI.createSupplierInvoice(jsonData);  
        console.log('Supplier invoice is created old (API)');
    }

}