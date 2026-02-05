import { IProjectDTO, IAccountingSettingsRowDTO, ISmallGenericType, IBudgetHeadDTO, IProjectWeekTotal, IProjectUserDTO, IProjectTimeBlockDTO, IProjectTimeBlockSaveDTO, IProjectCentralStatusDTO, IProjectSmallDTO, IProjectInvoiceSmallDTO, IEmployeeScheduleTransactionInfoDTO, IProjectGridDTO, IProjectTimeMatrixDTO, IProjectTimeMatrixSaveDTO, IProjectTimeMatrixSaveRowDTO, ITimeDeviationCauseDTO, ITimeCodeDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { CoreUtility } from "../../Util/CoreUtility";
import { TermGroup_ProjectType, TermGroup_ProjectStatus, TermGroup_ProjectAllocationType, SoeEntityState, TermGroup_ProjectUserType, OrderInvoiceRegistrationType, ProjectCentralHeaderGroupType, SoeOriginType, ProjectCentralStatusRowType, ProjectCentralBudgetRowType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { SmallGenericType } from "./SmallGenericType";

export class ProjectDTO implements IProjectDTO {
    projectId: number;
    type: TermGroup_ProjectType;
    actorCompanyId: number;
    parentProjectId: number;
    customerId: number;
    status: TermGroup_ProjectStatus;
    allocationType: TermGroup_ProjectAllocationType;
    invoiceId: number;
    budgetId: number;

    defaultDim1AccountId: number;
    defaultDim2AccountId: number;
    defaultDim3AccountId: number;
    defaultDim4AccountId: number;
    defaultDim5AccountId: number;
    defaultDim6AccountId: number;

    number: string;
    name: string;
    description: string;

    startDate: Date;
    stopDate: Date;
    note: string;
    useAccounting: boolean;
    priceListTypeId: number;

    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    workSiteKey: string;
    workSiteNumber: string;
    attestWorkFlowHeadId: number;

    // Collections
    accountingSettings: IAccountingSettingsRowDTO[];

    // Extensions
    statusName: string;
    creditAccounts: ISmallGenericType[];
    debitAccounts: ISmallGenericType[];

    budgetHead: IBudgetHeadDTO;

    orderTemplateId: number;
}

export class TimeProjectDTO extends ProjectDTO implements IProjectDTO {
    hasInvoices: boolean;
    invoiceProductAccountingPrio: string;
    numberOfInvoices: number;
    orderTemplateId: number;
    parentProjectName: string;
    parentProjectNr: string;
    payrollProductAccountingPrio: string;
    projectWeekTotals: IProjectWeekTotal[];
}

export class ProjectUserDTO implements IProjectUserDTO {
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    employeeCalculatedCost: number;
    internalId: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    projectId: number;
    projectUserId: number;
    state: SoeEntityState;
    timeCodeId: number;
    timeCodeName: string;
    type: TermGroup_ProjectUserType;
    typeName: string;
    userId: number;

    //Extension
    isModified: boolean;
}

export class ProjectTimeBlockDTO implements IProjectTimeBlockDTO {
    additionalTime: boolean;
    allocationType: TermGroup_ProjectAllocationType;
    created: Date;
    createdBy: string;
    customerId: number;
    customerInvoiceRowAttestStateColor: string;
    customerInvoiceRowAttestStateId: number;
    customerInvoiceRowAttestStateName: string;
    customerName: string;
    employeeChildName: string;
    employeeIsInactive: boolean;
    employeeName: string;
    employeeNr: string;
    hasComment: boolean;
    invoiceNr: string;
    isEditable: boolean;
    internOrderText: string;
    isPayrollEditable: boolean;
    isSalaryPayrollType: boolean;
    modified: Date;
    modifiedBy: string;
    month: string;
    orderClosed: boolean;
    projectInvoiceWeekId: number;
    projectName: string;
    projectNr: string;
    projectTimeBlockId: number;
    referenceOur: string;
    registrationType: OrderInvoiceRegistrationType;
    scheduledQuantity: number;
    showInvoiceRowAttestState: boolean;
    showPayrollAttestState: boolean;
    startTime: Date;
    stopTime: Date;
    timeBlockDateId: number;
    timeCodeName: string;
    timeDeviationCauseName: string;
    timeInvoiceTransactionId: number;
    timePayrollAttestStateColor: string;
    timePayrollAttestStateId: number;
    timePayrollAttestStateName: string;
    timePayrollTransactionIds: number[];
    timeSheetWeekId: number;
    week: string;
    weekDay: string;
    year: string;
    yearMonth: string;
    yearWeek: string;

    _selectedProject: any;
    get selectedProject(): any {
        return this._selectedProject;
    }
    set selectedProject(item: any) {
        this._selectedProject = item;
        this.projectId = item ? item.projectId : 0;
    }

    _projectId: number;
    get projectId(): number {
        return this._projectId;
    }
    set projectId(value: number) {
        if (this._projectId !== value) {
            this._projectId = value;
            this.isModified = true;
        }
    }

    _employeeChildId: number;
    get employeeChildId(): number {
        return this._employeeChildId;
    }
    set employeeChildId(value: number) {
        if (value !== this._employeeChildId) {
            this._employeeChildId = value;
            this.isModified = true;
        }
    }

    _selectedCustomerInvoice: any;
    get selectedCustomerInvoice(): any {
        return this._selectedCustomerInvoice;
    }
    set selectedCustomerInvoice(item: any) {
        this._selectedCustomerInvoice = item;
        this.customerInvoiceId = item ? item.invoiceId : 0;
    }

    _customerInvoiceId: number;
    get customerInvoiceId(): number {
        return this._customerInvoiceId;
    }
    set customerInvoiceId(value: number) {
        if (this._customerInvoiceId !== value) {
            this._customerInvoiceId = value;
            this.isModified = true;
        }
    }

    _selectedEmployee: any;
    get selectedEmployee(): any {
        return this._selectedEmployee;
    }
    set selectedEmployee(item: any) {
        this._selectedEmployee = item;
        this.employeeId = item ? item.employeeId : 0;
        this.employeeName = item ? item.name : undefined;
    }

    _employeeId: number;
    get employeeId(): number {
        return this._employeeId;
    }
    set employeeId(value: number) {
        if (this._employeeId !== value) {
            this._employeeId = value;
            this.isModified = true;
        }
    }

    _timeCodeId: number;
    get timeCodeId(): number {
        return this._timeCodeId;
    }
    set timeCodeId(value: number) {
        if (this._timeCodeId !== value) {
            this.isModified = true;
            this._timeCodeId = value;
        }
    }

    _timeDeviationCauseId: number;
    get timeDeviationCauseId(): number {
        return this._timeDeviationCauseId;
    }
    set timeDeviationCauseId(value: number) {
        if (value) {
            this.isModified = this._timeDeviationCauseId !== value;
        }
        this._timeDeviationCauseId = value;
    }

    _date: Date;
    get date(): Date {
        return this._date;
    }
    set date(value: Date) {
        this._date = CalendarUtility.convertToDate(value);
        this.isModified = true;
    }

    _timePayrollQuantity: number;
    get timePayrollQuantity(): number {
        return this._timePayrollQuantity;
    }
    set timePayrollQuantity(value: number) {
        if (value !== this._timePayrollQuantity) {
            this._timePayrollQuantity = value;
            this.isModified = true;
        }
    }

    _invoiceQuantity: number;
    get invoiceQuantity(): number {
        return this._invoiceQuantity;
    }
    set invoiceQuantity(value: number) {
        if (this._invoiceQuantity !== value) {
            this.isModified = true;
            this._invoiceQuantity = value;
        }
    }

    _internalNote: string;
    get internalNote(): string {
        return this._internalNote;
    }
    set internalNote(value: string) {
        if (this._internalNote !== value) {
            this._internalNote = value;
            this.isModified = true;
        }
    }

    _externalNote: string;
    get externalNote(): string {
        return this._externalNote;
    }
    set externalNote(value: string) {
        if (this._externalNote !== value) {
            this._externalNote = value;
            this.isModified = true;
        }
    }

    get noteIcon(): string {
        if ((this.externalNote && this.externalNote.length > 0) || (this.internalNote && this.internalNote.length > 0) )
            return "fal fa-file-alt";
        else
            return "fal fa-file";

    }

    // Extensions
    tempRowId: number;

    isModified: boolean;
    isDeleted: boolean;
    timeCodeReadOnly = false;
    mandatoryTime = false;
    showCustomerButton: boolean;
    showOrderButton: boolean;
    showProjectButton: boolean;
    autoGenTimeAndBreakForProject: boolean;

    filteredProjects: ProjectSmallDTO[] = []
    filteredInvoices: ProjectInvoiceSmallDTO[] = []
    filteredTimeCodes: any[] = [];
    childs: SmallGenericType[] = [];

    get weekNo(): string {
        return (this.date) ? this.date.week().toString() : "";
    }

    get timeAdditionalQuantityFormatted(): string {
        if (this.additionalTime)
            return CalendarUtility.minutesToTimeSpan(this.timePayrollQuantity);
        else
            return CalendarUtility.minutesToTimeSpan(0);
    }
    set timeAdditionalQuantityFormatted(time: string) {
        if (this.additionalTime) {
            const span = CalendarUtility.parseTimeSpan(time, false, false, false);
            this.timePayrollQuantity = CalendarUtility.timeSpanToMinutes(span);
        }
    }

    get timePayrollQuantityFormattedEdit(): string {
        if (this.additionalTime)
            return CalendarUtility.minutesToTimeSpan(0);
        else
            return CalendarUtility.minutesToTimeSpan(this.timePayrollQuantity);
    }
    set timePayrollQuantityFormattedEdit(time: string) {
        const span = CalendarUtility.parseTimeSpan(time, false, false, false);
        this.timePayrollQuantity = CalendarUtility.timeSpanToMinutes(span);
        if (this.startTime && (this.timePayrollQuantity !== undefined)) {
            this.stopTime = this.startTime.addMinutes(this.timePayrollQuantity);
        }
    }

    get timePayrollQuantityFormatted(): string {
        if (this.additionalTime)
            return CalendarUtility.minutesToTimeSpan(0);
        else
            return CalendarUtility.minutesToTimeSpan(this.timePayrollQuantity);
    }
    set timePayrollQuantityFormatted(time: string) {
        //not used
    }
    
    get invoiceQuantityFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.invoiceQuantity);
    }
    set invoiceQuantityFormatted(time: string) {
        const span = CalendarUtility.parseTimeSpan(time, false, false, false);
        this.invoiceQuantity = CalendarUtility.timeSpanToMinutes(span);
    }

    get scheduledQuantityFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.scheduledQuantity);
    }

    public toProjectTimeBlockSaveDTO(isFromTimeSheet: boolean): ProjectTimeBlockSaveDTO {
        const dto = new ProjectTimeBlockSaveDTO();

        dto.actorCompanyId = CoreUtility.actorCompanyId;
        dto.customerInvoiceId = this.customerInvoiceId;
        dto.date = this.date;
        dto.employeeId = this.employeeId;
        dto.externalNote = this.externalNote;
        dto.internalNote = this.internalNote;
        dto.invoiceQuantity = this.invoiceQuantity;
        dto.isFromTimeSheet = isFromTimeSheet;
        dto.projectId = this.projectId;
        dto.projectInvoiceDayId = 0;
        dto.projectInvoiceWeekId = this.projectInvoiceWeekId;
        dto.state = this.isDeleted ? SoeEntityState.Deleted : SoeEntityState.Active;
        dto.timeBlockDateId = this.timeBlockDateId;
        dto.timeCodeId = this.timeCodeId;
        dto.timePayrollQuantity = this.timePayrollQuantity;
        dto.timeSheetWeekId = this.timeSheetWeekId;

        return dto;
    }

    public static toProjectTimeBlockSaveDTOs(times: ProjectTimeBlockDTO[], isFromTimeSheet: boolean): ProjectTimeBlockSaveDTO[] {
        const dtos: ProjectTimeBlockSaveDTO[] = [];
        _.forEach(times, time => {
            dtos.push(time.toProjectTimeBlockSaveDTO(isFromTimeSheet));
        });

        return dtos;
    }

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
        this.date = CalendarUtility.convertToDate(this.date);
    }

    public get actualStartTime(): Date {
        const deltaDays: number = this.startTime.diffDays(Constants.DATETIME_DEFAULT);
        return this.date.mergeTime(this.startTime).addDays(deltaDays);
    }

    public get actualStopTime(): Date {
        const deltaDays: number = this.stopTime.diffDays(Constants.DATETIME_DEFAULT);
        return this.date.mergeTime(this.stopTime).addDays(deltaDays);
    }
}

