import { Injectable } from '@angular/core';
import { ISaveAccountDimModel } from '@shared/models/generated-interfaces/EconomyModels';
import { IAccountDimGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  accountDimByAccountDimId,
  deleteAccountDim,
  getAccountDimChars,
  getAccountDimGrid,
  saveAccountDim,
} from '@shared/services/generated-service-endpoints/economy/Account.endpoints';
import { map, Observable } from 'rxjs';
import { AccountDimDTO } from '../models/accounting-coding-levels.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { validateAccountDim } from '@shared/services/generated-service-endpoints/economy/AccountDim.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AccountingCodingLevelsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IAccountDimGridDTO[]> {
    return this.http.get<IAccountDimGridDTO[]>(
      getAccountDimGrid(false, true, id)
    );
  }

  get(id: number): Observable<AccountDimDTO> {
    return this.http
      .get<AccountDimDTO>(accountDimByAccountDimId(id, true))
      .pipe(
        map(d => {
          /*
        reassign the response due to state field not resetting in 
        AccountDimForm customPatch reset
      */

          const accountDim = new AccountDimDTO();
          Object.assign(accountDim, d);
          return accountDim;
        })
      );
  }

  getAccountDimChars(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getAccountDimChars(), {
      useCache: false,
    });
  }

  validateAccountNr(
    accountDimNr: number,
    accountDimId: number
  ): Observable<BackendResponse> {
    return this.http.get(validateAccountDim(accountDimNr, accountDimId));
  }

  save(
    model: AccountDimDTO,
    additionalData?: any
  ): Observable<BackendResponse> {
    return this.http.post(saveAccountDim(), <ISaveAccountDimModel>{
      accountDim: model,
      reset: additionalData.reset,
    });
  }

  delete(id: number): Observable<BackendResponse> {
    return this.deleteMany([id]);
  }

  deleteMany(ids: number[]): Observable<BackendResponse> {
    return this.http.delete(deleteAccountDim(ids));
  }
}
