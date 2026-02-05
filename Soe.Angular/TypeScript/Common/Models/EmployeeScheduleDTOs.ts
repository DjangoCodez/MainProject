import { IActivateScheduleControlDTO, IActivateScheduleControlHeadDTO, IActivateScheduleControlHeadResultDTO, IActivateScheduleControlRowDTO, IActivateScheduleGridDTO, IEmployeeScheduleDTO, IEmployeeSchedulePlacementGridViewDTO, IEmploymentDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityState, TermGroup_ControlEmployeeSchedulePlacementType } from "../../Util/CommonEnumerations";

export class EmployeeScheduleDTO implements IEmployeeScheduleDTO {
    created: Date;
    createdBy: string;
    employeeId: number;
    employeeScheduleId: number;
    isPreliminary: boolean;
    modified: Date;
    modifiedBy: string;
    startDate: Date;
    startDayNumber: number;
    state: SoeEntityState;
    stopDate: Date;
    timeScheduleTemplateHeadId: number;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }
}

export class ActivateScheduleGridDTO implements IActivateScheduleGridDTO {
    accountNamesString: string;
    categoryNamesString: string;
    employeeGroupId: number;
    employeeGroupName: string;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    employeeScheduleId: number;
    employeeScheduleStartDate: Date;
    employeeScheduleStartDayNumber: number;
    employeeScheduleStopDate: Date;
    isPersonalTemplate: boolean;
    employmentEndDate: Date;
    employeeHidden: boolean;
    isPlaced: boolean;
    isPreliminary: boolean;
    simpleSchedule: boolean;
    templateEmployeeId: number;
    templateStartDate: Date;
    timeScheduleTemplateHeadId: number;
    timeScheduleTemplateHeadName: string;

    public fixDates() {
        this.employeeScheduleStartDate = CalendarUtility.convertToDate(this.employeeScheduleStartDate);
        this.employeeScheduleStopDate = CalendarUtility.convertToDate(this.employeeScheduleStopDate);
        this.templateStartDate = CalendarUtility.convertToDate(this.templateStartDate);
    }
}

export class ActivateScheduleControlDTO implements IActivateScheduleControlDTO {
    discardCheckesAll: boolean;
    discardCheckesForAbsence: boolean;
    discardCheckesForManuallyAdjusted: boolean;
    hasWarnings: boolean;
    heads: ActivateScheduleControlHeadDTO[];
    key: string;
    resultHeads: ActivateScheduleControlHeadResultDTO[];

    public createResult() {
        this.resultHeads = [];
        _.forEach(this.heads, head => {
            var resultHead = new ActivateScheduleControlHeadResultDTO();
            resultHead.reActivateAbsenceRequest = head.reActivateAbsenceRequest;
            resultHead.employeeId = head.employeeId;
            resultHead.employeeRequestId = head.employeeRequestId;
            resultHead.type = head.type;
            this.resultHeads.push(resultHead);
        });
        this.discardCheckesForAbsence = true;
        this.discardCheckesForManuallyAdjusted = true;
    }
    public fixDates() {
        _.forEach(this.heads, head => {
            head.fixDates();
        });
    }
}

export class ActivateScheduleControlHeadResultDTO implements IActivateScheduleControlHeadResultDTO {
    reActivateAbsenceRequest: boolean;
    employeeId: number;
    employeeRequestId: number;
    type: TermGroup_ControlEmployeeSchedulePlacementType;
}

export class ActivateScheduleControlHeadDTO implements IActivateScheduleControlHeadDTO {
    comment: string;
    reActivateAbsenceRequest: boolean;
    employeeId: number;
    employeeNrAndName: string;
    employeeRequestId: number;
    resultStatusName: string;
    rows: ActivateScheduleControlRowDTO[];
    startDate: Date;
    statusName: string;
    stopDate: Date;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
    type: TermGroup_ControlEmployeeSchedulePlacementType;
    typeName: string;
    hideCheckbox: boolean;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
        _.forEach(this.rows, row => {
            row.fixDates();
        });
    }
}

export class ActivateScheduleControlRowDTO implements IActivateScheduleControlRowDTO {
    date: Date;
    isWholeDayAbsence: boolean;
    scheduleStart: Date;
    scheduleStop: Date;
    start: Date;
    stop: Date;
    timeScheduleTemplateBlockId: number;
    timeScheduleTemplateBlockType: number;
    type: TermGroup_ControlEmployeeSchedulePlacementType;

    public fixDates() {
        this.scheduleStart = CalendarUtility.convertToDate(this.scheduleStart);
        this.scheduleStop = CalendarUtility.convertToDate(this.scheduleStop);
        this.start = CalendarUtility.convertToDate(this.start);
        this.stop = CalendarUtility.convertToDate(this.stop);
    }
}

export class EmployeeSchedulePlacementGridViewDTO implements IEmployeeSchedulePlacementGridViewDTO {
    actorCompanyId: number;
    alwaysDiscardBreakEvaluation: boolean;
    autogenBreakOnStamping: boolean;
    autogenTimeblocks: boolean;
    breakDayMinutesAfterMidnight: number;
    employeeEndDate: Date;
    employeeFirstName: string;
    employeeGroupId: number;
    employeeGroupName: string;
    employeeGroupWorkTimeWeek: number;
    employeeId: number;
    employeeInfo: string;
    employeeLastName: string;
    employeeName: string;
    employeeNr: string;
    employeeNrSort: string;
    employeePosition: number;
    employeeScheduleId: number;
    employeeScheduleStartDate: Date;
    employeeScheduleStartDayNumber: number;
    employeeScheduleStopDate: Date;
    employeeWorkPercentage: number;
    employments: IEmploymentDTO[];
    isModified: boolean;
    isPersonalTemplate: boolean;
    isPlaced: boolean;
    isPreliminary: boolean;
    isSelected: boolean;
    isVisible: boolean;
    keepStampsTogetherWithinMinutes: number;
    mergeScheduleBreaksOnDay: boolean;
    templateEmployeeId: number;
    templateStartDate: Date;
    timeScheduleTemplateHeadId: number;
    timeScheduleTemplateHeadName: string;
    timeScheduleTemplateHeadNoOfDays: number;

    public fixDates() {
        this.employeeEndDate = CalendarUtility.convertToDate(this.employeeEndDate);
        this.employeeScheduleStartDate = CalendarUtility.convertToDate(this.employeeScheduleStartDate);
        this.employeeScheduleStopDate = CalendarUtility.convertToDate(this.employeeScheduleStopDate);
        this.templateStartDate = CalendarUtility.convertToDate(this.templateStartDate);
    }
}
