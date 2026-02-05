import { Page } from "@playwright/test";
import { EconomyAPI } from "../EconomyAPI";

export class FinanceUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly economyapi: EconomyAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.economyapi = new EconomyAPI(page, url);
    }



    async getFinancialPeriods() {
        return await this.economyapi.getfinancialPeriods();
    }

    async enableFinancialPeriods() {
        const currentYear = new Date().getFullYear();
        const previursYear = currentYear - 1;
        const financePeriods = await this.economyapi.getfinancialPeriods();
        const latestFinancePeriods = financePeriods.filter((period) => period.from.includes(currentYear.toString()) || period.from.includes(previursYear.toString()));
        const previousYear = currentYear - 1;
        // Close old open periods not in latest years
        await Promise.all(
            financePeriods
                .filter(p => !latestFinancePeriods.includes(p) && p.status === 2)
                .map(p => this.updateFinancialPeriodStatus(p, "Closed", "warningColor"))
        );
        // Create latest periods if not present and open them
        const isCurrentYearPresent = latestFinancePeriods.some((period) => period.from.includes(currentYear.toString()));
        if (!isCurrentYearPresent) {
            await this.createFinancialPeriodForYear(currentYear);
            const period = (await this.economyapi.getfinancialPeriods()).find((period) => period.from.includes(currentYear.toString()));
            await this.updateFinancialPeriodStatus(period, "Open", "okColor");
        }

        const missingYears = [currentYear, previousYear].filter(
            year => !latestFinancePeriods.some(period => period.from.includes(year.toString()))
        );
        for (const year of missingYears) {
            await this.createFinancialPeriodForYear(year);
            const periods = await this.economyapi.getfinancialPeriods();
            const period = periods.find(p => p.from.includes(year.toString()));
            if (period) {
                await this.updateFinancialPeriodStatus(period, "Open", "okColor");
            }
        }
        // Ensure latest periods are open
        for (const period of latestFinancePeriods) {
            if (period.status !== 2) {
                await this.updateFinancialPeriodStatus(period, "Open", "okColor");
            }
        }
    }

    async updateFinancialPeriodStatus(period: any, statusName: "Open" | "Closed" | "Not started" | "Locked", iconColor: "okColor" | "warningColor") {
        const { accountYearId, actorCompanyId, from, created, createdBy, modified, modifiedBy } = period;
        const financialYear = await this.economyapi.getFinancialPeriodByYear(accountYearId);
        const firstPeriod = financialYear.periods.find((p: { periodNr: number }) => p.periodNr === 1);

        if (!firstPeriod) {
            throw new Error('First period not found in financial year');
        }
        const year = parseInt(from.split('-')[0]);

        const statusMap = {
            "Not started": 1,
            "Open": 2,
            "Closed": 3,
            "Locked": 4
        };
        const status = statusMap[statusName];

        const periods = Array.from({ length: 12 }, (_, i) => {
            const month = i + 1;
            return {
                accountPeriodId: firstPeriod.accountPeriodId + i,
                accountYearId,
                periodNr: month,
                from: new Date(Date.UTC(year, i, 1)).toISOString(),
                to: new Date(Date.UTC(year, month, 0)).toISOString(),
                status,
                startValue: `${year}${month.toString().padStart(2, '0')}`,
                created,
                createdBy,
                modified,
                modifiedBy,
                isDeleted: false,
                hasExistingVouchers: false,
                statusName,
                monthName: new Date(2024, i, 1).toLocaleString('en-US', { month: 'long' }),
                periodName: `${year}-${month}`,
                statusIcon: `fas fa-circle ${iconColor}`,
                ag_node_id: "0"
            };
        });

        const payload = {
            accountYear: {
                accountYearId,
                actorCompanyId,
                from: `${year}-01-01T00:00:00.000Z`,
                to: `${year}-12-31T00:00:00.000Z`,
                status,
                created,
                createdBy,
                modified,
                modifiedBy,
                periods,
                noOfPeriods: 0,
                yearFromTo: `${year}0101 - ${year}1231`
            },
            voucherSeries: [],
            keepNumbers: false
        };

        await this.economyapi.createOrUpdateFinancialPeriod(payload as unknown as JSON);
    }

    async getVoucherSeries() {
        const voucherSeries = await this.economyapi.getVoucherSeries();
        return voucherSeries.map((series, index) => ({
            voucherSeriesTypeId: series.voucherSeriesTypeId,
            voucherSeriesTypeName: series.name,
            voucherSeriesTypeNr: series.voucherSeriesTypeNr,
            startNr: series.startNr,
            isModified: true,
            ag_node_id: `"${series.voucherSeriesTypeNr - 1}"`
        }));
    }

    async createFinancialPeriodForYear(year: number) {
        const vouchers = await this.getVoucherSeries();
        const periods = Array.from({ length: 12 }, (_, i) => ({
            from: new Date(Date.UTC(year, i, 1)).toISOString(),
            periodNr: i + 1,
            status: 1,
            isModified: true,
            statusName: "Not started",
            monthName: new Date(2024, i, 1).toLocaleString('en-US', { month: 'long' }),
            periodName: `${year}-${i + 1}`,
            statusIcon: "fas fa-circle",
            ag_node_id: `${i}`
        }));

        const payload = {
            accountYear: {
            status: 1,
            periods,
            from: `${year}-01-01T00:00:00.000Z`,
            to: `${year}-12-31T00:00:00.000Z`
            },
            voucherSeries: vouchers,
            keepNumbers: false
        };

        await this.economyapi.createOrUpdateFinancialPeriod(payload as unknown as JSON);
    }

}

