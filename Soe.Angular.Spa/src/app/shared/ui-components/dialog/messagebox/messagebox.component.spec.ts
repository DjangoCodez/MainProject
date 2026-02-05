import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MessageboxComponent } from './messagebox.component';
import { ToastrService } from 'ngx-toastr';
import {
  TranslateService,
  TranslateModule,
  TranslatePipe,
  TranslateLoader,
} from '@ngx-translate/core';
import {
  FontAwesomeModule,
  FaIconLibrary,
  FaIconComponent,
} from '@fortawesome/angular-fontawesome';
import { fal } from '@fortawesome/pro-light-svg-icons'; // Import the required icons
import { far } from '@fortawesome/pro-regular-svg-icons'; // Import the required icons
import { fas } from '@fortawesome/pro-solid-svg-icons'; // Import the required icons
import { of } from 'rxjs';
import { MatDialogRef } from '@angular/material/dialog';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';
import { ButtonComponent } from '@ui/button/button/button.component';

// Mock translation loader
export class FakeLoader implements TranslateLoader {
  getTranslation(lang: string) {
    return of({
      'core.ok': 'ok',
      'core.yes': 'yes',
      'core.no': 'no',
      'core.cancel': 'cancel',
    });
  }
}

describe('MessageboxComponent', () => {
  let component: MessageboxComponent<any>;
  let fixture: ComponentFixture<MessageboxComponent<any>>;
  let toastrService: ToastrService;
  // let mockTranslateService: TranslateService;
  let library: FaIconLibrary;
  let mockDialogRef: MatDialogRef<MessageboxComponent<any>>;

  beforeEach(async () => {
    toastrService = {
      info: vi.fn(),
      warning: vi.fn(),
    } as unknown as ToastrService;

    // mockTranslateService = {
    //   instant: vi.fn((key: string) => key),
    //   get: vi.fn((keys: string | string[]) => {
    //     const result: any = {};
    //     if (Array.isArray(keys)) {
    //       keys.forEach(key => (result[key] = key));
    //     } else {
    //       result[keys] = keys;
    //     }
    //     return of(result);
    //   }),
    // } as unknown as TranslateService;

    mockDialogRef = {
      afterClosed: vi.fn().mockReturnValue(of(undefined)), // Mock afterClosed method
      close: vi.fn(),
      addPanelClass: vi.fn(), // Mock addPanelClass method
      backdropClick: vi.fn().mockReturnValue(of(undefined)), // Mock backDropClick method
    } as unknown as MatDialogRef<MessageboxComponent<any>>;

    await TestBed.configureTestingModule({
      imports: [
        MessageboxComponent,
        SoftOneTestBed,
        FontAwesomeModule,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: FakeLoader },
        }),
      ],
      providers: [
        { provide: ToastrService, useValue: toastrService },
        { provide: MatDialogRef, useValue: mockDialogRef },
      ],
    }).compileComponents();

    const translateService = TestBed.inject(TranslateService);
    // translateService.setDefaultLang('en');
    translateService.setTranslation('en', {
      'core.ok': 'ok',
      'core.yes': 'yes',
      'core.no': 'no',
      'core.cancel': 'cancel',
    });
    translateService.use('en'); // Use the language to activate translations

    library = TestBed.inject(FaIconLibrary);
    library.addIconPacks(fal, far, fas); // Add the required icon to the library

    fixture = TestBed.createComponent(MessageboxComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    describe('setupHeaderAndIcon', () => {
      it('should set up information header and icon correctly', () => {
        component.data.type = 'information';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.headerClass).toBe('information');
        expect(component.iconName).toBe('circle-info');
      });
      it('should set up warning header and icon correctly', () => {
        component.data.type = 'warning';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.headerClass).toBe('warning');
        expect(component.iconName).toBe('circle-exclamation');
      });
      it('should set up error header and icon correctly', () => {
        component.data.type = 'error';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.headerClass).toBe('error');
        expect(component.iconName).toBe('triangle-exclamation');
      });
      it('should set up success header and icon correctly', () => {
        component.data.type = 'success';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.headerClass).toBe('success');
        expect(component.iconName).toBe('circle-check');
      });
      it('should set up question header and icon correctly', () => {
        component.data.type = 'question';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.headerClass).toBe('information');
        expect(component.iconName).toBe('circle-question');
      });
      it('should set up question abort header and icon correctly', () => {
        component.data.type = 'questionAbort';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.headerClass).toBe('information');
        expect(component.iconName).toBe('circle-question');
      });
      it('should set up forbidden header and icon correctly', () => {
        component.data.type = 'forbidden';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.headerClass).toBe('error');
        expect(component.iconName).toBe('ban');
      });
      it('should set up progress header and icon correctly', () => {
        component.data.type = 'progress';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.iconPrefix).toBe('far');
        expect(component.iconName).toBe('spinner');
        expect(component.iconAnimation).toBe('spin');
      });
      it('should set up default header and icon correctly', () => {
        component.data.type = '';
        component['setupHeaderAndIcon']();
        fixture.detectChanges();
        expect(component.noIcon).toBe(true);
      });
    });
    describe('setupButtons', () => {
      it('should set up ok button correctly', () => {
        component.data.buttons = 'ok';
        component['setupButtons']();
        fixture.detectChanges();
        expect(component.showButtonOk).toBe(true);
      });
      it('should set up okcancel button correctly', () => {
        component.data.buttons = 'okCancel';
        component['setupButtons']();
        fixture.detectChanges();
        expect(component.showButtonOk).toBe(true);
        expect(component.showButtonCancel).toBe(true);
      });
      it('should set up yesno button correctly', () => {
        component.data.buttons = 'yesNo';
        component['setupButtons']();
        fixture.detectChanges();
        expect(component.showButtonYes).toBe(true);
        expect(component.showButtonNo).toBe(true);
      });
      it('should set up yesnocancel button correctly', () => {
        component.data.buttons = 'yesNoCancel';
        component['setupButtons']();
        fixture.detectChanges();
        expect(component.showButtonYes).toBe(true);
        expect(component.showButtonNo).toBe(true);
        expect(component.showButtonCancel).toBe(true);
      });
    });
  });
  describe('methods', () => {
    describe('onDateChanged', () => {
      it('should set the date value', () => {
        const date = new Date(2020, 10, 11);
        component.onDateChanged(date);
        expect(component.data.inputDateValue).toEqual(date);
      });
    });
    describe('enterPressed', () => {
      it('should call close when showButtonOk is true', () => {
        component.showButtonOk = true;
        component.showButtonYes = false;
        component.closeDialog = vi.fn(); // Mock the closeDialog method
        component.enterPressed();
        expect(component.closeDialog).toHaveBeenCalled();
      });
      it('should call close when showButtonYes is true', () => {
        component.showButtonOk = false;
        component.showButtonYes = true;
        component.closeDialog = vi.fn(); // Mock the closeDialog method
        component.enterPressed();
        expect(component.closeDialog).toHaveBeenCalled();
      });
      it('should not call close when showButtonOk and showButtonYes is false', () => {
        component.showButtonYes = false;
        component.showButtonOk = false;
        component.closeDialog = vi.fn(); // Mock the closeDialog method
        component.enterPressed();
        expect(component.closeDialog).not.toHaveBeenCalled();
      });
    });
    describe('iconDoubleClick', () => {
      it('should do nothing if hiddenText is not set', () => {
        vi.spyOn(component, 'logHiddenText' as any);
        component['_doubleClickCount'] = 0;
        component.showHiddenText = false;
        component.data.hiddenText = '';

        component.iconDoubleClick();

        expect(component['_doubleClickCount']).toBe(0);
        expect(component.data.hiddenText).toBe('');
        expect(component.showHiddenText).toBe(false);
        expect(component['logHiddenText']).not.toHaveBeenCalled();
      });
      it('should show hidden text if double click count is 2', () => {
        vi.spyOn(component, 'logHiddenText' as any);
        component['_doubleClickCount'] = 2;
        component.showHiddenText = false;
        component.data.hiddenText = 'hidden text';

        component.iconDoubleClick();

        expect(component['_doubleClickCount']).toBe(0);
        expect(component.data.hiddenText).toBe('hidden text');
        expect(component.showHiddenText).toBe(true);
        expect(component['logHiddenText']).toHaveBeenCalled();
      });
    });
    describe('closeDialog', () => {
      it('should close dialog with response', () => {
        component.data.inputTextValue = 'text';
        component.data.inputCheckboxValue = true;
        component.data.inputDateValue = new Date(2020, 10, 11);
        component.closeDialog(true);
        expect(mockDialogRef.close).toHaveBeenCalledWith({
          result: true,
          textValue: 'text',
          checkboxValue: true,
          dateValue: new Date(2020, 10, 11),
          data: component.data,
        });
      });
    });
  });
  describe('DOM', () => {
    describe('dialog header', () => {
      let dialogHeaderDebugElement: DebugElement;
      let dialogHeaderElement: HTMLElement;
      beforeEach(() => {
        dialogHeaderDebugElement = fixture.debugElement.query(
          By.css('.soe-dialog-header')
        );
        dialogHeaderElement = dialogHeaderDebugElement.nativeElement;
      });
      it('should render the header correctly', () => {
        expect(dialogHeaderElement).toBeTruthy();
      });
      // it('should render the header text according to data.title', () => { TODO: Fix this test, it fails at detecting changes
      //   component.data.title = 'Title';
      //   fixture.detectChanges();
      //   /expect(dialogHeaderElement.textContent).toBe('Title');
      // });
      it('should render with class according to headerClass', () => {
        component.headerClass = 'information';
        fixture.detectChanges();
        expect(dialogHeaderElement.classList).toContain('information');
      });
      describe('button close', () => {
        let buttonCloseDebugElement: DebugElement;
        let buttonCloseElement: HTMLElement;
        beforeEach(() => {
          component.data.hideCloseButton = false;
          fixture.detectChanges();
          buttonCloseDebugElement = dialogHeaderDebugElement.query(
            By.css('.btn-close')
          );
          buttonCloseElement = buttonCloseDebugElement.nativeElement;
        });
        it('should render the close button', () => {
          expect(buttonCloseElement).toBeTruthy();
        });
        it('should not render the close button if hideCloseButton is true', () => {
          component.data.hideCloseButton = true;
          fixture.detectChanges();
          buttonCloseDebugElement = dialogHeaderDebugElement.query(
            By.css('.btn-close')
          );
          expect(buttonCloseDebugElement).toBeFalsy();
        });
        it('should call closeDialog when clicked', () => {
          vi.spyOn(component, 'closeDialog' as any);
          buttonCloseElement.click();
          expect(component.closeDialog).toHaveBeenCalled();
        });
        it('should have an icon with fal xmark', () => {
          const iconInstance = buttonCloseDebugElement.query(
            By.directive(FaIconComponent)
          ).componentInstance as FaIconComponent;
          expect(iconInstance).toBeTruthy();
          expect(iconInstance.icon()).toEqual(['fal', 'xmark']);
        });
      });
    });
    describe('dialog content', () => {
      let dialogContentDebugElement: DebugElement;
      let dialogContentElement: HTMLElement;
      beforeEach(() => {
        fixture.detectChanges();
        dialogContentDebugElement = fixture.debugElement.query(
          By.css('.soe-dialog__content')
        );
        dialogContentElement = dialogContentDebugElement.nativeElement;
      });
      it('should render the content correctly', () => {
        expect(dialogContentElement).toBeTruthy();
      });
      it('should render data.text as innerHTML if there is data.text', () => {
        component.data.text = 'text';
        fixture.detectChanges();
        const classElement = dialogContentDebugElement.query(
          By.css('.soe-dialog__text-content')
        );
        expect(classElement.nativeElement.innerHTML).toBe('text');
      });
      describe('faIcon', () => {
        let iconDebugElement: DebugElement;
        let iconInstance: FaIconComponent;
        beforeEach(() => {
          component.data.text = 'text';
          component.noIcon = false;
          component.iconPrefix = 'fal';
          component.iconName = 'circle-info';
          component.data.iconClass = 'information-color';
          component.iconAnimation = 'spin';
          fixture.detectChanges();
          iconDebugElement = dialogContentDebugElement.query(By.css('fa-icon'));
          iconInstance = dialogContentDebugElement.query(
            By.directive(FaIconComponent)
          ).componentInstance as FaIconComponent;
        });
        it('should have a fa-icon child if has a iconName with correct attributes', () => {
          expect(iconInstance).toBeTruthy();
          expect(iconInstance.icon()).toEqual(['fal', 'circle-info']);
          expect(component.iconAnimation).toBe('spin');
          expect(iconDebugElement.nativeElement.classList).toContain(
            'information-color'
          );
        });
        it('should not have a icon if noIcon is true', () => {
          component.noIcon = true;
          fixture.detectChanges();
          const faIconInstance = dialogContentDebugElement.query(
            By.directive(FaIconComponent)
          );
          expect(faIconInstance).toBeFalsy();
        });
        it('should not have a icon if iconName is not set', () => {
          component.iconName = '' as any;
          fixture.detectChanges();
          const faIconInstance = dialogContentDebugElement.query(
            By.directive(FaIconComponent)
          );
          expect(faIconInstance).toBeFalsy();
        });
        it('should trigger iconDoubleClick when icon is double clicked', () => {
          vi.spyOn(component, 'iconDoubleClick' as any);
          iconDebugElement.triggerEventHandler('dblclick', {});
          expect(component.iconDoubleClick).toHaveBeenCalled();
        });
      });
      describe('input text', () => {
        beforeEach(() => {
          component.data.showInputText = true;
          fixture.detectChanges();
        });
        describe('input text', () => {
          let inputDebugElement: DebugElement;
          let inputElement: HTMLInputElement;
          beforeEach(() => {
            component.data.inputTextRows = 1;
            fixture.detectChanges();
            inputDebugElement = fixture.debugElement.query(
              By.css('input.form-control')
            );
            inputElement = inputDebugElement.nativeElement;
          });
          it('should render input text', () => {
            expect(inputElement).toBeTruthy();
          });
          it('should not render if inputTextRows is 0', () => {
            component.data.inputTextRows = 0;
            fixture.detectChanges();
            inputDebugElement = fixture.debugElement.query(
              By.css('input.form-control')
            );
            expect(inputDebugElement).toBeFalsy();
          });
          it('should render input text with type password', () => {
            component.data.isPassword = true;
            fixture.detectChanges();
            expect(inputElement.type).toBe('password');
          });
          it('should render input text with type text', () => {
            component.data.isPassword = false;
            fixture.detectChanges();
            expect(inputElement.type).toBe('text');
          });
          it('should call enterPressed on keyup.enter', () => {
            vi.spyOn(component, 'enterPressed' as any);
            inputDebugElement.triggerEventHandler('keyup.enter', {});
            expect(component.enterPressed).toHaveBeenCalled();
          });
        });
        describe('textarea', () => {
          let textareaDebugElement: DebugElement;
          let textareaElement: HTMLTextAreaElement;
          beforeEach(() => {
            component.data.inputTextRows = 2;
            fixture.detectChanges();
            textareaDebugElement = fixture.debugElement.query(
              By.css('textarea.form-control')
            );
            textareaElement = textareaDebugElement.nativeElement;
          });
          it('should render textarea', () => {
            expect(textareaElement).toBeTruthy();
          });
          it('should not render if inputTextRows is 0 or 1', () => {
            component.data.inputTextRows = 0;
            fixture.detectChanges();
            textareaDebugElement = fixture.debugElement.query(
              By.css('textarea.form-control')
            );
            expect(textareaDebugElement).toBeFalsy();

            component.data.inputTextRows = 1;
            fixture.detectChanges();
            textareaDebugElement = fixture.debugElement.query(
              By.css('textarea.form-control')
            );
            expect(textareaDebugElement).toBeFalsy();
          });
          it('should render textarea with rows according to inputTextRows', () => {
            expect(textareaElement.rows).toBe(2);
          });
        });
        describe('datepicker', () => {
          let datepickerDebugElement: DebugElement;
          let datepickerElement: HTMLElement;
          beforeEach(() => {
            component.data.showInputDate = true;
            fixture.detectChanges();
            datepickerDebugElement = fixture.debugElement.query(
              By.css('soe-datepicker')
            );
            datepickerElement = datepickerDebugElement.nativeElement;
          });
          it('should render datepicker', () => {
            expect(datepickerElement).toBeTruthy();
          });
          it('should not render if showInputDate is false', () => {
            component.data.showInputDate = false;
            fixture.detectChanges();
            datepickerDebugElement = fixture.debugElement.query(
              By.css('soe-datepicker')
            );
            expect(datepickerDebugElement).toBeFalsy();
          });
          it('should have labelkey with inputDateLabel', () => {
            const testdatelabel = 'testinputDateLabel';
            component.data.inputDateLabel = testdatelabel;
            fixture.detectChanges();
            const datepickerComponent =
              datepickerDebugElement.componentInstance;
            expect(datepickerComponent.labelKey()).toBeTruthy();
            expect(datepickerComponent.labelKey()).toBe(testdatelabel);
          });
          it('should call onDateChanged when valueChanged event is emitted', () => {
            vi.spyOn(component, 'onDateChanged' as any);
            const testevent = new Date(2020, 10, 11);
            datepickerDebugElement.triggerEventHandler(
              'valueChanged',
              testevent
            );
            expect(component.onDateChanged).toHaveBeenCalledWith(testevent);
          });
        });
        describe('checkbox', () => {
          let checkboxDebugElement: DebugElement;
          let checkboxElement: HTMLInputElement;
          beforeEach(() => {
            component.data.showInputCheckbox = true;
            fixture.detectChanges();
            checkboxDebugElement = fixture.debugElement.query(
              By.css('input.form-check-input')
            );
            checkboxElement = checkboxDebugElement.nativeElement;
          });
          it('should render checkbox', () => {
            expect(checkboxElement).toBeTruthy();
          });
          it('should not render if showInputCheckbox is false', () => {
            component.data.showInputCheckbox = false;
            fixture.detectChanges();
            checkboxDebugElement = fixture.debugElement.query(
              By.css('input.form-check-input')
            );
            expect(checkboxDebugElement).toBeFalsy();
          });
        });
      });
      describe('hidden text', () => {
        let hiddenTextDebugElement: DebugElement;
        let hiddenTextElement: HTMLElement;
        beforeEach(() => {
          component.data.hiddenText = 'hidden text';
          component.showHiddenText = true;
          fixture.detectChanges();
          hiddenTextDebugElement = fixture.debugElement.query(
            By.css('.soe-dialog__hidden-content')
          );
          hiddenTextElement = hiddenTextDebugElement.nativeElement;
        });
        it('should render hidden text', () => {
          expect(hiddenTextElement).toBeTruthy();
        });
        it('should not render if showHiddenText is false', () => {
          component.showHiddenText = false;
          fixture.detectChanges();
          hiddenTextDebugElement = fixture.debugElement.query(
            By.css('.soe-dialog__hidden-content')
          );
          expect(hiddenTextDebugElement).toBeFalsy();
        });
        it('should render hidden text with data.hiddenText', () => {
          expect(hiddenTextElement.textContent).toBe('hidden text');
        });
      });
      describe('buttons', () => {
        describe('cancel button', () => {
          let buttonCancelDebugElement: DebugElement;
          let buttonCancelElement: HTMLElement;
          beforeEach(() => {
            component.showButtonCancel = true;
            fixture.detectChanges();
            buttonCancelDebugElement = fixture.debugElement.query(
              By.css('soe-button[id="button-cancel"]')
            );
            buttonCancelElement = buttonCancelDebugElement.nativeElement;
          });
          it('should render cancel button', () => {
            expect(buttonCancelElement).toBeTruthy();
          });
          it('should not render if showButtonCancel is false', () => {
            component.showButtonCancel = false;
            fixture.detectChanges();
            buttonCancelDebugElement = fixture.debugElement.query(
              By.css('soe-button[id="button-cancel"]')
            );
            expect(buttonCancelDebugElement).toBeFalsy();
          });
          it('should call closeDialog on action', () => {
            vi.spyOn(component, 'closeDialog' as any);
            buttonCancelDebugElement.triggerEventHandler('action', {});
            expect(component.closeDialog).toHaveBeenCalled();
          });
          it('should have caption according to buttonCancelLabel', () => {
            component.buttonCancelLabel = 'cancel';
            fixture.detectChanges();
            const buttonComponent = buttonCancelDebugElement.componentInstance;
            expect(buttonComponent.caption()).toBe('cancel');
          });
        });
        describe('ok button', () => {
          let buttonOkDebugElement: DebugElement;
          let buttonOkElement: HTMLElement;
          beforeEach(() => {
            component.showButtonOk = true;
            fixture.detectChanges();
            buttonOkDebugElement = fixture.debugElement.query(
              By.css('soe-button[id="button-ok"]')
            );
            buttonOkElement = buttonOkDebugElement.nativeElement;
          });
          it('should render ok button', () => {
            expect(buttonOkElement).toBeTruthy();
          });
          it('should not render if showButtonOk is false', () => {
            component.showButtonOk = false;
            fixture.detectChanges();
            buttonOkDebugElement = fixture.debugElement.query(
              By.css('soe-button[id="button-ok"]')
            );
            expect(buttonOkDebugElement).toBeFalsy();
          });
          it('should call closeDialog with true on action', () => {
            vi.spyOn(component, 'closeDialog' as any);
            buttonOkDebugElement.triggerEventHandler('action', {});
            expect(component.closeDialog).toHaveBeenCalledWith(true);
          });
          it('should have caption according to buttonOkLabel', () => {
            component.buttonOkLabel = 'ok';
            fixture.detectChanges();
            const buttonComponent = buttonOkDebugElement.componentInstance;
            expect(buttonComponent.caption()).toBe('ok');
          });
        });
        describe('no button', () => {
          let buttonNoDebugElement: DebugElement;
          let buttonNoElement: HTMLElement;
          beforeEach(() => {
            component.showButtonNo = true;
            fixture.detectChanges();
            buttonNoDebugElement = fixture.debugElement.query(
              By.css('soe-button[id="button-no"]')
            );
            buttonNoElement = buttonNoDebugElement.nativeElement;
          });
          it('should render no button', () => {
            expect(buttonNoElement).toBeTruthy();
          });
          it('should not render if showButtonNo is false', () => {
            component.showButtonNo = false;
            fixture.detectChanges();
            buttonNoDebugElement = fixture.debugElement.query(
              By.css('soe-button[id="button-no"]')
            );
            expect(buttonNoDebugElement).toBeFalsy();
          });
          it('should call closeDialog with false on action', () => {
            vi.spyOn(component, 'closeDialog' as any);
            buttonNoDebugElement.triggerEventHandler('action', {});
            expect(component.closeDialog).toHaveBeenCalledWith(false);
          });
          it('should have caption according to buttonNoLabel', () => {
            component.buttonNoLabel = 'no';
            fixture.detectChanges();
            const buttonComponent = buttonNoDebugElement.componentInstance;
            expect(buttonComponent.caption()).toBe('no');
          });
        });
        describe('yes button', () => {
          let buttonYesDebugElement: DebugElement;
          let buttonYesElement: HTMLElement;
          beforeEach(() => {
            component.showButtonYes = true;
            fixture.detectChanges();
            buttonYesDebugElement = fixture.debugElement.query(
              By.css('soe-button[id="button-yes"]')
            );
            buttonYesElement = buttonYesDebugElement.nativeElement;
          });
          it('should render yes button', () => {
            expect(buttonYesElement).toBeTruthy();
          });
          it('should not render if showButtonYes is false', () => {
            component.showButtonYes = false;
            fixture.detectChanges();
            buttonYesDebugElement = fixture.debugElement.query(
              By.css('soe-button[id="button-yes"]')
            );
            expect(buttonYesDebugElement).toBeFalsy();
          });
          it('should call closeDialog with true on action', () => {
            vi.spyOn(component, 'closeDialog' as any);
            buttonYesDebugElement.triggerEventHandler('action', {});
            expect(component.closeDialog).toHaveBeenCalledWith(true);
          });
          it('should have caption according to buttonYesLabel', () => {
            component.buttonYesLabel = 'yes';
            fixture.detectChanges();
            const buttonComponent = buttonYesDebugElement.componentInstance;
            expect(buttonComponent.caption()).toBe('yes');
          });
        });
      });
    });
  });
});
