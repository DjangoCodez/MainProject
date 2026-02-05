import { SoftOneTestBed } from '@src/SoftOneTestBed';
import {
  ComponentFixture,
  fakeAsync,
  TestBed,
  tick,
} from '@angular/core/testing';
import { TextboxComponent } from './textbox.component';
import { ReactiveFormsModule } from '@angular/forms';
import {
  ChangeDetectorRef,
  ViewContainerRef,
  ComponentRef,
  DebugElement,
} from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('TextboxComponent', () => {
  let component: TextboxComponent;
  let componentRef: ComponentRef<TextboxComponent>;
  let fixture: ComponentFixture<TextboxComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, ReactiveFormsModule, TextboxComponent],
      providers: [ViewContainerRef, ChangeDetectorRef],
    }).compileComponents(); // Compile components and template

    fixture = TestBed.createComponent(TextboxComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges(); // Initial change detection
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should initialize with default values', () => {
      expect(component.inputId()).toBeTruthy();
      expect(component.labelKey()).toBe('');
      expect(component.secondaryLabelKey()).toBe('');
      expect(component.secondaryLabelBold()).toBe(false);
      expect(component.secondaryLabelParantheses()).toBe(true);
      expect(component.secondaryLabelPrefixKey()).toBe('');
      expect(component.secondaryLabelPostfixKey()).toBe('');
      expect(component.placeholderKey()).toBe('');
      expect(component.inline()).toBe(false);
      expect(component.alignInline()).toBe(false);
      expect(component.width()).toBe(0);
      expect(component.maxLength()).toBe(10000);
      expect(component.isPassword()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('onValueChange', () => {
      it('should emit valueChanged when onValueChange is called', () => {
        const value = 'test';
        vi.spyOn(component.valueChanged, 'emit');
        component.onValueChange(value);
        expect(component.valueChanged.emit).toHaveBeenCalledWith(value);
      });
    });
    describe('ngAfterViewInit', () => {
      it('should call super.ngAfterViewInit', () => {
        vi.spyOn(Object.getPrototypeOf(component), 'ngAfterViewInit');
        component.ngAfterViewInit();
        expect(
          Object.getPrototypeOf(component).ngAfterViewInit
        ).toHaveBeenCalled();
      });
      it('should set hasContent to true if content has content', fakeAsync(() => {
        // setting hasContent to true
        component.hasContent.set(false);
        vi.spyOn(component, 'elemHasContent').mockReturnValue(true);
        vi.spyOn(component.hasContent, 'set');
        component.ngAfterViewInit();
        tick();
        expect(component.hasContent.set).toHaveBeenCalledWith(true);
        expect(component.hasContent()).toBe(true);
      }));
      it('should not set hasContent to true if content has no content', fakeAsync(() => {
        // setting hasContent to false
        component.hasContent.set(false);
        vi.spyOn(component, 'elemHasContent').mockReturnValue(false);
        vi.spyOn(component.hasContent, 'set');
        component.ngAfterViewInit();
        tick();
        expect(component.hasContent.set).not.toHaveBeenCalled();
        expect(component.hasContent()).toBe(false);
      }));
    });
  });
  describe('DOM', () => {
    describe('div', () => {
      let divDebugElement: DebugElement;
      let divElement: HTMLElement;
      beforeEach(() => {
        divDebugElement = fixture.debugElement.query(By.css('div'));
        divElement = fixture.debugElement.query(By.css('div')).nativeElement;
      });
      it('should initialize with default values', () => {
        // Classes
        expect(divElement.classList.contains('mt-2')).toBe(true);
        expect(divElement.classList.contains('d-flex')).toBe(false);
        expect(divElement.classList.contains('flex-nowrap')).toBe(false);
        expect(
          divElement.classList.contains('form-label-inline-topalignment')
        ).toBe(false);

        // Style
        expect(divElement.style.width).toBe('');
      });
      it('should apply d-flex and flex-nowrap when inline is true', () => {
        componentRef.setInput('inline', true);
        fixture.detectChanges();
        expect(divElement.classList.contains('mt-2')).toBe(false);
        expect(divElement.classList.contains('d-flex')).toBe(true);
        expect(divElement.classList.contains('flex-nowrap')).toBe(true);
      });
      it('should apply form-label-inline-top-alignment if alignInline is true', () => {
        componentRef.setInput('alignInline', true);
        fixture.detectChanges();
        expect(
          divElement.classList.contains('form-label-inline-top-alignment')
        ).toBe(true);
      });
      it('should have a style width if inline is false and there is a width', () => {
        componentRef.setInput('inline', false);
        componentRef.setInput('width', 100);
        fixture.detectChanges();
        expect(divElement.style.width).toBe('100px');
      });
    });
    describe('form-label-container', () => {
      let formlabelcontainerElement: HTMLElement;
      beforeEach(() => {
        componentRef.setInput('labelKey', 'testLabel');
        fixture.detectChanges();
        formlabelcontainerElement = fixture.debugElement.query(
          By.css('.form-label-container.d-flex.align-items-center')
        ).nativeElement;
      });
      it('should initialize without me-2', () => {
        expect(formlabelcontainerElement.classList.contains('me-2')).toBe(
          false
        );
      });
      it('should apply me-2 if inline is true', () => {
        componentRef.setInput('inline', true);
        fixture.detectChanges();
        expect(formlabelcontainerElement.classList.contains('me-2')).toBe(true);
      });
    });
    describe('soe-label', () => {
      let soeLabelDebugElement: DebugElement;
      let soeLabelElement: HTMLElement;
      let soeLabelComponentInstance: any;
      beforeEach(() => {
        componentRef.setInput('labelKey', 'testLabel');
        fixture.detectChanges();
        soeLabelDebugElement = fixture.debugElement.query(By.css('soe-label'));
        soeLabelElement = soeLabelDebugElement.nativeElement;
        soeLabelComponentInstance = soeLabelDebugElement.componentInstance;
      });
      it('should initialize with default values', () => {
        expect(soeLabelComponentInstance.labelKey()).toBe(component.labelKey());
        expect(soeLabelComponentInstance.secondaryLabelKey()).toBe(
          component.secondaryLabelKey()
        );
        expect(soeLabelComponentInstance.secondaryLabelBold()).toBe(
          component.secondaryLabelBold()
        );
        expect(soeLabelComponentInstance.secondaryLabelParantheses()).toBe(
          component.secondaryLabelParantheses()
        );
        expect(soeLabelComponentInstance.secondaryLabelPrefixKey()).toBe(
          component.secondaryLabelPrefixKey()
        );
        expect(soeLabelComponentInstance.secondaryLabelPostfixKey()).toBe(
          component.secondaryLabelPostfixKey()
        );
        expect(soeLabelComponentInstance.isRequired()).toBe(
          component.isRequired()
        );
      });
      it('should pass the correct values', () => {
        componentRef.setInput('labelKey', 'testLabel');
        componentRef.setInput('secondaryLabelKey', 'testSecondaryLabel');
        componentRef.setInput('secondaryLabelBold', true);
        componentRef.setInput('secondaryLabelParantheses', false);
        componentRef.setInput('secondaryLabelPrefixKey', 'prefix');
        componentRef.setInput('secondaryLabelPostfixKey', 'postfix');
        component.isRequired.set(true);

        fixture.detectChanges();

        expect(soeLabelComponentInstance.labelKey()).toBe('testLabel');
        expect(soeLabelComponentInstance.secondaryLabelKey()).toBe(
          'testSecondaryLabel'
        );
        expect(soeLabelComponentInstance.secondaryLabelBold()).toBe(true);
        expect(soeLabelComponentInstance.secondaryLabelParantheses()).toBe(
          false
        );
        expect(soeLabelComponentInstance.secondaryLabelPrefixKey()).toBe(
          'prefix'
        );
        expect(soeLabelComponentInstance.secondaryLabelPostfixKey()).toBe(
          'postfix'
        );
        expect(soeLabelComponentInstance.isRequired()).toBe(true);
      });
    });
    describe('input-group', () => {
      let inputGroupDebugElement: DebugElement;
      let inputGroupElement: HTMLElement;
      beforeEach(() => {
        inputGroupDebugElement = fixture.debugElement.query(
          By.css('.input-group')
        );
        inputGroupElement = inputGroupDebugElement.nativeElement;
      });
      it('should not have a width', () => {
        expect(inputGroupElement.style.width).toBe('');
      });
      it('should have a width if inline is true and there is a width', () => {
        componentRef.setInput('inline', true);
        componentRef.setInput('width', 100);
        fixture.detectChanges();
        expect(inputGroupElement.style.width).toBe('100px');
      });
    });
    describe('input', () => {
      let inputDebugElement: DebugElement;
      let inputElement: HTMLInputElement;
      beforeEach(() => {
        inputDebugElement = fixture.debugElement.query(By.css('input'));
        inputElement = inputDebugElement.nativeElement;
      });
      it('should initialize with default values', () => {
        expect(inputElement.id).toBe(component.inputId());
        if (component.isPassword()) {
          expect(inputElement.type).toBe('password');
        } else {
          expect(inputElement.type).toBe('text');
        }
        expect(inputElement.placeholder).toBe(component.placeholderKey());
        expect(inputElement.autofocus).toBe(component.autoFocus());
        expect(inputElement.maxLength).toBe(component.maxLength());
        expect(inputElement.style.width).toBe('');
        expect(inputElement.type).toBe('text');
      });
      it('should pass the correct values', () => {
        componentRef.setInput('inputId', 'testId');
        componentRef.setInput('placeholderKey', 'testPlaceholder');
        componentRef.setInput('autoFocus', true);
        componentRef.setInput('maxLength', 10);
        componentRef.setInput('isPassword', true);

        fixture.detectChanges();

        expect(inputElement.id).toBe('testId');
        expect(inputElement.placeholder).toBe('testPlaceholder');
        expect(inputElement.autofocus).toBe(true);
        expect(inputElement.maxLength).toBe(10);
        expect(inputElement.type).toBe('password');
      });
      it('should have a width if there is a width', () => {
        componentRef.setInput('width', 100);
        fixture.detectChanges();
        expect(inputElement.style.width).toBe('100px');
      });
      it('should apply is-invalid if control is invalid', () => {
        component.control.setErrors({ required: true });
        fixture.detectChanges();
        expect(inputElement.classList.contains('is-invalid')).toBe(true);
      });
      it('should apply no-border-right-radius if hasContent is true', () => {
        component.hasContent.set(true);
        fixture.detectChanges();
        expect(inputElement.classList.contains('no-border-right-radius')).toBe(
          true
        );
      });
      it('should have a type password if isPassword is true, else text', () => {
        expect(inputElement.type).toBe('text');
        // Set to password
        componentRef.setInput('isPassword', true);
        fixture.detectChanges();
        expect(inputElement.type).toBe('password');
      });
      it('should have a value if there is a value', () => {
        const value = 'test';
        component.control.patchValue(value);
        fixture.detectChanges();
        expect(inputElement.value).toBe(value);
      });
      it('should call onValueChange on change', () => {
        const value = 'test';
        vi.spyOn(component, 'onValueChange');
        component.control.patchValue(value);
        inputDebugElement.triggerEventHandler('change', null);
        expect(component.onValueChange).toHaveBeenCalledWith(value);
      });
    });
  });
});
