import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditComponentDialogComponent } from './edit-component-dialog.component';
import { Component } from '@angular/core';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';
import { SoeFormGroup } from '@shared/extensions';
import { FormControl } from '@angular/forms';
import { ValidationHandler } from '@shared/handlers';
import { By } from '@angular/platform-browser';
import { CrudResponse } from '@shared/interfaces';

// Mock component for editComponent
@Component({
  selector: 'soe-mock-edit',
  template: '<div class="mock-edit-content">Mock Edit Component</div>',
  standalone: true,
})
class MockEditComponent {}

describe('EditComponentDialogComponent', () => {
  let component: EditComponentDialogComponent<any, any, any>;
  let fixture: ComponentFixture<EditComponentDialogComponent<any, any, any>>;
  let dialogRefMock: any;
  let mockForm: SoeFormGroup;
  let dataMock: any;

  beforeEach(() => {
    // Create a mock form
    mockForm = new SoeFormGroup({} as ValidationHandler, {
      testControl: new FormControl(''),
    });

    dialogRefMock = {
      disableClose: false,
      addPanelClass: vi.fn(),
      close: vi.fn(),
    };

    dataMock = {
      form: mockForm,
      editComponent: MockEditComponent,
      title: 'Test Dialog',
      size: 'md',
    };

    TestBed.configureTestingModule({
      imports: [SoftOneTestBed, EditComponentDialogComponent],
      providers: [
        { provide: MatDialogRef, useValue: dialogRefMock },
        { provide: MAT_DIALOG_DATA, useValue: dataMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EditComponentDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Setup', () => {
    it('should create the component', () => {
      expect(component).toBeDefined();
    });

    it('should initialize data from MAT_DIALOG_DATA', () => {
      expect(component.data).toBe(dataMock);
      expect(component.data.form).toBe(mockForm);
      expect(component.data.editComponent).toBe(MockEditComponent);
      expect(component.data.title).toBe('Test Dialog');
      expect(component.data.size).toBe('md');
    });

    it('should initialize dialogRef', () => {
      expect(component.dialogRef).toBeDefined();
      expect(component.dialogRef).toBe(dialogRefMock);
    });

    it('should initialize closeDialogSignal as undefined', () => {
      expect(component.closeDialogSignal()).toBeUndefined();
    });

    it('should initialize form from data', () => {
      expect(component.form).toBe(mockForm);
    });

    it('should initialize editComponent from data', () => {
      expect(component.editComponent).toBe(MockEditComponent);
    });

    it('should initialize editComponentInputs with form and closeDialogSignal', () => {
      expect(component.editComponentInputs).toBeDefined();
      expect(component.editComponentInputs.form).toBe(mockForm);
      expect(component.editComponentInputs.closeDialogSignal).toBe(
        component.closeDialogSignal
      );
    });
  });

  describe('Methods', () => {
    describe('ngOnInit', () => {
      it('should set disableContentScroll to true', () => {
        component.ngOnInit();
        expect(component.data.disableContentScroll).toBe(true);
      });
    });

    describe('effect - closeDialogSignal', () => {
      it('should not close dialog when closeDialogSignal is undefined', () => {
        component.closeDialogSignal.set(undefined);
        fixture.detectChanges();

        expect(dialogRefMock.close).not.toHaveBeenCalled();
      });

      it('should close dialog when closeDialogSignal is set', () => {
        const mockResponse: CrudResponse = {
          success: true,
          booleanValue: false,
          booleanValue2: false,
          canUserOverride: false,
          dateTimeValue: '',
          decimalValue: 0,
          integerValue: 0,
          integerValue2: 0,
          modified: '',
          objectsAffected: 0,
          successNumber: 0,
          infoMessage: '',
          stringValue: '',
          value: {},
          value2: {},
        };

        component.closeDialogSignal.set(mockResponse);
        fixture.detectChanges();

        expect(dialogRefMock.close).toHaveBeenCalledWith({
          response: mockResponse,
          value: mockForm.value,
        });
      });

      it('should include form value when closing dialog', () => {
        mockForm.patchValue({ testControl: 'test value' });
        const mockResponse: CrudResponse = {
          success: true,
          booleanValue: false,
          booleanValue2: false,
          canUserOverride: false,
          dateTimeValue: '',
          decimalValue: 0,
          integerValue: 0,
          integerValue2: 0,
          modified: '',
          objectsAffected: 0,
          successNumber: 0,
          infoMessage: '',
          stringValue: '',
          value: {},
          value2: {},
        };

        component.closeDialogSignal.set(mockResponse);
        fixture.detectChanges();

        expect(dialogRefMock.close).toHaveBeenCalledWith({
          response: mockResponse,
          value: {
            created: null,
            createdBy: null,
            isActive: true,
            modified: null,
            modifiedBy: null,
            state: 0,
            testControl: 'test value',
          },
        });
      });
    });
  });

  describe('DOM', () => {
    describe('soe-dialog', () => {
      it('should render soe-dialog component', () => {
        const dialogElement = fixture.debugElement.query(By.css('soe-dialog'));
        expect(dialogElement).toBeTruthy();
      });
    });

    describe('dialog-content', () => {
      it('should render dialog-content div', () => {
        const contentElement = fixture.debugElement.query(
          By.css('[dialog-content]')
        );
        expect(contentElement).toBeTruthy();
      });

      it('should apply content-container class when noToolbar is false', () => {
        dataMock.noToolbar = false;
        fixture.detectChanges();

        const contentElement = fixture.debugElement.query(
          By.css('[dialog-content]')
        );
        expect(
          contentElement.nativeElement.classList.contains('content-container')
        ).toBe(true);
      });

      it('should not apply content-container class when noToolbar is true', () => {
        dataMock.noToolbar = true;
        fixture.detectChanges();

        const contentElement = fixture.debugElement.query(
          By.css('[dialog-content]')
        );
        expect(
          contentElement.nativeElement.classList.contains('content-container')
        ).toBe(false);
      });
    });

    describe('ngComponentOutlet', () => {
      it('should render the editComponent via ngComponentOutlet', () => {
        const mockEditElement = fixture.debugElement.query(
          By.css('soe-mock-edit')
        );
        expect(mockEditElement).toBeTruthy();
      });

      it('should render mock edit component content', () => {
        const mockEditElement = fixture.debugElement.query(
          By.css('.mock-edit-content')
        );
        expect(mockEditElement).toBeTruthy();
        expect(mockEditElement.nativeElement.textContent).toBe(
          'Mock Edit Component'
        );
      });
    });
  });

  describe('Integration', () => {
    it('should pass correct inputs to dynamically loaded component', () => {
      // The component should receive form and closeDialogSignal
      expect(component.editComponentInputs.form).toBe(mockForm);
      expect(component.editComponentInputs.closeDialogSignal).toBe(
        component.closeDialogSignal
      );
    });

    it('should handle complete flow: init, set signal, close dialog', () => {
      // Initialize
      component.ngOnInit();
      expect(component.data.disableContentScroll).toBe(true);

      // Set close dialog signal
      const mockResponse: CrudResponse = {
        success: true,
        booleanValue: false,
        booleanValue2: false,
        canUserOverride: false,
        dateTimeValue: '',
        decimalValue: 0,
        integerValue: 0,
        integerValue2: 0,
        modified: '',
        objectsAffected: 0,
        successNumber: 0,
        infoMessage: '',
        stringValue: '',
        value: {},
        value2: {},
      };
      component.closeDialogSignal.set(mockResponse);
      fixture.detectChanges();

      // Verify dialog closed
      expect(dialogRefMock.close).toHaveBeenCalledWith({
        response: mockResponse,
        value: mockForm.value,
      });
    });
  });
});
