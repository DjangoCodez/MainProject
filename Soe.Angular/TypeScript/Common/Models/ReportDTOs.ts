import { IReportDTO, IReportSelectionDateDTO, IReportSelectionIntDTO, IReportSelectionStrDTO, IReportMenuDTO, IReportTemplateDTO, IReportItemDTO, IReportViewDTO, IReportUserSelectionDTO, IReportDataSelectionDTO, IReportViewGridDTO, IReportPrintoutDTO, IReportUserSelectionAccessDTO, ISysReportTemplateViewGridDTO, IReportJobStatusDTO, IReportSettingDTO, IReportTemplateSettingDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_ReportExportFileType, TermGroup_ReportExportType, TermGroup_ReportGroupAndSortingTypes, SoeModule, SoeEntityState, TermGroup_ReportPrintoutStatus, SoeExportFormat, TermGroup_ReportPrintoutDeliveryType, SoeReportType, TermGroup_ReportUserSelectionAccessType, SoeReportTemplateType, ReportUserSelectionType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { BoolSelectionDTO, DateRangeSelectionDTO, DateSelectionDTO, DatesSelectionDTO, IdSelectionDTO, IdListSelectionDTO, EmployeeSelectionDTO, PayrollProductRowSelectionDTO, GeneralReportSelectionDTO, AccountFilterSelectionDTO, AccountFilterSelectionsDTO, AccountIntervalSelectionDTO, MatrixColumnSelectionDTO, MatrixColumnsSelectionDTO, TextSelectionDTO, UserDataSelectionDTO, PayrollPriceTypeSelectionDTO } from "./ReportDataSelectionDTO";
import { Constants } from "../../Util/Constants";
import { ReportMenuPages } from "../../Util/Enumerations";

export class ReportMenuDTO implements IReportMenuDTO {
    active: boolean;
    description: string;
    groupName: string;
    groupOrder: number;
    isCompanyTemplate: boolean;
    isFavorite: boolean;
    isStandard: boolean;
    isSystemReport: boolean;
    module: SoeModule;
    name: string;
    noPrintPermission: boolean;
    noRolesSpecified: boolean;
    printableFromMenu: boolean;
    reportId: number;
    reportNr: number;
    reportTemplateId: number;
    sysReportTemplateTypeId: number;
    sysReportType: SoeReportType

    // Extensions
    printoutDelivered: Date;
    printoutErrorMessage: string;
    printoutRequested: Date;
    printoutStatus: TermGroup_ReportPrintoutStatus;
    standardText: string;

    isGroup: boolean;
    reportItem: IReportItemDTO;

    public fixDates() {
        this.printoutDelivered = CalendarUtility.convertToDate(this.printoutDelivered);
        this.printoutRequested = CalendarUtility.convertToDate(this.printoutRequested);
    }

    public get isReport(): boolean {
        return this.sysReportType === SoeReportType.CrystalReport;
    }

    public get isAnalysis(): boolean {
        return this.sysReportType === SoeReportType.Analysis;
    }
}
export class ReportMenuPageDTO {
    page: ReportMenuPages;
    title: string;
    tooltip: string;
    icon: string;
    selected: boolean;

    constructor(page: ReportMenuPages, title: string, tooltip: string, icon: string, selected: boolean = false) {
        this.page = page;
        this.title = title;
        this.tooltip = tooltip;
        this.icon = icon;
        this.selected = selected;
    }
}

export class ReportMenuModuleDTO {
    module: SoeModule;
    title: string;
    imageSource: string;
    expanded: boolean;
    reports: ReportMenuDTO[];
    loadingReports: boolean;
    reportsLoaded: boolean;
    hasNoRolesSpecified: boolean;
    hasInsightsPermission: boolean;

    constructor(module: SoeModule, title: string, imageSource: string, expanded: boolean = false) {
        this.module = module;
        this.title = title;
        this.imageSource = imageSource;
        this.expanded = expanded;
        this.reports = [];
    }

    public get hasReports(): boolean {
        return this.reports && this.reports.length > 0;
    }
}

export class ReportDataSelectionDTO implements IReportDataSelectionDTO {
    key: string;
    typeName: string;
}

export class ReportUserSelectionDTO implements IReportUserSelectionDTO {
    access: ReportUserSelectionAccessDTO[];
    actorCompanyId: number;
    description: string;
    name: string;
    reportId: number;
    reportUserSelectionId: number;
    scheduledJobHeadId: number;
    selections: ReportDataSelectionDTO[];
    state: SoeEntityState;
    type: ReportUserSelectionType;
    userId: number;
    validForScheduledJobHead: boolean;

    public setTypes() {
        if (this.access) {
            this.access = this.access.map(a => {
                let aObj = new ReportUserSelectionAccessDTO();
                angular.extend(aObj, a);
                return aObj;
            });
        } else {
            this.access = [];
        }
    }

    public setSelectionTypes() {
        if (this.selections) {
            this.selections = this.selections.map(s => {
                let sObj;
                switch (s.typeName) {
                    case 'GeneralReportSelectionDTO':
                        sObj = new GeneralReportSelectionDTO((<GeneralReportSelectionDTO>s).exportType);
                        break;
                    case 'BoolSelectionDTO':
                        sObj = new BoolSelectionDTO((<BoolSelectionDTO>s).value);
                        break;
                    case 'TextSelectionDTO':
                        sObj = new TextSelectionDTO((<TextSelectionDTO>s).text);
                        break;
                    case 'DateSelectionDTO':
                        sObj = new DateSelectionDTO((<DateSelectionDTO>s).date, (<DateSelectionDTO>s).id);
                        break;
                    case 'DatesSelectionDTO':
                        sObj = new DatesSelectionDTO((<DatesSelectionDTO>s).dates);
                        break;
                    case 'DateRangeSelectionDTO':
                        sObj = new DateRangeSelectionDTO((<DateRangeSelectionDTO>s).rangeType, (<DateRangeSelectionDTO>s).from, (<DateRangeSelectionDTO>s).to, (<DateRangeSelectionDTO>s).useMinMaxIfEmpty, (<DateRangeSelectionDTO>s).id);
                        break;
                    case 'IdSelectionDTO':
                        sObj = new IdSelectionDTO((<IdSelectionDTO>s).id);
                        break;
                    case 'IdListSelectionDTO':
                        sObj = new IdListSelectionDTO((<IdListSelectionDTO>s).ids);
                        break;
                    case 'MatrixColumnsSelectionDTO':
                        let columns: MatrixColumnSelectionDTO[] = [];
                        _.forEach((<MatrixColumnsSelectionDTO>s).columns, col => {
                            columns.push(new MatrixColumnSelectionDTO(col.field, col.sort, col.title, col.options));
                        });

                        sObj = new MatrixColumnsSelectionDTO(columns);
                        sObj.analysisMode = (<MatrixColumnsSelectionDTO>s).analysisMode;
                        sObj.insightId = (<MatrixColumnsSelectionDTO>s).insightId;
                        sObj.insightName = (<MatrixColumnsSelectionDTO>s).insightName;
                        sObj.chartType = (<MatrixColumnsSelectionDTO>s).chartType;
                        sObj.valueType = (<MatrixColumnsSelectionDTO>s).valueType;
                        break;
                    case 'EmployeeSelectionDTO':
                        sObj = new EmployeeSelectionDTO((<EmployeeSelectionDTO>s).employeeIds, (<EmployeeSelectionDTO>s).accountIds, (<EmployeeSelectionDTO>s).categoryIds, (<EmployeeSelectionDTO>s).employeeGroupIds, (<EmployeeSelectionDTO>s).payrollGroupIds, (<EmployeeSelectionDTO>s).vacationGroupIds, (<EmployeeSelectionDTO>s).includeInactive, (<EmployeeSelectionDTO>s).onlyInactive, (<EmployeeSelectionDTO>s).includeEnded, (<EmployeeSelectionDTO>s).accountingType, (<EmployeeSelectionDTO>s).includeVacant, (<EmployeeSelectionDTO>s).includeHidden, (<EmployeeSelectionDTO>s).includeSecondary);
                        break;
                    case 'PayrollProductRowSelectionDTO':
                        sObj = new PayrollProductRowSelectionDTO(s.key, (<PayrollProductRowSelectionDTO>s).sysPayrollTypeLevel1, (<PayrollProductRowSelectionDTO>s).sysPayrollTypeLevel2, (<PayrollProductRowSelectionDTO>s).sysPayrollTypeLevel3, (<PayrollProductRowSelectionDTO>s).sysPayrollTypeLevel4, (<PayrollProductRowSelectionDTO>s).payrollProductIds);
                        break;
                    case 'UserDataSelectionDTO':
                        sObj = new UserDataSelectionDTO((<UserDataSelectionDTO>s).ids, (<UserDataSelectionDTO>s).includeInactive);
                        break;
                    default:
                        sObj = new ReportDataSelectionDTO();
                        break;
                }

                angular.extend(sObj, s);
                return sObj;
            });
        }
    }

    private getSelection(key: string): ReportDataSelectionDTO {
        return _.find(this.selections, s => s.key === key);
    }

    public getGeneralReportSelection(): GeneralReportSelectionDTO {
        const selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_GENERAL_REPORT);
        if (selection)
            return new GeneralReportSelectionDTO((<GeneralReportSelectionDTO>selection).exportType);

        return null;
    }

    public getBoolSelection(key: string): BoolSelectionDTO {
        const selection = this.getSelection(key);
        if (selection)
            return new BoolSelectionDTO((<BoolSelectionDTO>selection).value);

        return null;
    }

    public getTextSelection(key: string): TextSelectionDTO {
        const selection = this.getSelection(key);
        if (selection)
            return new TextSelectionDTO((<TextSelectionDTO>selection).text);

        return null;
    }

    public getDateSelectionFromKey(key: string): DateSelectionDTO {
        const selection = this.getSelection(key);
        if (selection)
            return new DateSelectionDTO((<DateSelectionDTO>selection).date);

        return null;
    }

    public getDateSelection(): DateSelectionDTO {
        const selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_DATE);
        if (selection)
            return new DateSelectionDTO((<DateSelectionDTO>selection).date, (<DateSelectionDTO>selection).id);

        return null;
    }

    public getDatesSelection(): DatesSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_DATES);
        if (selection)
            return new DatesSelectionDTO((<DatesSelectionDTO>selection).dates);

        return null;
    }

    public getDateRangeSelection(): DateRangeSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE);
        if (selection)
            return new DateRangeSelectionDTO((<DateRangeSelectionDTO>selection).rangeType, (<DateRangeSelectionDTO>selection).from, (<DateRangeSelectionDTO>selection).to, (<DateRangeSelectionDTO>selection).useMinMaxIfEmpty, (<DateRangeSelectionDTO>selection).id);

        return null;
    }

    public getDateRangeSelectionFromKey(key:string): DateRangeSelectionDTO {
        let selection = this.getSelection(key);
        if (selection)
            return new DateRangeSelectionDTO((<DateRangeSelectionDTO>selection).rangeType, (<DateRangeSelectionDTO>selection).from, (<DateRangeSelectionDTO>selection).to, (<DateRangeSelectionDTO>selection).useMinMaxIfEmpty, (<DateRangeSelectionDTO>selection).id);

        return null;
    }

    public getPaymentDateRangeSelection(): DateRangeSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_PAYMENTDATE_RANGE);
        if (selection)
            return new DateRangeSelectionDTO((<DateRangeSelectionDTO>selection).rangeType, (<DateRangeSelectionDTO>selection).from, (<DateRangeSelectionDTO>selection).to, (<DateRangeSelectionDTO>selection).useMinMaxIfEmpty, (<DateRangeSelectionDTO>selection).id);

        return null;
    }

    public getCreatedDateRangeSelection(): DateRangeSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_CREATEDDATE_RANGE);
        if (selection)
            return new DateRangeSelectionDTO((<DateRangeSelectionDTO>selection).rangeType, (<DateRangeSelectionDTO>selection).from, (<DateRangeSelectionDTO>selection).to, (<DateRangeSelectionDTO>selection).useMinMaxIfEmpty, (<DateRangeSelectionDTO>selection).id);

        return null;
    }

    public getIdSelection(key: string): IdSelectionDTO {
        let selection = this.getSelection(key);
        if (selection)
            return new IdSelectionDTO((<IdSelectionDTO>selection).id);

        return null;
    }

    public getIdListSelection(key: string): IdListSelectionDTO {
        let selection = this.getSelection(key);
        if (selection)
            return new IdListSelectionDTO((<IdListSelectionDTO>selection).ids);

        return null;
    }
    public getPayrollPriceTypeSelection(key: string): PayrollPriceTypeSelectionDTO {
        let selection = this.getSelection(key);
        if (selection)
            return new PayrollPriceTypeSelectionDTO((<PayrollPriceTypeSelectionDTO>selection).ids, (<PayrollPriceTypeSelectionDTO>selection).typeIds);

        return null;
    }

    public getUserSelection(): UserDataSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_USERS);
        if (selection)
            return new UserDataSelectionDTO((<UserDataSelectionDTO>selection).ids, (<UserDataSelectionDTO>selection).includeInactive);

        return null;
    }

    public getMatrixColumnSelection(): MatrixColumnsSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_MATRIX_COLUMNS);
        if (selection) {
            let columns: MatrixColumnSelectionDTO[] = [];
            _.forEach((<MatrixColumnsSelectionDTO>selection).columns, col => {
                columns.push(new MatrixColumnSelectionDTO(col.field, col.sort, col.title, col.options));
            });

            let matrix = new MatrixColumnsSelectionDTO(columns);
            matrix.analysisMode = (<MatrixColumnsSelectionDTO>selection).analysisMode;
            matrix.insightId = (<MatrixColumnsSelectionDTO>selection).insightId;
            matrix.insightName = (<MatrixColumnsSelectionDTO>selection).insightName;
            matrix.chartType = (<MatrixColumnsSelectionDTO>selection).chartType;
            matrix.valueType = (<MatrixColumnsSelectionDTO>selection).valueType;

            return matrix;
        }

        return null;
    }

    public getEmployeeSelection(): EmployeeSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES);
        if (selection)
            return new EmployeeSelectionDTO((<EmployeeSelectionDTO>selection).employeeIds, (<EmployeeSelectionDTO>selection).accountIds, (<EmployeeSelectionDTO>selection).categoryIds, (<EmployeeSelectionDTO>selection).employeeGroupIds, (<EmployeeSelectionDTO>selection).payrollGroupIds, (<EmployeeSelectionDTO>selection).vacationGroupIds, (<EmployeeSelectionDTO>selection).includeInactive, (<EmployeeSelectionDTO>selection).onlyInactive, (<EmployeeSelectionDTO>selection).includeEnded, (<EmployeeSelectionDTO>selection).accountingType, (<EmployeeSelectionDTO>selection).includeVacant, (<EmployeeSelectionDTO>selection).includeHidden, (<EmployeeSelectionDTO>selection).includeSecondary);

        return null;
    }

    public getIntervalFromSelection(): AccountIntervalSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_FROM);
        if (selection) {
            return <AccountIntervalSelectionDTO>selection;
        }
        return null;
    }

    public getIntervalToSelection(): AccountIntervalSelectionDTO {
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_TO);
        if (selection) {
            return <AccountIntervalSelectionDTO>selection;
        }
        return null;
    }

    public getNamedRangeSelection(): AccountFilterSelectionDTO[] {
        let selectionDTOs: AccountFilterSelectionDTO[] = [];
        let selection = this.getSelection(Constants.REPORTMENU_SELECTION_KEY_NAMED_FILTER_RANGES);
        if (selection) {
            var namedFilters = <AccountFilterSelectionsDTO>selection;
            namedFilters.filters.forEach(namedFilter => {
                selectionDTOs.push(new AccountFilterSelectionDTO(namedFilter.id, namedFilter.from, namedFilter.to));
            });
        }
        return selectionDTOs;
    }

    public getPayrollProductRowSelection(): PayrollProductRowSelectionDTO {
        let key = Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRODUCTS;
        let selection = this.getSelection(key);
        if (selection)
            return new PayrollProductRowSelectionDTO(key, (<PayrollProductRowSelectionDTO>selection).sysPayrollTypeLevel1, (<PayrollProductRowSelectionDTO>selection).sysPayrollTypeLevel2, (<PayrollProductRowSelectionDTO>selection).sysPayrollTypeLevel3, (<PayrollProductRowSelectionDTO>selection).sysPayrollTypeLevel4, (<PayrollProductRowSelectionDTO>selection).payrollProductIds);

        return null;
    }

    public getPayrollProductRowSelections(): PayrollProductRowSelectionDTO[] {
        let key = Constants.REPORTMENU_SELECTION_KEY_PAYROLL_PRODUCTS;
        let selections = _.filter(this.selections, s => s.key.startsWithCaseInsensitive(key));
        let selectionDTOs: PayrollProductRowSelectionDTO[] = [];
        _.forEach(selections, selection => {
            selectionDTOs.push(new PayrollProductRowSelectionDTO(selection.key, (<PayrollProductRowSelectionDTO>selection).sysPayrollTypeLevel1, (<PayrollProductRowSelectionDTO>selection).sysPayrollTypeLevel2, (<PayrollProductRowSelectionDTO>selection).sysPayrollTypeLevel3, (<PayrollProductRowSelectionDTO>selection).sysPayrollTypeLevel4, (<PayrollProductRowSelectionDTO>selection).payrollProductIds));
        });

        return selectionDTOs;
    }
}