export class ProjectTimeBlockSaveDTO implements IProjectTimeBlockSaveDTO {
    actorCompanyId: number;
    autoGenTimeAndBreakForProject: boolean;
    customerInvoiceId: number;
    date: Date;
    employeeChildId: number;
    employeeId: number;
    externalNote: string;
    from: Date;
    internalNote: string;
    invoiceQuantity: number;
    isFromTimeSheet: boolean;
    mandatoryTime: boolean;
    projectId: number;
    projectInvoiceDayId: number;
    projectInvoiceWeekId: number;
    projectTimeBlockId: number;
    state: SoeEntityState;
    timeBlockDateId: number;
    timeCodeId: number;
    timeDeviationCauseId: number;
    timePayrollQuantity: number;
    timeSheetWeekId: number;
    to: Date;
}

export class ProjectGridDTO implements IProjectGridDTO {
    categories: string;
    childProjects: string;
    customerId: number;
    customerName: string;
    customerNr: string;
    defaultDim2AccountName: string;
    defaultDim3AccountName: string;
    defaultDim4AccountName: string;
    defaultDim5AccountName: string;
    defaultDim6AccountName: string;
    description: string;
    isSelected: boolean;
    isVisible: boolean;
    loadOnlyPlannedAndActive: boolean;
    loadOrders: boolean;
    managerName: string;
    managerUserId: number;
    name: string;
    number: string;
    orderNr: string;
    parentProjectId: number;
    projectId: number;
    projectsWithoutCustomer: boolean;
    startDate: Date;
    state: SoeEntityState;
    status: TermGroup_ProjectStatus;
    statusName: string;
    stopDate: Date;
}

