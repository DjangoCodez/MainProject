import { IHttpService } from "../../Services/httpservice";
import { Constants } from "../../../Util/Constants";
import { ISmallGenericType, ISelectablePayrollTypeDTO, IPayrollProductGridDTO, ITimePeriodDTO, IReportJobDefinitionDTO, ISelectableTimePeriodDTO, IActionResult, ISelectablePayrollMonthYearDTO, IProjectGridDTO } from "../../../Scripts/TypeLite.Net4";
import { TermGroup_TimePeriodType, SoeReportTemplateType, TimeSchedulePlanningDisplayMode, TermGroup_EmployeeSelectionAccountingType, SoeModule } from "../../../Util/CommonEnumerations";
import { PayrollGroupSmallDTO } from "../../../Common/Models/PayrollGroupDTOs";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { TimePeriodDTO } from "../../../Common/Models/TimePeriodDTO";
import { TimePeriodHeadDTO } from "../../../Common/Models/TimePeriodHeadDTO";
import { IMessagingService } from "../../Services/MessagingService";
import { DateRangeDTO } from "../../../Common/Models/DateRangeDTO";
import { ReportPrintoutDTO } from "../../../Common/Models/ReportDTOs";
import { AccountYearDTO, AccountYearLightDTO } from "../../../Common/Models/AccountYear";
import { Insight, MatrixLayoutColumn } from "../../../Common/Models/MatrixResultDTOs";
import { UserGridDTO } from "../../../Common/Models/UserDTO";
import { PayrollPriceTypeDTO, PayrollPriceTypePeriodDTO } from "../../../Common/Models/PayrollPriceTypeDTOs";
import { StringUtility } from "../../../Util/StringUtility";

export interface IReportDataService {
    // GET
    getAccountDims(): ng.IPromise<AccountDimSmallDTO[]>;
    getAccountDimsSmall(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, useCache?: boolean, loadInactives?: boolean): ng.IPromise<AccountDimSmallDTO[]>;
    getAccountYears(): ng.IPromise<AccountYearLightDTO[]>;
    getAccountYearIntervals(id: number): ng.IPromise<AccountYearDTO>;
    getAllPayrollTimePeriods(): ng.IPromise<ISelectableTimePeriodDTO[]>;
    getDefaultTimePeriods(): ng.IPromise<ITimePeriodDTO[]>;
    getEmployeeCategories(): ng.IPromise<ISmallGenericType[]>;
    getEmployeeGroups(): ng.IPromise<ISmallGenericType[]>;
    getEmployeesDict(addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean): ng.IPromise<any>;
    getGroupsAndSorts(reportId: number, feature: number): ng.IPromise<ISmallGenericType[]>;
    getInventories(): ng.IPromise<any>;
    getInventoryAccounts(): ng.IPromise<any>;
    getInventorySettingAccounts(): ng.IPromise<any>;
    getMatrixLayoutColumns(sysReportTemplateTypeId: number, module: SoeModule): ng.IPromise<MatrixLayoutColumn[]>;
    getMatrixGridResult(reportPrintoutId: number): ng.IPromise<ReportPrintoutDTO>;
    getInsights(sysReportTemplateTypeId: number, module: SoeModule): ng.IPromise<Insight[]>;
    getPayrollGroups(): ng.IPromise<ISmallGenericType[]>;
    getPayrollMonths(): ng.IPromise<ISelectablePayrollMonthYearDTO[]>;
    getPayrollProducts(): ng.IPromise<IPayrollProductGridDTO[]>;
    getPayrollTypes(): ng.IPromise<ISelectablePayrollTypeDTO[]>;
    getPayrollYears(): ng.IPromise<ISelectablePayrollMonthYearDTO[]>;
    getProjects(onlyActive: boolean, hidden: boolean, setStatusName: boolean, includeManagerName: boolean, loadOrders: boolean, projectStatus: number): ng.IPromise<IProjectGridDTO[]>;
    getShiftTypes(): ng.IPromise<ISmallGenericType[]>;
    getStocksDict(addEmptyRow: boolean): ng.IPromise<any>;
    getStockInventories(): ng.IPromise<any>;
    getStockPlaces(addEmptyRow: boolean, stockId: number): ng.IPromise<any>;
    getProductGroups(addEmptyRow: boolean): ng.IPromise<any>;
    getSysTimeIntervalDateRange(sysTimeIntervalId: number): ng.IPromise<DateRangeDTO>;
    getSysTimeIntervals(): ng.IPromise<ISmallGenericType[]>;
    getTimeAccumulators(includeVacationBalance: boolean, includeWorkTimeAccountBalance: boolean): ng.IPromise<SmallGenericType[]>;
    getTimePeriodHeads(type: TermGroup_TimePeriodType, loadTypeNames: boolean, loadTimePeriods: boolean): ng.IPromise<TimePeriodHeadDTO[]>;
    getUsersByCompanyDate(actorCompanyId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, userCompanyRoleDate: Date): ng.IPromise<UserGridDTO[]>;
    getVacationGroups(): ng.IPromise<ISmallGenericType[]>;
    getVoucherRowMergeTypes(useCache?: boolean): ng.IPromise<any>;
    getVoucherSeriesTypes(useCache?: boolean): ng.IPromise<any>;
    getVoucherSeries(fromYearId: number, toYearId: number, useCache: boolean): ng.IPromise<any>;
    getBudgetHeadsDict(): ng.IPromise<ISmallGenericType[]>;
    getPayrollPriceTypes(): ng.IPromise<PayrollPriceTypeDTO[]>;
    getCurrentAccountYear(): ng.IPromise<AccountYearDTO>;

