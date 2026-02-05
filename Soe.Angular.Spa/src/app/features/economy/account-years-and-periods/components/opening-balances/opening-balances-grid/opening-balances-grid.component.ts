import { Component, inject, OnInit, signal } from '@angular/core';
import {
  AccountDimSmallDTO,
  AccountDTO,
} from '@shared/components/accounting-rows/models/accounting-rows-model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  SoeEntityState,
  TermGroup,
  TermGroup_AccountStatus,
  TermGroup_AccountType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountDimSmallDTO,
  IAccountDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { StringKeyOfNumberProperty } from '@shared/types';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { addEmptyOption } from '@shared/util/array-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { orderBy } from 'lodash';
import { Observable, of, take, tap } from 'rxjs';
import { EconomyService } from '../../../../services/economy.service';
import {
  AccountYearBalanceFlatDTO,
  AccountYearDTO,
  SaveAccountYearBalanceModel,
} from '../../../models/account-years-and-periods.model';
import { OpeningBalancesForm } from '../../../models/opening-balances-form.model';
import { AccountYearService } from '../../../services/account-year.service';
import { AccountingBalanceService } from '../../../services/accounting-balance.service';
import { SetAccountDialogComponent } from '../set-account-dialog/set-account-dialog.component';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';

export enum FunctionType {
  AddRow = 1,
  Remove = 2,
}
@Component({
  selector: 'soe-opening-balances-grid',
  templateUrl: './opening-balances-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class OpeningBalancesGridComponent
  extends GridBaseDirective<AccountYearBalanceFlatDTO, AccountingBalanceService>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);
  accountYearService = inject(AccountYearService);
  economyService = inject(EconomyService);
  service = inject(AccountingBalanceService);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  dialogService = inject(DialogService);
  private readonly messageboxService = inject(MessageboxService);
  ayService = inject(PersistedAccountingYearService);

  perform = new Perform<any>(this.progressService);

  accountYearsDict: ISmallGenericType[] = [];
  accountTypes: ISmallGenericType[] = [];
  accountYears: AccountYearDTO[] = [];
  accountDims: AccountDimSmallDTO[] = [];

  menuList: MenuButtonItem[] = [];

  //Summery
  debitAmount = 0;
  creditAmount = 0;
  diffAmount = 0;

  userAccountYearId = 0;
  isDeleteDisable = signal(true);
  isReadonly = signal(false);
  previousYearDisabled = signal(false);

  form: OpeningBalancesForm = new OpeningBalancesForm({
    validationHandler: this.validationHandler,
    element: new AccountYearDTO(),
  });
  terms: any;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_Balance,
      'economy.accounting.balance.balance',
      {
        lookups: [
          this.loadAccountYears(),
          this.loadAccounts(),
          this.loadUserSettings(),
          this.loadAccountTypes(),
        ],
      }
    );
  }

  override onFinished(): void {
    this.ayService.loadSelectedAccountYear().subscribe(() => {
      const soeAccountYearId = this.ayService.selectedAccountYearId();
      if (soeAccountYearId)
        this.form.accountYearId.patchValue(soeAccountYearId);
      else if (this.userAccountYearId > 0)
        this.form.accountYearId.patchValue(this.userAccountYearId);

      this.refreshGrid();
    });
  }

  changeSelectedYear() {
    const accYear = this.accountYears.find(
      a => a.accountYearId == this.form.value.accountYearId
    );
    if (accYear) {
      this.previousYearDisabled.set(
        !this.accountYears.find(a => a.from < accYear.from)
      );
    }

    this.isReadonly.set(
      !accYear ||
        accYear.status === TermGroup_AccountStatus.Closed ||
        accYear.status === TermGroup_AccountStatus.Locked
    );

    this.refreshGrid();
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      reloadOption: {
        onAction: () => this.refreshGrid(),
      },
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('transferBalance', {
          iconName: signal('arrow-right'),
          caption: signal('economy.accounting.balance.transferbalance'),
          tooltip: signal('economy.accounting.balance.transferbalance'),
          disabled: this.isReadonly || this.previousYearDisabled,
          onAction: () => this.transferBalanceFromPreviousYear(),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('newRow', {
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          disabled: this.isReadonly || this.previousYearDisabled,
          onAction: () => this.addRow(),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('delete', {
          caption: signal('common.row.remove'),
          tooltip: signal('common.row.remove'),
          disabled: this.isDeleteDisable,
          onAction: () => this.deleteBalance(),
        }),
      ],
    });
  }

  private loadAccountYears(): Observable<AccountYearDTO[]> {
    this.accountYearsDict = [];
    this.accountYears = [];

    return this.accountYearService
      .getGrid(undefined, { getPeriods: false, excludeNew: false })
      .pipe(
        tap(x => {
          this.accountYears = x;

          orderBy(x, 'from', 'desc').forEach(year => {
            year.from = new Date(year.from);
            year.to = new Date(year.to);
            this.accountYearsDict.push({
              id: year.accountYearId,
              name: year.yearFromTo,
            });
          });
        })
      );
  }

  private loadAccounts(): Observable<IAccountDimSmallDTO[]> {
    return this.economyService
      .getAccountDimsSmall(
        false,
        false,
        true,
        true,
        false,
        true,
        false,
        false,
        false
      )
      .pipe(
        tap(dimDicts => {
          this.accountDims = dimDicts;

          let index = 0;
          this.accountDims.forEach(ad => {
            index = index + 1;
            if (!ad.accounts) ad.accounts = [];

            if (ad.accountDimNr === 1)
              ad.accounts = ad.accounts.filter(
                a =>
                  a.accountTypeSysTermId === TermGroup_AccountType.Asset ||
                  a.accountTypeSysTermId === TermGroup_AccountType.Debt
              );

            if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0) {
              addEmptyOption(ad.accounts);
            }

            // Remove empty row from accounts
            if (ad.accounts.length > 1) {
              ad.accounts = ad.accounts.filter(
                element => element.accountId !== 0
              );
            }
          });
        })
      );
  }

  private loadAccountTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.AccountType, false, false)
      .pipe(
        tap(x => {
          this.accountTypes = x;
        })
      );
  }

  loadUserSettings() {
    const settingTypes: number[] = [];

    settingTypes.push(UserSettingType.AccountingAccountYear);

    return this.coreService.getUserSettings(settingTypes).pipe(
      tap(x => {
        this.userAccountYearId = SettingsUtil.getIntUserSetting(
          x,
          UserSettingType.AccountingAccountYear
        );
      })
    );
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: any
  ): Observable<AccountYearBalanceFlatDTO[]> {
    return this.perform.load$(
      this.service.getGrid(this.form.accountYearId.value)
    );
  }

  override onAfterLoadData() {
    this.summarize();
    this.form.markAsPristine();
  }

  onCellValueChanged(row: CellValueChangedEvent) {
    row.data.isModified = true;
    if (row.newValue != row.oldValue && row.newValue > 0) {
      switch (row.colDef.field) {
        case 'debitAmount':
          row.data.creditAmount = 0;
          break;
        case 'creditAmount':
          row.data.debitAmount = 0;
          break;

        default:
          break;
      }
    }
    this.form?.markAsDirty();
    this.form?.markAsTouched();
    this.grid.refreshCells();
    this.summarize();
  }

  protected onAccountingDimChanged(data: any, dimIndex: number) {
    const account = this.findAccount(data, dimIndex);
    data['dim' + dimIndex + 'Id'] = account ? account.accountId : 0;
    data['dim' + dimIndex + 'Name'] = account ? account.name : '';

    if (dimIndex === 1) this.setRowItemAccounts(data, account, true);
  }

  private findAccount(entity: any, dimIndex: number) {
    const nrToFind = entity['dim' + dimIndex + 'Nr'];
    if (!nrToFind) return null;

    const found = this.accountDims[dimIndex - 1].accounts.filter(
      account =>
        account.accountNr === nrToFind &&
        account.state === SoeEntityState.Active
    );
    return found.length ? found[0] : null;
  }

  selectionChanged() {
    this.isDeleteDisable.set(this.grid.getSelectedRows().length == 0);
  }

  override onGridReadyToDefine(grid: GridComponent<AccountYearBalanceFlatDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onSelectionChanged: this.selectionChanged.bind(this),
    });

    this.translate
      .get([
        'common.newrow',
        'common.accountingrows.rownr',
        'common.quantity',
        'common.debit',
        'common.credit',
        'core.deleterow',
        'economy.accounting.accounttype',
        'core.warning',
        'economy.accounting.balance.changeyearerror',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.enableRowSelection();
        this.grid.addColumnModified('isModified', { columnSeparator: true });

        this.grid.addColumnNumber(
          'rowNr',
          terms['common.accountingrows.rownr'],
          {
            maxWidth: 80,
            minWidth: 80,
            pinned: 'left',
            enableHiding: false,
            clearZero: true,
          }
        );

        this.accountDims.forEach((dim, i) => {
          const index = i + 1;
          const idField = `dim${index}Id`;
          const activeAccounts = this.filterAccounts(dim.accounts);

          this.grid.addColumnAutocomplete<IAccountDTO>(
            idField as StringKeyOfNumberProperty<AccountYearBalanceFlatDTO>,
            dim.name,
            {
              optionIdField: 'accountId',
              optionNameField: 'numberName',
              optionDisplayNameField: `dim${index}Nr` as any,
              scrollable: true,
              source: _ => activeAccounts,
              updater: (row, newOption) => {
                const editedRow = row as any;
                editedRow.isModified = true;
                editedRow[`dim${index}Nr`] = newOption?.accountNr;
                editedRow[`dim${index}Name`] = newOption?.name;
                editedRow[`dim${index}Id`] = newOption?.accountId;
                if (index == 1) {
                  const dim1TypeName = this.accountTypes.find(
                    f => f.id === newOption?.accountTypeSysTermId
                  );
                  if (dim1TypeName)
                    editedRow[`dim1TypeName`] = dim1TypeName.name;
                  this.onAccountingDimChanged(editedRow, 1);
                }
                this.form?.markAsDirty();
                this.form?.markAsTouched();
                this.grid.refreshCells();
              },
              suppressFilter: false,
              minWidth: 20,
              limit: 8,
              sortable: false,
              editable: true,
              enableGrouping: true,
              flex: 1,
            }
          );
          if (index === 1)
            this.grid.addColumnText(
              'dim1TypeName',
              this.terms['economy.accounting.accounttype'],
              { enableGrouping: true, enableHiding: true, flex: 1 }
            );
        });

        this.grid.addColumnNumber('quantity', terms['common.quantity'], {
          flex: 1,
          enableHiding: true,
          decimals: 2,
          aggFuncOnGrouping: 'sum',
        });
        this.grid.addColumnNumber('debitAmount', terms['common.debit'], {
          flex: 1,
          enableHiding: false,
          decimals: 2,
          aggFuncOnGrouping: 'sum',
          editable: true,
          enableGrouping: false,
        });
        this.grid.addColumnNumber('creditAmount', terms['common.credit'], {
          flex: 1,
          enableHiding: false,
          decimals: 2,
          aggFuncOnGrouping: 'sum',
          editable: true,
          enableGrouping: false,
        });

        this.grid.useGrouping({
          stickyGroupTotalRow: 'bottom',
          stickyGrandTotalRow: 'bottom',
        });

        super.finalizeInitGrid();
      });
  }

  public filterAccounts(accounts: IAccountDTO[]) {
    return orderBy(accounts, 'accountNr');
  }

  private summarize() {
    this.debitAmount = 0;
    this.creditAmount = 0;
    this.diffAmount = 0;

    // Calculate debit amount
    this.debitAmount = this.rowData
      .getValue()
      .filter(item => !item.isDeleted)
      .reduce((total, item) => total + item.debitAmount, 0);

    // Calculate credit amount
    this.creditAmount = this.rowData
      .getValue()
      .filter(item => !item.isDeleted)
      .reduce((total, item) => total + item.creditAmount, 0);

    const diff = this.debitAmount - this.creditAmount;
    this.diffAmount = diff > -0.001 && diff < 0.001 ? 0 : diff;
  }

  transferBalanceFromPreviousYear() {
    return this.perform.load(
      this.service
        .getBalanceFromPreviousYear(this.form.accountYearId.value)
        .pipe(
          tap(result => {
            if (result.success) {
              const balances: AccountYearBalanceFlatDTO[] = {
                ...this.rowData.getValue(),
              };
              result.value.$values.forEach((i: AccountYearBalanceFlatDTO) => {
                if (!i['dim2Name']) i['dim2Name'] = '';
                if (!i['dim3Name']) i['dim3Name'] = '';
                if (!i['dim4Name']) i['dim4Name'] = '';
                if (!i['dim5Name']) i['dim5Name'] = '';
                if (!i['dim6Name']) i['dim6Name'] = '';
                if (!i['dim2Nr']) i['dim2Nr'] = '';
                if (!i['dim3Nr']) i['dim3Nr'] = '';
                if (!i['dim4Nr']) i['dim4Nr'] = '';
                if (!i['dim5Nr']) i['dim5Nr'] = '';
                if (!i['dim6Nr']) i['dim6Nr'] = '';
                if (!i['quantity']) i['quantity'] = 0;
                i.isModified = true;

                balances.push(i);
              });
              const diffRow = balances.find(r => r.isDiffRow && !r.isDeleted);

              if (diffRow) {
                this.dialogService
                  .open(SetAccountDialogComponent, {
                    size: 'sm',
                    amount:
                      diffRow.creditAmount && diffRow.creditAmount != 0
                        ? diffRow.creditAmount
                        : diffRow.debitAmount,
                    accounts: this.accountDims[0].accounts,
                    title: 'economy.accounting.balance.selectaccount',
                  })
                  .afterClosed()
                  .subscribe(account => {
                    this.setRowItemAccounts(diffRow, account, false);
                    this.rowData.next(balances);
                    this.form.markAsDirty();
                  });
              }
            }
          })
        )
    );
  }

  addRow() {
    const row = new AccountYearBalanceFlatDTO();

    row.dim1Id = 0;
    row.dim2Id = 0;
    row.dim3Id = 0;
    row.dim4Id = 0;
    row.dim5Id = 0;
    row.dim6Id = 0;
    row.dim1Name = '';
    row.dim2Name = '';
    row.dim3Name = '';
    row.dim4Name = '';
    row.dim5Name = '';
    row.dim6Name = '';
    row.dim1Nr = '';
    row.dim2Nr = '';
    row.dim3Nr = '';
    row.dim4Nr = '';
    row.dim5Nr = '';
    row.dim6Nr = '';
    row.debitAmount = 0;
    row.creditAmount = 0;
    row.isModified = true;
    this.rowData.next([row, ...this.rowData.getValue()]);
    this.setFocusedFirstRowStandardAccountCell();
  }

  setFocusedFirstRowStandardAccountCell() {
    setTimeout((): void => {
      this.grid.api.setFocusedCell(0, 'dim1Id');
      this.grid.api.startEditingCell({
        rowIndex: 0,
        colKey: 'dim1Id',
      });
    }, 100);
  }

  deleteBalance() {
    const selectedRows = this.grid.getSelectedRows();
    selectedRows.forEach(r => {
      r.isDeleted = true;
    });
    this.saveBalance();
  }

  private setRowItemAccounts(
    rowItem: any,
    account: AccountDTO | null,
    setInternalAccountFromAccount: boolean,
    internalsFromStdIfMissing: boolean = false
  ) {
    // Set standard account
    rowItem.dim1Id = account != null ? account.accountId : 0;
    rowItem.dim1Nr = account != null ? account.accountNr : '';
    rowItem.dim1Name = account != null ? account.name : '';
    rowItem.dim1Disabled = false;
    rowItem.dim1Mandatory = true;
    rowItem.dim1Stop = true;
    rowItem.quantityStop = account != null ? account.unitStop : false;
    rowItem.unit = account != null ? account.unit : '';
    rowItem.amountStop = account != null ? account.amountStop : 1;
    rowItem.rowTextStop = account != null ? account.rowTextStop : true;
    rowItem.isAccrualAccount =
      account != null ? account.isAccrualAccount : false;

    if (setInternalAccountFromAccount) {
      // Clear internal accounts
      rowItem.dim2Id = 0;
      rowItem.dim2Nr = '';
      rowItem.dim2Name = '';
      rowItem.dim2Disabled = false;
      rowItem.dim2Mandatory = false;
      rowItem.dim2Stop = false;
      rowItem.dim3Id = 0;
      rowItem.dim3Nr = '';
      rowItem.dim3Name = '';
      rowItem.dim3Disabled = false;
      rowItem.dim3Mandatory = false;
      rowItem.dim3Stop = false;
      rowItem.dim4Id = 0;
      rowItem.dim4Nr = '';
      rowItem.dim4Name = '';
      rowItem.dim4Disabled = false;
      rowItem.dim4Mandatory = false;
      rowItem.dim4Stop = false;
      rowItem.dim5Id = 0;
      rowItem.dim5Nr = '';
      rowItem.dim5Name = '';
      rowItem.dim5Disabled = false;
      rowItem.dim5Mandatory = false;
      rowItem.dim5Stop = false;
      rowItem.dim6Id = 0;
      rowItem.dim6Nr = '';
      rowItem.dim6Name = '';
      rowItem.dim6Disabled = false;
      rowItem.dim6Mandatory = false;
      rowItem.dim6Stop = false;

      // Set internal accounts
      if (account != null && account.accountInternals != null) {
        // Get internal accounts from the account
        account.accountInternals.forEach(ai => {
          if (ai.accountDimNr > 1) {
            const index =
              this.accountDims.findIndex(
                ad => ad.accountDimNr === ai.accountDimNr
              ) + 1; //index is 0 based, our dims are 1 based
            rowItem[`dim${index}Id`] = ai.accountId || 0;
            rowItem[`dim${index}Nr`] = ai.accountNr || '';
            rowItem[`dim${index}Name`] = ai.name || '';
            rowItem[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
            rowItem[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
            rowItem[`dim${index}Stop`] = ai.mandatoryLevel === 3;
          }
        });
      }
    } else if (internalsFromStdIfMissing) {
      if (account != null && account.accountInternals != null) {
        // Get internal accounts from the account
        account.accountInternals.forEach(ai => {
          if (ai.accountDimNr > 1) {
            const index =
              this.accountDims.findIndex(
                ad => ad.accountDimNr === ai.accountDimNr
              ) + 1; //index is 0 based, our dims are 1 based
            if (!rowItem[`dim${index}Id`] || ai.mandatoryLevel === 1) {
              rowItem[`dim${index}Id`] = ai.accountId || 0;
              rowItem[`dim${index}Nr`] = ai.accountNr || '';
              rowItem[`dim${index}Name`] = ai.name || '';
              rowItem[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
              rowItem[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
              rowItem[`dim${index}Stop`] = ai.mandatoryLevel === 3;
            }
          }
        });
      }
    } else {
      // Keep internal accounts, just set number and names
      // If not found, keep values from server since it can be an account that has been inactivated but we should
      // always show choosen account dims...
      let index = 1;
      this.accountDims
        .filter(d => d.accountDimNr !== 1)
        .forEach(dim => {
          index = index + 1;
          const account = dim.accounts.find(
            a => a.accountId === rowItem[`dim${index}Id`]
          );
          if (account) {
            rowItem[`dim${index}Nr`] = account.accountNr;
            rowItem[`dim${index}Name`] = account.name;
          } else {
            rowItem[`dim${index}Nr`] = rowItem[`dim${index}Nr`]
              ? rowItem[`dim${index}Nr`]
              : '';
            rowItem[`dim${index}Name`] = rowItem[`dim${index}Name`]
              ? rowItem[`dim${index}Name`]
              : '';
          }
        });
    }
  }

  saveBalance(ignoreAccountValidation = false) {
    const validatedItems: AccountYearBalanceFlatDTO[] = [];
    const model = new SaveAccountYearBalanceModel();
    model.accountYearId = this.form.accountYearId.value;
    let itemsToSave = this.rowData
      .getValue()
      .filter(i => i.isModified || i.isDeleted);

    if (itemsToSave.some(i => !i.dim1Id || i.dim1Id === 0)) {
      if (ignoreAccountValidation) {
        itemsToSave = itemsToSave.filter(i => i.dim1Id && i.dim1Id > 0);
      } else {
        const title = this.translate.instant('core.verifyquestion');
        const text = this.translate.instant(
          'economy.accounting.balance.missingaccount'
        );
        this.messageboxService
          .warning(title, text)
          .afterClosed()
          .subscribe(res => {
            if (res.result) this.saveBalance(true); // Retry saving after user confirmed
          });

        return;
      }
    }

    if (itemsToSave.length === 0) {
      this.grid.setData(
        this.rowData
          .getValue()
          .filter(i => !i.isDeleted && i.dim1Id && i.dim1Id > 0)
      );
      return;
    }

    // Validate doubles
    let clonesExist = false;
    let clonesStr = '';
    const handledRows: number[] = [];
    const rowsToEmpty: {
      item: AccountYearBalanceFlatDTO;
      doubles: AccountYearBalanceFlatDTO[];
    }[] = [];

    itemsToSave.forEach((item: AccountYearBalanceFlatDTO) => {
      if (item.isDeleted) {
        if (item.accountYearBalanceHeadId && item.accountYearBalanceHeadId > 0)
          validatedItems.push(item);
      } else if (!handledRows.includes(item.rowNr)) {
        const clones = this.rowData
          .getValue()
          .filter(
            i =>
              !i.isDeleted &&
              i.dim1Id === item.dim1Id &&
              i.dim2Id === item.dim2Id &&
              i.dim3Id === item.dim3Id &&
              i.dim4Id === item.dim4Id &&
              i.dim5Id === item.dim5Id &&
              i.dim6Id === item.dim6Id
          );
        if (clones.length) {
          // Add first
          let group: AccountYearBalanceFlatDTO[] = [];

          // Add clones
          clones.forEach(c => {
            handledRows.push(c.rowNr);
            group.push(c);
          });

          // Sort
          group = group.sort((a, b) => a.rowNr - b.rowNr);

          if (clonesExist) clonesStr += ', ';

          let first = true;
          group.forEach(c => {
            clonesStr += first ? '(' + c.rowNr : ', ' + c.rowNr;
            first = false;
          });
          clonesStr += ')';
          clonesExist = true;

          rowsToEmpty.push({ item: group[0], doubles: group.slice(1) });
        } else {
          validatedItems.push(item);
        }
        handledRows.push(item.rowNr);
      }
    });

    if (!clonesExist) {
      model.items = itemsToSave;
      this.save(model);
      return;
    }

    const title = this.translate.instant('core.verifyquestion');
    const text =
      this.translate.instant('economy.accounting.balance.willmerge') +
      '\n\n' +
      this.translate.instant('economy.accounting.balance.sameaccounts') +
      '\n' +
      clonesStr;

    this.messageboxService
      .warning(title, text)
      .afterClosed()
      .subscribe(res => {
        if (res.result) {
          // merge rows
          rowsToEmpty.forEach(x => {
            x.doubles.forEach((y: AccountYearBalanceFlatDTO) => {
              x.item.balance += y.balance;
              x.item.debitAmount += y.debitAmount;
              x.item.creditAmount += y.creditAmount;

              if (
                y.accountYearBalanceHeadId &&
                y.accountYearBalanceHeadId > 0
              ) {
                y.isDeleted = true;
                validatedItems.push(y);
              }
            });
            x.item.isModified = true;
            validatedItems.push(x.item);
          });
          model.items = validatedItems;
          this.save(model);
        }
      });
  }

  save(model: SaveAccountYearBalanceModel) {
    return this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(
        tap(() => {
          this.refreshGrid();
        })
      )
    );
  }
}
