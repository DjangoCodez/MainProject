import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  TermGroup_TimePeriodType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IAccountProvisionBaseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import _ from 'lodash';
import { Observable, of, take, tap } from 'rxjs';
import { AccountProvisionBaseForm } from '../../models/account-provision-base-form.model';
import { AccountProvisionBaseService } from '../../services/account-provision-base.service';

@Component({
  selector: 'soe-account-provision-base-grid',
  standalone: false,
  templateUrl: 'account-provision-base-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class AccountProvisionBaseGridComponent
  extends GridBaseDirective<any, AccountProvisionBaseService>
  implements OnInit
{
  service = inject(AccountProvisionBaseService);
  validationHandler = inject(ValidationHandler);
  messageboxService = inject(MessageboxService);
  progress = new Perform<any[]>(this.progressService);
  timePeriodHeadId: number = 0;
  timePeriodId: number = 0;
  timePeriodHeads: any[] = [];
  timePeriods: any[] = [];
  columns: any[] = [];
  isLocked: boolean = true;
  showGrid: boolean = false;
  variableColumnStart = 4;

  form: AccountProvisionBaseForm | undefined;
  employeeAccountProvisionBases: IAccountProvisionBaseDTO[] | undefined;

  ngOnInit() {
    super.ngOnInit();
    this.form = this.createForm();

    this.startFlow(
      Feature.Time_Payroll_Provision_AccountProvisionBase,
      'Time.Time.AccountProvisionBase',
      {
        skipInitialLoad: true,
        lookups: [this.loadTimePeriodHeads()],
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IAccountProvisionBaseDTO>) {
    this.grid = grid;
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.payroll.accountprovision.accountnr',
        'time.payroll.accountprovision.accountname',
        'time.payroll.accountprovision.accountdesc',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified', {
          editable: false,
          flex: 1,
        });
        this.grid.addColumnText(
          'accountNr',
          terms['time.payroll.accountprovision.accountnr'],
          {
            editable: false,
            flex: 15,
          }
        );
        this.grid.addColumnText(
          'accountName',
          terms['time.payroll.accountprovision.accountname'],
          {
            editable: false,
            flex: 15,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'accountDescription',
          terms['time.payroll.accountprovision.accountdesc'],
          {
            editable: false,
            flex: 15,
            enableHiding: true,
          }
        );
        for (let i = 1; i <= 11; i++) {
          this.grid.addColumnText(`period${i}Value`, '', {
            editable: false,
            flex: 10,
            enableHiding: true,
          });
        }
        this.grid.addColumnText(`period12Value`, '', {
          editable: () => this.checkLocked(),
          flex: 10,
        });

        super.finalizeInitGrid();
        this.grid.resetColumns();
        this.grid.setData([]);
      });
  }

  createForm(element?: AccountProvisionBaseForm): AccountProvisionBaseForm {
    return new AccountProvisionBaseForm({
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

  loadAccountProvisionBaseColumns(timePeriodId: number) {
    this.timePeriodId = timePeriodId;
    return this.progress
      .load$(
        this.service.getAccountProvisionBaseColumns(timePeriodId).pipe(
          tap(value => {
            this.columns = value;
            this.showGrid = true;
            this.loadAccountProvisionBase(timePeriodId);
          })
        )
      )
      .subscribe();
  }

  loadAccountProvisionBase(timePeriodId: number) {
    return this.progress
      .load$(
        this.service.getAccountProvisionBase(timePeriodId).pipe(
          tap(value => {
            this.employeeAccountProvisionBases = value;
            this.isLocked = _.some(
              this.employeeAccountProvisionBases,
              (row: any) => row.isLocked
            );

            this.grid.resetColumns();
            this.form?.patchRows(this.employeeAccountProvisionBases);
            this.grid.setData(this.form?.value.rows);
            const columns = this.grid.api.getColumnDefs();

            columns?.forEach((column, index) => {
              if (
                index >= this.variableColumnStart &&
                index < 12 + this.variableColumnStart
              ) {
                column.headerName = this.columnHeader(
                  index - this.variableColumnStart
                );
              }
            });
            this.grid.api.updateGridOptions({ columnDefs: columns });

            this.form?.markAsPristine();
            this.form?.markAsUntouched();
          })
        )
      )
      .subscribe();
  }

  initLock() {
    const mb = this.messageboxService.warning(
      'time.payroll.accountprovision.locktitle',
      'time.payroll.accountprovision.lockmessage'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.lock();
    });
  }

  initUnLock() {
    const mb = this.messageboxService.warning(
      'time.payroll.accountprovision.unlocktitle',
      'time.payroll.accountprovision.unlockmessage'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.unLock();
    });
  }

  lock() {
    return of(
      this.progress.crud(
        CrudActionTypeEnum.Save,
        this.service
          .lockAccountProvisionBase(this.form?.value.timePeriodId)
          .pipe(
            tap((value: any) => {
              if (value.success) {
                this.loadAccountProvisionBase(this.form?.value.timePeriodId);
              }
            })
          )
      )
    ).subscribe();
  }

  unLock() {
    return of(
      this.progress.crud(
        CrudActionTypeEnum.Save,
        this.service
          .unLockAccountProvisionBase(this.form?.value.timePeriodId)
          .pipe(
            tap((value: any) => {
              if (value.success) {
                this.loadAccountProvisionBase(this.form?.value.timePeriodId);
              }
            })
          )
      )
    ).subscribe();
  }

  save() {
    this.grid.agGrid.api.stopEditing();
    this.form?.value.rows
      .filter((r: IAccountProvisionBaseDTO) =>
        this.employeeAccountProvisionBases?.some(
          (row: IAccountProvisionBaseDTO) =>
            r.accountNr === row.accountNr &&
            r.period12Value !== row.period12Value
        )
      )
      .forEach((r: IAccountProvisionBaseDTO) => {
        r.isModified = true;
      });
    const filteredList =
      this.form?.value.rows.filter(
        (row: IAccountProvisionBaseDTO) => row.isModified
      ) || [];

    return of(
      this.progress.crud(
        CrudActionTypeEnum.Save,
        this.service.saveAccountProvisionBase(filteredList).pipe(
          tap((value: any) => {
            if (value.success) {
              setTimeout((): void => {
                this.loadAccountProvisionBase(this.form?.value.timePeriodId);
              }, 100);
            } else {
              this.messageboxService.error('core.error', value.message);
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
        if (response?.result)
          this.loadAccountProvisionBaseColumns(timePeriodId);
        else this.form?.controls.timePeriodId.setValue(this.timePeriodId);
      });
    } else {
      this.loadAccountProvisionBaseColumns(timePeriodId);
    }
  }

  columnHeader(period: number): string {
    if (!this.columns[period]) return '';

    return this.columns[period].toString();
  }

  formIsDirty(): boolean {
    return this.form?.dirty ?? false;
  }

  checkLocked(): boolean {
    setTimeout((): void => {}, 100);
    return !this.isLocked;
  }

  hasChanged(): boolean {
    if (this.form?.value.rows.length === 0) return false;

    return (
      (this.form?.value.rows.filter((r: IAccountProvisionBaseDTO) =>
        this.employeeAccountProvisionBases?.some(
          (row: IAccountProvisionBaseDTO) =>
            r.accountNr === row.accountNr &&
            r.period12Value !== row.period12Value
        )
      )).length > 0
    );
  }
}