export class ReportUserSelectionAccessDTO implements IReportUserSelectionAccessDTO {
    created: Date;
    createdBy: string;
    messageGroupId: number;
    modified: Date;
    modifiedBy: string;
    reportUserSelectionAccessId: number;
    reportUserSelectionId: number;
    roleId: number;
    state: SoeEntityState;
    type: TermGroup_ReportUserSelectionAccessType;
}

export class ReportDTO implements IReportDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    description: string;
    detailedInformation: boolean;
    exportFileType: TermGroup_ReportExportFileType;
    exportType: TermGroup_ReportExportType;
    filePath: string;
    groupByLevel1: TermGroup_ReportGroupAndSortingTypes;
    groupByLevel2: TermGroup_ReportGroupAndSortingTypes;
    groupByLevel3: TermGroup_ReportGroupAndSortingTypes;
    groupByLevel4: TermGroup_ReportGroupAndSortingTypes;
    includeAllHistoricalData: boolean;
    includeBudget: boolean;
    isSortAscending: boolean;
    modified: Date;
    modifiedBy: string;
    module: SoeModule;
    name: string;
    noOfYearsBackinPreviousYear: number;
    original: boolean;
    reportId: number;
    reportNr: number;
    reportSelectionDate: IReportSelectionDateDTO[];
    reportSelectionId: number;
    reportSelectionInt: IReportSelectionIntDTO[];
    reportSelectionStr: IReportSelectionStrDTO[];
    reportSelectionText: string;
    reportTemplateId: number;
    showInAccountingReports: boolean;
    sortByLevel1: TermGroup_ReportGroupAndSortingTypes;
    sortByLevel2: TermGroup_ReportGroupAndSortingTypes;
    sortByLevel3: TermGroup_ReportGroupAndSortingTypes;
    sortByLevel4: TermGroup_ReportGroupAndSortingTypes;
    settings: IReportSettingDTO[];
    special: string;
    standard: boolean;
    state: SoeEntityState;
    sysReportTemplateTypeId: number;
    sysReportTemplateTypeSelectionType: number;
    roleIds: number[];
    isNewGroupsAndHeaders: boolean;
    importCompanyId: number;
    importReportId: number;
    nrOfDecimals?: number;
    showRowsByAccount: boolean;
    reportTemplateSettings: IReportTemplateSettingDTO[];
}

