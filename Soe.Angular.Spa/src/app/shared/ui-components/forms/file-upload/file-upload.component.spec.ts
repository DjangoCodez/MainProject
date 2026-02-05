import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { ComponentRef, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { FileUploadComponent } from './file-upload.component';

// Polyfill DataTransfer for testing
if (typeof DataTransfer === 'undefined') {
  global.DataTransfer = class DataTransfer {
    items = {
      add: vi.fn(),
      clear: vi.fn(),
    };
    files: File[] = [];
  } as any;
}

describe('FileUploadComponent', () => {
  let component: FileUploadComponent;
  let componentRef: ComponentRef<FileUploadComponent>;
  let fixture: ComponentFixture<FileUploadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, FileUploadComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FileUploadComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('component', () => {
    describe('setup', () => {
      it('should initalize with default values', () => {
        expect(component.inputId()).toBeTruthy();
        expect(component.labelKey()).toBe('');
        expect(component.placeholderKey()).toBe('');
        expect(component.inline()).toBe(false);
        expect(component.hideDetails()).toBe(false);
        expect(component.hideDropZone()).toBe(false);
        expect(component.hideInputField()).toBe(false);
      });
    });
  });
  describe('methods', () => {
    describe('onFileOver', () => {
      it('should handle file drag over', () => {
        const event = {
          type: 'dragover',
          preventDefault: vi.fn(),
        } as unknown as DragEvent;
        vi.spyOn(event, 'preventDefault');
        component.onFileOver(event);

        expect(event.preventDefault).toHaveBeenCalled();
        expect(component.isFileOver()).toBe(true);
      });
    });
    describe('onFileLeave', () => {
      it('should handle file drag leave', () => {
        const event = {
          type: 'dragleave',
          preventDefault: vi.fn(),
        } as unknown as DragEvent;
        vi.spyOn(event, 'preventDefault');
        component.onFileLeave(event);

        expect(event.preventDefault).toHaveBeenCalled();
        expect(component.isFileOver()).toBe(false);
      });
    });
    describe('removeFile', () => {
      it('should remove file if it exists', () => {
        component.control = new FormControl('');
        const file = { ...new File(['dummy content'], 'example.txt'), id: '1' };
        component.attachedFiles.set([file]);
        component.removeFile(file);
        expect(component.control.value).toBe('');
        expect(component.attachedFiles()).toEqual([]);
      });
      it('should remove file if it does not exist', () => {
        component.control = new FormControl('');
        const file = { ...new File(['dummy content'], 'example.txt'), id: '1' };
        component.attachedFiles.set([]);
        component.removeFile(file);
        expect(component.control.value).toBe('');
        expect(component.hasFile()).toBe(false);
      });
    });
  });
  describe('DOM', () => {
    describe('soe-dropzone', () => {
      let soeDropzoneElement: DebugElement;
      beforeEach(() => {
        soeDropzoneElement = fixture.debugElement.query(
          By.css('.soe-dropzone')
        );
      });
      it('should initalize', () => {
        expect(soeDropzoneElement).toBeTruthy();
      });
      it('should have style if inline is true', () => {
        componentRef.setInput('inline', true);
        fixture.detectChanges();
        expect(soeDropzoneElement.nativeElement.style.marginTop).toBe('2rem'); // expected marginTop style
      });
      it('should not have style if inline is false', () => {
        componentRef.setInput('inline', false);
        fixture.detectChanges();
        expect(soeDropzoneElement.nativeElement.style.marginTop).toBe('');
      });
    });
    describe('form-label', () => {
      it('should not initalize if labelKey is null', () => {
        componentRef.setInput('labelKey', '');
        fixture.detectChanges();
        const formLabelElement = fixture.debugElement.query(
          By.css('.form-label')
        );
        expect(formLabelElement).toBeFalsy();
      });
    });
    describe('form-control', () => {
      let formControlElement: DebugElement;
      beforeEach(() => {
        componentRef.setInput('hideInputField', false);
        fixture.detectChanges();
        formControlElement = fixture.debugElement.query(
          By.css('.form-control')
        );
      });
      it('should not initalize if hideInputField is true', () => {
        componentRef.setInput('hideInputField', true);
        fixture.detectChanges();
        formControlElement = fixture.debugElement.query(
          By.css('.form-control')
        );
        expect(formControlElement).toBeFalsy();
      });
      it('should initialize and mirror component values if hideInputfield is false', () => {
        expect(formControlElement).toBeTruthy();
        expect(
          formControlElement.nativeElement.getAttribute('placeholder')
        ).toBe(component.placeholderKey());
        expect(formControlElement.nativeElement.getAttribute('id')).toBe(
          component.inputId()
        );
        expect(formControlElement.nativeElement.disabled).toBe(
          component.control.disabled
        );
        expect(formControlElement.nativeElement.getAttribute('accept')).toBe(
          component.acceptedFileExtensions().join(',')
        );
      });
      it('should update values when they change', () => {
        componentRef.setInput('placeholderKey', 'test');
        componentRef.setInput('inputId', 123);
        component.control.disable();
        componentRef.setInput('acceptedFileExtensions', ['.txt', '.pdf']);
        fixture.detectChanges();
        expect(
          formControlElement.nativeElement.getAttribute('placeholder')
        ).toBe('test');
        expect(formControlElement.nativeElement.getAttribute('id')).toBe('123');
        expect(formControlElement.nativeElement.disabled).toBe(true);
        expect(formControlElement.nativeElement.getAttribute('accept')).toBe(
          '.txt,.pdf'
        );
      });
    });
    describe('dropzone', () => {
      let dropzoneElement: DebugElement;
      beforeEach(() => {
        componentRef.setInput('hideDropZone', false);
        fixture.detectChanges();
        dropzoneElement = fixture.debugElement.query(
          By.css('.soe-dropzone__section')
        );
      });
      it('should not initalize if hideDropZone is true', () => {
        componentRef.setInput('hideDropZone', true);
        fixture.detectChanges();
        dropzoneElement = fixture.debugElement.query(
          By.css('.soe-dropzone__section')
        );
        expect(dropzoneElement).toBeFalsy();
      });
      it('should initialize and mirror component properties if hideDropZone is false', () => {
        expect(dropzoneElement.nativeElement.classList).not.toContain(
          'file-over'
        );
        const dropZoneTitle: DebugElement = fixture.debugElement.query(
          By.css('.soe-dropzone__section-title')
        );
        expect(dropZoneTitle.nativeElement.textContent.trim()).toBe(
          'core.fileupload.dropfiles'
        );
      });
      it('should update class when isFileOver is true', () => {
        component.isFileOver.set(true);
        fixture.detectChanges();
        expect(dropzoneElement.nativeElement.classList).toContain('file-over');
      });
      it('should call onFileOver with event on dragover', () => {
        vi.spyOn(component, 'onFileOver');
        const dragoverEvent = {
          preventDefault: vi.fn(),
          type: 'dragover',
        } as any;
        dropzoneElement.triggerEventHandler('dragover', dragoverEvent);
        expect(component.onFileOver).toHaveBeenCalled();
      });
      it('should call onFileLeave with event on dragleave', () => {
        vi.spyOn(component, 'onFileLeave');
        const dragleaveEvent = {
          preventDefault: vi.fn(),
          type: 'dragleave',
        } as any;
        dropzoneElement.triggerEventHandler('dragleave', dragleaveEvent);
        expect(component.onFileLeave).toHaveBeenCalled();
      });
    });
    describe('table', () => {
      let tableElement: DebugElement;
      beforeEach(() => {
        const file = { ...new File(['dummy content'], 'example.txt'), id: '1' };
        component.attachedFiles.set([file]);
        componentRef.setInput('hideDetails', false);
        fixture.detectChanges();
        tableElement = fixture.debugElement.query(By.css('.table'));
      });
      it('should not initialize if hasFile is false', () => {
        component.attachedFiles.set([]);
        fixture.detectChanges();
        tableElement = fixture.debugElement.query(By.css('.table'));
        expect(tableElement).toBeFalsy();
      });
      it('should not initialize if hideDetails is true', () => {
        component.attachedFiles.set([]);
        componentRef.setInput('hideDetails', true);
        fixture.detectChanges();
        tableElement = fixture.debugElement.query(By.css('.table'));
        expect(tableElement).toBeFalsy();
      });
      it('should initialize with default values', () => {
        // TODO : Can't get test to work without setting formcontrol input.. OK?
        componentRef.setInput('fileSizeControl', new FormControl('hej'));
        componentRef.setInput('fileNameControl', new FormControl('hej'));
        fixture.detectChanges();

        const headers = tableElement.queryAll(By.css('th'));
        expect(headers[0].nativeElement.textContent).toBe(
          'core.fileupload.filename'
        );
        expect(headers[1].nativeElement.textContent).toBe(
          'core.fileupload.size'
        );
      });
      it('should update values', () => {
        const fileSizeControl = new FormControl('1MB');
        const fileNameControl = new FormControl('testfile.txt');

        componentRef.setInput('fileSizeControl', fileSizeControl);
        componentRef.setInput('fileNameControl', fileNameControl);
        fixture.detectChanges();

        // Values are bound via FormControls
        expect(fileSizeControl.value).toBe('1MB');
        expect(fileNameControl.value).toBe('testfile.txt');
      });
      it('should call removeFile on action', () => {
        const buttonElement = fixture.debugElement.query(
          By.css('soe-icon-button')
        );
        vi.spyOn(component, 'removeFile');
        buttonElement.triggerEventHandler('action', null);
        expect(component.removeFile).toHaveBeenCalled();
      });
    });
  });
});
