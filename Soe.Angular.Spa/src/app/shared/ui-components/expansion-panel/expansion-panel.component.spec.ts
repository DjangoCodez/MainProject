import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExpansionPanelComponent } from './expansion-panel.component';
import {
  ComponentRef,
  NO_ERRORS_SCHEMA,
  CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { vi } from 'vitest';
import { MatExpansionPanel } from '@angular/material/expansion';
import { By } from '@angular/platform-browser';

describe('ExpansionPanelComponent', () => {
  let component: ExpansionPanelComponent;
  let fixture: ComponentFixture<ExpansionPanelComponent>;
  let componentRef: ComponentRef<ExpansionPanelComponent>;

  let panel: any;
  let label: any;
  let labelComponent: any;
  let description: any;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ExpansionPanelComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
    });

    fixture = TestBed.createComponent(ExpansionPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    componentRef = fixture.componentRef;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should setup component properties with default values', () => {
      expect(component.labelKey()).toBe('');
      expect(component.secondaryLabelKey()).toBe('');
      expect(component.secondaryLabelBold()).toBe(false);
      expect(component.secondaryLabelParantheses()).toBe(true);
      expect(component.secondaryLabelPrefixKey()).toBe('');
      expect(component.secondaryLabelPostfixKey()).toBe('');
      expect(component.description()).toBe('');
      expect(component.disabled()).toBe(false);
      expect(component.open()).toBe(false);
      expect(component.static()).toBe(false);
      expect(component.noMargin()).toBe(false);
      expect(component.noPadding()).toBe(false);
      expect(component.doPadding()).toBe(false);
      expect(component.noTopPadding()).toBe(false);
      expect(component.addTopMargin()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('onOpened', () => {
      it('should call onOpened when onOpened is called', () => {
        vi.spyOn(component, 'onOpened');
        component.onOpened();
        fixture.detectChanges();
        expect(component.onOpened).toHaveBeenCalled();
      });
    });
    describe('onClosed', () => {
      it('should call onClosed when onClosed is called', () => {
        vi.spyOn(component, 'onClosed');
        component.onClosed();
        expect(component.onClosed).toHaveBeenCalled();
      });
    });
  });
  describe('DOM', () => {
    describe('panel', () => {
      beforeEach(() => {
        panel = fixture.nativeElement.querySelector('mat-expansion-panel');
      });
      it('should toggle expanded', () => {
        componentRef.setInput('open', true);
        fixture.detectChanges();
        expect(panel.classList.contains('mat-expanded')).toBe(true);

        componentRef.setInput('open', false);
        fixture.detectChanges();
        expect(panel.classList.contains('mat-expanded')).toBe(false);
      });
      it('should toggle disabled', () => {
        const panelComponent = fixture.debugElement.query(
          By.directive(MatExpansionPanel)
        ).componentInstance;

        componentRef.setInput('disabled', true);
        fixture.detectChanges();
        expect(panelComponent.disabled).toBe(true);

        componentRef.setInput('disabled', false);
        fixture.detectChanges();
        expect(panelComponent.disabled).toBe(false);
      });
      it('should toggle hideToggle', () => {
        const panelComponent = fixture.debugElement.query(
          By.directive(MatExpansionPanel)
        ).componentInstance;

        componentRef.setInput('static', true);
        fixture.detectChanges();
        expect(panelComponent.disabled).toBe(true);

        componentRef.setInput('static', false);
        fixture.detectChanges();
        expect(panelComponent.disabled).toBe(false);
      });
      it('should create no-margin class', () => {
        componentRef.setInput('noMargin', true);
        fixture.detectChanges();
        expect(
          fixture.nativeElement.querySelector('mat-expansion-panel.no-margin')
        ).toBeTruthy();

        componentRef.setInput('noMargin', false);
        fixture.detectChanges();
        expect(
          fixture.nativeElement.querySelector('mat-expansion-panel.no-margin')
        ).toBeFalsy();
      });
      it('should create no-padding class', () => {
        componentRef.setInput('noPadding', true);
        fixture.detectChanges();
        expect(
          fixture.nativeElement.querySelector('mat-expansion-panel.no-padding')
        ).toBeTruthy();

        componentRef.setInput('noPadding', false);
        fixture.detectChanges();
        expect(
          fixture.nativeElement.querySelector('mat-expansion-panel.no-padding')
        ).toBeFalsy();
      });
      it('should create do-padding class', () => {
        componentRef.setInput('doPadding', true);
        fixture.detectChanges();
        expect(
          fixture.nativeElement.querySelector('mat-expansion-panel.do-padding')
        ).toBeTruthy();

        componentRef.setInput('doPadding', false);
        fixture.detectChanges();
        expect(
          fixture.nativeElement.querySelector('mat-expansion-panel.do-padding')
        ).toBeFalsy();
      });
      it('should create no-top-padding class', () => {
        componentRef.setInput('noTopPadding', true);
        fixture.detectChanges();
        expect(
          fixture.nativeElement.querySelector(
            'mat-expansion-panel.no-top-padding'
          )
        ).toBeTruthy();

        componentRef.setInput('noTopPadding', false);
        fixture.detectChanges();
        expect(
          fixture.nativeElement.querySelector(
            'mat-expansion-panel.no-top-padding'
          )
        ).toBeFalsy();
      });
    });
    describe('label', () => {
      beforeEach(() => {
        label = fixture.debugElement.query(By.css('soe-label'));
        labelComponent = label.componentInstance;
      });
      it('should update labelKey', () => {
        componentRef.setInput('labelKey', 'test');
        fixture.detectChanges();
        expect(labelComponent.labelKey()).toBe('test');
      });
      it('should update secondaryLabelKey', () => {
        componentRef.setInput('secondaryLabelKey', 'test');
        fixture.detectChanges();
        expect(labelComponent.secondaryLabelKey()).toBe('test');
      });
      it('should update secondaryLabelBold', () => {
        componentRef.setInput('secondaryLabelBold', true);
        fixture.detectChanges();
        expect(labelComponent.secondaryLabelBold()).toBe(true);

        componentRef.setInput('secondaryLabelBold', false);
        fixture.detectChanges();
        expect(labelComponent.secondaryLabelBold()).toBe(false);
      });
      it('should update secondaryLabelParantheses', () => {
        componentRef.setInput('secondaryLabelParantheses', true);
        fixture.detectChanges();
        expect(labelComponent.secondaryLabelParantheses()).toBe(true);

        componentRef.setInput('secondaryLabelParantheses', false);
        fixture.detectChanges();
        expect(labelComponent.secondaryLabelParantheses()).toBe(false);
      });
      it('should update secondaryLabelPrefixKey', () => {
        componentRef.setInput('secondaryLabelPrefixKey', 'test');
        fixture.detectChanges();
        expect(labelComponent.secondaryLabelPrefixKey()).toBe('test');
      });
      it('should update secondaryLabelPostfixKey', () => {
        componentRef.setInput('secondaryLabelPostfixKey', 'test');
        fixture.detectChanges();
        expect(labelComponent.secondaryLabelPostfixKey()).toBe('test');
      });
    });
    describe('description', () => {
      beforeEach(() => {
        description = fixture.nativeElement.querySelector(
          'mat-panel-description'
        );
      });
      it('should update description', () => {
        componentRef.setInput('description', 'test');
        fixture.detectChanges();
        expect(description.textContent).toBe(' test ');
      });
    });
  });
});
