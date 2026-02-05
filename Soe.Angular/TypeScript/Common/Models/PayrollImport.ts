import { IPayrollImportHeadDTO, IPayrollImportEmployeeDTO, IPayrollImportEmployeeScheduleDTO, IPayrollImportEmployeeTransactionDTO, System, ISmallGenericType, IPayrollImportEmployeeTransactionLinkDTO, IPayrollImportEmployeeTransactionAccountInternalDTO, IPayrollStartValueHeadDTO, IPayrollStartValueRowDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityState, TermGroup_PayrollImportHeadFileType, TermGroup_PayrollImportHeadType, TermGroup_PayrollImportEmployeeScheduleStatus, TermGroup_PayrollImportEmployeeTransactionStatus, TermGroup_PayrollImportEmployeeTransactionType, TermGroup_PayrollImportEmployeeStatus, TermGroup_PayrollImportHeadStatus, TermGroup_SysPayrollType } from "../../Util/CommonEnumerations";
import { AccountingSettingsRowDTO } from "./AccountingSettingsRowDTO";

export class PayrollImportHeadDTO implements IPayrollImportHeadDTO {
    actorCompanyId: number;
    checksum: string;
    comment: string;
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    employees: PayrollImportEmployeeDTO[];
    errorMessage: string;
    file: number[];
    fileType: TermGroup_PayrollImportHeadFileType;
    fileTypeName: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    nrOfEmployees: number;
    paymentDate: Date;
    payrollImportHeadId: number;
    state: SoeEntityState;
    status: TermGroup_PayrollImportHeadStatus;
    statusName: string;
    type: TermGroup_PayrollImportHeadType;
    typeName: string;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
        this.paymentDate = CalendarUtility.convertToDate(this.paymentDate);
    }

    public setTypes() {
        if (this.employees) {
            this.employees = _.map(this.employees, e => {
                let eObj = new PayrollImportEmployeeDTO();
                angular.extend(eObj, e);
                eObj.setTypes();
                return eObj;
            });
        } else {
            this.employees = [];
        }
    }

    public get statusIcon(): string {
        switch (this.status) {
            case TermGroup_PayrollImportHeadStatus.Unprocessed:
                return "fas fa-circle lightGrayColor";
            case TermGroup_PayrollImportHeadStatus.Processed:
                return "fas fa-circle okColor";
            case TermGroup_PayrollImportHeadStatus.Error:
                return "fas fa-circle errorColor";
            case TermGroup_PayrollImportHeadStatus.PartlyProcessed:
                return "fas fa-circle warningColor";
        }
    }

    public getStatusName(statuses: ISmallGenericType[]): string {
        return _.find(statuses, s => s.id === this.status)?.name;
    }

    public get isDeleted(): boolean {
        return this.state === SoeEntityState.Deleted;
    }
}

export class PayrollImportEmployeeDTO implements IPayrollImportEmployeeDTO {
    employeeId: number;
    employeeInfo: string;
    payrollImportEmployeeId: number;
    payrollImportHeadId: number;
    schedule: PayrollImportEmployeeScheduleDTO[];
    scheduleBreakQuantity: System.ITimeSpan;
    scheduleQuantity: System.ITimeSpan;
    scheduleRowCount: number;
    state: SoeEntityState;
    status: TermGroup_PayrollImportEmployeeStatus;
    statusName: string;
    transactionAmount: number;
    transactionQuantity: number;
    transactionRowCount: number;
    transactions: PayrollImportEmployeeTransactionDTO[];

    public setTypes() {
        if (this.schedule) {
            this.schedule = _.map(this.schedule, s => {
                let sObj = new PayrollImportEmployeeScheduleDTO();
                angular.extend(sObj, s);
                sObj.fixDates();
                return sObj;
            });
        } else {
            this.schedule = [];
        }

        if (this.transactions) {
            this.transactions = _.map(this.transactions, t => {
                let tObj = new PayrollImportEmployeeTransactionDTO();
                angular.extend(tObj, t);
                tObj.fixDates();
                tObj.setTypes();
                return tObj;
            });
        } else {
            this.transactions = [];
        }
    }

