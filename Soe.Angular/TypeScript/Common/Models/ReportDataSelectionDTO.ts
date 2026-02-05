import { IEmployeeSelectionDTO, /*IAccDimSelectionDTO, */IDateRangeSelectionDTO, IDateSelectionDTO, IDatesSelectionDTO, IPayrollProductRowSelectionDTO, IReportDataSelectionDTO, IBoolSelectionDTO, IIdListSelectionDTO, ITextSelectionDTO, IIdSelectionDTO, IGeneralReportSelectionDTO, IYearAndPeriodSelectionDTO, IAccountIntervalSelectionDTO, IAccountFilterSelectionDTO, IAccountFilterSelectionsDTO, IMatrixColumnsSelectionDTO, IMatrixColumnSelectionDTO, IUserDataSelectionDTO, IPayrollPriceTypeSelectionDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { AnalysisMode, MatrixDataType, TermGroup_EmployeeSelectionAccountingType, TermGroup_InsightChartTypes, TermGroup_ReportExportType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { MatrixDefinitionColumnOptions } from "./MatrixResultDTOs";

export abstract class SelectionDTO implements IReportDataSelectionDTO {
    key: string;
    typeName: string;
}

export class GeneralReportSelectionDTO extends SelectionDTO implements IGeneralReportSelectionDTO {
    exportType: TermGroup_ReportExportType;

    constructor(exportType: TermGroup_ReportExportType) {
        super();
        this.key = Constants.REPORTMENU_SELECTION_KEY_GENERAL_REPORT;
        this.typeName = "GeneralReportSelectionDTO";
        this.exportType = exportType;
    }
}

export class BoolSelectionDTO extends SelectionDTO implements IBoolSelectionDTO {
    value: boolean;

    constructor(value: boolean) {
        super();
        this.typeName = "BoolSelectionDTO";
        this.value = value;
    }
}

export class AccDimSelectionDTO extends SelectionDTO /*implements IAccDimSelectionDTO*/ {
    accountDimId: number;
    accountId: number;
    level: number;

    constructor(accountDimId: number, accountId: number, level: number) {
        super();
        this.typeName = "AccountDimSelectionDTO";
        this.accountDimId = accountDimId;
        this.accountId = accountId;
        this.level = level;
    }
}


export class AccountDimSelectionDTO extends SelectionDTO /*implements IAccDimSelectionDTO*/ {
    accountDimId: number;
    accountIds: number[];
    selectionKey: string;
    level: number;

    constructor(accountDimId: number, accountIds: number[], selectionKey: string, level: number
    ) {
        super();
        this.typeName = "AccountDimSelectionDTO";
        this.accountDimId = accountDimId;
        this.accountIds = accountIds;
        this.selectionKey = selectionKey;
        this.level = level;
    }
}



export class DateSelectionDTO extends SelectionDTO implements IDateSelectionDTO {
    date: Date;
    id: number;

    constructor(date: Date, id: number = 0) {
        super();
        this.typeName = "DateSelectionDTO";
        this.date = !CalendarUtility.isEmptyDate(date) ? CalendarUtility.convertToDate(date) : null;
        this.id = id;
    }
}

export class DateRangeSelectionDTO extends SelectionDTO implements IDateRangeSelectionDTO {
    from: Date;
    id: number;
    rangeType: string;
    to: Date;
    useMinMaxIfEmpty: boolean;

    constructor(rangeType: string, from: Date, to: Date, useMinMaxIfEmpty: boolean = false, id: number = 0) {
        super();
        this.rangeType = rangeType;
        this.typeName = "DateRangeSelectionDTO";
        this.from = !CalendarUtility.isEmptyDate(from) ? CalendarUtility.convertToDate(from) : null;
        this.to = !CalendarUtility.isEmptyDate(to) ? CalendarUtility.convertToDate(to) : null;
        this.useMinMaxIfEmpty = useMinMaxIfEmpty;
        this.id = id;
    }
}

export class DatesSelectionDTO extends SelectionDTO implements IDatesSelectionDTO {
    dates: Date[];

    constructor(dates: Date[]) {
        super();
        this.typeName = "DatesSelectionDTO";
        this.dates = CalendarUtility.convertToDates(dates);
    }
}

export class TextSelectionDTO extends SelectionDTO implements ITextSelectionDTO {
    text: string;

    constructor(text: string) {
        super();
        this.typeName = "TextSelectionDTO";
        this.text = text;
    }
}

export class IdSelectionDTO extends SelectionDTO implements IIdSelectionDTO {
    id: number;

    constructor(id: number) {
        super();
        this.typeName = "IdSelectionDTO";
        this.id = id;
    }
}

export class IdListSelectionDTO extends SelectionDTO implements IIdListSelectionDTO {
    ids: number[];

    constructor(ids: number[]) {
        super();
        this.typeName = "IdListSelectionDTO";
        this.ids = ids;
    }
}
export class PayrollPriceTypeSelectionDTO extends SelectionDTO implements IPayrollPriceTypeSelectionDTO {
    ids: number[];
    typeIds: number[];
    constructor(ids: number[], typeIds: number[]) {
        super();
        this.typeName = "PayrollPriceTypeSelectionDTO";
        this.ids = ids;
        this.typeIds = typeIds;
    }
}
export class UserDataSelectionDTO extends SelectionDTO implements IUserDataSelectionDTO {
    ids: number[];
    includeInactive: boolean;

    constructor(ids: number[], includeInactive: boolean = false) {
        super();
        this.includeInactive = includeInactive;
        this.typeName = "UserDataSelectionDTO";
        this.ids = ids;
    }
}

export class MatrixColumnSelectionDTO extends SelectionDTO implements IMatrixColumnSelectionDTO {
    field: string;
    options: MatrixDefinitionColumnOptions;
    sort: number;
    title: string;

    // Extensions
    matrixDataType: MatrixDataType;

    constructor(field: string, sort: number, title: string = '', options = null) {
        super();
        this.typeName = "MatrixColumnSelectionDTO";
        this.field = field;
        this.sort = sort;
        this.title = title;
        this.options = options;
        if (this.options)
            this.options.changed = options.changed;
    }

    public get hasOptions(): boolean {
        return !!this.options && this.options.changed;
    }

    public get isGroupedOnSort(): number {
        return (this.options && this.options.groupBy) ? 1 : 0;
    }
}

export class MatrixColumnsSelectionDTO extends SelectionDTO implements IMatrixColumnsSelectionDTO {
    analysisMode: AnalysisMode;
    chartType: TermGroup_InsightChartTypes;
    columns: MatrixColumnSelectionDTO[];
    insightId: number;
    insightName: string;
    valueType: number;

    constructor(columns: MatrixColumnSelectionDTO[]) {
        super();
        this.typeName = "MatrixColumnsSelectionDTO";
        this.columns = columns;
    }

    public get maxSort(): number {
        return this.columns && this.columns.length > 0 ? _.max(this.columns.map(c => c.sort || 0)) : 0;
    }
}

export class AccountIntervalSelectionDTO extends SelectionDTO implements IAccountIntervalSelectionDTO {
    value: number;
    yearId: number;
    constructor(value: number, yearId: number) {
        super();
        this.typeName = "AccountIntervalSelectionDTO";
        this.value = value;
        this.yearId = yearId;
    }
}

export class AccountFilterSelectionDTO extends SelectionDTO implements IAccountFilterSelectionDTO {
    from: string;
    to: string;
    id: number;

    constructor(id: number = 0, from: string = '', to: string = '') {
        super();
        this.typeName = "AccountFilterSelectionDTO";
        this.from = from;
        this.to = to;
        this.id = id;
    }
}

export class AccountFilterSelectionsDTO extends SelectionDTO implements IAccountFilterSelectionsDTO {
    filters: AccountFilterSelectionDTO[]
    constructor(filters: AccountFilterSelectionDTO[]) {
        super();
        this.filters = filters;
        this.typeName = "AccountFilterSelectionsDTO";
    }
}

export class EmployeeSelectionDTO extends SelectionDTO implements IEmployeeSelectionDTO {
    accountingType: TermGroup_EmployeeSelectionAccountingType;
    accountIds: number[];
    categoryIds: number[];
    doValidateEmployment: boolean;
    employeeGroupIds: number[];
    employeeIds: number[];
    employeeNrs: string[];
    employeePostIds: number[];
    includeEnded: boolean;
    includeInactive: boolean;
    onlyInactive: boolean;
    isEmployeePost: boolean;
    payrollGroupIds: number[];
    includeHidden: boolean;
    includeSecondary: boolean;
    includeVacant: boolean;
    vacationGroupIds: number[];

    constructor(employeeIds: number[], accountIds: number[], categoryIds: number[], employeeGroupIds: number[], payrollGroupIds: number[], vacationGroupIds: number[], includeInactive: boolean, onlyInactive: boolean, includeEnded: boolean, accountingType: TermGroup_EmployeeSelectionAccountingType, includeVacant: boolean, includeHidden: boolean, includeSecondary: boolean) {
        super();
        this.typeName = "EmployeeSelectionDTO";
        this.employeeIds = employeeIds;
        this.accountIds = accountIds;
        this.categoryIds = categoryIds;
        this.employeeGroupIds = employeeGroupIds;
        this.payrollGroupIds = payrollGroupIds;
        this.vacationGroupIds = vacationGroupIds;
        this.includeInactive = includeInactive;
        this.onlyInactive = onlyInactive;
        this.includeEnded = includeEnded;
        this.accountingType = accountingType;
        this.includeVacant = includeVacant;
        this.includeHidden = includeHidden;
        this.includeSecondary = includeSecondary;
    }
}

export class PayrollProductRowSelectionDTO extends SelectionDTO implements IPayrollProductRowSelectionDTO {

    key: string;
    sysPayrollTypeLevel1: number;
    sysPayrollTypeLevel2: number;
    sysPayrollTypeLevel3: number;
    sysPayrollTypeLevel4: number;
    payrollProductIds: number[];

    constructor(key: string, sysPayrollTypeLevel1: number, sysPayrollTypeLevel2: number, sysPayrollTypeLevel3: number, sysPayrollTypeLevel4: number, payrollProductIds: number[]) {
        super();
        this.typeName = "PayrollProductRowSelectionDTO";
        this.key = key;
        this.sysPayrollTypeLevel1 = sysPayrollTypeLevel1;
        this.sysPayrollTypeLevel2 = sysPayrollTypeLevel2;
        this.sysPayrollTypeLevel3 = sysPayrollTypeLevel3;
        this.sysPayrollTypeLevel4 = sysPayrollTypeLevel4;
        this.payrollProductIds = payrollProductIds;
    }
}

export class YearAndPeriodSelectionDTO extends SelectionDTO implements IYearAndPeriodSelectionDTO {
    rangeType: string;
    from: string;
    to: string;
    id: number;

    constructor(rangeType: string, from: string, to: string, id: number = 0) {
        super();
        this.rangeType = rangeType;
        this.typeName = "YearAndPeriodSelectionDTO";
        this.from = from;
        this.to = to;
        this.id = id;
    }
}