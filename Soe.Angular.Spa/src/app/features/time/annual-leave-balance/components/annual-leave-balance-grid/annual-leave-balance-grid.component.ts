import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  TermGroup_AnnualLeaveTransactionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IAnnualLeaveTransactionGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { AnnualLeaveBalanceService } from '../../services/annual-leave-balance.service';
import { SearchAnnualLeaveTransactionModel } from '../../models/annual-leave-balance.model';
import { Perform } from '@shared/util/perform.class';
import { Observable } from 'rxjs';

@Component({
  selector: 'soe-annual-leave-balance-grid',
  templateUrl: './annual-leave-balance-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AnnualLeaveBalanceGridComponent
  extends GridBaseDirective<
    IAnnualLeaveTransactionGridDTO,
    AnnualLeaveBalanceService
  >
  implements OnInit
{
  model = new SearchAnnualLeaveTransactionModel();
  service = inject(AnnualLeaveBalanceService);
  performAction = new Perform<IAnnualLeaveTransactionGridDTO[]>(
    this.progressService
  );

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_AnnualLeaveBalance,
      'Time.Employee.AnnualLeaveBalance',
      {
        skipInitialLoad: true,
      }
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IAnnualLeaveTransactionGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'core.edit',
        'common.type',
        'time.annualleave.dateearned',
        'time.annualleave.minutesearned',
        'time.annualleave.accumulated',
        'time.annualleave.levelearned',
        'time.annualleave.datespent',
        'time.annualleave.minutesspent',
        'time.annualleave.daybalance',
        'time.annualleave.minutebalance',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('employeeNrAndName', terms['common.name'], {
          flex: 20,
        });
        this.grid.addColumnDate(
          'dateEarned',
          terms['time.annualleave.dateearned'],
          {
            flex: 10,
            enableHiding: true,
            cellClassRules: {
              'information-background-color': (params: any) =>
                this.isManuallyEarned(params.data),
            },
          }
        );
        this.grid.addColumnTimeSpan(
          'minutesEarned',
          terms['time.annualleave.minutesearned'],
          {
            flex: 10,
            enableHiding: true,
            clearZero: this.clearZeroValuesEarnedMinutes.bind(this),
          }
        );
        this.grid.addColumnTimeSpan(
          'accumulatedMinutes',
          terms['time.annualleave.accumulated'],
          {
            flex: 10,
            enableHiding: true,
            cellClassRules: {
              'information-background-color': (params: any) =>
                this.isManuallyEarned(params.data),
            },
            clearZero: this.clearZeroValuesAccumulatedMinutes.bind(this),
          }
        );
        this.grid.addColumnNumber(
          'levelEarned',
          terms['time.annualleave.levelearned'],
          {
            flex: 10,
            enableHiding: true,
            clearZero: this.isManuallyEarned.bind(this),
          }
        );
        this.grid.addColumnDate(
          'dateSpent',
          terms['time.annualleave.datespent'],
          {
            flex: 10,
            enableHiding: true,
            cellClassRules: {
              'information-background-color': (params: any) =>
                this.isManuallySpent(params.data),
            },
          }
        );
        this.grid.addColumnTimeSpan(
          'minutesSpent',
          terms['time.annualleave.minutesspent'],
          {
            flex: 10,
            enableHiding: true,
            cellClassRules: {
              'information-background-color': (params: any) =>
                this.isManuallySpent(params.data),
            },
            clearZero: this.clearZeroValuesSpentMinutes.bind(this),
          }
        );
        this.grid.addColumnNumber(
          'dayBalance',
          terms['time.annualleave.daybalance'],
          {
            flex: 10,
            enableHiding: true,
            clearZero: this.clearZeroValuesDayBalance.bind(this),
          }
        );
        this.grid.addColumnTimeSpan(
          'minuteBalance',
          terms['time.annualleave.minutebalance'],
          {
            flex: 10,
            enableHiding: true,
            clearZero: this.clearZeroValuesMinuteBalance.bind(this),
          }
        );
        this.grid.addColumnText('typeName', terms['common.type'], {
          flex: 10,
          enableHiding: true,
          shapeConfiguration: {
            shape: 'circle',
            colorField: 'typeColor',
            width: 16,
          },
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
          showIcon: row => this.transactionCanBeEdited(row),
        });
        super.finalizeInitGrid();
      });
  }

  transactionCanBeEdited(row: IAnnualLeaveTransactionGridDTO): boolean {
    return (
      row.type == TermGroup_AnnualLeaveTransactionType.ManuallyEarned ||
      (row.type == TermGroup_AnnualLeaveTransactionType.Calculated &&
        row.manuallySpent)
    );
  }

  doFilter(event: SearchAnnualLeaveTransactionModel) {
    this.model = event;
    this.refreshGrid();
  }

  isManuallySpent(data: IAnnualLeaveTransactionGridDTO): boolean {
    return (
      data.type === TermGroup_AnnualLeaveTransactionType.Calculated &&
      data.manuallySpent
    );
  }

  isManuallyEarned(data: IAnnualLeaveTransactionGridDTO): boolean {
    return data.type === TermGroup_AnnualLeaveTransactionType.ManuallyEarned;
  }

  clearZeroValuesDayBalance(data: IAnnualLeaveTransactionGridDTO): boolean {
    return data.type === TermGroup_AnnualLeaveTransactionType.ManuallyEarned;
  }

  clearZeroValuesAccumulatedMinutes(
    data: IAnnualLeaveTransactionGridDTO
  ): boolean {
    return data.type === TermGroup_AnnualLeaveTransactionType.YearlyBalance;
  }

  clearZeroValuesEarnedMinutes(data: IAnnualLeaveTransactionGridDTO): boolean {
    return (
      !data.dateEarned ||
      data.type === TermGroup_AnnualLeaveTransactionType.ManuallyEarned ||
      data.type === TermGroup_AnnualLeaveTransactionType.YearlyBalance
    );
  }

  clearZeroValuesSpentMinutes(data: IAnnualLeaveTransactionGridDTO): boolean {
    return (
      !data.dateSpent ||
      data.type === TermGroup_AnnualLeaveTransactionType.ManuallyEarned ||
      data.type === TermGroup_AnnualLeaveTransactionType.YearlyBalance
    );
  }

  clearZeroValuesMinuteBalance(data: IAnnualLeaveTransactionGridDTO): boolean {
    return data.type !== TermGroup_AnnualLeaveTransactionType.YearlyBalance;
  }

  override loadData(
    id?: number | undefined
  ): Observable<IAnnualLeaveTransactionGridDTO[]> {
    return this.performAction.load$(
      this.service.getGrid(undefined, { model: this.model })
    );
  }
}
