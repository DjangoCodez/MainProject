import { IStaffingStatisticsIntervalValue, IStaffingStatisticsIntervalRow, IStaffingStatisticsInterval, ITimeScheduleTemplateBlockTaskDTO, IStaffingNeedsHeadSmallDTO, IStaffingNeedsHeadUserDTO, IStaffingNeedsHeadDTO, IStaffingNeedsRowTaskDTO, IStaffingNeedsRowFrequencyDTO, IStaffingNeedsRowDTO, IStaffingNeedsCalculationTimeSlot, IStaffingNeedsRowPeriodDTO, IIncomingDeliveryTypeDTO, IIncomingDeliveryRowDTO, IIncomingDeliveryHeadDTO, ITimeScheduleTaskTypeGridDTO, ITimeScheduleTaskTypeDTO, ITimeScheduleTaskDTO, IStaffingNeedsTaskDTO, System, ITimeScheduleTaskGridDTO, ITimeScheduleTaskGeneratedNeedDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { ShiftTypeDTO } from "./ShiftTypeDTO";
import { DailyRecurrenceDatesOutput } from "./DailyRecurrencePatternDTOs";
import { DayOfWeek } from "../../Util/Enumerations";
import { SoeStaffingNeedsTaskType, SoeEntityState, StaffingNeedsRowOriginType, StaffingNeedsRowType, TermGroup_StaffingNeedsHeadStatus, StaffingNeedsHeadType, TermGroup_TimeSchedulePlanningFollowUpCalculationType } from "../../Util/CommonEnumerations";
import { GraphicsUtility } from "../../Util/GraphicsUtility";
import { Constants } from "../../Util/Constants";
import { EmployeePostDTO } from "./EmployeePostDTO";

export class StaffingNeedsTaskDTO implements IStaffingNeedsTaskDTO {
    account2Id: number;
    account3Id: number;
    account4Id: number;
    account5Id: number;
    account6Id: number;
    accountId: number;
    accountName: string;
    color: string;
    description: string;
    id: number;
    isFixed: boolean;
    length: number;
    name: string;
    recurrencePattern: string;
    shiftTypeId: number;
    shiftTypeName: string;
    startTime: Date;
    stopTime: Date;
    type: SoeStaffingNeedsTaskType;

    // Extensions
    index: number;
    parentId: number;
    typeId: number;
    dateId: number;
    label1: string;
    label2: string;
    toolTip: string;
    actualStartTime: Date;
    actualStopTime: Date;
    accountDim2Name: string;
    accountDim3Name: string;
    accountDim4Name: string;
    accountDim5Name: string;
    accountDim6Name: string;
    headName: string;
    headDescription: string;
    isReccurring: boolean;
    isVisible: boolean;
    selected: boolean;
    isStaffingNeedsFrequency: boolean;

    constructor(type: SoeStaffingNeedsTaskType) {
        this.id = 0;
        this.type = type;
    }

    public fixDates(spanWholeDay: boolean) {
        if (this.startTime) {
            this.actualStartTime = CalendarUtility.convertToDate(this.startTime);
            this.startTime = CalendarUtility.convertToDate(this.startTime);
            if (spanWholeDay)
                this.startTime = this.startTime.beginningOfDay();
        }

        if (this.stopTime) {
            this.actualStopTime = CalendarUtility.convertToDate(this.stopTime);
            this.stopTime = CalendarUtility.convertToDate(this.stopTime);
            if (spanWholeDay)
                this.stopTime = this.stopTime.endOfDay();
        }
    }

    public fixColors() {
        // Remove alpha values in color property
        this.color = GraphicsUtility.removeAlphaValue(this.color, Constants.SHIFT_TYPE_UNSPECIFIED_COLOR);
    }

    public setLabel(singleLine: boolean, termMinutes: string) {
        this.label1 = '';
        this.label2 = this.accountName ? this.accountName : '';

        // Time range
        this.label1 += "{0}-{1}".format(this.actualStartTime.toFormattedTime(), this.actualStopTime.toFormattedTime());
        if (!singleLine && this.actualStopTime.diffMinutes(this.actualStartTime) > this.length)
            this.label1 += " ({0} {1})".format(this.length.toString(), termMinutes);

        if (this.isTask) {
            // Description
            if (this.description) {
                if (singleLine) {
                    this.label1 += ", {0}".format(this.description);
                } else {
                    if (this.label2.length > 0)
                        this.label2 += ", ";
                    this.label2 += this.description;
                }
            }
        } else if (this.isDelivery) {
            this.label1 += ", {0}".format(this.name);
            if (!singleLine) {
                // Description
                if (this.description) {
                    if (this.label2.length > 0)
                        this.label2 += ", ";
                    this.label2 = this.description;
                }
            }
        }
    }

    public setToolTip(termMinutes: string) {
        var toolTip: string = '';

        // Time range
        toolTip += "{0}-{1}".format(this.actualStartTime.toFormattedTime(), this.actualStopTime.toFormattedTime());
        if (this.actualStopTime.diffMinutes(this.actualStartTime) > this.length)
            toolTip += " ({0} {1})".format(this.length.toString(), termMinutes);

        // Description
        if (this.description) {
            if (toolTip && toolTip.length > 0)
                toolTip += "\n";
            toolTip += this.description;
        }

        this.toolTip = toolTip;
    }

    public get taskId(): string {
        // The identity of a task is built up by task type and id to be unique
        // For incoming deliveries, head id is also used otherwise parentId is the same as id
        return "{0}_{1}_{2}_{3}".format(this.type.toString(), this.parentId.toString(), this.id.toString(), this.dateId.toString());
    }

    public get isTask(): boolean {
        return this.type === SoeStaffingNeedsTaskType.Task;
    }

    public get isDelivery(): boolean {
        return this.type === SoeStaffingNeedsTaskType.Delivery;
    }
}

export class TimeScheduleTaskDTO implements ITimeScheduleTaskDTO {
    account2Id: number;
    account3Id: number;
    account4Id: number;
    account5Id: number;
    account6Id: number;
    allowOverlapping: boolean;
    created: Date;
    createdBy: string;
    description: string;
    dontAssignBreakLeftovers: boolean;
    excludedDates: Date[];
    isStaffingNeedsFrequency: boolean;
    length: number;
    minSplitLength: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    nbrOfOccurrences: number;
    onlyOneEmployee: boolean;
    nbrOfPersons: number;
    recurrenceEndsOnDescription: string;
    recurrencePattern: string;
    recurrencePatternDescription: string;
    recurrenceStartsOnDescription: string;
    recurringDates: DailyRecurrenceDatesOutput;
    shiftTypeId: number;
    startDate: Date;
    startTime: Date;
    state: SoeEntityState;
    stopDate: Date;
    stopTime: Date;
    timeScheduleTaskId: number;
    timeScheduleTaskTypeId: number;
    accountId: number;
    accountName: string;

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public toStaffingNeedsTaskDTO(): StaffingNeedsTaskDTO {
        let task = new StaffingNeedsTaskDTO(SoeStaffingNeedsTaskType.Task);
        task.id = this.timeScheduleTaskId;
        task.parentId = this.timeScheduleTaskId;
        task.typeId = this.timeScheduleTaskTypeId;
        task.dateId = CalendarUtility.convertToDate(this.startDate).timeValueDay();
        task.shiftTypeId = this.shiftTypeId;
        task.name = this.name;
        task.description = this.description;
        task.startTime = this.startTime;
        task.stopTime = this.stopTime;
        task.length = this.length;
        task.recurrencePattern = this.recurrencePattern;
        task.accountId = this.accountId;
        task.accountName = this.accountName;
        task.account2Id = this.account2Id;
        task.account3Id = this.account3Id;
        task.account4Id = this.account4Id;
        task.account5Id = this.account5Id;
        task.account6Id = this.account6Id;
        task.isReccurring = this.recurrencePattern && this.recurrencePattern.length > 0;
        task.isStaffingNeedsFrequency = this.isStaffingNeedsFrequency;

        return task;
    }

    get lengthFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.length);
    }
    set lengthFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.length = CalendarUtility.timeSpanToMinutes(span);
    }

    get minSplitLengthFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.minSplitLength);
    }
    set minSplitLengthFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.minSplitLength = CalendarUtility.timeSpanToMinutes(span);
    }

    get hasRecurrenceDates(): boolean {
        return this.recurringDates && this.recurringDates.recurrenceDates && this.recurringDates.recurrenceDates.length > 0;
    }

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);

        if (this.excludedDates && this.excludedDates.length > 0)
            this.excludedDates = CalendarUtility.convertToDates(this.excludedDates);
        else
            this.excludedDates = [];
    }

    public setTypes() {
        let obj = new DailyRecurrenceDatesOutput();
        angular.extend(obj, this.recurringDates);
        this.recurringDates = obj;
    }

    public setTimesByRecurrence(date: Date) {
        this.startTime = this.startTime ? CalendarUtility.convertToDate(date).mergeTime(this.startTime) : CalendarUtility.convertToDate(date).beginningOfDay();
        this.stopTime = this.stopTime ? CalendarUtility.convertToDate(date).mergeTime(this.stopTime) : CalendarUtility.convertToDate(date).endOfDay();
    }
}