export class ReportPrintoutDTO implements IReportPrintoutDTO {
    actorCompanyId: number;
    cleanedTime: Date;
    created: Date;
    createdBy: string;
    data: number[];
    datas: number[];
    deliveredTime: Date;
    deliveryType: TermGroup_ReportPrintoutDeliveryType;
    emailFileName: string;
    emailMessage: string;
    emailRecipients: number[];
    emailTemplateId: number;
    exportFormat: SoeExportFormat;
    exportType: TermGroup_ReportExportType;
    forceValidation: boolean;
    isEmailValid: boolean;
    json: string;
    modified: Date;
    modifiedBy: string;
    orderedDeliveryTime: Date;
    reportFileName: string;
    reportFileType: string;
    reportId: number;
    reportName: string;
    reportPackageId: number;
    reportPrintoutId: number;
    reportTemplateId: number;
    reportUrlId: number;
    resultMessage: number;
    resultMessageDetails: string;
    roleId: number;
    selection: string;
    singleRecipient: string;
    status: number;
    sysReportTemplateTypeId: number;
    userId: number;
    xml: string;
    xMLCompressed: number[];
}

export class ReportJobStatusDTO implements IReportJobStatusDTO {
    exportType: TermGroup_ReportExportType;
    name: string;
    printoutDelivered: Date;
    printoutErrorMessage: string;
    printoutRequested: Date;
    printoutStatus: TermGroup_ReportPrintoutStatus;
    reportPrintoutId: number;
    sysReportTemplateTypeId: SoeReportTemplateType;

