import { Injectable } from '@angular/core';
import { IDecimalKeyValue } from '@shared/models/generated-interfaces/GenericType';
import { getInventoryTriggerAccounts } from '@shared/services/generated-service-endpoints/core/InventoryAccount.endpoints';
import { getAccountBalances } from '@shared/services/generated-service-endpoints/economy/AccountBalance.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AccountingRowsService {
  constructor(private http: SoeHttpClient) {}
  getAccountBalances(accountYearId: number): Observable<IDecimalKeyValue[]> {
    return this.http.post<IDecimalKeyValue[]>(
      getAccountBalances(accountYearId),
      accountYearId
    );
  }

  getInventoryTriggerAccounts(): Observable<any[]> {
    return this.http.get<any[]>(getInventoryTriggerAccounts());
  }
}
