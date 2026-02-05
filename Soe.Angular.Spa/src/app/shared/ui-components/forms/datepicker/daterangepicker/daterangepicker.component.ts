import {
  Component,
  inject,
  input,
  output,
  OnInit,
  Injector,
  effect,
  signal,
  AfterViewInit,
  viewChild,
  computed,
} from '@angular/core';
import { ValueAccessorDirective } from '../../directives/value-accessor.directive';
import { ValidationHandler } from '@shared/handlers';
import { DaterangepickerModel } from './models/daterangepicker.model';
import { DaterangepickerForm } from './models/daterangepicker-form.model';
import {
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { DateRangeValidator } from '@shared/validators/daterange.validator';
import { DateUtil } from '@shared/util/date-util';

import {
  DatepickerComponent,
  DatepickerView,
} from '@ui/forms/datepicker/datepicker.component';
import { LabelComponent } from '@ui/label/label.component';
import { TranslatePipe } from '@ngx-translate/core';

export type DateRangeValue = [Date | undefined, Date | undefined];

@Component({
  selector: 'soe-daterangepicker',
  imports: [
    ReactiveFormsModule,
    DatepickerComponent,
    LabelComponent,
    TranslatePipe,
  ],
  templateUrl: './daterangepicker.component.html',
  styleUrls: ['./daterangepicker.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: DaterangepickerComponent,
      multi: true,
    },
  ],
})
export class DaterangepickerComponent
  extends ValueAccessorDirective<DateRangeValue>
  implements OnInit, AfterViewInit
{
  // Inputs - From
  inputIdFrom = input<string>(Math.random().toString(24));
  labelKeyFrom = input('common.datefrom');
  secondaryLabelKeyFrom = input('');
  secondaryLabelBoldFrom = input(false);
  secondaryLabelParanthesesFrom = input(true);
  secondaryLabelPrefixKeyFrom = input('');
  secondaryLabelPostfixKeyFrom = input('');
  lastInPeriodFrom = input(false); // When having period view (other than 'day') set date of last day. Default is first day.
  isRequiredFrom = signal(false);

  // Inputs - To
  inputIdTo = input<string>(Math.random().toString(24));
  labelKeyTo = input('common.dateto');
  secondaryLabelKeyTo = input('');
  secondaryLabelBoldTo = input(false);
  secondaryLabelParanthesesTo = input(true);
  secondaryLabelPrefixKeyTo = input('');
  secondaryLabelPostfixKeyTo = input('');
  lastInPeriodTo = input(true); // When having period view (other than 'day') set date of last day. Default is first day.
  isRequiredTo = signal(false);

  // Inputs - General
  inline = input(false);
  alignInline = input(false);
  width = input(0);
  view = input<DatepickerView>('day');
  hideToday = input(false);
  hideClear = input(false);
  showArrows = input(false);
  hideCalendarButton = input(false);
  description = input('');
  minDate = input<Date>();
  maxDate = input<Date>();
  gridMode = input(false);
  separatorDash = input(false);
  autoAdjustRange = input(false);
  suppressInitValues = input(false);

  initialDates = input<DateRangeValue>([undefined, undefined]);
  deltaDays = input(0);
  offsetDaysOnStep = input(0);
  manualDisabled = input(false);

  isDisabled = computed(() => {
    return (this.control && this.control.disabled) || this.manualDisabled();
  });

  valueChanged = output<DateRangeValue | undefined>();
  validityChanged = output<boolean>();
  previouslyValid = true;

  dateFromRef = viewChild<DatepickerComponent>('datePickerFrom');
  dateToRef = viewChild<DatepickerComponent>('datePickerTo');

  validationHandler = inject(ValidationHandler);
  form: DaterangepickerForm = new DaterangepickerForm({
    validationHandler: this.validationHandler,
    element: new DaterangepickerModel(),
  });

  constructor(injector: Injector) {
    super(injector);

    effect(() => {
      if (this.isRequiredFrom()) {
        this.form.controls.dateFrom.addValidators(Validators.required);
        this.dateFromRef()?.control.updateValueAndValidity();
      }
      if (this.isRequiredTo()) {
        this.form.controls.dateTo.addValidators(Validators.required);
        this.dateToRef()?.control.updateValueAndValidity();
      }
      this.form.updateValueAndValidity();
    });

    effect(() => {
      // Set initial dates from input (used from toolbar-daterangepicker)
      const initialDates = this.initialDates();
      if (
        initialDates &&
        initialDates.length === 2 &&
        (initialDates[0] || initialDates[1])
      ) {
        this.updateFromAndTo(initialDates[0], initialDates[1], false);
      }
    });

    effect(() => {
      const deltaDays = this.deltaDays();
      if (deltaDays > 0 && this.form.dateFrom.value) {
        this.form.dateTo.setValue(this.form.dateFrom.value.addDays(deltaDays));
        this.updateDateRange(false);
      }
    });
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.updateRequiredState();
    this.notifyValidityChange();

    this.form.daterange.statusChanges.subscribe(status => {
      if (status === 'VALID' || status === 'INVALID') {
        this.notifyValidityChange();
      }
    });

    this.setInitialValues();
  }

  setInitialValues() {
    // Set initial dates from form
    if (this.control.value && !this.suppressInitValues()) {
      this.form.dateFrom.setValue(this.control.value[0]);
      this.form.dateTo.setValue(this.control.value[1]);
      this.form.daterange.setValue(this.control.value);

      this.form.daterange.updateValueAndValidity();
      this.form.updateValueAndValidity();
    }
  }

  updateRequiredState() {
    this.isRequiredFrom.set(
      this.control && this.control.hasValidator(DateRangeValidator.requiredFrom)
    );
    this.isRequiredTo.set(
      this.control && this.control.hasValidator(DateRangeValidator.requiredTo)
    );
  }

  updateFrom(value: Date | undefined) {
    this.form.dateFrom.setValue(value);

    if (value) {
      if (this.deltaDays() > 0)
        this.form.dateTo.setValue(value.addDays(this.deltaDays()));
      else if (value > this.form.dateTo.value && this.autoAdjustRange())
        this.form.dateTo.setValue(value.addDays(this.deltaDays()));
    }

    this.updateDateRange();
  }

  updateTo(value: Date | undefined) {
    this.form.dateTo.setValue(value);

    if (value) {
      if (this.deltaDays() > 0)
        this.form.dateFrom.setValue(value.addDays(-this.deltaDays()));
      else if (value < this.form.dateFrom.value && this.autoAdjustRange())
        this.form.dateFrom.setValue(value.addDays(-this.deltaDays()));
    }

    this.updateDateRange();
  }

  updateFromAndTo(from: Date | undefined, to: Date | undefined, notify = true) {
    this.form.dateFrom.setValue(from);
    this.form.dateTo.setValue(to);

    this.updateDateRange(notify);
  }

  updateDateRange(notify = true) {
    this.control.setValue([this.form.dateFrom.value, this.form.dateTo.value]);
    this.form.daterange.setValue([
      this.form.dateFrom.value,
      this.form.dateTo.value,
    ]);

    this.form.daterange.updateValueAndValidity();
    this.form.updateValueAndValidity();

    if (notify) {
      this.valueChanged.emit([
        this.form.dateFrom.value !== '' ? this.form.dateFrom.value : undefined,
        this.form.dateTo.value !== '' ? this.form.dateTo.value : undefined,
      ]);
    }
  }

  stepRight() {
    const dateFrom = new Date(this.form.dateFrom.value);
    const dateTo = new Date(this.form.dateTo.value);

    let newDateFrom = new Date(dateFrom);
    let newDateTo = new Date(dateTo);

    switch (this.view()) {
      case 'day':
      case 'week':
        newDateFrom = new Date(dateTo).addDays(1 + this.offsetDaysOnStep());
        if (DateUtil.isFullMonth(dateFrom, dateTo)) {
          newDateTo = DateUtil.getDateLastInMonth(newDateFrom);
        } else {
          const daysBetween = DateUtil.diffDays(dateTo, dateFrom);
          newDateTo = new Date(newDateFrom).addDays(daysBetween);
        }
        break;
      case 'month':
        const monthsBetween = DateUtil.diffMonths(dateTo, dateFrom);
        newDateFrom = new Date(dateTo).addMonths(1);
        newDateTo = new Date(newDateFrom).addMonths(monthsBetween);
        break;
      case 'year':
        const yearsBetween = DateUtil.diffYears(dateTo, dateFrom);
        newDateFrom = new Date(dateTo).addYears(1);
        newDateTo = new Date(newDateFrom).addYears(yearsBetween);
        break;
    }
    this.form.dateFrom.setValue(newDateFrom);
    this.form.dateTo.setValue(newDateTo);
    this.updateDateRange();
  }

  stepLeft() {
    const dateFrom = new Date(this.form.dateFrom.value);
    const dateTo = new Date(this.form.dateTo.value);

    let newDateFrom = new Date(dateFrom);
    let newDateTo = new Date(dateTo);

    switch (this.view()) {
      case 'day':
      case 'week':
        newDateTo = new Date(dateFrom).addDays(-(1 + this.offsetDaysOnStep()));
        if (DateUtil.isFullMonth(dateFrom, dateTo)) {
          newDateFrom = DateUtil.getDateFirstInMonth(newDateTo);
        } else {
          const daysBetween = DateUtil.diffDays(dateTo, dateFrom);
          newDateFrom = new Date(newDateTo).addDays(0 - daysBetween);
        }
        break;
      case 'month':
        const monthsBetween = DateUtil.diffMonths(dateTo, dateFrom);
        newDateTo = new Date(dateFrom).addMonths(-1);
        newDateFrom = new Date(newDateTo).addMonths(0 - monthsBetween);
        break;
      case 'year':
        const yearsBetween = DateUtil.diffYears(dateTo, dateFrom);
        newDateTo = new Date(dateFrom).addYears(-1);
        newDateFrom = new Date(newDateTo).addYears(0 - yearsBetween);
        break;
    }
    this.form.dateFrom.setValue(newDateFrom);
    this.form.dateTo.setValue(newDateTo);
    this.updateDateRange();
  }

  notifyValidityChange() {
    if (this.previouslyValid !== this.form.daterange.valid) {
      this.previouslyValid = this.form.daterange.valid;
      this.validityChanged.emit(this.form.daterange.valid);
    }
  }
}