    public fixDates() {
        this.printoutDelivered = CalendarUtility.convertToDate(this.printoutDelivered);
        this.printoutRequested = CalendarUtility.convertToDate(this.printoutRequested);
    }

    public get fileTypeClass(): string {
        switch (this.exportType) {
            case TermGroup_ReportExportType.Pdf:
                return "fa-file-pdf";
            case TermGroup_ReportExportType.Xml:
                return "fa-file-code";
            case TermGroup_ReportExportType.Excel:
            case TermGroup_ReportExportType.MatrixExcel:
                return "fa-file-excel";
            case TermGroup_ReportExportType.Word:
                return "fa-file-word";
            case TermGroup_ReportExportType.RichText:
            case TermGroup_ReportExportType.EditableRTF:
            case TermGroup_ReportExportType.Text:
                return "fa-file-alt";
            case TermGroup_ReportExportType.TabSeperatedText:
                return "fa-file-spreadsheet";
            case TermGroup_ReportExportType.CharacterSeparatedValues:
                return "fa-file-csv";
            case TermGroup_ReportExportType.MatrixGrid:
                return "fa-table";
            default:
                return "fa-file";
        }
    }

    public get isAnalysis(): boolean {
        return this.exportType === TermGroup_ReportExportType.MatrixExcel || this.exportType === TermGroup_ReportExportType.MatrixGrid;
    }
}

