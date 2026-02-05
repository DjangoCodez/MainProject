import { ITimeAbsenceRuleHeadDTO, ITimeAbsenceRuleRowDTO, ITimeAbsenceRuleRowPayrollProductsDTO, ITimeCodeDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_TimeAbsenceRuleType, TermGroup_TimeAbsenceRuleRowType, TermGroup_TimeAbsenceRuleRowScope } from "../../Util/CommonEnumerations";

export class TimeAbsenceRuleHeadDTO implements ITimeAbsenceRuleHeadDTO {
    actorCompanyId: number;
    companyName: string;
    created: Date;
    createdBy: string;
    description: string;
    employeeGroupIds: number[];
    employeeGroupNames: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    timeAbsenceRuleHeadId: number;
    timeAbsenceRuleRows: TimeAbsenceRuleRowDTO[];
    timeCode: ITimeCodeDTO;
    timeCodeId: number;
    timeCodeName: string;
    type: TermGroup_TimeAbsenceRuleType;
    typeName: string;
}

export class TimeAbsenceRuleRowDTO implements ITimeAbsenceRuleRowDTO {
    created: Date;
    createdBy: string;
    hasMultiplePayrollProducts: boolean;
    modified: Date;
    modifiedBy: string;
    payrollProductId: number;
    payrollProductName: string;
    payrollProductNr: string;
    payrollProductRows: TimeAbsenceRuleRowPayrollProductsDTO[];
    scope: TermGroup_TimeAbsenceRuleRowScope;
    start: number;
    state: SoeEntityState;
    stop: number;
    scopeName: string;
    timeAbsenceRuleHeadId: number;
    timeAbsenceRuleRowId: number;
    type: TermGroup_TimeAbsenceRuleRowType;
    typeName: string;
}

export class TimeAbsenceRuleRowPayrollProductsDTO implements ITimeAbsenceRuleRowPayrollProductsDTO {
    sourcePayrollProductId: number;
    sourcePayrollProductName: string;
    sourcePayrollProductNr: string;
    targetPayrollProductId: number;
    targetPayrollProductName: string;
    targetPayrollProductNr: string;
    timeAbsenceRuleRowPayrollProductsId: number;
}