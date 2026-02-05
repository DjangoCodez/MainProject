import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  computed,
  input,
  model,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { DateUtil } from '@shared/util/date-util';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { LabelComponent } from '@ui/label/label.component';
import { Subscription } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';

export type TimeboxValue = string | Date | number;

@Component({
  selector: 'soe-timebox',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LabelComponent,
    TranslatePipe,
    IconButtonComponent,
  ],
  templateUrl: './timebox.component.html',
  styleUrls: ['./timebox.component.scss'],
  providers: [
    { provide: NG_VALUE_ACCESSOR, multi: true, useExisting: TimeboxComponent },
  ],
})
export class TimeboxComponent
  extends ValueAccessorDirective<string | Date>
  implements OnInit, AfterViewInit, OnDestroy
{
  inputId = input<string>(Math.random().toString(24));
  labelKey = input('');
  labelLowercase = input(false);
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  secondaryLabelLowercase = input(false);
  disableDurationFormatting = input(false);
  hasLabel = computed(() => {
    return (
      this.labelKey() ||
      this.secondaryLabelKey() ||
      this.secondaryLabelPrefixKey() ||
      this.secondaryLabelPostfixKey()
    );
  });
  placeholderKey = model('core.time.placeholder.hoursminutes');
  inline = input(false);
  alignInline = input(false);
  width = input(0);
  isDuration = input(false);
  leadingZero = input(false);
  allowNegative = input(true);
  showDateAsTooltip = input(false);
  gridMode = input(false);
  showLeftRangeArrow = input(false);
  showRightRangeArrow = input(false);
  inRange = input(false);
  manualDisabled = input(false);

  valueChanged = output<TimeboxValue>();
  stepRight = output<number | undefined>();
  stepLeft = output<number | undefined>();

  content = viewChild<ElementRef>('content');
  input = viewChild<ElementRef>('input');

  hasContent = signal<boolean>(false);
  tooltipDate = signal<string | null>(null);

  isDisabled = computed(() => {
    return (
      (this.control && this.control.disabled) ||
      this.disabled() ||
      this.manualDisabled()
    );
  });

  private valueChangesSub?: Subscription;

  timeSpan = '';

  ngOnInit(): void {
    super.ngOnInit();

    if (
      this.isDuration() &&
      this.placeholderKey() === 'core.time.placeholder.hoursminutes'
    )
      this.placeholderKey.set('core.time.placeholder.isduration.hoursminutes');

    // Update and reformat data on init
    this.updateVisibleValue(this.control.value);
    this.formatValue(undefined, false);

    this.valueChangesSub = this.control.valueChanges.subscribe(value => {
      this.updateVisibleValue(value);
    });
  }

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    if (this.elemHasContent(this.content())) this.hasContent.set(true);

    if (this.gridMode()) {
      setTimeout(() => {
        this.input()?.nativeElement.focus();
        this.input()?.nativeElement.select();
      });
    }
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
    this.valueChangesSub?.unsubscribe();
  }

  updateVisibleValue(value: string | Date | number): void {
    const isEmpty =
      value === undefined ||
      value === null ||
      value.toString() === '' ||
      value.toString() === 'undefined' ||
      value.toString() === 'null';
    const isDate = value instanceof Date;
    const isNumeric = !isNaN(Number(value));

    if (isEmpty) {
      this.timeSpan = '';
    } else if (isDate && DateUtil.isValidDate(value)) {
      this.timeSpan = DateUtil.localeTimeFormat(value, true);
      this.setTooltipDate();
    } else if (isNumeric && this.isDuration()) {
      this.timeSpan = DateUtil.minutesToTimeSpan(Number(value));
    } else {
      this.timeSpan = value?.toString();
    }
  }

  formatValue(elem?: any, markAsDirty = true): void {
    let value = elem ? elem.value : this.timeSpan;
    if (!value) {
      if (this.isDuration()) {
        value = '0:00'; // Always reset value to 0:00 on empty when isDuration
      } else {
        const notify = this.control.value !== '';
        this.control.patchValue('');
        this.timeSpan = '';
        // Need to do this in a timeout for the container form to get the updated value
        // before emitting the change event, otherwise the form value will be one step behind
        setTimeout(() => {
          if (notify) this.emitChange('', markAsDirty);
        }, 0);
        return;
      }
    }

    if (value.charAt(0) === '-' && !this.allowNegative()) {
      value = value.replace('-', '');
    }

    let timeSpan = '';
    // TODO: padHours hard coded to true for Swedish time format
    const padHours = !this.isDuration() || this.leadingZero();

    let parts: string[] = [];
    let isHundreds: boolean = false;
    if (value.includes(':')) {
      parts = value.split(':');
    } else if (value.includes(',')) {
      parts = value.split(',');
      isHundreds = true;
    } else if (value.includes('.')) {
      parts = value.split('.');
      isHundreds = true;
    } else if (this.disableDurationFormatting()) {
      timeSpan = DateUtil.minutesToTimeSpan(
        parseInt(value, 10) * 60,
        false,
        false,
        padHours
      );
    } else {
      timeSpan = DateUtil.parseTimeSpan(value, false, padHours);
    }

    if (parts.length > 0) {
      let hours = parseInt(parts[0], 10);
      let minutes = 0;
      if (parts.length > 1) {
        if (isHundreds) {
          // Make sure minute part is two digits (eg: 0,25 = 25, 0,5 == 50, 0,750 = 75).
          // Otherwise convertion from hundreds to minutes below will be wrong.
          minutes = parseInt(parts[1].substring(0, 2).padEnd(2, '0'));
          minutes = ((minutes * 60) / 100).round(0);
        } else {
          minutes = parseInt(parts[1], 10);
        }
      }
      while (minutes > 59) {
        minutes -= 60;
        hours++;
      }
      if (!this.isDuration()) {
        while (hours > 23) {
          hours -= 24;
        }
      }

      timeSpan = DateUtil.minutesToTimeSpan(
        hours * 60 + minutes,
        false,
        false,
        padHours
      );
    }

    const isModified = this.timeSpan !== timeSpan;
    this.timeSpan = timeSpan;

    if (this.isDuration()) {
      // Duration
      this.control.patchValue(
        this.gridMode() ? DateUtil.timeSpanToMinutes(timeSpan) : timeSpan
      );
    } else {
      // Time
      let date: Date | undefined;
      if (this.control.value?.toString().length > 5) {
        date = DateUtil.parseDateOrJson(this.control.value);
        if (!date || !DateUtil.isValidDate(date))
          date = DateUtil.defaultDateTime();
      } else {
        date = DateUtil.defaultDateTime();
      }

      date.mergeTimeSpan(timeSpan, true);
      this.control.patchValue(date);
    }

    if (isModified) this.emitChange(this.control.value, markAsDirty);
  }

  private setTooltipDate(): void {
    if (!this.showDateAsTooltip()) this.tooltipDate.set(null);

    const value = this.control?.value;
    if (value instanceof Date && DateUtil.isValidDate(value)) {
      this.tooltipDate.set(DateUtil.localeDateFormat(value));
    } else {
      this.tooltipDate.set(null);
    }
  }

  private emitChange(value: TimeboxValue, markAsDirty: boolean): void {
    this.valueChanged.emit(value);
    if (markAsDirty) {
      this.control.markAsDirty();
      this.control.markAsTouched();
    }
  }

  notifyRangeStepRight() {
    this.stepRight.emit(Number(this.control.value));
  }

  notifyRangeStepLeft() {
    this.stepLeft.emit(Number(this.control.value));
  }
}
