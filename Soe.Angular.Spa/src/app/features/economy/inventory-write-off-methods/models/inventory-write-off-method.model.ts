import {
  TermGroup_InventoryWriteOffMethodType,
  TermGroup_InventoryWriteOffMethodPeriodType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { IInventoryWriteOffMethodDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class InventoryWriteOffMethodDTO implements IInventoryWriteOffMethodDTO {
  inventoryWriteOffMethodId: number;
  actorCompanyId: number;
  name: string;
  description: string;
  type!: TermGroup_InventoryWriteOffMethodType;
  periodType!: TermGroup_InventoryWriteOffMethodPeriodType;
  periodValue: number;
  yearPercent: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state!: SoeEntityState;
  typeName!: string;
  periodTypeName!: string;
  hasAcitveWirteOffs: boolean;

  constructor() {
    this.inventoryWriteOffMethodId = 0;
    this.actorCompanyId = 0;
    this.name = '';
    this.description = '';
    this.periodValue = 0;
    this.yearPercent = 0.0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.hasAcitveWirteOffs = false;
  }
}
