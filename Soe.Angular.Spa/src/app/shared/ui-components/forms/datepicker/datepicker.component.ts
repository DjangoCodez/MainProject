import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  computed,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  Inject,
  Injector,
  input,
  OnInit,
  output,
  ViewChild,
  ViewEncapsulation,
  OnDestroy,
} from '@angular/core';
import {
  NG_VALUE_ACCESSOR,
  NgControl,
  ReactiveFormsModule,
} from '@angular/forms';
import {
  DateAdapter,
  MAT_DATE_FORMATS,
  MAT_DATE_LOCALE,
  MatNativeDateModule,
} from '@angular/material/core';
import {
  MatCalendarCellClassFunction,
  MatDatepicker,
  MatDatepickerInputEvent,
  MatDatepickerModule,
} from '@angular/material/datepicker';
import { ShortcutService } from '@core/services/shortcut.service';
import { faCalendarDays } from '@fortawesome/pro-light-svg-icons';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { DateUtil } from '@shared/util/date-util'
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { IconModule } from '@ui/icon/icon.module'
import { LabelComponent } from '@ui/label/label.component';
import { ValueAccessorDirective } from '../directives/value-accessor.directive';
import { CustomDateAdapter } from './custom-date.adapter';
import { CustomDateFormat } from './mat-date-format.class';
import { Subscription } from 'rxjs';

export type DatepickerView = 'day' | 'week' | 'month' | 'year';

