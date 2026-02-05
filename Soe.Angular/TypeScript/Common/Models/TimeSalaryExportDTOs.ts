import { ITimeSalaryExportDTO, ITimeSalaryExportSelectionDTO, ITimeSalaryExportSelectionEmployeeDTO, ITimeSalaryExportSelectionGroupDTO, ITimeSalaryPaymentExportDTO, ITimeSalaryPaymentExportEmployeeDTO, ITimeSalaryPaymentExportGridDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, SoeTimeSalaryExportFormat, SoeTimeSalaryExportTarget, SoeTimeSalaryPaymentExportFormat, TermGroup_TimeSalaryPaymentExportType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TimeSalaryExportDTO implements ITimeSalaryExportDTO {
    actorCompanyId: number;
    comment: string;
    created: Date;
    createdBy: string;
    exportDate: Date;
    exportFormat: SoeTimeSalaryExportFormat;
    exportTarget: SoeTimeSalaryExportTarget;
    extension: string;
    file1: number[];
    file2: number[];
    modified: Date;
    modifiedBy: string;
    startInterval: Date;
    state: SoeEntityState;
    stopInterval: Date;
    targetName: string;
    timeSalaryExportId: number;
    isPreliminary: boolean;
    isPreliminaryText: string;

    public fixDates() {
        this.exportDate = CalendarUtility.convertToDate(this.exportDate);
        this.startInterval = CalendarUtility.convertToDate(this.startInterval);
        this.stopInterval = CalendarUtility.convertToDate(this.stopInterval);
    }
}

export class TimeSalaryExportSelectionDTO implements ITimeSalaryExportSelectionDTO {
    actorCompanyId: number;
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    dateFrom: Date;
    dateTo: Date;
    entirePeriodValidForExport: boolean;
    timeSalaryExportSelectionGroups: TimeSalaryExportSelectionGroupDTO[];

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}

export class TimeSalaryExportSelectionGroupDTO implements ITimeSalaryExportSelectionGroupDTO {
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    entirePeriodValidForExport: boolean;
    id: number;
    name: string;
    timeSalaryExportSelectionEmployees: TimeSalaryExportSelectionEmployeeDTO[];
    timeSalaryExportSelectionSubGroups: TimeSalaryExportSelectionGroupDTO[];

    // Extensions
    expanded: boolean;
    selected: boolean;
    sortBy: string;
    sortByReverse: boolean;

    public sort(column: string) {
        this.sortByReverse = !this.sortByReverse && this.sortBy === column;
        this.sortBy = column;
    }

    public get isAllEmployeesSelected(): boolean {
        return _.filter(this.timeSalaryExportSelectionEmployees, e => !e.selected).length === 0;
    }
}

export class TimeSalaryExportSelectionEmployeeDTO implements ITimeSalaryExportSelectionEmployeeDTO {
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    employeeId: number;
    employeeNr: string;
    entirePeriodValidForExport: boolean;
    name: string;

    // Extensions
    selected: boolean;
    visible: boolean;

    public get employeeNrSort(): string {
        return this.employeeNr.padLeft(50, '0');
    }

    public setVisible(filterText: string) {
        filterText = filterText.toLocaleLowerCase();
        this.visible = (!filterText || this.attestStateName.toLocaleLowerCase().includes(filterText) || this.employeeNr.toLocaleLowerCase().includes(filterText) || this.name.toLocaleLowerCase().includes(filterText));
    }
}

export class TimeSalaryPaymentExportDTO implements ITimeSalaryPaymentExportDTO {
    accountDepositNetAmount: number;
    accountDepositNetAmountCurrency: number;
    actorCompanyId: number;
    cashDepositNetAmount: number;
    created: Date;
    createdBy: string;
    exportDate: Date;
    exportFormat: SoeTimeSalaryPaymentExportFormat;
    exportType: TermGroup_TimeSalaryPaymentExportType;
    extension: string;
    file: number[];
    hasEmployeeDetails: boolean;
    isSelected: boolean;
    modified: Date;
    modifiedBy: string;
    paymentDate: Date;
    paymentDateString: string;
    payrollDateInterval: string;
    state: SoeEntityState;
    timePeriodHeadName: string;
    timePeriodId: number;
    timePeriodName: string;
    timeSalaryPaymentExportEmployees: TimeSalaryPaymentExportEmployeeDTO[];
    timeSalaryPaymentExportId: number;
    typeName: string;

    public fixDates() {
        this.exportDate = CalendarUtility.convertToDate(this.exportDate);
        this.paymentDate = CalendarUtility.convertToDate(this.paymentDate);
    }
}

export class TimeSalaryPaymentExportEmployeeDTO implements ITimeSalaryPaymentExportEmployeeDTO {
    netAmountCurrency: number;
    accountNr: string;
    accountNrGridStr: string;
    disbursementMethod: number;
    disbursementMethodName: string;
    employeeId: number;
    employeeNr: string;
    isSECashDeposit: boolean;
    isSEExtendedSelection: boolean;
    name: string;
    netAmount: number;
    paymentRowKey: string;
}

export class TimeSalaryPaymentExportGridDTO implements ITimeSalaryPaymentExportGridDTO {
    accountDepositNetAmount: number;
    accountDepositNetAmountCurrency: number;
    cashDepositNetAmount: number;
    currencyCode: string;
    currencyDate: Date;
    currencyRate: number;
    debitDate: Date;
    employees: TimeSalaryPaymentExportEmployeeDTO[];
    exportDate: Date;
    exportType: TermGroup_TimeSalaryPaymentExportType;
    msgKey: string;
    paymentDate: Date;
    paymentKey: string;
    payrollDateInterval: string;
    timePeriodHeadName: string;
    timePeriodName: string;
    timeSalaryPaymentExportId: number;
    typeName: string;
    salarySpecificationPublishDate: Date;

    public fixDates() {
        this.exportDate = CalendarUtility.convertToDate(this.exportDate);
        this.paymentDate = CalendarUtility.convertToDate(this.paymentDate);
    }
}
