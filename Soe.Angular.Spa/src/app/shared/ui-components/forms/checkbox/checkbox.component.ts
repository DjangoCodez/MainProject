import { Component, computed, input, output } from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import { CommonModule } from '@angular/common';
import { LabelComponent } from '@ui/label/label.component';

export type CheckboxBehaviour = 'checkbox' | 'switch';

@Component({
  selector: 'soe-checkbox',
  imports: [CommonModule, ReactiveFormsModule, LabelComponent],
  templateUrl: './checkbox.component.html',
  styleUrls: ['./checkbox.component.scss'],
  providers: [
    { provide: NG_VALUE_ACCESSOR, multi: true, useExisting: CheckboxComponent },
  ],
})
export class CheckboxComponent extends ValueAccessorDirective<boolean> {
  behaviour = input<CheckboxBehaviour>('checkbox');
  inputId = input<string>(Math.random().toString(24));
  labelKey = input('');
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  hasLabel = computed(() => {
    return (
      this.labelKey() ||
      this.secondaryLabelKey() ||
      this.secondaryLabelPrefixKey() ||
      this.secondaryLabelPostfixKey()
    );
  });
  inline = input(false);
  inToolbar = input(false);
  noMargin = input(false);
  checked = input(false);

  valueChanged = output<boolean>();

  onValueChange = (value: boolean) => this.valueChanged.emit(value);
}