    public get statusIcon(): string {
        switch (this.status) {
            case TermGroup_PayrollImportEmployeeStatus.Unprocessed:
                return "fal fa-circle";
            case TermGroup_PayrollImportEmployeeStatus.Processed:
                return "fal fa-check-circle okColor";
            case TermGroup_PayrollImportEmployeeStatus.Error:
                return "fal fa-exclamation-triangle errorColor";
            case TermGroup_PayrollImportEmployeeStatus.PartlyProcessed:
                return "fal fa-exclamation-circle warningColor";
        }
    }

    public getStatusName(statuses: ISmallGenericType[]): string {
        return _.find(statuses, s => s.id === this.status)?.name;
    }

    public get isDeleted(): boolean {
        return this.state === SoeEntityState.Deleted;
    }
}

export class PayrollImportEmployeeScheduleDTO implements IPayrollImportEmployeeScheduleDTO {
    date: Date;
    errorMessage: string;
    isBreak: boolean;
    payrollImportEmployeeId: number;
    payrollImportEmployeeScheduleId: number;
    quantity: number;
    startTime: Date;
    state: SoeEntityState;
    status: TermGroup_PayrollImportEmployeeScheduleStatus;
    statusName: string;
    stopTime: Date;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }

    get quantityFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.quantity);
    }
    set quantityFormatted(time: string) {
        let span = CalendarUtility.parseTimeSpan(time);
        this.quantity = CalendarUtility.timeSpanToMinutes(span);
    }

    public get duration(): number {
        return this.stopTime.diffMinutes(this.startTime);
    }

    public get statusIcon(): string {
        switch (this.status) {
            case TermGroup_PayrollImportEmployeeScheduleStatus.Unprocessed:
                return "fal fa-circle";
            case TermGroup_PayrollImportEmployeeScheduleStatus.Processed:
                return "fal fa-check-circle okColor";
            case TermGroup_PayrollImportEmployeeScheduleStatus.Error:
                return "fal fa-exclamation-triangle errorColor";
        }
    }

    public getStatusName(statuses: ISmallGenericType[]): string {
        return _.find(statuses, s => s.id === this.status)?.name;
    }
}

export class PayrollImportEmployeeTransactionDTO implements IPayrollImportEmployeeTransactionDTO {
    accountCode: string;
    accountInternals: PayrollImportEmployeeTransactionAccountInternalDTO[];
    accountStdDimNr: number;
    accountStdId: number;
    accountStdName: string;
    accountStdNr: string;
    amount: number;
    code: string;
    date: Date;
    errorMessage: string;
    name: string;
    note: string;
    payrollImportEmployeeId: number;
    payrollImportEmployeeTransactionId: number;
    payrollImportEmployeeTransactionLinks: PayrollImportEmployeeTransactionLinkDTO[];
    payrollProductId: number;
    quantity: number;
    startTime: Date;
    state: SoeEntityState;
    status: TermGroup_PayrollImportEmployeeTransactionStatus;
    statusName: string;
    stopTime: Date;
    timeCodeAdditionDeductionId: number;
    timeDeviationCauseId: number;
    type: TermGroup_PayrollImportEmployeeTransactionType;
    typeName: string;

