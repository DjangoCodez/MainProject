import { Component, OnInit, inject } from '@angular/core';
import { AccountYearForm } from '@features/economy/account-years-and-periods/models/account-year-form.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { StorageService } from '@shared/services/storage.service';
import { Perform } from '@shared/util/perform.class';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import {
  AccountPeriodDTO,
  AccountYearDTO,
} from '../../../models/account-years-and-periods.model';
import { AccountYearService } from '../../../services/account-year.service';
import { AccountYearEditComponent } from '../account-year-edit/account-year-edit.component';

@Component({
  selector: 'soe-account-year-grid',
  templateUrl: './account-year-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountYearGridComponent
  extends GridBaseDirective<AccountYearDTO, AccountYearService>
  implements OnInit
{
  service = inject(AccountYearService);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);
  storageService = inject(StorageService);

  performAction = new Perform<AccountYearDTO[]>(this.progressService);

  private accountStatuses: ISmallGenericType[] = [];
  private budgetSubTypes: ISmallGenericType[] = [];

  latestTo!: unknown;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_AccountPeriods,
      'economy.accounting.accountyear.accountyears',
      {
        lookups: [this.loadAccountStatuses(), this.loadBudgetSubTypes()],
      }
    );
  }

  private loadAccountStatuses(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AccountStatus, true, false)
      .pipe(
        tap(x => {
          this.accountStatuses = x;
        })
      );
  }

  private loadBudgetSubTypes(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.AccountingBudgetSubType,
        false,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.budgetSubTypes = x;
        })
      );
  }

  override onFinished(): void {
    if (this.storageService.get('newData')) {
      const row = this.storageService.get('newData');
      this.edit(
        {
          ...row,
          accountYearId: row.accountYearId,
        },
        {
          filteredRows: [],
          editComponent: AccountYearEditComponent,
          editTabLabel: 'economy.accounting.accountyear.accountyear',
          FormClass: AccountYearForm,
        }
      );
      this.storageService.set('newData', undefined);
    }
  }

  loadDetailRows(params: any) {
    params.data.periods.forEach((period: AccountPeriodDTO) => {
      const periodStatus = this.accountStatuses.find(
        o => o.id === period.status
      );
      const monthName = this.budgetSubTypes.find(
        t => t.id === period.from.getMonth() + 1
      );

      if (periodStatus) period.statusName = periodStatus.name;
      if (monthName) period.monthName = monthName.name;
      period.statusIcon = this.service.getStatusIcon(period.status);
      period.statusName = <string>(
        this.accountStatuses.find(s => s.id == period.status)?.name
      );
      period.periodName =
        period.from.getFullYear().toString() +
        '-' +
        (period.from.getMonth() + 1).toString();
    });

    params.successCallback(params.data.periods);
  }

  override onGridReadyToDefine(grid: GridComponent<AccountYearDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'economy.accounting.accountyear',
        'common.status',
        'common.number',
        'common.period',
        'core.time.month',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        //Details
        this.grid.enableMasterDetail(
          {
            detailRowHeight: 200,

            columnDefs: [
              ColumnUtil.createColumnNumber(
                'periodNr',
                terms['common.number'],
                {
                  flex: 40,
                  pinned: 'left',
                }
              ),
              ColumnUtil.createColumnText(
                'periodName',
                terms['common.period'],
                {
                  flex: 1,
                }
              ),
              ColumnUtil.createColumnText(
                'monthName',
                terms['core.time.month'],
                {
                  flex: 1,
                }
              ),
              ColumnUtil.createColumnText(
                'statusName',
                terms['common.status'],
                {
                  flex: 1,
                }
              ),
              ColumnUtil.createColumnShape('status', '', {
                flex: 1,
                shape: 'circle',
                colorField: 'statusIcon',
                tooltipField: 'statusName',
                pinned: 'right',
              }),
            ],
          },
          {
            autoHeight: false,
            getDetailRowData: (params: any) => {
              this.loadDetailRows(params);
            },
          }
        );

        //Master
        this.grid.addColumnText(
          'yearFromTo',
          terms['economy.accounting.accountyear'],
          {
            flex: 1,
            enableHiding: false,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnText('statusText', terms['common.status'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnShape('statusIcon', '', {
          flex: 1,
          enableHiding: true,
          shape: 'circle',
          colorField: 'statusIcon',
          tooltipField: 'statusText',
          pinned: 'right',
        });
        this.grid.addColumnIconEdit({
          iconName: 'pencil',
          iconClass: 'pencil',
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit({ ...row, accountYearId: row.accountYearId });
          },
        });

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      getPeriods: boolean;
      excludeNew: boolean;
    }
  ): Observable<AccountYearDTO[]> {
    return super.loadData(id, {
      getPeriods: true,
      excludeNew: false,
    });
  }
}
