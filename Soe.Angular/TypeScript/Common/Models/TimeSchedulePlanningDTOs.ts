import { CalendarUtility } from "../../Util/CalendarUtility";
import { ITimeScheduleEventForPlanningDTO, ITemplateScheduleEmployeeDTO, ITimeScheduleShiftQueueDTO, IOrderListDTO, ITimeScheduleTypeFactorSmallDTO, ITimeSchedulePlanningMonthDetailShiftDTO, ITimeSchedulePlanningMonthDetailDTO, ISmallGenericType, IOrderShiftDTO, IShiftHistoryDTO, ITimeScheduleScenarioHeadDTO, ITimeScheduleScenarioAccountDTO, ITimeScheduleScenarioEmployeeDTO, IPreviewActivateScenarioDTO, IActivateScenarioRowDTO, IActivateScenarioDTO, ICreateTemplateFromScenarioDTO, ICreateTemplateFromScenarioRowDTO, IPreviewCreateTemplateFromScenarioDTO, IEmployeePeriodTimeSummary, IPlanningPeriodHead, IPlanningPeriod, ITimeLeisureCodeSmallDTO, ITimeScheduleEmployeePeriodDetailDTO, IAutomaticAllocationResultDTO, IAutomaticAllocationEmployeeResultDTO, IAutomaticAllocationEmployeeDayResultDTO } from "../../Scripts/TypeLite.Net4";
import { DayOfWeek } from "../../Util/Enumerations";
import { SmallGenericType } from "./SmallGenericType";
import { Guid } from "../../Util/StringUtility";
import { TimeScheduleTemplateBlockTaskDTO } from "./StaffingNeedsDTOs";
import { TermGroup_TimeScheduleTemplateBlockType, TermGroup_TimeScheduleTemplateBlockShiftStatus, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, TermGroup_TimeSchedulePlanningShiftStyle, TermGroup_OrderPlanningShiftInfo, XEMailAnswerType, TermGroup_TimeSchedulePlanningFollowUpCalculationType, SoeEntityState, SoeScheduleWorkRules, TermGroup_TimeScheduleScenarioHeadSourceType, TermGroup_TimeSchedulePlanningShiftTypePosition, TermGroup_TimeSchedulePlanningTimePosition, TermGroup_TimeSchedulePlanningBreakVisibility, SoeTimeScheduleEmployeePeriodDetailType, LeisureCodeAllocationEmployeeStatus, TermGroup_TimeScheduleTemplateBlockAbsenceType, TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { GraphicsUtility } from "../../Util/GraphicsUtility";
import { TimeScheduleTemplateHeadSmallDTO } from "./TimeScheduleTemplateDTOs";
import { ShiftTypeDTO } from "./ShiftTypeDTO";

export class ShiftPeriodDTO {
    date: Date;
    dayDescription: string;

    open: number;
    assigned: number;
    wanted: number;
    unwanted: number;
    absenceRequested: number;
    absenceApproved: number;
    preliminary: number;

    plannedMinutes: number;
    workTimeMinutes: number;
    grossTime: number;
    totalCost: number;
    totalCostIncEmpTaxAndSuppCharge: number;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class TimeSchedulePlanningMonthDetailDTO implements ITimeSchedulePlanningMonthDetailDTO {
    date: Date;
    open: ITimeSchedulePlanningMonthDetailShiftDTO[];
    assigned: ITimeSchedulePlanningMonthDetailShiftDTO[];
    wanted: ITimeSchedulePlanningMonthDetailShiftDTO[];
    unwanted: ITimeSchedulePlanningMonthDetailShiftDTO[];
    absenceRequested: ITimeSchedulePlanningMonthDetailShiftDTO[];
    absenceApproved: ITimeSchedulePlanningMonthDetailShiftDTO[];
    preliminary: ITimeSchedulePlanningMonthDetailShiftDTO[];
}

export class ShiftDTO {
    type: TermGroup_TimeScheduleTemplateBlockType;
    timeScheduleTemplateBlockId: number;
    tempTimeScheduleTemplateBlockId: number;
    timeScheduleTemplateHeadId: number;
    timeScheduleTemplatePeriodId: number;
    timeScheduleEmployeePeriodId: number;
    timeScheduleScenarioHeadId: number;
    startTime: Date;
    stopTime: Date;
    absenceStartTime: Date;
    absenceStopTime: Date;
    //weekNr: number;
    belongsToPreviousDay: boolean;
    belongsToNextDay: boolean;
    timeScheduleTypeId: number;
    timeScheduleTypeCode: string;
    timeScheduleTypeName: string;
    timeScheduleTypeIsNotScheduleTime: boolean;
    timeScheduleTypeFactors: ITimeScheduleTypeFactorSmallDTO[];
    shiftTypeTimeScheduleTypeId: number;
    shiftTypeTimeScheduleTypeCode: string;
    shiftTypeTimeScheduleTypeName: string;
    //userId: number;    
    employeeId: number;
    employeeName: string;
    employeePostId: number;
    employeeChildId: number;
    _replaceWithEmployee: SmallGenericType;
    //employeeInfo: string;
    //isHiddenEmployee: boolean;
    //isVacant: boolean;
    description: string;
    //dayName: System.DayOfWeek;
    //netTime: System.ITimeSpan;
    grossTime: number;
    //breakTime: System.ITimeSpan;
    //iwhTime: System.ITimeSpan;
    //grossNetDiff: System.ITimeSpan;
    //costPerHour: number;
    totalCost: number;
    totalCostIncEmpTaxAndSuppCharge: number;
    break1Id: number;
    break1TimeCodeId: number;
    break1StartTime: Date;
    break1Minutes: number;
    break1Link: string;
    break1IsPreliminary: boolean;
    break2Id: number;
    break2TimeCodeId: number;
    break2StartTime: Date;
    break2Minutes: number;
    break2Link: string;
    break2IsPreliminary: boolean;
    break3Id: number;
    break3TimeCodeId: number;
    break3StartTime: Date;
    break3Minutes: number;
    break3Link: string;
    break3IsPreliminary: boolean;
    break4Id: number;
    break4TimeCodeId: number;
    break4StartTime: Date;
    break4Minutes: number;
    break4Link: string;
    break4IsPreliminary: boolean;
    timeCodeId: number;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
    absenceType: TermGroup_TimeScheduleTemplateBlockAbsenceType;
    shiftTypeId: number;
    shiftTypeCode: string;
    shiftTypeName: string;
    shiftTypeDesc: string;
    shiftTypeColor: string;
    shiftStatus: TermGroup_TimeScheduleTemplateBlockShiftStatus;
    shiftStatusName: string;
    shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus;
    shiftUserStatusName: string;
    extraShift: boolean;
    substituteShift: boolean;
    accountId: number;
    accountName: string;
    isPreliminary: boolean;
    nbrOfWantedInQueue: number;
    iamInQueue: boolean;
    hasSwapRequest: boolean;
    swapShiftInfo: string;
    hasShiftRequest: boolean;
    shiftRequestAnswerType: XEMailAnswerType;
    approvalTypeId: number;
    //absenceRequestShiftPlanningAction: number;
    nbrOfWeeks: number;
    originalBlockId: number;
    dayNumber: number;
    link: string;
    isLinked: boolean;
    isAbsenceRequest: boolean;
    staffingNeedsRowId: number;
    order: OrderListDTO;
    isModified: boolean;
    isDeleted: boolean;
    sortOrder: number;
    hasMultipleEmployeeAccountsOnDate: boolean;
    plannedTime: number;

    // Extensions
    index: number;
    label1: string;
    label2: string;
    toolTip: string;
    availabilityToolTip: string;
    date: Date;
    actualDateOnLoad: Date;
    actualStartTime: Date;
    actualStopTime: Date;
    duration: number;
    actualStartTimeDuringMove: Date;
    actualStopTimeDuringMove: Date;
    isBreak: boolean;
    isVisible: boolean;
    isReadOnly: boolean;
    isLended: boolean;
    isOtherAccount: boolean;
    selected: boolean;
    highlighted: boolean;
    shiftStyle: TermGroup_TimeSchedulePlanningShiftStyle = TermGroup_TimeSchedulePlanningShiftStyle.Detailed;
    tasks: TimeScheduleTemplateBlockTaskDTO[];
    shiftTypes: ShiftTypeDTO[];
    timeScheduleEmployeePeriodDetailId: number;
    timeLeisureCodeId: number;

    constructor(type: TermGroup_TimeScheduleTemplateBlockType = TermGroup_TimeScheduleTemplateBlockType.Schedule) {
        this.type = type;
    }

    public copy(keepLink: boolean, keepTasks: boolean): ShiftDTO {
        let dto = _.cloneDeep(this);
        dto.timeScheduleTemplateBlockId = 0;
        dto.tempTimeScheduleTemplateBlockId = 0;
        dto.break1Id = 0;
        dto.break2Id = 0;
        dto.break3Id = 0;
        dto.break4Id = 0;
        if (!keepLink)
            dto.link = Guid.newGuid();
        if (!keepTasks)
            dto.tasks = [];
        dto.nbrOfWantedInQueue = 0;
        dto.isModified = true;

        return dto;
    }

    public fixDates() {
        this.actualStartTime = CalendarUtility.convertToDate(this.startTime);
        this.actualStopTime = CalendarUtility.convertToDate(this.stopTime);
        this.startTime = CalendarUtility.convertToDate(this.startTime).beginningOfDay();
        this.stopTime = CalendarUtility.convertToDate(this.stopTime).endOfDay();
        this.absenceStartTime = CalendarUtility.convertToDate(this.absenceStartTime);
        this.absenceStopTime = CalendarUtility.convertToDate(this.absenceStopTime);

        this.date = CalendarUtility.convertToDate(this.startTime).date();
        this.actualDateOnLoad = CalendarUtility.convertToDate(this['actualDate']);

        this.break1StartTime = CalendarUtility.convertToDate(this.break1StartTime);
        this.break2StartTime = CalendarUtility.convertToDate(this.break2StartTime);
        this.break3StartTime = CalendarUtility.convertToDate(this.break3StartTime);
        this.break4StartTime = CalendarUtility.convertToDate(this.break4StartTime);

        // Convert breaks from 0000-01-01 to 0001-01-01 due to time zone conversion
        if (this.break1StartTime && this.break1StartTime.getUTCFullYear() === 0)
            this.break1StartTime = Constants.DATETIME_EMPTY;
        if (this.break2StartTime && this.break2StartTime.getUTCFullYear() === 0)
            this.break2StartTime = Constants.DATETIME_EMPTY;
        if (this.break3StartTime && this.break3StartTime.getUTCFullYear() === 0)
            this.break3StartTime = Constants.DATETIME_EMPTY;
        if (this.break4StartTime && this.break4StartTime.getUTCFullYear() === 0)
            this.break4StartTime = Constants.DATETIME_EMPTY;

        if (this.absenceStartTime && this.absenceStartTime.getUTCFullYear() === 0)
            this.absenceStartTime = Constants.DATETIME_EMPTY;
        if (this.absenceStopTime && this.absenceStopTime.getUTCFullYear() === 0)
            this.absenceStopTime = Constants.DATETIME_EMPTY;
    }

    public fixColors() {
        // Remove alpha values in color property
        this.shiftTypeColor = GraphicsUtility.removeAlphaValue(this.shiftTypeColor, Constants.SHIFT_TYPE_UNSPECIFIED_COLOR);
    }

    public get actualStartDate(): Date {
        let date: Date = CalendarUtility.convertToDate(this.actualStartTime).date();
        if (this.belongsToPreviousDay)
            date = date.addDays(-1);
        else if (this.belongsToNextDay)
            date = date.addDays(1);

        return date;
    }

    public get isSchedule(): boolean {
        return this.type === TermGroup_TimeScheduleTemplateBlockType.Schedule;
    }

    public get isOrder(): boolean {
        return this.type === TermGroup_TimeScheduleTemplateBlockType.Order && !!this.order;
    }

    public get isBooking(): boolean {
        return this.type === TermGroup_TimeScheduleTemplateBlockType.Booking;
    }

    public get isStandby(): boolean {
        return this.type === TermGroup_TimeScheduleTemplateBlockType.Standby;
    }

    public get isOnDuty(): boolean {
        return this.type === TermGroup_TimeScheduleTemplateBlockType.OnDuty;
    }

    public get isLeisureCode(): boolean {
        return !!this.timeLeisureCodeId;
    }

    public get isAnnualLeave(): boolean {
        return this.absenceType === TermGroup_TimeScheduleTemplateBlockAbsenceType.AnnualLeave;
    }

    public get isNeed(): boolean {
        return this.type === TermGroup_TimeScheduleTemplateBlockType.Need || (this.staffingNeedsRowId && this.staffingNeedsRowId !== 0);
    }

    public get isAbsence(): boolean {
        return this.timeDeviationCauseId && this.timeDeviationCauseId !== 0;
    }

    public get isWholeDay(): boolean {
        return this.actualStartTime.isSameMinuteAs(this.actualStartTime.beginningOfDay()) && this.actualStopTime.isSameMinuteAs(this.actualStopTime.endOfDay());
    }

    public get isWholeDayAbsence(): boolean {
        return this.isWholeDay && this.isAbsence;
    }

    public get isWanted(): boolean {
        return this.nbrOfWantedInQueue > 0;
    }

    public get isUnwanted(): boolean {
        return this.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted;
    }

    public get isZeroShift(): boolean {
        return this.actualStartTime.isSameMinuteAs(this.actualStopTime);
    }

    public get hasMultipleScheduleTypes(): boolean {
        return this.timeScheduleTypeId && this.shiftTypeTimeScheduleTypeId && this.timeScheduleTypeId !== this.shiftTypeTimeScheduleTypeId;
    }

    public setBelongsToBasedOnStartTime(date: Date) {
        if (this.actualStartTime.isSameDayAs(date)) {
            this.belongsToPreviousDay = false;
            this.belongsToNextDay = false;
        } else if (this.actualStartTime.isBeforeOnDay(date)) {
            this.belongsToPreviousDay = false;
            this.belongsToNextDay = true;
        } else {
            this.belongsToPreviousDay = true;
            this.belongsToNextDay = false;
        }
    }

    public isSameStartDate(date: Date): boolean {
        return this.actualStartTime.isSameDayAs(date);
    }

    public isSameStopDate(date: Date): boolean {
        return this.actualStopTime.isSameDayAs(date);
    }

    public getTimeScheduleTypeCodes(includeShiftTypeTimeScheduleType: boolean): string {
        let shiftTypes: string[] = [];
        if (includeShiftTypeTimeScheduleType && this.hasMultipleScheduleTypes && this.shiftTypeTimeScheduleTypeCode)
            shiftTypes.push(this.shiftTypeTimeScheduleTypeCode);
        if (this.timeScheduleTypeCode)
            shiftTypes.push(this.timeScheduleTypeCode);

        return shiftTypes.join(', ');
    }

    public getTimeScheduleTypeNames(includeShiftTypeTimeScheduleType: boolean): string {
        let shiftTypes: string[] = [];
        if (includeShiftTypeTimeScheduleType && this.hasMultipleScheduleTypes && this.shiftTypeTimeScheduleTypeName)
            shiftTypes.push(this.shiftTypeTimeScheduleTypeName);
        if (this.timeScheduleTypeName)
            shiftTypes.push(this.timeScheduleTypeName);

        return shiftTypes.join(', ');
    }

    public get replaceWithEmployee() {
        return this._replaceWithEmployee;
    }

    public set replaceWithEmployee(item: SmallGenericType) {
        this._replaceWithEmployee = item;
        if (item) {
            this.employeeId = item.id;
        }
        else {
            this.employeeId = 0;
        }
    }

    public setLabel(breakLabel: string, wholeDayLabel: string, absenceLabel: string, includeTotalBreak: boolean = false, includeAccountName: boolean = false, singleLine: boolean = false, orderPlanningShiftInfoTopRight: TermGroup_OrderPlanningShiftInfo = TermGroup_OrderPlanningShiftInfo.NoInfo, orderPlanningShiftInfoBottomLeft: TermGroup_OrderPlanningShiftInfo = TermGroup_OrderPlanningShiftInfo.NoInfo, orderPlanningShiftInfoBottomRight: TermGroup_OrderPlanningShiftInfo = TermGroup_OrderPlanningShiftInfo.NoInfo, includeShiftTypeTimeScheduleType: boolean = false, hideTimeDevistionCauseName: boolean = false, useShiftTypeCode: boolean = false) {
        if (this.isLeisureCode) {
            // Description
            this.label1 = this.description;
            return;
        }

        // First line

        this.label1 = "";

        // Deviation cause
        if (this.timeDeviationCauseName)
            this.label1 += hideTimeDevistionCauseName ? absenceLabel : this.timeDeviationCauseName;

        if (!this.isAbsenceRequest) {
            if (this.isAnnualLeave) {
                // Length
                this.label1 += " {0}".format(CalendarUtility.minutesToTimeSpan(this.getShiftLength()));
            } else {
                // Time range
                if (this.isWholeDay)
                    this.label1 += " {0}".format(wholeDayLabel);
                else
                    this.label1 += " {0}-{1}".format(this.actualStartTime.toFormattedTime(), this.actualStopTime.toFormattedTime());
            }
        }

        if (this.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.Detailed) {
            if (singleLine) {
                // Order number
                if (this.isOrder)
                    this.label1 += "  {0}".format(this.order.customerName);

                // Shift type
                if (useShiftTypeCode && this.shiftTypeCode)
                    this.label1 += " {0}".format(this.shiftTypeCode);
                else if (this.shiftTypeName)
                    this.label1 += " {0}".format(this.shiftTypeName);

                if (includeAccountName && this.accountName)
                    this.label1 += " ({0})".format(this.accountName);

                // ScheduleTypes
                let scheduleTypeCodes = this.getTimeScheduleTypeCodes(includeShiftTypeTimeScheduleType);
                if (scheduleTypeCodes)
                    this.label1 += " - {0}".format(scheduleTypeCodes);

                // Description
                if (this.description)
                    this.label1 += ", {0}".format(this.description.replace(/[\n\r]/g, " "));
            } else {
                if (this.isOrder) {
                    switch (orderPlanningShiftInfoTopRight) {
                        case TermGroup_OrderPlanningShiftInfo.ShiftType:
                            if (this.shiftTypeName)
                                this.label1 += "  {0}".format(this.shiftTypeName);
                            break;
                        case TermGroup_OrderPlanningShiftInfo.CustomerName:
                            this.label1 += "  {0}".format(this.order.customerName);
                            break;
                    }
                }
            }

            // Break length
            if (includeTotalBreak)
                this.label1 += this.getTotalBreakText(breakLabel);
        }
        else
            singleLine = true;

        // Second line

        this.label2 = "";

        if (!singleLine) {
            if (this.isOrder) {
                switch (orderPlanningShiftInfoBottomLeft) {
                    case TermGroup_OrderPlanningShiftInfo.ShiftType:
                        if (this.shiftTypeName)
                            this.label2 += this.shiftTypeName;
                        break;
                    case TermGroup_OrderPlanningShiftInfo.CustomerName:
                        this.label2 += this.order.customerName;
                        break;
                    case TermGroup_OrderPlanningShiftInfo.DeliveryAddress:
                        if (this.order.deliveryAddress)
                            this.label2 += this.order.deliveryAddress;
                        break;
                }
                switch (orderPlanningShiftInfoBottomRight) {
                    case TermGroup_OrderPlanningShiftInfo.ShiftType:
                        if (this.shiftTypeName) {
                            if (this.label2.length > 0)
                                this.label2 += ", ";
                            this.label2 += this.shiftTypeName;
                        }
                        break;
                    case TermGroup_OrderPlanningShiftInfo.CustomerName:
                        if (this.label2.length > 0)
                            this.label2 += ", ";
                        this.label2 += this.order.customerName;
                        break;
                    case TermGroup_OrderPlanningShiftInfo.DeliveryAddress:
                        if (this.order.deliveryAddress) {
                            if (this.label2.length > 0)
                                this.label2 += ", ";
                            this.label2 += this.order.deliveryAddress;
                        }
                        break;
                }
            } else {
                // Shift type
                if (useShiftTypeCode && this.shiftTypeCode)
                    this.label2 += this.shiftTypeCode;
                else if (this.shiftTypeName)
                    this.label2 += this.shiftTypeName;

                // ScheduleType
                let scheduleTypeCodes = this.getTimeScheduleTypeCodes(includeShiftTypeTimeScheduleType);
                if (scheduleTypeCodes) {
                    if (this.label2)
                        this.label2 += " - ";

                    this.label2 += scheduleTypeCodes;
                }

                // Description
                if (this.description) {
                    if (this.label2)
                        this.label2 += ", ";
                    this.label2 += this.description.replace(/[\n\r]/g, " ");
                }
            }
        }
    }

    // Times

    public getDayName() {
        return CalendarUtility.getDayName(this.date.dayOfWeek()).toUpperCaseFirstLetter();
    }

    //public getShiftTypeNameWithAbsence() {
    //    return this.shiftTypeName + (this.isAbsence ? " (" + this.timeDeviationCauseName + ")" : "");
    //}

    public getAbsenceRequestShiftTypeLabel() {
        let label = this.shiftTypeName;
        if (this.hasMultipleEmployeeAccountsOnDate && this.accountName)
            label += "/" + this.accountName;
        if (this.isAbsence)
            label += " (" + this.timeDeviationCauseName + ")"
        return label;
    }

    public getShiftLength() {
        return (!this.isWholeDayAbsence && this.actualStartTime && this.actualStopTime) ? this.actualStopTime.diffMinutes(this.actualStartTime) : 0;
    }

    public getShiftLengthDuringMove() {
        const start: Date = this.actualStartTimeDuringMove ? this.actualStartTimeDuringMove : this.actualStartTime;
        const stop: Date = this.actualStopTimeDuringMove ? this.actualStopTimeDuringMove : this.actualStopTime;
        return (start && stop) ? stop.diffMinutes(start) : 0;
    }

    public setDefaultTimes(periodStart: Date = null) {
        // Set default times
        this.startTime = this.actualStartTime = this.startTime.beginningOfDay();
        this.stopTime = this.actualStopTime = this.startTime.beginningOfDay();
        if (periodStart)
            this.setDayNumber(periodStart);
    }

    public prepareShiftsForSave(defaultTimeCodeId: number) {
        // Fix times
        this.setTimesForSave();

        // Whole day absence stop time must be restored back before saved
        // When loaded it set to stretch over the whole day
        if (this.isWholeDayAbsence)
            this.stopTime = this.stopTime.beginningOfDay();

        // Default TimeCode
        if (!this.timeCodeId && defaultTimeCodeId)
            this.timeCodeId = defaultTimeCodeId;

        // Remove properties not used for save
        //this.shiftTypes = [];
    }

    public setTimesForSave() {
        this.clearSeconds();
        this['originalStartTime'] = this.startTime;
        this['originalStopTime'] = this.stopTime;
        this.startTime = this.actualStartTime;
        this.stopTime = this.actualStopTime;
    }

    public resetTimesForSave() {
        this.startTime = this['originalStartTime'];
        this.stopTime = this['originalStopTime'];
    }

    public setDayNumber(periodStart: Date) {
        this.dayNumber = this.actualStartDate.beginningOfDay().diffDays(periodStart.beginningOfDay()) + 1;
    }

    public clearSeconds() {
        if (this.startTime)
            this.startTime = this.startTime.clearSeconds();
        if (this.stopTime)
            this.stopTime = this.stopTime.clearSeconds();
        if (this.actualStartTime)
            this.actualStartTime = this.actualStartTime.clearSeconds();
        if (this.actualStopTime)
            this.actualStopTime = this.actualStopTime.clearSeconds();
    }

    public roundTimes(clockRounding: number, onlyActual: boolean = true) {
        if (clockRounding === 0)
            return;

        this.actualStartTime = this.actualStartTime.roundMinutes(clockRounding);
        this.actualStopTime = this.actualStopTime.roundMinutes(clockRounding);
        if (!onlyActual) {
            this.startTime = this.startTime.roundMinutes(clockRounding);
            this.stopTime = this.stopTime.roundMinutes(clockRounding);
        }
    }

    // Breaks

    public hasBreaks(): boolean {
        return this.break1TimeCodeId !== 0 || this.break2TimeCodeId !== 0 || this.break3TimeCodeId !== 0 || this.break4TimeCodeId !== 0;
    }

    public clearBreaks() {
        this.clearBreak(1);
        this.clearBreak(2);
        this.clearBreak(3);
        this.clearBreak(4);
    }

    public clearBreak(breakNr: number) {
        if (breakNr >= 1 && breakNr <= 4) {
            this[`break${breakNr}Id`] = 0;
            this[`break${breakNr}StartTime`] = Constants.DATETIME_EMPTY;
            this[`break${breakNr}TimeCodeId`] = 0;
            this[`break${breakNr}Minutes`] = 0;
            this[`break${breakNr}Link`] = null;
            this[`break${breakNr}IsPreliminary`] = false;
        }
    }

    public setBreakInformation(breakNr: number, blockId: number, start: Date, timeCodeId: number, length: number, link?: string, isPreliminary?: boolean) {
        if (breakNr >= 1 && breakNr <= 4) {
            this[`break${breakNr}Id`] = blockId;
            this[`break${breakNr}StartTime`] = start;
            this[`break${breakNr}Minutes`] = length;
            this[`break${breakNr}TimeCodeId`] = timeCodeId;
            this[`break${breakNr}Link`] = link;
            this[`break${breakNr}IsPreliminary`] = !!isPreliminary;
        }
    }

    public linkBreaks() {
        this.linkBreak(1);
        this.linkBreak(2);
        this.linkBreak(3);
        this.linkBreak(4);
    }

    public linkBreak(breakNr: number) {
        // If a break is within the shift boundary, set the same link on the break as on the actual shift
        var breakStart: Date = this.actualStartTime.mergeTime(this[`break${breakNr}StartTime`]);
        var breakEnd: Date = breakStart.addMinutes(this[`break${breakNr}Minutes`]);

        if (this[`break${breakNr}Minutes`] > 0 && breakStart.isSameOrAfterOnMinute(this.actualStartTime) && breakEnd.isSameOrBeforeOnMinute(this.actualStopTime))
            this[`break${breakNr}Link`] = this.link;
        else
            this.clearBreak(breakNr);
    }

    public getBreakTimeWithinShift(shiftStart?: Date, shiftStop?: Date): number {
        if (!shiftStart)
            shiftStart = this.actualStartTime;
        if (!shiftStop)
            shiftStop = this.actualStopTime;

        // Get breaks within current shift
        var breakTime: number = 0;
        var breakStart: Date = new Date();
        var breakStop: Date = new Date();

        // Break 1

        if (this.break1TimeCodeId) {
            breakStart = (this.break1StartTime.year() === 1900) ? this.startTime.mergeTime(this.break1StartTime) : this.break1StartTime;
            breakStop = breakStart.addMinutes(this.break1Minutes);
            breakTime += CalendarUtility.getIntersectingDuration(shiftStart, shiftStop, breakStart, breakStop);
        }

        // Break 2

        if (this.break2TimeCodeId) {
            breakStart = (this.break2StartTime.year() === 1900) ? this.startTime.mergeTime(this.break2StartTime) : this.break2StartTime;
            breakStop = breakStart.addMinutes(this.break2Minutes);
            breakTime += CalendarUtility.getIntersectingDuration(shiftStart, shiftStop, breakStart, breakStop);
        }

        // Break 3

        if (this.break3TimeCodeId) {
            breakStart = (this.break3StartTime.year() === 1900) ? this.startTime.mergeTime(this.break3StartTime) : this.break3StartTime;
            breakStop = breakStart.addMinutes(this.break3Minutes);
            breakTime += CalendarUtility.getIntersectingDuration(shiftStart, shiftStop, breakStart, breakStop);
        }

        // Break 4

        if (this.break4TimeCodeId) {
            breakStart = (this.break4StartTime.year() === 1900) ? this.startTime.mergeTime(this.break4StartTime) : this.break4StartTime;
            breakStop = breakStart.addMinutes(this.break4Minutes);
            breakTime += CalendarUtility.getIntersectingDuration(shiftStart, shiftStop, breakStart, breakStop);
        }

        return breakTime;
    }

    public getTimeScheduleTypeFactorsWithinShift(): number {
        if (!this.timeScheduleTypeIsNotScheduleTime && (!this.timeScheduleTypeFactors || this.timeScheduleTypeFactors.length === 0))
            return 0;

        var totalMinutes: number = 0;

        if (this.timeScheduleTypeIsNotScheduleTime) {
            // Subtract the whole shift time
            totalMinutes = this.getBreakTimeWithinShift() - this.getShiftLength();
        } else {
            _.forEach(this.timeScheduleTypeFactors, factor => {
                factor.fromTime = CalendarUtility.convertToDate(factor.fromTime);
                factor.toTime = CalendarUtility.convertToDate(factor.toTime);

                var fromTime: Date = this.actualStartTime.date().mergeTime(factor.fromTime);
                var toTime: Date = fromTime.addMinutes(factor.toTime.diffMinutes(factor.fromTime));

                var maxStart: Date = CalendarUtility.getMaxDate(this.actualStartTime, fromTime);
                var minStop: Date = CalendarUtility.getMinDate(this.actualStopTime, toTime);
                var factorMinutes: number = CalendarUtility.getIntersectingDuration(this.actualStartTime, this.actualStopTime, fromTime, toTime);
                factorMinutes -= this.getBreakTimeWithinShift(maxStart, minStop);

                // Reduce factor with 1 to get value that should be added to original time.
                // E.g: 60 minutes with factor 2 will return  60 minutes (to be added to the original 60 minutes = 120).
                //      60 minutes with factor 4 will return 180 minutes (to be added to the original 60 minutes = 240).
                var factorValue: number = factor.factor;
                if (factor.factor >= 0)
                    factorValue -= 1;

                totalMinutes += (factorMinutes * factorValue);
            });
        }

        return totalMinutes;
    }

    public createBreaksFromShift(selectedDate?: Date): ShiftDTO[] {
        if (!selectedDate)
            selectedDate = this.actualStartTime.date();

        let breaks: ShiftDTO[] = [];
        let brk: ShiftDTO;

        if (this.break1TimeCodeId) {
            brk = this.copy(true, false);
            brk.timeScheduleTemplateBlockId = brk.tempTimeScheduleTemplateBlockId = this.break1Id;
            brk.break1TimeCodeId = this.break1TimeCodeId;   // Break1TimeCodeId is used for all breaks
            brk.actualStartTime = this.break1StartTime.date().isAfterOnDay(Constants.DATETIME_DEFAULT.addDays(2)) ? this.break1StartTime : this.startTime.mergeTime(this.break1StartTime).addDays(this.break1StartTime.date().diffDays(Constants.DATETIME_DEFAULT));
            brk.actualStopTime = brk.actualStartTime.addMinutes(this.break1Minutes);
            brk.isPreliminary = this.break1IsPreliminary;
            this.setCommonBreakInfo(brk, selectedDate);
            breaks.push(brk);
        }
        if (this.break2TimeCodeId) {
            brk = this.copy(true, false);
            brk.timeScheduleTemplateBlockId = brk.tempTimeScheduleTemplateBlockId = this.break2Id;
            brk.break1TimeCodeId = this.break2TimeCodeId;   // Break1TimeCodeId is used for all breaks
            brk.actualStartTime = this.break2StartTime.date().isAfterOnDay(Constants.DATETIME_DEFAULT.addDays(2)) ? this.break2StartTime : this.startTime.mergeTime(this.break2StartTime).addDays(this.break2StartTime.date().diffDays(Constants.DATETIME_DEFAULT));
            brk.actualStopTime = brk.actualStartTime.addMinutes(this.break2Minutes);
            brk.isPreliminary = this.break2IsPreliminary;
            this.setCommonBreakInfo(brk, selectedDate);
            breaks.push(brk);
        }
        if (this.break3TimeCodeId) {
            brk = this.copy(true, false);
            brk.timeScheduleTemplateBlockId = brk.tempTimeScheduleTemplateBlockId = this.break3Id;
            brk.break1TimeCodeId = this.break3TimeCodeId;   // Break1TimeCodeId is used for all breaks
            brk.actualStartTime = this.break3StartTime.date().isAfterOnDay(Constants.DATETIME_DEFAULT.addDays(2)) ? this.break3StartTime : this.startTime.mergeTime(this.break3StartTime).addDays(this.break3StartTime.date().diffDays(Constants.DATETIME_DEFAULT));
            brk.actualStopTime = brk.actualStartTime.addMinutes(this.break3Minutes);
            brk.isPreliminary = this.break3IsPreliminary;
            this.setCommonBreakInfo(brk, selectedDate);
            breaks.push(brk);
        }
        if (this.break4TimeCodeId) {
            brk = this.copy(true, false);
            brk.timeScheduleTemplateBlockId = brk.tempTimeScheduleTemplateBlockId = this.break4Id;
            brk.break1TimeCodeId = this.break4TimeCodeId;   // Break1TimeCodeId is used for all breaks
            brk.actualStartTime = this.break4StartTime.date().isAfterOnDay(Constants.DATETIME_DEFAULT.addDays(2)) ? this.break4StartTime : this.startTime.mergeTime(this.break4StartTime).addDays(this.break4StartTime.date().diffDays(Constants.DATETIME_DEFAULT));
            brk.actualStopTime = brk.actualStartTime.addMinutes(this.break4Minutes);
            brk.isPreliminary = this.break4IsPreliminary;
            this.setCommonBreakInfo(brk, selectedDate);
            breaks.push(brk);
        }
        return breaks;
    }

    private setCommonBreakInfo(brk: ShiftDTO, selectedDate: Date) {
        brk.isBreak = true;
        brk.belongsToPreviousDay = brk.actualStartTime.isAfterOnDay(selectedDate);
        brk.belongsToNextDay = brk.actualStartTime.isBeforeOnDay(selectedDate);
        brk.employeeChildId = 0;
        brk.timeDeviationCauseId = 0;
        brk.timeDeviationCauseName = '';
        brk.timeScheduleTypeId = 0;
        brk.timeScheduleTypeCode = '';
        brk.timeScheduleTypeName = '';
        brk.shiftTypeTimeScheduleTypeId = 0;
        brk.shiftTypeTimeScheduleTypeCode = '';
        brk.shiftTypeTimeScheduleTypeName = '';
        brk.isModified = false;
    }

    public shiftToBreaks(): ShiftBreakDTO[] {
        var breaks: ShiftBreakDTO[] = [];
        for (var i = 1; i <= 4; i++) {
            var brk = this.shiftToBreak(i);
            if (brk)
                breaks.push(brk);
        }

        return breaks;
    }

    public shiftToBreak(breakNr: number): ShiftBreakDTO {
        var brk: ShiftBreakDTO = null;

        if (breakNr >= 1 && breakNr <= 4) {
            if (this[`break${breakNr}TimeCodeId`]) {
                brk = new ShiftBreakDTO();
                brk.breakId = this[`break${breakNr}Id`];
                brk.breakStartTime = this[`break${breakNr}StartTime`];
                brk.breakMinutes = this[`break${breakNr}Minutes`];
                brk.breakTimeCodeId = this[`break${breakNr}TimeCodeId`];
                brk.breakLink = this[`break${breakNr}Link`];
            }
        }

        return brk;
    }

    public breakToShift(brk: ShiftBreakDTO, breakNr: number) {
        if (breakNr >= 1 && breakNr <= 4) {
            this[`break${breakNr}Id`] = brk.breakId;
            this[`break${breakNr}StartTime`] = brk.breakStartTime;
            this[`break${breakNr}Minutes`] = brk.breakMinutes;
            this[`break${breakNr}TimeCodeId`] = brk.breakTimeCodeId;
            this[`break${breakNr}Link`] = brk.breakLink;
        }
    }

    public getTotalBreakText(breakLabel: string): string {
        if (!breakLabel)
            breakLabel = "({0})";

        const totalBreakMinutes: number = this.break1Minutes + this.break2Minutes + this.break3Minutes + this.break4Minutes;
        return " {0}".format(breakLabel.format(totalBreakMinutes.toString()));
    }

    // Order

    public getOrderRemainingTime(): number {
        return this.isOrder ? this.order.remainingTime : 0;
    }

    // Static methods

    public static getOverlappingShifts(shifts: ShiftDTO[], brk: ShiftDTO): ShiftDTO[] {
        // From the specified list of shifts, return only shifts that are overlapped by the specified break.
        return _.filter(shifts, s => CalendarUtility.isRangesOverlapping(s.actualStartTime, s.actualStopTime, brk.actualStartTime, brk.actualStopTime));
    }

    public static areShiftsOverlapping(shifts: ShiftDTO[]): boolean {
        // Check if any of the specified shifts are overlapping each other
        var prevShift: ShiftDTO = null;
        _.forEach(_.orderBy(shifts, ['actualStartTime', 'actualStopTime']), shift => {
            if (prevShift && shift.actualStartTime.isBeforeOnMinute(prevShift.actualStopTime))
                return true;
            prevShift = shift;
        });

        return false;
    }

    public static hasOverlappingBreaks(shifts: ShiftDTO[]): boolean {
        var result: boolean = false;

        // Check if the specified list of shifts and breaks has any break that spans over multiple shifts.
        // shifts should be a list of shifts and breaks (breaks as separate records).
        _.forEach(_.orderBy(_.filter(shifts, s => s.isBreak), s => s.actualStartTime), brk => {
            _.forEach(_.orderBy(_.filter(shifts, s => !s.isBreak), s => s.actualStartTime), shift => {
                var duration = CalendarUtility.getIntersectingDuration(brk.actualStartTime, brk.actualStopTime, shift.actualStartTime, shift.actualStopTime);
                if (duration) {
                    // Break intersects with current shift, check if whole break length is inside shift
                    if (duration !== brk.getShiftLength()) {
                        result = true;
                        return false;
                    }
                }
            });
        });

        return result;
    }

    public static wholeDayStartTimeSort(a: ShiftDTO, b: ShiftDTO) {
        // True before false

        // Absence request after normal shift
        if (a.isAbsenceRequest != b.isAbsenceRequest)
            return a.isAbsenceRequest ? 1 : -1;

        // On duty shift after normal shift
        if (a.isOnDuty != b.isOnDuty)
            return a.isOnDuty ? 1 : -1;

        // Whole day before not whole day
        if (a.isWholeDay != b.isWholeDay)
            return a.isWholeDay ? -1 : 1;

        if (a.actualStartTime < b.actualStartTime)
            return -1;

        if (a.actualStartTime > b.actualStartTime)
            return 1;

        return 0;
    }
}

export class ShiftBreakDTO {
    breakId: number;
    breakTimeCodeId: number;
    breakStartTime: Date;
    breakMinutes: number;
    breakLink: string;
}

export class ShiftHistoryDTO implements IShiftHistoryDTO {
    absenceRequestApprovedText: string;
    created: Date;
    createdBy: string;
    dateAndTimeChanged: boolean;
    employeeChanged: boolean;
    extraShiftChanged: boolean;
    fromDateAndTime: string;
    fromEmployeeId: number;
    fromEmployeeName: string;
    fromEmployeeNr: string;
    fromExtraShift: string;
    fromShiftStatus: string;
    fromShiftType: string;
    fromShiftUserStatus: string;
    fromStart: string;
    fromStop: string;
    fromTime: string;
    fromTimeDeviationCause: string;
    originEmployeeName: string;
    originEmployeeNr: string;
    shiftStatusChanged: boolean;
    shiftTypeChanged: boolean;
    shiftUserStatusChanged: boolean;
    timeChanged: boolean;
    timeDeviationCauseChanged: boolean;
    timeScheduleTemplateBlockId: number;
    toDateAndTime: string;
    toEmployeeId: number;
    toEmployeeName: string;
    toEmployeeNr: string;
    toExtraShift: string;
    toShiftStatus: string;
    toShiftType: string;
    toShiftUserStatus: string;
    toStart: string;
    toStop: string;
    toTime: string;
    toTimeDeviationCause: string;
    typeName: string;

    public get fromEmployeeNrAndName(): string {
        return this.fromEmployeeNr === '0' || this.fromEmployeeNr === '-1' ? this.fromEmployeeName : "({0}) {1}".format(this.fromEmployeeNr, this.fromEmployeeName);
    }

    public get toEmployeeNrAndName(): string {
        return this.toEmployeeNr === '0' || this.toEmployeeNr === '-1' ? this.toEmployeeName : "({0}) {1}".format(this.toEmployeeNr, this.toEmployeeName);
    }
}

export class SlotDTO {
    startTime: Date;
    stopTime: Date;
    employeeId: number;
    isReadOnly: boolean;
}

export class TimeScheduleShiftQueueDTO implements ITimeScheduleShiftQueueDTO {
    date: Date;
    employeeAgeDays: number;
    employeeId: number;
    employeeName: string;
    employmentDays: number;
    sort: number;
    timeScheduleTemplateBlockId: number;
    type: number;
    typeName: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class TemplateScheduleEmployeeDTO implements ITemplateScheduleEmployeeDTO {
    copyFromEmployeeId: number;
    copyFromTemplateHeadId: number;
    currentTemplate: string;
    currentTemplateNbrOfWeeks: number;
    employeeId: number;
    employeeNr: string;
    employeeNrSort: string;
    isRunning: boolean;
    isSelected: boolean;
    name: string;
    nbrOfWeeks: number;
    resultError: string;
    resultSuccess: boolean;
    templateStartDate: Date;
    templateStopDate: Date;

    // Extensions
    copyFromEmployeeName: string;
    copyFromTemplateHeadName: string;
    templateFirstMondayOfCycle: Date;
    templates: TimeScheduleTemplateHeadSmallDTO[];
    isProcessed: boolean;
    status: string;
    loadingTemplates: boolean;

    public get numberAndName(): string {
        return this.employeeNr !== '0' ? "({0}) {1}".format(this.employeeNr, this.name) : this.name;
    }
}

export class TemplateScheduleShiftDTO {
    timeScheduleTemplateBlockId: number;
    weekNbr: number;
    dayOfWeek: DayOfWeek;
    shiftTypeName: string;
    startTime: Date;
    stopTime: Date;
    breakLength: number;
    duration: number;

    public static convertShiftsToDTO(shifts: ShiftDTO[], startDate?: Date, nbrOfDays?: number): TemplateScheduleShiftDTO {
        if (!shifts || shifts.length === 0)
            return null;

        var firstShift = _.head(_.orderBy(shifts, s => s.actualStartTime, 'asc'));
        var lastShift = _.head(_.orderBy(shifts, s => s.actualStartTime, 'desc'));

        var dto = new TemplateScheduleShiftDTO();
        dto.timeScheduleTemplateBlockId = firstShift.timeScheduleTemplateBlockId;
        dto.weekNbr = CalendarUtility.getWeekNr(firstShift.dayNumber);
        dto.dayOfWeek = firstShift.actualStartTime.dayOfWeek();
        dto.shiftTypeName = _.map(shifts, s => s.shiftTypeName).join(', ');
        dto.startTime = firstShift.actualStartTime;
        dto.stopTime = lastShift.actualStopTime;
        dto.breakLength = firstShift.break1Minutes + firstShift.break2Minutes + firstShift.break3Minutes + firstShift.break4Minutes;
        dto.duration = dto.stopTime.diffMinutes(dto.startTime) - dto.breakLength;

        return dto;
    }
}

export class TimeScheduleScenarioHeadDTO implements ITimeScheduleScenarioHeadDTO {
    accounts: TimeScheduleScenarioAccountDTO[];
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    employees: TimeScheduleScenarioEmployeeDTO[];
    modified: Date;
    modifiedBy: string;
    name: string;
    sourceDateFrom: Date;
    sourceDateTo: Date;
    sourceType: TermGroup_TimeScheduleScenarioHeadSourceType;
    state: SoeEntityState;
    timeScheduleScenarioHeadId: number;

    public fixDates() {
        this.sourceDateFrom = CalendarUtility.convertToDate(this.sourceDateFrom);
        this.sourceDateTo = CalendarUtility.convertToDate(this.sourceDateTo);
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }

    public get sourceOffsetDays(): number {
        if (this.sourceDateFrom && this.dateFrom)
            return this.dateFrom.diffDays(this.sourceDateFrom);

        return 0;
    }
}

export class TimeScheduleScenarioAccountDTO implements ITimeScheduleScenarioAccountDTO {
    accountId: number;
    accountName: string;
    timeScheduleScenarioAccountId: number;
    timeScheduleScenarioHeadId: number;
}

export class TimeScheduleScenarioEmployeeDTO implements ITimeScheduleScenarioEmployeeDTO {
    employeeId: number;
    employeeName: string;
    employeeNumberAndName: string;
    needsReplacement: boolean;
    replacementEmployeeId: number;
    replacementEmployeeNumberAndName: string;
    timeScheduleScenarioEmployeeId: number;
    timeScheduleScenarioHeadId: number;
}

export class PreviewActivateScenarioDTO implements IPreviewActivateScenarioDTO {
    date: Date;
    employeeId: number;
    name: string;
    shiftTextScenario: string;
    shiftTextSchedule: string;
    statusMessage: string;
    statusName: string;
    workRule: SoeScheduleWorkRules;
    workRuleCanOverride: boolean;
    workRuleName: string;
    workRuleText: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }

    public get hasScheduleDiff(): boolean {
        return this.shiftTextScenario !== this.shiftTextSchedule;
    }

    public get hasWorkRule(): boolean {
        return !!this.workRuleName;
    }

    public get hasInvalidWorkRule(): boolean {
        return this.workRule !== SoeScheduleWorkRules.None && !this.workRuleCanOverride;
    }

    public get statusText(): string {
        let text = '';
        if (this.statusName)
            text = this.statusName;

        if (this.statusMessage)
            text += ` (${this.statusMessage || ''})`;

        return text;
    }
}

export class ActivateScenarioDTO implements IActivateScenarioDTO {
    preliminaryDateFrom: Date;
    rows: ActivateScenarioRowDTO[];
    sendMessage: boolean;
    timeScheduleScenarioHeadId: number;
    key: string;
}

export class ActivateScenarioRowDTO implements IActivateScenarioRowDTO {
    date: Date;
    employeeId: number;
    key: string;

    constructor(employeeId: number, date: Date) {
        this.employeeId = employeeId;
        this.date = date;
    }
}

export class PreviewCreateTemplateFromScenarioDTO implements IPreviewCreateTemplateFromScenarioDTO {
    date: Date;
    employeeId: number;
    name: string;
    shiftTextScenario: string;
    templateDateFrom: Date;
    templateDateTo: Date;
    workRule: SoeScheduleWorkRules;
    workRuleCanOverride: boolean;
    workRuleName: string;
    workRuleText: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.templateDateFrom = CalendarUtility.convertToDate(this.templateDateFrom);
        this.templateDateTo = CalendarUtility.convertToDate(this.templateDateTo);
    }

    public get hasWorkRule(): boolean {
        return !!this.workRuleName;
    }

    public get hasInvalidWorkRule(): boolean {
        return this.workRule !== SoeScheduleWorkRules.None && !this.workRuleCanOverride;
    }
}

export class CreateTemplateFromScenarioDTO implements ICreateTemplateFromScenarioDTO {
    dateFrom: Date;
    dateTo: Date;
    rows: CreateTemplateFromScenarioRowDTO[];
    timeScheduleScenarioHeadId: number;
    weekInCycle: number;
}

export class CreateTemplateFromScenarioRowDTO implements ICreateTemplateFromScenarioRowDTO {
    date: Date;
    employeeId: number;

    constructor(employeeId: number, date: Date) {
        this.employeeId = employeeId;
        this.date = date;
    }
}

export class TimeScheduleEmployeePeriodDetailDTO implements ITimeScheduleEmployeePeriodDetailDTO {
    date: Date;
    employeeId: number;
    state: SoeEntityState;
    timeLeisureCodeId: number;
    timeScheduleEmployeePeriodDetailId: number;
    timeScheduleEmployeePeriodId: number;
    timeScheduleScenarioHeadId: number;
    type: SoeTimeScheduleEmployeePeriodDetailType;
}

export class EmployeePeriodTimeSummary implements IEmployeePeriodTimeSummary {
    employeeId: number;
    timePeriodId: number;
    parentScheduledTimeMinutes: number;
    parentWorkedTimeMinutes: number;
    parentRuleWorkedTimeMinutes: number;
    parentPeriodBalanceTimeMinutes: number;
    childScheduledTimeMinutes: number;
    childWorkedTimeMinutes: number;
    childRuleWorkedTimeMinutes: number;
    childPeriodBalanceTimeMinutes: number;
}

export class PlanningPeriodHead implements IPlanningPeriodHead {
    timePeriodHeadId: number;
    name: string;
    childId: number;
    childName: string;
    parentPeriods: PlanningPeriod[];
    childPeriods: PlanningPeriod[];

    public setTypes() {
        if (this.parentPeriods) {
            this.parentPeriods = this.parentPeriods.map(p => {
                let obj = new PlanningPeriod();
                angular.extend(obj, p);
                obj.fixDates();
                return obj;
            });
        } else {
            this.parentPeriods = [];
        }
        if (this.childPeriods) {
            this.childPeriods = this.childPeriods.map(p => {
                let obj = new PlanningPeriod();
                angular.extend(obj, p);
                obj.fixDates();
                return obj;
            });
        } else {
            this.childPeriods = [];
        }
    }

    getChildByDate(date: Date): PlanningPeriod {
        return this.childPeriods?.length > 0 ? this.childPeriods.find(p => date >= p.startDate && date <= p.stopDate) : null;
    }
}

export class PlanningPeriod implements IPlanningPeriod {
    timePeriodId: number;
    name: string;
    startDate: Date;
    stopDate: Date;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }
}

