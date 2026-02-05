import {
  AfterViewInit,
  Component,
  computed,
  ElementRef,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import { CommonModule } from '@angular/common';
import { LabelComponent } from '@ui/label/label.component';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'soe-textbox',
  imports: [CommonModule, ReactiveFormsModule, LabelComponent, TranslatePipe],
  templateUrl: './textbox.component.html',
  styleUrls: ['./textbox.component.scss'],
  providers: [
    { provide: NG_VALUE_ACCESSOR, multi: true, useExisting: TextboxComponent },
  ],
})
export class TextboxComponent
  extends ValueAccessorDirective<string>
  implements AfterViewInit
{
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
  placeholderKey = input('');
  inline = input(false);
  alignInline = input(false);
  width = input(0);
  maxLength = input<number | undefined>(10000);
  isPassword = input(false);
  manualReadOnly = input(false);

  valueChanged = output<string>();

  content = viewChild<ElementRef>('content');

  hasContent = signal<boolean>(false);

  onValueChange = (value: string) => this.valueChanged.emit(value);

  isReadOnly = computed(() => this.readOnly() || this.manualReadOnly());

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    if (this.elemHasContent(this.content())) {
      setTimeout(() => {
        this.hasContent.set(true);
      });
    }
  }
}
