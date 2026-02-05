import {
  AfterViewInit,
  Component,
  computed,
  ElementRef,
  input,
  output,
  signal,
  viewChild,
  OnInit,
  OnDestroy,
} from '@angular/core';
import {
  NgControl,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
} from '@angular/forms';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import { CommonModule } from '@angular/common';
import { LabelComponent } from '@ui/label/label.component';
import { Subscription } from 'rxjs';

export interface SelectProps {
  id: number;
  name: string;
}

@Component({
  selector: 'soe-select',
  imports: [CommonModule, ReactiveFormsModule, LabelComponent],
  templateUrl: './select.component.html',
  styleUrls: ['./select.component.scss'],
  providers: [
    { provide: NG_VALUE_ACCESSOR, multi: true, useExisting: SelectComponent },
    { provide: NgControl, multi: true, useExisting: SelectComponent },
  ],
})
export class SelectComponent<T extends SelectProps>
  extends ValueAccessorDirective<number>
  implements AfterViewInit, OnInit, OnDestroy
{
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
  alignInline = input(false);
  width = input(0);
  items = input<Array<T>>([]);
  optionIdField = input<keyof T>('id');
  optionNameField = input<keyof T>('name');
  selectedItem = input<T | undefined>(undefined);
  selectedId = input(0);
  manualDisabled = input(false);
  manualReadOnly = input(false);
  noBorderLeftRadius = input(false);
  tabindex = input<number | string | undefined>(undefined);

  isDisabled = computed(() => {
    return this.control?.disabled || this.manualDisabled();
  });
  isReadOnly = computed(() => this.readOnly() || this.manualReadOnly());

  valueChanged = output<number>();

  content = viewChild<ElementRef>('content');

  hasContent = signal(false);

  private valueChangesSub?: Subscription;

  valueLabel = computed(() => {
    const value = this.value();
    if (!value) return ' ';

    const selectedItem = this.items().find(
      item => item[this.optionIdField()] === value
    );
    if (!selectedItem) return ' ';
    return selectedItem[this.optionNameField()] as string;
  });

  ngOnInit(): void {
    super.ngOnInit();
    if (this.control) {
      this.valueChangesSub = this.control.valueChanges.subscribe(d => {
        this.value.set(d);
      });
    }
  }

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    if (this.elemHasContent(this.content())) this.hasContent.set(true);
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
    this.valueChangesSub?.unsubscribe();
  }

  onValueChange(valueEvent: any) {
    this.value.set(
      Number(
        valueEvent.target.options[valueEvent.target.options.selectedIndex].value
      )
    );
    this.control?.setValue(this.value());
    this.valueChanged.emit(this.value() ?? 0);
  }
}
