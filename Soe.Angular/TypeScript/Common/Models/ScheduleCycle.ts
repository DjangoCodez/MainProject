import { IScheduleCycleRuleTypeDTO, IScheduleCycleRuleDTO, IScheduleCycleDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";


export class ScheduleCycleRuleTypeDTO implements IScheduleCycleRuleTypeDTO {
    accountId: number;
    accountName: string;
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    dayOfWeekIds: number[];
    dayOfWeeks: string;
    dayOfWeeksGridString: string;
    hours: number;
    isEvening: boolean;
    isWeekEndOnly: boolean;
    lenght: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    scheduleCycleRuleTypeId: number;
    startTime: Date;
    state: SoeEntityState;
    stopTime: Date;
}

export class ScheduleCycleRuleDTO implements IScheduleCycleRuleDTO {
    created: Date;
    createdBy: string;
    maxOccurrences: number;
    minOccurrences: number;
    modified: Date;
    modifiedBy: string;
    scheduleCycleId: number;
    scheduleCycleRuleId: number;
    scheduleCycleRuleTypeDTO: ScheduleCycleRuleTypeDTO;
    scheduleCycleRuleTypeId: number;
    state: SoeEntityState;

    get selectedScheduleCycleRuleType() {
        return this.scheduleCycleRuleTypeDTO;
    }
    set selectedScheduleCycleRuleType(item: ScheduleCycleRuleTypeDTO) {
        this.scheduleCycleRuleTypeDTO = item;
        this.scheduleCycleRuleTypeId = item ? item.scheduleCycleRuleTypeId : 0;
    }

}

export class ScheduleCycleDTO implements IScheduleCycleDTO {
    accountId: number;
    accountName: string;
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    nbrOfWeeks: number;
    scheduleCycleId: number;
    scheduleCycleRuleDTOs: ScheduleCycleRuleDTO[];
    state: SoeEntityState;
}

