
import { SoeReportTemplateType } from '../generated-interfaces/Enumerations';
import { IHouseholdTaxDeductionPrintDTO } from '../generated-interfaces/ReportPrintDTO';
import { ReportPrintDTO } from './report-print.model';

export class HouseholdTaxDeductionPrintDTO
  extends ReportPrintDTO
  implements IHouseholdTaxDeductionPrintDTO
{
  sysReportTemplateTypeId: SoeReportTemplateType;
  sequenceNumber: number;
  useGreen: boolean;

  constructor(
    ids: number[]
  ) {
    super(ids);

    this.sysReportTemplateTypeId = SoeReportTemplateType.Unknown;
    this.sequenceNumber = 0;
    this.useGreen = false;
  }
}
