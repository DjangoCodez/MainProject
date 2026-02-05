import { ITimeCodeBreakGroupGridDTO, ITimeCodeAbsenceDTO, ITimeCodeBreakDTO, ITimeCodeMaterialDTO, ITimeCodeWorkDTO, ITimeCodeBaseDTO, ITimeCodeAdditionDeductionDTO, ITimeCodeInvoiceProductDTO, ITimeCodePayrollProductDTO, ITimeCodeSaveDTO, ITimeCodeRuleDTO, ITimeCodeBreakTimeCodeDeviationCauseDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_TimeCodeRegistrationType, TermGroup_TimeCodeRoundingType, SoeEntityState, SoeTimeCodeType,TermGroup_TimeCodeRuleType, TermGroup_ExpenseType, TermGroup_AdjustQuantityByBreakTime, TermGroup, TermGroup_TimeCodeClassification} from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TimeCodeBaseDTO implements ITimeCodeBaseDTO {
    actorCompanyId: number;
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    factorBasedOnWorkPercentage: boolean;
    invoiceProducts: TimeCodeInvoiceProductDTO[];
    minutesByConstantRules: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    payed: boolean;
    payrollProducts: TimeCodePayrollProductDTO[];
    registrationType: TermGroup_TimeCodeRegistrationType;
    roundingType: TermGroup_TimeCodeRoundingType;
    roundingValue: number;
    roundingTimeCodeId: number;
    roundingInterruptionTimeCodeId: number;
    roundingGroupKey: string;
    roundStartTime: boolean;
    state: SoeEntityState;
    timeCodeId: number;
    timeCodeRuleTime: Date;
    timeCodeRuleType: number;
    timeCodeRuleValue: number;
    type: SoeTimeCodeType;    
    classification: TermGroup_TimeCodeClassification

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }
}

export class TimeCodeInvoiceProductDTO implements ITimeCodeInvoiceProductDTO {
    factor: number;
    invoiceProductId: number;
    invoiceProductPrice: number;
    timeCodeId: number;
    timeCodeInvoiceProductId: number;

    // Extensions
    productName: string;
}

export class TimeCodePayrollProductDTO implements ITimeCodePayrollProductDTO {
    factor: number;
    payrollProductId: number;
    timeCodeId: number;
    timeCodePayrollProductId: number;

    // Extensions
    productName: string;
}

export class TimeCodeAbsenceDTO extends TimeCodeBaseDTO implements ITimeCodeAbsenceDTO {
    adjustQuantityByBreakTime: TermGroup_AdjustQuantityByBreakTime;
    adjustQuantityTimeCodeId: number;
    adjustQuantityTimeScheduleTypeId: number;
    isAbsence: boolean;
    kontekId: number;

    public fixDates() {
        this.timeCodeRuleTime = CalendarUtility.convertToDate(this.timeCodeRuleTime);
    }
}

export class TimeCodeAdditionDeductionDTO extends TimeCodeBaseDTO implements ITimeCodeAdditionDeductionDTO {
    comment: string;
    commentMandatory: boolean;
    expenseType: TermGroup_ExpenseType;
    fixedQuantity: number;
    hasInvoiceProducts: boolean;
    hideForEmployee: boolean;

    showInTerminal: boolean;
    stopAtAccounting: boolean;
    stopAtComment: boolean;
    stopAtDateStart: boolean;
    stopAtDateStop: boolean;
    stopAtPrice: boolean;
    stopAtVat: boolean;
}

export class TimeCodeBreakDTO extends TimeCodeBaseDTO implements ITimeCodeBreakDTO {
    defaultMinutes: number;
    employeeGroupIds: number[];
    maxMinutes: number;
    minMinutes: number;
    startTime: Date;
    startTimeMinutes: number;
    startType: number;
    stopTimeMinutes: number;
    stopType: number;
    template: boolean;
    timeCodeBreakGroupId: number;
    timeCodeDeviationCauses: TimeCodeBreakTimeCodeDeviationCauseDTO[];
    timeCodeRules: TimeCodeRuleDTO[];

