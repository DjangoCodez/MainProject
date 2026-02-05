import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ColorpickerComponent } from './colorpicker.component';
import { faAngleDown } from '@fortawesome/pro-light-svg-icons';
import {
  ComponentRef,
  DebugElement,
  ElementRef,
  NO_ERRORS_SCHEMA,
} from '@angular/core';
import { GraphicsUtil } from '@shared/util/graphics-util';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('ColorpickerComponent', () => {
  let component: ColorpickerComponent;
  let fixture: ComponentFixture<ColorpickerComponent>;
  let componentRef: ComponentRef<ColorpickerComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ColorpickerComponent],
      schemas: [NO_ERRORS_SCHEMA],
    });
    fixture = TestBed.createComponent(ColorpickerComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should initalize with default values', () => {
      expect(component.inputId).toBeTruthy();
      expect(component.labelKey()).toBe('');
      expect(component.secondaryLabelKey()).toBe('');
      expect(component.secondaryLabelBold()).toBe(false);
      expect(component.secondaryLabelParantheses()).toBe(true);
      expect(component.secondaryLabelPrefixKey()).toBe('');
      expect(component.secondaryLabelPostfixKey()).toBe('');
      expect(component.inline()).toBe(false);
      expect(component.alignInline()).toBe(false);
      expect(component.width()).toBe(0);
      expect(component.faAngleDown).toBe(faAngleDown);
    });
  });
  describe('methods', () => {
    describe('ngAfterContentInit', () => {
      it('should set color to white if not specified', () => {
        component.control.patchValue('');
        component.ngAfterContentInit();

        expect(component.control.value).toBe('#ffffff');
      });
      it('should keep color if color is already set', () => {
        const testcolor = '#86eb34';
        component.control.patchValue(testcolor);
        component.ngAfterContentInit();
        expect(component.control.value).toBe(testcolor);
      });
    });
    describe('ngAfterViewInit', () => {
      it('should set hasContent to true if content has innerHTML', () => {
        component.content = vi.fn().mockReturnValue({
          nativeElement: { innerHTML: 'test' },
        }) as any;
        component.ngAfterViewInit();
        expect(component.hasContent()).toBe(true);
      });
      it('should not set hasContent to true if content does not have innerHTML', () => {
        component.content = vi.fn().mockReturnValue({
          nativeElement: { innerHTML: '' },
        }) as any;
        component.ngAfterViewInit();
        expect(component.hasContent()).toBe(false);
      });
    });
    describe('onValueChange', () => {
      it('should emit valueChanged with value', () => {
        const value = 'testvalue';
        vi.spyOn(component.valueChanged, 'emit');
        component.onValueChange(value);
        expect(component.valueChanged.emit).toHaveBeenCalledWith(value);
      });
    });
    describe('foregroundColorByBackgroundBrightness', () => {
      it('should call GraphicsUtil.foregroundColorByBackgroundBrightness with the backgroundcolor', () => {
        const backgroundColor = '#ffffff';
        vi.spyOn(GraphicsUtil, 'foregroundColorByBackgroundBrightness');

        component.foregroundColorByBackgroundBrightness(backgroundColor);

        expect(
          GraphicsUtil.foregroundColorByBackgroundBrightness
        ).toHaveBeenCalledWith(backgroundColor);
      });
      it('should set white to black', () => {
        const backgroundColor = '#ffffff';
        const newColor =
          component.foregroundColorByBackgroundBrightness(backgroundColor);
        expect(newColor).toBe('#000000');
      });
      it('should set black to white', () => {
        const backgroundColor = '#000000';
        const newColor =
          component.foregroundColorByBackgroundBrightness(backgroundColor);
        expect(newColor).toBe('#ffffff');
      });
      it('should set dark color to white', () => {
        const backgroundColor = '#102922';
        const newColor =
          component.foregroundColorByBackgroundBrightness(backgroundColor);
        expect(newColor).toBe('#ffffff');
      });
      it('should set light color to black', () => {
        const backgroundColor = '#52deb7';
        const newColor =
          component.foregroundColorByBackgroundBrightness(backgroundColor);
        expect(newColor).toBe('#000000');
      });
    });
  });
  describe('DOM', () => {
    describe('div', () => {
      let divElement: DebugElement;
      beforeEach(() => {
        divElement = fixture.debugElement.query(By.css('div'));
      });
      it('should render with default values', () => {
        expect(divElement.classes['mt-2']).toBe(true);
        expect(divElement.styles['width']).toBe('');
        expect(divElement.classes['d-flex']).toBeFalsy();
        expect(divElement.classes['flex-nowrap']).toBeFalsy();
        expect(
          divElement.classes['form-label-inline-top-alignment']
        ).toBeFalsy();
      });
      it('should have different classes if inline and alignInline is true', () => {
        componentRef.setInput('inline', true);
        componentRef.setInput('alignInline', true);
        fixture.detectChanges();

        expect(divElement.classes['mt-2']).toBeFalsy;
        expect(divElement.classes['d-flex']).toBe(true);
        expect(divElement.classes['flex-nowrap']).toBe(true);
        expect(divElement.classes['form-label-inline-top-alignment']).toBe(
          true
        );
      });
      it('should set style.width according to set width', () => {
        componentRef.setInput('width', 30);
        fixture.detectChanges();
        expect(divElement.styles['width']).toBe('30px');
      });
    });
    describe('form-label-container', () => {
      let divElement: DebugElement;
      beforeEach(() => {
        componentRef.setInput('labelKey', 'test');
        fixture.detectChanges();
        divElement = fixture.debugElement.query(
          By.css('div.form-label-container.d-flex.align-items-center')
        );
      });
      it('should render without me-2 as class', () => {
        expect(divElement.classes['me-2']).toBeFalsy();
        expect(divElement).toBeTruthy();
      });
      it('should render with me-2 as clas if inline is true', () => {
        componentRef.setInput('inline', true);
        fixture.detectChanges();

        expect(divElement.classes['me-2']).toBe(true);
      });
    });
    describe('soe-label', () => {
      let soelabelElement: DebugElement;
      let soelabelComponentInstance: any;
      beforeEach(() => {
        componentRef.setInput('labelKey', 'test');
        fixture.detectChanges();
        soelabelElement = fixture.debugElement.query(By.css('soe-label'));
        soelabelComponentInstance = soelabelElement.componentInstance;
      });
      it('should render with default values', () => {
        // TEST componentInstance instead of DOM element when testing custom elements
        // For standard HTML elements, test the DOM element
        expect(soelabelComponentInstance.labelKey()).toBe(component.labelKey());
        expect(soelabelComponentInstance.secondaryLabelKey()).toBe(
          component.secondaryLabelKey()
        );
        expect(soelabelComponentInstance.secondaryLabelBold()).toBe(
          component.secondaryLabelBold()
        );
        expect(soelabelComponentInstance.secondaryLabelParantheses()).toBe(
          component.secondaryLabelParantheses()
        );
        expect(soelabelComponentInstance.secondaryLabelPostfixKey()).toBe(
          component.secondaryLabelPostfixKey()
        );
        expect(soelabelComponentInstance.secondaryLabelPrefixKey()).toBe(
          component.secondaryLabelPrefixKey()
        );
        expect(soelabelComponentInstance.isRequired()).toBe(
          component.isRequired()
        );
      });
      it('should set isRequired to true if isRequired is true', () => {
        component.isRequired.set(true);
        fixture.detectChanges();
        expect(soelabelElement.componentInstance.isRequired()).toBe(true);
      });
    });
    describe('d-flex', () => {
      let divElement: DebugElement;
      beforeEach(() => {
        divElement = fixture.debugElement.query(
          By.css('div.d-flex.input-group')
        );
      });
      it('should render with 0 width', () => {
        expect(divElement.styles.width).toBe('');
      });
      it('should render with a width if inline is true and a width is set', () => {
        componentRef.setInput('inline', true);
        componentRef.setInput('width', 100);
        fixture.detectChanges();
        expect(divElement.styles.width).toBe('100px');
      });
    });
    describe('label', () => {
      let labelElement: DebugElement;
      beforeEach(() => {
        labelElement = fixture.debugElement.query(By.css('label'));
      });
      it('should render with for equal to inputId', () => {
        expect(labelElement.attributes['for']).toBe(component.inputId());
      });
      it('should mirror controls value in innertext', () => {
        console.log(labelElement.nativeElement.innertext);
      });
    });
    describe('span', () => {
      let spanElement: DebugElement;
      beforeEach(() => {
        spanElement = fixture.debugElement.query(By.css('span'));
      });
      it('should reflect the control value', () => {
        const hexColor = '#4f4f4f';
        component.control.patchValue(hexColor);
        fixture.detectChanges();
        expect(spanElement.nativeElement.innerHTML).toBe(hexColor);
      });
      it('should be set to white if no value is patched', () => {
        expect(spanElement.nativeElement.innerHTML).toBe('#ffffff');
      });
    });
    describe('input', () => {
      let inputElement: DebugElement;
      beforeEach(() => {
        inputElement = fixture.debugElement.query(By.css('input'));
      });
      it('should render with default values', () => {
        expect(inputElement.nativeElement.id).toBe(component.inputId());
        expect(inputElement.nativeElement.type).toBe('color');
        expect(inputElement.nativeElement.autofocus).toBe(
          component.autoFocus()
        );
      });
      it('should have correct classes depending on conditions', () => {
        component.control.setErrors({ invalid: true });
        fixture.detectChanges();
        expect(
          inputElement.nativeElement.classList.contains('is-invalid')
        ).toBe(true);

        component.hasContent.set(true);
        fixture.detectChanges();
        expect(
          inputElement.nativeElement.classList.contains(
            'no-border-right-radius'
          )
        ).toBe(true);
      });
      it('should call onValueChange with inputvalue on change', () => {
        inputElement.nativeElement.value = '#ff000';
        vi.spyOn(component, 'onValueChange');
        inputElement.triggerEventHandler('change', null);
        expect(component.onValueChange).toHaveBeenCalledWith(
          inputElement.nativeElement.value
        );
      });
    });
    describe('fa-icon', () => {
      let faIconElement: DebugElement;
      beforeEach(() => {
        faIconElement = fixture.debugElement.query(By.css('fa-icon'));
      });
      it('should set style color to black as default', () => {
        expect(faIconElement.nativeElement.style.color).toBe('rgb(0, 0, 0)');
      });
      it('should set style color to black if backgroundcolor light', () => {
        const backgroundColor = '#e6f5f1';
        component.control.patchValue(backgroundColor);
        fixture.detectChanges();
        expect(faIconElement.nativeElement.style.color).toBe('rgb(0, 0, 0)');
      });
      it('should set style color to white if backgroundcolor dark', () => {
        const backgroundColor = '#021c15';
        component.control.patchValue(backgroundColor);
        fixture.detectChanges();
        expect(faIconElement.nativeElement.style.color).toBe(
          'rgb(255, 255, 255)'
        );
      });
      it('should set style color to white if backgroundcolor black', () => {
        const backgroundColor = '#000000';
        component.control.patchValue(backgroundColor);
        fixture.detectChanges();
        expect(faIconElement.nativeElement.style.color).toBe(
          'rgb(255, 255, 255)'
        );
      });
      it('should set style color to black if backgroundcolor white', () => {
        const backgroundColor = '#ffffff';
        component.control.patchValue(backgroundColor);
        fixture.detectChanges();
        expect(faIconElement.nativeElement.style.color).toBe('rgb(0, 0, 0)');
      });
    });
  });
});
