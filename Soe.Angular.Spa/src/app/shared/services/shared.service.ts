import { Injectable } from '@angular/core';
import { SoeHttpClient } from './http.service';
import { Observable } from 'rxjs';
import { getAccountsFromHierarchyByUserSetting } from './generated-service-endpoints/economy/Account.endpoints';
import { DateUtil } from '@shared/util/date-util';
import { AccountDTO } from '@shared/models/account.model'
import { SmallGenericType } from '@shared/models/generic-type.model';
import { getCompCurrenciesGenericType } from './generated-service-endpoints/shared/Currency.endpoints';
import {
  getAccountIdsFromHierarchyByUser,
  getAccountsFromHierarchyByUser,
} from './generated-service-endpoints/manage/UserV2.endpoints';

@Injectable({
  providedIn: 'root',
})
export class SharedService {
  constructor(private http: SoeHttpClient) {}

  getAccountsFromHierarchyByUserSetting(
    dateFrom: Date,
    dateTo: Date,
    useMaxAccountDimId = false,
    includeVirtualParented = false,
    includeOnlyChildrenOneLevel = false,
    useDefaultEmployeeAccountDimEmployee = false
  ): Observable<AccountDTO[]> {
    const dateFromString = dateFrom
      ? DateUtil.toDateTimeString(dateFrom)
      : 'null';
    const dateToString = dateTo ? DateUtil.toDateTimeString(dateTo) : 'null';

    return this.http.get<AccountDTO[]>(
      getAccountsFromHierarchyByUserSetting(
        dateFromString,
        dateToString,
        useMaxAccountDimId,
        includeVirtualParented,
        includeOnlyChildrenOneLevel,
        useDefaultEmployeeAccountDimEmployee
      )
    );
  }

  getAccountIdsFromHierarchyByUser(
    dateFrom: Date,
    dateTo: Date,
    useMaxAccountDimId = false,
    includeVirtualParented = false,
    includeOnlyChildrenOneLevel = false,
    onlyDefaultAccounts = false,
    useDefaultEmployeeAccountDimEmployee = false,
    includeAbstract = false
  ): Observable<number[]> {
    const dateFromString = dateFrom
      ? DateUtil.toDateTimeString(dateFrom)
      : 'null';
    const dateToString = dateTo ? DateUtil.toDateTimeString(dateTo) : 'null';

    return this.http.get<number[]>(
      getAccountIdsFromHierarchyByUser(
        dateFromString,
        dateToString,
        useMaxAccountDimId,
        includeVirtualParented,
        includeOnlyChildrenOneLevel,
        onlyDefaultAccounts,
        useDefaultEmployeeAccountDimEmployee,
        includeAbstract
      )
    );
  }

  getAccountsFromHierarchyByUser(
    dateFrom: Date,
    dateTo: Date,
    useMaxAccountDimId = false,
    includeVirtualParented = false,
    includeOnlyChildrenOneLevel = false,
    onlyDefaultAccounts = false,
    useDefaultEmployeeAccountDimEmployee = false
  ): Observable<AccountDTO[]> {
    const dateFromString = dateFrom
      ? DateUtil.toDateTimeString(dateFrom)
      : 'null';
    const dateToString = dateTo ? DateUtil.toDateTimeString(dateTo) : 'null';

    return this.http.get<AccountDTO[]>(
      getAccountsFromHierarchyByUser(
        dateFromString,
        dateToString,
        useMaxAccountDimId,
        includeVirtualParented,
        includeOnlyChildrenOneLevel,
        onlyDefaultAccounts,
        useDefaultEmployeeAccountDimEmployee
      )
    );
  }

  getCurrencies(useCache: boolean = false) {
    return this.http.get<SmallGenericType[]>(
      getCompCurrenciesGenericType(false, false),
      { useCache }
    );
  }
}