export class TimeScheduleTaskGridDTO implements ITimeScheduleTaskGridDTO {
    accountName: string;
    allowOverlapping: boolean;
    description: string;
    dontAssignBreakLeftovers: boolean;
    isStaffingNeedsFrequency: boolean;
    length: number;
    name: string;
    nbrOfPersons: number;
    onlyOneEmployee: boolean;
    recurrenceEndsOnDescription: string;
    recurrencePatternDescription: string;
    recurrenceStartsOnDescription: string;
    shiftTypeId: number;
    shiftTypeName: string;
    startTime: Date;
    state: SoeEntityState;
    stopTime: Date;
    timeScheduleTaskId: number;
    typeId: number;
    typeName: string;

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }
}

export class TimeScheduleTaskGeneratedNeedDTO implements ITimeScheduleTaskGeneratedNeedDTO {
    date: Date;
    occurs: string;
    staffingNeedsRowId: number;
    staffingNeedsRowPeriodId: number;
    startTime: Date;
    stopTime: Date;
    type: string;
    weekDay: DayOfWeek;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }
}

export class TimeScheduleTaskTypeDTO implements ITimeScheduleTaskTypeDTO {
    accountId: number;
    accountName: string;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    timeScheduleTaskTypeId: number;
    description: string;
}

export class TimeScheduleTaskTypeGridDTO implements ITimeScheduleTaskTypeGridDTO {
    accountId: number;
    accountName: string;
    name: string;
    timeScheduleTaskTypeId: number;
    description: string;
}

