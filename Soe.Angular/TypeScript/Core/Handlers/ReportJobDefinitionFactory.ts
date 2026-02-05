import { SelectionCollection } from "../RightMenu/ReportMenu/SelectionCollection";
import { PayrollProductRowSelectionDTO, EmployeeSelectionDTO, BoolSelectionDTO, DateRangeSelectionDTO, DateSelectionDTO, DatesSelectionDTO, IdSelectionDTO, IdListSelectionDTO } from "../../Common/Models/ReportDataSelectionDTO";
import { PayrollProductReport } from "../RightMenu/ReportMenu/ReportTypes/PayrollProductReport/PayrollProductReportComponent";
import { IReportJobDefinitionDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_ReportExportType, SoeReportTemplateType, TermGroup_TimeSchedulePlanningDayViewGroupBy, TermGroup_TimeSchedulePlanningDayViewSortBy, TermGroup_EmployeeSelectionAccountingType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";

export interface IReportJobDefinitionFactory {
}

export class ReportJobDefinitionFactory implements IReportJobDefinitionFactory {
    //@ngInject
    constructor() { }

    public static createSimpleTimeReportDefinition(reportId: number, sysReportTemplateType: SoeReportTemplateType, employeeIds: number[], from: Date, to: Date, exportType: TermGroup_ReportExportType = TermGroup_ReportExportType.Unknown): IReportJobDefinitionDTO {

        let collection: SelectionCollection = new SelectionCollection();
        this.addEmployeeSelection(collection, new EmployeeSelectionDTO(employeeIds, null, null, null, null, null, null, null, null, null, null, null, null));
        this.addDateRangeSelection(collection, new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_DATERANGE, from, to));

        return <IReportJobDefinitionDTO>{
            selections: collection.materialize(),
            reportId: reportId,
            sysReportTemplateTypeId: sysReportTemplateType,
            exportType: exportType,
            forceValidation: false,
        };
    }

    public static createSimplePayrollReportDefinition(reportId: number, sysReportTemplateType: SoeReportTemplateType, employeeIds: number[], timePeriodIds: number[], exportType: TermGroup_ReportExportType = TermGroup_ReportExportType.Unknown): IReportJobDefinitionDTO {

        let collection: SelectionCollection = new SelectionCollection();
        this.addEmployeeSelection(collection, new EmployeeSelectionDTO(employeeIds, null, null, null, null, null, null, null, null, null, null, null, null));
        this.addTimePeriodIdSelection(collection, new IdListSelectionDTO(timePeriodIds));

        return <IReportJobDefinitionDTO>{
            selections: collection.materialize(),
            reportId: reportId,
            sysReportTemplateTypeId: sysReportTemplateType,
            exportType: exportType,
            forceValidation: false,
        };
    }

    public static createSimpleScheduleReportDefinition(reportId: number, sysReportTemplateType: SoeReportTemplateType, employeeIds: number[], from: Date, to: Date, shiftTypeIds: number[], timeScheduleScenarioHeadId?: number, exportType: TermGroup_ReportExportType = TermGroup_ReportExportType.Unknown, includeVacant: boolean = false, includeHidden: boolean = false, includeSecondary: boolean = false, accountIds: number[] = [], accountingType: TermGroup_EmployeeSelectionAccountingType = TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock, isEmployeePost: boolean = false, excludeAbsence: boolean = false): IReportJobDefinitionDTO {

        let collection: SelectionCollection = new SelectionCollection();

        let employeeSelectionDTO = new EmployeeSelectionDTO(employeeIds, accountIds, null, null, null, null, null, null, null, accountingType, includeVacant, includeHidden, includeSecondary);
        employeeSelectionDTO.isEmployeePost = isEmployeePost;

        this.addEmployeeSelection(collection, employeeSelectionDTO);
        this.addDateRangeSelection(collection, new DateRangeSelectionDTO(Constants.REPORTMENU_DATERANGESELECTION_TYPE_DATERANGE, from, to));
        this.addShiftTypeSelection(collection, new IdListSelectionDTO(shiftTypeIds));
        this.addIdSelection(collection, new IdSelectionDTO(TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee), Constants.REPORTMENU_SELECTION_KEY_INCLUDE_GROUP_BY);
        this.addIdSelection(collection, new IdSelectionDTO(TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname), Constants.REPORTMENU_SELECTION_KEY_INCLUDE_SORT_BY);
        if (timeScheduleScenarioHeadId)
            this.addIdSelection(collection, new IdSelectionDTO(timeScheduleScenarioHeadId), "timeScheduleScenarioHeadId");

        // By default, include absence in line schedule reports from schedule planning. Should this be a setting from printout dialog instead?
        if (sysReportTemplateType == SoeReportTemplateType.TimeEmployeeLineSchedule)
            this.addBoolSelection(collection, new BoolSelectionDTO(true), "includeAbsence");

        if (excludeAbsence)
            this.addBoolSelection(collection, new BoolSelectionDTO(true), "excludeAbsence");

        return <IReportJobDefinitionDTO>{
            selections: collection.materialize(),
            reportId: reportId,
            sysReportTemplateTypeId: sysReportTemplateType,
            exportType: exportType,
            forceValidation: false,
        };
    }

    public static createEmployeeVacationDebtReportDefinition(reportId: number, employeeIds: number[], employeeCalculateVacationResultHeadId?: number, exportType: TermGroup_ReportExportType = TermGroup_ReportExportType.Unknown): IReportJobDefinitionDTO {

        let collection: SelectionCollection = new SelectionCollection();
        this.addEmployeeSelection(collection, new EmployeeSelectionDTO(employeeIds, null, null, null, null, null, null, null, null, null, null, null, null));
        if (employeeCalculateVacationResultHeadId)
            this.addIdSelection(collection, new IdSelectionDTO(employeeCalculateVacationResultHeadId), Constants.REPORTMENU_SELECTION_KEY_TIME_EMPLOYEECALCULATEVACATIONRESULTHEADID);

        return <IReportJobDefinitionDTO>{
            selections: collection.materialize(),
            reportId: reportId,
            sysReportTemplateTypeId: SoeReportTemplateType.EmployeeVacationDebtReport,
            exportType: exportType,
            forceValidation: false,
        };
    }

    public static createEmploymentContractFromEmployeeReportDefinition(reportId: number, sysReportTemplateType: SoeReportTemplateType, employeeId: number, employmentId: number, date: Date, savePrintout: boolean, employeeTemplateId: number): IReportJobDefinitionDTO {
        var employeeIds: number[] = [];
        employeeIds.push(employeeId);

        let collection: SelectionCollection = new SelectionCollection();
        this.addEmployeeSelection(collection, new EmployeeSelectionDTO(employeeIds, null, null, null, null, null, null, null, null, null, null, null, null));
        this.addDateSelection(collection, new DateSelectionDTO(date));
        this.addIdSelection(collection, new IdSelectionDTO(employmentId), "employmentId");
        this.addBoolSelection(collection, new BoolSelectionDTO(savePrintout), "savePrintout");
        this.addIdSelection(collection, new IdSelectionDTO(employeeTemplateId), "employeeTemplateId");

        return <IReportJobDefinitionDTO>{
            selections: collection.materialize(),
            reportId: reportId,
            sysReportTemplateTypeId: sysReportTemplateType,
            exportType: TermGroup_ReportExportType.Unknown,
            forceValidation: false,
        };
    }

    public static createEmploymentContractFromPlanningReportDefinition(reportId: number, sysReportTemplateType: SoeReportTemplateType, employeeIds: number[], savePrintout: boolean, substituteDates?: Date[], exportType: TermGroup_ReportExportType = TermGroup_ReportExportType.Pdf): IReportJobDefinitionDTO {

        let collection: SelectionCollection = new SelectionCollection();
        this.addEmployeeSelection(collection, new EmployeeSelectionDTO(employeeIds, null, null, null, null, null, null, null, null, null, null, null, null));
        this.addDatesSelection(collection, new DatesSelectionDTO(substituteDates));
        this.addBoolSelection(collection, new BoolSelectionDTO(true), "isPrintedFromSchedulePlanning");
        this.addBoolSelection(collection, new BoolSelectionDTO(savePrintout), "savePrintout");

        return <IReportJobDefinitionDTO>{
            selections: collection.materialize(),
            reportId: reportId,
            sysReportTemplateTypeId: sysReportTemplateType,
            exportType: exportType,
            forceValidation: false
        };
    }

    public static createPayrollProductReportDefinition(reportId: number, sysReportTemplateTypeId: number, productIds: number[], exportType: TermGroup_ReportExportType = TermGroup_ReportExportType.Pdf): IReportJobDefinitionDTO {

        let collection: SelectionCollection = new SelectionCollection();
        let selection: PayrollProductRowSelectionDTO = new PayrollProductRowSelectionDTO('', 0, 0, 0, 0, productIds);
        collection.upsert(PayrollProductReport.componentKey, selection);

        return <IReportJobDefinitionDTO>{
            selections: collection.materialize(),
            reportId: reportId,
            sysReportTemplateTypeId: sysReportTemplateTypeId,
            exportType: exportType,
            forceValidation: false,
        };
    }

    static addIdSelection(collection: SelectionCollection, selection: IdSelectionDTO, key: string) {
        collection.upsert(key, selection);
    }

    static addBoolSelection(collection: SelectionCollection, selection: BoolSelectionDTO, key: string) {
        collection.upsert(key, selection);
    }

    static addDateSelection(collection: SelectionCollection, selection: DateSelectionDTO) {
        collection.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE, selection);
    }

    static addDatesSelection(collection: SelectionCollection, selection: DatesSelectionDTO) {
        collection.upsert(Constants.REPORTMENU_SELECTION_KEY_DATES, selection);
    }

    static addDateRangeSelection(collection: SelectionCollection, selection: DateRangeSelectionDTO) {
        collection.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, selection);
    }

    static addEmployeeSelection(collection: SelectionCollection, selection: EmployeeSelectionDTO) {
        collection.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    static addShiftTypeSelection(collection: SelectionCollection, selection: IdListSelectionDTO) {
        this.addIdListSelection(Constants.REPORTMENU_SELECTION_KEY_SHIFT_TYPES, collection, selection);
    }

    static addTimePeriodIdSelection(collection: SelectionCollection, selection: IdListSelectionDTO) {
        this.addIdListSelection(Constants.REPORTMENU_SELECTION_KEY_PERIODS, collection, selection);
    }

    static addIdListSelection(key: string, collection: SelectionCollection, selection: IdListSelectionDTO) {
        collection.upsert(key, selection);
    }

}