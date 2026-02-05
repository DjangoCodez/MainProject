import { Injectable } from '@angular/core';
import { SoeHttpClient } from '../../../shared/services/http.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  getAccountDimInternals,
  getAccountsInternalsByCompany,
  getAccountStdsNameNumber,
  getAccountYears,
  getStdAccounts,
  getSysAccountSruCodes,
  getSysVatAccounts,
  saveAccountSmall,
} from '@shared/services/generated-service-endpoints/economy/Account.endpoints';
import {
  getVoucherSeriesByYear,
  getVoucherSeriesDictByYear,
} from '@shared/services/generated-service-endpoints/economy/VoucherSeries.endpoints';
import { VoucherSeriesDTO } from '../account-years-and-periods/models/account-years-and-periods.model';
import { AccountStdNumberNameDTO } from '../models/account-std.model';
import { AccountDTO } from '@shared/models/account.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { AccountDimDTO } from '../accounting-coding-levels/models/accounting-coding-levels.model';
import {
  copySysAccountStd,
  getSysAccountStd,
} from '@shared/services/generated-service-endpoints/economy/SysAccountStd.endpoints';
import { ISysAccountStdDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { Observable } from 'rxjs';
import { ISaveAccountSmallModel } from '@shared/models/generated-interfaces/EconomyModels';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AccountingService {
  constructor(private http: SoeHttpClient) {}

  getAccountYearDict(addEmptyRow: boolean, excludeNew: boolean = false) {
    return this.http.get<ISmallGenericType[]>(
      getAccountYears(addEmptyRow, excludeNew)
    );
  }

  getVoucherSeriesByYear(accountYearId: number, includeTemplate: boolean) {
    return this.http.get<VoucherSeriesDTO[]>(
      getVoucherSeriesByYear(accountYearId, includeTemplate)
    );
  }

  getVoucherSeriesDictByYear(
    accountYearId: number,
    addEmptyRow: boolean = true,
    includeTemplate: boolean = false
  ) {
    return this.http.get<ISmallGenericType[]>(
      getVoucherSeriesDictByYear(accountYearId, addEmptyRow, includeTemplate)
    );
  }

  getAccountStdsNumberName(addEmptyRow: boolean) {
    return this.http.get<AccountStdNumberNameDTO[]>(
      getAccountStdsNameNumber(addEmptyRow)
    );
  }

  getAccountsInternalsByCompany(
    loadAccount: boolean,
    loadAccountDim: boolean,
    loadAccountMapping: boolean
  ) {
    return this.http.get<AccountDTO[]>(
      getAccountsInternalsByCompany(
        loadAccount,
        loadAccountDim,
        loadAccountMapping
      )
    );
  }

  getAccountDimInternals(active?: boolean) {
    return this.http.get<AccountDimDTO[]>(getAccountDimInternals(active));
  }

  getStdAccounts() {
    return this.http.get<AccountDTO[]>(getStdAccounts());
  }

  getSysVatAccounts(sysCountryId: number, addEmptyRow: boolean) {
    return this.http.get<SmallGenericType[]>(
      getSysVatAccounts(sysCountryId, addEmptyRow)
    );
  }

  getSysAccountSruCodes(addEmptyRow: boolean) {
    return this.http.get<SmallGenericType[]>(
      getSysAccountSruCodes(addEmptyRow)
    );
  }

  getSysAccountStd(sysAccountStdTypeId: number, accountNr: string) {
    return this.http.get<ISysAccountStdDTO>(
      getSysAccountStd(sysAccountStdTypeId, accountNr)
    );
  }

  copySysAccountStd(sysAccountStdId: number) {
    return this.http.get<AccountDTO>(copySysAccountStd(sysAccountStdId));
  }

  saveAccountSmall(model: ISaveAccountSmallModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveAccountSmall(), model);
  }
}
