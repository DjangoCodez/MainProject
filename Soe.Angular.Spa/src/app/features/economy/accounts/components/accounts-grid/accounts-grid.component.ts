import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeEntityType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { MessagingService } from '@shared/services/messaging.service';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { map, Observable, of, take, tap } from 'rxjs';
import { AccountsService } from '../../services/accounts.service';
import { AccountsGridDTO } from '../../models/accounts.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { EconomyService } from '../../../services/economy.service';
import { IAccountDimDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ProgressOptions, ProgressService } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BatchUpdateDialogData } from '@shared/components/batch-update/models/batch-update-dialog-data.model';
import { BatchUpdateComponent } from '@shared/components/batch-update/components/batch-update/batch-update.component';
import { AccountingCodingLevelsEditComponent } from '@features/economy/accounting-coding-levels/components/accounting-coding-levels-edit/accounting-coding-levels-edit.component';
import { AccountDimForm } from '@features/economy/accounting-coding-levels/models/accounting-coding-levels-form.model';
import { AccountingCodingLevelsService } from '@features/economy/accounting-coding-levels/services/accounting-coding-levels.service';
import { AccountDimDTO } from '@features/economy/accounting-coding-levels/models/accounting-coding-levels.model';
import { UpdateAccountDimStdComponent } from '../update-account-dim-std/update-account-dim-std.component';
import { AccountUrlParamsService } from '../../services/account-params.service';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-accounts-grid',
  templateUrl: 'accounts-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountsGridComponent
  extends GridBaseDirective<AccountsGridDTO, AccountsService>
  implements OnInit
{
  paramService = inject(AccountUrlParamsService);
  service = inject(AccountsService);
  accountingService = inject(AccountingCodingLevelsService);
  coreService = inject(CoreService);
  economyService = inject(EconomyService);
  progressService = inject(ProgressService);
  dialogService = inject(DialogService);
  messagingService = inject(MessagingService);
  messageboxService = inject(MessageboxService);
  ayService = inject(PersistedAccountingYearService);
  performAction = new Perform<BackendResponse>(this.progressService);

  typeFilterOptions: SmallGenericType[] = [];
  vatTypeFilterOptions: SmallGenericType[] = [];
  accountStdTypes: SmallGenericType[] = [];
  accountDim!: IAccountDimDTO;
  hasBatchUpdatePermission = signal(false);

  private toolbarSaveDisabled = signal(true);
  private toolbarBatchUpdateDisabled = signal(true);
  private toolbarBatchUpdateHidden = signal(false);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Accounting_AccountRoles,
      'economy.accounting.accounts',
      {
        lookups: [
          this.loadAccountTypes(),
          this.loadSysVatAccounts(),
          this.loadAccountDim(),
        ],
        additionalModifyPermissions: [
          Feature.Economy_Accounting_Accounts_BatchUpdate,
          Feature.Economy_Import_Sie_Account,
        ],
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      saveOption: {
        disabled: this.toolbarSaveDisabled,
        onAction: () => this.saveStatus(),
      },
    });

    if (this.paramService.isAccountStd && this.paramService.accountDimId) {
      if (this.hasBatchUpdatePermission()) {
        this.toolbarService.createItemGroup({
          items: [
            this.toolbarService.createToolbarButton('batch-update', {
              iconName: signal('pen'),
              tooltip: signal('common.batchupdate.title'),
              caption: signal('common.batchupdate.title'),
              disabled: this.toolbarBatchUpdateDisabled,
              hidden: this.toolbarBatchUpdateHidden,
              onAction: () => this.openBatchUpdate(),
            }),
          ],
        });
      }

      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarButton('list-alt', {
            iconName: signal('list-alt'),
            tooltip: signal('economy.accounting.accountdimstd'),
            caption: signal('economy.accounting.accountdimstd'),
            onAction: () => this.onEventOpenAccountDim(),
          }),
          this.toolbarService.createToolbarButton('file-import', {
            iconName: signal('file-import'),
            tooltip: signal('economy.accounting.importaccountsysstdtype'),
            caption: signal('economy.accounting.importaccountsysstdtype'),
            onAction: () => this.onEventChangeAccountStd(),
          }),
        ],
      });
    }
  }

  override selectionChanged(data: AccountsGridDTO[]): void {
    const disabled = data.length === 0;
    this.toolbarSaveDisabled.set(disabled);
    this.toolbarBatchUpdateDisabled.set(disabled);
  }

  saveStatus(options?: ProgressOptions, skipStateValidation: boolean = false) {
    options = options || {};
    options.showDialogOnError = false;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .updateAccountsState({
          dict: this.grid.selectedItemsService.toDict().dict,
          skipStateValidation: skipStateValidation,
        })
        .pipe(
          tap((result: any) => {
            if (!result.success) {
              if (result.canUserOverride) {
                const dialog = this.messageboxService.question(
                  this.translate.instant(
                    'time.employee.employee.delete.action.inactivate'
                  ),
                  result.errorMessage
                );
                dialog
                  .afterClosed()
                  .subscribe((response: IMessageboxComponentResponse) => {
                    if (response.result) {
                      this.saveStatus({}, true);
                    }
                  });
              } else {
                this.messageboxService.error(
                  'time.employee.employee.delete.action.inactivate',
                  result.errorMessage,
                  {
                    type: 'forbidden',
                  }
                );
              }
            }
          })
        ),
      this.updateStatesAndEmitChange.bind(this),
      undefined,
      options
    );
  }

  onEventOpenAccountDim(): void {
    if (!this.accountDim) return;
    this.rowEdited.set({
      gridIndex: this.gridIndex(),
      rows: [],
      row: <any>this.accountDim,
      filteredRows: [],
      additionalProps: {
        editComponent: AccountingCodingLevelsEditComponent,
        FormClass: AccountDimForm,
        editTabLabel: ' ',
      },
    });
  }

  onEventChangeAccountStd(): void {
    this.dialogService
      .open(UpdateAccountDimStdComponent, {
        title: 'economy.accounting.importaccountsysstdtype',
        size: 'md',
      })
      .afterClosed()
      .subscribe(res => {
        if (res) {
          this.refreshGrid();
        }
      });
  }

  openBatchUpdate(): void {
    const dialogOpts = <Partial<BatchUpdateDialogData>>{
      title: 'common.batchupdate.title',
      size: 'lg',
      disableClose: true,
      entityType: SoeEntityType.Account,
      selectedIds: this.grid?.getSelectedIds('accountId'),
    };
    this.dialogService
      .open(BatchUpdateComponent, dialogOpts)
      .afterClosed()
      .subscribe(res => {
        if (res) {
          this.refreshGrid();
        }
      });
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();

    this.hasBatchUpdatePermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Accounting_Accounts_BatchUpdate
      )
    );

    this.toolbarBatchUpdateHidden.set(
      !this.flowHandler.hasModifyAccess(Feature.Economy_Import_Sie_Account)
    );
  }

  updateStatesAndEmitChange = (backendResponse: BackendResponse) => {
    if (backendResponse.success) {
      this.grid.selectedItemsService.items = {};
      this.refreshGrid();
    }
  };

  override onGridReadyToDefine(grid: GridComponent<AccountsGridDTO>): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.active',
        'common.number',
        'common.name',
        'core.edit',
        'economy.accounting.account.externalcode',
        'common.categories',
        'economy.accounting.account.parentcode',

        'economy.accounting.accounttype',
        'economy.accounting.account.sysvataccount',
        'economy.accounting.accountbalance',
        'economy.accounting.account.islinkedtoshifttype',
        'economy.accounting.account.parentaccount',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.setRowSelection('multiRow');
        this.grid.addColumnActive('isActive', terms['common.active'], {
          editable: true,
          idField: 'accountId',
          minWidth: 65,
          maxWidth: 65,
        });

        this.grid.addColumnText('accountNr', terms['common.number'], {
          width: 100,
        });
        this.grid.addColumnText('name', terms['common.name']);
        if (this.paramService.isAccountStd) {
          this.grid.addColumnSelect(
            'accountTypeSysTermId',
            terms['economy.accounting.accounttype'],
            this.typeFilterOptions,
            undefined,
            {
              flex: 1,
            }
          );
          this.grid.addColumnSelect(
            'sysVatAccountId',
            terms['economy.accounting.account.sysvataccount'],
            this.vatTypeFilterOptions,
            undefined,
            {
              flex: 1,
            }
          );
          this.grid.addColumnNumber(
            'balance',
            terms['economy.accounting.accountbalance'],
            { flex: 1, decimals: 2 }
          );
        } else {
          this.grid.addColumnText(
            'externalCode',
            terms['economy.accounting.account.externalcode'],
            { flex: 1, enableHiding: true }
          );
          this.grid.addColumnText('categories', terms['common.categories'], {
            flex: 1,
            enableHiding: true,
          });
          this.grid.addColumnText(
            'parentAccountName',
            terms['economy.accounting.account.parentaccount'],
            { flex: 1, enableHiding: true }
          );
          this.grid.addColumnIcon('isLinkedToShiftType', ' ', {
            iconName: 'link',
            showIcon: row => row.isLinkedToShiftType,
            tooltip: 'economy.accounting.account.islinkedtoshifttype',
          });
        }
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => this.edit(row),
        });

        super.finalizeInitGrid();
      });
  }

  loadAccountTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AccountType, false, true)
      .pipe(
        take(1),
        tap((x: SmallGenericType[]) => {
          this.typeFilterOptions = x;
        })
      );
  }

  loadSysVatAccounts() {
    return this.economyService
      .getSysVatAccounts(SoeConfigUtil.sysCountryId, false)
      .pipe(
        map((x: SmallGenericType[]) => {
          this.vatTypeFilterOptions = x;
        })
      );
  }

  loadAccountDim(): Observable<AccountDimDTO> {
    return this.accountingService.get(this.paramService.accountDimId).pipe(
      take(1),
      tap(x => {
        this.accountDim = x;
      })
    );
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      accountDimId: number;
      accountYearId: number;
      setLinkedToShiftType: boolean;
      getCategories: boolean;
      setParent: boolean;
      isUseCache: boolean;
      ignoreHierarchyOnly: boolean;
    }
  ): Observable<AccountsGridDTO[]> {
    const isStdAccount: boolean = this.paramService.isAccountStd;
    const getCategories: boolean = !isStdAccount;
    const setParent: boolean = !isStdAccount;

    return this.ayService.ensureAccountYearIsLoaded$(() =>
      super
        .loadData(id, {
          accountDimId: this.paramService.accountDimId,
          accountYearId: this.ayService.selectedAccountYearId(),
          setLinkedToShiftType: false,
          getCategories: getCategories,
          setParent: setParent,
          isUseCache: false,
          ignoreHierarchyOnly: false,
        })
        .pipe(
          tap((data: AccountsGridDTO[]) => {
            data.forEach(x => {
              const hasAccountType = this.typeFilterOptions.some(
                y => y.id === x.accountTypeSysTermId
              );
              const hasVatType = this.vatTypeFilterOptions.some(
                y => y.id === x.sysVatAccountId
              );
              if (!hasAccountType || !x.accountTypeSysTermId) {
                x.accountTypeSysTermId = undefined;
              }

              if (!hasVatType || !x.sysVatAccountId) {
                x.sysVatAccountId = undefined;
              }
            });
          })
        )
    );
  }
}
