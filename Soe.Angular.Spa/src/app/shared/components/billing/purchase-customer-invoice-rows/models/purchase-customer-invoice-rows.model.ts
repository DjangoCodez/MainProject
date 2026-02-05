import {
  SoeEntityState,
  SoeModule,
  TermGroup_AttestEntity,
} from '@shared/models/generated-interfaces/Enumerations';
import { IAttestStateDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class PurchaseCustomerInvoiceRowsDTO {
  attestStateTo: number;

  constructor() {
    this.attestStateTo = 0;
  }
}

export class AttestStateDTO implements IAttestStateDTO {
  attestStateId!: number;
  actorCompanyId!: number;
  entity!: TermGroup_AttestEntity;
  module!: SoeModule;
  name!: string;
  description!: string;
  color!: string;
  imageSource!: string;
  sort!: number;
  initial!: boolean;
  closed!: boolean;
  hidden!: boolean;
  locked!: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;
  langId?: number;
  entityName!: string;
}
