import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';

export class VatCodeDTO {
  accountId?: number;
  accountNr?: string;
  code: string;
  created?: Date;
  createdBy?: string;
  modified?: Date;
  modifiedBy?: string;
  name: string;
  percent: number;
  purchaseVATAccountId?: number;
  purchaseVATAccountNr?: string;
  state?: SoeEntityState;
  vatCodeId: number;

  constructor() {
    this.vatCodeId = 0;
    this.code = '';
    this.name = '';
    this.percent = 0;
  }
}
