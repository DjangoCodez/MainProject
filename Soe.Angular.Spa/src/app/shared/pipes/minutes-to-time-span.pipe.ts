import { Pipe, PipeTransform } from '@angular/core';
import { DateUtil } from '@shared/util/date-util';

@Pipe({
  name: 'minutesToTimeSpan',
  standalone: true,
})
export class MinutesToTimeSpanPipe implements PipeTransform {
  transform(
    value: number,
    showDays = false,
    showSeconds = false,
    padHours = false,
    maxOneDay = false
  ): string {
    return DateUtil.minutesToTimeSpan(
      value,
      showDays,
      showSeconds,
      padHours,
      maxOneDay
    );
  }
}
