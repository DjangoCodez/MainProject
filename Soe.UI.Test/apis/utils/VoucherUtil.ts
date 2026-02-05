import { expect, Page } from "@playwright/test";
import { EconomyAPI } from "../EconomyAPI";

export class VoucherUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly economyApi: EconomyAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.economyApi = new EconomyAPI(page, url);
    }

    /**
     * Updates accounting rows with the provided entries.
     * 
     * @param entries Array of accounting entries to apply.
     * Each entry should include:
     *  - type   ("debit" or "credit")
     *  - amount (number to update)
     *  - balance (balance)
     *  - amount_2 (same should be passed ammount one but credit should has negative value)
     * 
     * Example:
     * [
     *   { type: "debit", amount: 100, balance: "100", amount_2: 100 },
     *   { type: "credit", amount: 100, balance: "100", amount_2: -100 }
     * ]
     */
    async createVoucher(text: string, entries: { type: string; amount: number; balance: number; amount_2: number }[] = [{ type: "debit", amount: 100, balance: 100, amount_2: 100 }, { type: "credit", amount: -100, balance: -100, amount_2: -100 }]) {
        const fs = require('fs');
        const filePath = this.basePathJsons + 'voucher.json';
        const rawData = fs.readFileSync(filePath);
        const jsonData = JSON.parse(rawData);
        jsonData.voucherHead.text = text;
        const today = new Date();
        jsonData.date = new Date(today.getTime() + 3 * 24 * 60 * 60 * 1000).toISOString();
        jsonData.voucherHead.voucherNr = Math.floor(Math.random() * 1200000);
        if (Array.isArray(jsonData.voucherHead.accountingRows)) {
            jsonData.voucherHead.accountingRows = jsonData.voucherHead.accountingRows.map((row: any, index: number) => ({
                ...row,
                balance: entries[index]?.balance ?? row.balance,
                amount: entries[index]?.amount ?? row.amount,
                amountEntCurrency: entries[index]?.amount_2 ?? row.amount_2,
                amountCurrency: entries[index]?.amount_2 ?? row.amount_2,
                amountLedgerCurrency: entries[index]?.amount_2 ?? row.amount_2,
                creditAmount: entries[index]?.type === "credit" ? Math.abs(entries[index]?.amount) : 0,
                creditAmountCurrency: entries[index]?.type === "credit" ? Math.abs(entries[index]?.amount) : 0,
                creditAmountEntCurrency: entries[index]?.type === "credit" ? Math.abs(entries[index]?.amount) : 0,
                creditAmountLedgerCurrency: entries[index]?.type === "credit" ? Math.abs(entries[index]?.amount) : 0,
                debitAmount: entries[index]?.type === "debit" ? entries[index]?.amount : 0,
                debitAmountCurrency: entries[index]?.type === "debit" ? entries[index]?.amount : 0,
                debitAmountEntCurrency: entries[index]?.type === "debit" ? entries[index]?.amount : 0,
                debitAmountLedgerCurrency: entries[index]?.type === "debit" ? entries[index]?.amount : 0,
            }));
        }
        if (Array.isArray(jsonData.accountingRows)) {
            jsonData.accountingRows = jsonData.accountingRows.map((row: any, index: number) => ({
                ...row,
                balance: entries[index]?.balance ?? row.balance,
                amount: entries[index]?.amount_2 ?? row.amount_2,
                amountEntCurrency: entries[index]?.amount_2 ?? row.amount_2,
                amountCurrency: entries[index]?.amount_2 ?? row.amount_2,
                amountLedgerCurrency: entries[index]?.amount_2 ?? row.amount_2,
                creditAmount: entries[index]?.type === "credit" ? Math.abs(entries[index]?.amount) : 0,
                creditAmountCurrency: entries[index]?.type === "credit" ? Math.abs(entries[index]?.amount) : 0,
                creditAmountEntCurrency: entries[index]?.type === "credit" ? Math.abs(entries[index]?.amount) : 0,
                creditAmountLedgerCurrency: entries[index]?.type === "credit" ? Math.abs(entries[index]?.amount) : 0,
                debitAmount: entries[index]?.type === "debit" ? entries[index]?.amount : 0,
                debitAmountCurrency: entries[index]?.type === "debit" ? entries[index]?.amount : 0,
                debitAmountEntCurrency: entries[index]?.type === "debit" ? entries[index]?.amount : 0,
                debitAmountLedgerCurrency: entries[index]?.type === "debit" ? entries[index]?.amount : 0,
            }));
        }
        const supplier = await this.economyApi.createVoucher(jsonData);
        expect(supplier.ok()).toBeTruthy();
        console.log('Voucher is created old (API)');
    }
}