    // Extensions
    typeValue: string;
    accountingSettings: AccountingSettingsRowDTO[];

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }

    public setTypes() {
        if (this.accountInternals) {
            this.accountInternals = _.map(this.accountInternals, a => {
                let aObj = new PayrollImportEmployeeTransactionAccountInternalDTO();
                angular.extend(aObj, a);
                return aObj;
            });
        } else {
            this.accountInternals = [];
        }

        if (this.payrollImportEmployeeTransactionLinks) {
            this.payrollImportEmployeeTransactionLinks = _.map(this.payrollImportEmployeeTransactionLinks, l => {
                let lObj = new PayrollImportEmployeeTransactionLinkDTO();
                angular.extend(lObj, l);
                lObj.fixDates();
                return lObj;
            });
        } else {
            this.payrollImportEmployeeTransactionLinks = [];
        }
    }

    get quantityFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.quantity);
    }
    set quantityFormatted(time: string) {
        let span = CalendarUtility.parseTimeSpan(time);
        this.quantity = CalendarUtility.timeSpanToMinutes(span);
    }

    get quantityTime(): number {
        return this.quantity;
    }

    public get isDeleted(): boolean {
        return this.state === SoeEntityState.Deleted;
    }

    public get statusIcon(): string {
        switch (this.status) {
            case TermGroup_PayrollImportEmployeeTransactionStatus.Unprocessed:
                return "fal fa-circle";
            case TermGroup_PayrollImportEmployeeTransactionStatus.Processed:
                return "fal fa-check-circle okColor";
            case TermGroup_PayrollImportEmployeeTransactionStatus.Error:
                return "fal fa-exclamation-triangle errorColor";
        }
    }

    public getStatusName(statuses: ISmallGenericType[]): string {
        return _.find(statuses, s => s.id === this.status)?.name;
    }
}

export class PayrollImportEmployeeTransactionAccountInternalDTO implements IPayrollImportEmployeeTransactionAccountInternalDTO {
    accountCode: string;
    accountDimNr: number;
    accountId: number;
    accountName: string;
    accountNr: string;
    accountSIEDimNr: number;
    payrollImportEmployeeTransactionAccountInternalId: number;
    payrollImportEmployeeTransactionId: number;
}

export class PayrollImportEmployeeTransactionLinkDTO implements IPayrollImportEmployeeTransactionLinkDTO {
    attestStateColor: string;
    attestStateName: string;
    date: Date;
    employeeId: number;
    payrollImportEmployeeTransactionId: number;
    payrollImportEmployeeTransactionLinkId: number;
    productName: string;
    productNr: string;
    quantity: number;
    start: Date;
    state: SoeEntityState;
    stop: Date;
    timeBlockId: number;
    timePayrollTransactionId: number;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }

    get isDeleted(): boolean {
        return this.state === SoeEntityState.Deleted;
    }
}

export class PayrollStartValueHeadDTO implements IPayrollStartValueHeadDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    importedFrom: string;
    payrollStartValueHeadId: number;

    // Extensions
    rows: PayrollStartValueRowDTO[];

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }

    public setTypes() {
        if (this.rows) {
            this.rows = this.rows.map(x => {
                let obj = new PayrollStartValueRowDTO();
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            });
        } else {
            this.rows = [];
        }
    }
}

export class PayrollStartValueRowDTO implements IPayrollStartValueRowDTO {
    absenceTimeMinutes: number;
    actorCompanyId: number;
    amount: number;
    appellation: string;
    date: Date;
    doCreateTransaction: boolean;
    employeeId: number;
    employeeNr: string;
    payrollStartValueHeadId: number;
    payrollStartValueRowId: number;
    productId: number;
    productName: string;
    productNr: string;
    productNrAndName: string;
    quantity: number;
    scheduleTimeMinutes: number;
    state: SoeEntityState;
    sysPayrollStartValueId: number;
    sysPayrollTypeLevel1: TermGroup_SysPayrollType;
    sysPayrollTypeLevel2: TermGroup_SysPayrollType;
    sysPayrollTypeLevel3: TermGroup_SysPayrollType;
    sysPayrollTypeLevel4: TermGroup_SysPayrollType;

    timePayrollTransactionId: number;
    transactionAmount: number;
    transactionComment: string;
    transactionDate: Date;
    transactionLevel1: TermGroup_SysPayrollType;
    transactionLevel2: TermGroup_SysPayrollType;
    transactionLevel3: TermGroup_SysPayrollType;
    transactionLevel4: TermGroup_SysPayrollType;
    transactionProductId: number;
    transactionProductName: string;
    transactionProductNr: string;
    transactionProductNrAndName: string;
    transactionQuantity: number;
    transactionUnitPrice: number;

    // Extensions
    isModified: boolean;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.transactionDate = CalendarUtility.convertToDate(this.transactionDate);
    }
}