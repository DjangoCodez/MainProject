import { IEmployeeTimePeriodDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEmployeeTimePeriodStatus } from "../../Util/CommonEnumerations";

export class EmployeeTimePeriodDTO implements IEmployeeTimePeriodDTO {
    employeeTimePeriodId: number;
    actorCompanyId: number;
    employeeId: number;
    timePeriodId: number;
    status: SoeEmployeeTimePeriodStatus;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    salarySpecificationPublishDate: Date;
}
