import {
  Component,
  effect,
  inject,
  Injector,
  input,
  output,
} from '@angular/core';
import { DatepickerComponent, DatepickerView } from '@ui/forms/datepicker/datepicker.component';
import { ToolbarDatepickerForm } from './models/toolbar-datepicker-form.model';
import { ToolbarDatepickerModel } from './models/toolbar-datepicker.model';
import { ValidationHandler } from '@shared/handlers';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import {
  FormsModule,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
} from '@angular/forms';

export type ToolbarDatepickerAction =
  | { key: string; value: Date | undefined }
  | undefined;

@Component({
  selector: 'soe-toolbar-datepicker',
  imports: [DatepickerComponent, FormsModule, ReactiveFormsModule],
  templateUrl: './toolbar-datepicker.component.html',
  styleUrl: './toolbar-datepicker.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: ToolbarDatepickerComponent,
      multi: true,
    },
  ],
})
export class ToolbarDatepickerComponent extends ValueAccessorDirective<
  Date | undefined
> {
  inputId = input<string>(Math.random().toString(24));
  key = input('');
  labelKey = input('');
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  width = input(0);
  view = input<DatepickerView>('day');
  hideToday = input(false);
  hideClear = input(false);
  showArrows = input(false);
  hideCalendarButton = input(false);
  minDate = input<Date>();
  maxDate = input<Date>();
  disabled = input(false);
  hidden = input(false);
  initialDate = input<Date | undefined>(undefined);

  onValueChanged = output<ToolbarDatepickerAction>();

  valueChanged = (value: Date | undefined) => {
    this.onValueChanged.emit({ key: this.key(), value: value });
  };

  validationHandler = inject(ValidationHandler);
  form: ToolbarDatepickerForm = new ToolbarDatepickerForm({
    validationHandler: this.validationHandler,
    element: new ToolbarDatepickerModel(),
  });

  constructor() {
    super(inject(Injector));

    effect(() => {
      const initialDateSignal = this.initialDate();
      if (initialDateSignal) {
        this.form.patchValue({ date: initialDateSignal });
      }
    });

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
