import { Component, inject, OnInit, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DistributionSalesEuGridFilterForm } from '@features/economy/distribution-sales-eu/models/distribution-sales-eu-grid-filter-form.model';
import { DistributionSalesEuFilterDTO } from '@features/economy/distribution-sales-eu/models/distribution-sales-eu.model';
import { DistributionSalesEuService } from '@features/economy/distribution-sales-eu/services/distribution-sales-eu.service';
import { SoeFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DatePeriodType } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountPeriodDTO,
  IAccountYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util'
import { SoeConfigUtil } from '@shared/util/soeconfig-util'
import { addEmptyOption } from '@shared/util/array-util';
import { Observable, pairwise, switchMap, tap } from 'rxjs';

@Component({
  selector: 'soe-distribution-sales-eu-grid-filter',
  templateUrl: './distribution-sales-eu-grid-filter.component.html',
  styleUrl: './distribution-sales-eu-grid-filter.component.scss',
  standalone: false,
})
export class DistributionSalesEuGridFilterComponent implements OnInit {
  dateSelectionChanged = output<DistributionSalesEuFilterDTO | undefined>();

  private readonly validationHandler = inject(ValidationHandler);
  protected readonly form = new DistributionSalesEuGridFilterForm({
    validationHandler: this.validationHandler,
    element: new DistributionSalesEuFilterDTO(),
  });

  private readonly service = inject(DistributionSalesEuService);
  private accountYear?: IAccountYearDTO;
  private months: ISmallGenericType[] = [];
  private fromQuarters: ISmallGenericType[] = [];
  private toQuarters: ISmallGenericType[] = [];
  protected accountYears = signal<ISmallGenericType[]>([]);
  protected fromPeriods = signal<ISmallGenericType[]>([]);
  protected toPeriods = signal<ISmallGenericType[]>([]);

  constructor() {
    this.form?.accountYear.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(accountYear => {
        this.toggleControl(
          this.form?.fromInterval,
          accountYear && accountYear > 0
        );
        this.toggleControl(
          this.form?.toInterval,
          accountYear && accountYear > 0
        );
        this.setIntervals(accountYear);
      });

    this.form?.reportPeriod.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(reportPeriod => {
        this.changeReportPeriod(reportPeriod);
      });

    this.form?.fromInterval.valueChanges
      .pipe(pairwise(), takeUntilDestroyed())
      .subscribe(([prev, next]: [number, number]) => {
        if (next != 0) {
          if (next && prev != next) {
            if (!isNaN(next)) {
              this.form.fromInterval.setValue(Number(next), {
                emitEvent: false,
                emitModelToViewChange: false,
                emitViewToModelChange: false,
              });
              this.emitChange(true);
            }
          }
        } else this.emitChange(false);
      });

    this.form?.toInterval.valueChanges
      .pipe(pairwise(), takeUntilDestroyed())
      .subscribe(([prev, next]: [number, number]) => {
        if (next != 0) {
          if (next && prev != next) {
            if (!isNaN(next)) {
              this.form.toInterval.setValue(Number(next), {
                emitEvent: false,
                emitModelToViewChange: false,
                emitViewToModelChange: false,
              });
              this.emitChange(true);
            }
          }
        } else this.emitChange(false);
      });
  }

  ngOnInit(): void {
    this.initAccountYears();
  }

  private initAccountYears(): void {
    this.service
      .getCurrentAccountYear()
      .pipe(
        switchMap(currentYear => {
          return this.getAccountYears(currentYear.accountYearId);
        })
      )
      .subscribe();
  }

  private emitChange(isValidInterval: boolean): void {
    const dto = this.form?.value as DistributionSalesEuFilterDTO;
    if (
      !isValidInterval ||
      !(
        dto.accountYear &&
        dto.reportPeriod &&
        dto.fromInterval &&
        dto.toInterval
      )
    ) {
      this.dateSelectionChanged.emit(undefined);
      return;
    }

    dto.startDate = !isNaN(dto.fromInterval)
      ? this.getFromDate(Number(dto.fromInterval))
      : undefined;
    dto.stopDate = !isNaN(dto.toInterval)
      ? this.getToDate(Number(dto.toInterval))
      : undefined;

    this.dateSelectionChanged.emit(dto);
  }