export class ReportViewDTO implements IReportViewDTO {
    actorCompanyId: number;
    exportType: number;
    reportDescription: string;
    reportId: number;
    reportName: string;
    reportNameDesc: string;
    reportNr: number;
    reportSelectionId: number;
    showInAccountingReports: boolean;
    sysReportTemplateTypeId: number;
    sysReportTypeName: string;
    employeeTemplateId: number;
}

export class ReportViewGridDTO extends ReportViewDTO implements IReportViewGridDTO {
    exportTypeName: string;
    isMigrated: boolean;
    original: boolean;
    reportSelectionText: string;
    roleNames: string;
    selectionType: any;
    standard: boolean;
    standardText: string;
}

export class ProjectTransactionsReportDTO {
    reportId: number;
    sysReportTemplateTypeId: number;
    exportType: number;
    dim2From: string;
    dim2Id: number;
    dim2To: string;
    dim3From: string;
    dim3Id: number;
    dim3To: string;
    dim4From: string;
    dim4Id: number;
    dim4To: string;
    dim5From: string;
    dim5Id: number;
    dim5To: string;
    dim6From: string;
    dim6Id: number;
    dim6To: string;
    employeeNrFrom: string;
    employeeNrTo: string;
    includeChildProjects: boolean;
    invoiceNrFrom: string;
    invoiceNrTo: string;
    invoiceProductNrFrom: string;
    invoiceProductNrTo: string;
    invoiceTransactionDateFrom: Date;
    invoiceTransactionDateTo: Date;
    offerNrFrom: string;
    offerNrTo: string;
    orderNrFrom: string;
    orderNrTo: string;
    payrollProductNrFrom: string;
    payrollProductNrTo: string;
    payrollTransactionDateFrom: Date;
    payrollTransactionDateTo: Date;
    projectIds: number[];

