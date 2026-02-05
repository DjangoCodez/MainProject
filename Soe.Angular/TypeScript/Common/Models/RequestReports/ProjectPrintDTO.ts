import { IProjectPrintDTO } from "../../../Scripts/TypeLite.Net4";
import { ReportPrintDTO } from "./ReportPrintDTO";

export class ProjectPrintDTO
  extends ReportPrintDTO
  implements IProjectPrintDTO
{
  reportId: number;
  sysReportTemplateTypeId: number;
  includeChildProjects: boolean;
  dateFrom: Date;
  dateTo: Date;
}