export class IncomingDeliveryTypeDTO implements IIncomingDeliveryTypeDTO {
    accountName: string;
    accountId: number;
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    description: string;
    incomingDeliveryTypeId: number;
    length: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    nbrOfPersons: number;
    state: SoeEntityState;
}

export class IncomingDeliveryHeadDTO implements IIncomingDeliveryHeadDTO {
    created: Date;
    createdBy: string;
    description: string;
    excludedDates: Date[];
    incomingDeliveryHeadId: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    nbrOfOccurrences: number;
    recurrenceEndsOnDescription: string;
    recurrencePattern: string;
    recurrencePatternDescription: string;
    recurrenceStartsOnDescription: string;
    recurringDates: DailyRecurrenceDatesOutput;
    rows: IncomingDeliveryRowDTO[];
    startDate: Date;
    state: SoeEntityState;
    stopDate: Date;
    accountId: number;
    accountName: string;

    // Extensions
    get hasRecurrenceDates(): boolean {
        return this.recurringDates?.recurrenceDates && this.recurringDates.recurrenceDates.length > 0;
    }

    public fixDates() {
        this.excludedDates = CalendarUtility.convertToDates(this.excludedDates);
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }

    public setTypes() {
        let obj = new DailyRecurrenceDatesOutput();
        angular.extend(obj, this.recurringDates);
        this.recurringDates = obj;

        this.rows = this.rows.map(x => {
            let rObj = new IncomingDeliveryRowDTO();
            angular.extend(rObj, x);
            rObj.fixDates();
            return rObj;
        });
    }
}

export class IncomingDeliveryRowDTO implements IIncomingDeliveryRowDTO {
    account2Id: number;
    account3Id: number;
    account4Id: number;
    account5Id: number;
    account6Id: number;
    allowOverlapping: boolean;
    created: Date;
    createdBy: string;
    description: string;
    dontAssignBreakLeftovers: boolean;
    headAccountId: number;
    headAccountName: string;
    incomingDeliveryHeadId: number;
    incomingDeliveryRowId: number;
    incomingDeliveryTypeDTO: IIncomingDeliveryTypeDTO;
    incomingDeliveryTypeId: number;
    length: number;
    minSplitLength: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    nbrOfPackages: number;
    nbrOfPersons: number;
    offsetDays: number;
    onlyOneEmployee: boolean;
    shiftTypeId: number;
    startTime: Date;
    state: SoeEntityState;
    stopTime: Date;
    typeName: string;

