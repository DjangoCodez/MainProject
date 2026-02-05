import {
  SoeEntityState,
  SoeInvoiceMatchingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IMatchCodeDTO,
  IMatchCodeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class MatchCodeGridDTO implements IMatchCodeGridDTO {
  matchCodeId!: number;
  type!: string;
  typeName!: string;
  name!: string;
  description!: string;
  accountNr!: string;
  vatAccountNr!: string;
  state!: SoeEntityState;
}

export class MatchCodeDTO implements IMatchCodeDTO {
  matchCodeId: number;
  actorCompanyId!: number;
  accountId: number;
  vatAccountId?: number;
  type!: SoeInvoiceMatchingType;
  typeId: number;
  typeName: string;
  name: string;
  description: string;
  accountNr: string;
  vatAccountNr: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;

  constructor() {
    this.matchCodeId = 0;
    this.accountId = 0;
    this.typeId = 0;
    this.typeName = '';
    this.name = '';
    this.description = '';
    this.accountNr = '';
    this.vatAccountNr = '';
  }
}