export class TimeScheduleEventForPlanningDTO implements ITimeScheduleEventForPlanningDTO {
    closingTime: Date;
    date: Date;
    description: string;
    name: string;
    openingHoursId: number;
    openingTime: Date;
    timeScheduleEventId: number;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.openingTime = CalendarUtility.convertToDate(this.openingTime);
        this.closingTime = CalendarUtility.convertToDate(this.closingTime);
    }
}

export class TimeLeisureCodeSmallDTO implements ITimeLeisureCodeSmallDTO {
    code: string;
    name: string;
    timeLeisureCodeId: number;
}

export class AutomaticAllocationResultDTO implements IAutomaticAllocationResultDTO {
    employeeResults: AutomaticAllocationEmployeeResultDTO[];
    success: boolean;
    message: string;

    setTypes() {
        if (this.employeeResults) {
            this.employeeResults = this.employeeResults.map(r => {
                let obj = new AutomaticAllocationEmployeeResultDTO();
                angular.extend(obj, r);
                obj.setTypes();
                return obj;
            });
        } else {
            this.employeeResults = [];
        }
    }
}

export class AutomaticAllocationEmployeeResultDTO implements IAutomaticAllocationEmployeeResultDTO {
    employeeId: number;
    dayResults: AutomaticAllocationEmployeeDayResultDTO[];
    message: string;
    status: LeisureCodeAllocationEmployeeStatus;