    // Extensions
    headName: string;
    incomingDeliveryTypeName: string;
    shiftTypeName: string;
    totalLength: number;
    isReccurring: boolean;
    recurrencePattern: string;
    //For showing timespan instead of minutes
    lengthTimeSpan: any;
    totalLengthTimeSpan: any;
    minSplitLengthTimeSpan: any;

    _selectedIncomingDeliveryType: IncomingDeliveryTypeDTO;
    get selectedIncomingDeliveryType(): IncomingDeliveryTypeDTO {
        return this._selectedIncomingDeliveryType;
    }
    set selectedIncomingDeliveryType(item: IncomingDeliveryTypeDTO) {
        this._selectedIncomingDeliveryType = item;
        if (this._selectedIncomingDeliveryType) {
            this.length = this._selectedIncomingDeliveryType.length;
        }
        this.incomingDeliveryTypeId = item ? item.incomingDeliveryTypeId : 0;
    }

    _selectedShiftType: ShiftTypeDTO;
    get selectedShiftType(): ShiftTypeDTO {
        return this._selectedShiftType;
    }
    set selectedShiftType(item: ShiftTypeDTO) {
        this._selectedShiftType = item;
        this.shiftTypeId = item ? item.shiftTypeId : 0;
    }

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }

    public toStaffingNeedsTaskDTO(): StaffingNeedsTaskDTO {
        let task = new StaffingNeedsTaskDTO(SoeStaffingNeedsTaskType.Delivery);
        task.id = this.incomingDeliveryRowId;
        task.parentId = this.incomingDeliveryHeadId;
        task.dateId = this.startTime.timeValueDay();
        task.shiftTypeId = this.shiftTypeId;
        task.name = this.name;
        task.description = this.description;
        task.startTime = this.startTime;
        task.stopTime = this.stopTime;
        task.length = this.length;
        task.account2Id = this.account2Id;
        task.account3Id = this.account3Id;
        task.account4Id = this.account4Id;
        task.account5Id = this.account5Id;
        task.account6Id = this.account6Id;
        task.isReccurring = this.isReccurring;
        task.recurrencePattern = this.recurrencePattern;

        task.headName = this.headName;
        task.accountId = this.headAccountId;
        task.accountName = this.headAccountName;

        return task;
    }

    public setTimesByRecurrence(date: Date) {
        this.startTime = this.startTime ? CalendarUtility.convertToDate(date).mergeTime(this.startTime) : CalendarUtility.convertToDate(date).beginningOfDay();
        this.stopTime = this.stopTime ? CalendarUtility.convertToDate(date).mergeTime(this.stopTime) : CalendarUtility.convertToDate(date).endOfDay();
    }
}

export class StaffingNeedsRowPeriodDTO implements IStaffingNeedsRowPeriodDTO {
    date: Date;
    incomingDeliveryRowId: number;
    interval: number;
    isBaseNeed: boolean;
    isBreak: boolean;
    isRemovedNeed: boolean;
    isSpecificNeed: boolean;
    length: number;
    parentId: number;
    periodGuid: System.IGuid;
    shiftTypeColor: string;
    shiftTypeId: number;
    shiftTypeName: string;
    shiftTypeNeedsCode: string;
    staffingNeedsRowId: number;
    staffingNeedsRowPeriodId: number;
    startTime: Date;
    timeScheduleTaskId: number;
    timeSlot: IStaffingNeedsCalculationTimeSlot;
    value: number;

    // Extensions
    isVisible: boolean;
    index: number;
    actualStartTime: Date;
    actualStopTime: Date;

    public fixDates() {
        if (this.startTime)
            this.startTime = CalendarUtility.convertToDate(this.startTime);
    }

    public fixColors() {
        // Remove alpha values in color property
        this.shiftTypeColor = GraphicsUtility.removeAlphaValue(this.shiftTypeColor, Constants.SHIFT_TYPE_UNSPECIFIED_COLOR);
    }
}

