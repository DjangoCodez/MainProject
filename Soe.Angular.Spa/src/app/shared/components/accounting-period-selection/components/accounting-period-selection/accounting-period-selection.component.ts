import {
  Component,
  effect,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { ValidationHandler } from '../../../../handlers/validation.handler';
import {
  AccountingPeriodSelectionForm,
  validateDateRange,
} from '../../models/accounting-period-selection-form.model';
import { IAccountingPeriodSelection } from '../../models/accounting-period-selection.model';
import { AccountingPeriodSelectionService } from '../../services/accounting-period-selection.service';
import {
  IAccountPeriodDTO,
  IAccountYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SoeFormControl, SoeFormGroup } from '@shared/extensions';
import { distinctUntilChanged, Observable, of, tap } from 'rxjs';
import { DateUtil } from '@shared/util/date-util'
import { addEmptyOption } from '@shared/util/array-util';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';

@Component({
  selector: 'soe-accounting-period-selection',
  templateUrl: './accounting-period-selection.component.html',
  styleUrl: './accounting-period-selection.component.scss',
  providers: [AccountingPeriodSelectionService],
  standalone: false,
})
export class AccountingPeriodSelectionComponent implements OnInit {
  containerForm = input.required<SoeFormGroup>();
  hideAccountingYearTo = input<boolean>(false);
  valueChanged = output<IAccountingPeriodSelection>();
  loaded = output<void>();
  private readonly service = inject(AccountingPeriodSelectionService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly ayService = inject(PersistedAccountingYearService);

  private fromAccountYear?: IAccountYearDTO;
  private toAccountYear?: IAccountYearDTO;
  protected accountYears = signal<IAccountYearDTO[]>([]);
  protected fromPeriods = signal<ISmallGenericType[]>([]);
  protected toPeriods = signal<ISmallGenericType[]>([]);
  protected readonly form = new AccountingPeriodSelectionForm({
    validationHandler: this.validationHandler,
    element: <IAccountingPeriodSelection>{},
  });

  constructor() {
    this.form?.accountingYearFrom.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(fromAccountYear => {
        this.toggleControl(
          this.form?.monthFrom,
          fromAccountYear && fromAccountYear > 0
        );
        if (this.hideAccountingYearTo()) {
          this.toggleControl(
            this.form?.monthTo,
            fromAccountYear && fromAccountYear > 0
          );
        }

        if (typeof fromAccountYear === 'number') {
          this.setFromIntervals(fromAccountYear, true);
        }
      });

    this.form?.accountingYearTo.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(toAccountYear => {
        this.toggleControl(
          this.form?.monthTo,
          toAccountYear && toAccountYear > 0
        );
        if (typeof toAccountYear === 'number') {
          this.setToIntervals(toAccountYear);
        }
      });

    this.form?.monthFrom.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed())
      .subscribe(monthFrom => {
        if (typeof monthFrom === 'number' && this.hideAccountingYearTo()) {
          this.form.monthTo.setValue(monthFrom);
        }
      });

    this.form?.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      this.emitChange();
    });

    effect(() => {
      this.form.accountingYears = this.accountYears();

      if (this.containerForm()) {
        this.containerForm().addControl('accountingPeriod', this.form);
        if (!this.containerForm().hasValidator(validateDateRange()))
          this.containerForm().addValidators(validateDateRange());
      }
      this.containerForm().updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    this.initAccountYears();
  }

  private initAccountYears(): void {
    this.ayService
      .ensureAccountYearIsLoaded$(() =>
        this.getAccountYears(this.ayService.selectedAccountYearId())
      )
      .subscribe(() => {
        this.loaded.emit();
      });
  }

  private getAccountYears(
    currentAccountYearId: number
  ): Observable<IAccountYearDTO[]> {
    return this.service.getAccountYears().pipe(
      tap(years => {
        addEmptyOption(years);
        this.accountYears.set(years);

        this.form?.setCurrentAccountYear(currentAccountYearId);
      })
    );
  }

  private getAccountYearIntervals(
    accountYearId: number
  ): Observable<IAccountYearDTO | undefined> {
    if (!accountYearId || accountYearId === 0) return of(undefined);
    return this.service.getAccountYearIntervals(accountYearId);
  }

  private setFromIntervals(
    accountYearId: number,
    loadToIntervals: boolean = false
  ): void {
    this.getAccountYearIntervals(accountYearId)
      .pipe(
        tap(accountYear => {
          if (accountYear) {
            this.form.fromAccountYearDto = accountYear;
            this.fromPeriods.set(this.transformPeriods(accountYear?.periods));

            //set period start
            this.form?.monthFrom.setValue(
              this.fromPeriods()
                .filter(x => x.id !== 0)
                .sort(z => Number(z.name))[
                this.getPeriodListSortLength(this.fromPeriods()) > 0 ? 0 : 0
              ]?.id ?? 0,
              {
                emitEvent: true,
                emitModelToViewChange: true,
                emitViewToModelChange: false,
              }
            );
          } else {
            this.form?.monthFrom.setValue(0);
          }

          if (loadToIntervals) this.updateToInIntervals(accountYear);
        })
      )
      .subscribe();
  }

  private setToIntervals(accountYearId: number): void {
    this.getAccountYearIntervals(accountYearId)
      .pipe(
        tap(accountYear => {
          this.updateToInIntervals(accountYear);
        })
      )
      .subscribe();
  }

  private updateToInIntervals(accountYear?: IAccountYearDTO): void {
    if (accountYear) {
      this.form.toAccountYearDto = accountYear;
      this.toPeriods.set(this.transformPeriods(accountYear.periods));

      //set period end
      this.form?.monthTo.setValue(
        this.toPeriods()
          .filter(x => x.id !== 0)
          .sort(z => Number(z.name))[
          this.getPeriodListSortLength(this.toPeriods()) > 0
            ? this.getPeriodListSortLength(this.toPeriods()) - 1
            : 0
        ]?.id ?? 0,
        {
          emitEvent: true,
          emitModelToViewChange: true,
          emitViewToModelChange: false,
        }
      );
    } else {
      this.form?.monthTo.setValue(0);
    }
  }

  private emitChange(): void {
    // if (!isValidInterval || !this.form?.isValidSelection()) {
    //   this.periodSelectionChanged.emit(undefined);
    //   return;
    // }

    this.form?.populateDates();
    this.valueChanged.emit(this.form?.value);
  }

  private transformPeriods(periods: IAccountPeriodDTO[]): ISmallGenericType[] {
    const _periods = periods.map(
      x =>
        <ISmallGenericType>{
          id: x.accountPeriodId,
          name: x.startValue,
        }
    );
    addEmptyOption(_periods);

    return _periods;
  }

  private getFromDate(intervalId: number): Date | undefined {
    const interval = this.fromAccountYear?.periods.find(
      x => x.accountPeriodId === intervalId
    );
    if (interval) {
      return DateUtil.parseDateOrJson(interval.from);
    } else return undefined;
  }

  private getToDate(intervalId: number): Date | undefined {
    const interval = this.toAccountYear?.periods.find(
      x => x.accountPeriodId === intervalId
    );
    if (interval) {
      return DateUtil.parseDateOrJson(interval.to);
    } else return undefined;
  }

  private toggleControl(control: SoeFormControl, enable: boolean): void {
    if (enable) control.enable();
    else control.disable();
  }

  private getPeriodListSortLength(periods: ISmallGenericType[]): number {
    return periods.filter(x => x.id !== 0).sort(z => Number(z.name)).length;
  }
}