    setTypes() {
        if (this.dayResults) {
            this.dayResults = this.dayResults.map(r => {
                let obj = new AutomaticAllocationEmployeeDayResultDTO();
                angular.extend(obj, r);
                obj.fixDates();
                return obj;
            });
        } else {
            this.dayResults = [];
        }
    }
}

export class AutomaticAllocationEmployeeDayResultDTO implements IAutomaticAllocationEmployeeDayResultDTO {
    date: Date;
    success: boolean;
    message: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class OrderListDTO implements IOrderListDTO {
    attestStateColor: string;
    attestStateName: string;
    customerId: number;
    customerName: string;
    customerNr: string;
    deliveryAddress: string;
    estimatedTime: number;
    internalDescription: string;
    keepAsPlanned: boolean;
    orderId: number;
    orderNr: number;
    plannedStartDate: Date;
    plannedStopDate: Date;
    priority: number;
    projectId: number;
    projectName: string;
    projectNr: string;
    remainingTime: number;
    shiftTypeColor: string;
    shiftTypeId: number;
    shiftTypeName: string;
    workingDescription: string;

    // Extensions
    categories: ISmallGenericType[];
    toolTip: string;
    selected: boolean;

    public fixDates() {
        this.plannedStartDate = CalendarUtility.convertToDate(this.plannedStartDate);
        this.plannedStopDate = CalendarUtility.convertToDate(this.plannedStopDate);
    }

    public fixColors() {
        this.attestStateColor = GraphicsUtility.removeAlphaValue(this.attestStateColor, Constants.SHIFT_TYPE_UNSPECIFIED_COLOR);
        this.shiftTypeColor = GraphicsUtility.removeAlphaValue(this.shiftTypeColor, Constants.SHIFT_TYPE_UNSPECIFIED_COLOR);
    }

    public fixCategories() {
        var tmpCategories: SmallGenericType[] = [];
        if (this.categories) {
            _.forEach(Object.keys(this.categories), key => {
                tmpCategories.push(new SmallGenericType(parseInt(key, 10), this.categories[key]));
            });
        }
        this.categories = tmpCategories;
    }

    public unfixCategories() {
        var tmpCategories: any = {};
        if (this.categories) {
            _.forEach(this.categories, category => {
                tmpCategories[category.id] = category.name;
            });
        }
        this.categories = tmpCategories;
    }

    public get categoryString(): string {
        var str: string = '';
        _.forEach(this.categories, category => {
            if (str.length > 0)
                str += ", ";
            str += category.name;
        });

        return str;
    }

    public get hasPlannedStopDate(): boolean {
        return !!this.plannedStopDate;
    }

    public get hasPlannedStartDate(): boolean {
        return !!this.plannedStartDate;
    }
}

export class OrderShiftDTO implements IOrderShiftDTO {
    date: Date;
    employeeName: string;
    from: string;
    shiftTypeName: string;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
    timeScheduleTemplateBlockId: number;
    to: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class AvailableTimeDTO {
    // Note! This is a copy of GetAvailableTimeOutputDTO in Soe:Business.Core
    scheduledMinutes: number;
    plannedMinutes: number;
    bookedMinutes: number;
    availableMinutes: number;

    constructor() {
        this.scheduledMinutes = 0;
        this.plannedMinutes = 0;
        this.bookedMinutes = 0;
        this.availableMinutes = 0;
    }
}

export class TimeSchedulePlanningSettingsDTO {
    doNotSearchOnFilter: boolean;
    showHiddenShifts: boolean;
    showInactiveEmployees: boolean;
    showUnemployedEmployees: boolean;
    showFullyLendedEmployees: boolean;
    defaultUserSelectionId: number;