export class StaffingNeedsRowDTO implements IStaffingNeedsRowDTO {
    name: string;
    originType: StaffingNeedsRowOriginType;
    periods: StaffingNeedsRowPeriodDTO[];
    rowFrequencys: IStaffingNeedsRowFrequencyDTO[];
    rowNr: number;
    shiftTypeColor: string;
    shiftTypeId: number;
    shiftTypeName: string;
    staffingNeedsHeadId: number;
    staffingNeedsLocationGroupId: number;
    staffingNeedsRowId: number;
    tasks: IStaffingNeedsRowTaskDTO[];
    tempId: number;
    toolTip: string;
    type: StaffingNeedsRowType;

    // Extensions
    isVisible: boolean;
    totalLength: number;

    public get visiblePeriods(): StaffingNeedsRowPeriodDTO[] {
        return _.filter(this.periods, p => p.isVisible);
    }
}

export class StaffingNeedsHeadDTO implements IStaffingNeedsHeadDTO {
    accountId: number;
    date: Date;
    dayTypeId: number;
    description: string;
    fromDate: Date;
    interval: number;
    name: string;
    parentId: number;
    periodGuid: System.IGuid;
    rows: StaffingNeedsRowDTO[];
    staffingNeedsHeadId: number;
    staffingNeedsHeadUsers: IStaffingNeedsHeadUserDTO[];
    status: TermGroup_StaffingNeedsHeadStatus;
    type: StaffingNeedsHeadType;
    weekday: DayOfWeek;

    // Extensions
    statusName: string;

    public fixDates() {
        if (this.date)
            this.date = CalendarUtility.convertToDate(this.date);

        if (this.fromDate)
            this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class StaffingNeedsHeadSmallDTO implements IStaffingNeedsHeadSmallDTO {
    date: Date;
    dayTypeId: number;
    interval: number;
    name: string;
    staffingNeedsHeadId: number;
    status: TermGroup_StaffingNeedsHeadStatus;
    type: StaffingNeedsHeadType;
    weekday: DayOfWeek;

    // Extensions
    dayTypeName: string;
    statusName: string;
}

export class TimeScheduleTemplateBlockTaskDTO implements ITimeScheduleTemplateBlockTaskDTO {
    created: Date;
    createdBy: string;
    description: string;
    incomingDeliveryRowId: number;
    isIncomingDeliveryRow: boolean;
    isTimeScheduleTask: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    startTime: Date;
    state: SoeEntityState;
    stopTime: Date;
    timeScheduleTaskId: number;
    timeScheduleTemplateBlockId: number;
    timeScheduleTemplateBlockTaskId: number;

    //extensions
    tempTimeScheduleTemplateBlockId: number;

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }
}

export class StaffingStatisticsInterval implements IStaffingStatisticsInterval {
    employeeId: number;
    interval: Date;
    rows: StaffingStatisticsIntervalRow[];

    public fixDates() {
        if (this.interval)
            this.interval = CalendarUtility.convertToDate(this.interval);
    }
}

export class StaffingStatisticsIntervalRow implements IStaffingStatisticsIntervalRow {
    budget: StaffingStatisticsIntervalValue;
    forecast: StaffingStatisticsIntervalValue;
    key: number;
    targetCalculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType;
    modifiedCalculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType;
    need: number;
    needFrequency: number;
    needRowFrequency: number;
    schedule: StaffingStatisticsIntervalValue;
    scheduleAndTime: IStaffingStatisticsIntervalValue;
    templateSchedule: StaffingStatisticsIntervalValue;
    templateScheduleForEmployeePost: StaffingStatisticsIntervalValue;
    time: StaffingStatisticsIntervalValue;

    public getBudgetValue(calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType): number {
        if (this.budget) {
            if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                return this.budget.sales.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return this.budget.hours.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                return this.budget.personelCost.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                return this.budget.lpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                return this.budget.fpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)
                return this.budget.bpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                return this.budget.salaryPercent.round(2);
        }

        return 0;
    }

    public getForecastValue(calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType): number {
        if (this.forecast) {
            if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                return this.forecast.sales.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return this.forecast.hours.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                return this.forecast.personelCost.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                return this.forecast.lpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                return this.forecast.fpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)
                return this.forecast.bpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                return this.forecast.salaryPercent.round(2);
        }

