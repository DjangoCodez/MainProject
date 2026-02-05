import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ITimeWorkReductionReconciliationGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs';
import { EmployeeGroupsService } from '@features/time/employee-groups/services/employee-groups.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TimeWorkReductionService } from '../../services/time-work-reduction.service';

@Component({
  selector: 'soe-time-work-reduction-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeWorkReductionGridComponent
  extends GridBaseDirective<
    ITimeWorkReductionReconciliationGridDTO,
    TimeWorkReductionService
  >
  implements OnInit
{
  service = inject(TimeWorkReductionService);
  employeeGroupsService = inject(EmployeeGroupsService);
  coreService = inject(CoreService);

  timeWorkReductionWithdrawalMethods: SmallGenericType[] | undefined;
  timeAccumulators: SmallGenericType[] | undefined;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Time_TimeWorkReduction,
      'Time.Time.TimeWorkReduction',
      {
        lookups: [
          this.loadTimeAccumulators(),
          this.loadDefaultWithdrawalMethod(),
        ],
      }
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeWorkReductionReconciliationGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.description',
        'core.edit',
        'time.payroll.worktimeaccount.usepensiondeposit',
        'time.payroll.worktimeaccount.usedirectpayment',
        'time.time.timeaccumulators.timeaccumulator',
        'time.payroll.worktimeaccount.defaultwithdrawalmethod',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'timeAccumulatorId',
          terms['time.time.timeaccumulators.timeaccumulator'],
          this.timeAccumulators || [],
          undefined,
          {
            flex: 12,
            editable: false,
            enableHiding: false,
          }
        );

        this.grid.addColumnText('description', terms['common.description'], {
          flex: 10,
          editable: false,
          enableHiding: true,
        });

        this.grid.addColumnBool(
          'usePensionDeposit',
          terms['time.payroll.worktimeaccount.usepensiondeposit'],
          {
            flex: 10,
            editable: false,
            enableHiding: true,
          }
        );
        this.grid.addColumnBool(
          'useDirectPayment',
          terms['time.payroll.worktimeaccount.usedirectpayment'],
          {
            flex: 10,
            editable: false,
            enableHiding: true,
          }
        );

        this.grid.addColumnSelect(
          'defaultWithdrawalMethod',
          terms['time.payroll.worktimeaccount.defaultwithdrawalmethod'],
          this.timeWorkReductionWithdrawalMethods || [],
          undefined,
          {
            flex: 12,
            editable: false,
            enableHiding: true,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  private loadTimeAccumulators() {
    return this.employeeGroupsService
      .getTimeAccumulatorsDict(true, false, true)
      .pipe(tap(x => (this.timeAccumulators = x)));
  }

  private loadDefaultWithdrawalMethod() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeWorkReductionWithdrawalMethod,
        false,
        false
      )
      .pipe(
        tap(x => {
          this.timeWorkReductionWithdrawalMethods = x;
        })
      );
  }
}
