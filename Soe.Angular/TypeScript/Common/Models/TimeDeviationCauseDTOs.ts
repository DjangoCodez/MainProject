import { ITimeCodeDTO, ITimeDeviationCauseDTO, ITimeDeviationCauseGridDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_TimeDeviationCauseType } from "../../Util/CommonEnumerations";

export class TimeDeviationCauseDTO implements ITimeDeviationCauseDTO {
    adjustTimeInsideOfPlannedAbsence: number;
    adjustTimeOutsideOfPlannedAbsence: number;
    allowGapToPlannedAbsence: boolean;
    calculateAsOtherTimeInSales: boolean;
    candidateForOvertime: boolean;
    changeCauseInsideOfPlannedAbsence: number;
    changeCauseOutsideOfPlannedAbsence: number;
    actorCompanyId: number;
    attachZeroDaysNbrOfDaysAfter: number;
    attachZeroDaysNbrOfDaysBefore: number;
    changeDeviationCauseAccordingToPlannedAbsence: boolean;
    created: Date;
    createdBy: string;
    description: string;
    employeeGroupIds: number[];
    employeeRequestPolicyNbrOfDaysBefore: number;
    employeeRequestPolicyNbrOfDaysBeforeCanOverride: boolean;
    excludeFromPresenceWorkRules: boolean;
    excludeFromScheduleWorkRules: boolean;
    extCode: string;
    externalCodes: string[];
    imageSource: string;
    isAbsence: boolean;
    isPresence: boolean;
    isVacation: boolean;
    mandatoryNote: boolean;
    mandatoryTime: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    notChargeable: boolean;
    onlyWholeDay: boolean;
    payed: boolean;
    showZeroDaysInAbsencePlanning: boolean;
    specifyChild: boolean;
    state: SoeEntityState;
    timeCode: ITimeCodeDTO;
    timeCodeId: number;
    timeCodeName: string;
    timeDeviationCauseId: number;
    type: TermGroup_TimeDeviationCauseType;
    typeName: string;
    validForHibernating: boolean;
    validForStandby: boolean;
}

export class TimeDeviationCauseGridDTO implements ITimeDeviationCauseGridDTO {
    candidateForOvertime: boolean;
    description: string;
    imageSource: string;
    name: string;
    specifyChild: boolean;
    timeCodeName: string;
    timeDeviationCauseId: number;
    type: TermGroup_TimeDeviationCauseType;
    typeName: string;
    validForHibernating: boolean;
    validForStandby: boolean;
    mandatoryNote: boolean;

    public get icon(): string {
        return this.imageSource ? 'fal fa-' + this.imageSource : '';
    }
}