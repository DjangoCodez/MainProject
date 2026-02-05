import { Component, inject, OnInit, signal } from '@angular/core';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DatespickerComponent } from '@ui/forms/datepicker/datespicker/datespicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogFooterComponent } from '@ui/footer/dialog-footer/dialog-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import {
  DailyRecurrencePatternDialogData,
  DailyRecurrencePatternForm,
  DailyRecurrenceRangeForm,
} from './models/daily-recurrence-pattern-dialog-form.model';
import { ValidationHandler } from '@shared/handlers';
import {
  DailyRecurrencePatternDTO,
  DailyRecurrenceRangeDTO,
} from '@shared/models/recurrence.model';
import { MatDialogRef } from '@angular/material/dialog';
import { ReactiveFormsModule } from '@angular/forms';
import { DailyRecurrencePatternDialogService } from './services/daily-recurrence-pattern-dialog.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { DateUtil } from '@shared/util/date-util'
import { clearAndSetFormArray } from '@shared/util/form-util';
import { MultiSelectGridModule } from '../multi-select-grid';
import {
  DailyRecurrencePatternType,
  DailyRecurrenceRangeType,
} from '@shared/models/generated-interfaces/Enumerations';
import { DayOfWeek } from '@shared/util/Enumerations';

export type DailyRecurrencePatternDialogResult = {
  pattern: DailyRecurrencePatternDTO;
  range: DailyRecurrenceRangeDTO;
  excludedDates: Date[];
};

