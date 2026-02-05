import { Component, input, output } from '@angular/core';
import { CheckboxBehaviour } from '@ui/forms/checkbox/checkbox.component';
import { LabelComponent } from '@ui/label/label.component';

export type ToolbarCheckboxAction = { key: string; value: boolean };

@Component({
  selector: 'soe-toolbar-checkbox',
  imports: [LabelComponent],
  templateUrl: './toolbar-checkbox.component.html',
  styleUrl: './toolbar-checkbox.component.scss',
})
export class ToolbarCheckboxComponent {
  behaviour = input<CheckboxBehaviour>('checkbox');
  inputId = input<string>(Math.random().toString(24));
  key = input('');
  labelKey = input('');
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  checked = input(false);

  tooltip = input('');
  disabled = input(false);
  hidden = input(false);

  onValueChanged = output<ToolbarCheckboxAction>();

  valueChanged = (value: boolean) => {
    this.onValueChanged.emit({ key: this.key(), value: value });
  };
}
