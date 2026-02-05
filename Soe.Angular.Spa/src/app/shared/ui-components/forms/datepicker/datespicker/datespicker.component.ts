import { Component, inject, input, model, OnInit } from '@angular/core';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { DatespickerForm } from './models/datespicker-form.model';
import { DatespickerModel } from './models/datespicker.model';
import { ValidationHandler } from '@shared/handlers';
import { DateUtil } from '@shared/util/date-util';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { IconModule } from '@ui/icon/icon.module';
import { SortDatesPipe } from '@shared/pipes';

@Component({
  selector: 'soe-datespicker',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DatepickerComponent,
    IconModule,
    SortDatesPipe,
  ],
  templateUrl: './datespicker.component.html',
})
export class DatespickerComponent
  extends ValueAccessorDirective<Date>
  implements OnInit
{
  labelKey = input('');
  dates = model<Date[]>();
  sortByDesc = input(false);

  validationHandler = inject(ValidationHandler);
  form: DatespickerForm = new DatespickerForm({
    validationHandler: this.validationHandler,
    element: new DatespickerModel(),
  });

  ngOnInit(): void {
    super.ngOnInit();

    // Set initial dates in form
    if (this.dates()) this.form.patchDates(this.dates());
  }

  addDate(date: Date | undefined) {
    if (date) {
      const existingDates: Date[] = this.form.dates.value;

      if (!DateUtil.includesDate(existingDates, date)) {
        existingDates.push(date);
        this.patchDates(existingDates);
      }

      // Clear selected date from date picker
      this.form.patchValue({ date: undefined });
    }
  }

  removeDate(date: Date) {
    const existingDates: Date[] = this.form.dates.value;

    const index = existingDates.indexOf(date, 0);
    if (index > -1) {
      existingDates.splice(index, 1);
      this.patchDates(existingDates);
    }
  }

  private patchDates(dates: Date[]) {
    // Patch dates to form and emit changes to parent compoent
    this.form.patchDates(dates);
    this.dates.set(this.form.dates.value);
  }
}
