import {
  SoeEntityState,
  TermGroup_StaffingNeedsRuleUnit,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IStaffingNeedsLocationDTO,
  IStaffingNeedsLocationGridDTO,
  IStaffingNeedsLocationGroupDTO,
  IStaffingNeedsRuleDTO,
  IStaffingNeedsRuleRowDTO,
  ITimeScheduleTaskDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class StaffingNeedsLocationGroupDTO
  implements IStaffingNeedsLocationGroupDTO
{
  accountId?: number;
  accountName: string;
  actorCompanyId: number;
  created?: Date;
  createdBy: string;
  description: string;
  modified?: Date;
  modifiedBy: string;
  name: string;
  selectedShiftTypeNames: string;
  shiftTypeIds: number[];
  staffingNeedsLocationGroupId: number;
  staffingNeedsLocations: IStaffingNeedsLocationDTO[];
  state: SoeEntityState;
  timeScheduleTask!: ITimeScheduleTaskDTO;
  timeScheduleTaskId: number;
  timeScheduleTaskName: string;

  constructor() {
    this.staffingNeedsLocationGroupId = 0;
    this.actorCompanyId = 0;
    this.timeScheduleTaskId = 0;
    this.name = '';
    this.description = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.accountName = '';
    this.selectedShiftTypeNames = '';
    this.shiftTypeIds = [];
    this.staffingNeedsLocations = [];
    this.timeScheduleTaskName = '';
  }
}

export class StaffingNeedsLocationDTO implements IStaffingNeedsLocationDTO {
  staffingNeedsLocationId: number;
  staffingNeedsLocationGroupId: number;
  name: string;
  description: string;
  externalCode: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;

  constructor() {
    this.staffingNeedsLocationId = 0;
    this.staffingNeedsLocationGroupId = 0;
    this.name = '';
    this.description = '';
    this.externalCode = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
  }
}

export class StaffingNeedsLocationGridDTO
  implements IStaffingNeedsLocationGridDTO
{
  staffingNeedsLocationId: number;
  name: string;
  description: string;
  externalCode: string;
  groupId: number;
  groupName: string;
  groupAccountId: number;
  groupAccountName: string;

  constructor() {
    this.staffingNeedsLocationId = 0;
    this.name = '';
    this.description = '';
    this.externalCode = '';
    this.groupId = 0;
    this.groupName = '';
    this.groupAccountId = 0;
    this.groupAccountName = '';
  }
}

export class StaffingNeedsRuleDTO implements IStaffingNeedsRuleDTO {
  staffingNeedsRuleId: number;
  staffingNeedsLocationGroupId: number;
  name: string;
  unit: TermGroup_StaffingNeedsRuleUnit;
  maxQuantity: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  accountId?: number;
  rows!: IStaffingNeedsRuleRowDTO[];

  constructor() {
    this.staffingNeedsRuleId = 0;
    this.staffingNeedsLocationGroupId = 0;
    this.name = '';
    this.unit = TermGroup_StaffingNeedsRuleUnit.Unknown;
    this.maxQuantity = 0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
  }
}
