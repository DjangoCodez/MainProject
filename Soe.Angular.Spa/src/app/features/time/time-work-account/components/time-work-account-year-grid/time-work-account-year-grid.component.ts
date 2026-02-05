import {
  Component,
  Input,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take } from 'rxjs/operators';
import {
  Feature,
  TermGroup_TimeWorkAccountWithdrawalMethod,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ITimeWorkAccountYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, of } from 'rxjs';
import {
  ITimeWorkAccountYearDialogData,
  TimeWorkAccountYearEditComponent,
} from '../time-work-account-year-edit/time-work-account-year-edit.component';
import { TimeWorkAccountService } from '../../services/time-work-account.service';

@Component({
  selector: 'soe-time-work-account-year-grid',
  templateUrl: 'time-work-account-year-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeWorkAccountYearGridComponent
  extends GridBaseDirective<ITimeWorkAccountYearDTO>
  implements OnInit
{
  dialogService = inject(DialogService);
  timeWorkAccountService = inject(TimeWorkAccountService);

  @Input() rows!: BehaviorSubject<ITimeWorkAccountYearDTO[]>;
  timeWorkAccountId = input<number>(0);
  @Input() usePension!: boolean;
  @Input() useDirectPayment!: boolean;
  @Input() usePaidLeave!: boolean;
  @Input() defaultPaidLeaveNotUsed!: TermGroup_TimeWorkAccountWithdrawalMethod;

  isDisabled = computed(() => {
    return this.timeWorkAccountId() == 0;
  });

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Payroll_TimeWorkAccount,
      'Time.Payroll.WorkTimeAccount',
      { skipInitialLoad: true }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ITimeWorkAccountYearDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.payroll.worktimeaccount.earningstart',
        'time.payroll.worktimeaccount.earningstop',
        'time.payroll.worktimeaccount.employeelastdecideddate',
        'time.payroll.worktimeaccount.paidabsencestopdate',
        'time.payroll.worktimeaccount.directpaymentlastdate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;
        this.grid.addColumnDate(
          'earningStart',
          this.terms['time.payroll.worktimeaccount.earningstart'],
          { flex: 20 }
        );
        this.grid.addColumnDate(
          'earningStop',
          this.terms['time.payroll.worktimeaccount.earningstop'],
          { flex: 20 }
        );
        this.grid.addColumnDate(
          'employeeLastDecidedDate',
          this.terms['time.payroll.worktimeaccount.employeelastdecideddate'],
          { flex: 20 }
        );
        this.grid.addColumnDate(
          'paidAbsenceStopDate',
          this.terms['time.payroll.worktimeaccount.paidabsencestopdate'],
          { flex: 20 }
        );
        this.grid.addColumnDate(
          'directPaymentLastDate',
          this.terms['time.payroll.worktimeaccount.directpaymentlastdate'],
          { flex: 20 }
        );
        this.grid.addColumnIconEdit({
          tooltip: this.terms['core.edit'],
          onClick: row => {
            this.editTimeWorkAccountYear(row.timeWorkAccountYearId);
          },
        });
        super.finalizeInitGrid();
      });
  }
  // ACTIONS

  editTimeWorkAccountYear(id = 0) {
    this.dialogService
      .open(TimeWorkAccountYearEditComponent, {
        title: id != 0 ? 'core.edit' : 'common.new',
        size: 'fullscreen',
        hideFooter: true,
        new: id == 0,
        id: id,
        timeWorkAccountId: this.timeWorkAccountId(),
        usePension: this.usePension,
        useDirectPayment: this.useDirectPayment,
        usePaidLeave: this.usePaidLeave,
        defaultPaidLeaveNotUsed: this.defaultPaidLeaveNotUsed,
      } as ITimeWorkAccountYearDialogData)
      .afterClosed()
      .pipe(take(1))
      .subscribe(() => {
        this.loadGridData();
      });
  }

  override createGridToolbar() {
    //this.annualRunsToolbarService.clearItemGroups();
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('newyear', {
          iconName: signal('times-circle'),
          caption: signal('time.payroll.worktimeaccount.employee.newyear'),
          tooltip: signal('time.payroll.worktimeaccount.employee.newyear'),
          disabled: this.isDisabled,
          onAction: () => {
            this.editTimeWorkAccountYear(0);
          },
        }),
      ],
    });
  }

  private loadGridData() {
    return of(
      this.timeWorkAccountService
        .get(this.timeWorkAccountId(), true)
        .subscribe(data => {
          this.grid.setData(data.timeWorkAccountYears);
          this.rows.next(data.timeWorkAccountYears);
        })
    );
  }
}
