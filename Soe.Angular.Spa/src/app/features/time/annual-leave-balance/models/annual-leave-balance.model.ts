import { ISearchAnnualLeaveTransactionModel } from '@shared/models/generated-interfaces/TimeModels';

export class SearchAnnualLeaveTransactionModel
  implements ISearchAnnualLeaveTransactionModel
{
  employeeIds: number[];
  dateFrom: Date;
  dateTo: Date;

  constructor() {
    this.employeeIds = [];
    this.dateFrom = new Date();
    this.dateTo = new Date();
  }
}
