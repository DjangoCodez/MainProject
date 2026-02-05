import { Injectable, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import { AccountYearBalanceFlatDTO } from '../models/account-years-and-periods.model';
import {
  getAccountYearBalance,
  getAccountYearBalanceFromPreviousYear,
  saveAccountYearBalances,
} from '@shared/services/generated-service-endpoints/economy/Balance.endpoints';
import { ISaveAccountYearBalanceModel } from '@shared/models/generated-interfaces/EconomyModels';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AccountingBalanceService {
  validationHandler = inject(ValidationHandler);

  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<AccountYearBalanceFlatDTO[]> {
    return this.http.get<AccountYearBalanceFlatDTO[]>(
      getAccountYearBalance(id!)
    );
  }

  getBalanceFromPreviousYear(id: number): Observable<any> {
    return this.http.get<any>(getAccountYearBalanceFromPreviousYear(id));
  }

  save(model: ISaveAccountYearBalanceModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveAccountYearBalances(), model);
  }
}
