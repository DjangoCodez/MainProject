import { Page } from "@playwright/test";

export class CoreAPI {

    readonly page: Page;
    readonly dominaUrl: string;

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url + "/api/";
    }


    async getCustomers() {
        const response = await this.page.request.get(this.dominaUrl + 'Core/Customer/Customer/?onlyActive=true');
        if (!response.ok()) {
            throw new Error(`Failed to fetch customers: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async createCustomerInvoice(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + "Economy/Common/CustomerLedger/", { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to create customer invoice: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async createCustomer(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + "Core/Customer/Customer/", { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to create customer : ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

}