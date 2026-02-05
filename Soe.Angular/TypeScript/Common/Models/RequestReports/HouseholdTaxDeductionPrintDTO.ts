import { IHouseholdTaxDeductionPrintDTO } from "../../../Scripts/TypeLite.Net4";
import { SoeReportTemplateType } from "../../../Util/CommonEnumerations";
import { ReportPrintDTO } from "./ReportPrintDTO";

export class HouseholdTaxDeductionPrintDTO
  extends ReportPrintDTO
  implements IHouseholdTaxDeductionPrintDTO
{
  sysReportTemplateTypeId: SoeReportTemplateType;
  sequenceNumber: number;
  useGreen: boolean;
}
