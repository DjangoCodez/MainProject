import { TermGroup_ReportExportType } from '../generated-interfaces/Enumerations';
import { IReportPrintDTO } from '../generated-interfaces/ReportPrintDTO';

export class ReportPrintDTO implements IReportPrintDTO {
  reportId!: number;
  ids: number[];
  queue: boolean;
  returnAsBinary!: boolean;
  exportType!: TermGroup_ReportExportType;
  constructor(ids: number[]) {
    this.ids = ids;
    this.queue = false;
  }
}
