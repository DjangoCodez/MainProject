import {
  Component,
  effect,
  inject,
  Injector,
  input,
  output,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DatepickerView } from '@ui/forms/datepicker/datepicker.component';
import {
  DaterangepickerComponent,
  DateRangeValue,
} from '@ui/forms/datepicker/daterangepicker/daterangepicker.component';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { ValidationHandler } from '@shared/handlers';
import { DaterangepickerModel } from '@ui/forms/datepicker/daterangepicker/models/daterangepicker.model';
import { DaterangepickerForm } from '@ui/forms/datepicker/daterangepicker/models/daterangepicker-form.model';

export type ToolbarDaterangepickerAction =
  | { key: string; value: DateRangeValue | undefined }
  | undefined;

@Component({
  selector: 'soe-toolbar-daterangepicker',
  imports: [DaterangepickerComponent, FormsModule, ReactiveFormsModule],
  templateUrl: './toolbar-daterangepicker.component.html',
  styleUrl: './toolbar-daterangepicker.component.scss',
})
export class ToolbarDaterangepickerComponent extends ValueAccessorDirective<DateRangeValue> {
  // Inputs - From
  inputIdFrom = input<string>(Math.random().toString(24));
  labelKeyFrom = input('');
  secondaryLabelKeyFrom = input('');
  secondaryLabelBoldFrom = input(false);
  secondaryLabelParanthesesFrom = input(true);
  secondaryLabelPrefixKeyFrom = input('');
  secondaryLabelPostfixKeyFrom = input('');
  lastInPeriodFrom = input(false); // When having period view (other than 'day') set date of last day. Default is first day.

  // Inputs - To
  inputIdTo = input<string>(Math.random().toString(24));
  labelKeyTo = input('');
  secondaryLabelKeyTo = input('');
  secondaryLabelBoldTo = input(false);
  secondaryLabelParanthesesTo = input(true);
  secondaryLabelPrefixKeyTo = input('');
  secondaryLabelPostfixKeyTo = input('');
  lastInPeriodTo = input(true); // When having period view (other than 'day') set date of last day. Default is first day.

  // Inputs - General
  key = input('');
  width = input(0);
  view = input<DatepickerView>('day');
  hideToday = input(false);
  hideClear = input(false);
  showArrows = input(false);
  hideCalendarButton = input(false);
  description = input('');
  minDate = input<Date>();
  maxDate = input<Date>();
  separatorDash = input(false);
  autoAdjustRange = input(false);
  disabled = input(false);
  hidden = input(false);
  initialDates = input<DateRangeValue>([undefined, undefined]);
  deltaDays = input(0);
  offsetDaysOnStep = input(0);

  onValueChanged = output<ToolbarDaterangepickerAction>();

  valueChanged = (value: DateRangeValue | undefined) => {
    this.onValueChanged.emit({ key: this.key(), value: value });
  };

  validationHandler = inject(ValidationHandler);
  form: DaterangepickerForm = new DaterangepickerForm({
    validationHandler: this.validationHandler,
    element: new DaterangepickerModel(),
  });

  constructor() {
    super(inject(Injector));

    effect(() => {
      const disabledSignal = this.disabled();
      if (disabledSignal) {
        this.form.disable();
      } else {
        this.form.enable();
      }
    });
  }
}
