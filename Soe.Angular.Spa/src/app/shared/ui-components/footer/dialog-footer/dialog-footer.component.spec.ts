import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DialogFooterComponent } from './dialog-footer.component';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TranslateService } from '@ngx-translate/core';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import {
  ComponentRef,
  CUSTOM_ELEMENTS_SCHEMA,
  NO_ERRORS_SCHEMA,
} from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('DialogFooterComponent', () => {
  let component: DialogFooterComponent;
  let fixture: ComponentFixture<DialogFooterComponent>;
  let componentRef: ComponentRef<DialogFooterComponent>;
  let mockTranslateService: TranslateService;
  let mockMessageboxService: MessageboxService;
  let mockValidationHandler: ValidationHandler;

  beforeEach(() => {
    mockTranslateService = {
      instant: vi.fn(key => key), // Mocking the `instant` method to return the key itself
    } as unknown as TranslateService;

    mockMessageboxService = {} as MessageboxService; // Mock an empty object for simplicity

    mockValidationHandler = new ValidationHandler(
      mockTranslateService,
      mockMessageboxService
    );

    TestBed.configureTestingModule({
      imports: [DialogFooterComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogFooterComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;

    const mockForm = new SoeFormGroup(
      mockValidationHandler,
      {}, // Pass an empty object for form controls
      {} // Pass an empty object for additional configuration
    );

    componentRef.setInput('form', mockForm);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('component should have default values', () => {
      expect(component.hideCancel()).toBe(false);
      expect(component.cancelLabelKey()).toBe('core.cancel');
      expect(component.hideOk()).toBe(false);
      expect(component.okLabelKey()).toBe('core.ok');
      expect(component.okDisabled()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('onCancelled', () => {
      it('should emit the cancelled event', () => {
        const spy = vi.spyOn(component.cancelled, 'emit');
        component.onCancelled();
        expect(spy).toHaveBeenCalled();
      });
    });
    describe('onCommitted', () => {
      it('should emit the committed event', () => {
        const spy = vi.spyOn(component.committed, 'emit');
        component.onCommitted();
        expect(spy).toHaveBeenCalled();
      });
    });
  });
  describe('DOM', () => {
    describe('formgroup', () => {
      it('should render the formgroup', () => {
        const formGroup = fixture.nativeElement.querySelector(
          '.d-flex.justify-content-between'
        );
        expect(formGroup).toBeTruthy();
      });
    });
    describe('cancel button', () => {
      let cancelButton: any;
      beforeEach(() => {
        cancelButton = fixture.debugElement.query(By.css('soe-button'));
      });
      it('should render the cancel button', () => {
        expect(cancelButton).toBeTruthy();
      });
      it('should not render if hideCancel is true', () => {
        componentRef.setInput('hideCancel', true);
        fixture.detectChanges();
        cancelButton = fixture.debugElement.query(By.css('soe-button'));
        expect(cancelButton).toBeFalsy();
      });
      it('should have the correct attributes', () => {
        expect(cancelButton.componentInstance.caption()).toBe(
          component.cancelLabelKey()
        );
        expect(cancelButton.componentInstance.tooltip()).toBe(
          component.cancelLabelKey()
        );
      });
      it('should emit onCancelled on action', () => {
        const spy = vi.spyOn(component, 'onCancelled');
        cancelButton.triggerEventHandler('action', {});
        expect(spy).toHaveBeenCalled();
      });
    });

    describe('ok button', () => {
      let saveButton: any;
      let saveButtonInstance: any;
      beforeEach(() => {
        saveButton = fixture.debugElement.query(By.css('soe-save-button'));
        saveButtonInstance = saveButton.componentInstance;
      });
      it('should render both buttons', () => {
        expect(saveButton).toBeTruthy();
      });
      it('should not render if hideOk is true', () => {
        componentRef.setInput('hideOk', true);
        fixture.detectChanges();
        saveButton = fixture.debugElement.query(By.css('soe-save-button'));
        expect(saveButton).toBeFalsy();
      });
      it('should have the correct attributes', () => {
        expect(saveButton.componentInstance.caption()).toBe(
          component.okLabelKey()
        );
        expect(saveButton.componentInstance.tooltip()).toBe(
          component.okLabelKey()
        );
        expect(saveButton.componentInstance.disabled()).toBe(
          component.okDisabled()
        );
        expect(saveButton.componentInstance.invalid()).toBe(
          component.form().invalid
        );
        expect(saveButton.componentInstance.dirty()).toBe(
          component.form().dirty
        );
      });
      it('should emit onCommitted on action', () => {
        const spy = vi.spyOn(component, 'onCommitted');
        saveButton.triggerEventHandler('action', {});
        expect(spy).toHaveBeenCalled();
      });
      it('should emit onValidationErrorsAction on validationErrorsAction', () => {
        const spy = vi.spyOn(component.form(), 'openFormValidationErrors');
        saveButton.triggerEventHandler('validationErrorsAction', {});
        expect(spy).toHaveBeenCalled();
      });
    });
  });
});
