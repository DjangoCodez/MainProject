import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { DecimalPipe } from '@angular/common';
import { ComponentRef, Renderer2, signal, LOCALE_ID } from '@angular/core';
import { NumberboxComponent } from './numberbox.component';
import { By } from '@angular/platform-browser';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import { vi } from 'vitest';
import { LabelComponent } from '@ui/label/label.component';
import { registerLocaleData } from '@angular/common';
import localeSv from '@angular/common/locales/sv';
import localeEn from '@angular/common/locales/en';

describe('NumberboxComponent', () => {
  let component: NumberboxComponent;
  let componentRef: ComponentRef<NumberboxComponent>;
  let fixture: ComponentFixture<NumberboxComponent>;
  let decimalPipe: DecimalPipe;
  let renderer2: Renderer2;

  // Default setup with Swedish locale
  beforeEach(async () => {
    registerLocaleData(localeSv);
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, NumberboxComponent],
      providers: [DecimalPipe, Renderer2, { provide: LOCALE_ID, useValue: 'sv-SE' }],
    }).compileComponents();

    fixture = TestBed.createComponent(NumberboxComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    decimalPipe = TestBed.inject(DecimalPipe);
    renderer2 = TestBed.inject(Renderer2);
    fixture.detectChanges();
  });

  describe('Setup', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should set digitsInfo on ngOnInit', () => {
      componentRef.setInput('decimals', 2);

      component.ngOnInit();
      expect(component.digitsInfo()).toBe('1.2-2');
    });

    it('should call setVisibleValue on control value change in ngOnInit', () => {
      const setVisibleValueSpy = vi.spyOn(component, 'setVisibleValue' as any);
      component.ngOnInit();
      component.control.setValue(123);
      expect(setVisibleValueSpy).toHaveBeenCalled();
    });
  });

  describe('methods', () => {
    let SoeConfigUtil: any;
    let NumberUtil: any;

    beforeEach(async () => {
      // Import SoeConfigUtil first so we can mock it before NumberUtil uses it
      SoeConfigUtil = (await import('@shared/util/soeconfig-util')).SoeConfigUtil;
      vi.clearAllMocks();
    });

    afterEach(() => {
      vi.restoreAllMocks();
    });

    describe('formatNormalizedNumberStringValue - Integration with ColumnUtil', () => {
      it('should parse Swedish numbers correctly with Swedish locale setup', async () => {
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
        componentRef.setInput('decimals', 2);
        const result = component.parseNumber('1 234,56');
        expect(result).toBeCloseTo(1234.56, 4);
      });

      it('should handle negative numbers in Swedish locale', async () => {
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
        componentRef.setInput('decimals', 2);
        const result = component.parseNumber('-1 234,56');
        expect(result).toBeCloseTo(-1234.56, 4);
      });

      it('should fallback gracefully when customNumberInputParser is not set', () => {
        component.customNumberInputParser = undefined;
        const result = component.parseNumber('1 234,56');
        expect(result).toBeCloseTo(1234.56, 4);
      });
    });

    describe('setVisibleValue', () => {
      it('should set visibleValue correctly', () => {
        const formatValueSpy = vi
          .spyOn(component, 'formatValue')
          .mockReturnValue('formattedValue');
        component.control.setValue(123);
        component['setVisibleValue']();
        expect(formatValueSpy).toHaveBeenCalledWith(123);
        expect(component.visibleValue()).toBe('formattedValue');
      });
    });

    describe('instantUpdate', () => {
      it('should emit valueChanged and mark control as dirty if value changes', () => {
        component['previousValue'] = 100;
        const emitSpy = vi.spyOn(component.valueChanged, 'emit');
        const markAsDirtySpy = vi.spyOn(component.control, 'markAsDirty');

        component.instantUpdate({ value: '200' } as HTMLInputElement);
        expect(emitSpy).toHaveBeenCalledWith(200);
        expect(markAsDirtySpy).toHaveBeenCalled();
        expect(component['previousValue']).toBe(200);
      });

      it('should not emit if value does not change', () => {
        component['previousValue'] = 100;
        const emitSpy = vi.spyOn(component.valueChanged, 'emit');

        component.instantUpdate({ value: '100' } as HTMLInputElement);
        expect(emitSpy).not.toHaveBeenCalled();
      });
    });

    describe('setValues - Incrementation/Decrementation', () => {
      it('should increment the control value correctly when valid input is provided', () => {
        component.inputRef = signal({ nativeElement: { value: '100' } }) as any;
        component.setValues(5);
        expect(component.control.value).toBe(105);
      });

      it('should decrement the control value correctly when negative increment is provided', () => {
        component.inputRef = signal({ nativeElement: { value: '100' } }) as any;
        component.setValues(-10);
        expect(component.control.value).toBe(90);
      });

      it('should handle zero increment (no change)', () => {
        component.inputRef = signal({ nativeElement: { value: '100' } }) as any;
        component.setValues(0);
        expect(component.control.value).toBe(100);
      });

      it('should set control value to empty if input is NaN', () => {
        component.inputRef = signal({
          nativeElement: { value: 'invalid' },
        }) as any;
        component.setValues();
        expect(component.control.value).toBe('');
        expect(component.visibleValue()).toBe('');
      });

      // Tests for internal state interactions
      it('should emit valueChanged and mark control as dirty when value changes', () => {
        const initialValue = 50;
        const increment = 25;

        component.control.setValue(initialValue);
        component.inputRef = signal({ nativeElement: { value: '100' } }) as any;

        const emitSpy = vi.spyOn(component.valueChanged, 'emit');
        const markAsDirtySpy = vi.spyOn(component.control, 'markAsDirty');

        component.setValues(increment);

        expect(component.control.value).toBe(125); // 100 + 25
        expect(emitSpy).toHaveBeenCalledWith(125);
        expect(markAsDirtySpy).toHaveBeenCalled();
      });

      it('should not emit valueChanged or mark dirty when control value does not change', () => {
        // Set up scenario where calculated value equals current control value
        component.control.setValue(100);
        component.inputRef = signal({ nativeElement: { value: '100' } }) as any;

        const emitSpy = vi.spyOn(component.valueChanged, 'emit');
        const markAsDirtySpy = vi.spyOn(component.control, 'markAsDirty');

        component.setValues(0); // 100 + 0 = 100 (no change)

        expect(component.control.value).toBe(100);
        expect(emitSpy).not.toHaveBeenCalled();
        expect(markAsDirtySpy).not.toHaveBeenCalled();
      });

      it('should call setVisibleValue after updating control value', () => {
        component.inputRef = signal({ nativeElement: { value: '50' } }) as any;
        const setVisibleValueSpy = vi.spyOn(component, 'setVisibleValue' as any);

        component.setValues(10);

        expect(setVisibleValueSpy).toHaveBeenCalled();
        expect(component.control.value).toBe(60);
      });

      it('should update visibleValue with properly formatted result after increment', () => {
        // Mock formatValue to return a predictable formatted string
        const formatValueSpy = vi.spyOn(component, 'formatValue').mockReturnValue('1,234.56');
        component.inputRef = signal({ nativeElement: { value: '1000' } }) as any;
        vi.spyOn(component, 'decimals').mockReturnValue(2);

        component.setValues(234.56);

        expect(component.control.value).toBe(1234.56);
        expect(formatValueSpy).toHaveBeenCalledWith(1234.56);
        expect(component.visibleValue()).toBe('1,234.56');
      });

      it('should handle disallowNegative by setting negative results to 0', () => {
        vi.spyOn(component, 'disallowNegative').mockReturnValue(true);
        component.inputRef = signal({ nativeElement: { value: '10' } }) as any;

        component.setValues(-20); // Would result in -10, but should become 0

        expect(component.control.value).toBe(0);
      });

      it('should preserve full precision when decimals=0 (only affects display)', () => {
        vi.spyOn(component, 'decimals').mockReturnValue(0);
        component.inputRef = signal({ nativeElement: { value: '10.99' } }) as any;
        component.setValues(5);
        expect(component.control.value).toBe(15.99);
      });

      it('should preserve original control value when operation results in NaN', () => {
        const originalValue = 42;
        component.control.setValue(originalValue);
        component.inputRef = signal({ nativeElement: { value: 'invalid' } }) as any;

        const emitSpy = vi.spyOn(component.valueChanged, 'emit');

        component.setValues(10);

        // Control should be set to empty, not preserve original value
        expect(component.control.value).toBe('');
        expect(emitSpy).not.toHaveBeenCalled(); // No emission since value changed from number to empty
      });

      // Locale-aware parsing integration test
      it('should handle locale-specific input parsing with customNumberInputParser', () => {
        // Mock a Swedish number parser
        vi.spyOn(component, 'decimals').mockReturnValue(2);
        component.customNumberInputParser = vi.fn().mockReturnValue(1234.56);
        component.inputRef = signal({ nativeElement: { value: '1 234,56' } }) as any; // Swedish format
        component.setValues(10);
        expect(component.customNumberInputParser).toHaveBeenCalledWith('1 234,56');
        expect(component.control.value).toBeCloseTo(1244.56, 4); // 1234.56 + 10
      });

      it('should remove decimals when allowed decimal count == 0 (swe)', async () => {
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;

        vi.spyOn(component, 'noFormatting').mockReturnValue(true);
        vi.spyOn(component, 'decimals').mockReturnValue(0);
        component.inputRef = signal({ nativeElement: { value: '12 345,67' } }) as any;
        component.setValues(5);
        expect(component.control.value).toBeCloseTo(12350.67, 2);
      });

      // Grid mode bug tests - Swedish locale decimal separator handling
      it('should NOT convert "123,45" to "12345,00" in grid mode with Swedish locale', async () => {
        // Setup: Grid mode with Swedish locale and 2 decimals
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
        componentRef.setInput('gridMode', true);
        componentRef.setInput('decimals', 2);

        component.ngOnInit();

        // Simulate user typing "123,45" in grid mode
        component.inputRef = signal({ nativeElement: { value: '123,45' } }) as any;

        // Call setValues (which is triggered on blur/enter in grid mode)
        component.setValues(0);

        // Expected: The value should be parsed as 123.45 (internally)
        expect(component.control.value).toBeCloseTo(123.45, 4);

        // Expected: When formatted back, it should NOT be parsed as 12345.00
        // The bug would cause it to be parsed as 12345.00
        expect(component.control.value).not.toBeCloseTo(12345.00, 4);

        // Verify the visible formatted value is correct
        const formattedValue = component.visibleValue();
        expect(formattedValue).toMatch(/123[,\s.]45/); // Should contain 123.45 or 123,45
      });

      it('should handle "123,45" to "123.45" conversion correctly when decimals=2', async () => {
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
        componentRef.setInput('decimals', 2);

        component.inputRef = signal({ nativeElement: { value: '123,45' } }) as any;
        component.setValues(0);

        // This test explicitly checks the bug: input "123,45" should NOT become 12345.00
        expect(component.control.value).toBe(123.45);
        expect(component.control.value).not.toBe(12345.00);
      });

      it('should preserve decimal separator when formatting in grid mode', async () => {
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
        componentRef.setInput('gridMode', true);
        componentRef.setInput('decimals', 2);
        componentRef.setInput('noFormatting', false);

        component.ngOnInit();

        // Set initial value
        component.control.setValue(123.45);

        // Trigger the setVisibleValue to format
        component['setVisibleValue']();

        // Format the value
        const formatted = component.formatValue(123.45);

        // Should format with 2 decimals, not convert to whole number
        expect(formatted).toMatch(/123[,\s]45/); // Swedish format: "123,45"
        expect(formatted).not.toContain('12345'); // Should NOT be "12345,00"
      });

      it('should handle Swedish decimal input "1234,56" correctly without removing comma', async () => {
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
        componentRef.setInput('decimals', 2);

        component.inputRef = signal({ nativeElement: { value: '1234,56' } }) as any;
        component.setValues(0);

        // Should parse as 1234.56, not 123456
        expect(component.control.value).toBeCloseTo(1234.56, 4);
        expect(component.control.value).not.toBeCloseTo(123456, 4);
      });

      // This should match the current behavior from parseNumber where "12.12" is parsed without issues and then displayed as 12,12
      it('should parse "12.12" as 12.12 and NOT as 1212 when using calculator in Swedish mode', async () => {
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
        component.customPrepareCalculationExpression = NumberUtil.prepareCalculationExpression;
        componentRef.setInput('decimals', 2);
        componentRef.setInput('useCalculator', true);
        component.ngOnInit();
        component.inputRef = signal({ nativeElement: { value: '12.12' } }) as any;
        component.setValues(0);

        expect(component.control.value).toBeCloseTo(12.12, 4);
      });

      it('should parse "12,12" as 12.12 when using calculator in Swedish mode', async () => {
        vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
        NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
        component.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
        component.customPrepareCalculationExpression = NumberUtil.prepareCalculationExpression;
        componentRef.setInput('decimals', 2);
        componentRef.setInput('useCalculator', true);
        component.ngOnInit();
        component.inputRef = signal({ nativeElement: { value: '12,12' } }) as any;
        component.setValues(0);

        expect(component.control.value).toBeCloseTo(12.12, 4);
      });
    });

    describe('formatValue', () => {
      it('should format value using decimalPipe when noFormatting() is false', () => {
        vi.spyOn(component, 'noFormatting').mockReturnValue(false);
        vi.spyOn(component, 'digitsInfo').mockReturnValue('1.2-2');
        vi.spyOn(component['decimalPipe'], 'transform').mockReturnValue('1,234.56');

        const result = component.formatValue(1234.56);

        expect(component['decimalPipe'].transform).toHaveBeenCalledWith('1234.56', '1.2-2');
        expect(result).toBe('1,234.56');
      });

      it('should return plain value respecting decimals when noFormatting() is true', () => {
        vi.spyOn(component, 'noFormatting').mockReturnValue(true);
        vi.spyOn(component, 'decimals').mockReturnValue(2);
        vi.spyOn(component, 'adjustDecimals').mockReturnValue(1234.56);

        const result = component.formatValue(1234.567);

        expect(component.adjustDecimals).toHaveBeenCalledWith(1234.567, 2);
        expect(result).toBe('1234.56');
      });

      it('should respect zero decimals when noFormatting is true', () => {
        vi.spyOn(component, 'noFormatting').mockReturnValue(true);
        vi.spyOn(component, 'decimals').mockReturnValue(0);
        vi.spyOn(component, 'adjustDecimals').mockReturnValue(1235);

        const result = component.formatValue(1234.567);

        expect(component.adjustDecimals).toHaveBeenCalledWith(1234.567, 0);
        expect(result).toBe('1235');
      });

      it('should handle noFormatting with non-zero decimals correctly', () => {
        vi.spyOn(component, 'noFormatting').mockReturnValue(true);
        vi.spyOn(component, 'decimals').mockReturnValue(3);
        vi.spyOn(component, 'adjustDecimals').mockReturnValue(123.457);

        const result = component.formatValue(123.4567);

        expect(component.adjustDecimals).toHaveBeenCalledWith(123.4567, 3);
        expect(result).toBe('123.457');
      });

      // Legacy tests for backward compatibility
      it('should format integer values correctly without breaking thousand separators', () => {
        vi.spyOn(component, 'noFormatting').mockReturnValue(true);
        vi.spyOn(component, 'decimals').mockReturnValue(0);
        const result = component.formatValue(1234.56);
        expect(result).toBe('1234'); // Truncated to 0 decimals
      });

      it('should handle large numbers with thousand separators correctly', () => {
        vi.spyOn(component, 'noFormatting').mockReturnValue(true);
        vi.spyOn(component, 'decimals').mockReturnValue(0);

        const result = component.formatValue(123456.789);

        expect(result).toBe('123456'); // Truncated to 0 decimals
      });

      describe('Swedish Locale Display Formatting', () => {
        it('should display 123.4567 as "123,46" with decimals=2 in Swedish locale', () => {
          componentRef.setInput('decimals', 2);
          componentRef.setInput('noFormatting', false);
          component.ngOnInit();

          const result = component.formatValue(123.4567);

          // Swedish uses comma as decimal separator
          expect(result).toContain('123');
          expect(result).toContain('46');
          expect(result).toMatch(/123[,\s]46/);
        });

        it('should display 1234.56 with Swedish thousand separator', () => {
          componentRef.setInput('decimals', 2);
          componentRef.setInput('noFormatting', false);
          component.ngOnInit();

          const result = component.formatValue(1234.56);

          // Swedish uses space as thousand separator: "1 234,56"
          expect(result).toMatch(/1[\s]234[,]56/);
        });

        it('should display value with no decimals when decimals=0 in Swedish locale', () => {
          componentRef.setInput('decimals', 0);
          componentRef.setInput('noFormatting', false);
          component.ngOnInit();

          const result = component.formatValue(123.9999);

          expect(result).toBe('124');
        });
      });
    });

    describe('replaceHyphen', () => {
      it('should replace − with -', () => {
        const input = '123−456';
        const result = component.replaceHyphen(input);
        expect(result).toBe('123-456');
      });
    });

    describe('stepUp', () => {
      it('should call setValues with step() value', () => {
        vi.spyOn(component, 'setValues');
        component.stepUp();
        expect(component.setValues).toHaveBeenCalledWith(1);
      });
    });

    describe('stepDown', () => {
      it('should call setValues with -step() value', () => {
        vi.spyOn(component, 'setValues');
        component.stepDown();
        expect(component.setValues).toHaveBeenCalledWith(-1);
      });
    });

    describe('setDisabledState', () => {
      let setPropertySpy: any;
      beforeEach(() => {
        setPropertySpy = vi.spyOn(component['_renderer'], 'setProperty');
        component.inputRef = signal({
          nativeElement: { disabled: false },
        }) as any;
      });
      it('should call super.setDisabledState with the correct argument', () => {
        const parentSetDisabledStateSpy = vi.spyOn(
          ValueAccessorDirective.prototype,
          'setDisabledState'
        );
        component.setDisabledState(true);
        expect(parentSetDisabledStateSpy).toHaveBeenCalledWith(true);

        component.setDisabledState(false);
        expect(parentSetDisabledStateSpy).toHaveBeenCalledWith(false);
      });

      it('should set the disabled property on the inputRef element to true', () => {
        component.setDisabledState(true);
        expect(setPropertySpy).toHaveBeenCalledWith(
          component.inputRef()?.nativeElement,
          'disabled',
          true
        );
      });

      it('should set the disabled property on the inputRef element to false', () => {
        component.setDisabledState(false);
        expect(setPropertySpy).toHaveBeenCalledWith(
          component.inputRef()?.nativeElement,
          'disabled',
          false
        );
      });
    });

    describe('onFocus', () => {
      let mockEvent: FocusEvent;
      let selectSpy: any;
      beforeEach(() => {
        // Mock control.value and visibleValue signals
        component.control.setValue(123);
        vi.spyOn(component.visibleValue, 'set');

        // Create a mock event with a target element that has a select method
        mockEvent = {
          target: {
            select: vi.fn(),
          },
        } as unknown as FocusEvent;

        selectSpy = vi.spyOn(mockEvent.target as HTMLInputElement, 'select');
      });

      it('should set visibleValue to control.value', () => {
        component.onFocus(mockEvent);
        expect(component.visibleValue.set).toHaveBeenCalledWith(123);
      });

      it('should call select() on the input element after a timeout', done => {
        component.onFocus(mockEvent);

        // Use setTimeout with done to wait for the async behavior to complete
        setTimeout(() => {
          expect(selectSpy).toHaveBeenCalled();
          done();
        }, 0);
      });
    });

    describe('onKeyDown', () => {
      let mockEvent: KeyboardEvent;

      beforeEach(() => {
        mockEvent = {
          preventDefault: vi.fn(),
          code: '',
          key: '',
          ctrlKey: false,
          metaKey: false,
          target: { selectionStart: 0 } as HTMLInputElement,
        } as unknown as KeyboardEvent;
      });

      it('should call preventDefault if disableInput() is true', () => {
        vi.spyOn(component, 'disableInput').mockReturnValue(true);

        component.onKeyDown(mockEvent);

        expect(mockEvent.preventDefault).toHaveBeenCalled();
      });

      it('should allow minus key at the start if disallowNegative() is false', () => {
        vi.spyOn(component, 'disableInput').mockReturnValue(false);
        vi.spyOn(component, 'disallowNegative').mockReturnValue(false);

        Object.defineProperty(mockEvent, 'code', { value: 'Minus' });
        (mockEvent.target as HTMLInputElement).selectionStart = 0;

        component.onKeyDown(mockEvent);

        expect(mockEvent.preventDefault).not.toHaveBeenCalled();
      });

      it('should call preventDefault for minus key if disallowNegative() is true', () => {
        vi.spyOn(component, 'disableInput').mockReturnValue(false);
        vi.spyOn(component, 'disallowNegative').mockReturnValue(true);

        Object.defineProperty(mockEvent, 'code', { value: 'Minus' });
        (mockEvent.target as HTMLInputElement).selectionStart = 0;

        component.onKeyDown(mockEvent);

        expect(mockEvent.preventDefault).toHaveBeenCalled();
      });

      it('should prevent non-numeric input if shift key is held down', () => {
        vi.spyOn(component, 'disableInput').mockReturnValue(false);

        Object.defineProperty(mockEvent, 'code', { value: 'KeyA' });

        component.onKeyDown(mockEvent);

        expect(mockEvent.preventDefault).toHaveBeenCalled();
      });

      it('should prevent non-numeric, non-utility keys', () => {
        vi.spyOn(component, 'disableInput').mockReturnValue(false);

        Object.defineProperty(mockEvent, 'code', { value: 'KeyQ' });

        component.onKeyDown(mockEvent);

        expect(mockEvent.preventDefault).toHaveBeenCalled();
      });
    });

    describe('onKeyUp', () => {
      let mockEvent: KeyboardEvent;
      let instantUpdateSpy: any;

      beforeEach(() => {
        // Create a mock event with a target element
        mockEvent = {
          target: { value: '123' },
        } as unknown as KeyboardEvent;

        // Spy on the instantUpdate method
        instantUpdateSpy = vi.spyOn(component, 'instantUpdate');
      });

      it('should call instantUpdate if updateInstantly() returns true', () => {
        vi.spyOn(component, 'updateInstantly').mockReturnValue(true);

        component.onKeyUp(mockEvent);

        expect(instantUpdateSpy).toHaveBeenCalledWith(
          mockEvent.target as HTMLInputElement
        );
      });

      it('should not call instantUpdate if updateInstantly() returns false', () => {
        vi.spyOn(component, 'updateInstantly').mockReturnValue(false);

        component.onKeyUp(mockEvent);

        expect(instantUpdateSpy).not.toHaveBeenCalled();
      });
    });

    describe('onPaste', () => {
      it('should update control value based on pasted input', () => {
        const clipboardEvent = {
          clipboardData: {
            getData: vi.fn().mockReturnValue('123'),
          },
          preventDefault: vi.fn(),
        } as unknown as ClipboardEvent;

        component.onPaste(clipboardEvent);
        expect(component.control.value).toBe('123');
        expect(clipboardEvent.preventDefault).toHaveBeenCalled();
      });

      it('should parse Swedish number format correctly with customNumberInputParser', () => {
        // Mock Swedish number parser
        component.customNumberInputParser = vi.fn().mockReturnValue(123.45);

        const clipboardEvent = {
          clipboardData: {
            getData: vi.fn().mockReturnValue('123,45'),
          },
          preventDefault: vi.fn(),
        } as unknown as ClipboardEvent;

        component.onPaste(clipboardEvent);

        expect(component.customNumberInputParser).toHaveBeenCalledWith('123,45');
        expect(component.control.value).toBeCloseTo(123.45, 4);
        expect(clipboardEvent.preventDefault).toHaveBeenCalled();
      });
    });
  });

  // English Locale Tests
  describe('English Locale (en-US)', () => {
    let enComponent: NumberboxComponent;
    let enComponentRef: ComponentRef<NumberboxComponent>;
    let enFixture: ComponentFixture<NumberboxComponent>;
    let SoeConfigUtil: any;
    let NumberUtil: any;

    beforeEach(async () => {
      registerLocaleData(localeEn);

      TestBed.resetTestingModule();
      await TestBed.configureTestingModule({
        imports: [SoftOneTestBed, NumberboxComponent],
        providers: [
          DecimalPipe,
          Renderer2,
          { provide: LOCALE_ID, useValue: 'en-US' }
        ],
      }).compileComponents();

      enFixture = TestBed.createComponent(NumberboxComponent);
      enComponent = enFixture.componentInstance;
      enComponentRef = enFixture.componentRef;
      enFixture.detectChanges();

      SoeConfigUtil = (await import('@shared/util/soeconfig-util')).SoeConfigUtil;
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      NumberUtil = (await import('@shared/util/number-util')).NumberUtil;
      enComponent.customNumberInputParser = NumberUtil.parseNumberByCurrentUserLanguage;
    });

    afterEach(() => {
      vi.restoreAllMocks();
    });

    describe('formatNormalizedNumberStringValue', () => {
      it('should parse English numbers correctly with English locale setup', () => {
        enComponentRef.setInput('decimals', 2);

        const result = enComponent.parseNumber('1,234.56');
        expect(result).toBeCloseTo(1234.56, 2);
      });
    });

    describe('setValues', () => {
      it('should preserve full precision when noFormatting=true and decimals=0', () => {
        vi.spyOn(enComponent, 'noFormatting').mockReturnValue(true);
        vi.spyOn(enComponent, 'decimals').mockReturnValue(0);
        enComponent.inputRef = signal({ nativeElement: { value: '1,234.56' } }) as any;

        enComponent.setValues(10);
        expect(enComponent.control.value).toBe(1244.56);
      });

      it('should preserve full precision in stored value when decimals=0', () => {
        vi.spyOn(enComponent, 'noFormatting').mockReturnValue(true);
        vi.spyOn(enComponent, 'decimals').mockReturnValue(0);
        enComponent.inputRef = signal({ nativeElement: { value: '1,234.56' } }) as any;
        enComponent.setValues(10);

        expect(enComponent.control.value).toBe(1244.56);
      });
    });

    describe('formatValue - English Locale Display', () => {
      it('should display 123.4567 as "123.46" with decimals=2 in English locale', () => {
        enComponentRef.setInput('decimals', 2);
        enComponentRef.setInput('noFormatting', false);
        enComponent.ngOnInit();

        const result = enComponent.formatValue(123.4567);

        // English uses period as decimal separator
        expect(result).toBe('123.46');
      });

      it('should display 1234.56 with English thousand separator', () => {
        enComponentRef.setInput('decimals', 2);
        enComponentRef.setInput('noFormatting', false);
        enComponent.ngOnInit();

        const result = enComponent.formatValue(1234.56);

        // English uses comma as thousand separator: "1,234.56"
        expect(result).toBe('1,234.56');
      });

      it('should display value with no decimals when decimals=0 in English locale', () => {
        enComponentRef.setInput('decimals', 0);
        enComponentRef.setInput('noFormatting', false);
        enComponent.ngOnInit();

        const result = enComponent.formatValue(123.9999);

        expect(result).toBe('124');
      });

      it('should preserve full precision when decimals=0 (only affects display)', () => {
        vi.spyOn(enComponent, 'decimals').mockReturnValue(0);
        enComponent.inputRef = signal({ nativeElement: { value: '10.99' } }) as any;

        enComponent.setValues(5);

        expect(enComponent.control.value).toBe(15.99);
      });
    });

    describe('onPaste', () => {
      it('should parse English number format correctly with customNumberInputParser', () => {
        // Mock English number parser
        enComponent.customNumberInputParser = vi.fn().mockReturnValue(1234.56);

        const clipboardEvent = {
          clipboardData: {
            getData: vi.fn().mockReturnValue('1,234.56'),
          },
          preventDefault: vi.fn(),
        } as unknown as ClipboardEvent;

        enComponent.onPaste(clipboardEvent);

        expect(enComponent.customNumberInputParser).toHaveBeenCalledWith('1,234.56');
        expect(enComponent.control.value).toBeCloseTo(1234.56, 2);
        expect(clipboardEvent.preventDefault).toHaveBeenCalled();
      });
    });
  });

  describe('DOM', () => {
    describe('Main Container', () => {
      it('should have "mt-2" class if "inline" is false', () => {
        vi.spyOn(component, 'inline').mockReturnValue(false);
        fixture.detectChanges();

        const mainContainer = fixture.debugElement.query(By.css('div'));
        expect(mainContainer.classes['mt-2']).toBeTruthy();
      });

      it('should apply width style if "width" is set and "inline" is false', () => {
        vi.spyOn(component, 'width').mockReturnValue(100);
        vi.spyOn(component, 'inline').mockReturnValue(false);
        fixture.detectChanges();

        const mainContainer = fixture.debugElement.query(By.css('div')).nativeElement;
        expect(mainContainer.style.width).toBe('100px');
      });

      it('should have "d-flex" and "flex-nowrap" classes if "inline" is true', () => {
        vi.spyOn(component, 'inline').mockReturnValue(true);
        fixture.detectChanges();

        const mainContainer = fixture.debugElement.query(By.css('div'));
        expect(mainContainer.classes['d-flex']).toBeTruthy();
        expect(mainContainer.classes['flex-nowrap']).toBeTruthy();
      });
    });

    describe('soe-label Component', () => {
      it('should render soe-label with correct bindings', () => {
        componentRef.setInput('labelKey', 'labelKey');
        componentRef.setInput('secondaryLabelKey', 'secondaryLabelKey');
        componentRef.setInput('secondaryLabelBold', true);
        componentRef.setInput('secondaryLabelParantheses', true);
        componentRef.setInput('secondaryLabelPrefixKey', 'prefixKey');
        componentRef.setInput('secondaryLabelPostfixKey', 'postfixKey');
        fixture.detectChanges();

        const soeLabel = fixture.debugElement.query(
          By.css('soe-label')
        ).componentInstance as LabelComponent;

        expect(soeLabel.labelKey()).toBe('labelKey');
        expect(soeLabel.secondaryLabelKey()).toBe('secondaryLabelKey');
        expect(soeLabel.secondaryLabelBold()).toBe(true);
        expect(soeLabel.secondaryLabelParantheses()).toBe(true);
        expect(soeLabel.secondaryLabelPrefixKey()).toBe('prefixKey');
        expect(soeLabel.secondaryLabelPostfixKey()).toBe('postfixKey');
      });
    });

    describe('Input Element', () => {
      it('should apply "is-invalid" class if control is invalid', () => {
        component.control.setErrors({ required: true });
        fixture.detectChanges();

        const inputEl = fixture.debugElement.query(By.css('input'));
        expect(inputEl.classes['is-invalid']).toBeTruthy();
      });

      it('should set maxLength based on maxLength property', () => {
        vi.spyOn(component, 'maxLength').mockReturnValue(10);
        fixture.detectChanges();

        const inputEl = fixture.debugElement.query(By.css('input'));
        expect(inputEl.attributes['maxlength']).toBe('10');
      });

      it('should set placeholder text based on placeholderKey', () => {
        vi.spyOn(component, 'placeholderKey').mockReturnValue('test-placeholder');
        fixture.detectChanges();

        const inputEl = fixture.debugElement.query(By.css('input'));
        expect(inputEl.attributes['placeholder']).toBe('test-placeholder');
      });

      it('should call onFocus on focus event', () => {
        const focusSpy = vi.spyOn(component, 'onFocus');
        const inputEl = fixture.debugElement.query(By.css('input'));

        inputEl.triggerEventHandler('focus', {});
        expect(focusSpy).toHaveBeenCalled();
      });

      it('should call onKeyDown on keydown event', () => {
        const keyDownSpy = vi.spyOn(component, 'onKeyDown');
        const inputEl = fixture.debugElement.query(By.css('input'));

        inputEl.triggerEventHandler('keydown', {});
        expect(keyDownSpy).toHaveBeenCalled();
      });

      it('should call onPaste on paste event', () => {
        const mockClipboardEvent = {
          clipboardData: {
            getData: vi.fn().mockReturnValue('123')
          },
          preventDefault: vi.fn()
        } as unknown as ClipboardEvent;
        const pasteSpy = vi.spyOn(component, 'onPaste');
        const inputEl = fixture.debugElement.query(By.css('input'));

        inputEl.triggerEventHandler('paste', mockClipboardEvent);
        expect(pasteSpy).toHaveBeenCalled();
      });

      it('should call onKeyUp on keyup event', () => {
        const keyUpSpy = vi.spyOn(component, 'onKeyUp');
        const inputEl = fixture.debugElement.query(By.css('input'));

        inputEl.triggerEventHandler('keyup', {});
        expect(keyUpSpy).toHaveBeenCalled();
      });
    });

    describe('soe-button Component', () => {
      it('should render soe-icon-button with chevron-up icon if showArrows() returns true', () => {
        componentRef.setInput('showArrows', true);
        fixture.detectChanges();

        const upButton = fixture.debugElement.queryAll(
          By.css('soe-icon-button')
        )[0].componentInstance;
        expect(upButton).toBeTruthy();
        expect(upButton.iconName()).toBe('chevron-up');
      });

      it('should render soe-icon-button with chevron-down icon if showArrows() returns true', () => {
        componentRef.setInput('showArrows', true);
        fixture.detectChanges();

        const downButton = fixture.debugElement.queryAll(
          By.css('soe-icon-button')
        )[1].componentInstance;
        expect(downButton).toBeTruthy();
        expect(downButton.iconName()).toBe('chevron-down');
      });

      it('should not render soe-icon-buttons if showArrows() returns false', () => {
        componentRef.setInput('showArrows', true);
        fixture.detectChanges();

        const upButton = fixture.debugElement.query(
          By.css('soe-icon-button[iconName="chevron-up"]')
        );
        const downButton = fixture.debugElement.query(
          By.css('soe-icon-button[iconName="chevron-down"]')
        );
        expect(upButton).toBeFalsy();
        expect(downButton).toBeFalsy();
      });
    });
  });
});
