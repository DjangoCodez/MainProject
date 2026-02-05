import {
  getCompCurrencies,
  getEnterpriseCurrency,
  getCompCurrencyRate,
  getLedgerCurrency,
  getCompCurrenciesDictSmall,
  getCompanyCurrency,
  getCompCurrenciesDict,
} from './generated-service-endpoints/core/CoreCurrency.endpoints';
import { inject, Injectable } from '@angular/core';
import {
  ICompCurrencyDTO,
  ICompCurrencySmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { forkJoin, Observable, of, tap } from 'rxjs';
import { CacheSettingsFactory, SoeHttpClient } from './http.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { DateUtil } from '@shared/util/date-util';

@Injectable({
  providedIn: 'root',
})
export class CurrencyCoreService {
  private initdone = false;

  private _baseCurrency?: ICompCurrencyDTO;
  private _enterpriseCurrency?: ICompCurrencyDTO;

  get baseCurrency(): ICompCurrencyDTO | undefined {
    return this._baseCurrency;
  }
  get baseCurrencyCode(): string | undefined {
    return this._baseCurrency?.code;
  }

  get enterpriseCurrency(): ICompCurrencyDTO | undefined {
    return this._enterpriseCurrency;
  }

  get enterpriseCurrencyCode(): string | undefined {
    return this._enterpriseCurrency?.code;
  }

  public _currencies: ICompCurrencyDTO[] = [];
  get currencies(): ICompCurrencyDTO[] {
    return this._currencies;
  }

  constructor(private http: SoeHttpClient) {}

  public init() {
    if (!this.initdone) {
      forkJoin([
        this.loadCompCurrencies(),
        this.loadEnterpriseCurrency(),
      ]).subscribe(() => {
        this.initdone = true;
      });
    }
  }

  public findCurrency(currencyId: number): ICompCurrencyDTO | undefined {
    return this._currencies.find(c => c.currencyId === currencyId);
  }

  public isBaseCurrency(currencyId: number): boolean {
    return (
      this.baseCurrency != undefined &&
      currencyId == this.baseCurrency.currencyId
    );
  }

  public getBaseCurrencyCode(): string {
    return this.baseCurrencyCode ? this.baseCurrencyCode : '';
  }

  public getEnterpriseCurrencyRate(date: Date): Observable<number> {
    if (!this.enterpriseCurrency) {
      return of(1);
    }
    return this.getCompCurrencyRateHttp(
      this.enterpriseCurrency.sysCurrencyId,
      DateUtil.format(date, `yyyyMMdd'T'HHmmss`),
      true
    );
  }

  public getLedgerCurrency(actorId: number): Observable<ICompCurrencyDTO> {
    return this.getLedgerCurrencyHttp(actorId);
  }

  public getCompCurrencyRate(
    sysCurrencyId: number,
    date: Date
  ): Observable<number> {
    return this.getCompCurrencyRateHttp(
      sysCurrencyId,
      DateUtil.toSwedishFormattedDate(date),
      true
    );
  }

  private setBaseCurrency() {
    // If no base currency is specified, use first currency in list as base currency
    if (!this.baseCurrency && this._currencies.length > 0)
      this._baseCurrency = this._currencies[0];
  }

  private loadCompCurrencies(): Observable<ICompCurrencyDTO[]> {
    return this.getCompCurrenciesHttp(true, false).pipe(
      tap(c => {
        this._currencies = c;
        this.setBaseCurrency();
      })
    );
  }

  private loadEnterpriseCurrency(): Observable<ICompCurrencyDTO> {
    return this.getEnterpriseCurrencyHttp(false).pipe(
      tap(ec => {
        this._enterpriseCurrency = ec;
      })
    );
  }

  //#region http calls

  private getCompCurrenciesHttp(
    loadRates: boolean,
    useCache = true
  ): Observable<ICompCurrencyDTO[]> {
    return this.http.get<ICompCurrencyDTO[]>(getCompCurrencies(loadRates), {
      useCache,
    });
  }

  private getEnterpriseCurrencyHttp(
    useCache = true
  ): Observable<ICompCurrencyDTO> {
    return this.http.get<ICompCurrencyDTO>(getEnterpriseCurrency(), {
      useCache,
    });
  }

  private getCompCurrencyRateHttp(
    sysCurrencyId: number,
    date: string,
    rateToBase: boolean,
    useCache = true
  ): Observable<number> {
    return this.http.get<number>(
      getCompCurrencyRate(sysCurrencyId, date, rateToBase),
      {
        useCache,
      }
    );
  }

  private getLedgerCurrencyHttp(
    actorId: number,
    useCache = true
  ): Observable<ICompCurrencyDTO> {
    return this.http.get<ICompCurrencyDTO>(getLedgerCurrency(actorId), {
      useCache,
    });
  }

  private getBaseCurrencyHttp(useCache = true): Observable<ICompCurrencyDTO> {
    const options = useCache ? CacheSettingsFactory.long() : {};
    return this.http.get<ICompCurrencyDTO>(getCompanyCurrency(), options);
  }

  private getCompCurrenciesSmallHttp(
    useCache = true
  ): Observable<ICompCurrencySmallDTO[]> {
    return this.http.get<ICompCurrencySmallDTO[]>(
      getCompCurrenciesDictSmall(),
      {
        useCache,
      }
    );
  }

  private getCompCurrenciesDictHttp(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getCompCurrenciesDict(addEmptyRow)
    );
  }

  //#endregion
}
