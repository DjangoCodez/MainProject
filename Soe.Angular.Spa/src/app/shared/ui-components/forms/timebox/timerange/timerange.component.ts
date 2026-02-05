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
import {
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { LabelComponent } from '@ui/label/label.component';
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component';
import { TranslatePipe } from '@ngx-translate/core';
import { TimerangeForm } from './models/timerange-form.model';
import { TimerangeModel } from './models/timerange.model';
import { TimeRangeValidator } from '@shared/validators/timerange.validator';
import { differenceInMinutes } from 'date-fns';

export type TimeRangeSingleValue = string | Date | number | undefined;
export type TimeRangeValue = [TimeRangeSingleValue, TimeRangeSingleValue];
//export type ArrowsType = 'step' | 'range' | undefined;

@Component({
  selector: 'soe-timerange',
  imports: [
    ReactiveFormsModule,
    LabelComponent,
    TimeboxComponent,
    TranslatePipe,
  ],

  templateUrl: './timerange.component.html',
  styleUrls: ['./timerange.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: TimerangeComponent,
      multi: true,
    },
  ],
})
export class TimerangeComponent
  extends ValueAccessorDirective<TimeRangeValue>
  implements OnInit, AfterViewInit
{
  // Inputs - From
  inputIdFrom = input<string>(Math.random().toString(24));
  labelKeyFrom = input('common.from');
  secondaryLabelKeyFrom = input('');
  secondaryLabelBoldFrom = input(false);
  secondaryLabelParanthesesFrom = input(true);
  secondaryLabelPrefixKeyFrom = input('');
  secondaryLabelPostfixKeyFrom = input('');
  isRequiredFrom = signal(false);

  // Inputs - To
  inputIdTo = input<string>(Math.random().toString(24));
  labelKeyTo = input('common.to');
  secondaryLabelKeyTo = input('');
  secondaryLabelBoldTo = input(false);
  secondaryLabelParanthesesTo = input(true);
  secondaryLabelPrefixKeyTo = input('');
  secondaryLabelPostfixKeyTo = input('');
  isRequiredTo = signal(false);

  // Inputs - General
  inline = input(false);
  alignInline = input(false);
  width = input(0);
  showArrows = input(false);
  description = input('');
  isDuration = input(false);
  leadingZero = signal(false);
  allowNegative = signal(false);
  step = input(0); // Step in minutes for duration
  gridMode = input(false);
  separatorDash = input(false);
  autoAdjustRange = input(false);
  suppressInitValues = input(false);

  initialValues = input<TimeRangeValue>([undefined, undefined]);
  deltaValue = input(0);
  manualDisabled = input(false);

  isDisabled = computed(() => {
    return (this.control && this.control.disabled) || this.manualDisabled();
  });

  showRangeArrows = computed(() => {
    return this.showArrows() && this.isDuration() && !this.isDisabled();
  });

  valueChanged = output<TimeRangeValue | undefined>();
  validityChanged = output<boolean>();
  previouslyValid = true;

  timeFromRef = viewChild<TimeboxComponent>('timeFrom');
  timeToRef = viewChild<TimeboxComponent>('timeTo');

  validationHandler = inject(ValidationHandler);
  form: TimerangeForm = new TimerangeForm({
    validationHandler: this.validationHandler,
    element: new TimerangeModel(),
  });

  constructor(injector: Injector) {
    super(injector);

    effect(() => {
      if (this.isRequiredFrom()) {
        this.form.controls.valueFrom.addValidators(Validators.required);
        this.timeFromRef()?.control.updateValueAndValidity();
      }
      if (this.isRequiredTo()) {
        this.form.controls.valueTo.addValidators(Validators.required);
        this.timeToRef()?.control.updateValueAndValidity();
      }
      this.form.updateValueAndValidity();
    });

    effect(() => {
      // Set initial values from input
      const initialValues = this.initialValues();
      if (
        initialValues &&
        initialValues.length === 2 &&
        (initialValues[0] || initialValues[1])
      ) {
        this.updateFromAndTo(initialValues[0], initialValues[1], false);
      }
    });

    effect(() => {
      const deltaValue = this.deltaValue();
      if (deltaValue > 0 && this.form.valueFrom.value) {
        this.form.valueTo.setValue(this.form.valueFrom.value + deltaValue);
        this.updateTimeRange(false);
      }
    });
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.updateRequiredState();
    this.notifyValidityChange();

    this.form.timerange.statusChanges.subscribe(status => {
      if (status === 'VALID' || status === 'INVALID') {
        this.notifyValidityChange();
      }
    });
    this.setInitialValues();
  }

  setInitialValues() {
    // Set initial values from form
    if (this.control.value && !this.suppressInitValues()) {
      this.form.valueFrom.setValue(this.control.value[0]);
      this.form.valueTo.setValue(this.control.value[1]);
      this.form.timerange.setValue(this.control.value);

      this.form.timerange.updateValueAndValidity();
      this.form.updateValueAndValidity();
    }
    setTimeout(() => {
      this.updateFromAndTo(
        this.form.valueFrom.value,
        this.form.valueTo.value,
        false
      );
    }, 0);
  }

  rangeToMinutes(value: string): number {
    if (!value) return 0;

    const parts = value.split(':');
    if (parts.length !== 2) return 0;

    const hours = parseInt(parts[0], 10);
    const minutes = parseInt(parts[1], 10);

    return hours * 60 + minutes;
  }

  minutesToRange(value: number): string {
    if (value === undefined || value === null) return '';
    const hours = Math.floor(value / 60);
    const minutes = value % 60;

    return `${hours}:${minutes < 10 ? '0' : ''}${minutes}`;
  }

  updateRequiredState() {
    this.isRequiredFrom.set(
      this.control && this.control.hasValidator(TimeRangeValidator.requiredFrom)
    );
    this.isRequiredTo.set(
      this.control && this.control.hasValidator(TimeRangeValidator.requiredTo)
    );
  }

  updateFrom(value: TimeRangeSingleValue) {
    if (typeof value === 'string') {
      const valueMinutes = this.rangeToMinutes(value.toString());
      this.form.valueFrom.setValue(valueMinutes);

      // If timerange then we can calculate the new valueTo based on the deltaValue
      if (value && this.isDuration()) {
        const existingValueTo =
          typeof this.form.valueTo.value === 'string'
            ? this.rangeToMinutes(this.form.valueTo.value.toString())
            : this.form.valueTo.value;
        const newValueTo = valueMinutes + this.deltaValue();

        if (
          this.deltaValue() > 0 ||
          (valueMinutes > existingValueTo && this.autoAdjustRange())
        ) {
          this.form.valueTo.setValue(newValueTo);
        }
      }
    } else if (value instanceof Date) {
      this.form.valueFrom.setValue(value);

      if (this.autoAdjustRange()) {
        const existingValueTo = this.form.valueTo.value;
        const diff = differenceInMinutes(
          this.form.valueFrom.value,
          existingValueTo
        );

        if (diff > 0) {
          this.form.valueTo.setValue(new Date(this.form.valueFrom.value));
        }
      }
    }
    this.updateTimeRange();
  }

  updateTo(value: TimeRangeSingleValue) {
    if (typeof value === 'string') {
      const valueMinutes = this.rangeToMinutes(value.toString());
      this.form.valueTo.setValue(valueMinutes);

      // If timerange then we can calculate the new valueFrom based on the deltaValue
      if (value && this.isDuration()) {
        const existingValueFrom =
          typeof this.form.valueFrom.value === 'string'
            ? this.rangeToMinutes(this.form.valueFrom.value.toString())
            : this.form.valueFrom.value;
        const newValueFrom = valueMinutes - this.deltaValue();

        if (
          this.deltaValue() > 0 ||
          (valueMinutes < existingValueFrom && this.autoAdjustRange())
        )
          this.form.valueFrom.setValue(newValueFrom);
      }
    } else if (value instanceof Date) {
      this.form.valueTo.setValue(value);

      if (this.autoAdjustRange()) {
        const existingValueFrom = this.form.valueFrom.value;
        const diff = differenceInMinutes(
          existingValueFrom,
          this.form.valueTo.value
        );

        if (diff > 0) {
          this.form.valueFrom.setValue(new Date(this.form.valueTo.value));
        }
      }
    }
    this.updateTimeRange();
  }

  updateFromAndTo(
    from: TimeRangeSingleValue,
    to: TimeRangeSingleValue,
    notify = true
  ) {
    if (typeof from === 'string') from = this.rangeToMinutes(from.toString());
    if (typeof to === 'string') to = this.rangeToMinutes(to.toString());

    this.form.valueFrom.setValue(from);
    this.form.valueTo.setValue(to);

    this.updateTimeRange(notify);
  }

  updateTimeRange(notify = true) {
    if (!this.isDuration() && this.form.valueFrom.value === 0) {
      this.form.valueFrom.setValue('');
    }
    if (!this.isDuration() && this.form.valueTo.value === 0) {
      this.form.valueTo.setValue('');
    }

    this.control.setValue([this.form.valueFrom.value, this.form.valueTo.value]);
    this.form.timerange.setValue([
      this.form.valueFrom.value,
      this.form.valueTo.value,
    ]);
    this.form.timerange.updateValueAndValidity();
    this.form.updateValueAndValidity();

    if (notify) {
      this.valueChanged.emit([
        this.form.valueFrom.value !== ''
          ? this.form.valueFrom.value
          : undefined,
        this.form.valueTo.value !== '' ? this.form.valueTo.value : undefined,
      ]);
    }
  }

  stepRight() {
    if (!this.isDuration()) return;

    const valueFrom =
      typeof this.form.valueFrom.value === 'string'
        ? this.rangeToMinutes(this.form.valueFrom.value)
        : this.form.valueFrom.value;
    const valueTo =
      typeof this.form.valueTo.value === 'string'
        ? this.rangeToMinutes(this.form.valueTo.value)
        : this.form.valueTo.value;

    const deltaValue = valueTo - valueFrom;
    const newValueFrom = valueTo + this.step();
    const newValueTo = newValueFrom + deltaValue;

    this.form.valueFrom.setValue(newValueFrom);
    this.form.valueTo.setValue(newValueTo);

    this.updateTimeRange();
  }

  stepLeft() {
    if (!this.isDuration()) return;

    const valueFrom =
      typeof this.form.valueFrom.value === 'string'
        ? this.rangeToMinutes(this.form.valueFrom.value)
        : this.form.valueFrom.value;
    const valueTo =
      typeof this.form.valueTo.value === 'string'
        ? this.rangeToMinutes(this.form.valueTo.value)
        : this.form.valueTo.value;

    const deltaValue = valueTo - valueFrom;
    const newValueTo = valueFrom - this.step();
    const newValueFrom = newValueTo + (0 - deltaValue);

    this.form.valueFrom.setValue(newValueFrom);
    this.form.valueTo.setValue(newValueTo);

    this.updateTimeRange();
  }

  notifyValidityChange() {
    if (this.previouslyValid !== this.form.timerange.valid) {
      this.previouslyValid = this.form.timerange.valid;
      this.validityChanged.emit(this.form.timerange.valid);
    }
  }
}
