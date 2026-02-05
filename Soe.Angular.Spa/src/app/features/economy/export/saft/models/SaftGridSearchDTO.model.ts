import { DateUtil } from '@shared/util/date-util';

export class SaftGridSearchFormDTO implements ISaftGridSearch {
  fromDate!: Date;
  toDate!: Date;

  constructor(_fromDate?: Date, _toDate?: Date) {
    const today = DateUtil.getToday();

    this.fromDate = _fromDate ?? DateUtil.getDateFirstInMonth(today);
    this.toDate = _toDate ?? DateUtil.getDateLastInMonth(today);
  }
}
export interface ISaftGridSearch {
  fromDate: Date;
  toDate: Date;
}
