import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import {
  CacheSettingsFactory,
  SoeHttpClient,
} from '@shared/services/http.service';
import {
  deleteCurrency,
  getCurrenciesGrid,
  getCurrency,
  getSysCurrencies,
  saveCurrency,
  getSysCurrenciesDict,
} from '@shared/services/generated-service-endpoints/core/CoreCurrency.endpoints';
import { ICurrencyDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CurrencyDTO, CurrencyGridDTO } from '../models/currencies.model';
import { ISysCurrencyDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CurrenciesService {
  private _existingSysCurrencyCodes = new Set<string>();
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number) {
    return this.http.get<CurrencyGridDTO[]>(getCurrenciesGrid(id || 0)).pipe(
      map(data => {
        data.forEach(item => {
          item.description = `${item.name} (${item.code})`;
          this._existingSysCurrencyCodes.add(item.code);
        });

        return data;
      })
    );
  }

  get(id: number): Observable<ICurrencyDTO> {
    return this.http.get<ICurrencyDTO>(getCurrency(id));
  }

  save(model: CurrencyDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveCurrency(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteCurrency(id));
  }

  getSysCurrencies(applyFilter: boolean) {
    const cacheOptions = CacheSettingsFactory.long();
    return this.http
      .get<ISysCurrencyDTO[]>(getSysCurrencies(), cacheOptions)
      .pipe(
        map(data => {
          const filtered = applyFilter
            ? data.filter(c => !this._existingSysCurrencyCodes.has(c.code))
            : data;

          return filtered.sort((a, b) => a.name.localeCompare(b.name));
        })
      );
  }

  getSysCurrenciesDict(
    addEmptyRow: boolean,
    useCode: boolean
  ): Observable<SmallGenericType[]> {
    const url = `${getSysCurrenciesDict()}?addEmptyRow=${addEmptyRow}&useCode=${useCode}`;
    const cacheOptions = CacheSettingsFactory.long();
    return this.http.get<SmallGenericType[]>(url, cacheOptions);
  }
}