    // POST
    saveFavorite(reportId: number): ng.IPromise<IActionResult>;
    renameFavorite(reportId: number, name: string): ng.IPromise<IActionResult>;

    validateReportJob(job: IReportJobDefinitionDTO): ng.IPromise<IActionResult>;
    createReportJob(job: IReportJobDefinitionDTO, toggleReportMenu?: boolean): ng.IPromise<ReportPrintoutDTO>;
    reCreateReportJob(reportPrintoutId: number, toggleReportMenu?: boolean, force?: boolean): ng.IPromise<IActionResult>;
    createInsight(job: IReportJobDefinitionDTO): ng.IPromise<ReportPrintoutDTO>;

    getFilteredEmployees(fromDate: Date, toDate: Date, timePeriodIds: number[], soeReportTemplateType: SoeReportTemplateType, accountingType: TermGroup_EmployeeSelectionAccountingType, accountIds?: number[], categoryIds?: number[], employeeGroupIds?: number[], payrollGroupIds?: number[], vacationGroupIds?: number[], includeInactive?: boolean, onlyInactive?: boolean, includeEnded?: boolean, includeVacant?: boolean, includeHidden?: boolean, includeSecondary?: boolean): ng.IPromise<ISmallGenericType[]>

    // DELETE
    deleteFavorite(reportId: number): ng.IPromise<IActionResult>;
}

type TimePeriodApi = {
    displayName: string;
    id: number;
    start: string;
    stop: string;
}

export class ReportDataService implements IReportDataService {

    //@ngInject
    constructor(private httpService: IHttpService, private messagingService: IMessagingService, private $timeout: ng.ITimeoutService, private $q: ng.IQService) {
    }

