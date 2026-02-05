import { Injectable, inject } from '@angular/core';
import { NativeDateAdapter } from '@angular/material/core';
import { DateUtil } from '@shared/util/date-util'
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { ViewFormat } from './mat-date-format.class';
import { TranslateService } from '@ngx-translate/core';

@Injectable()
export class CustomDateAdapter extends NativeDateAdapter {
  translate = inject(TranslateService);

  override getFirstDayOfWeek(): number {
    return SoeConfigUtil.firstDayOfWeek;
  }

  override format(date: Date, displayFormat: Object): string {
    if (displayFormat === ViewFormat.WeekYear) {
      const formatDate = new Date(date);
      formatDate.setFullYear(DateUtil.getWeekYear(date));
      return (
        this.translate.instant('common.week') +
        ' ' +
        DateUtil.getWeekNumber(formatDate) +
        ' ' +
        formatDate.getFullYear()
      );
    }
    return DateUtil.format(date, displayFormat.toString());
  }

  override getMonthNames(style: 'long' | 'short' | 'narrow'): string[] {
    return super.getMonthNames('long');
  }
}
