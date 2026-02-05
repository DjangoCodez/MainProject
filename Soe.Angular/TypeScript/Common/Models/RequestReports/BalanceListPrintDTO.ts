import { IBalanceListPrintDTO } from "../../../Scripts/TypeLite.Net4";
import { CompanySettingType } from "../../../Util/CommonEnumerations";
import { ReportPrintDTO } from "./ReportPrintDTO";

export class BalanceListPrintDTO
  extends ReportPrintDTO
  implements IBalanceListPrintDTO
{
  companySettingType: CompanySettingType;
  paymentRowIds: number[];
}
