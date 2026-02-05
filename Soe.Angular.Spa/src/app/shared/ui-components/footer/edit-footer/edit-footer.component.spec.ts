import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditFooterComponent } from './edit-footer.component';
import { ComponentRef, DebugElement } from '@angular/core';
import { SoeFormGroup } from '@shared/extensions';
import { FormControl, Validators } from '@angular/forms';
import { ValidationHandler } from '@shared/handlers';
import { TranslateService } from '@ngx-translate/core';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('EditFooterComponent', () => {
  let component: EditFooterComponent;
  let fixture: ComponentFixture<EditFooterComponent>;
  let translateService: TranslateService;
  let messageboxService: MessageboxService;
  let componentRef: ComponentRef<EditFooterComponent>;

  beforeEach(async () => {
    // translateService = {
    //   instant: vi.fn(),
    //   translate: vi.fn(),
    // } as unknown as TranslateService;

    messageboxService = {
      show: vi.fn(),
    } as unknown as MessageboxService;

    TestBed.configureTestingModule({
      imports: [SoftOneTestBed, EditFooterComponent],
      providers: [
        { provide: TranslateService },
        { provide: MessageboxService, useValue: messageboxService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EditFooterComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;

    translateService = TestBed.inject(TranslateService);

    const formGroup = new SoeFormGroup(
      new ValidationHandler(translateService, messageboxService),
      {
        controlName1: new FormControl('', Validators.required),
        controlName2: new FormControl('', Validators.minLength(5)),
      }
    );
    componentRef.setInput('form', formGroup);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('component', () => {
    describe('setup', () => {
      it('should initalize with default values', () => {
        expect(component.modifyPermission()).toBe(false);
        expect(component.idFieldName()).toBe('');
        expect(component.showActive()).toBe(false);
        expect(component.showCancel()).toBe(false);
        expect(component.hideDelete()).toBe(false);
        expect(component.hideSave()).toBe(false);
        expect(component.inProgress()).toBe(false);
      });
    });
  });

  describe('methods', () => {
    describe('onActiveChanged', () => {
      it('should emit activeChanged event', () => {
        vi.spyOn(component.activeChanged, 'emit');
        component.onActiveChanged(true);
        expect(component.activeChanged.emit).toHaveBeenCalledWith(true);
      });
    });
    describe('onCancelled', () => {
      it('should emit cancelled event', () => {
        vi.spyOn(component.cancelled, 'emit');
        component.onCancelled();
        expect(component.cancelled.emit).toHaveBeenCalled();
      });
    });
    describe('onDeleted', () => {
      it('should emit deleted event', () => {
        vi.spyOn(component.deleted, 'emit');
        component.onDeleted();
        expect(component.deleted.emit).toHaveBeenCalled();
      });
    });
    describe('onSaved', () => {
      it('should emit saved event', () => {
        vi.spyOn(component.saved, 'emit');
        component.onSaved();
        expect(component.saved.emit).toHaveBeenCalled();
      });
    });
  });

  describe('DOM', () => {
    describe('soe-checkbox', () => {
      let soeCheckbox: any;
      beforeEach(() => {
        componentRef.setInput('showActive', true);
        fixture.detectChanges();
        soeCheckbox = fixture.debugElement.query(By.css('soe-checkbox'));
      });

      it('should render soe-checkbox if showActive is set to true', () => {
        expect(soeCheckbox).toBeTruthy();
      });
      it('should not render soe-checkbox if showActive is set to false', () => {
        componentRef.setInput('showActive', false);
        fixture.detectChanges();

        soeCheckbox = fixture.debugElement.query(By.css('soe-checkbox'));

        expect(soeCheckbox).toBeFalsy();
      });
      it('should call onActiveChanged when soe-checkbox emits change', () => {
        vi.spyOn(component, 'onActiveChanged');

        soeCheckbox.triggerEventHandler('valueChanged', true);

        expect(component.onActiveChanged).toHaveBeenCalledWith(true);
      });
    });

    describe('soe-buttons', () => {
      describe('cancel button', () => {
        let cancelButton: DebugElement;
        beforeEach(() => {
          componentRef.setInput('showCancel', true);
          fixture.detectChanges();
          cancelButton = fixture.debugElement.query(By.css('soe-button'));
        });
        it('should render the cancel button', () => {
          expect(cancelButton).toBeTruthy();
        });
        it('should not render the cancel button if showCancel is set to false', () => {
          componentRef.setInput('showCancel', false);
          fixture.detectChanges();
          cancelButton = fixture.debugElement.query(By.css('soe-button'));
          expect(cancelButton).toBeFalsy();
        });
        it('should call onCancelled when action is emitted', () => {
          vi.spyOn(component, 'onCancelled');
          cancelButton.triggerEventHandler('action', null);
          expect(component.onCancelled).toHaveBeenCalled();
        });
      });
      describe('delete button', () => {
        let deleteButton: DebugElement;
        beforeEach(() => {
          componentRef.setInput('hideDelete', false);
          componentRef.setInput('idFieldName', 'controlName1');
          component.form().patchValue({ controlName1: 1 });
          fixture.detectChanges();
          deleteButton = fixture.debugElement.query(
            By.css('soe-delete-button')
          );
        });
        it('should render with correct properties', () => {
          expect(deleteButton).toBeTruthy();
          expect(deleteButton.componentInstance.disabled()).toBe(
            !component.modifyPermission()
          );
        });
        it('should not render if hideDelete is set to true', () => {
          componentRef.setInput('hideDelete', true);
          fixture.detectChanges();

          deleteButton = fixture.debugElement.query(By.css('soe-button'));

          expect(deleteButton).toBeFalsy();
        });
        it('should be enabled if modifyPermission is true', () => {
          componentRef.setInput('modifyPermission', true);
          fixture.detectChanges();

          expect(deleteButton).toBeTruthy();
          expect(deleteButton.componentInstance.disabled()).toBe(false);
        });
        it('should call onDeleted when action is emitted', () => {
          vi.spyOn(component, 'onDeleted');

          deleteButton.triggerEventHandler('action', null);

          expect(component.onDeleted).toHaveBeenCalled();
        });
      });
      describe('save buttons', () => {
        beforeEach(() => {
          componentRef.setInput('hideSave', false);
          fixture.detectChanges();
        });
        describe('menu button', () => {
          let menuButton: DebugElement;
          beforeEach(() => {
            vi.spyOn(component, 'saveMenuList').mockReturnValue([{}, {}]);
            fixture.detectChanges();
            menuButton = fixture.debugElement.query(By.css('soe-menu-button'));
          });
          it('should render with correct properties when saveMenuList has items', () => {
            expect(menuButton).toBeTruthy();
            expect(menuButton.componentInstance.disabled()).toBe(
              component.saveIsDisabled
            );
            expect(menuButton.componentInstance.list()).toBe(
              component.saveMenuList()
            );
            expect(menuButton.componentInstance.dropUp()).toBe(
              component.saveMenuDropUp()
            );
            expect(menuButton.componentInstance.dropLeft()).toBe(
              component.saveMenuDropLeft()
            );
          });
          it('should not render when saveMenuList is empty', () => {
            vi.spyOn(component, 'saveMenuList').mockReturnValue([]);
            fixture.detectChanges();

            menuButton = fixture.debugElement.query(By.css('soe-menu-button'));

            expect(menuButton).toBeFalsy();
          });
          it('should call onSaveMenuListItemSelected when itemSelected is emitted', () => {
            vi.spyOn(component, 'onSaveMenuListItemSelected');

            menuButton.triggerEventHandler('itemSelected', {});

            expect(component.onSaveMenuListItemSelected).toHaveBeenCalledWith(
              {}
            );
          });
          it('should call saveMenuDefaultItem when itemSelected is emitted', () => {
            vi.spyOn(component, 'saveMenuDefaultItem');

            menuButton.triggerEventHandler('itemSelected', null);

            expect(component.saveMenuDefaultItem).toHaveBeenCalled();
          });
        });
        describe('save button', () => {
          let saveButton: DebugElement;
          beforeEach(() => {
            saveButton = fixture.debugElement.query(By.css('soe-save-button'));
          });
          it('should render save button with correct properties', () => {
            expect(saveButton).toBeTruthy();
            expect(saveButton.componentInstance.inProgress()).toBe(
              component.inProgress()
            );
            expect(saveButton.componentInstance.disabled()).toBe(
              component.saveIsDisabled
            );
            expect(saveButton.componentInstance.invalid()).toBe(
              component.form().invalid
            );
            expect(saveButton.componentInstance.dirty()).toBe(
              component.form().dirty
            );
          });
          it('should change inProgress according to its property', () => {
            componentRef.setInput('inProgress', true);
            fixture.detectChanges();

            expect(saveButton).toBeTruthy();
            expect(saveButton.componentInstance.inProgress()).toBe(true);
          });
          it('should set invalid and dirty to true', () => {
            component.form().markAllAsTouched();
            component.form().markAsDirty();
            fixture.detectChanges();

            expect(saveButton).toBeTruthy();
            expect(saveButton.componentInstance.invalid()).toBe(true);
            expect(saveButton.componentInstance.dirty()).toBe(true);
          });
          it('should be enabled if saveIsDisabled is false', () => {
            vi.spyOn(component, 'saveIsDisabled', 'get').mockReturnValue(false);
            fixture.detectChanges();

            expect(saveButton).toBeTruthy();
            expect(saveButton.componentInstance.disabled()).toBe(false);
          });
          it('should call onSaved when action is emitted', () => {
            vi.spyOn(component, 'onSaved');

            saveButton.triggerEventHandler('action', null);

            expect(component.onSaved).toHaveBeenCalled();
          });
          it('should call openFormValidationErrors when validationErrorsAction is emitted', () => {
            vi.spyOn(component.form(), 'openFormValidationErrors');

            saveButton.triggerEventHandler('validationErrorsAction', null);

            expect(
              component.form().openFormValidationErrors
            ).toHaveBeenCalled();
          });
        });
      });
    });
  });
});
