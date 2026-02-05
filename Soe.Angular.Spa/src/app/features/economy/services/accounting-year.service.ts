import { computed, inject, Injectable, signal } from '@angular/core';
import { IAccountYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getCurrentAccountYear,
  getSelectedAccountYear,
} from '@shared/services/generated-service-endpoints/economy/AccountYear.endpoints';
import { Observable, shareReplay, switchMap, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PersistedAccountingYearService {
  http = inject(SoeHttpClient);
  private accountYearLoaded$: Observable<IAccountYearDTO> | null = null;

  selectedAccountYear = signal<IAccountYearDTO | null>(null);
  selectedAccountYearId = computed(
    () => this.selectedAccountYear()?.accountYearId ?? 0
  );

  getCurrentAccountYear() {
    return this.http.get<IAccountYearDTO>(getCurrentAccountYear());
  }

  getSelectedAccountYear() {
    return this.http.get<IAccountYearDTO>(getSelectedAccountYear());
  }

  public loadSelectedAccountYear(): Observable<IAccountYearDTO> {
    if (!this.accountYearLoaded$) {
      this.accountYearLoaded$ = this.getSelectedAccountYear().pipe(
        tap(ay => {
          this.selectedAccountYear.set(ay);
        }),
        shareReplay(1)
      );
    }
    return this.accountYearLoaded$;
  }

  public ensureAccountYearIsLoaded$<T>(
    obs: () => Observable<T>
  ): Observable<T> {
    return this.loadSelectedAccountYear().pipe(
      switchMap(() => {
        return obs();
      })
    );
  }
}
