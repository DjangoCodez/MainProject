import { CommonModule } from '@angular/common';
import {
  AfterContentInit,
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
import { faAngleDown } from '@fortawesome/pro-light-svg-icons';
import { GraphicsUtil } from '@shared/util/graphics-util';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { IconModule } from '@ui/icon/icon.module'
import { LabelComponent } from '@ui/label/label.component';

@Component({
  selector: 'soe-colorpicker',
  imports: [CommonModule, ReactiveFormsModule, IconModule, LabelComponent],
  templateUrl: './colorpicker.component.html',
  styleUrls: ['./colorpicker.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: ColorpickerComponent,
    },
  ],
})
export class ColorpickerComponent
  extends ValueAccessorDirective<string>
  implements AfterContentInit, AfterViewInit
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
  inline = input(false);
  alignInline = input(false);
  width = input(0);

  valueChanged = output<string>();

  content = viewChild<ElementRef>('content');

  hasContent = signal(false);

  readonly faAngleDown = faAngleDown;

  ngAfterContentInit() {
    // Set default color to white if not specified
    if (!this.control.value) this.control.setValue('#ffffff');
  }

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    if (this.elemHasContent(this.content())) this.hasContent.set(true);
  }

  onValueChange = (value: string) => this.valueChanged.emit(value);

  foregroundColorByBackgroundBrightness(backgroundColor: string): string {
    return GraphicsUtil.foregroundColorByBackgroundBrightness(backgroundColor);
  }
}
