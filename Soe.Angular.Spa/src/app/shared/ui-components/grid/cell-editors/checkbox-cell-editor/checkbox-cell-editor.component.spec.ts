import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { CheckboxCellEditor } from './checkbox-cell-editor.component';
import { SoeDateFormControl } from '@shared/extensions';
import { ICellEditorParams } from 'ag-grid-community';
import { AG_NODE_PROPS } from '@ui/grid/grid.component';
import { By } from '@angular/platform-browser';
import { DebugElement, ElementRef, Signal } from '@angular/core';
import { FormControl } from '@angular/forms';
import { before } from 'lodash';
import { vi } from 'vitest';

describe('CheckboxCellEditor', () => {
  let component: CheckboxCellEditor<any>;
  let fixture: ComponentFixture<CheckboxCellEditor<any>>;
  let mockParams: any;
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [CheckboxCellEditor],
    });
    fixture = TestBed.createComponent(CheckboxCellEditor);
    component = fixture.componentInstance;
    fixture.detectChanges();

    mockParams = {
      value: false,
      data: { someProp: true },
      showCheckbox: true,
      disabled: false,
      onClick: vi.fn(),
    };
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('methods', () => {
    describe('agInit', () => {
      it('should initialize control and params correctly', () => {
        component.agInit(mockParams);

        expect(component.control.value).toBe(false);
        expect(component.params).toEqual(mockParams);
      });

      it('should evaluate showCheckbox as true if a function returns true', () => {
        mockParams.showCheckbox = (data: any) => data.someProp;
        component.agInit(mockParams);

        expect(component.showCheckbox()).toBe(true);
      });

      it('should evaluate showCheckbox as false if a function returns false', () => {
        mockParams.showCheckbox = (data: any) => !data.someProp;
        component.agInit(mockParams);

        expect(component.showCheckbox()).toBe(false);
      });

      it('should set showCheckbox to true if showCheckbox is not specified', () => {
        delete mockParams.showCheckbox;
        component.agInit(mockParams);

        expect(component.showCheckbox()).toBe(true);
      });
    });
    describe('afterGuiAttached', () => {
      it('should focus on the checkbox input element', () => {
        const mockFocus = vi.fn();
        const mockSignal = vi.fn(() => ({
          nativeElement: { focus: mockFocus },
        }));
        (mockSignal as any)[Symbol.for('ɵSIGNAL')] = true;
        component.input = mockSignal as unknown as Signal<ElementRef<any>>;

        component.afterGuiAttached();

        expect(mockFocus).toHaveBeenCalled();
      });
    });
    describe('getValue', () => {
      it('should return the current value when set', () => {
        component.control = new FormControl();
        component.control.setValue(true);
        expect(component.getValue()).toEqual(true);
      });
    });
    describe('isPopup', () => {
      it('should return false', () => {
        expect(component.isPopup!()).toBe(false);
      });
    });

    describe('getGui', () => {
      it('should return the input element reference', () => {
        const input = component.input();

        expect(component.getGui()).toBe(input);
      });
    });

    describe('isCancelBeforeStart', () => {
      it('should return true if params.disabled is boolean and true', () => {
        mockParams.disabled = true;
        component.agInit(mockParams);

        expect(component.isCancelBeforeStart!()).toBe(true);
      });

      it('should evaluate params.disabled function and return true if it returns true', () => {
        mockParams.disabled = vi.fn().mockReturnValue(true);
        component.agInit(mockParams);

        expect(component.isCancelBeforeStart!()).toBe(true);
      });

      it('should return false if params.disabled is not specified', () => {
        delete mockParams.disabled;
        component.agInit(mockParams);

        expect(component.isCancelBeforeStart!()).toBe(false);
      });
    });

    describe('isCancelAfterEnd', () => {
      it('should always return false', () => {
        expect(component.isCancelAfterEnd!()).toBe(false);
      });
    });

    describe('onChange', () => {
      it('should call params.onClick with the control value and params data', () => {
        component.agInit(mockParams);
        component.control.setValue(true);

        component.onChange();

        expect(mockParams.onClick).toHaveBeenCalledWith(true, mockParams.data);
      });

      it('should not throw if params.onClick is not defined', () => {
        delete mockParams.onClick;
        component.agInit(mockParams);
        component.control.setValue(true);

        expect(() => component.onChange()).not.toThrow();
      });
    });
  });
  describe('DOM', () => {
    beforeEach(() => {
      component.agInit(mockParams);
    });
    it('should bind the `checked` attribute to `control.value`', () => {
      component.showCheckbox.set(true); // Signal returns true
      component.control = new FormControl(true);

      fixture.detectChanges(); // Trigger DOM update

      const checkbox = fixture.debugElement.query(
        By.css('input[type="checkbox"]')
      ).nativeElement;
      expect(checkbox.checked).toBe(true);

      // Update the control value
      component.control.setValue(false);
      fixture.detectChanges(); // Trigger DOM update

      expect(checkbox.checked).toBe(false);
    });

    it('should call `onChange` when the checkbox value changes', () => {
      component.showCheckbox.set(true);
      component.control = new FormControl(true);
      fixture.detectChanges();

      component.control.setValue(false);
      component.onChange();

      expect(component.params.onClick).toHaveBeenCalledWith(
        false,
        component.params.data
      );
    });
  });
});
