import { type Page, expect } from '@playwright/test';
import { BillingAPI } from '../BillingAPI';
import { getDateUtil, getFormatYYMMDD } from '../../utils/CommonUtil';

export class OrderUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly billingAPI: BillingAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.billingAPI = new BillingAPI(page, url);
    }

    async CreateOrder(customerId: number = 0) {
        const fs = require('fs');
        const filePath = this.basePathJsons + 'order.json';
        const rawData = fs.readFileSync(filePath);
        const jsonData = JSON.parse(rawData);
        jsonData.modifiedFields.invoicedate = await getDateUtil(2, true);
        jsonData.modifiedFields.voucherdate = await getDateUtil(2, true);
        jsonData.modifiedFields.duedate = await getDateUtil(2, true);
        jsonData.modifiedFields.invoicetext = getFormatYYMMDD() + 'Planning_CreateOrder';
        jsonData.modifiedFields.workingdescription = getFormatYYMMDD() + 'Planning_CreateOrder';
        jsonData.modifiedFields.origindescription = getFormatYYMMDD() + 'Planning_CreateOrder';
        jsonData.modifiedFields.actorid = customerId > 0 ? customerId : jsonData.modifiedFields.actorid
        jsonData.modifiedFields.plannedstartdate = await getDateUtil(0);
        const order = await this.billingAPI.createOrder(jsonData);
        console.log('Order is created (API)');
        return order.value;
    }



}