@Component({
  selector: 'soe-daily-recurrence-pattern-dialog',
  templateUrl: './daily-recurrence-pattern-dialog.component.html',
  styleUrl: './daily-recurrence-pattern-dialog.component.scss',
  imports: [
    DialogComponent,
    ReactiveFormsModule,
    TranslateModule,
    CheckboxComponent,
    DatepickerComponent,
    DatespickerComponent,
    ExpansionPanelComponent,
    DialogFooterComponent,
    InstructionComponent,
    LabelComponent,
    MultiSelectGridModule,
    NumberboxComponent,
    SelectComponent,
  ],
})
export class DailyRecurrencePatternDialogComponent
  extends DialogComponent<DailyRecurrencePatternDialogData>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);
  patternForm: DailyRecurrencePatternForm = new DailyRecurrencePatternForm({
    validationHandler: this.validationHandler,
    element: new DailyRecurrencePatternDTO(),
  });
  rangeForm: DailyRecurrenceRangeForm = new DailyRecurrenceRangeForm({
    validationHandler: this.validationHandler,
    element: new DailyRecurrenceRangeDTO(),
  });
  dialogRef = inject(MatDialogRef);
  service = inject(DailyRecurrencePatternDialogService);
  translationService = inject(TranslateService);

  dayOfWeeks: SmallGenericType[] = [];
  months: SmallGenericType[] = [];
  holidayTypes: SmallGenericType[] = [];

  excludedDates = signal<Date[]>([]);

  date!: Date;
  isNew = false;

  suggestionInfo = signal('');

  ngOnInit(): void {
    if (!this.service.performTypes.data) this.service.getTypes().subscribe();
    if (!this.service.performWeekendIndexes.data)
      this.service.getWeekendIndexes().subscribe();
    if (!this.service.performRangeTypes.data)
      this.service.getRangeTypes().subscribe();

    if (!this.service.performHolidayTypes.data)
      this.service.performHolidayTypes.load(this.service.getHolidayTypes());

    // TODO: Above must be loaded before continuing

    this.setupDayOfWeeks();
    this.setupMonths();

    this.date = this.data.date ?? DateUtil.getToday();
    this.suggestionInfo.set(
      this.translationService
        .instant('common.dailyrecurrencepattern.suggestinfo')
        .format(this.date.toLocaleDateString())
    );

    if (this.data.pattern) {
      this.patternForm.customPatchValue(this.data.pattern);
      this.setSelectedDaysOfWeek();
    } else {
      this.isNew = true;
      this.patternForm.patchValue(new DailyRecurrencePatternDTO());
      this.onTypeChanged();
    }

    if (this.data.range) {
      this.rangeForm.patchValue(this.data.range);
    } else {
      this.rangeForm.patchValue(new DailyRecurrenceRangeDTO());
      this.rangeForm.patchValue({
        type: DailyRecurrenceRangeType.NoEnd,
        startDate: this.data.date,
      });
      this.onRangeTypeChanged();
    }

    if (this.data.excludedDates)
      this.excludedDates.set(this.data.excludedDates);
  }

  private setupDayOfWeeks() {
    this.dayOfWeeks = DateUtil.getDayOfWeekNames(true, undefined, true);
  }

  private setupMonths() {
    this.months = DateUtil.getMonthNames(true);
  }

  getDayName(day: number): string {
    return DateUtil.getDayOfWeekName(day, true);
  }

  getMonthName(month: number): string {
    return DateUtil.getMonthName(month - 1, true);
  }

  // HELP-METHODS

  private clearPattern() {
    this.patternForm.patchValue({
      interval: 0,
      dayOfMonth: 0,
      month: 0,
      daysOfWeek: [],
      firstDayOfWeek: undefined,
      weekIndex: 0,
      sysHolidayTypeIds: [],
    });
  }

  private setDefaultInterval() {
    this.patternForm.patchValue({ interval: 1 });
  }

  private setDefaultDayOfMonth() {
    this.patternForm.patchValue({ dayOfMonth: this.date.getDate() });
  }

  private setDefaultMonth() {
    this.patternForm.patchValue({ month: this.date.getMonth() + 1 });
  }

  private setDefaultFirstDayOfWeek() {
    this.patternForm.patchValue({ firstDayOfWeek: this.date.dayOfWeek() });
  }

  private setDefaultWeekIndex() {
    const dayOfMonth = this.date.getDate();
    const week = Math.floor(dayOfMonth / 7);
    this.patternForm.patchValue({ weekIndex: week });
  }

  private setSelectedDaysOfWeek() {
    if (!this.patternForm.value.daysOfWeek)
      this.patternForm.patchValue({ daysOfWeek: [] });

    this.patternForm.value.daysOfWeek.forEach((day: DayOfWeek) => {
      if (day === DayOfWeek.Monday)
        this.patternForm.patchValue({ mondaySelected: true });
      if (day === DayOfWeek.Tuesday)
        this.patternForm.patchValue({ tuesdaySelected: true });
      if (day === DayOfWeek.Wednesday)
        this.patternForm.patchValue({ wednesdaySelected: true });
      if (day === DayOfWeek.Thursday)
        this.patternForm.patchValue({ thursdaySelected: true });
      if (day === DayOfWeek.Friday)
        this.patternForm.patchValue({ fridaySelected: true });
      if (day === DayOfWeek.Saturday)
        this.patternForm.patchValue({ saturdaySelected: true });
      if (day === DayOfWeek.Sunday)
        this.patternForm.patchValue({ sundaySelected: true });
    });
  }

  // EVENTS

  onTypeChanged() {
    // Set suggestions based on specified date
    this.clearPattern();
    switch (this.patternForm.value.type) {
      case DailyRecurrencePatternType.None:
        break;
      case DailyRecurrencePatternType.Daily:
        this.setDefaultInterval();
        break;
      case DailyRecurrencePatternType.Weekly:
      case DailyRecurrencePatternType.AbsoluteMonthly:
        this.setDefaultInterval();
        this.setDefaultDayOfMonth();
        break;
      case DailyRecurrencePatternType.RelativeMonthly:
        this.setDefaultInterval();
        this.setDefaultWeekIndex();
        this.setDefaultFirstDayOfWeek();
        break;
      case DailyRecurrencePatternType.AbsoluteYearly:
        this.setDefaultDayOfMonth();
        this.setDefaultMonth();
        break;
      case DailyRecurrencePatternType.RelativeYearly:
        this.setDefaultWeekIndex();
        this.setDefaultFirstDayOfWeek();
        this.setDefaultMonth();
        break;
      case DailyRecurrencePatternType.SysHoliday:
        this.setDefaultInterval();
        break;
    }
  }

  onRangeTypeChanged() {
    switch (this.rangeForm.value.type) {
      case DailyRecurrenceRangeType.NoEnd:
        this.rangeForm.patchValue({
          endDate: undefined,
          numberOfOccurrences: 0,
        });
        break;
      case DailyRecurrenceRangeType.EndDate:
        this.rangeForm.patchValue({
          endDate: this.date,
          numberOfOccurrences: 0,
        });
        break;
      case DailyRecurrenceRangeType.Numbered:
        this.rangeForm.patchValue({
          endDate: undefined,
          numberOfOccurrences: 1,
        });
        break;
    }
  }

  onHolidayTypesChanged(selectedIds: number[]) {
    clearAndSetFormArray(selectedIds, this.patternForm.sysHolidayTypeIds, true);
  }

  cancel() {
    this.dialogRef.close();
  }

  ok() {
    const result: DailyRecurrencePatternDialogResult = {
      pattern: this.patternForm.value,
      range: this.rangeForm.value,
      excludedDates: this.excludedDates(),
    };
    this.dialogRef.close(result);
  }
}