    showEmployeeGroup: boolean;

    showCyclePlannedTime: boolean;
    showScheduleTypeFactorTime: boolean;
    showGrossTime: boolean;
    showTotalCost: boolean;
    showTotalCostIncEmpTaxAndSuppCharge: boolean;
    showWeekendSalary: boolean;
    includeLendedShiftsInTimeCalculations: boolean;
    showPlanningPeriodSummary: boolean;
    planningPeriodHeadId: number;
    showAnnualLeaveBalance: boolean;
    showAnnualLeaveBalanceFormat: TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat;

    useShiftTypeCode: boolean;
    showWeekNumber: boolean;
    shiftTypePosition: TermGroup_TimeSchedulePlanningShiftTypePosition;
    timePosition: TermGroup_TimeSchedulePlanningTimePosition;
    hideTimeOnShiftShorterThanMinutes: number;
    breakVisibility: TermGroup_TimeSchedulePlanningBreakVisibility;

    showAvailability: boolean;

    showAbsenceRequests: boolean;

    skipXEMailOnChanges: boolean;
    skipWorkRules: boolean;

    // Follow up - Common
    followUpOnBudget: boolean;
    followUpOnForecast: boolean;
    followUpOnTemplateSchedule: boolean;
    followUpOnTemplateScheduleForEmployeePost: boolean;
    followUpOnSchedule: boolean;
    followUpOnTime: boolean;