        return 0;
    }

    public getTemplateScheduleValue(calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType): number {
        if (this.templateSchedule) {
            if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                return this.templateSchedule.sales.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return this.templateSchedule.hours.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                return this.templateSchedule.personelCost.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                return this.templateSchedule.lpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                return this.templateSchedule.fpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)
                return this.templateSchedule.bpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                return this.templateSchedule.salaryPercent.round(2);
        }

        return 0;
    }

    public getTemplateScheduleForEmployeePostValue(calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType): number {
        if (this.templateScheduleForEmployeePost) {
            if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return this.templateScheduleForEmployeePost.hours.round(0);
        }

        return 0;
    }

    public getScheduleValue(calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType): number {
        if (this.schedule) {
            if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                return this.schedule.sales.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return this.schedule.hours.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                return this.schedule.personelCost.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                return this.schedule.lpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                return this.schedule.fpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)
                return this.schedule.bpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                return this.schedule.salaryPercent.round(2);
        }

        return 0;
    }

    public getScheduleAndTimeValue(calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType): number {
        if (this.scheduleAndTime) {
            if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                return this.scheduleAndTime.sales.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return this.scheduleAndTime.hours.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                return this.scheduleAndTime.personelCost.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                return this.scheduleAndTime.lpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                return this.scheduleAndTime.fpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)
                return this.scheduleAndTime.bpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                return this.scheduleAndTime.salaryPercent.round(2);
        }

        return 0;
    }

    public getTimeValue(calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType): number {
        if (this.time) {
            if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                return this.time.sales.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return this.time.hours.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                return this.time.personelCost.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                return this.time.lpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                return this.time.fpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)
                return this.time.bpat.round(0);
            else if (calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                return this.time.salaryPercent.round(2);
        }

        return 0;
    }

    get budgetHoursFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.budget ? this.budget.hours : 0);
    }
    set budgetHoursFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time, false, false, true, true);
        this.budget.hours = CalendarUtility.timeSpanToMinutes(span);
    }
    get forecastHoursFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.forecast ? this.forecast.hours : 0);
    }
    set forecastHoursFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time, false, false, true, true);
        this.forecast.hours = CalendarUtility.timeSpanToMinutes(span);
    }
}

export class StaffingStatisticsIntervalValue implements IStaffingStatisticsIntervalValue {
    bpat: number;
    fpat: number;
    hours: number;
    lpat: number;
    personelCost: number;
    salaryPercent: number;
    sales: number;
}

export class PreAnalysisInformation {
    workTimePerDay: number;
    workTimePerDayLessThanRuleWorkTimeDayMinimum: boolean;
    workTimePerDayMoreThanRuleWorkTimeDayMaximumWorkDay: boolean;
    ruleWorkTimeDayMaximumWorkDay: number;
    ruleWorkTimeDayMinimum: number;
    workTimeWeekMin: number;
    workTimeWeekMax: number;
    employeePost: EmployeePostDTO;
    preAnalysisInformationDays: PreAnalysisInformationDay[];
    allEmployeePostPeriodItems: CalculationPeriodItem[];
    remainingEmployeePostPeriodItems: CalculationPeriodItem[];
    totalAllSkillPercent: number;
    totalRemainingSkillPercent: number;

    public setTypes() {
        if (this.employeePost) {
            let eObj = new EmployeePostDTO();
            angular.extend(eObj, this.employeePost);
            eObj.fixDates();
            this.employeePost = eObj;
        }

        if (this.preAnalysisInformationDays) {
            this.preAnalysisInformationDays = this.preAnalysisInformationDays.map(d => {
                let dObj = new PreAnalysisInformationDay();
                angular.extend(dObj, d);
                dObj.fixDates();
                dObj.setTypes();
                return dObj;
            });
        } else {
            this.preAnalysisInformationDays = [];
        }

        if (this.allEmployeePostPeriodItems) {
            this.allEmployeePostPeriodItems = this.allEmployeePostPeriodItems.map(p => {
                let pObj = new CalculationPeriodItem();
                angular.extend(pObj, p);
                pObj.fixDates();
                pObj.setTypes();
                return pObj;
            });
        } else {
            this.allEmployeePostPeriodItems = [];
        }

        if (this.remainingEmployeePostPeriodItems) {
            this.remainingEmployeePostPeriodItems = this.remainingEmployeePostPeriodItems.map(p => {
                let pObj = new CalculationPeriodItem();
                angular.extend(pObj, p);
                pObj.fixDates();
                pObj.setTypes();
                return pObj;
            });
        } else {
            this.remainingEmployeePostPeriodItems = [];
        }
    }
}