@Component({
  selector: 'soe-datepicker',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IconButtonComponent,
    IconModule,
    LabelComponent,
    TranslatePipe,
    MatNativeDateModule,
    MatDatepickerModule,
  ],
  templateUrl: './datepicker.component.html',
  styleUrls: ['./datepicker.component.scss'],
  encapsulation: ViewEncapsulation.None,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: DatepickerComponent,
    },
    { provide: MAT_DATE_LOCALE, useValue: DateUtil.getLocale('sv-SE') },
    { provide: NgControl, multi: true, useExisting: DatepickerComponent },
    {
      provide: DateAdapter,
      useClass: CustomDateAdapter,
      deps: [MAT_DATE_LOCALE],
    },
    {
      provide: MAT_DATE_FORMATS,
      useClass: CustomDateFormat,
      deps: [MAT_DATE_LOCALE],
    },
  ],
})
export class DatepickerComponent
  extends ValueAccessorDirective<Date | Date[]>
  implements OnInit, AfterViewInit, OnDestroy
{
  inputId = input<string>(Math.random().toString(24));
  labelKey = input('');
  secondaryLabelKey = input('');
  secondaryLabelBold = input(false);
  secondaryLabelParantheses = input(true);
  secondaryLabelPrefixKey = input('');
  secondaryLabelPostfixKey = input('');
  hasLabel = computed(() => {
    return (
      this.labelKey() ||
      this.secondaryLabelKey() ||
      this.secondaryLabelPrefixKey() ||
      this.secondaryLabelPostfixKey()
    );
  });
  inline = input(false);
  alignInline = input(false);
  width = input(0);
  view = input<DatepickerView>('day');
  hideToday = input(false);
  hideClear = input(false);
  showArrows = input(false);
  showLeftArrow = input(false); // used by the date range picker component
  showRightArrow = input(false); // used by the date range picker component
  hideCalendarButton = input(false);
  description = input('');
  minDate = input<Date>();
  maxDate = input<Date>();
  initialDate = input<Date | undefined>(undefined);
  manualDisabled = input(false);
  manualReadOnly = input(false);
  lastInPeriod = input(false); // When having period view (other than 'day') set date of last day. Default is first day.
  gridMode = input(false);
  inRangepicker = input(false); // used by the date range picker component

  isDisabled = computed(() => {
    return this.disabled() || this.manualDisabled();
  });

  isReadOnly = computed(() => this.readOnly() || this.manualReadOnly());

  valueChanged = output<Date | undefined>();
  stepRight = output<Date | undefined>();
  stepLeft = output<Date | undefined>();
  closed = output<void>();

  @ViewChild('picker') datePicker?: MatDatepicker<Date>;
  @ViewChild('pickerInput') datePickerInput?: ElementRef;

  private shortcutService = inject(ShortcutService);

  isFirstChange = true;
  readonly faCalendarDays = faCalendarDays;
  readonly currentLanguage = SoeConfigUtil.language;
  readonly dateFormatFns = DateUtil.dateFnsLanguageDateFormats;
  readonly dateFormat = DateUtil.languageDateFormat;
  dateFormatText = DateUtil.languageDateFormatText;

  tmpValue = '';
  dateAdapter = inject(DateAdapter<Date>);
  translate = inject(TranslateService);

  private valueChangesSub?: Subscription;

  constructor(
    @Inject(MAT_DATE_FORMATS) public matDateFormat: CustomDateFormat,
    private element: ElementRef,
    private destroyRef: DestroyRef
  ) {
    super(inject(Injector));

    effect(() => {
      if (this.control) {
        if (this.disabled() || this.manualDisabled()) this.control.disable();
        else this.control.enable();
      }
    });
  }

  dateClass: MatCalendarCellClassFunction<Date> = (cellDate: Date, view) => {
    if (view === 'month') {
      const day = cellDate.getDay();
      const isMonOrSunday = day === 0 || day === 1;
      let prefix = '';

      if (isMonOrSunday) {
        const week = DateUtil.getWeekNumber(cellDate);
        prefix += `show-iso-week iso-week-${week} day-of-week-${day}`;
      }

      return prefix;
    } else if (view === 'year') {
      return 'mat-month-cell';
    }
    return '';
  };

  ngOnInit(): void {
    super.ngOnInit();

    this.dateAdapter.setLocale(SoeConfigUtil.language);

    if (this.view() !== 'day') {
      this.dateFormatText = this.translate.instant('core.choose');

      if (this.control) {
        this.valueChangesSub = this.control.valueChanges.subscribe(d => {
          d = this.getFirstOrLast(d);
          this.control.patchValue(d, { emitEvent: false });
        });
      }
    }

    if (this.initialDate()) {
      this.control.patchValue(this.initialDate(), { emitEvent: false });
    }

    this.updateDateFormats();
    this.initValue();
  }

  ngAfterViewInit(): void {
    super.ngAfterViewInit();

    this.setupKeyboardShortcuts();

    if (this.gridMode()) {
      setTimeout(() => {
        this.datePickerInput?.nativeElement.focus();
        this.datePickerInput?.nativeElement.select();
      });
    }
    this.datePicker!.closedStream.subscribe(() => {
      this.closed.emit();
    });
    this.datePicker!.openedStream.subscribe(() => {
      // When opening the calendar popup and no value is set, open to today
      if (
        this.currentDate == null ||
        this.currentDate == undefined ||
        DateUtil.isDefaultDateTime(this.currentDate)
      ) {
        const today = DateUtil.getToday();
        this.currentDate = today;
        this.datePicker!.startAt = today;
        this.control.patchValue(today);
      }
    });
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
    this.valueChangesSub?.unsubscribe();
  }

  private updateDateFormats() {
    this.matDateFormat.updateDateFormat({
      dateInput: DateUtil.getDateFormatForView(this.view()),
    });
  }

  private initValue() {
    if (this.control) {
      // Set date of first or last day in period according to view
      const date = this.getFirstOrLast(this.currentDate);
      // Clear time from value
      if (date instanceof Date) date.clearHours();
      this.control.patchValue(date);
    }
  }

  private setupKeyboardShortcuts() {
    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Insert'],
      e => this.setToday()
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Delete'],
      e => this.clearDatepickerValue()
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['ArrowUp'],
      e => this.stepDay(1)
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['ArrowDown'],
      e => this.stepDay(-1)
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['PageUp'],
      e => this.stepMonth(1)
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['PageDown'],
      e => this.stepMonth(-1)
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Home'],
      e => this.stepYear(1)
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['End'],
      e => this.stepYear(-1)
    );
  }

  getStartView(): 'month' | 'year' | 'multi-year' {
    switch (this.view()) {
      case 'week':
      case 'day':
        return 'month';
      case 'month':
        return 'year';
      case 'year':
        return 'multi-year';
    }
  }

  // EVENTS

  formatDateOnChange(event: MatDatepickerInputEvent<Date>) {
    let changedDate: Date;
    if (this.view() === 'day') {
      changedDate = DateUtil.parseDateByString(
        this.datePickerInput?.nativeElement.value
      ) as Date;
    } else {
      changedDate = event.value as Date;
    }
    changedDate = this.getFirstOrLast(changedDate);
    this.setCurrentDate(changedDate);
  }

  switchToToday(): void {
    const d = new Date();
    this.setCurrentDate(d);
    this.datePicker?.close();
  }

  clearDatepickerValue(): void {
    if (this.disabled() || this.manualDisabled()) return;

    this.setCurrentDate(undefined);
    this.datePicker?.close();
  }

  setMonth(event: Date) {
    this.setCurrentDate(event);
    this.datePicker?.close();
  }

  setYear(event: Date) {
    this.setCurrentDate(event);
    this.datePicker?.close();
  }

  step(value: number) {
    switch (this.view()) {
      case 'week':
        this.stepDay(value * 7);
        break;
      case 'day':
        this.stepDay(value);
        break;
      case 'month':
        this.stepMonth(value);
        break;
      case 'year':
        this.stepYear(value);
        break;
    }
  }

  // HELP-METHODS

  getView() {
    return this.view();
  }

  private get currentDate(): Date {
    return this.control.value as Date;
  }

  private set currentDate(date: Date | undefined) {
    if (this.control) this.control.patchValue(date != undefined ? date : '');
  }

  private setCurrentDate(
    date: Date | undefined,
    suppressEmitChange: boolean = false
  ) {
    this.currentDate = date;
    if (!suppressEmitChange) {
      this.valueChanged.emit(date);
    }
  }

  private getFirstOrLast(date: Date) {
    if (date) {
      if (this.lastInPeriod()) {
        switch (this.view()) {
          case 'week':
            date = DateUtil.getDateLastInWeek(date);
            break;
          case 'month':
            date = DateUtil.getDateLastInMonth(date);
            break;
          case 'year':
            date = DateUtil.getDateLastInYear(date);
            break;
        }
      } else {
        switch (this.view()) {
          case 'week':
            date = DateUtil.getDateFirstInWeek(date);
            break;
          case 'month':
            date = DateUtil.getDateFirstInMonth(date);
            break;
          case 'year':
            date = DateUtil.getDateFirstInYear(date);
            break;
        }
      }
    }
    return date;
  }

  // KEYBOARD SHORTCUTS

  private setToday() {
    if (this.disabled() || this.manualDisabled()) return;

    this.currentDate = DateUtil.getToday();
  }

  private stepDay(value: number) {
    if (this.datePicker?.opened || this.disabled() || this.manualDisabled())
      return;

    this.notifyStep(value);

    if (!this.inRangepicker()) {
      const newDate = this.dateAdapter.addCalendarDays(this.currentDate, value);
      this.setCurrentDate(newDate);
    }
  }

  private stepMonth(value: number) {
    if (this.datePicker?.opened || this.disabled() || this.manualDisabled())
      return;

    this.notifyStep(value);
    if (!this.inRangepicker()) {
      const newDate = this.dateAdapter.addCalendarMonths(
        this.currentDate,
        value
      );
      this.setCurrentDate(newDate);
    }
  }

  private stepYear(value: number) {
    if (this.datePicker?.opened || this.disabled() || this.manualDisabled())
      return;

    this.notifyStep(value);
    if (!this.inRangepicker()) {
      const newDate = this.dateAdapter.addCalendarYears(
        this.currentDate,
        value
      );
      this.setCurrentDate(newDate);
    }
  }

  notifyStep(value: number) {
    if (this.inRangepicker() && value > 0) {
      this.stepRight.emit(this.currentDate);
    } else if (this.inRangepicker() && value < 0) {
      this.stepLeft.emit(this.currentDate);
    }
  }

  formattedDate() {
    if (!this.currentDate) return ' ';
    return DateUtil.localeDateFormat(this.currentDate) || ' ';
  }
}
