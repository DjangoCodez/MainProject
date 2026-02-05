import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DialogService } from '../services/dialog.service';
import { DialogComponent } from './dialog.component';
import { of } from 'rxjs';
import { Dialog } from '@angular/cdk/dialog';
import {
  FaIconComponent,
  FaIconLibrary,
} from '@fortawesome/angular-fontawesome';
import { fal } from '@fortawesome/pro-light-svg-icons';
import { ComponentRef, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('DialogComponent', () => {
  let component: DialogComponent<any>;
  let componentRef: ComponentRef<DialogComponent<any>>;
  let fixture: ComponentFixture<DialogComponent<any>>;
  let dialogRefMock: any;
  let dialogServiceMock: any;
  let dataMock: any;
  let library: FaIconLibrary;

  beforeEach(() => {
    dialogRefMock = {
      disableClose: false,
      addPanelClass: vi.fn(),
      close: vi.fn(),
    };
    dialogServiceMock = {};
    dataMock = {
      disableClose: true,
      size: 'lg',
      callbackAction: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [
        DialogComponent,
        { provide: MAT_DIALOG_DATA, useValue: dataMock },
        { provide: MatDialogRef, useValue: dialogRefMock },
        { provide: DialogService, useValue: dialogServiceMock },
      ],
    });
    library = TestBed.inject(FaIconLibrary);
    library.addIconPacks(fal); // Add the required icon to the library

    fixture = TestBed.createComponent(DialogComponent);
    component = TestBed.inject(DialogComponent);
    // componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setups', () => {
    describe('component properties', () => {
      it('should initialize with correct data and dialogRef properties', () => {
        expect(component.data).toBe(dataMock);
        expect(component.dialogRef).toBe(dialogRefMock);
        expect(component.dialogService).toBe(dialogServiceMock);
        expect(component.submitting()).toBe(false);
        expect(dialogRefMock.addPanelClass).toHaveBeenCalledWith('size-lg');
      });
    });
  });
  describe('methods', () => {
    describe('triggerPrimaryAction', () => {
      it('should have called callAction and close dialog with true in triggerPrimaryAction', async () => {
        const observableMock = of(true);
        dataMock.callbackAction.mockReturnValue(observableMock);
        await component.triggerPrimaryAction();
        expect(dataMock.callbackAction).toHaveBeenCalled();
        expect(dialogRefMock.close).toHaveBeenCalledWith(true);
      });
    });
    describe('callAction', () => {
      it('should await Observable in callAction', async () => {
        const observableMock = of(true);
        dataMock.callbackAction.mockReturnValue(observableMock);
        await component.callAction();
        expect(dataMock.callbackAction).toHaveBeenCalled();
      });
      it('should await Promise in callAction', async () => {
        const promiseMock = Promise.resolve(true);
        dataMock.callbackAction.mockReturnValue(promiseMock);
        await component.callAction();
        expect(dataMock.callbackAction).toHaveBeenCalled();
      });
    });
    describe('closeDialog', () => {
      it('should close dialog with false in closeDialog', () => {
        component.closeDialog();
        expect(dialogRefMock.close).toHaveBeenCalledWith(false);
      });
    });
  });
  describe('DOM', () => {
    describe('close button', () => {
      let closebuttonDebugElement: DebugElement;
      let closebuttonNativeElement: HTMLElement;
      beforeEach(() => {
        closebuttonDebugElement = fixture.debugElement.query(
          By.css('button.btn-close')
        );
        closebuttonNativeElement = closebuttonDebugElement.nativeElement;
      });
      it('should have close button', () => {
        expect(closebuttonDebugElement).toBeTruthy();
      });
      it('should close dialogRef on click', () => {
        const closeDialogRefSpy = vi.spyOn(component.dialogRef, 'close');
        closebuttonNativeElement.click();
        expect(closeDialogRefSpy).toHaveBeenCalled();
      });
      it('should have an icon with fal xmark icon', () => {
        // Access Fa-icon component to test the icon
        const iconComponent = closebuttonDebugElement.query(
          By.directive(FaIconComponent)
        ).componentInstance as FaIconComponent;
        expect(iconComponent).toBeTruthy();
        expect(iconComponent.icon()).toEqual(['fal', 'xmark']);
      });
    });
    describe('dialog content', () => {
      let dialogContentDebugElement: DebugElement;
      let dialogContentNativeElement: HTMLElement;
      beforeEach(() => {
        component.data.disableContentScroll = false;
        fixture.detectChanges();
        dialogContentDebugElement = fixture.debugElement.queryAll(
          By.css('div')
        )[2];
        dialogContentNativeElement = dialogContentDebugElement.nativeElement;
      });
      it('should have dialog content with class if disableContentScroll is false', () => {
        expect(dialogContentNativeElement).toBeTruthy();
        expect(dialogContentNativeElement.classList).toContain(
          'soe-dialog__content'
        );
      });
      it('should not have dialog content with class if disableContentScroll is true', () => {
        component.data.disableContentScroll = true;
        fixture.detectChanges();
        dialogContentDebugElement = fixture.debugElement.queryAll(
          By.css('div')
        )[2];
        dialogContentNativeElement = dialogContentDebugElement.nativeElement;
        expect(dialogContentNativeElement).toBeTruthy();
        expect(dialogContentNativeElement.classList).not.toContain(
          'soe-dialog__content'
        );
      });
      it('should have correct content', () => {
        component.data.content = '<strong>Test content</strong>';
        fixture.detectChanges();
        expect(dialogContentNativeElement.textContent).toContain(
          'Test content'
        );
      });
    });
    describe('dialog footer', () => {
      let dialogFooterDebugElement: DebugElement;
      let dialogFooterNativeElement: HTMLElement;
      beforeEach(() => {
        component.data.hideFooter = false;
        fixture.detectChanges();
        dialogFooterDebugElement = fixture.debugElement.query(
          By.css('.soe-dialog__footer')
        );
        // dialogFooterNativeElement = dialogFooterDebugElement.nativeElement;
      });
      describe('primary text button', () => {
        let buttonDebug: DebugElement;
        let buttonNative: HTMLElement;
        let buttonInstance: any;
        beforeEach(() => {
          component.data.primaryText = 'Primary test';
          fixture.detectChanges();
          buttonDebug = dialogFooterDebugElement.query(By.css('soe-button'));
          // buttonNative = buttonDebug.nativeElement;
          buttonInstance = buttonDebug.componentInstance;
        });
        it('should have primary button', () => {
          expect(buttonDebug).toBeTruthy();
        });
        it('should have primary button with caption Primary test', () => {
          expect(buttonInstance.caption()).toBe('Primary test');
        });
        it('should have primary button with behaviour primary', () => {
          expect(buttonInstance.behaviour()).toBe('primary');
        });
        it('should have primary button with inprogress set to submitting', () => {
          expect(buttonInstance.inProgress()).toBe(component.submitting());
        });
      });
      describe('secondary text button', () => {
        let buttonDebug: DebugElement;
        let buttonNative: HTMLElement;
        let buttonInstance: any;
        beforeEach(() => {
          component.data.secondaryText = 'Secondary test';
          fixture.detectChanges();
          buttonDebug = dialogFooterDebugElement.query(By.css('soe-button'));
          // buttonNative = buttonDebug.nativeElement;
          buttonInstance = buttonDebug.componentInstance;
        });
        it('should have secondary button', () => {
          expect(buttonDebug).toBeTruthy();
        });
        it('should have secondary button with caption Secondary test', () => {
          expect(buttonInstance.caption()).toBe('Secondary test');
        });
        it('should have secondary button with behaviour close', () => {
          expect(buttonInstance.behaviour()).toBe('standard');
        });
        it('should have secondary button with inprogress set to submitting', () => {
          expect(buttonInstance.inProgress()).toBe(component.submitting());
        });
      });
    });
  });
});
