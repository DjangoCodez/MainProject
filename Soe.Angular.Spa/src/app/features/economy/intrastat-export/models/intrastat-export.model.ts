import { IntrastatReportingType } from '@shared/models/generated-interfaces/Enumerations';

export class IntrastatExportGridHeaderDTO {
  fromDate: Date;
  endDate: Date;
  reportingType: IntrastatReportingType;

  constructor() {
    this.fromDate = new Date(
      new Date().getFullYear(),
      new Date().getMonth() - 1,
      1
    );
    this.endDate = new Date(new Date().getFullYear(), new Date().getMonth(), 0);
    this.reportingType = IntrastatReportingType.Both;
  }
}