    followUpAccountDimId: number;
    followUpAccountId: number;

    // Follow up - Chart
    followUpCalculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType;
    followUpOnNeed: boolean;
    followUpOnNeedFrequency: boolean;
    followUpOnNeedRowFrequency: boolean;

    // Follow up - Table

    public get showBudget(): boolean {
        return this.followUpShowCalculationTypeSalesBudget ||
            this.followUpShowCalculationTypeHoursBudget ||
            this.followUpShowCalculationTypePersonelCostBudget ||
            this.followUpShowCalculationTypeSalaryPercentBudget ||
            this.followUpShowCalculationTypeLPATBudget ||
            this.followUpShowCalculationTypeFPATBudget;
    }

    public get showForecast(): boolean {
        return this.followUpShowCalculationTypeSalesForecast ||
            this.followUpShowCalculationTypeHoursForecast ||
            this.followUpShowCalculationTypePersonelCostForecast ||
            this.followUpShowCalculationTypeSalaryPercentForecast ||
            this.followUpShowCalculationTypeLPATForecast ||
            this.followUpShowCalculationTypeFPATForecast;
    }

    public get showTemplateSchedule(): boolean {
        return this.followUpShowCalculationTypeHoursTemplateSchedule ||
            this.followUpShowCalculationTypePersonelCostTemplateSchedule ||
            this.followUpShowCalculationTypeSalaryPercentTemplateSchedule ||
            this.followUpShowCalculationTypeLPATTemplateSchedule ||
            this.followUpShowCalculationTypeFPATTemplateSchedule;
    }