export class PreAnalysisInformationDay {
    date: Date;
    freeDay: boolean;
    items: CalculationPeriodItem[];
    employeePost: EmployeePostDTO;
    totalDaySkillPercent: number;
    preAnalysysInformationDayShifts: PreAnalysysInformationDayShift[];

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }

    public setTypes() {
        if (this.employeePost) {
            let eObj = new EmployeePostDTO();
            angular.extend(eObj, this.employeePost);
            eObj.fixDates();
            this.employeePost = eObj;
        }

        if (this.preAnalysysInformationDayShifts) {
            this.preAnalysysInformationDayShifts = this.preAnalysysInformationDayShifts.map(s => {
                let sObj = new PreAnalysysInformationDayShift();
                angular.extend(sObj, s);
                sObj.fixDates();
                return sObj;
            });
        } else {
            this.preAnalysysInformationDayShifts = [];
        }

        if (this.items) {
            this.items = this.items.map(p => {
                let pObj = new CalculationPeriodItem();
                angular.extend(pObj, p);
                pObj.fixDates();
                pObj.setTypes();
                return pObj;
            });
        } else {
            this.items = [];
        }
    }
}

export class PreAnalysysInformationDayShift {
    disposed: boolean;
    disposeReason: string;
    skills: string;
    start: Date;
    stop: Date;
    length: number;
    totalRemainingSkillPercent: number;
    breakLength: number;
    preferredLength: number;

    public fixDates() {
        this.start = CalendarUtility.convertToDate(this.start);
        this.stop = CalendarUtility.convertToDate(this.stop);
    }
}

export class CalculationPeriodItem {
    info: string;
    timeSlot: StaffingNeedsCalculationTimeSlot;
    calculationPeriodItemGuid: string;
    calculationGuid: string;
    staffingNeedsRowGuid: string;
    suggestedTargetCalculationGuid: string;
    staffingNeedsCalcutionHeadRowGuid: string;
    originalCalculationRowGuid: string;
    periodGuid: string;
    tempGuid: string;
    staffingNeedsHeadId: number;
    staffingNeedsRowId: number;
    staffingNeedsRowPeriodId: number;
    shiftTypeId: number;
    timeScheduleTaskId: number;
    timeScheduleTaskTypeId: number;
    incomingDeliveryRowId: number;
    timeCodeBreakGroupId: number;
    dayTypeId: number;
    employeePostId: number;
    name: string;
    weekday: DayOfWeek;
    date: Date;
    startTime: Date;
    length: number;
    interval: number;
    value: number;
    calculationRowNr: number;
    movedNrOfTimes: number;
    minSplitLength: number;
    fromBreakRules: boolean;
    onlyOneEmployee: boolean;
    dontAssignBreakLeftovers: boolean;
    allowOverlapping: boolean;
    isStaffingNeedsFrequency: boolean;
    isBreak: boolean;
    parentId: number;
    ignore: boolean;
    isFixed: boolean;
    isMergable: boolean;
    forceGrossTime: boolean;
    isNetTime: boolean;
    allowBreaks: boolean;
    breakFillsNeed: boolean;
    tempBreakMinutes: number;
    rowState: number;
    periodState: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    scheduleDate: Date;
    incomingDeliveryRowKey: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
        this.scheduleDate = CalendarUtility.convertToDate(this.scheduleDate);
    }

    public setTypes() {
        if (this.timeSlot) {
            let obj = new StaffingNeedsCalculationTimeSlot();
            angular.extend(obj, this.timeSlot);
            obj.fixDates();
            this.timeSlot = obj;
        }
    }
}

export class StaffingNeedsCalculationTimeSlot implements IStaffingNeedsCalculationTimeSlot {
    calculationGuid: System.IGuid;
    from: Date;
    isBreak: boolean;
    isFixed: boolean;
    maxTo: Date;
    middle: Date;
    minFrom: Date;
    minutes: number;
    shiftTypeId: number;
    timeSlotLength: number;
    to: Date;

    public fixDates() {
        this.from = CalendarUtility.convertToDate(this.from);
        this.maxTo = CalendarUtility.convertToDate(this.maxTo);
        this.middle = CalendarUtility.convertToDate(this.middle);
        this.minFrom = CalendarUtility.convertToDate(this.minFrom);
        this.to = CalendarUtility.convertToDate(this.to);
    }
}