import { Injectable } from '@angular/core';
import { AccountPeriodDTO } from '@features/economy/account-years-and-periods/models/account-years-and-periods.model';
import {
  IAccountYearDTO,
  IBudgetHeadGridDTO,
  IReportViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getAccountPeriods } from '@shared/services/generated-service-endpoints/economy/AccountPeriod.endpoints';
import { getAllAccountYears } from '@shared/services/generated-service-endpoints/economy/AccountYear.endpoints';
import { getBudgetList } from '@shared/services/generated-service-endpoints/economy/Budget.endpoints';
import { Observable, of } from 'rxjs';
import {
  DrilldownReportGridFlattenedDTO,
  SearchVoucherRowsAngDTO,
} from '../models/drill-down-reports.model';
import {
  getDrilldownReport,
  getDrilldownReports,
  getDrilldownReportVoucherRows,
} from '@shared/services/generated-service-endpoints/report/DrilldownReport.endpoints';
import { ISearchVoucherRowDTO } from '@shared/models/generated-interfaces/SearchVoucherRowDTO';

@Injectable({
  providedIn: 'root',
})
export class DrillDownReportsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<DrilldownReportGridFlattenedDTO[]> {
    return of([]);
  }

  getDrilldownReports(
    onlyOriginal: boolean,
    onlyStandard: boolean
  ): Observable<IReportViewDTO[]> {
    return this.http.get<IReportViewDTO[]>(
      getDrilldownReports(onlyOriginal, onlyStandard)
    );
  }

  getBudgetList(
    budgetType: number,
    actorId: number = 0
  ): Observable<IBudgetHeadGridDTO[]> {
    return this.http.get<IBudgetHeadGridDTO[]>(
      getBudgetList(budgetType, actorId)
    );
  }

  getAccountYears(addEmptyRow: boolean, excludeNew: boolean) {
    return this.http.get<IAccountYearDTO[]>(
      getAllAccountYears(addEmptyRow, excludeNew)
    );
  }

  getAccountPeriods(accountYearId: number) {
    return this.http.get<AccountPeriodDTO[]>(getAccountPeriods(accountYearId));
  }

  getDrilldownReport(
    reportId: number,
    accountPeriodIdFrom: number,
    accountPeriodIdTo: number,
    budgetHeadId: number
  ) {
    return this.http.get<DrilldownReportGridFlattenedDTO[]>(
      getDrilldownReport(
        reportId,
        accountPeriodIdFrom,
        accountPeriodIdTo,
        budgetHeadId
      )
    );
  }

  getDrilldownReportVoucherRows(
    model: SearchVoucherRowsAngDTO
  ): Observable<ISearchVoucherRowDTO[]> {
    return this.http.post(getDrilldownReportVoucherRows(), model);
  }
}