    public get showTemplateScheduleForEmployeePost(): boolean {
        // TODO: Implement?
        return false;
    }

    public get showSchedule(): boolean {
        return this.followUpShowCalculationTypeHoursSchedule ||
            this.followUpShowCalculationTypePersonelCostSchedule ||
            this.followUpShowCalculationTypeSalaryPercentSchedule ||
            this.followUpShowCalculationTypeLPATSchedule ||
            this.followUpShowCalculationTypeFPATSchedule;
    }

    public get showScheduleAndTime(): boolean {
        return this.followUpShowCalculationTypePersonelCostScheduleAndTime;
    }

    public get showTime(): boolean {
        return this.followUpShowCalculationTypeSalesTime ||
            this.followUpShowCalculationTypeHoursTime ||
            this.followUpShowCalculationTypePersonelCostTime ||
            this.followUpShowCalculationTypeSalaryPercentTime ||
            this.followUpShowCalculationTypeLPATTime ||
            this.followUpShowCalculationTypeFPATTime;
    }

    public get showSales(): boolean {
        return this.followUpShowCalculationTypeSalesBudget ||
            this.followUpShowCalculationTypeSalesForecast ||
            this.followUpShowCalculationTypeSalesTime;
    }

    public get showHours(): boolean {
        return this.followUpShowCalculationTypeHoursBudget ||
            this.followUpShowCalculationTypeHoursForecast ||
            this.followUpShowCalculationTypeHoursTemplateSchedule ||
            this.followUpShowCalculationTypeHoursSchedule ||
            this.followUpShowCalculationTypeHoursTime;
    }

    public get showPersonelCost(): boolean {
        return this.followUpShowCalculationTypePersonelCostBudget ||
            this.followUpShowCalculationTypePersonelCostForecast ||
            this.followUpShowCalculationTypePersonelCostTemplateSchedule ||
            this.followUpShowCalculationTypePersonelCostSchedule ||
            this.followUpShowCalculationTypePersonelCostScheduleAndTime ||
            this.followUpShowCalculationTypePersonelCostTime;
    }

    // Sales
    private _followUpShowCalculationTypeSales: boolean;
    public get followUpShowCalculationTypeSales(): boolean {
        return this._followUpShowCalculationTypeSales;
    }
    public set followUpShowCalculationTypeSales(value: boolean) {
        this._followUpShowCalculationTypeSales = value;
        if (value) {
            this._followUpShowCalculationTypeSalesBudget = true;
            this._followUpShowCalculationTypeSalesForecast = true;
            this._followUpShowCalculationTypeSalesTime = true;
        } else {
            this._followUpShowCalculationTypeSalesBudget = false;
            this._followUpShowCalculationTypeSalesForecast = false;
            this._followUpShowCalculationTypeSalesTime = false;
        }
    }

    private _followUpShowCalculationTypeSalesBudget: boolean;
    public get followUpShowCalculationTypeSalesBudget(): boolean {
        return this._followUpShowCalculationTypeSalesBudget;
    }
    public set followUpShowCalculationTypeSalesBudget(value: boolean) {
        this._followUpShowCalculationTypeSalesBudget = value;
        if (value)
            this._followUpShowCalculationTypeSales = true;
        else if (!this.followUpShowCalculationTypeSalesTime)
            this._followUpShowCalculationTypeSales = false;
    }

    private _followUpShowCalculationTypeSalesForecast: boolean;
    public get followUpShowCalculationTypeSalesForecast(): boolean {
        return this._followUpShowCalculationTypeSalesForecast;
    }
    public set followUpShowCalculationTypeSalesForecast(value: boolean) {
        this._followUpShowCalculationTypeSalesForecast = value;
        if (value)
            this._followUpShowCalculationTypeSales = true;
        else if (!this.followUpShowCalculationTypeSalesTime && !this.followUpShowCalculationTypeSalesBudget)
            this._followUpShowCalculationTypeSales = false;
    }

    private _followUpShowCalculationTypeSalesTime: boolean;
    public get followUpShowCalculationTypeSalesTime(): boolean {
        return this._followUpShowCalculationTypeSalesTime;
    }
    public set followUpShowCalculationTypeSalesTime(value: boolean) {
        this._followUpShowCalculationTypeSalesTime = value;
        if (value)
            this._followUpShowCalculationTypeSales = true;
        else if (!this.followUpShowCalculationTypeSalesBudget && !this.followUpShowCalculationTypeSalesForecast)
            this._followUpShowCalculationTypeSales = false;
    }

    // Hours
    private _followUpShowCalculationTypeHours: boolean;
    public get followUpShowCalculationTypeHours(): boolean {
        return this._followUpShowCalculationTypeHours;
    }
    public set followUpShowCalculationTypeHours(value: boolean) {
        this._followUpShowCalculationTypeHours = value;
        if (value) {
            this._followUpShowCalculationTypeHoursBudget = true;
            this._followUpShowCalculationTypeHoursForecast = true;
            this._followUpShowCalculationTypeHoursTemplateSchedule = true;
            this._followUpShowCalculationTypeHoursSchedule = true;
            this._followUpShowCalculationTypeHoursTime = true;
        } else {
            this._followUpShowCalculationTypeHoursBudget = false;
            this._followUpShowCalculationTypeHoursForecast = false;
            this._followUpShowCalculationTypeHoursTemplateSchedule = false;
            this._followUpShowCalculationTypeHoursSchedule = false;
            this._followUpShowCalculationTypeHoursTime = false;
        }
    }

    private _followUpShowCalculationTypeHoursBudget: boolean;
    public get followUpShowCalculationTypeHoursBudget(): boolean {
        return this._followUpShowCalculationTypeHoursBudget;
    }
    public set followUpShowCalculationTypeHoursBudget(value: boolean) {
        this._followUpShowCalculationTypeHoursBudget = value;
        if (value)
            this._followUpShowCalculationTypeHours = true;
        else if (!this.followUpShowCalculationTypeHoursForecast && !this.followUpShowCalculationTypeHoursTemplateSchedule && !this.followUpShowCalculationTypeHoursSchedule && !this.followUpShowCalculationTypeHoursTime)
            this._followUpShowCalculationTypeHours = false;
    }

    private _followUpShowCalculationTypeHoursForecast: boolean;
    public get followUpShowCalculationTypeHoursForecast(): boolean {
        return this._followUpShowCalculationTypeHoursForecast;
    }
    public set followUpShowCalculationTypeHoursForecast(value: boolean) {
        this._followUpShowCalculationTypeHoursForecast = value;
        if (value)
            this._followUpShowCalculationTypeHours = true;
        else if (!this.followUpShowCalculationTypeHoursBudget && !this.followUpShowCalculationTypeHoursTemplateSchedule && !this.followUpShowCalculationTypeHoursSchedule && !this.followUpShowCalculationTypeHoursTime)
            this._followUpShowCalculationTypeHours = false;
    }

    private _followUpShowCalculationTypeHoursTemplateSchedule: boolean;
    public get followUpShowCalculationTypeHoursTemplateSchedule(): boolean {
        return this._followUpShowCalculationTypeHoursTemplateSchedule;
    }
    public set followUpShowCalculationTypeHoursTemplateSchedule(value: boolean) {
        this._followUpShowCalculationTypeHoursTemplateSchedule = value;
        if (value)
            this._followUpShowCalculationTypeHours = true;
        else if (!this.followUpShowCalculationTypeHoursBudget && !this.followUpShowCalculationTypeHoursForecast && !this.followUpShowCalculationTypeHoursSchedule && !this.followUpShowCalculationTypeHoursTime)
            this._followUpShowCalculationTypeHours = false;
    }

    private _followUpShowCalculationTypeHoursSchedule: boolean;
    public get followUpShowCalculationTypeHoursSchedule(): boolean {
        return this._followUpShowCalculationTypeHoursSchedule;
    }
    public set followUpShowCalculationTypeHoursSchedule(value: boolean) {
        this._followUpShowCalculationTypeHoursSchedule = value;
        if (value)
            this._followUpShowCalculationTypeHours = true;
        else if (!this.followUpShowCalculationTypeHoursBudget && !this.followUpShowCalculationTypeHoursForecast && !this.followUpShowCalculationTypeHoursTemplateSchedule && !this.followUpShowCalculationTypeHoursTime)
            this._followUpShowCalculationTypeHours = false;
    }

    private _followUpShowCalculationTypeHoursTime: boolean;
    public get followUpShowCalculationTypeHoursTime(): boolean {
        return this._followUpShowCalculationTypeHoursTime;
    }
    public set followUpShowCalculationTypeHoursTime(value: boolean) {
        this._followUpShowCalculationTypeHoursTime = value;
        if (value)
            this._followUpShowCalculationTypeHours = true;
        else if (!this.followUpShowCalculationTypeHoursBudget && !this.followUpShowCalculationTypeHoursForecast && !this.followUpShowCalculationTypeHoursTemplateSchedule && !this.followUpShowCalculationTypeHoursSchedule)
            this._followUpShowCalculationTypeHours = false;
    }

    // PersonelCost
    private _followUpShowCalculationTypePersonelCost: boolean;
    public get followUpShowCalculationTypePersonelCost(): boolean {
        return this._followUpShowCalculationTypePersonelCost;
    }
    public set followUpShowCalculationTypePersonelCost(value: boolean) {
        this._followUpShowCalculationTypePersonelCost = value;
        if (value) {
            this._followUpShowCalculationTypePersonelCostBudget = true;
            this._followUpShowCalculationTypePersonelCostForecast = true;
            this._followUpShowCalculationTypePersonelCostTemplateSchedule = true;
            this._followUpShowCalculationTypePersonelCostSchedule = true;
            this._followUpShowCalculationTypePersonelCostScheduleAndTime = true;
            this._followUpShowCalculationTypePersonelCostTime = true;
        } else {
            this._followUpShowCalculationTypePersonelCostBudget = false;
            this._followUpShowCalculationTypePersonelCostForecast = false;
            this._followUpShowCalculationTypePersonelCostTemplateSchedule = false;
            this._followUpShowCalculationTypePersonelCostSchedule = false;
            this._followUpShowCalculationTypePersonelCostScheduleAndTime = false;
            this._followUpShowCalculationTypePersonelCostTime = false;
        }
    }

    private _followUpShowCalculationTypePersonelCostBudget: boolean;
    public get followUpShowCalculationTypePersonelCostBudget(): boolean {
        return this._followUpShowCalculationTypePersonelCostBudget;
    }
    public set followUpShowCalculationTypePersonelCostBudget(value: boolean) {
        this._followUpShowCalculationTypePersonelCostBudget = value;
        if (value)
            this._followUpShowCalculationTypePersonelCost = true;
        else if (!this.followUpShowCalculationTypePersonelCostForecast && !this.followUpShowCalculationTypePersonelCostTemplateSchedule && !this.followUpShowCalculationTypePersonelCostSchedule && !this.followUpShowCalculationTypePersonelCostScheduleAndTime && !this.followUpShowCalculationTypePersonelCostTime)
            this._followUpShowCalculationTypePersonelCost = false;
    }

