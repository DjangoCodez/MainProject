import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IAccountDimGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { GridResizeType } from '@ui/grid/enums/resize-type.enum';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IDefaultFilterSettings } from '@ui/grid/interfaces';
import { take } from 'rxjs';
import { AccountingCodingLevelsService } from '../../services/accounting-coding-levels.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-accounting-coding-levels-grid',
  templateUrl: './accounting-coding-levels-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountingCodingLevelsGridComponent
  extends GridBaseDirective<IAccountDimGridDTO, AccountingCodingLevelsService>
  implements OnInit
{
  service = inject(AccountingCodingLevelsService);
  private readonly messageBoxService = inject(MessageboxService);
  private readonly performAction = new Perform<BackendResponse>(
    this.progressService
  );
  protected isDisabled = signal<boolean>(true);

  protected hasEditPermission: boolean = false;

  ngOnInit(): void {
    this.startFlow(
      Feature.Economy_Accounting_AccountRoles,
      'Economy.Accounting.AccountDims',
      {
        additionalModifyPermissions: [
          Feature.Economy_Accounting_AccountRoles_Inactivate,
          Feature.Economy_Accounting_AccountRoles_Edit,
        ],
      }
    );
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();

    this.hasEditPermission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Accounting_AccountRoles_Edit
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IAccountDimGridDTO>): void {
    super.onGridReadyToDefine(grid);
    this.grid.api.updateGridOptions({
      onColumnMoved: (): void => {
        this.grid.resizeColumns(GridResizeType.ToFit);
      },
    });

    this.translate
      .get([
        'common.active',
        'common.number',
        'common.name',
        'common.shortname',
        'core.edit',
        'economy.accounting.siedim',
        'economy.accounting.account.childdim',
        'economy.accounting.useinscheduleplanning',
        'economy.accounting.excludeinaccountingexport',
        'economy.accounting.excludeinsalaryreport',
        'economy.accounting.onlyallowaccountswithparent',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        if (this.hasEditPermission) {
          this.grid.enableRowSelection();
        }

        if (
          this.flowHandler.hasModifyAccess(
            Feature.Economy_Accounting_AccountRoles_Inactivate
          )
        )
          this.grid.addColumnActive('isActive', terms['common.active'], {
            width: 80,
            enableHiding: false,
            setChecked: true,
            pinned: undefined,
          });

        this.grid.addColumnText('accountDimNr', terms['common.number'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 2,
          enableHiding: false,
        });
        this.grid.addColumnText('shortName', terms['common.shortname'], {
          flex: 2,
          enableHiding: false,
        });
        this.grid.addColumnText(
          'parentAccountDimName',
          terms['economy.accounting.account.childdim'],
          {
            flex: 2,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'sysSieDimNr',
          terms['economy.accounting.siedim'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnBool(
          'useInSchedulePlanning',
          terms['economy.accounting.useinscheduleplanning'],
          { width: 40, enableHiding: true }
        );
        this.grid.addColumnBool(
          'excludeinAccountingExport',
          terms['economy.accounting.excludeinaccountingexport'],
          { width: 40, enableHiding: true }
        );
        this.grid.addColumnBool(
          'excludeinSalaryReport',
          terms['economy.accounting.excludeinsalaryreport'],
          { width: 40, enableHiding: true }
        );
        this.grid.addColumnBool(
          'onlyAllowAccountsWithParent',
          terms['economy.accounting.onlyallowaccountswithparent'],
          { width: 40, enableHiding: true }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        this.grid.columnsAreSizedToFit;
        const defaultFilter: IDefaultFilterSettings = {
          field: 'isActive',
          filterModel: {
            values: ['true'],
          },
        };
        super.finalizeInitGrid(undefined, defaultFilter);
      });
  }

  protected accountRowSelection(rows: IAccountDimGridDTO[]): void {
    this.isDisabled.set(rows.length === 0);
  }

  protected deleteDims(): void {
    this.messageBoxService
      .warning('core.warning', 'core.deleterowwarning')
      .afterClosed()
      .subscribe((res: IMessageboxComponentResponse): void => {
        if (res.result === true) {
          const _ids = this.grid.getSelectedIds('accountDimId');
          this.performAction.crud(
            CrudActionTypeEnum.Delete,
            this.service.deleteMany(_ids),
            this.loadGrid.bind(this),
            undefined,
            {
              showToastOnComplete: false,
            }
          );
        }
      });
  }

  private loadGrid(data: BackendResponse): void {
    const msg = ResponseUtil.getMessageValue(data);
    if (msg?.length) {
      this.messageBoxService.success('core.worked', msg);
    }

    this.refreshGrid();
  }
}
