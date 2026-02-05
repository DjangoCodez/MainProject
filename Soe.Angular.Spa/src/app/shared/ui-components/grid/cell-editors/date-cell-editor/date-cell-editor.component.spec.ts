import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import {
  DateCellEditor,
  DateCellEditorParams,
} from './date-cell-editor.component';
import { SoeDateFormControl } from '@shared/extensions';
import { ICellEditorParams } from 'ag-grid-community';
import { AG_NODE_PROPS } from '@ui/grid/grid.component';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';
import { vi } from 'vitest';

describe('DateCellEditor', () => {
  let component: DateCellEditor<any>;
  let fixture: ComponentFixture<DateCellEditor<any>>;
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [DateCellEditor],
    });
    fixture = TestBed.createComponent(DateCellEditor);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Methods', () => {
    describe('getValue', () => {
      it('should return the current date when set', () => {
        const testDate = new Date(2022, 11, 25);
        component.date = testDate;
        expect(component.getValue()).toEqual(testDate);
      });

      it('should return undefined when date is not set', () => {
        component.date = undefined;
        expect(component.getValue()).toBeUndefined();
      });
    });

    describe('agInit', () => {
      it('should initialize params and set the date correctly', () => {
        const params = {
          value: '2022-12-25',
          disabled: false,
        } as unknown as DateCellEditorParams<any>;
        component.agInit(params);
        expect(component.params).toEqual(params);
        expect(component.date).toEqual(new Date('2022-12-25'));
        expect(component.localControl.value).toEqual(new Date('2022-12-25'));
      });

      it('should handle undefined date value gracefully', () => {
        const params = {
          value: undefined,
        } as DateCellEditorParams<any>;
        component.agInit(params);
        expect(component.params).toEqual(params);
        expect(component.date).toBeUndefined();
      });
    });

    describe('isCancelBeforeStart', () => {
      it('should return true if params.disabled is true', () => {
        component.params = { disabled: true } as any;
        expect(component.isCancelBeforeStart()).toBe(true);
      });

      it('should return false if params.disabled is false', () => {
        component.params = { disabled: false } as any;
        expect(component.isCancelBeforeStart()).toBe(false);
      });

      it('should call disabled function with params.data if it is a function', () => {
        const mockDisabledFn = vi.fn().mockReturnValue(true);
        component.params = { disabled: mockDisabledFn, data: { id: 1 } } as any;
        expect(component.isCancelBeforeStart()).toBe(true);
        expect(mockDisabledFn).toHaveBeenCalledWith({ id: 1 });
      });

      it('should return false if params.disabled is not set', () => {
        component.params = {} as any;
        expect(component.isCancelBeforeStart()).toBe(false);
      });
    });

    describe('isCancelAfterEnd', () => {
      it('should always return false', () => {
        expect(component.isCancelAfterEnd()).toBe(false);
      });
    });

    describe('isPopup', () => {
      it('should always return true', () => {
        expect(component.isPopup()).toBe(true);
      });
    });

    describe('onCalendarClosed', () => {
      it('should call params.stopEditing after timeout when date is set', () => {
        vi.useFakeTimers();
        const stopEditingSpy = vi.fn();
        component.params = { stopEditing: stopEditingSpy } as any;
        component.date = new Date(2022, 11, 25);

        component.onCalendarClosed();

        vi.advanceTimersByTime(100);
        expect(stopEditingSpy).toHaveBeenCalled();
        vi.useRealTimers();
      });

      it('should not call params.stopEditing when date is not set', () => {
        vi.useFakeTimers();
        const stopEditingSpy = vi.fn();
        component.params = { stopEditing: stopEditingSpy } as any;
        component.date = undefined;

        component.onCalendarClosed();

        vi.advanceTimersByTime(100);
        expect(stopEditingSpy).not.toHaveBeenCalled();
        vi.useRealTimers();
      });
    });

    describe('onDateChanged', () => {
      it('should update the date to the changed value', () => {
        const newDate = new Date(2022, 11, 25);
        component.onDateChanged(newDate);
        expect(component.date).toEqual(newDate);
      });

      it('should set the date to undefined if no value is provided', () => {
        component.onDateChanged(undefined);
        expect(component.date).toBeUndefined();
      });
    });
  });
  describe('DOM', () => {
    describe('soe-datepicker', () => {
      let datePickerDebug: DebugElement;
      let datePickerInstance: any;

      beforeEach(() => {
        datePickerDebug = fixture.debugElement.query(By.css('soe-datepicker'));
        datePickerInstance = datePickerDebug.componentInstance;
      });

      it('should bind [labelKey] to "DatePicker"', () => {
        const labelKey = datePickerInstance.labelKey();
        expect(labelKey).toBe('DatePicker');
      });

      it('should bind [gridMode] to true', () => {
        const gridMode = datePickerInstance.gridMode();
        expect(gridMode).toBe(true);
      });

      it('should bind [formControl] to localControl', () => {
        const formControlValue = component.localControl.value;
        expect(formControlValue).toBeNull(); // Initial value
      });

      it('should emit (valueChanged) and update the date', () => {
        vi.spyOn(component, 'onDateChanged');

        const testDate = new Date(2022, 11, 25);
        datePickerDebug.triggerEventHandler('valueChanged', testDate);

        fixture.detectChanges();

        expect(component.onDateChanged).toHaveBeenCalled();
        expect(component.date).toEqual(testDate);
      });
    });
  });
});
