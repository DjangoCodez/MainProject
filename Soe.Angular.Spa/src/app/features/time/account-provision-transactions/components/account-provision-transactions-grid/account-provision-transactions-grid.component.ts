import { Component, inject, OnInit } from '@angular/core';
import { AttestStateDTO } from '@shared/components/billing/purchase-customer-invoice-rows/models/purchase-customer-invoice-rows.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  TermGroup_AttestEntity,
  TermGroup_TimePeriodType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountProvisionTransactionGridDTO,
  IAttestStateDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IAccountProvisionTransactionsModel } from '@shared/models/generated-interfaces/TimeModels';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { AccountProvisionTransactionsForm } from '../../models/account-provision-transactions-form.model';
import { AccountProvisionTransactionsService } from '../../services/account-provision-transactions.service';

@Component({
  selector: 'soe-account-provision-transactions-grid',
  standalone: false,
  templateUrl: 'account-provision-transactions-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class AccountProvisionTransactionsGridComponent
  extends GridBaseDirective<any, AccountProvisionTransactionsService>
  implements OnInit
{
  service = inject(AccountProvisionTransactionsService);
  coreService = inject(CoreService);
  validationHandler = inject(ValidationHandler);
  messageboxService = inject(MessageboxService);
  attestStates: AttestStateDTO[] = [];
  attestStateInitial: IAttestStateDTO | undefined;
  menuList: MenuButtonItem[] = [];
  progress = new Perform<any[]>(this.progressService);
  timePeriodHeadId: number = 0;
  timePeriodId: number = 0;
  timePeriodHeads: any[] = [];
  timePeriods: any[] = [];
  showGrid: boolean = false;

  form: AccountProvisionTransactionsForm | undefined;
  employeeAccountProvisionTransactions:
    | IAccountProvisionTransactionGridDTO[]
    | undefined;

  ngOnInit() {
    super.ngOnInit();
    this.form = this.createForm();

    this.startFlow(
      Feature.Time_Payroll_Provision_AccountProvisionTransaction,
      'Time.Time.AccountProvisionTransactions',
      {
        skipInitialLoad: true,
        lookups: [
          this.loadTimePeriodHeads(),
          this.loadAttestStates(),
          this.loadInitialAttestState(TermGroup_AttestEntity.PayrollTime),
        ],
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<any>) {
    this.grid = grid;
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.employee.employee.employeenr',
        'common.firstname',
        'common.lastname',
        'time.payroll.accountprovision.accountnr',
        'time.payroll.accountprovision.accountname',
        'time.payroll.accountprovision.accountdesc',
        'time.payroll.accountprovision.worktime',
        'time.payroll.accountprovision.comment',
        'common.amount',
        'time.atteststate.state',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'employeeNr',
          terms['time.employee.employee.employeenr'],
          {
            editable: false,
            flex: 5,
          }
        );
        this.grid.addColumnText(
          'employeeFirstName',
          terms['common.firstname'],
          {
            editable: false,
            flex: 12,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('employeeLastName', terms['common.lastname'], {
          editable: false,
          enableHiding: true,
          flex: 12,
        });
        this.grid.addColumnText(
          'accountNr',
          terms['time.payroll.accountprovision.accountnr'],
          {
            editable: false,
            enableHiding: true,
            flex: 10,
          }
        );
        this.grid.addColumnText(
          'accountName',
          terms['time.payroll.accountprovision.accountname'],
          {
            editable: false,
            enableHiding: true,
            flex: 10,
          }
        );
        this.grid.addColumnText(
          'accountDescription',
          terms['time.payroll.accountprovision.accountdesc'],
          {
            editable: false,
            enableHiding: true,
            flex: 10,
          }
        );
        this.grid.addColumnText(
          'workTime',
          terms['time.payroll.accountprovision.worktime'],
          {
            editable: false,
            enableHiding: true,
            flex: 5,
          }
        );
        this.grid.addColumnText(
          'comment',
          terms['time.payroll.accountprovision.comment'],
          {
            editable: x => this.isRowEditable(x.data),
            flex: 20,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('amount', terms['common.amount'], {
          editable: x => this.isRowEditable(x.data),
          flex: 5,
        });
        this.grid.addColumnShape('attestStateColor', '', {
          shape: 'circle',
          colorField: 'attestStateColor',
          flex: 1,
        });
        this.grid.addColumnText(
          'attestStateName',
          terms['time.atteststate.state'],
          {
            editable: false,
            flex: 10,
          }
        );

        super.finalizeInitGrid();
        this.grid.resetColumns();
        this.grid.setData([]);
      });
  }

  createForm(
    element?: AccountProvisionTransactionsForm
  ): AccountProvisionTransactionsForm {
    return new AccountProvisionTransactionsForm({
      validationHandler: this.validationHandler,
      element,
    });
  }

  loadTimePeriodHeads(): Observable<any[]> {
    return this.progress.load$(
      this.service
        .getTimePeriodHeadsDict(TermGroup_TimePeriodType.Payroll, false)
        .pipe(
          tap(value => {
            this.timePeriodHeads = value;
          })
        )
    );
  }

  loadTimePeriods(timePeriodHeadId: number) {
    this.timePeriodHeadId = timePeriodHeadId;
    return this.progress
      .load$(
        this.service.getTimePeriodsDict(timePeriodHeadId, true).pipe(
          tap(value => {
            this.timePeriods = value;
            if (this.timePeriods.length !== 0) {
              this.form?.controls.timePeriodId.enable();
            } else {
              this.form?.controls.timePeriodId.disable();
            }
          })
        )
      )
      .subscribe();
  }

  loadAttestStates(): Observable<AttestStateDTO[]> {
    this.attestStates = [];

    return this.progress.load$(
      this.coreService
        .getUserValidAttestStates(
          TermGroup_AttestEntity.PayrollTime,
          ' ',
          ' ',
          true
        )
        .pipe(
          tap(result => {
            this.attestStates = result;

            result.forEach((attestState: AttestStateDTO) => {
              this.menuList.push({
                id: attestState.attestStateId,
                label: attestState.name,
              });
            });
          })
        )
    );
  }

  loadInitialAttestState(
    entity: TermGroup_AttestEntity
  ): Observable<IAttestStateDTO> {
    return this.coreService.getAttestStateInitial(entity).pipe(
      tap(x => {
        this.attestStateInitial = x;
      })
    );
  }
  loadGrid(timePeriodId: number) {
    this.showGrid = true;
    this.timePeriodId = timePeriodId;
    return this.progress
      .load$(
        this.service.getAccountProvisionTransactions(timePeriodId).pipe(
          tap(x => {
            this.employeeAccountProvisionTransactions = x;
            this.form?.patchRows(this.employeeAccountProvisionTransactions);
            this.grid.setData(this.employeeAccountProvisionTransactions);
          })
        )
      )
      .subscribe();
  }

  save() {
    const modifiedTransactions: IAccountProvisionTransactionsModel = {
      transactions: [],
    };
    this.grid.agGrid.api.stopEditing();
    this.employeeAccountProvisionTransactions
      ?.filter((r: any) =>
        this.form?.value.rows?.some(
          (row: any) =>
            r.timePayrollTransactionId === row.timePayrollTransactionId &&
            (r.amount !== row.amount || r.comment !== row.comment)
        )
      )
      .forEach((x: any) => {
        x.isModified = true;
        modifiedTransactions.transactions.push(x);
      });

    return of(
      this.progress.crud(
        CrudActionTypeEnum.Save,
        this.service
          .updateAccountProvisionTransactions(modifiedTransactions)
          .pipe(
            tap((value: any) => {
              if (value.success) {
                setTimeout((): void => {
                  this.loadGrid(this.form?.value.timePeriodId);
                }, 100);
              }
            })
          )
      )
    ).subscribe();
  }
  peformAction(selected: MenuButtonItem) {
    const modifiedTransactions: IAccountProvisionTransactionsModel = {
      transactions: [],
    };
    this.grid.agGrid.api.stopEditing();

    this.employeeAccountProvisionTransactions
      ?.filter((r: any) =>
        this.grid
          .getSelectedRows()
          .some(
            (row: any) =>
              r.timePayrollTransactionId === row.timePayrollTransactionId &&
              r.attestStateId !== selected.id
          )
      )
      .forEach((x: any) => {
        x.isModified = true;
        x.attestStateId = selected.id;
        modifiedTransactions.transactions.push(x);
      });

    return of(
      this.progress.crud(
        CrudActionTypeEnum.Save,
        this.service.saveAttestForAccountProvision(modifiedTransactions).pipe(
          tap((value: any) => {
            if (value.success) {
              setTimeout((): void => {
                this.loadGrid(this.form?.value.timePeriodId);
              }, 100);
            }
          })
        )
      )
    ).subscribe();
  }
  timePeriodHeadSelected(timePeriodHeadId: number) {
    if (this.hasChanged()) {
      const mb = this.messageboxService.warning(
        'core.warning',
        'time.payroll.accountprovision.changeperiodwarning'
      );
      mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
        if (response?.result) this.loadTimePeriods(timePeriodHeadId);
        else
          this.form?.controls.timePeriodHeadId.setValue(this.timePeriodHeadId);
      });
    } else {
      this.loadTimePeriods(timePeriodHeadId);
    }
  }

  timePeriodSelected(timePeriodId: number) {
    if (this.hasChanged()) {
      const mb = this.messageboxService.warning(
        'core.warning',
        'time.payroll.accountprovision.changeperiodwarning'
      );

      mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
        if (response?.result) {
          this.loadGrid(timePeriodId);
        } else this.form?.controls.timePeriodId.setValue(this.timePeriodId);
      });
    } else {
      this.loadGrid(timePeriodId);
    }
  }

  hasChanged(): boolean {
    if (this.form?.value.rows.length === 0) return false;

    return (
      (this.form?.value.rows.filter((r: any) =>
        this.employeeAccountProvisionTransactions?.some(
          (row: any) =>
            r.timePayrollTransactionId === row.timePayrollTransactionId &&
            (r.amount !== row.amount || r.comment !== row.comment)
        )
      )).length > 0
    );
  }

  hasSelectedRows(): boolean {
    if (this.grid === undefined) return false;

    return this.grid.getSelectedRows().length > 0;
  }

  isRowEditable(row: any) {
    return (
      this.flowHandler.modifyPermission() &&
      row.attestStateId === this.attestStateInitial?.attestStateId
    );
  }
}
