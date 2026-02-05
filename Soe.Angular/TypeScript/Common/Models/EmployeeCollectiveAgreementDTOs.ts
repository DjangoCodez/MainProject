import { IEmployeeCollectiveAgreementDTO, IEmployeeCollectiveAgreementGridDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class EmployeeCollectiveAgreementDTO implements IEmployeeCollectiveAgreementDTO {
    actorCompanyId: number;
    annualLeaveGroupId: number;
    annualLeaveGroupName: string;
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    employeeCollectiveAgreementId: number;
    employeeGroupId: number;
    employeeGroupName: string;
    externalCode: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    payrollGroupId: number;
    payrollGroupName: string;
    state: SoeEntityState;
    vacationGroupId: number;
    vacationGroupName: string;
    
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }
}

export class EmployeeCollectiveAgreementGridDTO implements IEmployeeCollectiveAgreementGridDTO {
    code: string;
    description: string;
    employeeCollectiveAgreementId: number;
    employeeGroupName: string;
    employeeTemplateNames: string;
    externalCode: string;
    name: string;
    payrollGroupName: string;
    state: SoeEntityState;
    vacationGroupName: string;

    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }
}
