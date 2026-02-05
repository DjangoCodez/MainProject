import {
  AfterViewInit,
  Directive,
  ElementRef,
  Inject,
  Injector,
  OnDestroy,
  OnInit,
  ViewChild,
  input,
  model,
  signal,
} from '@angular/core';
import {
  ControlValueAccessor,
  FormControl,
  FormControlDirective,
  FormControlName,
  FormGroupDirective,
  NgControl,
  Validators,
} from '@angular/forms';
import { BrowserUtil } from '@shared/util/browser-util';
import { focusOnElement } from '@shared/util/focus-util';
import { Subject } from 'rxjs';
import { distinctUntilChanged, startWith, takeUntil } from 'rxjs/operators';

@Directive({
  selector: '[soeValueAccessor]',
  standalone: false,
})
export class ValueAccessorDirective<T>
  implements OnInit, AfterViewInit, OnDestroy, ControlValueAccessor
{
  autoFocus = input(false);
  autoFocusDelay = input<number | undefined>(200);
  disabled = input(false);
  readOnly = input(false);
  control!: FormControl;
  value = model<T>();
  protected _destroy$ = new Subject<void>();
  private _onTouched: (() => void) | undefined;
  private _isDisabled = false;
  isRequired = signal(false);
  formControlName = '';

  @ViewChild('input') inputER: ElementRef | undefined;

  constructor(@Inject(Injector) private injector: Injector) {}

  ngOnInit(): void {
    this.setComponentControl();
    if (this.control) {
      this.control.statusChanges
        .pipe(takeUntil(this._destroy$))
        .subscribe(() => this._updateRequiredState());
    }

    this._updateRequiredState();
  }

  private _updateRequiredState() {
    this.isRequired.set(
      this.control &&
        (this.control.hasValidator(Validators.required) ||
          this.control.hasValidator(Validators.requiredTrue)) // For checkboxes
    );
  }

  ngAfterViewInit(): void {
    this.autoFocus() &&
      this.inputER &&
      focusOnElement(this.inputER.nativeElement, this.autoFocusDelay());

    this.setTestId(this.formControlName);
  }

  setComponentControl(): void {
    try {
      // Will attempt to fetch the attached control
      const fc = this.injector.get(NgControl);

      switch (fc.constructor) {
        case FormControlName:
          this.control = this.injector
            .get(FormGroupDirective)
            .getControl(fc as FormControlName);
          this.formControlName = fc.name as string;
          break;
        default:
          this.control = (fc as FormControlDirective).form as FormControl;
          break;
      }
    } catch (err) {
      this.control = new FormControl();
    }
  }

  writeValue(v: T | undefined): void {
    // Bail if no changes has been made
    if (v === this.control?.value) return;
    if (v === undefined && this.control?.value === '') return;
    if (isNaN(Number(v))) return;

    if (this.control) {
      this.control.setValue(v);
    } else {
      this.control = new FormControl(v);
    }
  }

  public get controlValue(): T | null | undefined | '' {
    return this.control.value;
  }

  registerOnChange(_: (val: T | null) => T): void {
    this.control?.valueChanges
      .pipe(
        startWith(this.control.value),
        distinctUntilChanged(),
        takeUntil(this._destroy$)
      )
      .subscribe(() => this.control.markAsUntouched());
  }

  registerOnTouched(fn: () => void): void {
    this._onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this._isDisabled = isDisabled;
  }

  elemHasContent(elementRef?: ElementRef): boolean {
    return BrowserUtil.elementHasContent(elementRef);
  }

  setTestId(testId: string): void {
    if (testId) this.inputER?.nativeElement.setAttribute('data-testid', testId);
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