  private changeReportPeriod(reportPeriod: number): void {
    this.setReportPeriod(reportPeriod);
    this.form.fromInterval.setValue(0, {
      emitEvent: false,
    });
    this.form.toInterval.setValue(0, { emitEvent: false });
    this.emitChange(false);
  }

  private setReportPeriod(reportPeriod: number): void {
    if (reportPeriod === DatePeriodType.Month) {
      this.fromPeriods.set(this.months);
      this.toPeriods.set(this.months);
    } else if (reportPeriod === DatePeriodType.Quarter) {
      this.fromPeriods.set(this.fromQuarters);
      this.toPeriods.set(this.toQuarters);
    }
  }

  private getAccountYears(
    currentAccountYearId: number
  ): Observable<ISmallGenericType[]> {
    return this.service.getAccountYears().pipe(
      tap(years => {
        addEmptyOption(years.reverse());
        this.accountYears.set(years);

        this.form?.patchValue({
          accountYear: currentAccountYearId,
        });
      })
    );
  }

  private getAccountYearIntervals(
    accountYearId: number
  ): Observable<IAccountYearDTO> {
    return this.service.getAccountYearIntervals(accountYearId);
  }

  private setIntervals(accountYearId: number): void {
    this.getAccountYearIntervals(accountYearId)
      .pipe(
        tap(p => {
          this.accountYear = p;
          this.transformPeriods(p.periods);
          this.changeReportPeriod(this.form?.reportPeriod.value);
        })
      )
      .subscribe();
  }

  private getFromDate(intervalId: number): Date | undefined {
    let date: Date | undefined;
    const startDate = this.accountYear?.periods.find(
      x => x.periodNr === intervalId
    )?.from;
    date = DateUtil.parseDateOrJson(startDate);
    return date;
  }

  private getToDate(intervalId: number): Date | undefined {
    let date: Date | undefined;

    const endDate = this.accountYear?.periods.find(
      x => x.periodNr === intervalId
    )?.to;
    date = DateUtil.parseDateOrJson(endDate);
    return date;
  }

  private transformPeriods(periods: IAccountPeriodDTO[]) {
    const locale = SoeConfigUtil.language;

    this.months = periods.map(p => {
      return {
        id: p.periodNr,
        name: `${p.from.toLocaleString(locale, { month: 'long' })} - ${p.from.getFullYear()}`,
      };
    });

    this.fromQuarters = periods
      .filter(p => (p.periodNr - 1) % 3 === 0)
      .sort((a, b) => a.periodNr - b.periodNr)
      .map((p: IAccountPeriodDTO, index: number) => {
        const quarter = index + 1;
        const year = p.from.getFullYear();
        const month = (p.from.getMonth() + 1).toString().padStart(2, '0');
        return {
          id: p.periodNr,
          name: `${quarter} - ${year}-${month}`,
        };
      });

    this.toQuarters = periods
      .filter(p => (p.periodNr - 1) % 3 === 2)
      .sort((a, b) => a.periodNr - b.periodNr)
      .map((p: IAccountPeriodDTO, index: number) => {
        const quarter = index + 1;
        const year = p.to.getFullYear();
        const month = (p.to.getMonth() + 1).toString().padStart(2, '0');
        return {
          id: p.periodNr,
          name: `${quarter} - ${year}-${month}`,
        };
      });

    addEmptyOption(this.months);
    addEmptyOption(this.fromQuarters);
    addEmptyOption(this.toQuarters);
  }

  private toggleControl(control: SoeFormControl, enable: boolean): void {
    if (enable) control.enable();
    else control.disable();
  }
}