    // Extensions
    get minMinutesFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.minMinutes);
    }
    set minMinutesFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.minMinutes = CalendarUtility.timeSpanToMinutes(span);
    }

    get maxMinutesFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.maxMinutes);
    }
    set maxMinutesFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.maxMinutes = CalendarUtility.timeSpanToMinutes(span);
    }

    get defaultMinutesFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.defaultMinutes);
    }
    set defaultMinutesFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.defaultMinutes = CalendarUtility.timeSpanToMinutes(span);
    }

    get startTimeMinutesFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.startTimeMinutes);
    }
    set startTimeMinutesFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.startTimeMinutes = CalendarUtility.timeSpanToMinutes(span);
    }

    get stopTimeMinutesFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.stopTimeMinutes);
    }
    set stopTimeMinutesFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.stopTimeMinutes = CalendarUtility.timeSpanToMinutes(span);
    }

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
    }
}

export class TimeCodeBreakGroupGridDTO implements ITimeCodeBreakGroupGridDTO {
    description: string;
    name: string;
    timeCodeBreakGroupId: number;
}
export class TimeCodeBreakTimeCodeDeviationCauseDTO implements ITimeCodeBreakTimeCodeDeviationCauseDTO {
    timeCodeBreakId: number;
    timeCodeBreakTimeCodeDeviationCauseId: number;
    timeCodeDeviationCauseId: number;
    timeCodeId: number;

    // Extensions
    timeCodeName: string;
    timeDeviationCauseName: string;
}

export class TimeCodeMaterialDTO extends TimeCodeBaseDTO implements ITimeCodeMaterialDTO {
    note: string;
}

export class TimeCodeRuleDTO implements ITimeCodeRuleDTO {
    type: TermGroup_TimeCodeRuleType;
    value: number;
    time: Date;

    constructor(type: TermGroup_TimeCodeRuleType, value: number) {
        this.type = type;
        this.value = value;
    }
}

export class TimeCodeSaveDTO implements ITimeCodeSaveDTO {
    adjustQuantityByBreakTime: TermGroup_AdjustQuantityByBreakTime;
    adjustQuantityTimeCodeId: number;
    adjustQuantityTimeScheduleTypeId: number;
    code: string;
    comment: string;
    commentMandatory: boolean;
    defaultMinutes: number;
    description: string;
    employeeGroupIds: number[];
    expenseType: TermGroup_ExpenseType;
    fixedQuantity: number;
    hideForEmployee: boolean;
    invoiceProducts: TimeCodeInvoiceProductDTO[];
    isAbsence: boolean;
    isWorkOutsideSchedule: boolean;
    factorBasedOnWorkPercentage: boolean;
    kontekId: number;
    maxMinutes: number;
    minMinutes: number;
    minutesByConstantRules: number;
    name: string;
    note: string;
    payed: boolean;
    payrollProducts: TimeCodePayrollProductDTO[];
    registrationType: TermGroup_TimeCodeRegistrationType;
    roundingType: TermGroup_TimeCodeRoundingType;
    roundingValue: number;
    roundingTimeCodeId: number;
    roundingInterruptionTimeCodeId: number;
    roundingGroupKey: string;
    roundStartTime: boolean;
    showInTerminal: boolean;
    startTime: Date;
    startTimeMinutes: number;
    startType: number;
    state: SoeEntityState;
    stopAtAccounting: boolean;
    stopAtComment: boolean;
    stopAtDateStart: boolean;
    stopAtDateStop: boolean;
    stopAtPrice: boolean;
    stopAtVat: boolean;
    stopTimeMinutes: number;
    stopType: number;
    template: boolean;
    timeCodeBreakGroupId: number;
    timeCodeDeviationCauses: TimeCodeBreakTimeCodeDeviationCauseDTO[];
    timeCodeId: number;
    timeCodeRuleTime: Date;
    timeCodeRuleType: TermGroup_TimeCodeRuleType;
    timeCodeRuleValue: number;
    timeCodeRules: ITimeCodeRuleDTO[];
    type: SoeTimeCodeType;
    classification: TermGroup_TimeCodeClassification;
}

export class TimeCodeWorkDTO extends TimeCodeBaseDTO implements ITimeCodeWorkDTO {
    adjustQuantityByBreakTime: TermGroup_AdjustQuantityByBreakTime;
    adjustQuantityTimeCodeId: number;
    adjustQuantityTimeScheduleTypeId: number;
    isWorkOutsideSchedule: boolean;

    public fixDates() {
        this.timeCodeRuleTime = CalendarUtility.convertToDate(this.timeCodeRuleTime);
    }
}

