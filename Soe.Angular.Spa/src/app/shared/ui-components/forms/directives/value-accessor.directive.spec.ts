import { Component, DebugElement } from '@angular/core';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import {
  ReactiveFormsModule,
  FormGroup,
  FormControl,
  Validators,
} from '@angular/forms';
import { By } from '@angular/platform-browser';
import { ValueAccessorDirective } from './value-accessor.directive';
import { vi } from 'vitest';

@Component({
  selector: 'app-test',
  standalone: false,
  template: `
    <form [formGroup]="form">
      <input formControlName="testControl" soeValueAccessor />
    </form>
  `,
})
class TestComponent {
  form = new FormGroup({
    testControl: new FormControl('', Validators.requiredTrue),
  });
}

describe('ValueAccessorDirective', () => {
  let fixture: ComponentFixture<TestComponent>;
  let component: TestComponent;
  let directive: ValueAccessorDirective<any>;
  let debugElement: DebugElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, ReactiveFormsModule],
      declarations: [TestComponent, ValueAccessorDirective],
    }).compileComponents();

    fixture = TestBed.createComponent(TestComponent);
    // component = fixture.componentInstance;
    debugElement = fixture.debugElement.query(
      By.directive(ValueAccessorDirective)
    );
    directive = debugElement.injector.get(ValueAccessorDirective);
  });

  describe('ngOnInit', () => {
    it('should set the control and check required state', () => {
      directive.control = new FormControl('', Validators.requiredTrue);
      // directive.control.hasValidator = jest.fn().mockReturnValue(true);
      vi.spyOn(directive.control, 'hasValidator').mockReturnValue(true);
      directive.ngOnInit();

      fixture.detectChanges();

      expect(directive.control).toBeTruthy();
      expect(directive.isRequired()).toBe(true);
    });

    it('should set isRequired to false if no required validator exists', () => {
      directive.control = new FormControl('');
      directive.ngOnInit();

      expect(directive.isRequired()).toBe(false);
    });
  });

  describe('ngAfterViewInit', () => {
    it('should focus the input element if autoFocus is true', () => {
      vi.useFakeTimers();
      const inputElement = document.createElement('input');
      const focusSpy = vi.spyOn(inputElement, 'focus');

      directive.inputER = { nativeElement: inputElement } as any;
      vi.spyOn(directive, 'autoFocus').mockReturnValue(true);

      directive.ngAfterViewInit();

      // Advance the fake timers to let setTimeout finish
      vi.runAllTimers();

      expect(focusSpy).toHaveBeenCalled();
    });

    it('should set a test ID if formControlName is available', () => {
      directive.formControlName = 'testControl';
      const inputElement = debugElement.nativeElement as HTMLInputElement;
      directive.inputER = { nativeElement: inputElement };

      directive.ngAfterViewInit();

      expect(inputElement.getAttribute('data-testid')).toBe('testControl');
    });
  });

  describe('writeValue', () => {
    it('should not set the value if it matches the current value', () => {
      directive.control = new FormControl('sameValue');
      const setValueSpy = vi.spyOn(directive.control, 'setValue');

      directive.writeValue('sameValue');

      expect(setValueSpy).not.toHaveBeenCalled();
    });
  });

  describe('registerOnChange', () => {
    it('should subscribe to value changes and mark the control as untouched', () => {
      directive.control = new FormControl('initial');
      const markUntouchedSpy = vi.spyOn(directive.control, 'markAsUntouched');
      directive.registerOnChange(() => {});

      directive.control.setValue('newValue');
      expect(markUntouchedSpy).toHaveBeenCalled();
    });
  });

  describe('registerOnTouched', () => {
    it('should set the _onTouched callback', () => {
      const onTouched = vi.fn();
      directive.registerOnTouched(onTouched);

      directive['_onTouched']!();
      expect(onTouched).toHaveBeenCalled();
    });
  });

  describe('setDisabledState', () => {
    it('should set the _isDisabled flag', () => {
      directive.setDisabledState(true);
      expect(directive['_isDisabled']).toBe(true);

      directive.setDisabledState(false);
      expect(directive['_isDisabled']).toBe(false);
    });
  });

  describe('elemHasContent', () => {
    it('should return true if element contains content', () => {
      const element = { nativeElement: { innerHTML: 'content' } };
      expect(directive.elemHasContent(element as any)).toBe(true);
    });

    it('should return false if element does not contain content', () => {
      const element = { nativeElement: { innerHTML: '<!--container-->' } };
      expect(directive.elemHasContent(element as any)).toBe(false);
    });
  });

  describe('setTestId', () => {
    it('should set the data-testid attribute on the input element', () => {
      const inputElement = document.createElement('input');
      directive.inputER = { nativeElement: inputElement };

      directive.setTestId('test-id');
      expect(inputElement.getAttribute('data-testid')).toBe('test-id');
    });
  });

  describe('ngOnDestroy', () => {
    it('should complete the _destroy$ subject', () => {
      const completeSpy = vi.spyOn(directive['_destroy$'], 'complete');
      directive.ngOnDestroy();

      expect(completeSpy).toHaveBeenCalled();
    });
  });
});
function Signal(arg0: boolean): import('@angular/core').InputSignal<boolean> {
  throw new Error('Function not implemented.');
}
