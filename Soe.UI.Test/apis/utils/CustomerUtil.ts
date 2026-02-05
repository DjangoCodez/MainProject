import { Page } from "@playwright/test";
import { CoreAPI } from "../CoreAPI";
import fs from 'fs'
import { setJsonValues } from "utils/CommonUtil";

export class CustomerUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly coreApi: CoreAPI
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.coreApi = new CoreAPI(page, url);
    }


    async getCustomers() {
        const response = await this.coreApi.getCustomers();
        return response;
    }

    async createCustomer(customerData: any = {}) {
        const keys = Object.keys(customerData)
        const filePath = this.basePathJsons + 'customer.json';
        const rawData = fs.readFileSync(filePath, 'utf-8');
        const data = JSON.parse(rawData);
        const rand = Math.floor(100000 + Math.random() * 900000)
        const deleveryAddress = `DeleveryAddress${rand}`
        const invoiceAddres = `InvoiceAdderss${rand}`
        const customerName = `Customer${rand}`
        const customerNumber = rand
        const contactAddresses = [
            {
                "contactAddressItemType": 1,
                "isAddress": true,
                "sysContactAddressTypeId": 1,
                "name": "Delivery address",
                "displayAddress": deleveryAddress,
                "address": deleveryAddress,
                "ag_node_id": "0"
            },
            {
                "contactAddressItemType": 3,
                "isAddress": true,
                "sysContactAddressTypeId": 3,
                "name": "Invoice address",
                "displayAddress": invoiceAddres,
                "address": invoiceAddres,
                "ag_node_id": "1"
            }
        ]
        data.customer.contactAddresses = contactAddresses
        data.customer.customerNr = customerNumber
        data.customer.name = customerName
        for (let key of keys) {
            setJsonValues(data, key, customerData[key])
        }
        const { integerValue } = await this.coreApi.createCustomer(data)
        return { customerId: integerValue, customer: data.customer }
    }
}


