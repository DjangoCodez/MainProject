import {
  ComponentFixture,
  fakeAsync,
  TestBed,
  tick,
} from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { RadioComponent } from './radio.component';
import { ComponentRef, DebugElement } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('RadioComponent', () => {
  let component: RadioComponent<string>;
  let fixture: ComponentFixture<RadioComponent<string>>;
  let componentRef: ComponentRef<RadioComponent<string>>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed, ReactiveFormsModule, RadioComponent],
    });
    fixture = TestBed.createComponent(RadioComponent<string>);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;

    componentRef.setInput('group', 'test'); // Required input

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('component', () => {
    describe('setup', () => {
      it('should initalize input properties', () => {
        expect(component.inputId()).toBeTruthy();
        expect(component.labelKey()).toBe('');
        expect(component.inline()).toBe(false);
        expect(component.noMargin()).toBe(false);
        expect(component.group()).toBe('test');
      });
    });
  });
  describe('methods', () => {
    describe('onValueChange', () => {
      it('should emit valueChanged event on value change', () => {
        vi.spyOn(component.valueChanged, 'emit');
        const mockEvent = new Event('change');
        component.onValueChange(mockEvent);
        expect(component.valueChanged.emit).toHaveBeenCalledWith(
          component.value()
        );
      });
    });
  });
  describe('DOM', () => {
    describe('form-check', () => {
      let formCheckElement: DebugElement;
      beforeEach(() => {
        formCheckElement = fixture.debugElement.query(By.css('.form-check'));
      });
      it('should initialize with correct properties', () => {
        expect(formCheckElement).toBeTruthy();
        const classList = formCheckElement.nativeElement.classList;
        const style = formCheckElement.nativeElement.style;
        expect(classList.contains('form-check-inline')).toBe(false);
        expect(classList.contains('me-3')).toBe(false);
        expect(classList.contains('mt-2')).toBe(true);
        expect(style.marginTop).toBe('');
      });
      it('should change classes if inline is true', () => {
        componentRef.setInput('inline', true);
        componentRef.setInput('noMargin', true);
        fixture.detectChanges();
        const classList = formCheckElement.nativeElement.classList;
        expect(classList.contains('form-check-inline')).toBe(true);
        expect(classList.contains('me-3')).toBe(true);
        expect(classList.contains('mt-2')).toBe(false);
      });
      it('should change margin if inline is true', () => {
        const style = formCheckElement.nativeElement.style;
        componentRef.setInput('inline', true);
        fixture.detectChanges();
        expect(style.marginTop).toBe('2.2rem');
      });
    });
    describe('input', () => {
      let inputElement: DebugElement;
      beforeEach(fakeAsync(() => {
        fixture.detectChanges();
        inputElement = fixture.debugElement.query(By.css('.form-check-input'));
      }));
      it('should set the name attribute correctly when group input is set', () => {
        componentRef.setInput('group', 'test');
        fixture.detectChanges();

        fixture.whenStable().then(() => {
          // Fixes sync issue with name -> group()
          fixture.detectChanges();
          expect(inputElement).toBeTruthy();
          expect(inputElement.nativeElement.id).toBe(component.inputId());
          expect(inputElement.nativeElement.name).toBe('test');
          expect(inputElement.nativeElement.value).toBe(
            component.control.value
          );
        });
      });
      it('should emit valueChanged event on change', () => {
        const event = new Event('change');
        vi.spyOn(component, 'onValueChange');
        inputElement.triggerEventHandler('change', event);
        expect(component.onValueChange).toHaveBeenCalledWith(event);
      });
    });
    describe('formcheck-label', () => {
      let formchecklabelElement: DebugElement;
      beforeEach(() => {
        formchecklabelElement = fixture.debugElement.query(
          By.css('.form-check-label')
        );
      });
      it('should initialize with correct values', () => {
        expect(formchecklabelElement).toBeTruthy();
        expect(formchecklabelElement.nativeElement.getAttribute('for')).toBe(
          component.inputId()
        );
      });
    });
  });
});
