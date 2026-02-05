import { type Page } from '@playwright/test';

export class EconomyAPI {
    readonly page: Page;
    readonly dominaUrl: string;

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
    }

    async getActiveSupliers() {
        let url = this.dominaUrl + "/api/V2/Economy/Supplier/Supplier/Dict/?onlyActive=true&addEmptyRow=false";
        const response = await this.page.request.get(url);
        const suppliers = await response.json();
        if (suppliers.length == 0) {
            throw new Error('Data Issue : There should be atleast one active supplier available in environment.');
        }
        return suppliers as [];
    }

    async createVoucher(jsonData: JSON) {
        const response = await this.page.request.post(this.dominaUrl + "/api/Economy/Accounting/Voucher/", { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to create customer invoice: ${response.status()} ${response.statusText()}`);
        }
        this.CalculateAccountBalanceForAccountsFromVoucher();
        this.GetAccountBalances();
        return response;
    }

    async CalculateAccountBalanceForAccountsFromVoucher() {
        const payload = { AccountYearId: 1 };
        const response = await this.page.request.post(this.dominaUrl + "/api/Economy/Accounting/AccountBalance/CalculateAccountBalanceForAccountsFromVoucher/", { data: payload });
        if (!response.ok()) {
            throw new Error(`Failed to calculate account balance: ${response.status()} ${response.statusText()}`);
        }
        return response;
    }

    async GetAccountBalances() {
        const response = await this.page.request.post(this.dominaUrl + "/api/Economy/Accounting/AccountBalance/GetAccountBalances/1", { data: {} });
        if (!response.ok()) {
            throw new Error(`Failed to get account balances: ${response.status()} ${response.statusText()}`);
        }
        return response;
    }

    async getSuppliers() {
        let url = this.dominaUrl + "/api/V2/Economy/Supplier/Supplier/?onlyActive=false&supplierId=undefined";
        const response = await this.page.request.get(url);
        return await response.json();
    }

    async getNextSupplierNumber() {
        const response = await this.page.request.get(this.dominaUrl + '/api/V2/Economy/Supplier/Supplier/NextSupplierNr/');
        if (!response.ok()) {
            throw new Error('Failed to get next supplier number: ' + response.status() + ' ' + response.statusText());
        }
        return await response.json();
    }

    async createSupplier(jsonData: Object) {
        const response = await this.page.request.post(this.dominaUrl + '/api/V2/Economy/Supplier/Supplier', { data: jsonData })
        if (!response.ok()) {
            throw new Error('Failed to get next supplier number: ' + response.status() + ' ' + response.statusText());
        }
        const { integerValue } = await response.json();
        return { integerValue };
    }

    async updateSettings(data: any, c: number = 90) {
        const url = `${this.dominaUrl}/soe/economy/preferences/suppinvoicesettings/default.aspx?c=${c}`;
        console.log('Updating settings at URL:', url);
        const response = await this.page.request.post(url, { headers: { 'Content-Type': 'application/x-www-form-urlencoded', }, form: data });
        console.log('Settings update response status:', response.status());
        if (!response.ok()) {
            throw new Error('Failed to get next supplier number: ' + response.status() + ' ' + response.statusText());
        }
    }

    async getfinancialPeriods() {
        let url = this.dominaUrl + "/api/Economy/Accounting/AccountYear/All//false/false";
        const response = await this.page.request.get(url);
        type FinancialPeriod = {
            accountYearId: number;
            actorCompanyId: number;
            from: string;
            to: string;
            status: number;
            created: string;
            createdBy: string;
            modified: string;
            modifiedBy: string;
            noOfPeriods: number;
            yearFromTo: string;
        }
        return await response.json() as Array<FinancialPeriod>;
    }

    async getFinancialPeriodByYear(year: number) {
        console.log('Getting financial period for year:', year);
        const url = this.dominaUrl + `/api/Economy/Accounting/AccountYear/${year}/true`;
        const response = await this.page.request.get(url);
        return await response.json();
    }

    async createOrUpdateFinancialPeriod(jsonData: JSON) {
        const response = await this.page.request.post(this.dominaUrl + "/api/Economy/Accounting/AccountYear/", { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to create or update financial period: ${response.status()} ${response.statusText()}`);
        }
        return response;
    }

    async getVoucherSeries() {
        const url = this.dominaUrl + "/api/Economy/Accounting/VoucherSeriesType/";
        const response = await this.page.request.get(url);
        type Voucher = {
            voucherSeriesTypeId: number;
            name: string;
            voucherSeriesTypeNr: number;
            startNr: number;
        }
        return await response.json() as Array<Voucher>;
    }

}