    // GET
    public getAccountDims(): ng.IPromise<AccountDimSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_PLANNING_ACCOUNT_DIM + true + "/" + false + "/" + TimeSchedulePlanningDisplayMode.Admin + "/" + false, false).then(x => {
            return x.map(y => {
                var obj = new AccountDimSmallDTO();
                angular.extend(obj, y);

                if (obj.accounts) {
                    obj.accounts = _.sortBy(obj.accounts.map(a => {
                        var aObj = new AccountDTO();
                        angular.extend(aObj, a);
                        return aObj;
                    }), a => a.name);

                    // Copy all accounts as filtered accounts (meaning show all)
                    obj.filteredAccounts = _.sortBy(obj.accounts.map(a => {
                        var aObj = new AccountDTO();
                        angular.extend(aObj, a);
                        return aObj;
                    }), a => a.name);
                }

                obj.selectedAccounts = [];

                return obj;
            });
        });
    }
    public getCurrentAccountYear(): ng.IPromise<AccountYearDTO> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_CURRENT_ACCOUNT_YEAR,false).then(x => {
            return x;
        });
    }

    public getAccountYears(addEmptyRow: boolean = false): ng.IPromise<AccountYearLightDTO[]> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR_DICT + addEmptyRow + "/" + false, false).then(x => {
            return x;
        });
    }
    public getAccountYearIntervals(id: number): ng.IPromise<AccountYearDTO> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR + id + "/" + true, false).then(x => {
            return x;
        });
    }
    public getAllPayrollTimePeriods(): ng.IPromise<ISelectableTimePeriodDTO[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_TIMEPERIODS_PAYROLL, true).then(timePeriods => {
            return timePeriods.map(t => <ISelectableTimePeriodDTO>{
                displayName: t.displayName,
                id: t.id,
                start: new Date(t.start),
                stop: new Date(t.stop),
                paymentDate: new Date(t.paymentDate)
            });
        });
    }
    public getBudgetHeadsDict(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_BUDGET_HEADS, true);
    }

    public getDefaultTimePeriods(): ng.IPromise<TimePeriodDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD, true).then(x => {
            return x.map(y => {
                let obj = new TimePeriodDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    public getEmployeeCategories(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_EMPLOYEE_CATEGORIES, true);
    }

    public getEmployeeGroups(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.getCache(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GROUP + "?addEmptyRow=" + false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    public getGroupsAndSorts(reportId: number, feature: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_GROUPSSORTS + "?reportId=" + reportId + "&feature=" + feature, true);
    }

    public getMatrixLayoutColumns(sysReportTemplateTypeId: number, module: SoeModule): ng.IPromise<MatrixLayoutColumn[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_MATRIX_LAYOUT_COLUMNS + sysReportTemplateTypeId + '/' + module, false).then(x => {
            return x.map(y => {
                let obj = new MatrixLayoutColumn();
                angular.extend(obj, y);
                obj.setTypes();
                return obj;
            });
        });
    }

    public getMatrixGridResult(reportPrintoutId: number): ng.IPromise<ReportPrintoutDTO> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_JOBS_FOR_MENU_MATRIX_GRID + reportPrintoutId, false);
    }

    public getInsights(sysReportTemplateTypeId: number, module: SoeModule): ng.IPromise<Insight[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_INSIGHTS + sysReportTemplateTypeId + '/' + module, false).then(x => {
            return x.map(y => {
                let obj = new Insight();
                angular.extend(obj, y);
                obj.setTypes();
                return obj;
            });
        });
    }

    public getPayrollGroups(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP + "?addEmptyRow=" + false, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_LONG, false).then((groups: PayrollGroupSmallDTO[]) => {
            return groups.map(g => <ISmallGenericType>{
                id: g.payrollGroupId,
                name: g.name
            })
        });
    }

    public getPayrollMonths(): ng.IPromise<ISelectablePayrollMonthYearDTO[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_PAYROLL_MONTHS, true).then(timePeriods => {
            return timePeriods.map(t => <ISelectablePayrollMonthYearDTO>{
                displayName: t.displayName,
                id: t.id,
                timePeriodIds: t.timePeriodIds
            });
        });
    }

    public getPayrollProducts(): ng.IPromise<IPayrollProductGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT, true);
    }

    public getPayrollTypes(): ng.IPromise<ISelectablePayrollTypeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_PAYROLLTYPES, true);
    }

    public getPayrollYears(): ng.IPromise<ISelectablePayrollMonthYearDTO[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_PAYROLL_YEARS, true).then(timePeriods => {
            return timePeriods.map(t => <ISelectablePayrollMonthYearDTO>{
                displayName: t.displayName,
                id: t.id,
                timePeriodIds: t.timePeriodIds

            });
        });
    }

    public getShiftTypes(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_SHIFT_TYPES, true);
    }

    public getSysTimeIntervalDateRange(sysTimeIntervalId: number): ng.IPromise<DateRangeDTO> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_SYS_TIME_INTERVAL_DATE_RANGE + "?sysTimeIntervalId=" + sysTimeIntervalId, false).then(x => {
            let obj = new DateRangeDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }

    public getSysTimeIntervals(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_SYS_TIME_INTERVALS, true);
    }

    public getTimeAccumulators(includeVacationBalance: boolean, includeWorkTimeAccountBalance: boolean): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ACCUMULATOR + "?addEmptyRow=" + false + "&includeVacationBalance=" + includeVacationBalance + "&includeWorkTimeAccountBalance=" + includeWorkTimeAccountBalance, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    public getTimePeriodHeads(type: TermGroup_TimePeriodType, loadTypeNames: boolean, loadTimePeriods: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD + "?type=" + type + "&loadTypeNames=" + loadTypeNames + "&loadTimePeriods=" + loadTimePeriods, false, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(y => {
                let obj = new TimePeriodHeadDTO();
                angular.extend(obj, y);

                if (y.timePeriods) {
                    obj.timePeriods = obj.timePeriods.map(p => {
                        let pObj = new TimePeriodDTO();
                        angular.extend(pObj, p);
                        pObj.fixDates();
                        return pObj;
                    });
                } else {
                    obj.timePeriods = [];
                }

                return obj;
            });
        });
    }

    getUsersByCompanyDate(actorCompanyId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, userCompanyRoleDate: Date): ng.IPromise<UserGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_BY_COMPANY + actorCompanyId + "/" + setDefaultRoleName + "/" + includeInactive + "/" + includeEnded + "/" + userCompanyRoleDate.toDateTimeString(), false).then(x => {
            return x.map(y => {
                let obj = new UserGridDTO();
                angular.extend(obj, y);

                return obj;
            });
        });
    }

    public getVacationGroups(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_GROUP + false, false);
    }

    public getVoucherRowMergeTypes(useCache: boolean = true) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_ROW_MERGE_TYPE, useCache);
    }

    public getVoucherSeriesTypes(useCache: boolean = true) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES_TYPE, useCache);
    }

    public getVoucherSeries(fromYearId: number, toYearId: number, useCache: boolean = true): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES_BY_YEAR_RANGE + '/' + fromYearId + '/' + toYearId, useCache);
    }

    public getInventories(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORIES_DICT, false);
    }

    public getInventoryAccounts(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORYACCOUNTS,false);
    }

    public getInventorySettingAccounts(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_SETTINGACCOUNTS, false);
    }

    getProjects(onlyActive: boolean, hidden: boolean, setStatusName: boolean, includeManagerName: boolean, loadOrders: boolean, projectStatus: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT + onlyActive + "/" + hidden + "/" + setStatusName + "/" + includeManagerName + "/" + loadOrders + "/" + projectStatus, false);
    }

    getEmployeesDict(addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_EMPLOYEE + "?addEmptyRow=" + addEmptyRow + "&concatNumberAndName=" + concatNumberAndName + "&getHidden=" + getHidden + "&orderByName=" + orderByName, null, Constants.CACHE_EXPIRE_LONG);
    }

    getStocksDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_STOCK_DICT + addEmptyRow, false);
    }

    getStockPlaces(addEmptyRow: boolean, stockId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PLACE + addEmptyRow + "/" + stockId, false);
    }

    getProductGroups(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PRODUCT_GROUP + addEmptyRow, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_LONG, false);
    }

    getStockInventories() {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_INVENTORIES, false);
    }

    getPayrollPriceTypes() {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_TYPE, false).then(x => {
            return x.map(y => {
                let obj = new PayrollPriceTypeDTO();
                angular.extend(obj, y);
                obj.fixDates();

                if (y.periods) {
                    obj.periods = y.periods.map(p => {
                        let pObj = new PayrollPriceTypePeriodDTO();
                        angular.extend(pObj, p);
                        pObj.fixDates();
                        return pObj;
                    })
                } else {
                    obj.periods = [];
                }

                return obj;
            });
        });
    }

    // POST

    public saveFavorite(reportId: number): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_REPORT_FAVORITE + reportId, null);
    }

    public renameFavorite(reportId: number, name: string): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_REPORT_FAVORITE + reportId + "/" + name, null);
    }

    public validateReportJob(job: IReportJobDefinitionDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORT_JOBS_FOR_MENU_VALIDATE, job);
    }

    public createReportJob(job: IReportJobDefinitionDTO, toggleReportMenu: boolean = false): ng.IPromise<ReportPrintoutDTO> {
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORT_JOBS_FOR_MENU, job).then((reportPrintout: any) => {
            if (toggleReportMenu)
                this.messagingService.publish(Constants.EVENT_SHOW_REPORT_MENU, null);
            return reportPrintout;
        });
    }

    public reCreateReportJob(reportPrintoutId: number, toggleReportMenu: boolean = false, forceValidation: boolean = false): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORT_JOBS_FOR_MENU + reportPrintoutId + "/" + forceValidation, null).then((result: any) => {
            if (toggleReportMenu)
                this.messagingService.publish(Constants.EVENT_SHOW_REPORT_MENU, null);
            return result;
        });
    }

    public createInsight(job: IReportJobDefinitionDTO): ng.IPromise<ReportPrintoutDTO> {
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORT_JOBS_FOR_MENU_INSIGHT, job).then((reportPrintout: any) => {
            return reportPrintout;
        });
    }

    public getFilteredEmployees(fromDate: Date, toDate: Date, timePeriodIds: number[], soeReportTemplateType: SoeReportTemplateType, accountingType: TermGroup_EmployeeSelectionAccountingType, accountIds?: number[], categoryIds?: number[], employeeGroupIds?: number[], payrollGroupIds?: number[], vacationGroupIds?: number[], includeInactive: boolean = false, onlyInactive: boolean = false, includeEnded: boolean = false, includeVacant: boolean = false, includeHidden: boolean = false, includeSecondary: boolean = false): ng.IPromise<ISmallGenericType[]> {
        var model = {
            fromDate: fromDate,
            toDate: toDate,
            soeReportTemplateType: soeReportTemplateType,
            accountingType: accountingType,
            employeeGroupIds: employeeGroupIds,
            categoryIds: categoryIds,
            vacationGroupIds: vacationGroupIds,
            payrollGroupIds: payrollGroupIds,
            accountIds: accountIds,
            timePeriodIds: timePeriodIds,
            includeInactive: includeInactive,
            onlyInactive: onlyInactive,
            includeEnded: includeEnded,
            includeVacant: includeVacant,
            includeHidden: includeHidden,
            includeSecondary: includeSecondary
        };

        return this.httpService.post(Constants.WEBAPI_REPORT_DATA_EMPLOYEES, model);
    }

    // DELETE

    public deleteFavorite(reportId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_REPORT_FAVORITE + reportId);
    }

    public getAccountDimsSmall(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactives: boolean = false) {
        const url = StringUtility.buildUrl(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM, {
            onlyStandard: onlyStandard,
            onlyInternal: onlyInternal,
            loadAccounts: loadAccounts,
            loadInternalAccounts: loadInternalAccounts,
            loadInactives: loadInactives
        } as Record<string, boolean>);

        return this.httpService.get(url, false).then((accountDims: AccountDimSmallDTO[]) => {
            return accountDims.map(accountDim => {
                let accountDimDTO = new AccountDimSmallDTO();
                angular.extend(accountDimDTO, accountDim);
                if (accountDimDTO.accounts) {
                    accountDimDTO.accounts = accountDimDTO.accounts.map(account => {
                        let accountDTO = new AccountDTO();
                        angular.extend(accountDTO, account);
                        return accountDTO;
                    }).sort((a, b) => {
                        if (a.accountNr < b.accountNr)
                            return -1;
                        if (a.accountNr > b.accountNr)
                            return 1;
                        return 0;
                    });
                }
                return accountDimDTO;
            })
        });
    }
}