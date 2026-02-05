import {
  AfterContentInit,
  AfterViewInit,
  Component,
  ElementRef,
  Injector,
  OnInit,
  Renderer2,
  computed,
  inject,
  input,
  output,
  signal,
  viewChild,
  OnDestroy,
} from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import { CommonModule, DecimalPipe } from '@angular/common';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { LabelComponent } from '@ui/label/label.component';
import { TranslatePipe } from '@ngx-translate/core';
import { CalculatorService } from '@shared/services/calculator/calculator.service';
import { Subscription } from 'rxjs';
import { NumberUtil } from '@shared/util/number-util';

@Component({
  selector: 'soe-numberbox',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IconButtonComponent,
    LabelComponent,
    TranslatePipe,
  ],
  templateUrl: './numberbox.component.html',
  styleUrls: ['./numberbox.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: NumberboxComponent,
    },
    DecimalPipe,
  ],
})
export class NumberboxComponent
  extends ValueAccessorDirective<number>
  implements OnInit, AfterViewInit, AfterContentInit, OnDestroy {
  calculator = inject(CalculatorService);
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
  decimals = input(0);
  step = input(1);
  maxLength = input<number | undefined>(10000);
  noFormatting = input(false);
  disallowNegative = input(false);
  disableInput = input(false);
  allowTextualInput = input(false);
  showArrows = input(false);
  showLeftRangeArrow = input(false);
  showRightRangeArrow = input(false);
  noMargins = input(false);
  manualDisabled = input(false);
  manualReadOnly = input(false);
  updateInstantly = input(false);
  gridMode = input(false);
  useCalculator = input(false);
  inRange = input(false);
  private previousValue: number | undefined = undefined;

  // When used in ag-grid and we configure a custom valueFormatter, as we currently do, we need to parse the input value back to number using the same format expectations.
  // Thus customNumberInputParser and customPrepareCalculationExpression should also be set by ColummUtil.createColumnNumber
  customNumberInputParser?: (value: string) => number;
  customPrepareCalculationExpression?: (value: string) => string;

  valueChanged = output<number>();
  stepRight = output<number | undefined>();
  stepLeft = output<number | undefined>();
  keyDown = output<KeyboardEvent>();
  keyUp = output<KeyboardEvent>();

  content = viewChild<ElementRef>('content');
  inputRef = viewChild<ElementRef>('input');

  hasContent = signal(false);

  visibleValue = signal('');
  digitsInfo = signal('');

  isDisabled = computed(() => {
    return (
      (this.control && this.control.disabled) ||
      this.disabled() ||
      this.manualDisabled()
    );
  });

  isReadOnly = computed(() => this.readOnly() || this.manualReadOnly());

  private valueChangesSub?: Subscription;

  constructor(
    private decimalPipe: DecimalPipe,
    private _renderer: Renderer2
  ) {
    super(inject(Injector));
  }

  parseNumber = (value: string):number => {
    if (this.customNumberInputParser) {
      const n = this.customNumberInputParser(value);
      return n;
    }
    // Else assume swedish formatting to preserve legacy behavior
    return Number(value.replace(/\s/g, '').replace(/,/g, '.'));
  };

  ngOnInit(): void {
    super.ngOnInit();
    this.digitsInfo.set('1.' + this.decimals() + '-' + this.decimals());
    if (this.control) {
      this.valueChangesSub = this.control.valueChanges.subscribe(() => {
        this.setVisibleValue();
      });
    }
  }

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    if (this.elemHasContent(this.content())) this.hasContent.set(true);

    this.setVisibleValue();

    if (this.gridMode()) {
      setTimeout(() => {
        this.inputRef()?.nativeElement.focus();
        this.inputRef()?.nativeElement.select();
      });
    }
  }

  ngAfterContentInit(): void {
    this.setVisibleValue();
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
    this.valueChangesSub?.unsubscribe();
  }

  private setVisibleValue() {
    this.visibleValue.set(this.formatValue(this.control.value));
  }

  private truncateToDecimals(value: number, n = 0) {
    if (!Number.isFinite(value)) return value;
    if (n <= 0) return Math.trunc(value);
    const f = 10 ** n;
    const t = Math.trunc(value * f) / f;
    return Object.is(t, -0) ? 0 : t;
  }

  public instantUpdate(input: HTMLInputElement) {
    const value = Number(input.value.replace(',', '.'));
    if (this.previousValue == value) return;

    this.valueChanged.emit(value);
    this.control.markAsDirty();
    this.previousValue = value;
  }

  setValues(increment = 0) {
    const controlPrevValue = this.control.value;
    let currentValue = this.replaceHyphen(this.inputRef()?.nativeElement.value);

    if (currentValue.length > 0) {
      let newValue: number;

      if (this.useCalculator()) {
        let cleanedExpression: string;
        if (this.customPrepareCalculationExpression)
          cleanedExpression = this.customPrepareCalculationExpression(currentValue);
        else {
          console.warn('Numberbox: When using calculator, provide a customPrepareCalculationExpression function for consistent number formatting handling. Using fallback assuming Swedish number formatting.');
          cleanedExpression = currentValue.replaceAll(/\s/g, '').replaceAll(',', '.');
        }
        let calculatedValue = this.calculator.calculate(cleanedExpression);
        newValue = calculatedValue + increment;
      } else {
        newValue = this.parseNumber(currentValue) + increment;
      }

      if (isNaN(newValue)) {
        this.control.setValue('');
        this.visibleValue.set('');
        return;
      }

      if (this.disallowNegative() && newValue < 0)
        newValue = 0;

      this.control.setValue(newValue);
      this.setVisibleValue();
    } else {
      this.control.setValue(null);
    }

    // Mark dirty and emit value only if value actually changed
    if (controlPrevValue !== this.control.value) {
      this.valueChanged.emit(Number(this.control.value));
      this.control.markAsDirty();
    }
  }

  formatValue(value: number): string {
    if (value || value === 0) {
      if (!this.noFormatting()) {
        // Use DecimalPipe for full locale-aware formatting
        return (
          this.decimalPipe.transform(
            this.replaceHyphen(value.toString()),
            this.digitsInfo()
          ) || ''
        );
      } else {
        // No formatting: return plain number respecting decimal precision
        // Always respect the decimals() setting for data consistency
        const adjustedValue = this.adjustDecimals(value, this.decimals());
        return adjustedValue.toString();
      }
    }
    return '';
  }

  /**
   * Adjusts the amount of decimals to the requested number of decimals.
   * Return type equal to input type (string | Number).
   * When input is string, current user language setting is used to format the resulting string value.
   * @param value
   * @param decimalCount
   */
  adjustDecimals(value: string, decimalCount: number): string;
  adjustDecimals(value: number, decimalCount: number): number;
  adjustDecimals(value: string | number, decimalCount: number): string | number {
    if (typeof value === 'string') {
      const truncated = this.truncateToDecimals(this.parseNumber(value), decimalCount);
      return NumberUtil.formatDecimal(truncated, decimalCount);
    } else {
      return this.truncateToDecimals(value, decimalCount);
    }
  }

  replaceHyphen(value: string): string {
    return value.replace('−', '-');
  }

  stepUp() {
    this.setValues(this.step());
  }

  stepDown() {
    this.setValues(-this.step());
  }

  setDisabledState(isDisabled: boolean): void {
    super.setDisabledState(isDisabled);
    this._renderer.setProperty(
      this.inputRef()?.nativeElement,
      'disabled',
      isDisabled
    );
  }

  onFocus(event: FocusEvent): void {
    this.visibleValue.set(this.control.value);
    setTimeout(() => {
      (event.target as HTMLInputElement)?.select();
    });
  }

  onKeyDown(e: KeyboardEvent) {
    if (!this.disableInput()) {
      if (this.allowTextualInput()) {
        return;
      }
      const isUtilityKeys = (event: KeyboardEvent): boolean => {
        return (
          [
            'Delete',
            'Tab',
            'Escape',
            'Enter',
            'NumpadEnter',
            //'Space',
            'NumpadComma',
            'NumpadDecimal',
            'Comma',
            'Period',
          ].indexOf(event.code) > -1 ||
          [
            'Backspace',
            'Delete',
            'Decimal',
            'ArrowLeft',
            'ArrowRight',
            'Home',
            'End',
            'Paste',
            'Redo',
            'Undo',
          ].indexOf(event.key) > -1
        );
      };
      const allowMinus = (event: KeyboardEvent): boolean => {
        return (
          //Hyphen. Slash "code" on Swedish keysboards
          //Minus.
          !this.disallowNegative() &&
          (event.target as HTMLInputElement).selectionStart === 0 &&
          (event.code === 'Minus' ||
            event.code === 'NumpadSubtract' ||
            event.key === '-' ||
            event.key === '−')
        );
      };
      //since keyCode is deprecated
      if (e.code) {
        if (
          isUtilityKeys(e) ||
          allowMinus(e) ||
          ((e.code === 'KeyA' ||
            e.code === 'KeyC' ||
            e.code === 'KeyV' ||
            e.code === 'KeyX') &&
            (e.ctrlKey || e.metaKey))
        ) {
          return;
        }

        if (
          !(
            (!e.shiftKey && e.code.startsWith('Digit')) ||
            (!isNaN(Number(e.key)) &&
              Number(e.key) >= 0 &&
              Number(e.key) <= 9 &&
              e.code === 'Numpad' + e.key)
          )
        )
          e.preventDefault();
      }
    } else e.preventDefault();

    this.keyDown.emit(e);
  }

  onKeyUp(e: KeyboardEvent) {
    if (this.updateInstantly() || e.key === 'Enter' || e.key === 'NumpadEnter')
      this.instantUpdate(e.target as HTMLInputElement);

    this.keyUp.emit(e);
  }

  onPaste(event: ClipboardEvent): void {
    const pasteVal = event.clipboardData?.getData('text');
    const hasMinusFirst = pasteVal![0] === '-';
    let newValue = this.customNumberInputParser
      ? this.customNumberInputParser(pasteVal || '')
      : Number(pasteVal?.replaceAll(new RegExp(/\D*/g), ''));

    if (this.disallowNegative() && hasMinusFirst)
      newValue = Math.abs(newValue);

    event.preventDefault();
    this.control.setValue(newValue.toString());
  }

  notifyRangeStepRight() {
    this.stepRight.emit(Number(this.control.value));
  }

  notifyRangeStepLeft() {
    this.stepLeft.emit(Number(this.control.value));
  }
}
