import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';

export class VoucherSeriesTypeDTO {
  voucherSeriesTypeId!: number;
  actorCompanyId: number;
  name: string;
  voucherSeriesTypeNr: number;
  startNr!: number;
  template!: boolean;
  yearEndSerie!: boolean;
  externalSerie!: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;

  constructor() {
    this.actorCompanyId = 0;
    this.name = '';
    this.voucherSeriesTypeNr = 0;
  }
}