    constructor(reportId: number, sysReportTemplateTypeId: number, exportType: number, projectIds: any[], offerNrFrom: string, offerNrTo: string, orderNrFrom: string, orderNrTo: string, invoiceNrFrom: string, invoiceNrTo: string, employeeNrFrom: string, employeeNrTo: string,
        payrollProductNrFrom: string, payrollProductNrTo: string, invoiceProductNrFrom: string, invoiceProductNrTo: string,
        payrollTransactionDateFrom: Date, payrollTransactionDateTo: Date, invoiceTransactionDateFrom: Date, invoiceTransactionDateTo: Date, inclChildProjects: boolean,
        dim2id: number, dim2from: string, dim2to: string, dim3id: number, dim3from: string, dim3to: string, dim4id: number, dim4from: string, dim4to: string,
        dim5id: number, dim5from: string, dim5to: string, dim6id: number, dim6from: string, dim6to: string) {

        this.reportId = reportId;
        this.sysReportTemplateTypeId = sysReportTemplateTypeId;
        this.exportType = exportType;
        this.projectIds = projectIds;
        this.offerNrFrom = offerNrFrom;
        this.offerNrTo = offerNrTo;
        this.orderNrFrom = orderNrFrom;
        this.orderNrTo = orderNrTo;
        this.invoiceNrFrom = invoiceNrFrom;
        this.invoiceNrTo = invoiceNrTo;
        this.employeeNrFrom = employeeNrFrom;
        this.employeeNrTo = employeeNrTo;
        this.payrollProductNrFrom = payrollProductNrFrom;
        this.payrollProductNrTo = payrollProductNrTo;
        this.invoiceProductNrFrom = invoiceProductNrFrom;
        this.invoiceProductNrTo = invoiceProductNrTo;
        this.payrollTransactionDateFrom = payrollTransactionDateFrom;
        this.payrollTransactionDateTo = payrollTransactionDateTo;
        this.invoiceTransactionDateFrom = invoiceTransactionDateFrom;
        this.invoiceTransactionDateTo = invoiceTransactionDateTo;
        this.includeChildProjects = inclChildProjects;
        this.dim2Id = dim2id;
        this.dim2From = dim2from;
        this.dim2To = dim2to;
        this.dim3Id = dim3id;
        this.dim3From = dim3from;
        this.dim3To = dim3to;
        this.dim4Id = dim4id;
        this.dim4From = dim4from;
        this.dim4To = dim4to;
        this.dim5Id = dim5id;
        this.dim5From = dim5from;
        this.dim5To = dim5to;
        this.dim6Id = dim6id;
        this.dim6From = dim6from;
        this.dim6To = dim6to;
    }
}

