import {
  Component,
  Injector,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { SelectComponent, SelectProps } from '@ui/forms/select/select.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ValidationHandler } from '@shared/handlers';
import { ToolbarSelectForm } from './models/toolbar-select-form.model';
import { ToolbarSelectModel } from './models/toolbar-select.model';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

export type ToolbarSelectAction = { key: string; value: number };

@Component({
  selector: 'soe-toolbar-select',
  imports: [SelectComponent, FormsModule, ReactiveFormsModule],
  templateUrl: './toolbar-select.component.html',
  styleUrl: './toolbar-select.component.scss',
})
export class ToolbarSelectComponent extends ValueAccessorDirective<
  number | undefined
> {
  key = input('');
  labelKey = input('');
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  width = input(0);
  items = input<ISmallGenericType[]>([]);
  optionIdField = input<keyof SelectProps>('id');
  optionNameField = input<keyof SelectProps>('name');
  selectedItem = input<ISmallGenericType | undefined>(undefined);
  selectedId = input(0);
  initialSelectedId = input(0);
  disabled = input(false);
  hidden = input(false);
  onValueChanged = output<ToolbarSelectAction>();

  valueChanged = (value: number) => {
    if (value !== this.selectedId())
      this.onValueChanged.emit({ key: this.key(), value: value });
  };

  validationHandler = inject(ValidationHandler);
  form: ToolbarSelectForm = new ToolbarSelectForm({
    validationHandler: this.validationHandler,
    element: new ToolbarSelectModel(),
  });

  constructor() {
    super(inject(Injector));

    effect(() => {
      this.form.patchValue({ selectedId: this.initialSelectedId() });
    });

    effect(() => {
      this.disabled() ? this.form.disable() : this.form.enable();
    });
  }
}
