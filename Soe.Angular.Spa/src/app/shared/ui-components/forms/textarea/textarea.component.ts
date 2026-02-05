import {
  AfterViewInit,
  Component,
  computed,
  ElementRef,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import { CommonModule } from '@angular/common';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { LabelComponent } from '@ui/label/label.component';
import { TranslatePipe } from '@ngx-translate/core';
import { AIUtilityService } from '@shared/services/ai-utility.service';

@Component({
  selector: 'soe-textarea',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LabelComponent,
    TranslatePipe,
    IconButtonComponent,
  ],
  templateUrl: './textarea.component.html',
  styleUrls: ['./textarea.component.scss'],
  providers: [
    { provide: NG_VALUE_ACCESSOR, multi: true, useExisting: TextareaComponent },
  ],
})
export class TextareaComponent
  extends ValueAccessorDirective<string>
  implements AfterViewInit
{
  textUtilityService = inject(AIUtilityService);

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
  rows = input(3);
  resizeable = input(false);
  showLength = input(false);
  maxLength = input<number | undefined>(10000);

  valueChange = output<string>();

  content = viewChild<ElementRef>('content');

  hasContent = signal<boolean>(false);

  onValueChange = (value: string) => this.valueChange.emit(value);

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    if (this.elemHasContent(this.content())) {
      setTimeout(() => {
        this.hasContent.set(true);
      });
    }
  }

  aiImprove() {
    if (!this.controlValue) return;
    this.textUtilityService
      .professionalizeText(this.controlValue)
      .subscribe(result => {
        result && this.control.setValue(result);
      });
  }
}