    private _followUpShowCalculationTypePersonelCostForecast: boolean;
    public get followUpShowCalculationTypePersonelCostForecast(): boolean {
        return this._followUpShowCalculationTypePersonelCostForecast;
    }
    public set followUpShowCalculationTypePersonelCostForecast(value: boolean) {
        this._followUpShowCalculationTypePersonelCostForecast = value;
        if (value)
            this._followUpShowCalculationTypePersonelCost = true;
        else if (!this.followUpShowCalculationTypePersonelCostBudget && !this.followUpShowCalculationTypePersonelCostTemplateSchedule && !this.followUpShowCalculationTypePersonelCostSchedule && !this.followUpShowCalculationTypePersonelCostScheduleAndTime && !this.followUpShowCalculationTypePersonelCostTime)
            this._followUpShowCalculationTypePersonelCost = false;
    }

    private _followUpShowCalculationTypePersonelCostTemplateSchedule: boolean;
    public get followUpShowCalculationTypePersonelCostTemplateSchedule(): boolean {
        return this._followUpShowCalculationTypePersonelCostTemplateSchedule;
    }
    public set followUpShowCalculationTypePersonelCostTemplateSchedule(value: boolean) {
        this._followUpShowCalculationTypePersonelCostTemplateSchedule = value;
        if (value)
            this._followUpShowCalculationTypePersonelCost = true;
        else if (!this.followUpShowCalculationTypePersonelCostBudget && !this.followUpShowCalculationTypePersonelCostForecast && !this.followUpShowCalculationTypePersonelCostSchedule && !this.followUpShowCalculationTypePersonelCostScheduleAndTime && !this.followUpShowCalculationTypePersonelCostTime)
            this._followUpShowCalculationTypePersonelCost = false;
    }

    private _followUpShowCalculationTypePersonelCostSchedule: boolean;
    public get followUpShowCalculationTypePersonelCostSchedule(): boolean {
        return this._followUpShowCalculationTypePersonelCostSchedule;
    }
    public set followUpShowCalculationTypePersonelCostSchedule(value: boolean) {
        this._followUpShowCalculationTypePersonelCostSchedule = value;
        if (value)
            this._followUpShowCalculationTypePersonelCost = true;
        else if (!this.followUpShowCalculationTypePersonelCostBudget && !this.followUpShowCalculationTypePersonelCostForecast && !this.followUpShowCalculationTypePersonelCostTemplateSchedule && !this.followUpShowCalculationTypePersonelCostSchedule && !this.followUpShowCalculationTypePersonelCostScheduleAndTime && !this.followUpShowCalculationTypePersonelCostTime)
            this._followUpShowCalculationTypePersonelCost = false;
    }

    private _followUpShowCalculationTypePersonelCostScheduleAndTime: boolean;
    public get followUpShowCalculationTypePersonelCostScheduleAndTime(): boolean {
        return this._followUpShowCalculationTypePersonelCostScheduleAndTime;
    }
    public set followUpShowCalculationTypePersonelCostScheduleAndTime(value: boolean) {
        this._followUpShowCalculationTypePersonelCostScheduleAndTime = value;
        if (value)
            this._followUpShowCalculationTypePersonelCost = true;
        else if (!this.followUpShowCalculationTypePersonelCostBudget && !this.followUpShowCalculationTypePersonelCostForecast && !this.followUpShowCalculationTypePersonelCostTemplateSchedule && !this.followUpShowCalculationTypePersonelCostSchedule && !this.followUpShowCalculationTypePersonelCostTime)
            this._followUpShowCalculationTypePersonelCost = false;
    }

    private _followUpShowCalculationTypePersonelCostTime: boolean;
    public get followUpShowCalculationTypePersonelCostTime(): boolean {
        return this._followUpShowCalculationTypePersonelCostTime;
    }
    public set followUpShowCalculationTypePersonelCostTime(value: boolean) {
        this._followUpShowCalculationTypePersonelCostTime = value;
        if (value)
            this._followUpShowCalculationTypePersonelCost = true;
        else if (!this.followUpShowCalculationTypePersonelCostBudget && !this.followUpShowCalculationTypePersonelCostForecast && !this.followUpShowCalculationTypePersonelCostTemplateSchedule && !this.followUpShowCalculationTypePersonelCostSchedule && !this.followUpShowCalculationTypePersonelCostScheduleAndTime)
            this._followUpShowCalculationTypePersonelCost = false;
    }

    // SalaryPercent
    private _followUpShowCalculationTypeSalaryPercent: boolean;
    public get followUpShowCalculationTypeSalaryPercent(): boolean {
        return this._followUpShowCalculationTypeSalaryPercent;
    }
    public set followUpShowCalculationTypeSalaryPercent(value: boolean) {
        this._followUpShowCalculationTypeSalaryPercent = value;
        if (value) {
            this._followUpShowCalculationTypeSalaryPercentBudget = true;
            this._followUpShowCalculationTypeSalaryPercentForecast = true;
            this._followUpShowCalculationTypeSalaryPercentTemplateSchedule = true;
            this._followUpShowCalculationTypeSalaryPercentSchedule = true;
            this._followUpShowCalculationTypeSalaryPercentTime = true;
        } else {
            this._followUpShowCalculationTypeSalaryPercentBudget = false;
            this._followUpShowCalculationTypeSalaryPercentForecast = false;
            this._followUpShowCalculationTypeSalaryPercentTemplateSchedule = false;
            this._followUpShowCalculationTypeSalaryPercentSchedule = false;
            this._followUpShowCalculationTypeSalaryPercentTime = false;
        }
    }

    private _followUpShowCalculationTypeSalaryPercentBudget: boolean;
    public get followUpShowCalculationTypeSalaryPercentBudget(): boolean {
        return this._followUpShowCalculationTypeSalaryPercentBudget;
    }
    public set followUpShowCalculationTypeSalaryPercentBudget(value: boolean) {
        this._followUpShowCalculationTypeSalaryPercentBudget = value;
        if (value)
            this._followUpShowCalculationTypeSalaryPercent = true;
        else if (!this.followUpShowCalculationTypeSalaryPercentForecast && !this.followUpShowCalculationTypeSalaryPercentTemplateSchedule && !this.followUpShowCalculationTypeSalaryPercentSchedule && !this.followUpShowCalculationTypeSalaryPercentTime)
            this._followUpShowCalculationTypeSalaryPercent = false;
    }

    private _followUpShowCalculationTypeSalaryPercentForecast: boolean;
    public get followUpShowCalculationTypeSalaryPercentForecast(): boolean {
        return this._followUpShowCalculationTypeSalaryPercentForecast;
    }
    public set followUpShowCalculationTypeSalaryPercentForecast(value: boolean) {
        this._followUpShowCalculationTypeSalaryPercentForecast = value;
        if (value)
            this._followUpShowCalculationTypeSalaryPercent = true;
        else if (!this.followUpShowCalculationTypeSalaryPercentBudget && !this.followUpShowCalculationTypeSalaryPercentTemplateSchedule && !this.followUpShowCalculationTypeSalaryPercentSchedule && !this.followUpShowCalculationTypeSalaryPercentTime)
            this._followUpShowCalculationTypeSalaryPercent = false;
    }

    private _followUpShowCalculationTypeSalaryPercentTemplateSchedule: boolean;
    public get followUpShowCalculationTypeSalaryPercentTemplateSchedule(): boolean {
        return this._followUpShowCalculationTypeSalaryPercentTemplateSchedule;
    }
    public set followUpShowCalculationTypeSalaryPercentTemplateSchedule(value: boolean) {
        this._followUpShowCalculationTypeSalaryPercentTemplateSchedule = value;
        if (value)
            this._followUpShowCalculationTypeSalaryPercent = true;
        else if (!this.followUpShowCalculationTypeSalaryPercentBudget && !this.followUpShowCalculationTypeSalaryPercentForecast && !this.followUpShowCalculationTypeSalaryPercentSchedule && !this.followUpShowCalculationTypeSalaryPercentTime)
            this._followUpShowCalculationTypeSalaryPercent = false;
    }

    private _followUpShowCalculationTypeSalaryPercentSchedule: boolean;
    public get followUpShowCalculationTypeSalaryPercentSchedule(): boolean {
        return this._followUpShowCalculationTypeSalaryPercentSchedule;
    }
    public set followUpShowCalculationTypeSalaryPercentSchedule(value: boolean) {
        this._followUpShowCalculationTypeSalaryPercentSchedule = value;
        if (value)
            this._followUpShowCalculationTypeSalaryPercent = true;
        else if (!this.followUpShowCalculationTypeSalaryPercentBudget && !this.followUpShowCalculationTypeSalaryPercentForecast && !this.followUpShowCalculationTypeSalaryPercentTemplateSchedule && !this.followUpShowCalculationTypeSalaryPercentTime)
            this._followUpShowCalculationTypeSalaryPercent = false;
    }

    private _followUpShowCalculationTypeSalaryPercentTime: boolean;
    public get followUpShowCalculationTypeSalaryPercentTime(): boolean {
        return this._followUpShowCalculationTypeSalaryPercentTime;
    }
    public set followUpShowCalculationTypeSalaryPercentTime(value: boolean) {
        this._followUpShowCalculationTypeSalaryPercentTime = value;
        if (value)
            this._followUpShowCalculationTypeSalaryPercent = true;
        else if (!this.followUpShowCalculationTypeSalaryPercentBudget && !this.followUpShowCalculationTypeSalaryPercentForecast && !this.followUpShowCalculationTypeSalaryPercentTemplateSchedule && !this.followUpShowCalculationTypeSalaryPercentSchedule)
            this._followUpShowCalculationTypeSalaryPercent = false;
    }

    // LPAT
    private _followUpShowCalculationTypeLPAT: boolean;
    public get followUpShowCalculationTypeLPAT(): boolean {
        return this._followUpShowCalculationTypeLPAT;
    }
    public set followUpShowCalculationTypeLPAT(value: boolean) {
        this._followUpShowCalculationTypeLPAT = value;
        if (value) {
            this._followUpShowCalculationTypeLPATBudget = true;
            this._followUpShowCalculationTypeLPATForecast = true;
            this._followUpShowCalculationTypeLPATTemplateSchedule = true;
            this._followUpShowCalculationTypeLPATSchedule = true;
            this._followUpShowCalculationTypeLPATTime = true;
        } else {
            this._followUpShowCalculationTypeLPATBudget = false;
            this._followUpShowCalculationTypeLPATForecast = false;
            this._followUpShowCalculationTypeLPATTemplateSchedule = false;
            this._followUpShowCalculationTypeLPATSchedule = false;
            this._followUpShowCalculationTypeLPATTime = false;
        }
    }

    private _followUpShowCalculationTypeLPATBudget: boolean;
    public get followUpShowCalculationTypeLPATBudget(): boolean {
        return this._followUpShowCalculationTypeLPATBudget;
    }
    public set followUpShowCalculationTypeLPATBudget(value: boolean) {
        this._followUpShowCalculationTypeLPATBudget = value;
        if (value)
            this._followUpShowCalculationTypeLPAT = true;
        else if (!this.followUpShowCalculationTypeLPATForecast && !this.followUpShowCalculationTypeLPATTemplateSchedule && !this.followUpShowCalculationTypeLPATSchedule && !this.followUpShowCalculationTypeLPATTime)
            this._followUpShowCalculationTypeLPAT = false;
    }

