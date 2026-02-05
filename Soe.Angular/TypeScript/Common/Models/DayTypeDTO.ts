import { IDayTypeDTO, IEmployeeGroupDTO, ITimeHalfdayDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_SysDayType } from "../../Util/CommonEnumerations";


export class DayTypeDTO implements IDayTypeDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    dayTypeId: number;
    description: string;
    employeeGroups: IEmployeeGroupDTO[];
    import: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    standardWeekdayFrom: number;
    standardWeekdayTo: number;
    state: SoeEntityState;
    sysDayTypeId: number;
    timeHalfdays: ITimeHalfdayDTO[];
    type: TermGroup_SysDayType;
    weekendSalary: boolean;
}
