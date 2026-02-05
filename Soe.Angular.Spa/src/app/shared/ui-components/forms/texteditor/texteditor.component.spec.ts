import {
  ComponentFixture,
  fakeAsync,
  TestBed,
  tick,
} from '@angular/core/testing';

import { TexteditorComponent } from './texteditor.component';
import {
  ComponentRef,
  DebugElement,
  NO_ERRORS_SCHEMA,
  CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { By } from '@angular/platform-browser';
import { FormControl } from '@angular/forms';
import { vi } from 'vitest';

describe('TexteditorComponent', () => {
  let component: TexteditorComponent;
  let componentRef: ComponentRef<TexteditorComponent>;
  let fixture: ComponentFixture<TexteditorComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TexteditorComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
    });
    fixture = TestBed.createComponent(TexteditorComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should initialize with default values', () => {
      expect(component.inputId()).toBeTruthy();
      expect(component.labelKey()).toBe('');
      expect(component.secondaryLabelKey()).toBe('');
      expect(component.secondaryLabelBold()).toBe(false);
      expect(component.secondaryLabelParantheses()).toBe(true);
      expect(component.secondaryLabelPrefixKey()).toBe('');
      expect(component.secondaryLabelPostfixKey()).toBe('');
      expect(component.placeholderKey()).toBe('');
      expect(component.width()).toBe(0);
      expect(component.minHeight()).toBe(100);
      expect(component.maxHeight()).toBe(500);

      //Toolbar
      expect(component.showFontFormat()).toBe(true);
      expect(component.showBlock()).toBe(true);
      expect(component.showSubSuper()).toBe(true);
      expect(component.showBullet()).toBe(true);
      expect(component.showIndent()).toBe(true);
      expect(component.showAlign()).toBe(true);
      expect(component.showFont()).toBe(true);
      expect(component.showFontSize()).toBe(true);
      expect(component.showHeading()).toBe(true);
      expect(component.showColor()).toBe(true);
      expect(component.showClean()).toBe(true);
      expect(component.showLink()).toBe(true);
    });
  });
  describe('methods', () => {
    describe('onValueChange', () => {
      it('should emit value change', () => {
        vi.spyOn(component.valueChange, 'emit');
        const testValue = 'test';
        component.onValueChange(testValue);
        expect(component.valueChange.emit).toHaveBeenCalledWith(testValue);
      });
    });
    describe('ngOnInit', () => {
      it('should call super ngOnInit', () => {
        vi.spyOn(Object.getPrototypeOf(component), 'ngOnInit');
        component.ngOnInit();
        expect(Object.getPrototypeOf(component).ngOnInit).toHaveBeenCalled();
      });
      it('should call buildToolbar', () => {
        vi.spyOn(component as any, 'buildToolbar');
        component.ngOnInit();
        expect(component['buildToolbar']).toHaveBeenCalled();
      });
      it('should not build toolbar if all flags are false', () => {
        component.modules.toolbar = [];
        componentRef.setInput('showFontFormat', false);
        componentRef.setInput('showBlock', false);
        componentRef.setInput('showSubSuper', false);
        componentRef.setInput('showBullet', false);
        componentRef.setInput('showIndent', false);
        componentRef.setInput('showAlign', false);
        componentRef.setInput('showFont', false);
        componentRef.setInput('showFontSize', false);
        componentRef.setInput('showHeading', false);
        componentRef.setInput('showColor', false);
        componentRef.setInput('showClean', false);
        componentRef.setInput('showLink', false);

        fixture.detectChanges();
        component.ngOnInit();

        expect(component.modules.toolbar).toEqual([]);
      });
      it('should build toolbar correctly based on input flags', () => {
        component.modules.toolbar = [];
        componentRef.setInput('showFontFormat', true);
        componentRef.setInput('showBlock', true);
        componentRef.setInput('showSubSuper', true);
        componentRef.setInput('showBullet', true);
        componentRef.setInput('showIndent', true);
        componentRef.setInput('showAlign', true);
        componentRef.setInput('showFont', true);
        componentRef.setInput('showFontSize', true);
        componentRef.setInput('showHeading', true);
        componentRef.setInput('showColor', true);
        componentRef.setInput('showClean', true);
        componentRef.setInput('showLink', true);

        fixture.detectChanges();
        component.ngOnInit();

        expect(component.modules.toolbar).toEqual([
          ['bold', 'italic', 'underline', 'strike'],
          ['blockquote', 'code-block'],
          [{ script: 'sub' }, { script: 'super' }],
          [{ list: 'bullet' }, { list: 'ordered' }],
          [{ indent: '+1' }, { indent: '-1' }],
          [{ align: [] }],
          [{ font: [] }],
          [{ size: ['small', false, 'large', 'huge'] }],
          [{ header: [1, 2, 3, 4, 5, 6, false] }],
          [{ color: [] }, { background: [] }],
          ['clean'],
          ['link', 'image'],
        ]);
      });
    });
    describe('ngAfterViewInit', () => {
      afterEach(() => {
        vi.restoreAllMocks(); // Since mocking document.querySelector is global, restore it after each test
      });
      it('should set editor min and max height in ngAfterViewInit', fakeAsync(() => {
        componentRef.setInput('minHeight', 10);
        componentRef.setInput('maxHeight', 50);
        fixture.detectChanges();
        // Mock the editor element
        const editorMock = {
          style: {
            minHeight: '',
            maxHeight: '',
          },
        };

        vi.spyOn(document, 'querySelector').mockReturnValue(editorMock as any);

        component.ngAfterViewInit();
        tick(200);

        expect(editorMock.style.minHeight).toBe('10px'); // Default minHeight
        expect(editorMock.style.maxHeight).toBe('50px'); // Default maxHeight
      }));

      it('should not set styles if the editor element is not found', fakeAsync(() => {
        // Mock querySelector to return null (no element found)
        vi.spyOn(document, 'querySelector').mockReturnValue(null);

        component.ngAfterViewInit();
        tick(200);

        // Since no element is found, no styles should be set, and no errors should occur
        expect(document.querySelector).toHaveBeenCalledWith('.ql-editor');
      }));
    });
  });
  describe('DOM', () => {
    describe('div', () => {
      let divElement: HTMLElement;
      let divDebugElement: DebugElement;
      beforeEach(() => {
        divDebugElement = fixture.debugElement.query(By.css('div'));
        divElement = divDebugElement.nativeElement;
      });
      it('should initialize without style', () => {
        expect(divElement.style.width).toBeFalsy();
      });
      it('should set width', () => {
        componentRef.setInput('width', 100);
        fixture.detectChanges();
        expect(divElement.style.width).toBe('100px');
      });
    });
    describe('soe-label', () => {
      let soeLabelDebugElement: DebugElement;
      let soeLabelElement: HTMLElement;
      let soeLabelComponentInstance: any;
      beforeEach(() => {
        componentRef.setInput('labelKey', 'testLabel');
        fixture.detectChanges();
        soeLabelDebugElement = fixture.debugElement.query(By.css('soe-label'));
        soeLabelElement = soeLabelDebugElement.nativeElement;
        soeLabelComponentInstance = soeLabelDebugElement.componentInstance;
      });
      it('should initialize with default values', () => {
        expect(soeLabelComponentInstance.labelKey()).toBe(component.labelKey());
        expect(soeLabelComponentInstance.secondaryLabelKey()).toBe(
          component.secondaryLabelKey()
        );
        expect(soeLabelComponentInstance.secondaryLabelBold()).toBe(
          component.secondaryLabelBold()
        );
        expect(soeLabelComponentInstance.secondaryLabelParantheses()).toBe(
          component.secondaryLabelParantheses()
        );
        expect(soeLabelComponentInstance.secondaryLabelPrefixKey()).toBe(
          component.secondaryLabelPrefixKey()
        );
        expect(soeLabelComponentInstance.secondaryLabelPostfixKey()).toBe(
          component.secondaryLabelPostfixKey()
        );
        expect(soeLabelComponentInstance.isRequired()).toBe(
          component.isRequired()
        );
      });
      it('should pass the correct values', () => {
        componentRef.setInput('labelKey', 'testLabel');
        componentRef.setInput('secondaryLabelKey', 'testSecondaryLabel');
        componentRef.setInput('secondaryLabelBold', true);
        componentRef.setInput('secondaryLabelParantheses', false);
        componentRef.setInput('secondaryLabelPrefixKey', 'prefix');
        componentRef.setInput('secondaryLabelPostfixKey', 'postfix');

        fixture.detectChanges();

        soeLabelDebugElement = fixture.debugElement.query(By.css('soe-label'));
        soeLabelElement = soeLabelDebugElement.nativeElement;
        soeLabelComponentInstance = soeLabelDebugElement.componentInstance;

        expect(soeLabelComponentInstance.labelKey()).toBe('testLabel');
        expect(soeLabelComponentInstance.secondaryLabelKey()).toBe(
          'testSecondaryLabel'
        );
        expect(soeLabelComponentInstance.secondaryLabelBold()).toBe(true);
        expect(soeLabelComponentInstance.secondaryLabelParantheses()).toBe(
          false
        );
        expect(soeLabelComponentInstance.secondaryLabelPrefixKey()).toBe(
          'prefix'
        );
        expect(soeLabelComponentInstance.secondaryLabelPostfixKey()).toBe(
          'postfix'
        );
        expect(soeLabelComponentInstance.isRequired()).toBe(false);
      });
    });
    describe('quill-editor', () => {
      it('should initialize with default values when control is defined', () => {
        component.control = new FormControl();
        fixture.detectChanges();

        const quillEditorDebugElement = fixture.debugElement.query(
          By.css('quill-editor')
        );
        const quillEditorElement = quillEditorDebugElement.nativeElement;
        const quillEditorComponentInstance =
          quillEditorDebugElement.componentInstance;

        expect(quillEditorComponentInstance).toBeTruthy();
        expect(quillEditorElement.id).toBe(component.inputId());
        expect(quillEditorComponentInstance.modules()).toEqual(
          component.modules
        );
        expect(quillEditorComponentInstance.placeholder()).toBe(
          component.placeholderKey()
        );
        expect(quillEditorComponentInstance.required()).toBe(
          component.isRequired()
        );
      });
      it('should not render when control is not defined', () => {
        component.control = undefined as unknown as FormControl;
        fixture.detectChanges();

        const quillEditorDebugElement = fixture.debugElement.query(
          By.css('quill-editor')
        );

        expect(quillEditorDebugElement).toBeFalsy();
      });
    });
  });
});
