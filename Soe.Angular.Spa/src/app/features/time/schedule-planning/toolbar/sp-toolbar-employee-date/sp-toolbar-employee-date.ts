import { DatePipe, TitleCasePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'sp-toolbar-employee-date',
  imports: [DatePipe, TitleCasePipe],
  templateUrl: './sp-toolbar-employee-date.html',
  styleUrl: './sp-toolbar-employee-date.scss',
})
export class SpToolbarEmployeeDate {
  employeeName = input('');
  dateFrom = input<Date | undefined>(undefined);
  dateTo = input<Date | undefined>(undefined);
  showWeekdayColor = input(false);

  multipleDates = computed((): boolean => {
    return (
      this.dateFrom() !== undefined &&
      this.dateTo() !== undefined &&
      this.dateFrom()?.isSameDay(this.dateTo()!) === false
    );
  });

  isDateFromSaturday = computed((): boolean => {
    return this.showWeekdayColor() && (this.dateFrom()?.isSaturday() ?? false);
  });

  isDateFromSunday = computed((): boolean => {
    return this.showWeekdayColor() && (this.dateFrom()?.isSunday() ?? false);
  });

  isDateToSaturday = computed((): boolean => {
    return this.showWeekdayColor() && (this.dateTo()?.isSaturday() ?? false);
  });

  isDateToSunday = computed((): boolean => {
    return this.showWeekdayColor() && (this.dateTo()?.isSunday() ?? false);
  });
}
