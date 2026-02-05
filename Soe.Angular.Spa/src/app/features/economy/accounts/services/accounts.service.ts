import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { AccountEditDTO, AccountsGridDTO } from '../models/accounts.model';
import { Observable } from 'rxjs';
import {
  deleteAccount,
  getAccount,
  getAccountsGrid,
  getChildrenAccounts,
  saveAccount,
  updateAccountsState,
  validateAccount,
} from '@shared/services/generated-service-endpoints/economy/Account.endpoints';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';
import { ISaveAccountModel } from '@shared/models/generated-interfaces/EconomyModels';
import { IAccountDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AccountsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    accountDimId: 0,
    accountYearId: 0,
    setLinkedToShiftType: false,
    getCategories: false,
    setParent: false,
    isUseCache: false,
    ignoreHierarchyOnly: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      accountDimId: number;
      accountYearId: number;
      setLinkedToShiftType: boolean;
      getCategories: boolean;
      setParent: boolean;
      isUseCache: boolean;
      ignoreHierarchyOnly: boolean;
    }
  ): Observable<AccountsGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<AccountsGridDTO[]>(
      getAccountsGrid(
        this.getGridAdditionalProps.accountDimId,
        this.getGridAdditionalProps.accountYearId,
        this.getGridAdditionalProps.setLinkedToShiftType,
        this.getGridAdditionalProps.getCategories,
        this.getGridAdditionalProps.setParent,
        this.getGridAdditionalProps.ignoreHierarchyOnly,
        id
      ),
      { useCache: this.getGridAdditionalProps.isUseCache }
    );
  }

  get(id: number): Observable<AccountEditDTO> {
    return this.http.get<AccountEditDTO>(getAccount(id));
  }

  save(model: ISaveAccountModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveAccount(), model);
  }

  updateAccountsState(model: IUpdateEntityStatesModel): Observable<any> {
    return this.http.post<IUpdateEntityStatesModel>(
      updateAccountsState(),
      model
    );
  }

  getChildrenAccounts(accountId: number): Observable<IAccountDTO[]> {
    return this.http.get<IAccountDTO[]>(getChildrenAccounts(accountId));
  }

  validateAccountNr(
    accountNr: string,
    accountId: number,
    accountDimId: number
  ): Observable<BackendResponse> {
    return this.http.get(validateAccount(accountNr, accountId, accountDimId));
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteAccount(id));
  }
}
