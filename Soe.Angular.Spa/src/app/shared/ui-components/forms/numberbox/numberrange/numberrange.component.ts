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
import { NumberRangeValidator } from '@shared/validators/numberrange.validator';
import { NumberrangeForm } from './models/numberrange-form.model';
import { NumberrangeModel } from './models/numberrange.model';

import { LabelComponent } from '@ui/label/label.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { TranslatePipe } from '@ngx-translate/core';

export type NumberRangeValue = [number | undefined, number | undefined];
export type ArrowsType = 'step' | 'range' | undefined;

@Component({
  selector: 'soe-numberrange',
  imports: [
    ReactiveFormsModule,
    LabelComponent,
    NumberboxComponent,
    TranslatePipe,
  ],

  templateUrl: './numberrange.component.html',
  styleUrls: ['./numberrange.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: NumberrangeComponent,
      multi: true,
    },
  ],
})
export class NumberrangeComponent
  extends ValueAccessorDirective<NumberRangeValue>
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
  hideClear = input(false);
  arrows = input<ArrowsType>(undefined);
  description = input('');
  minValue = input<Date>();
  maxValue = input<Date>();
  decimals = input(0);
  step = input(1);
  gridMode = input(false);
  separatorDash = input(false);
  autoAdjustRange = input(false);
  suppressInitValues = input(false);

  initialValues = input<NumberRangeValue>([undefined, undefined]);
  deltaValue = input(0);
  manualDisabled = input(false);

  isDisabled = computed(() => {
    return (this.control && this.control.disabled) || this.manualDisabled();
  });
  showStepArrows = computed(() => {
    return this.arrows() === 'step' && !this.isDisabled();
  });
  showRangeArrows = computed(() => {
    return this.arrows() === 'range' && !this.isDisabled();
  });

  valueChanged = output<NumberRangeValue | undefined>();
  validityChanged = output<boolean>();
  previouslyValid = true;

  numberFromRef = viewChild<NumberboxComponent>('numberFrom');
  numberToRef = viewChild<NumberboxComponent>('numberTo');

  validationHandler = inject(ValidationHandler);
  form: NumberrangeForm = new NumberrangeForm({
    validationHandler: this.validationHandler,
    element: new NumberrangeModel(),
  });

  constructor(injector: Injector) {
    super(injector);

    effect(() => {
      if (this.isRequiredFrom()) {
        this.form.controls.valueFrom.addValidators(Validators.required);
        this.numberFromRef()?.control.updateValueAndValidity();
      }
      if (this.isRequiredTo()) {
        this.form.controls.valueTo.addValidators(Validators.required);
        this.numberToRef()?.control.updateValueAndValidity();
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
        this.updateNumberRange(false);
      }
    });
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.updateRequiredState();
    this.notifyValidityChange();

    this.form.numberrange.statusChanges.subscribe(status => {
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
      this.form.numberrange.setValue(this.control.value);

      this.form.numberrange.updateValueAndValidity();
      this.form.updateValueAndValidity();
    }
  }

  updateRequiredState() {
    this.isRequiredFrom.set(
      this.control &&
        this.control.hasValidator(NumberRangeValidator.requiredFrom)
    );
    this.isRequiredTo.set(
      this.control && this.control.hasValidator(NumberRangeValidator.requiredTo)
    );
  }

  updateFrom(value: number | undefined) {
    this.form.valueFrom.setValue(value);

    if (value) {
      if (
        this.deltaValue() > 0 ||
        (value > this.form.valueTo.value && this.autoAdjustRange())
      )
        this.form.valueTo.setValue(value + this.deltaValue());
    }

    this.updateNumberRange();
  }

  updateTo(value: number | undefined) {
    this.form.valueTo.setValue(value);

    if (value) {
      if (
        this.deltaValue() > 0 ||
        (value < this.form.valueFrom.value && this.autoAdjustRange())
      )
        this.form.valueFrom.setValue(value - this.deltaValue());
    }

    this.updateNumberRange();
  }

  updateFromAndTo(
    from: number | undefined,
    to: number | undefined,
    notify = true
  ) {
    this.form.valueFrom.setValue(from);
    this.form.valueTo.setValue(to);

    this.updateNumberRange(notify);
  }

  updateNumberRange(notify = true) {
    this.control.setValue([this.form.valueFrom.value, this.form.valueTo.value]);
    this.form.numberrange.setValue([
      this.form.valueFrom.value,
      this.form.valueTo.value,
    ]);

    this.form.numberrange.updateValueAndValidity();
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
    const valueFrom = this.form.valueFrom.value;
    const valueTo = this.form.valueTo.value;

    const deltaValue = valueTo - valueFrom;
    const newValueFrom = valueTo + this.step();
    const newValueTo = newValueFrom + deltaValue;

    this.form.valueFrom.setValue(newValueFrom);
    this.form.valueTo.setValue(newValueTo);
    this.updateNumberRange();
  }

  stepLeft() {
    const valueFrom = this.form.valueFrom.value;
    const valueTo = this.form.valueTo.value;

    const deltaValue = valueTo - valueFrom;
    const newValueTo = valueFrom - this.step();
    const newValueFrom = newValueTo + (0 - deltaValue);

    this.form.valueFrom.setValue(newValueFrom);
    this.form.valueTo.setValue(newValueTo);
    this.updateNumberRange();
  }

  notifyValidityChange() {
    if (this.previouslyValid !== this.form.numberrange.valid) {
      this.previouslyValid = this.form.numberrange.valid;
      this.validityChanged.emit(this.form.numberrange.valid);
    }
  }
}