export class ProjectCentralStatusDTO {
    associatedId: number;
    budget: number;
    budgetTime: number;
    costTypeName: string;
    description: string;
    diff: number;
    diff2: number;
    employeeId: number;
    employeeName: string;
    groupRowType: ProjectCentralHeaderGroupType;
    groupRowTypeName: string;
    hasInfo: boolean;
    info: string;
    isEditable: boolean;
    isVisible: boolean;
    modified: string;
    modifiedBy: string;
    name: string;
    originType: SoeOriginType;
    rowType: ProjectCentralStatusRowType;
    type: ProjectCentralBudgetRowType;
    typeName: string;
    value: number;
    value2: number;
}

export class ProjectSmallDTO implements IProjectSmallDTO {
    allocationType: TermGroup_ProjectAllocationType;
    customerId: number;
    customerName: string;
    customerNumber: string;
    invoices: ProjectInvoiceSmallDTO[];
    name: string;
    number: string;
    numberName: string;
    projectEmployees: number[];
    projectId: number;
    projectUsers: number[];
    timeCodeId: number;
}

export class ProjectInvoiceSmallDTO implements IProjectInvoiceSmallDTO {
    customerName: string;
    invoiceId: number;
    invoiceNr: string;
    numberName: string;
    projectId: number;
}

export class EmployeeScheduleTransactionInfoDTO implements IEmployeeScheduleTransactionInfoDTO {
    autoGenTimeAndBreakForProject: boolean;
    date: Date;
    employeeGroupId: number;
    employeeId: number;
    scheduleBlocks: IProjectTimeBlockDTO[];
    timeBlocks: IProjectTimeBlockDTO[];
    timeDeviationCauseId: number;
}
