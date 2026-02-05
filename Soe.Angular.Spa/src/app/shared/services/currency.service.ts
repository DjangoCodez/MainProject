import {
  computed,
  DestroyRef,
  effect,
  inject,
  Injectable,
  signal,
} from '@angular/core';
import { CurrencyCoreService } from './currency-core.service';
import { ICompCurrencyDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TermGroup_CurrencyType } from '@shared/models/generated-interfaces/Enumerations';
import {
  BehaviorSubject,
  catchError,
  EMPTY,
  distinctUntilChanged,
  of,
  tap,
} from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DateUtil } from '@shared/util/date-util';

type DateInput = Date | string | undefined;

@Injectable()
export class CurrencyService {
  private readonly currencyCore = inject(CurrencyCoreService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly currencyId = signal<number>(0);
  private readonly currencyDate = signal<Date | undefined>(undefined);
  private readonly currencyIdSubject = new BehaviorSubject<number>(0);

  private _transactionCurrencyRate = 1;
  private _ledgerCurrencyRate = 1;
  private _enterpriseCurrencyRate = 1;
  private _currencyRateDate: Date | undefined = undefined;

  private ledgerCurrency?: ICompCurrencyDTO;
  private transactionCurrency?: ICompCurrencyDTO;

  public readonly isBaseCurrency = computed(() =>
    this.currencyCore.isBaseCurrency(this.currencyId())
  );

  public get transactionCurrencyRate(): number {
    return this._transactionCurrencyRate;
  }

  public get currencyRateDate(): Date | undefined {
    return this._currencyRateDate;
  }

  public get baseCurrency(): ICompCurrencyDTO | undefined {
    return this.currencyCore.baseCurrency;
  }

  public get baseCurrencyId(): number | undefined {
    return this.currencyCore.baseCurrency?.currencyId;
  }

  public get baseCurrencyCode(): string {
    return this.currencyCore?.getBaseCurrencyCode() ?? '';
  }

  public get transactionCurrencyCode(): string | undefined {
    return this.transactionCurrency?.code;
  }

  public get hasLedgerCurrency(): boolean {
    return !!this.ledgerCurrency;
  }

  public get currencyRateDateString(): string {
    return this.currencyRateDate
      ? DateUtil.localeDateFormat(this.currencyRateDate)
      : '';
  }

  public get currencies(): ICompCurrencyDTO[] {
    return this.currencyCore.currencies || [];
  }

  get currencyIdChanged$() {
    return this.currencyIdSubject
      .asObservable()
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this.destroyRef));
  }

  private set transactionCurrencyRate(rate: number) {
    this._transactionCurrencyRate = rate;
  }

  private get ledgerCurrencyRate(): number {
    return this._ledgerCurrencyRate;
  }

  private set ledgerCurrencyRate(rate: number) {
    this._ledgerCurrencyRate = rate;
  }

  private get enterpriseCurrencyRate(): number {
    return this._enterpriseCurrencyRate;
  }

  private set enterpriseCurrencyRate(rate: number) {
    this._enterpriseCurrencyRate = rate;
  }

  private set currencyRateDate(date: Date | undefined) {
    this._currencyRateDate = date;
  }

  constructor() {
    this.currencyCore.init();
    this.setupCurrencyUpdateEffect();
  }

  public getCurrencyAmount(
    amount: number,
    sourceCurrencyType: TermGroup_CurrencyType,
    targetCurrencyType: TermGroup_CurrencyType
  ): number {
    return this.getCurrencyAmountNonAsync(
      amount,
      sourceCurrencyType,
      targetCurrencyType
    );
  }

  public getCurrencyAmountNonAsync(
    amount: number,
    sourceCurrencyType: TermGroup_CurrencyType,
    targetCurrencyType: TermGroup_CurrencyType
  ): number {
    if (sourceCurrencyType === targetCurrencyType) {
      return amount;
    }

    const sourceRate = this.getCurrencyRate(sourceCurrencyType);
    const baseAmount = amount * sourceRate;

    const targetRate = this.getCurrencyRate(targetCurrencyType);
    const convertedAmount = baseAmount / targetRate;

    return Number(convertedAmount.toFixed(5));
  }

  public getCurrencyId(): number {
    return this.currencyId();
  }

  public setCurrencyId(value: number): void {
    this.setCurrencyIdValue(value);
    this.setCurrencyChanges();
  }

  public setCurrencyDate(value: DateInput): void {
    this.setCurrencyDateValue(value);
    this.setCurrencyChanges();
  }

  public setCurrencyIdAndDate(value: number, date: DateInput): void {
    this.setCurrencyIdValue(value);
    this.setCurrencyDateValue(date);
    this.setCurrencyChanges();
  }

  public loadLedgerCurrency(actorId: number): void {
    if (!actorId) {
      this.ledgerCurrency = this.currencyCore.baseCurrency;
      return;
    }

    this.currencyCore
      .getLedgerCurrency(actorId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        catchError(error => {
          console.error(
            'CurrencyService.loadLedgerCurrency: Error loading ledger currency',
            error
          );
          return EMPTY;
        })
      )
      .subscribe(currency => {
        this.ledgerCurrency = currency;
        if (this.currencyRateDate) {
          this.loadLedgerCurrencyRate();
        }
      });
  }

  public toForm(form: any): void {
    if (!form) {
      return;
    }

    try {
      form.patchValue(
        {
          currencyId: this.getCurrencyId(),
        },
        { emitEvent: false }
      );

      if (form.currencyRate) {
        form.currencyRate.setValue(this.transactionCurrencyRate ?? 1);
      }

      if (form.currencyDate) {
        form.currencyDate.setValue(
          this.currencyRateDate ?? DateUtil.defaultDateTime()
        );
      }
    } catch (error) {
      console.error('CurrencyService.toForm: Error updating form', error);
    }
  }

  private setupCurrencyUpdateEffect() {
    effect(() => {
      const currencyId = this.currencyId();

      if (this.currencyIdSubject.getValue() !== currencyId) {
        this.currencyIdSubject.next(currencyId);
      }
    });
  }

  private setCurrencyIdValue(value: number): void {
    this.currencyId.set(value);
  }

  private setCurrencyDateValue(value: DateInput): void {
    if (value instanceof Date) {
      this.currencyDate.set(value);
    } else {
      this.currencyDate.set(value ? new Date(value) : undefined);
    }
  }

  private getCurrencyRate(currencyType: TermGroup_CurrencyType): number {
    switch (currencyType) {
      case TermGroup_CurrencyType.TransactionCurrency:
        return this.transactionCurrencyRate;
      case TermGroup_CurrencyType.EnterpriseCurrency:
        return this.enterpriseCurrencyRate;
      case TermGroup_CurrencyType.LedgerCurrency:
        return this.ledgerCurrencyRate;
      default:
        return 1;
    }
  }

  private getCurrencyDate(): Date | undefined {
    return this.currencyDate();
  }

  private setCurrencyRateDate(value: Date | undefined): void {
    this.currencyRateDate = value ? new Date(value) : undefined;
    this.afterCurrencyRateDateSet();
  }

  private afterCurrencyRateDateSet(): void {
    this.setEnterpriseCurrencyRate();
    this.loadLedgerCurrencyRate();
  }

  private setTransactionCurrency(currencyId: number): void {
    const currency = this.currencyCore.findCurrency(currencyId);

    if (!currency) {
      this.resetTransactionCurrency();
      return;
    }

    this.transactionCurrency = currency;

    if (this.hasValidCurrencyRates) {
      this.setRateFromHistory();
    } else {
      this.setCurrentRate();
    }
  }

  private get hasValidCurrencyRates(): boolean {
    return !!(
      this.transactionCurrency?.compCurrencyRates &&
      this.transactionCurrency.compCurrencyRates.length > 0
    );
  }

  private setRateFromHistory(): void {
    const currencyDate = this.getCurrencyDate();
    if (!this.transactionCurrency?.compCurrencyRates || !currencyDate) {
      this.setCurrentRate();
      return;
    }

    const validRates = this.transactionCurrency.compCurrencyRates
      .filter(
        rate =>
          rate.date &&
          (rate.date.isSameDay(currencyDate) ||
            rate.date.isBefore(currencyDate))
      )
      .sort((a, b) => {
        if (!a.date || !b.date) return 0;
        return b.date.getTime() - a.date.getTime();
      });

    const currentRate = validRates[0];
    this.transactionCurrencyRate =
      currentRate?.rateToBase ?? this.transactionCurrency.rateToBase;
    this.setCurrencyRateDate(currentRate?.date);
  }

  private setCurrentRate(): void {
    if (this.transactionCurrency) {
      this.transactionCurrencyRate = this.transactionCurrency.rateToBase;
      this.setCurrencyRateDate(this.transactionCurrency.date);
    }
  }

  private resetTransactionCurrency(): void {
    this.transactionCurrency = undefined;
    this.transactionCurrencyRate = 1;
  }

  private setEnterpriseCurrencyRate(): void {
    const currencyDate = this.getCurrencyDate();
    if (!this.currencyCore.enterpriseCurrency || !currencyDate) {
      return;
    }

    this.currencyCore
      .getEnterpriseCurrencyRate(currencyDate)
      .pipe(
        catchError(() => of(1)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(rate => {
        this.enterpriseCurrencyRate = rate === 0 ? 1 : rate;
      });
  }

  private loadLedgerCurrencyRate(): void {
    const ledgerCurrency = this.ledgerCurrency;
    const rateDate = this.currencyRateDate;

    if (ledgerCurrency && rateDate) {
      this.currencyCore
        .getCompCurrencyRate(ledgerCurrency.sysCurrencyId, rateDate)
        .pipe(
          tap(rate => {
            this.ledgerCurrencyRate = rate === 0 ? 1 : rate;
          }),
          catchError(() => EMPTY),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe();
    }
  }

  private setCurrencyChanges(): void {
    const currencyId = this.getCurrencyId();
    if (currencyId === 0) {
      this.resetTransactionCurrency();
      return;
    }

    const finalCurrencyId = currencyId || this.baseCurrencyId || 0;

    if (finalCurrencyId !== 0) {
      this.setTransactionCurrency(finalCurrencyId);
    }
  }
}
