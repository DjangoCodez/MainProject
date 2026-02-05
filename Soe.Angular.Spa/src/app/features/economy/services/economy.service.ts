import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  getAccountStdsDict,
  getAccountStdsNameNumber,
  getAccountYears,
  getAccountDimsSmall,
  getAccountsSmall,
  accountDim,
  getSysAccountStdTypes,
  getSysVatAccounts,
  accountDimByAccountDimId,
  getAccountsDict,
  getAccountMappings,
  getSysAccountSruCodes,
  getProjectAccountDim,
  getShiftTypeAccountDim,
  getSysVatRate,
} from '@shared/services/generated-service-endpoints/economy/Account.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { AccountStdNumberNameDTO } from '../models/account-std.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountBalanceDTO,
  IAccountDimDTO,
  IAccountDimSmallDTO,
  IAccountMappingDTO,
  IAccountSmallDTO,
  IUserWithNameAndLoginDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  calculateAccountBalanceForAccountInAccountYears,
  calculateForAccountsAllYears,
  getAccountBalanceByAccount,
} from '@shared/services/generated-service-endpoints/economy/AccountBalance.endpoints';
import { getUserNamesWithLogin } from '@shared/services/generated-service-endpoints/manage/UserV2.endpoints';
import { getAccountDimStd } from '@shared/services/generated-service-endpoints/economy/AccountDim.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class EconomyService {
  constructor(private http: SoeHttpClient) {}

  getAccountStdsDict(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getAccountStdsDict(addEmptyRow));
  }

  getAccountSysVatRate(accountId: number): Observable<number> {
    return this.http.get<number>(getSysVatRate(accountId));
  }

  getAccountStdsNameNumber(addEmptyRow: boolean, accountType?: number) {
    return this.http.get<AccountStdNumberNameDTO[]>(
      getAccountStdsNameNumber(addEmptyRow, accountType)
    );
  }

  getAccountYears() {
    return this.http.get<ISmallGenericType[]>(getAccountYears(false, false));
  }

  getAccountDimStd() {
    return this.http.get<IAccountDimDTO>(getAccountDimStd());
  }

  getAccountDimsSmall(
    onlyStandard: boolean,
    onlyInternal: boolean,
    loadAccounts: boolean,
    loadInternalAccounts: boolean,
    loadParent: boolean,
    loadInactives: boolean,
    loadInactiveDims: boolean,
    includeParentAccounts: boolean,
    useCache: boolean = true,
    ignoreHierarchyOnly: boolean = false,
    actorCompanyId: number = 0,
    includeOrphanAccounts: boolean = false
  ): Observable<IAccountDimSmallDTO[]> {
    return this.http.get<IAccountDimSmallDTO[]>(
      getAccountDimsSmall(
        onlyStandard,
        onlyInternal,
        loadAccounts,
        loadInternalAccounts,
        loadParent,
        loadInactives,
        loadInactiveDims,
        includeParentAccounts,
        ignoreHierarchyOnly,
        actorCompanyId,
        includeOrphanAccounts
      ),
      { useCache }
    );
  }

  getAccountsSmall(accountDimId: number, accountYearId: number) {
    return this.http.get<IAccountSmallDTO[]>(
      getAccountsSmall(accountDimId, accountYearId)
    );
  }

  getAccountDims(
    onlyStandard: boolean,
    onlyInternal: boolean,
    loadAccounts: boolean,
    loadInternalAccounts: boolean,
    loadParent: boolean,
    loadInactives: boolean,
    loadInactiveDims: boolean,
    includeParentAccounts: boolean
  ): Observable<IAccountDimDTO[]> {
    return this.http.get<IAccountDimDTO[]>(
      accountDim(
        onlyStandard,
        onlyInternal,
        loadAccounts,
        loadInternalAccounts,
        loadParent,
        loadInactives,
        loadInactiveDims,
        includeParentAccounts
      )
    );
  }

  getAccountDimByAccountDimId(
    accountDimId: number,
    loadInactiveDims: boolean
  ): Observable<IAccountDimDTO> {
    return this.http.get<IAccountDimDTO>(
      accountDimByAccountDimId(accountDimId, loadInactiveDims)
    );
  }

  getAccountsDict(
    accountDimId: number,
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getAccountsDict(accountDimId, addEmptyRow)
    );
  }

  getProjectAccountDim(): Observable<IAccountDimDTO> {
    return this.http.get<IAccountDimDTO>(getProjectAccountDim());
  }

  getShiftTypeAccountDim(
    loadAccounts: boolean,
    useCache: boolean = true
  ): Observable<IAccountDimDTO> {
    return this.http.get<IAccountDimDTO>(
      getShiftTypeAccountDim(loadAccounts, useCache)
    );
  }

  getAccountMappings(accountId: number): Observable<IAccountMappingDTO[]> {
    return this.http.get<IAccountMappingDTO[]>(getAccountMappings(accountId));
  }

  getSysAccountStdTypes(): Observable<ISmallGenericType[]> {
    return this.http.get(getSysAccountStdTypes());
  }

  getSysVatAccounts(
    sysCountryId: number,
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getSysVatAccounts(sysCountryId, addEmptyRow)
    );
  }

  getSysAccountSruCodes(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getSysAccountSruCodes(addEmptyRow)
    );
  }

  getAccountBalanceByAccount(
    accountId: number,
    loadYear: boolean
  ): Observable<IAccountBalanceDTO[]> {
    return this.http.get<IAccountBalanceDTO[]>(
      getAccountBalanceByAccount(accountId, loadYear)
    );
  }

  calculateAccountBalanceForAccountsAllYears(): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      calculateForAccountsAllYears(),
      null
    );
  }

  calculateAccountBalanceForAccountInAccountYears(accountId: number) {
    return this.http.post<BackendResponse>(
      calculateAccountBalanceForAccountInAccountYears(accountId),
      null
    );
  }

  importSysAccountStdType(
    sysAccountStdTypeId: number
  ): Observable<BackendResponse> {
    const url = getSysAccountStdTypes() + `IMPORT/${sysAccountStdTypeId}`;
    return this.http.post<BackendResponse>(url, null);
  }

  getUserNamesWithLogin(): Observable<IUserWithNameAndLoginDTO[]> {
    return this.http.get<IUserWithNameAndLoginDTO[]>(getUserNamesWithLogin());
  }
}
