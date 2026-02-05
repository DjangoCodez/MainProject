import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckboxComponent } from './checkbox.component';
import { ComponentRef, DebugElement, NO_ERRORS_SCHEMA } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('CheckboxComponent', () => {
  let component: CheckboxComponent;
  let fixture: ComponentFixture<CheckboxComponent>;
  let componentRef: ComponentRef<CheckboxComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CheckboxComponent],
      schemas: [NO_ERRORS_SCHEMA],
    });
    fixture = TestBed.createComponent(CheckboxComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('properties', () => {
    it('should initialize with the correct default values', () => {
      expect(component.inputId).toBeTruthy();
      expect(component.labelKey()).toBe('');
      expect(component.secondaryLabelKey()).toBe('');
      expect(component.secondaryLabelBold()).toBe(false);
      expect(component.secondaryLabelParantheses()).toBe(true);
      expect(component.secondaryLabelPrefixKey()).toBe('');
      expect(component.secondaryLabelPostfixKey()).toBe('');
      expect(component.inline()).toBe(false);
      expect(component.inToolbar()).toBe(false);
      expect(component.noMargin()).toBe(false);
      expect(component.checked()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('onValueChange', () => {
      it('should emit the value', () => {
        vi.spyOn(component.valueChanged, 'emit');
        component.onValueChange(true);
        expect(component.valueChanged.emit).toHaveBeenCalledWith(true);
      });
    });
  });
  describe('DOM', () => {
    describe('div', () => {
      let div: DebugElement;
      beforeEach(() => {
        div = fixture.debugElement.query(By.css('div'));
      });
      it('should render with correct classes', () => {
        expect(div).toBeTruthy();
        expect(div.classes['form-check']).toBe(true);
        expect(div.classes['mt-2']).toBe(true);
      });
      it('should render with different classes if inline and inToolbar are set to true', () => {
        componentRef.setInput('inline', true);
        componentRef.setInput('inToolbar', true);
        fixture.detectChanges();

        expect(div.classes['form-check']).toBe(true);
        expect(div.classes['form-check-inline']).toBe(true);
        expect(div.classes['inline']).toBe(true);
        expect(div.classes['me-3']).toBe(true);
        expect(div.classes['mt-1']).toBe(true);
        expect(div.classes['mt-2']).toBeFalsy();
      });
    });
    describe('input', () => {
      let input: DebugElement;
      beforeEach(() => {
        input = fixture.debugElement.query(By.css('input'));
      });
      it('should render with default values', () => {
        expect(input.attributes['id']).toBe(component.inputId());
        expect(input.attributes['type']).toBe('checkbox');
      });
      it('should render checked as true if the controls value is true', () => {
        component.control.patchValue(true);
        fixture.detectChanges();
        expect(input.nativeElement.checked).toBe(true);
      });
      it('should render formControl with the components control', () => {
        // The formControl directive is applied through [formControl] binding
        expect(input.nativeElement).toBeTruthy();
        expect(component.control).toBeTruthy();
      });
      it('should call onValueChanged with true on change', () => {
        component.control.patchValue(true);
        fixture.detectChanges();

        vi.spyOn(component, 'onValueChange');

        input.nativeElement.checked = true;
        input.nativeElement.dispatchEvent(new Event('change'));

        expect(component.onValueChange).toHaveBeenCalledWith(true);
      });
      it('should call onValueChanged with false on change', () => {
        component.control.patchValue(false);
        fixture.detectChanges();

        vi.spyOn(component, 'onValueChange');

        input.nativeElement.checked = false;
        input.nativeElement.dispatchEvent(new Event('change'));

        expect(component.onValueChange).toHaveBeenCalledWith(false);
      });
    });
    describe('soe-label', () => {
      let label: DebugElement;
      beforeEach(() => {
        componentRef.setInput('labelKey', 'test');
        fixture.detectChanges();
        label = fixture.debugElement.query(By.css('soe-label'));
      });
      it('should render with default values', () => {
        expect(label.componentInstance.labelKey()).toBe(component.labelKey());
        expect(label.componentInstance.forRef()).toBe(component.inputId());
        expect(label.componentInstance.secondaryLabelKey()).toBe(
          component.secondaryLabelKey()
        );
        expect(label.componentInstance.secondaryLabelBold()).toBe(
          component.secondaryLabelBold()
        );
        expect(label.componentInstance.secondaryLabelParantheses()).toBe(
          component.secondaryLabelParantheses()
        );
        expect(label.componentInstance.secondaryLabelPostfixKey()).toBe(
          component.secondaryLabelPostfixKey()
        );
        expect(label.componentInstance.secondaryLabelPrefixKey()).toBe(
          component.secondaryLabelPrefixKey()
        );
        expect(label.componentInstance.isRequired()).toBe(
          component.isRequired()
        );
      });
      it('should update labelKey if the value changes', () => {
        componentRef.setInput('labelKey', 'testvalue');
        fixture.detectChanges();
        expect(label.componentInstance.labelKey()).toBe('testvalue');
      });
      it('should update forRef if the value changes', () => {
        componentRef.setInput('inputId', 'testvalue');
        fixture.detectChanges();
        expect(label.componentInstance.forRef()).toBe('testvalue');
      });
      it('should update secondaryLabelKey if the value changes', () => {
        componentRef.setInput('secondaryLabelKey', 'testvalue');
        fixture.detectChanges();
        expect(label.componentInstance.secondaryLabelKey()).toBe('testvalue');
      });
      it('should update secondaryLabelBold if the value changes', () => {
        componentRef.setInput('secondaryLabelBold', 'testvalue');
        fixture.detectChanges();
        expect(label.componentInstance.secondaryLabelBold()).toBe('testvalue');
      });
      it('should update secondaryLabelParantheses if the value changes', () => {
        componentRef.setInput('secondaryLabelParantheses', 'testvalue');
        fixture.detectChanges();

        expect(label.componentInstance.secondaryLabelParantheses()).toBe(
          'testvalue'
        );
      });
      it('should update secondaryLabelPostfixKey if the value changes', () => {
        componentRef.setInput('secondaryLabelPostfixKey', 'testvalue');
        fixture.detectChanges();

        expect(label.componentInstance.secondaryLabelPostfixKey()).toBe(
          'testvalue'
        );
      });
      it('should update secondaryLabelPrefixKey if the value changes', () => {
        componentRef.setInput('secondaryLabelPrefixKey', 'testvalue');
        fixture.detectChanges();

        expect(label.componentInstance.secondaryLabelPrefixKey()).toBe(
          'testvalue'
        );
      });
      it('should render isRequired as true if isRequired is true', () => {
        component.isRequired.set(true);
        fixture.detectChanges();

        expect(label.componentInstance.isRequired()).toBe(true);
      });
    });
  });
});
