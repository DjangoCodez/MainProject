import {
  Component,
  effect,
  inject,
  input,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';
import { VoucherService } from '@features/economy/voucher/services/voucher.service';
import { SoeDateFormControl, SoeSelectFormControl } from '@shared/extensions';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  TermGroup,
  TermGroup_DatePeriodSelection,
} from '@shared/models/generated-interfaces/Enumerations';
import { IAccountYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { forkJoin, merge, Observable, of, Subject, takeUntil, tap } from 'rxjs';
import { DateUtil } from '@shared/util/date-util';

@Component({
  selector: 'soe-date-period',
  templateUrl: './date-period.component.html',
  styleUrls: ['./date-period.component.scss'],
  standalone: false,
})
export class DatePeriodComponent implements OnInit, OnDestroy {
  labelKey = input<string>('');
  dateFromControl = input.required<SoeDateFormControl>();
  dateToControl = input.required<SoeDateFormControl>();
  defaultPeriod = input<TermGroup_DatePeriodSelection>(
    TermGroup_DatePeriodSelection.Today
  );
  manualDisabled = input<boolean>(false);
  private readonly coreService = inject(CoreService);
  private readonly voucherService = inject(VoucherService);
  private readonly ayService = inject(PersistedAccountingYearService);
  private _destroy$ = new Subject<void>();

  protected datePeriodControl: SoeSelectFormControl = new SoeSelectFormControl(
    undefined
  );
  protected datePeriodSelectionOptions: SmallGenericType[] = [];
  protected showDateControls = signal(false);
  protected selectedDates = signal('');

  private currentAccountYear?: IAccountYearDTO;
  private previousAccountYear?: IAccountYearDTO;

  private get isCurrentYearAccountYearSame(): boolean {
    const firstDate = new Date(DateUtil.getToday().getFullYear(), 0, 1);
    const lastDate = new Date(DateUtil.getToday().getFullYear(), 11, 31);
    return (
      !!this.currentAccountYear &&
      this.datesEqual(this.currentAccountYear.from, firstDate) &&
      this.datesEqual(this.currentAccountYear.to, lastDate)
    );
  }

  private get isPreviousYearPreviousAccountYearSame(): boolean {
    const firstDate = new Date(DateUtil.getToday().getFullYear() - 1, 0, 1);
    const lastDate = new Date(DateUtil.getToday().getFullYear() - 1, 11, 31);
    return (
      !!this.previousAccountYear &&
      this.datesEqual(this.previousAccountYear.from, firstDate) &&
      this.datesEqual(this.previousAccountYear.to, lastDate)
    );
  }

  constructor() {
    this.loadData();

    effect(() => {
      const dPeriod = this.defaultPeriod();
      this.datePeriodControl.setValue(dPeriod.valueOf());
    });

    effect(() => {
      const disabled = this.manualDisabled();
      if (disabled) this.datePeriodControl.disable();
      else this.datePeriodControl.enable();
    });
  }

  ngOnInit(): void {
    this.datePeriodControl.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(v => {
        this.showDateControls.set(v === TermGroup_DatePeriodSelection.Custom);
        this.setDates(<TermGroup_DatePeriodSelection>v);
      });

    merge([
      this.dateFromControl().statusChanges,
      this.dateToControl().statusChanges,
    ])
      .pipe(takeUntil(this._destroy$))
      .subscribe(() => {
        if (this.dateFromControl().disabled || this.dateToControl().disabled) {
          this.datePeriodControl.disable();
        } else {
          this.datePeriodControl.enable();
        }
      });
  }

  private datesEqual(date1: Date, date2: Date): boolean {
    return (
      date1.getFullYear() === date2.getFullYear() &&
      date1.getMonth() === date2.getMonth() &&
      date1.getDate() === date2.getDate()
    );
  }

  private loadData(): void {
    forkJoin([this.loadAccountYear(), this.loadPeriodSelection()])
      .pipe(takeUntil(this._destroy$))
      .subscribe(
        ([_, periods]: [IAccountYearDTO | undefined, SmallGenericType[]]) => {
          if (this.isCurrentYearAccountYearSame)
            periods = periods.filter(
              p => p.id !== TermGroup_DatePeriodSelection.CurrentFinancialYear
            );
          if (this.isPreviousYearPreviousAccountYearSame)
            periods = periods.filter(
              p => p.id !== TermGroup_DatePeriodSelection.PreviousFinancialYear
            );

          if (this.ayService.selectedAccountYearId() <= 0)
            periods = periods.filter(
              p =>
                p.id !== TermGroup_DatePeriodSelection.CurrentFinancialYear &&
                p.id !== TermGroup_DatePeriodSelection.PreviousFinancialYear
            );
          this.datePeriodSelectionOptions = periods;
        }
      );
  }

  private loadPeriodSelection(): Observable<SmallGenericType[]> {
    return this.coreService.getTermGroupContent(
      TermGroup.DatePeriodSelection,
      false,
      true,
      true
    );
  }

  private loadAccountYear() {
    return this.ayService.ensureAccountYearIsLoaded$(() => {
      this.currentAccountYear =
        this.ayService.selectedAccountYear() ?? undefined;
      return this.loadPreviousYear();
    });
  }

  private loadPreviousYear() {
    if (!this.currentAccountYear) return of(undefined);

    return this.voucherService
      .getAccountYearByDate(
        DateUtil.getToday().addYears(-1).toDateTimeString(),
        true
      )
      .pipe(
        tap(year => {
          this.previousAccountYear = year;
        })
      );
  }

  private setDates(periodSelection: TermGroup_DatePeriodSelection): void {
    switch (periodSelection) {
      case TermGroup_DatePeriodSelection.Today:
        this.dateFromControl().setValue(DateUtil.getToday());
        this.dateToControl().setValue(DateUtil.getToday());
        break;
      case TermGroup_DatePeriodSelection.Yesterday:
        this.dateFromControl().setValue(DateUtil.getToday().addDays(-1));
        this.dateToControl().setValue(DateUtil.getToday().addDays(-1));
        break;
      case TermGroup_DatePeriodSelection.CurrentWeek:
        this.dateFromControl().setValue(
          DateUtil.getDateFirstInWeek(DateUtil.getToday())
        );
        this.dateToControl().setValue(
          DateUtil.getDateLastInWeek(DateUtil.getToday())
        );
        break;
      case TermGroup_DatePeriodSelection.PreviousWeek:
        this.dateFromControl().setValue(
          DateUtil.getDateFirstInWeek(DateUtil.getToday().addWeeks(-1))
        );
        this.dateToControl().setValue(
          DateUtil.getDateLastInWeek(DateUtil.getToday().addWeeks(-1))
        );
        break;
      case TermGroup_DatePeriodSelection.CurrentMonth:
        this.dateFromControl().setValue(
          DateUtil.getDateFirstInMonth(DateUtil.getToday())
        );
        this.dateToControl().setValue(
          DateUtil.getDateLastInMonth(DateUtil.getToday())
        );
        break;
      case TermGroup_DatePeriodSelection.PreviousMonth:
        this.dateFromControl().setValue(
          DateUtil.getDateFirstInMonth(DateUtil.getToday().addMonths(-1))
        );
        this.dateToControl().setValue(
          DateUtil.getDateLastInMonth(DateUtil.getToday().addMonths(-1))
        );
        break;
      case TermGroup_DatePeriodSelection.CurrentYear:
        this.dateFromControl().setValue(
          DateUtil.getDateFirstInYear(DateUtil.getToday())
        );
        this.dateToControl().setValue(
          DateUtil.getDateLastInYear(DateUtil.getToday())
        );
        break;
      case TermGroup_DatePeriodSelection.PreviousYear:
        this.dateFromControl().setValue(
          DateUtil.getDateFirstInYear(DateUtil.getToday().addYears(-1))
        );
        this.dateToControl().setValue(
          DateUtil.getDateLastInYear(DateUtil.getToday().addYears(-1))
        );
        break;
      case TermGroup_DatePeriodSelection.CurrentFinancialYear:
        if (this.currentAccountYear) {
          this.dateFromControl().setValue(this.currentAccountYear.from);
          this.dateToControl().setValue(this.currentAccountYear.to);
        }
        break;
      case TermGroup_DatePeriodSelection.PreviousFinancialYear:
        if (this.previousAccountYear) {
          this.dateFromControl().setValue(this.previousAccountYear.from);
          this.dateToControl().setValue(this.previousAccountYear.to);
        }
        break;
      case TermGroup_DatePeriodSelection.Custom:
        this.dateFromControl().setValue(undefined);
        this.dateToControl().setValue(undefined);
        break;
    }

    this.setDateLabel(periodSelection);
  }

  private setDateLabel(periodSelection: TermGroup_DatePeriodSelection): void {
    switch (periodSelection) {
      case TermGroup_DatePeriodSelection.Today:
      case TermGroup_DatePeriodSelection.Yesterday:
        this.selectedDates.set(
          DateUtil.parseDateOrJson(
            this.dateFromControl().value
          )?.toLocaleDateString() ?? ''
        );
        break;
      case TermGroup_DatePeriodSelection.CurrentWeek:
      case TermGroup_DatePeriodSelection.PreviousWeek:
      case TermGroup_DatePeriodSelection.CurrentMonth:
      case TermGroup_DatePeriodSelection.PreviousMonth:
      case TermGroup_DatePeriodSelection.CurrentYear:
      case TermGroup_DatePeriodSelection.PreviousYear:
      case TermGroup_DatePeriodSelection.CurrentFinancialYear:
      case TermGroup_DatePeriodSelection.PreviousFinancialYear:
        {
          let dateStr = '';

          dateStr =
            DateUtil.parseDateOrJson(
              this.dateFromControl().value
            )?.toLocaleDateString() ?? '';

          dateStr +=
            ' - ' +
            (DateUtil.parseDateOrJson(
              this.dateToControl().value
            )?.toLocaleDateString() ?? '');
          this.selectedDates.set(dateStr);
        }
        break;
      case TermGroup_DatePeriodSelection.Custom:
        this.selectedDates.set('');
        break;
    }
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
