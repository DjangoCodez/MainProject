import { IPayrollLevelDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class PayrollLevelDTO implements IPayrollLevelDTO {

    actorCompanyId: number;
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    externalCode: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    nameAndDesc: string;
    payrollLevelId: number;
    state: SoeEntityState;

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }
}
