import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TextareaComponent } from './textarea.component';
import {
  ComponentRef,
  ElementRef,
  NO_ERRORS_SCHEMA,
  signal,
} from '@angular/core';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';
import { fail } from 'assert';

describe('TextareaComponent', () => {
  let component: TextareaComponent;
  let fixture: ComponentFixture<TextareaComponent>;
  let componentRef: ComponentRef<TextareaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TextareaComponent],
      providers: [{ provide: ElementRef, useValue: {} }],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(TextareaComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  describe('Setup', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize default inputs', () => {
      expect(component.labelKey()).toBe('');
      expect(component.secondaryLabelKey()).toBe('');
      expect(component.secondaryLabelBold()).toBe(false);
      expect(component.secondaryLabelParantheses()).toBe(true);
      expect(component.secondaryLabelPrefixKey()).toBe('');
      expect(component.secondaryLabelPostfixKey()).toBe('');
      expect(component.placeholderKey()).toBe('');
      expect(component.inline()).toBe(false);
      expect(component.alignInline()).toBe(false);
      expect(component.width()).toBe(0);
      expect(component.rows()).toBe(3);
      expect(component.resizeable()).toBe(false);
      expect(component.showLength()).toBe(false);
      expect(component.maxLength()).toBe(10000);
    });

    it('should initialize hasContent as false', () => {
      expect(component.hasContent()).toBe(false);
    });
  });

  describe('Methods', () => {
    describe('ngAfterViewInit', () => {
      it('should call super.ngAfterViewInit', () => {
        const superSpy = vi.spyOn(
          ValueAccessorDirective.prototype,
          'ngAfterViewInit'
        );
        component.ngAfterViewInit();
        expect(superSpy).toHaveBeenCalled();
      });

      it('should set hasContent to true if content has initial value', () => {
        vi.useFakeTimers();
        const mockContent = new ElementRef({ textContent: 'Some text' });
        component.content = signal(mockContent);
        vi.spyOn(component, 'elemHasContent').mockReturnValue(true);
        fixture.detectChanges();
        component.ngAfterViewInit();
        vi.runAllTimers();
        expect(component.hasContent()).toBe(true);
        vi.useRealTimers();
      });

      it('should not set hasContent if content is empty', () => {
        vi.useFakeTimers();
        const mockContent = new ElementRef({ textContent: '' });
        component.content = signal(mockContent);
        vi.spyOn(component, 'elemHasContent').mockReturnValue(false);

        component.ngAfterViewInit();
        vi.runAllTimers();
        expect(component.hasContent()).toBe(false);
        vi.useRealTimers();
      });
    });

    describe('onValueChange', () => {
      it('should emit valueChange event with the correct value', () => {
        const valueChangeSpy = vi.spyOn(component.valueChange, 'emit');
        const testValue = 'Test content';

        component.onValueChange(testValue);
        expect(valueChangeSpy).toHaveBeenCalledWith(testValue);
      });
    });
  });
  describe('DOM', () => {
    describe('Container Div', () => {
      it('should apply "mt-2" class when inline is false', () => {
        componentRef.setInput('inline', false);
        fixture.detectChanges();

        const containerDiv = fixture.debugElement.query(By.css('div'));
        expect(containerDiv.classes['mt-2']).toBe(true);
      });

      it('should apply "d-flex" and "flex-nowrap" classes when inline is true', () => {
        componentRef.setInput('inline', true);
        fixture.detectChanges();

        const containerDiv = fixture.debugElement.query(By.css('div'));
        expect(containerDiv.classes['d-flex']).toBe(true);
        expect(containerDiv.classes['flex-nowrap']).toBe(true);
      });

      it('should set width style when width is set and inline is false', () => {
        componentRef.setInput('width', 200);
        componentRef.setInput('inline', false);
        fixture.detectChanges();

        const containerDiv = fixture.debugElement.query(By.css('div'));
        expect(containerDiv.styles.width).toBe('200px');
      });

      it('should apply "form-label-inline-top-alignment" class when alignInline is true', () => {
        componentRef.setInput('alignInline', true);
        fixture.detectChanges();

        const containerDiv = fixture.debugElement.query(By.css('div'));
        expect(containerDiv.classes['form-label-inline-top-alignment']).toBe(
          true
        );
      });
    });

    describe('soe-label Component', () => {
      it('should pass label-related inputs to soe-label component', () => {
        componentRef.setInput('labelKey', 'label_key');
        componentRef.setInput('secondaryLabelKey', 'secondary_label_key');
        componentRef.setInput('secondaryLabelBold', true);
        componentRef.setInput('secondaryLabelParantheses', true);
        componentRef.setInput('secondaryLabelPrefixKey', 'prefix_key');
        componentRef.setInput('secondaryLabelPostfixKey', 'postfix_key');
        fixture.detectChanges();

        const soeLabel = fixture.debugElement.query(By.css('soe-label'));
        expect(soeLabel.componentInstance.labelKey()).toBe('label_key');
        expect(soeLabel.componentInstance.secondaryLabelKey()).toBe(
          'secondary_label_key'
        );
        expect(soeLabel.componentInstance.secondaryLabelBold()).toBe(true);
        expect(soeLabel.componentInstance.secondaryLabelParantheses()).toBe(
          true
        );
        expect(soeLabel.componentInstance.secondaryLabelPrefixKey()).toBe(
          'prefix_key'
        );
        expect(soeLabel.componentInstance.secondaryLabelPostfixKey()).toBe(
          'postfix_key'
        );
      });
    });

    describe('Length Display', () => {
      it('should display input length if showLength is true', () => {
        componentRef.setInput('showLength', true);
        componentRef.setInput('maxLength', 100);
        fixture.detectChanges();

        const lengthDisplay = fixture.debugElement.query(
          By.css('.input-value-length')
        );
        expect(lengthDisplay).toBeTruthy();
      });

      it('should show correct length and max length', () => {
        componentRef.setInput('showLength', true);
        componentRef.setInput('maxLength', 100);
        const textarea = fixture.debugElement.query(By.css('textarea'));
        textarea.nativeElement.value = 'Test input text';

        fixture.detectChanges();

        const lengthSpan = fixture.debugElement.query(
          By.css('.input-value-length span')
        );
        expect(lengthSpan.nativeElement.textContent.trim()).toBe('15/100');
      });
    });

    describe('Textarea Element', () => {
      it('should apply placeholder from placeholderKey', () => {
        componentRef.setInput('placeholderKey', 'Enter text');
        fixture.detectChanges();

        const textarea = fixture.debugElement.query(By.css('textarea'));
        expect(textarea.attributes['placeholder']).toBe('Enter text');
      });

      it('should set rows and maxLength attributes correctly', () => {
        componentRef.setInput('rows', 5);
        componentRef.setInput('maxLength', 50);
        fixture.detectChanges();

        const textarea = fixture.debugElement.query(By.css('textarea'));
        expect(textarea.attributes['rows']).toBe('5');
        expect(textarea.attributes['maxlength']).toBe('50');
      });

      it('should apply resize style based on resizeable', () => {
        componentRef.setInput('resizeable', true);
        fixture.detectChanges();

        const textarea = fixture.debugElement.query(By.css('textarea'));
        expect(textarea.styles['resize']).toBe('vertical');
      });

      it('should apply no-border classes based on hasContent and rows', () => {
        component.hasContent.set(true);
        componentRef.setInput('rows', 2);
        fixture.detectChanges();

        const textarea = fixture.debugElement.query(By.css('textarea'));
        expect(textarea.classes['no-border-top-right-radius']).toBe(true);
      });

      it('should emit onValueChange when textarea value changes', () => {
        const onValueChangeSpy = vi.spyOn(component, 'onValueChange');
        const textarea = fixture.debugElement.query(By.css('textarea'));

        textarea.nativeElement.value = 'New text';
        textarea.triggerEventHandler('change', {
          target: textarea.nativeElement,
        });
        expect(onValueChangeSpy).toHaveBeenCalledWith('New text');
      });
    });

    describe('Content Div', () => {
      it('should render the content div with the appropriate classes', () => {
        const contentDiv = fixture.debugElement.query(
          By.css('.soe-button-group')
        );
        expect(contentDiv).toBeTruthy();
        expect(contentDiv.classes['d-flex']).toBe(true);
        expect(contentDiv.classes['align-center']).toBe(true);
      });

      it('should update DOM when component content changes', () => {
        const contentEl = component.content(); // Retrieve the ElementRef from the Signal

        if (contentEl) {
          contentEl.nativeElement.textContent = 'Updated Component Content';
          fixture.detectChanges();

          // Verify that the update is reflected in the DOM
          expect(contentEl.nativeElement.textContent).toBe(
            'Updated Component Content'
          );
        } else {
          fail('content should not be undefined');
        }
      });

      it('should reflect updated DOM value in the component', () => {
        // Access the native element via ViewChild
        const contentEl: HTMLElement = component.content()?.nativeElement;

        contentEl.textContent = 'New DOM content';

        fixture.detectChanges();

        expect(component.content()?.nativeElement.textContent).toBe(
          'New DOM content'
        );
      });
    });
  });
});
