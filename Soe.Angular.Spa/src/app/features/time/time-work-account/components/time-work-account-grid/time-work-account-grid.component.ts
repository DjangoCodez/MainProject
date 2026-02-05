import { Component, OnInit, inject } from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take, tap } from 'rxjs/operators';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ITimeWorkAccountGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TimeWorkAccountService } from '../../services/time-work-account.service';

@Component({
  selector: 'soe-time-work-account-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeWorkAccountGridComponent
  extends GridBaseDirective<ITimeWorkAccountGridDTO, TimeWorkAccountService>
  implements OnInit
{
  service = inject(TimeWorkAccountService);
  coreService = inject(CoreService);
  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Payroll_TimeWorkAccount,
      'Time.Payroll.WorkTimeAccount',
      {
        lookups: this.loadWithdrawalMethods(),
      }
    );
  }
  withdrawalMethods: SmallGenericType[] = [];
  terms: any = [];

  override onGridReadyToDefine(grid: GridComponent<ITimeWorkAccountGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.code',
        'core.edit',
        'time.payroll.worktimeaccount.usepensiondeposit',
        'time.payroll.worktimeaccount.usepaidleave',
        'time.payroll.worktimeaccount.usedirectpayment',
        'time.payroll.worktimeaccount.defaultwithdrawalmethod',
        'time.payroll.worktimeaccount.defaultpaidleavenotused',
        'core.yes',
        'core.no',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;
        this.grid.addColumnText('name', this.terms['common.name'], {
          flex: 100,
        });
        this.grid.addColumnText('code', this.terms['common.code']);
        this.grid.addColumnBool(
          'usePensionDeposit',
          this.terms['time.payroll.worktimeaccount.usepensiondeposit']
        );
        this.grid.addColumnBool(
          'usePaidLeave',
          this.terms['time.payroll.worktimeaccount.usepaidleave']
        );
        this.grid.addColumnBool(
          'useDirectPayment',
          this.terms['time.payroll.worktimeaccount.usedirectpayment']
        );
        this.grid.addColumnSelect(
          'defaultWithdrawalMethod',
          this.terms['time.payroll.worktimeaccount.defaultwithdrawalmethod'],
          this.withdrawalMethods,
          null,
          { flex: 100 }
        );
        this.grid.addColumnSelect(
          'defaultPaidLeaveNotUsed',
          this.terms['time.payroll.worktimeaccount.defaultpaidleavenotused'],
          this.withdrawalMethods,
          null,
          { flex: 100 }
        );
        this.grid.addColumnIconEdit({
          tooltip: this.terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  private loadWithdrawalMethods() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeWorkAccountWithdrawalMethod,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.withdrawalMethods = x;
          this.withdrawalMethods.push({ id: 0, name: '' });
        })
      );
  }
}