export class ReportTemplateDTO implements IReportTemplateDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    description: string;
    fileName: string;
    groupByLevel1: number;
    groupByLevel2: number;
    groupByLevel3: number;
    groupByLevel4: number;
    isSortAscending: boolean;
    isSystem: boolean;
    modified: Date;
    modifiedBy: string;
    module: SoeModule;
    name: string;
    reportNr: number;
    reportTemplateId: number;
    showGroupingAndSorting: boolean;
    showOnlyTotals: boolean;
    sortByLevel1: number;
    sortByLevel2: number;
    sortByLevel3: number;
    sortByLevel4: number;
    special: string;
    state: SoeEntityState;
    sysCountryIds: number[];
    sysReportTemplateTypeId
    sysReportTemplateTypeName: string;
    sysReportTypeId: number;
    validExportTypes: number[];
    isSystemReport: boolean;
    reportTemplateSettings: any;
}

export class SysReportTemplateViewGridDTO implements ISysReportTemplateViewGridDTO {
    description: string;
    groupName: string;
    name: string;
    reportNr: number;
    sysCountryIds: number[];
    sysReportTemplateId: number;
    sysReportTemplateTypeName: string;
    isSystemReport: boolean;

    public get groupAndName(): string {
        let str: string = this.groupName + " - ";
        if (this.sysReportTemplateTypeName)
            str += this.sysReportTemplateTypeName + ", ";
        str += this.name;

        return str;
    }

    public get groupAndNameAndNr(): string {
        let str: string = this.groupName + " - ";
        if (this.sysReportTemplateTypeName)
            str += this.sysReportTemplateTypeName + ", ";
        str += this.name;
        if (this.reportNr)
            str += " (" + this.reportNr.toString() + ")";
        return str;
    }
}