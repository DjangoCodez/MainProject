import { expect, Page } from "@playwright/test";
import * as allure from "allure-js-commons";
import { SalesBasePage } from "../SalesBasePage";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";


export class ProjectOverviewPageJS extends SalesBasePage {
    readonly projectOverviewGrid: SingleGridPageJS;

    constructor(page: Page) {
        super(page);
        this.projectOverviewGrid = new SingleGridPageJS(page, 1, 'ctrl.gridHandler.gridAg.options.gridOptions');
    }

    async waitForPageLoad() {
        await allure.step("Wait for page load", async () => {
            const pageLocator = this.page.getByRole('button', { name: 'Load data' });
            await expect(pageLocator).toBeVisible({ timeout: 30_000 });
        });
    }

    async verifyIncomeInvoiced(incomeInvoiced: string) {
        await allure.step("Verify income invoiced", async () => {
            let incomeInvoicedValue = await this.projectOverviewGrid.getCellValueFromGrid('ag-Grid-AutoColumn', 'Income invoiced', 'value', true);
            if (typeof incomeInvoicedValue === 'string') {
                incomeInvoicedValue = incomeInvoicedValue.replace(/\u00A0/g, ' ').trim();
            }
            expect(incomeInvoicedValue).toBe(incomeInvoiced);
        });
    }

    async verifyUninvoiceIncome(incomeInvoiced: string) {
        await allure.step("Verify Uninvoiced income", async () => {
            let incomeInvoicedValue = await this.projectOverviewGrid.getCellValueFromGrid('ag-Grid-AutoColumn', 'Uninvoiced income', 'value', true);
            if (typeof incomeInvoicedValue === 'string') {
                incomeInvoicedValue = incomeInvoicedValue.replace(/\u00A0/g, ' ').trim();
            }
            expect(incomeInvoicedValue).toBe(incomeInvoiced);
        });
    }

    async verifyExpenses(expenses: string) {
        await allure.step("Verify expenses", async () => {
            let expensesValue = await this.projectOverviewGrid.getCellValueFromGrid('ag-Grid-AutoColumn', 'Expenses', 'value', true);
            if (typeof expensesValue === 'string') {
                expensesValue = expensesValue.replace(/\u00A0/g, ' ').trim();
            }
            expect(expensesValue).toBe(expenses);
        });
    }

    async verifyMaterialCost(expenses: string) {
        await allure.step("Verify material cost", async () => {
            let expensesValue = await this.projectOverviewGrid.getCellValueFromGrid('ag-Grid-AutoColumn', 'Material cost', 'value', true);
            if (typeof expensesValue === 'string') {
                expensesValue = expensesValue.replace(/\u00A0/g, ' ').trim();
            }
            expect(expensesValue).toBe(expenses);
        });
    }
}