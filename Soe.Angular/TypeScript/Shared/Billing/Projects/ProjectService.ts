import { IHttpService } from "../../../Core/Services/HttpService";
import { ISmallGenericType, IEmployeeTimeCodeDTO, IProductComparisonDTO, IProjectCentralStatusDTO, ITimeCodeDTO, ITimeDeviationCauseDTO, IEmployeeDTO, IEmployeeSmallDTO, ITimeProjectDTO, IProjectTimeBlockSaveDTO, IProjectTimeMatrixDTO, IProjectTimeMatrixSaveDTO, IProjectPrintDTO } from "../../../Scripts/TypeLite.Net4";
import { ProjectTimeBlockDTO, ProjectSmallDTO } from "../../../Common/Models/ProjectDTO";
import { SoeOriginType, SoeTimeCodeType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { Guid } from "../../../Util/StringUtility";

export interface IProjectService {

    // GET
    getEmployeesForProject(addEmptyRow: boolean, getHidden: boolean, addNoReplacementEmployee: boolean, includeEmployeeId?: number): ng.IPromise<ISmallGenericType[]>;
    getEmployeesForProjectTimeCode(addEmptyRow: boolean, getHidden: boolean, addNoReplacementEmployee: boolean, includeEmployeeId?: number, fromDate?: Date, toDate?: Date, employeeCategories?: number[]): ng.IPromise<IEmployeeTimeCodeDTO[]>;
    getPricelists(comparisonPriceListTypeId: number, priceListTypeId: number, loadAll: boolean, priceDate: Date): ng.IPromise<IProductComparisonDTO[]>;
    getAllPriceLists(): ng.IPromise<any>
    getProjectCentralStatus(projectId: number, includeChildProjects: boolean, from: Date, to: Date, loadDetails: boolean): ng.IPromise<IProjectCentralStatusDTO[]>;
    getProjectUsers(projectId: number, loadTypeNames: boolean): ng.IPromise<any>;
    getProject(projectId: number): ng.IPromise<any>;
    getProjectGridDTO(projectId: number): ng.IPromise<any>;
    getProjectProductRows(projectId: number, originType: SoeOriginType, includeChildProject: boolean, fromDate: Date, toDate: Date): ng.IPromise<any>;
    getTimeCodes(projectId: number): ng.IPromise<any>;
    getTimeCodesByType(timeCodeType: SoeTimeCodeType, onlyActive: boolean, loadPayrollProducts: boolean, onlyWithInvoiceProduct?: boolean): ng.IPromise<ITimeCodeDTO[]>;
    getTimeDeviationCauses(employeeGroupId: number, getEmployeeGroups?: boolean, onlyUseInTimeTerminal?:boolean): ng.IPromise<ITimeDeviationCauseDTO[]>;
    getEmployeesForTimeProjectRegistration(projectId: number): ng.IPromise<IEmployeeDTO[]>;
    getEmployeesForTimeProjectRegistrationDict(projectId: number): ng.IPromise<any>;
    getEmployeesForTimeProjectRegistrationSmall(projectId: number, fromDate: Date, toDate: Date): ng.IPromise<IEmployeeTimeCodeDTO[]>;
    getEmployeeForUser(): ng.IPromise<IEmployeeSmallDTO>;
    getEmployeeForUserWithTimeCode(dateFrom:Date): ng.IPromise<IEmployeeTimeCodeDTO>;
    getEmployeeAndGroupForUser(): ng.IPromise<IEmployeeDTO>;
    getProjectTotals(projectId: number, recordId: number, recordType: number): ng.IPromise<any>;
    getCustomersDict(onlyActive: boolean, addEmptyRow: boolean): ng.IPromise<any>;
    getProjectTimeBlocksForTimeSheetFiltered(employeeId: number, dateFrom: Date, dateTo: Date, employees: number[], projects: number[], orders: number[], employeeCategories: number[], groupByDate: boolean, incPlannedAbsence: boolean, incInternOrderText: boolean, timeDeviationCauses: number[]): ng.IPromise<ProjectTimeBlockDTO[]>
    getProjectTimeBlocksForTimeSheetFilteredByProject(dateFrom: Date, dateTo: Date, projectId: number, includeChildProjects: boolean, employeeId: number): ng.IPromise<ProjectTimeBlockDTO[]>
    getProjectTimeBlocksForMatrix(employeeId: number, selectedEmployeeId: number, dateFrom: Date, dateTo: Date, isCopying:boolean): ng.IPromise<IProjectTimeMatrixDTO[]>
    getProjectStatisticsReportUrl(invoiceId: number, projectId: number, dateFrom: Date, dateTo: Date, employees: number[]): ng.IPromise<any>;
    getProjectExpenseReportUrl(invoiceId: number, projectId: number): ng.IPromise<any>;
    getProjectsForTimeSheet(employeeId: number): ng.IPromise<ProjectSmallDTO[]>;
    getProjectsForTimeSheetEmployees(employeeIds: number[], projectId?: number): ng.IPromise<ProjectSmallDTO[]>;
    getAttestWorkFlowGroupsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getFirstEligableTimeForEmployee(employeeId: number, date: Date): ng.IPromise<any>;
    getEmployeeChilds(employeeId: number): ng.IPromise<any>;
    migrateToProjectTimeBlocks(guid: Guid): ng.IPromise<any>;
    getMigrateToProjectTimeBlocksResult(guid: Guid): ng.IPromise<any>;

    // POST
    saveProject(project: ITimeProjectDTO, priceLists: any[], categoryRecords: any[], accountSettings: any[], projectUsers: any[], newPricelist: boolean, pricelistName: string): ng.IPromise<any>;
    validateSaveProjectTimeBlocks(items: any): ng.IPromise<any>;
    saveProjectTimeBlocks(projectTimeBlockSaveDTOs: IProjectTimeBlockSaveDTO[]): ng.IPromise<any>;
    saveNotesForProjectTimeBlock(projectTimeBlockSaveDTOs: IProjectTimeBlockSaveDTO): ng.IPromise<any>;
    saveAttestForTransactionsValidation(items: any, attestStateId: number, isMySelf: boolean): ng.IPromise<any>;
    saveAttestForTransactions(items: any, attestStateId: number, isMySelf: boolean): ng.IPromise<any>;
    recalculateWorkTime(projectTimeBlockSaveDTOs: IProjectTimeBlockSaveDTO[]): ng.IPromise<any>;
    moveTimeRowsToOrder(customerInvoiceId: number, projectTimeBlockIds: number[]): ng.IPromise<any>;
    moveTimeRowsToDate(selectedDate: Date, projectTimeBlockIds: number[]): ng.IPromise<any>;
    moveTimeRowsToOrderRow(customerInvoiceId: number, customerInvoiceRowId: number, projectTimeBlockIds: number[]): ng.IPromise<any>;
    saveProjectTimeMatrixBlocks(projectTimeMatrixBlockDTOs: IProjectTimeMatrixSaveDTO[]): ng.IPromise<any>;

    // DELETE
    deleteProject(projectId: number): ng.IPromise<any>;
}

export class ProjectService implements IProjectService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) {
    }

    // GET
    getEmployeesForProject(addEmptyRow: boolean, getHidden: boolean, addNoReplacementEmployee: boolean, includeEmployeeId?: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PROJECT + addEmptyRow + "/" + getHidden + "/" + addNoReplacementEmployee + "/" + includeEmployeeId, false);
    }

    getEmployeesForProjectTimeCode(addEmptyRow: boolean, getHidden: boolean, addNoReplacementEmployee: boolean, includeEmployeeId?: number, fromDate?: Date, toDate?: Date, employeeCategories?: number[]): ng.IPromise<IEmployeeTimeCodeDTO[]> {
        const fromDateString: string = (fromDate) ? fromDate.toDateTimeString() : null;
        const toDateString: string = (toDate) ? toDate.toDateTimeString() : null;

        const model = {
            addEmptyRow: addEmptyRow,
            getHidden: getHidden,
            addNoReplacementEmployee: addNoReplacementEmployee,
            includeEmployeeId: includeEmployeeId,
            fromDateString: fromDateString,
            toDateString: toDateString,
            employeeCategories: employeeCategories
        }

        return this.httpService.post(Constants.WEBAPI_BILLING_PROJECT_EMPLOYEES, model);
    }

    getPricelists(comparisonPriceListTypeId: number, priceListTypeId: number, loadAll: boolean, priceDate: Date) {
        let dateString: string = null;
        if (priceDate)
            dateString = priceDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT_PRICELIST + comparisonPriceListTypeId + "/" + priceListTypeId + "/" + loadAll + "/" + dateString, false);
    }

    getAllPriceLists() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST, false);
    }

    getProjectCentralStatus(projectId: number, includeChildProjects: boolean, from: Date, to: Date, loadDetails: boolean) {
        let dateFromString: string = null;
        if (from)
            dateFromString = from.toDateTimeString();
        let dateToString: string = null;
        if (to)
            dateToString = to.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT_PROJECTCENTRALSTATUS + projectId + "/" + includeChildProjects + "/" + dateFromString + "/" + dateToString + "/" + loadDetails, false);
    }

    getTimeCodes(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT_TIMECODES + "?projectId=" + projectId, false);
    }

    getProjectUsers(projectId: number, loadTypeNames: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT_USERS + "?projectId=" + projectId + "&loadTypeNames=" + loadTypeNames, false);
    }

    getProject(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT + projectId, false);
    }

    getProjectGridDTO(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT_GRIDDTO + projectId, false);
    }

    getProjectProductRows(projectId: number, originType: SoeOriginType, includeChildProject: boolean, fromDate: Date, toDate: Date) {
        let dateFromString: string = null;
        if (fromDate)
            dateFromString = fromDate.toDateTimeString();
        let dateToString: string = null;
        if (toDate)
            dateToString = toDate.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT_PRODUCTROWS + projectId + "/" + originType + "/" + includeChildProject + "/" + dateFromString + "/" + dateToString, false);
    }


    getTimeCodesByType(timeCodeType: SoeTimeCodeType, onlyActive: boolean, loadPayrollProducts: boolean, onlyWithInvoiceProduct = false) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE + "?timeCodeType=" + timeCodeType + "&onlyActive=" + onlyActive + "&loadPayrollProducts=" + loadPayrollProducts + "&onlyWithInvoiceProduct=" + onlyWithInvoiceProduct, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getTimeDeviationCauses(employeeGroupId: number, getEmployeeGroups = false, onlyUseInTimeTerminal = false) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + "?employeeGroupId=" + employeeGroupId + "&getEmployeeGroups=" + getEmployeeGroups + "&onlyUseInTimeTerminal=" + onlyUseInTimeTerminal, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_VERY_SHORT,false);
        //return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + "?employeeGroupId=" + employeeGroupId + "&getEmployeeGroups=" + getEmployeeGroups + "&onlyUseInTimeTerminal=" + onlyUseInTimeTerminal, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getEmployeesForTimeProjectRegistration(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_EMPLOYEES + projectId, false);
    }

    getEmployeesForTimeProjectRegistrationDict(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_EMPLOYEES_DICT + projectId, false);
    }

    getEmployeesForTimeProjectRegistrationSmall(projectId: number, fromDate: Date, toDate: Date) {
        const fromDateString: string = fromDate ? fromDate.toDateTimeString() : null;
        const toDateString: string = toDate ? toDate.toDateTimeString() : null;
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_EMPLOYEES_SMALL + projectId + "/" + fromDateString + "/" + toDateString, false);
    }

    getEmployeeForUser() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_FOR_USER, false);
    }

    getEmployeeForUserWithTimeCode(dateFrom:Date) {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_FOR_USER_TIMECODE + dateFromString, false);
    }

    getEmployeeAndGroupForUser() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_AND_GROUP_FOR_USER, false);
    }

    getProjectTotals(projectId: number, recordId: number, recordType: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_TOTALS + projectId + "/" + recordId + "/" + recordType, false);
    }

    getEmployeeChilds(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT_EMPLOYEE_CHILDS + employeeId, false);
    }

    getCustomersDict(onlyActive: boolean, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER + "?onlyActive=" + onlyActive + "&addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getProjectTimeBlocksForTimeSheetFiltered(employeeId: number, dateFrom: Date, dateTo: Date, employees: number[], projects: number[], orders: number[], employeeCategories: number[], groupByDate: boolean, incPlannedAbsence: boolean, incInternOrderText: boolean, timeDeviationCauses: number[]) {
        const model = {
            employeeId: employeeId,
            from: dateFrom,
            to: dateTo,
            employees: employees,
            projects: projects,
            orders: orders,
            groupByDate: groupByDate,
            incPlannedAbsence: incPlannedAbsence,
            incInternOrderText: incInternOrderText,
            employeeCategories: employeeCategories,
            timeDeviationCauses: timeDeviationCauses
        }

        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_TIMEBLOCKSFORTIMESHEETFILTERED, model);
    }

    getProjectTimeBlocksForTimeSheetFilteredByProject(dateFrom: Date, dateTo: Date, projectId: number, includeChildProjects: boolean, employeeId: number): ng.IPromise<ProjectTimeBlockDTO[]> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(`${Constants.WEBAPI_CORE_PROJECT_TIMEBLOCKSFORTIMESHEETFILTEREDBYPROJECT}${dateFromString}/${dateToString}/${projectId}/${includeChildProjects}/${employeeId}`, false)
    }

    getProjectTimeBlocksForMatrix(employeeId: number, selectedEmployeeId: number, dateFrom: Date, dateTo: Date, isCopying: boolean) {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT_TIMEBLOCKSFORMATRIX + employeeId + "/" + selectedEmployeeId + "/" + dateFromString + "/" + dateToString + "/" + isCopying, false);
    }

    getProjectStatisticsReportUrl(invoiceId: number, projectId: number, dateFrom: Date, dateTo: Date, employees: number[]) {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_PROJECTSTATISTICSREPORT + invoiceId + "/" + projectId + "/" + dateFromString + "/" + dateToString + (employees && employees.length > 0 ? "/?selectedEmployees=" + employees.join(',') : ""), false);
    }

    getProjectExpenseReportUrl(invoiceId: number, projectId: number) {
        return this.httpService.get(Constants.WEBAPI_EXPENSE_REPORT + invoiceId + "/" + projectId, false);
    }
    getProjectsForTimeSheet(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_PROJECTSFORTIMESHEET + employeeId, false);
    }

    getProjectsForTimeSheetEmployees(employeeIds: number[], projectId:number = null) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_PROJECTSFORTIMESHEET_EMPLOYEES + (projectId ? projectId.toString() : "0") + "/?empIds=" + employeeIds.join(','), false);
    }

    getAttestWorkFlowGroupsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getFirstEligableTimeForEmployee(employeeId: number, date: Date) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_EMPLOYEEFIRSTTIME + employeeId + "/" + date.toDateTimeString(), false);
    }

    migrateToProjectTimeBlocks(guid: Guid) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT_MIGRATE + guid, false);
    }

    getMigrateToProjectTimeBlocksResult(guid: Guid) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT_MIGRATE_RESULT + guid, false);
    }


    // POST
    saveProject(project: ITimeProjectDTO, priceLists: any[], categoryRecords: any[], accountSettings: any[], projectUsers: any[], newPricelist: boolean, pricelistName: string) {
        const model = {
            invoiceProject: project,
            priceLists: priceLists,
            categoryRecords: categoryRecords,
            accountSettings: accountSettings,
            projectUsers: projectUsers,
            newPricelist: newPricelist,
            pricelistName: pricelistName
        }
        return this.httpService.post(Constants.WEBAPI_BILLING_PROJECT, model);
    }

    validateSaveProjectTimeBlocks(items: any) {
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_VALIDATEPROJECTTIMEBLOCKSAVEDTO, items);
    }

    saveProjectTimeBlocks(projectTimeBlockSaveDTOs: IProjectTimeBlockSaveDTO[]) {
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_PROJECTTIMEBLOCKSAVEDTO, projectTimeBlockSaveDTOs);
    }

    saveNotesForProjectTimeBlock(projectTimeBlockSaveDTOs: IProjectTimeBlockSaveDTO) {
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_SAVENOTESFORPROJECTTIMEBLOCK, projectTimeBlockSaveDTOs);
    }

    saveProjectTimeMatrixBlocks(projectTimeMatrixBlockDTOs: IProjectTimeMatrixSaveDTO[]) {
        return this.httpService.post(Constants.WEBAPI_BILLING_PROJECT_TIMEBLOCKSFORMATRIX, projectTimeMatrixBlockDTOs);
    }

    saveAttestForTransactionsValidation(items: any, attestStateToId: number, isMySelf: boolean) {
        const model = { items: items, attestStateToId: attestStateToId, isMySelf: isMySelf };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TRANSACTIONS_VALIDATION, model);
    }

    saveAttestForTransactions(items: any, attestStateToId: number, isMySelf: boolean) {
        const model = { items: items, attestStateToId: attestStateToId, isMySelf: isMySelf };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TRANSACTIONS, model);
    }

    recalculateWorkTime(projectTimeBlockSaveDTOs: IProjectTimeBlockSaveDTO[]) {
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_RECALCULATE_WORKTIME, projectTimeBlockSaveDTOs);
    }

    moveTimeRowsToOrder(customerInvoiceId: number, projectTimeBlockIds: number[]) {
        const model = { customerInvoiceId: customerInvoiceId, projectTimeBlockIds: projectTimeBlockIds };
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_MOVE_TIMEROWS_TO_ORDER, model);
    }

    moveTimeRowsToDate(selectedDate: Date, projectTimeBlockIds: number[]) {
        const model = { selectedDate: selectedDate.toDateTimeString(), projectTimeBlockIds: projectTimeBlockIds };
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_MOVE_TIMEROWS_TO_DATE, model);
    }

    moveTimeRowsToOrderRow(customerInvoiceId: number, customerInvoiceRowId: number, projectTimeBlockIds: number[]) {
        const model = { customerInvoiceId: customerInvoiceId, customerInvoiceRowId: customerInvoiceRowId, projectTimeBlockIds: projectTimeBlockIds };
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_MOVE_TIMEROWS_TO_ORDER_ROW, model);
    }

    // DELETE
    deleteProject(projectId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_PROJECT + projectId);
    }
}
