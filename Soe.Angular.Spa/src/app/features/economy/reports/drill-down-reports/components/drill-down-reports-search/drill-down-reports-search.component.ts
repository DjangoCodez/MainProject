import { Component, inject, output } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DrillDownReportsService } from '../../services/drill-down-reports.service';
import { Perform } from '@shared/util/perform.class';
import { ProgressService } from '@shared/services/progress/progress.service';
import { IGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BudgetService } from '@features/economy/budget/services/budget.service';
import { DistributionCodeBudgetType } from '@shared/models/generated-interfaces/Enumerations';
import { IAccountYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AccountPeriodDTO } from '@features/economy/account-years-and-periods/models/account-years-and-periods.model';
import { DrillDownReportForm } from '../../models/drill-down-reports-form.model';
import {
  DrillDownReportDTO,
  DrilldownReportGridFlattenedDTO,
  IDrillDownReportDTO,
  ReportViewDTO,
} from '../../models/drill-down-reports.model';
import { tap } from 'rxjs';
import { ValidationHandler } from '@shared/handlers';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';
import { orderBy } from 'lodash';

@Component({
  selector: 'soe-drill-down-reports-search',
  templateUrl: './drill-down-reports-search.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class DrillDownReportSearchComponent {
  service = inject(DrillDownReportsService);
  validationHandler = inject(ValidationHandler);
  budgetService = inject(BudgetService);
  progressService = inject(ProgressService);
  ayService = inject(PersistedAccountingYearService);
  flowHandlerService = inject(FlowHandlerService);

  performLoad = new Perform<any>(this.progressService);

  reports: ReportViewDTO[] = [];
  budgets: IGenericType[] = [];
  accountYears: IAccountYearDTO[] = [];
  accountPeriodsFrom: AccountPeriodDTO[] = [];
  accountPeriodsTo: AccountPeriodDTO[] = [];

  valueChange = output<IDrillDownReportDTO>();
  createReport = output<DrilldownReportGridFlattenedDTO[]>();
  form: DrillDownReportForm = new DrillDownReportForm({
    validationHandler: this.validationHandler,
    element: new DrillDownReportDTO(),
  });

  constructor() {
    this.flowHandlerService.execute({
      lookups: [
        this.getDrilldownReports(),
        this.getBudgetList(),
        this.loadSelectedAccountYear(),
      ],
    });
  }

  //#region Help Methods

  onSearchValueChange(value: IDrillDownReportDTO) {
    this.valueChange.emit(value);
  }

  onAccountPeriodChanged(accountPeriodId: number, isFrom: boolean) {
    if (isFrom) {
      const accountPeriodFrom = this.accountPeriodsFrom.find(
        x => x.accountPeriodId === accountPeriodId
      );
      if (accountPeriodFrom) {
        this.form.patchValue({ accountPeriodFrom: accountPeriodFrom.from });
      }
    } else {
      const accountPeriodTo = this.accountPeriodsTo.find(
        x => x.accountPeriodId === accountPeriodId
      );

      if (accountPeriodTo) {
        this.form.patchValue({ accountPeriodTo: accountPeriodTo.from });
      }
    }
    this.onSearchValueChange(this.form.value as IDrillDownReportDTO);
  }

  //#endregion

  //#region Data Loding Functions

  loadSelectedAccountYear() {
    return this.ayService.ensureAccountYearIsLoaded$(() =>
      this.ayService.loadSelectedAccountYear().pipe(
        tap(() => {
          this.getAccountYears().subscribe();
        })
      )
    );
  }

  getDrilldownReports() {
    return this.performLoad.load$(
      this.service.getDrilldownReports(true, false).pipe(
        tap(reports => {
          const sorted = orderBy(
            reports,
            ['reportName', 'reportNr'],
            ['asc', 'asc']
          );
          this.reports = sorted.map(report => ({
            ...report,
            name: `${report.reportNr} ${report.reportName}`,
          }));
          if (this.reports.length > 0) {
            this.form.patchValue({ reportId: this.reports[0].reportId });
            this.changeReport(this.reports[0].reportId);
          }
        })
      )
    );
  }

  getBudgetList() {
    return this.performLoad.load$(
      this.service
        .getBudgetList(DistributionCodeBudgetType.AccountingBudget)
        .pipe(
          tap(budgets => {
            this.budgets = [{ id: 0, name: '' } as IGenericType];
            const budgetList = budgets
              .map(
                budget =>
                  ({
                    id: budget.budgetHeadId,
                    name: budget.name,
                  }) as IGenericType
              )
              .sort((a, b) => a.name.localeCompare(b.name));
            this.budgets.push(...budgetList);
            this.form.patchValue({
              budgetId: 0,
            });
            this.onSearchValueChange(this.form.value as IDrillDownReportDTO);
          })
        )
    );
  }

  getAccountYears() {
    return this.performLoad.load$(
      this.service.getAccountYears(false, true).pipe(
        tap(data => {
          this.accountYears = data.reverse();
          this.form.patchValue({
            accountYearFromId: this.ayService.selectedAccountYearId(),
            accountYearToId: this.ayService.selectedAccountYearId(),
          });
          this.getAccountPeriods(
            this.ayService.selectedAccountYearId(),
            true,
            true
          ).subscribe();
        })
      )
    );
  }

  getAccountPeriods(accountYearId: number, isFrom: boolean, isTo: boolean) {
    return this.performLoad.load$(
      this.service.getAccountPeriods(accountYearId).pipe(
        tap(data => {
          if (isFrom) {
            this.accountPeriodsFrom = data;
            if (data.length > 0) {
              this.form.patchValue({
                accountPeriodFromId: data[0].accountPeriodId,
                accountPeriodFrom: data[0].from,
              });
            }
          }
          if (isTo) {
            this.accountPeriodsTo = data;
            if (data.length > 0) {
              this.form.patchValue({
                accountPeriodToId: data[data.length - 1].accountPeriodId,
                accountPeriodTo: data[data.length - 1].to,
              });
            }
          }
          this.onSearchValueChange(this.form.value as IDrillDownReportDTO);
        })
      )
    );
  }

  //#endregion

  //#region UI events

  changeReport(reportId: number) {
    if (reportId) {
      const selectedReport = this.reports.find(
        report => report.reportId === reportId
      );
      if (selectedReport) {
        this.form.patchValue({
          sysReportTemplateTypeId: selectedReport.sysReportTemplateTypeId,
        });
        this.onSearchValueChange(this.form.value as IDrillDownReportDTO);
      }
    }
  }

  changeAccountYearFrom(accountYearId: number) {
    this.getAccountPeriods(accountYearId, true, false).subscribe();
  }

  changeAccountYearTo(accountYearId: number) {
    this.getAccountPeriods(accountYearId, false, true).subscribe();
  }

  changeAccountPeriodsFrom(accountPeriodFromId: number) {
    this.onAccountPeriodChanged(accountPeriodFromId, true);
  }
  changeAccountPeriodsTo(accountPeriodToId: number) {
    this.onAccountPeriodChanged(accountPeriodToId, false);
  }
  createDrillDownReport() {
    this.performLoad
      .load$(
        this.service
          .getDrilldownReport(
            this.form.reportId.value,
            this.form.accountPeriodFromId.value,
            this.form.accountPeriodToId.value,
            this.form.budgetId.value
          )
          .pipe(
            tap(data => {
              let reports: DrilldownReportGridFlattenedDTO[] = [];
              if (data && data.length > 0) {
                reports = data;
              }
              this.createReport.emit(reports);
            })
          )
      )
      .subscribe();
  }

  //#endregion
}
