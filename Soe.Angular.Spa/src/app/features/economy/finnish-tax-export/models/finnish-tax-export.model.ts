import { IFinnishTaxExportDTO } from '@shared/models/generated-interfaces/FinishTaxExportDTO';

export class FinnishTaxExportDTO implements IFinnishTaxExportDTO {
  lengthOfTaxPeriod!: number;
  taxPeriod!: number;
  taxPeriodYear!: number;
  noActivity: boolean = false;
  correction: boolean = false;
  cause!: number;
}
