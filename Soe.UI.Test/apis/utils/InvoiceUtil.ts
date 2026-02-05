import { type Page, expect } from '@playwright/test';
import { CoreAPI } from '../CoreAPI';

export class InvoiceUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly coreAPI: CoreAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.coreAPI = new CoreAPI(page, url);
    }

    async CreateCustomerInvoice(invoiceNumber: string) {
        const fs = require('fs');
        const filePath = this.basePathJsons + 'customer-invoice.json';
        const rawData = fs.readFileSync(filePath);
        const jsonData = JSON.parse(rawData);
        jsonData.invoice.invoiceNr = invoiceNumber;
        jsonData.invoice.dueDate = new Date().toISOString();
        const today = new Date();
        jsonData.invoice.voucherDate = new Date(today.getTime() + 3 * 24 * 60 * 60 * 1000).toISOString();
        jsonData.invoice.invoiceDate = new Date(today.getTime() + 3 * 24 * 60 * 60 * 1000).toISOString();
        jsonData.invoice.dueDate = new Date(today.getTime() + 3 * 24 * 60 * 60 * 1000).toISOString();
        await this.coreAPI.createCustomerInvoice(jsonData);
    }

}