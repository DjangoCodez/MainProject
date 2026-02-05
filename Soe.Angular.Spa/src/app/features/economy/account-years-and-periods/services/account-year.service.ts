import { Injectable, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { CoreService } from '@shared/services/core.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  copyGrossProfitCodes,
  copyVoucherTemplatesFromPreviousAccountYear,
  deleteAccountYear,
  getAccountYearId,
  getAllAccountYears,
  saveAccountYear,
} from '@shared/services/generated-service-endpoints/economy/AccountYear.endpoints';
import { forkJoin, map, Observable } from 'rxjs';
import {
  AccountYearDTO,
  SaveAccountYearModel,
} from '../models/account-years-and-periods.model';
import { orderBy } from 'lodash';
import {
  TermGroup,
  TermGroup_AccountStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { AccountYearsValidator } from '../models/account-year-form.validators';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AccountYearService {
  validationHandler = inject(ValidationHandler);
  validator = inject(AccountYearsValidator);
  readonly coreService = inject(CoreService);
  readonly http = inject(SoeHttpClient);

  latestAccountingYear: AccountYearDTO | null = null;

  getGridAdditionalProps = {
    getPeriods: false,
    excludeNew: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      getPeriods: boolean;
      excludeNew: boolean;
    }
  ): Observable<AccountYearDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return forkJoin({
      accountYears: this.http.get<AccountYearDTO[]>(
        getAllAccountYears(
          this.getGridAdditionalProps.getPeriods,
          this.getGridAdditionalProps.excludeNew
        )
      ),
      accountStatuses: this.coreService.getTermGroupContent(
        TermGroup.AccountYearStatus,
        true,
        false
      ),
    }).pipe(
      map(({ accountYears, accountStatuses }): AccountYearDTO[] => {
        accountYears.forEach(ay => {
          ay.statusIcon = this.getStatusIcon(ay.status);
          ay.statusText = <string>(
            accountStatuses.find(s => s.id == ay.status)?.name
          );
        });
        this.latestAccountingYear = this.getLatestYear(accountYears);
        this.validator.setSortedAccountYears(accountYears);
        return orderBy(accountYears, 'from', ['desc']);
      })
    );
  }

  getStatusIcon(status: number): string {
    switch (status) {
      case TermGroup_AccountStatus.New:
        return '#1e1e1e';
      case TermGroup_AccountStatus.Open:
        return '#24a148';
      case TermGroup_AccountStatus.Closed:
        return '#ff832b';
      case TermGroup_AccountStatus.Locked:
        return '#da1e28';
      default:
        return '';
    }
  }

  private getLatestYear(rows: AccountYearDTO[]) {
    return rows.reduce(
      (latestYear, currentYear) => {
        if (!latestYear?.to) return currentYear;
        return currentYear.to > latestYear.to ? currentYear : latestYear;
      },
      null as AccountYearDTO | null
    );
  }

  get(id: number): Observable<AccountYearDTO> {
    return this.http.get<AccountYearDTO>(getAccountYearId(id, true)).pipe(
      map(data => {
        const obj = new AccountYearDTO();
        Object.assign(obj, data);
        return obj;
      })
    );
  }

  copyVoucherTemplate(accountYearId: number): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      copyVoucherTemplatesFromPreviousAccountYear(accountYearId),
      accountYearId
    );
  }

  copyGrossProfitCodes(accountYearId: number): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      copyGrossProfitCodes(accountYearId),
      accountYearId
    );
  }

  save(model: SaveAccountYearModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveAccountYear(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteAccountYear(id));
  }
}
