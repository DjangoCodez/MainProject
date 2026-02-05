
import { IProjectPrintDTO } from '../generated-interfaces/ReportPrintDTO';
import { ReportPrintDTO } from './report-print.model';

export class ProjectPrintDTO
  extends ReportPrintDTO
  implements IProjectPrintDTO
{
  reportId: number;
  sysReportTemplateTypeId: number;
  includeChildProjects: boolean;
  dateFrom?: Date;
  dateTo?: Date;

  constructor(
    ids: number[]
  ) {
    super(ids);
    this.reportId = 0;
    this.sysReportTemplateTypeId = 0;
    this.includeChildProjects = false;
    this.dateFrom = undefined;
    this.dateTo = undefined;
  }
}