    private _followUpShowCalculationTypeLPATForecast: boolean;
    public get followUpShowCalculationTypeLPATForecast(): boolean {
        return this._followUpShowCalculationTypeLPATForecast;
    }
    public set followUpShowCalculationTypeLPATForecast(value: boolean) {
        this._followUpShowCalculationTypeLPATForecast = value;
        if (value)
            this._followUpShowCalculationTypeLPAT = true;
        else if (!this.followUpShowCalculationTypeLPATBudget && !this.followUpShowCalculationTypeLPATTemplateSchedule && !this.followUpShowCalculationTypeLPATSchedule && !this.followUpShowCalculationTypeLPATTime)
            this._followUpShowCalculationTypeLPAT = false;
    }

    private _followUpShowCalculationTypeLPATTemplateSchedule: boolean;
    public get followUpShowCalculationTypeLPATTemplateSchedule(): boolean {
        return this._followUpShowCalculationTypeLPATTemplateSchedule;
    }
    public set followUpShowCalculationTypeLPATTemplateSchedule(value: boolean) {
        this._followUpShowCalculationTypeLPATTemplateSchedule = value;
        if (value)
            this._followUpShowCalculationTypeLPAT = true;
        else if (!this.followUpShowCalculationTypeLPATBudget && !this.followUpShowCalculationTypeLPATForecast && !this.followUpShowCalculationTypeLPATSchedule && !this.followUpShowCalculationTypeLPATTime)
            this._followUpShowCalculationTypeLPAT = false;
    }

    private _followUpShowCalculationTypeLPATSchedule: boolean;
    public get followUpShowCalculationTypeLPATSchedule(): boolean {
        return this._followUpShowCalculationTypeLPATSchedule;
    }
    public set followUpShowCalculationTypeLPATSchedule(value: boolean) {
        this._followUpShowCalculationTypeLPATSchedule = value;
        if (value)
            this._followUpShowCalculationTypeLPAT = true;
        else if (!this.followUpShowCalculationTypeLPATBudget && !this.followUpShowCalculationTypeLPATForecast && !this.followUpShowCalculationTypeLPATTemplateSchedule && !this.followUpShowCalculationTypeLPATTime)
            this._followUpShowCalculationTypeLPAT = false;
    }

    private _followUpShowCalculationTypeLPATTime: boolean;
    public get followUpShowCalculationTypeLPATTime(): boolean {
        return this._followUpShowCalculationTypeLPATTime;
    }
    public set followUpShowCalculationTypeLPATTime(value: boolean) {
        this._followUpShowCalculationTypeLPATTime = value;
        if (value)
            this._followUpShowCalculationTypeLPAT = true;
        else if (!this.followUpShowCalculationTypeLPATBudget && !this.followUpShowCalculationTypeLPATForecast && !this.followUpShowCalculationTypeLPATTemplateSchedule && !this.followUpShowCalculationTypeLPATSchedule)
            this._followUpShowCalculationTypeLPAT = false;
    }

    // FPAT
    private _followUpShowCalculationTypeFPAT: boolean;
    public get followUpShowCalculationTypeFPAT(): boolean {
        return this._followUpShowCalculationTypeFPAT;
    }
    public set followUpShowCalculationTypeFPAT(value: boolean) {
        this._followUpShowCalculationTypeFPAT = value;
        if (value) {
            this._followUpShowCalculationTypeFPATBudget = true;
            this._followUpShowCalculationTypeFPATForecast = true;
            this._followUpShowCalculationTypeFPATTemplateSchedule = true;
            this._followUpShowCalculationTypeFPATSchedule = true;
            this._followUpShowCalculationTypeFPATTime = true;
        } else {
            this._followUpShowCalculationTypeFPATBudget = false;
            this._followUpShowCalculationTypeFPATForecast = false;
            this._followUpShowCalculationTypeFPATTemplateSchedule = false;
            this._followUpShowCalculationTypeFPATSchedule = false;
            this._followUpShowCalculationTypeFPATTime = false;
        }
    }

    private _followUpShowCalculationTypeFPATBudget: boolean;
    public get followUpShowCalculationTypeFPATBudget(): boolean {
        return this._followUpShowCalculationTypeFPATBudget;
    }
    public set followUpShowCalculationTypeFPATBudget(value: boolean) {
        this._followUpShowCalculationTypeFPATBudget = value;
        if (value)
            this._followUpShowCalculationTypeFPAT = true;
        else if (!this.followUpShowCalculationTypeFPATForecast && !this.followUpShowCalculationTypeFPATTemplateSchedule && !this.followUpShowCalculationTypeFPATSchedule && !this.followUpShowCalculationTypeFPATTime)
            this._followUpShowCalculationTypeFPAT = false;
    }

    private _followUpShowCalculationTypeFPATForecast: boolean;
    public get followUpShowCalculationTypeFPATForecast(): boolean {
        return this._followUpShowCalculationTypeFPATForecast;
    }
    public set followUpShowCalculationTypeFPATForecast(value: boolean) {
        this._followUpShowCalculationTypeFPATForecast = value;
        if (value)
            this._followUpShowCalculationTypeFPAT = true;
        else if (!this.followUpShowCalculationTypeFPATBudget && !this.followUpShowCalculationTypeFPATTemplateSchedule && !this.followUpShowCalculationTypeFPATSchedule && !this.followUpShowCalculationTypeFPATTime)
            this._followUpShowCalculationTypeFPAT = false;
    }

    private _followUpShowCalculationTypeFPATTemplateSchedule: boolean;
    public get followUpShowCalculationTypeFPATTemplateSchedule(): boolean {
        return this._followUpShowCalculationTypeFPATTemplateSchedule;
    }
    public set followUpShowCalculationTypeFPATTemplateSchedule(value: boolean) {
        this._followUpShowCalculationTypeFPATTemplateSchedule = value;
        if (value)
            this._followUpShowCalculationTypeFPAT = true;
        else if (!this.followUpShowCalculationTypeFPATBudget && !this.followUpShowCalculationTypeFPATForecast && !this.followUpShowCalculationTypeFPATSchedule && !this.followUpShowCalculationTypeFPATTime)
            this._followUpShowCalculationTypeFPAT = false;
    }

    private _followUpShowCalculationTypeFPATSchedule: boolean;
    public get followUpShowCalculationTypeFPATSchedule(): boolean {
        return this._followUpShowCalculationTypeFPATSchedule;
    }
    public set followUpShowCalculationTypeFPATSchedule(value: boolean) {
        this._followUpShowCalculationTypeFPATSchedule = value;
        if (value)
            this._followUpShowCalculationTypeFPAT = true;
        else if (!this.followUpShowCalculationTypeFPATBudget && !this.followUpShowCalculationTypeFPATForecast && !this.followUpShowCalculationTypeFPATTemplateSchedule && !this.followUpShowCalculationTypeFPATTime)
            this._followUpShowCalculationTypeFPAT = false;
    }

    private _followUpShowCalculationTypeFPATTime: boolean;
    public get followUpShowCalculationTypeFPATTime(): boolean {
        return this._followUpShowCalculationTypeFPATTime;
    }
    public set followUpShowCalculationTypeFPATTime(value: boolean) {
        this._followUpShowCalculationTypeFPATTime = value;
        if (value)
            this._followUpShowCalculationTypeFPAT = true;
        else if (!this.followUpShowCalculationTypeFPATBudget && !this.followUpShowCalculationTypeFPATForecast && !this.followUpShowCalculationTypeFPATTemplateSchedule && !this.followUpShowCalculationTypeFPATSchedule)
            this._followUpShowCalculationTypeFPAT = false;
    }

    constructor(setDefaultValues: boolean) {
        if (setDefaultValues)
            this.setDefaultValues();
    }

    public setDefaultValues() {
        this.doNotSearchOnFilter = false;
        this.showHiddenShifts = true;
        this.showInactiveEmployees = false;
        this.showUnemployedEmployees = false;
        this.showFullyLendedEmployees = false;
        this.showEmployeeGroup = false;
        this.showCyclePlannedTime = false;
        this.showGrossTime = false;
        this.showTotalCost = false;
        this.showTotalCostIncEmpTaxAndSuppCharge = false;
        this.showWeekendSalary = false;
        this.includeLendedShiftsInTimeCalculations = false;
        this.showPlanningPeriodSummary = false;
        this.showAnnualLeaveBalance = false;
        this.showAnnualLeaveBalanceFormat = TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Days;
        this.useShiftTypeCode = false;
        this.showWeekNumber = true;
        this.shiftTypePosition = TermGroup_TimeSchedulePlanningShiftTypePosition.Left;
        this.timePosition = TermGroup_TimeSchedulePlanningTimePosition.Left;
        this.hideTimeOnShiftShorterThanMinutes = 0;
        this.breakVisibility = TermGroup_TimeSchedulePlanningBreakVisibility.Details;
        this.showAvailability = false;
        this.skipXEMailOnChanges = false;
        this.skipWorkRules = false;
        this.followUpOnBudget = false;
        this.followUpOnForecast = false;
        this.followUpOnTemplateSchedule = false;
        this.followUpOnTemplateScheduleForEmployeePost = false;
        this.followUpOnSchedule = false;
        this.followUpOnTime = false;
        this.followUpCalculationType = TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales;
        this.followUpOnNeed = false;
        this.followUpOnNeedFrequency = false;
        this.followUpOnNeedRowFrequency = false;
        this.followUpShowCalculationTypeSales = false;
        this.followUpShowCalculationTypeSalesBudget = false;
        this.followUpShowCalculationTypeSalesForecast = false;
        this.followUpShowCalculationTypeSalesTime = false;
        this.followUpShowCalculationTypeHours = false;
        this.followUpShowCalculationTypeHoursBudget = false;
        this.followUpShowCalculationTypeHoursForecast = false;
        this.followUpShowCalculationTypeHoursTemplateSchedule = false;
        this.followUpShowCalculationTypeHoursSchedule = false;
        this.followUpShowCalculationTypeHoursTime = false;
        this.followUpShowCalculationTypePersonelCost = false;
        this.followUpShowCalculationTypePersonelCostBudget = false;
        this.followUpShowCalculationTypePersonelCostForecast = false;
        this.followUpShowCalculationTypePersonelCostTemplateSchedule = false;
        this.followUpShowCalculationTypePersonelCostSchedule = false;
        this.followUpShowCalculationTypePersonelCostScheduleAndTime = false;
        this.followUpShowCalculationTypePersonelCostTime = false;
        this.followUpShowCalculationTypeSalaryPercent = false;
        this.followUpShowCalculationTypeSalaryPercentBudget = false;
        this.followUpShowCalculationTypeSalaryPercentForecast = false;
        this.followUpShowCalculationTypeSalaryPercentTemplateSchedule = false;
        this.followUpShowCalculationTypeSalaryPercentSchedule = false;
        this.followUpShowCalculationTypeSalaryPercentTime = false;
        this.followUpShowCalculationTypeLPAT = false;
        this.followUpShowCalculationTypeLPATBudget = false;
        this.followUpShowCalculationTypeLPATForecast = false;
        this.followUpShowCalculationTypeLPATTemplateSchedule = false;
        this.followUpShowCalculationTypeLPATSchedule = false;
        this.followUpShowCalculationTypeLPATTime = false;
        this.followUpShowCalculationTypeFPAT = false;
        this.followUpShowCalculationTypeFPATBudget = false;
        this.followUpShowCalculationTypeFPATForecast = false;
        this.followUpShowCalculationTypeFPATTemplateSchedule = false;
        this.followUpShowCalculationTypeFPATSchedule = false;
        this.followUpShowCalculationTypeFPATTime = false;
    }
}
