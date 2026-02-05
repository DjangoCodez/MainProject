import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { AccountDistributionService } from '@features/economy/account-distribution/services/account-distribution.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { IAccountingRowDTO } from '@shared/models/generated-interfaces/AccountingRowDTO';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { MessagingService } from '@shared/services/messaging.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { AccountingRowsContainers } from '@shared/util/Enumerations';
import {
  AccountDimSmallDTO,
  AccountDTO,
  AccountingRowDTO,
  AmountStop,
  SteppingRules,
} from '../models/accounting-rows-model';
import {
  AccountingRowType,
  CompanySettingType,
  Feature,
  SoeEntityState,
  SupplierInvoiceAccountRowAttestStatus,
  TermGroup_AccountDistributionTriggerType,
  TermGroup_CurrencyType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  BehaviorSubject,
  forkJoin,
  map,
  Observable,
  of,
  take,
  tap,
} from 'rxjs';
import { Constants } from '@shared/util/client-constants';
import { Guid } from '@shared/util/string-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { IDecimalKeyValue } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountDTO,
  IAccountDimSmallDTO,
  IAccountDistributionHeadDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

import { CurrencyService } from '@shared/services/currency.service';
import { DistributionHelperService } from '@shared/services/distribution-helper.service';
import { StringKeyOfNumberProperty } from '@shared/types';
import { AggregationType, ISoeAggregationResult } from '@ui/grid/interfaces';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { AccountingRowsService } from '../services/accounting-rows.service';
import {
  CellPosition,
  CellValueChangedEvent,
  Column,
  IRowNode,
  TabToNextCellParams,
} from 'ag-grid-community';
import { VoucherSearchDialogComponent } from '@features/economy/voucher-search/components/voucher-search-dialog/voucher-search-dialog.component';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';
import { AddAccountDialogComponent } from '@shared/components/add-account-dialog/components/add-account-dialog/add-account-dialog.component';
import {
  AccountEditDTO,
  AddAccountDialogResultData,
  AddAccountDialogResultType,
  SaveAccountSmallModel,
} from '@shared/components/add-account-dialog/models/add-account.model';
import { ISysAccountStdDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { AccountingService } from '@features/economy/services/accounting.service';
import { TwoValueCellRenderer } from '@ui/grid/cell-renderers/two-value-cell-renderer/two-value-cell-renderer.component';
import { AccountingRowHelperService } from '../services/accounting-row-helper.service';

@Component({
  selector: 'soe-accounting-rows',
  templateUrl: './accounting-rows.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountingRowsComponent
  extends GridBaseDirective<AccountingRowDTO>
  implements OnInit, OnChanges
{
  @Input() container: AccountingRowsContainers =
    AccountingRowsContainers.Voucher;
  @Input() currencyService!: CurrencyService;
  @Input() addRowWithSetAccountDimFocus = signal(false);

  form = input<SoeFormGroup | undefined>();
  rowsIn = input<AccountingRowDTO[]>([]);
  //voucherSeriesId = input(0);
  defaultAttestRowDebitAccountId = input(0);

  //lockVoucherSeries = input(false);
  //showSortButtons = input(false);
  //showInstructions = input(false);
  //allowZeroAmount = input(false);
  //minRowsToShow = input(0);
  //showVoucherSeries = input(false);
  //voucherDate?: Date;

  isReadOnly = input(false);
  hideStdDim = input(false);
  useNoAccount = input(false);
  showAccrualColumns = input(false);
  showRegenerateButton = input(false);
  showReloadAccountsButton = input(false);
  showRowNr = input(false);
  showTextValue = input(false);
  showQuantity = input(false);
  oneColumnAmountValue = input(false);
  showTransactionCurrency = input(false);
  showEnterpriseCurrency = input(false);
  showLedgerCurrency = input(false);
  showAttestUser = input(false);
  showBalance = input(false);
  oneColumnAmount = input(false);
  showGrouping = input(false);
  overrideDiffValidation = input(false);
  clearInternalAccounts = input(true);
  showAmountSummary = input(false);
  numberOfDecimals = input(2);
  hideFilter = input(false);
  parentGuid = input<string>('');
  private decimals: number = 2;

  actorId = input(0);
  defaultAttestRowAmount = input(0);

  @Output() registerControl: EventEmitter<any> = new EventEmitter<any>();
  @Output() amountConverted: EventEmitter<any[]> = new EventEmitter<any>();
  @Output() editCompletedForBalancedAccount: EventEmitter<any> =
    new EventEmitter<any>();
  @Output() updateAllAccountRowDimAccounts: EventEmitter<any> =
    new EventEmitter<any>();
  regenerateCoding = output();
  accountingRowsChanged = output<AccountingRowDTO[]>();
  openVoucher = output<number>();
  hasDebitCreditBalanceError = output<boolean>();
  accountingRowsReady = output<Guid>();

  private steppingRules!: SteppingRules;

  accountDistributionHelper!: DistributionHelperService;
  controllIsReady = false;
  delayedSetRowItemAccountsOnAllRows = false;
  delayedSetRowItemAccountsOnAllRowsIfMissing = false;
  accountDims: AccountDimSmallDTO[] = [];
  accountBalances: IDecimalKeyValue[] = [];
  inventoryTriggerAccounts: any[] = [];
  debugMode = false;
  internalIdCounter = 1;
  pendingAutobalance = false;

  // Grid wrapper data
  gridRowsSubject = new BehaviorSubject<AccountingRowDTO[]>([]);

  // Converted init parameters
  private showQuantityValue: boolean = false;

  // Company settings
  private useAutomaticAccountDistribution: boolean = false;
  private allowUnbalancedRows: boolean = false;
  private useDimsInRegistration: boolean = false;

  // Grouping
  private groupColumn: any;

  handler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  accountingRowsService = inject(AccountingRowsService);
  readonly coreService = inject(CoreService);
  messageboxService = inject(MessageboxService);
  accountDistributionService = inject(AccountDistributionService);
  dialogService = inject(DialogService);
  messagingService = inject(MessagingService);
  ayService = inject(PersistedAccountingYearService);
  toasterService = inject(ToasterService);
  accountingService = inject(AccountingService);
  accountingRowHelperService = inject(AccountingRowHelperService);

  readonly performLoad = new Perform<any[]>(this.progressService);
  terms: any;
  private isInitialized = false;

  // allowNavigateFrom = (
  //   value: number,
  //   data: AccountingRowDTO,
  //   colIndex: number
  // ) => {
  //   return this.allowNavigationFromAutocomplete(value, data, colIndex);
  // };

  constructor() {
    super();

    //TODO: This in combination with ngOnChanges results in redundant call to setRowItemAccountsOnAllRows.
    effect(() => {
      this.setRowItemAccountsOnAllRows(this.rowsIn());
    });
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.None, 'Common.Directives.AccountingRows', {
      skipInitialLoad: true,
      lookups: [this.loadAccounts(false), this.loadAccountBalances()],
    });

    this.decimals = Math.round(this.numberOfDecimals());
    this.calculateAccountBalances(true);
    this.syncDistributionRows();
    this.setupSteppingRules();
    this.setupMessageListners();
    this.isInitialized = true;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isReadOnly) {
      if (!this.isReadOnly()) {
        if (!this.accountDistributionHelper)
          this.loadAccountDistributions(true);

        this.setRowItemAccountsOnAllRows(undefined, false);
      }

      this.getGridRows().forEach(row => {
        // Use built in read only functionality in soeGridOptions
        row.isReadOnly = this.isReadOnly() || row.isAttestReadOnly;
      });
    }

    if (this.isInitialized) {
      this.setRowItemAccountsOnAllRows();
    }
  }

  private getGridRows(): AccountingRowDTO[] {
    return this.gridRowsSubject.getValue() as AccountingRowDTO[];
  }

  protected onTabToNextCell(params: TabToNextCellParams): CellPosition | false {
    const previous = params.previousCellPosition;
    const next = params.nextCellPosition;
    if (!previous || !next) return false;

    const rowIndex = previous.rowIndex;
    const rowNode = params.api.getDisplayedRowAtIndex(rowIndex);
    const row = rowNode?.data as AccountingRowDTO;
    if (!rowNode || !row) return false;

    // Step 1: Handle balanced account completion if moving from dim1 column
    this.handleBalancedAccountCompletion(previous, rowIndex, row);

    // Step 2: Attempt to find the next editable column based on stepping rules
    const steppedColumn = this.findNextEditableColumn(params, row, rowNode);
    if (steppedColumn) {
      this.startEditingAfterDelay(rowIndex, steppedColumn.getColId());
      return { rowIndex, column: steppedColumn } as CellPosition;
    }

    // Step 3: Perform auto-balance & account distribution logic
    this.handleAutoBalanceAndDistribution(row, previous);

    // Step 4: Move focus to the next or new row
    const nextRowIndex = this.navigateToNextRow(params, rowNode);
    if (params.backwards && nextRowIndex < 0) return false; // Safety check
    const targetCol = params.backwards
      ? this.getCreditAmountColumn()
      : this.getDim1Column();

    this.startEditingAfterDelay(nextRowIndex, targetCol?.getColId());
    return { rowIndex: nextRowIndex, column: targetCol } as CellPosition;
  }

  private handleBalancedAccountCompletion(
    previous: CellPosition,
    rowIndex: number,
    row: AccountingRowDTO
  ): void {
    const prevField = previous.column.getColDef().field;
    if (rowIndex > 0 && prevField === this.getDim1Column()?.getColId()) {
      this.tryCompleteEditOnBalancedAccount(row);
    }
  }

  private findNextEditableColumn(
    params: TabToNextCellParams,
    row: AccountingRowDTO,
    rowNode: IRowNode
  ): Column | undefined | null {
    let column = params.nextCellPosition?.column;
    const getNextCol = (col: Column) => {
      const next = params.backwards
        ? params.api.getDisplayedColBefore(col)
        : params.api.getDisplayedColAfter(col);
      return next ?? undefined;
    };
    while (column && this.steppingRules) {
      if (column.isCellEditable(rowNode)) {
        const rule =
          this.steppingRules[
            column.getColId() as StringKeyOfNumberProperty<SteppingRules>
          ];
        const shouldStop = rule ? rule.call(this, row) : false;

        if (shouldStop) return column;
      }
      column = getNextCol(column);
    }
    return undefined;
  }

  private handleAutoBalanceAndDistribution(
    row: AccountingRowDTO,
    previous: CellPosition
  ): void {
    const field = previous.column.getColDef()?.field ?? '';
    this.pendingAutobalance = true;

    if (field) {
      setTimeout(() => this.handleCheckForAutoBalance(row, field), 0);
    }

    if (row.isModified) {
      setTimeout(() => {
        const handled = this.checkAccountDistribution(row);
        if (handled) {
          this.grid.api.clearFocusedCell();
        }
      }, 0);
    }
  }

  private navigateToNextRow(
    params: TabToNextCellParams,
    rowNode: IRowNode
  ): number {
    const siblings = rowNode.parent?.childrenAfterSort ?? [];
    const childIndex = siblings.findIndex(r => r === rowNode) ?? 0;
    let nextRowIndex = params.backwards ? childIndex - 1 : childIndex + 1;
    if (nextRowIndex >= 0) {
      const nextNode = params.api.getDisplayedRowAtIndex(nextRowIndex);
      if (!nextNode) {
        const added = this.addRow();
        nextRowIndex = added.rowIndex;
      }
    }
    return nextRowIndex;
  }

  private checkInventoryAccounts(row: AccountingRowDTO) {
    const inventoryId: number = row.inventoryId ? row.inventoryId : 0;
    const invAccount = this.inventoryTriggerAccounts.find(
      i => i.key == row.dim1Id && inventoryId == 0
    );
    //if (invAccount) this.openInventoryDialog(row, invAccount.value);
  }

  private loadInventoryTriggerAccounts() {
    return this.accountingRowsService.getInventoryTriggerAccounts().pipe(
      tap(x => {
        this.inventoryTriggerAccounts = x;
      })
    );
  }

  private setupMessageListners() {
    this.messagingService
      .onEvent(Constants.EVENT_VOUCHER_DATE_CHANGED)
      .subscribe((data: any) => {
        if (
          data &&
          data.data &&
          data.container &&
          data.container === this.container
        ) {
          this.calculateRowCurrencyAmounts(
            data.data as AccountingRowDTO,
            TermGroup_CurrencyType.TransactionCurrency,
            TermGroup_CurrencyType.BaseCurrency
          );
          setTimeout(() => {
            this.checkAccountDistribution(data.data as AccountingRowDTO);
          });
        }
      });
  }

  private setupSteppingRules(): void {
    this.steppingRules = {
      dim1Id: row =>
        row.dim1Stop || row.dim1Mandatory || this.useDimsInRegistration,
      dim2Id: row =>
        row.dim2Stop ||
        row.dim2Mandatory ||
        (this.useDimsInRegistration && !row.dim2Disabled),
      dim3Id: row =>
        row.dim3Stop ||
        row.dim3Mandatory ||
        (this.useDimsInRegistration && !row.dim3Disabled),
      dim4Id: row =>
        row.dim4Stop ||
        row.dim4Mandatory ||
        (this.useDimsInRegistration && !row.dim4Disabled),
      dim5Id: row =>
        row.dim5Stop ||
        row.dim5Mandatory ||
        (this.useDimsInRegistration && !row.dim5Disabled),
      dim6Id: row =>
        row.dim6Stop ||
        row.dim6Mandatory ||
        (this.useDimsInRegistration && !row.dim6Disabled),
      text: row => !!row.rowTextStop,
      quantity: row => !!row.quantityStop,
      unit: row => !!row.quantityStop,
      debitAmount: row => row.amountStop === +AmountStop.DebitAmountStop,
      creditAmount: row =>
        row.amountStop === +AmountStop.CreditAmountStop || !row.debitAmount,
    };
  }

  private tryCompleteEditOnBalancedAccount(row: AccountingRowDTO) {
    if (row.dim1Id || this.getCurrentDiff() != 0) {
      return;
    }

    this.editCompletedForBalancedAccount.emit();
    setTimeout(() => {
      this.clearFocusedCell();
    }, 0);
  }

  private getDim1Column() {
    return this.grid.api.getColumn('dim1Id');
  }

  private getCreditAmountColumn() {
    return this.grid.api.getColumn('creditAmount');
  }

  private onCellValueChanged($event: CellValueChangedEvent) {
    const field: string = $event.colDef.field ?? '';
    const arrCopy = [...this.getGridRows()];
    const rowData = new AccountingRowDTO($event.data);

    switch ($event.colDef.field) {
      case 'dim1Id':
        this.setRowItemAccountsOnRow(rowData, true);
        break;
      case 'dim2Id':
        this.setInternalAccountOnRow(rowData, 2);
        break;
      case 'dim3Id':
        this.setInternalAccountOnRow(rowData, 3);
        break;
      case 'dim4Id':
        this.setInternalAccountOnRow(rowData, 4);
        break;
      case 'dim5Id':
        this.setInternalAccountOnRow(rowData, 5);
        break;
      case 'dim6Id':
        this.setInternalAccountOnRow(rowData, 6);
        break;
      default:
        // Do nothing
        break;
    }
    rowData.isModified = true;
    this.adjustNegativeDebitCredit(field, rowData);
    this.setDebitCreditAmounts(rowData, field, $event.value);
    const updatedRow = new AccountingRowDTO(rowData);
    arrCopy[$event.rowIndex!] = updatedRow;
    this.setGridRows(arrCopy);
    this.setFormDirty();
  }

  private setFormDirty() {
    if (this.form()) {
      this.form()?.markAsDirty();
      this.form()?.markAsTouched();
    }
  }

  private adjustNegativeDebitCredit(field: string, rowItem: AccountingRowDTO) {
    if (field !== 'creditAmount' && field !== 'debitAmount') {
      return; // invalid field, bail out
    }

    type AmountField = 'creditAmount' | 'debitAmount';
    const typedField = field as AmountField;
    const oppositeField: AmountField =
      typedField === 'creditAmount' ? 'debitAmount' : 'creditAmount';

    const value = rowItem[typedField];
    if (!value || value >= 0) return; // invalid amount, bail out
    rowItem[oppositeField] = Math.abs(Number(value));
    rowItem[typedField] = 0;
    const messageKey =
      typedField === 'creditAmount'
        ? 'common.accountingrows.negetivecreditvalue.interchange.message'
        : 'common.accountingrows.negetivedebitvalue.interchange.message';
    this.toasterService.info(this.translate.instant(messageKey));
  }

  private setDebitCreditAmounts(
    rowData: AccountingRowDTO,
    field: string,
    value: number
  ) {
    let sourceCurrencyType: TermGroup_CurrencyType =
      TermGroup_CurrencyType.BaseCurrency;
    let calculateCurrencyAmounts: boolean = false;
    let amountChanged: boolean = false;

    if (
      field.startsWithCaseInsensitive('debit') ||
      field.startsWithCaseInsensitive('credit') ||
      field === 'amount' ||
      field === 'amountCurrency'
    ) {
      if (!this.oneColumnAmountValue) {
        // Clear opposite amount
        if (field === 'debitAmount' && rowData.debitAmount !== 0) {
          rowData.creditAmount = 0;
        } else if (field === 'creditAmount' && rowData.creditAmount !== 0) {
          rowData.debitAmount = 0;
        }
      }

      // Get currency type
      if (field === 'amount' || field.endsWithCaseInsensitive('Amount'))
        sourceCurrencyType = TermGroup_CurrencyType.BaseCurrency;
      else if (
        field === 'amountCurrency' &&
        field.endsWithCaseInsensitive('AmountCurrency')
      )
        sourceCurrencyType = TermGroup_CurrencyType.TransactionCurrency;
      else if (field.endsWithCaseInsensitive('AmountEntCurrency'))
        sourceCurrencyType = TermGroup_CurrencyType.EnterpriseCurrency;
      else if (field.endsWithCaseInsensitive('AmountLedgerCurrency'))
        sourceCurrencyType = TermGroup_CurrencyType.LedgerCurrency;

      const isCreditEdited: boolean = field.startsWithCaseInsensitive('credit');
      const hasAmount: boolean = value !== 0;

      rowData.isCreditRow = isCreditEdited && hasAmount;
      rowData.isDebitRow = !rowData.isCreditRow;

      calculateCurrencyAmounts = true;
      amountChanged = true;
      this.gridRowChanged(
        rowData,
        sourceCurrencyType,
        calculateCurrencyAmounts
      );
    }
    if (amountChanged && this.pendingAutobalance) {
      //handleCheckForAutoBalance
      this.pendingAutobalance = false;
      this.handleCheckForAutoBalance(rowData, field);
    }
  }

  private clearFocusedCell() {
    if (this.grid) {
      this.grid.clearFocusedCell();
    }
  }

  public applyGridChanges() {
    this.grid.applyChanges();
    this.updateParent();
  }

  private handleCheckForAutoBalance(entity: AccountingRowDTO, field: string) {
    const autoBalancedColField = this.checkForAndExecuteAutoBalancing(
      entity,
      field
    );
    if (autoBalancedColField) {
      this.pendingAutobalance = false;
      this.gridRowChanged(entity, TermGroup_CurrencyType.BaseCurrency, true);
    }
  }

  private checkForAndExecuteAutoBalancing(
    row: AccountingRowDTO,
    field: string
  ): boolean {
    if (
      row.creditAmount !== undefined &&
      !row.creditAmount &&
      !row.debitAmount
    ) {
      const diff = this.getCurrentDiff();

      if (diff === 0) return false;

      if (diff > 0) {
        row.debitAmount = diff;
        row.isDebitRow = true;
        row.isCreditRow = false;
        row.amountCurrency = Math.abs(diff);
      } else if (diff < 0) {
        row.creditAmount = Math.abs(diff);
        row.isDebitRow = false;
        row.isCreditRow = true;
        row.amountCurrency = diff;
      }
      return true;
    }
    return false;
  }

  private getCurrentDiff(): any {
    const sums = this.getGridRows().reduce(
      (aggr: any, curr: AccountingRowDTO) => {
        aggr.credit += curr.creditAmount || 0;
        aggr.debit += curr.debitAmount || 0;
        return aggr;
      },
      { credit: 0, debit: 0 }
    );

    const diff = sums.credit - sums.debit;
    return parseFloat(diff.toFixed(2));
  }

  protected gridRowChanged(
    row: AccountingRowDTO,
    sourceCurrencyType: TermGroup_CurrencyType,
    calculateCurrencyAmounts: boolean
  ) {
    this.validateAccountingRow(row);
    this.calculateAccountBalances();

    if (calculateCurrencyAmounts) {
      this.calculateRowAllCurrencyAmounts(row, sourceCurrencyType);
    }
    this.runRowValidation();
    this.grid.refreshCells();
    this.setFormDirty();
  }
  private validateAccountingRow(row: AccountingRowDTO) {
    this.accountDims.forEach((_, i) => {
      const index = i + 1;
      const val = (row as any)[`dim${index}Nr`];
      const mandatory = (row as any)[`dim${index}Mandatory`];
      const name = (row as any)[`dim${index}Name`];

      if (!val && mandatory) {
        (row as any)[`dim${index}Error`] =
          this.terms['common.accountingrows.missingaccount'];
      } else if (val && !name) {
        (row as any)[`dim${index}Error`] =
          this.terms['common.accountingrows.invalidaccount'];
      } else {
        (row as any)[`dim${index}Error`] = null;
      }
    });
  }

  runRowValidation() {
    if (this.gridIsDefined) {
      const aggrigationError = this.validateDebitCreditAmounts();
      this.grid.setAggregationsErrorRow([
        aggrigationError as ISoeAggregationResult<AccountingRowDTO>,
      ]);
      if (aggrigationError) {
        this.hasDebitCreditBalanceError.emit(true);
      } else {
        this.hasDebitCreditBalanceError.emit(false);
      }
    }
  }

  validateDebitCreditAmounts() {
    const rows = this.getGridRows();
    if (rows.length === 0) return undefined;
    const { totalDebit, totalCredit } = rows.reduce(
      (totals, row) => {
        totals.totalDebit += row.debitAmount ?? 0;
        totals.totalCredit += row.creditAmount ?? 0;
        return totals;
      },
      { totalDebit: 0, totalCredit: 0 }
    );

    const diff = totalDebit - totalCredit;

    if (diff === 0) return undefined;

    return diff > 0 ? { creditAmount: diff } : { debitAmount: -diff };
  }

  private setInternalAccountOnRow(row: AccountingRowDTO, dim: number) {
    if (dim < 2 || dim > 6) return;

    const rowIdKey = `dim${dim}Id` as keyof AccountingRowDTO;
    const acccount = this.accountDims[dim - 1].accounts.find(
      f => f.accountId === row[rowIdKey]
    );

    const rowNrKey = `dim${dim}Nr` as keyof AccountingRowDTO;
    const rowNameKey = `dim${dim}Name` as keyof AccountingRowDTO;
    (row as any)[rowNrKey] = acccount?.accountNr ?? '0';
    (row as any)[rowNameKey] = acccount?.name ?? '';
  }

  private setGridRows(
    data: AccountingRowDTO[] = this.getGridRows(),
    clearFocus = true
  ) {
    this.gridRowsSubject.next(
      data
        .filter(
          r =>
            !r.isDeleted ||
            this.container == AccountingRowsContainers.SupplierInvoiceAttest ||
            this.debugMode
        )
        //.sort((a, b) => a.rowNr - b.tempRowId)
        .sort((a, b) => a.rowNr - b.rowNr)
    );
    this.runRowValidation();
    this.updateParent();
    if (clearFocus) this.clearFocusedCell();
  }

  override loadCompanySettings() {
    const settingTypes: number[] = [];
    // Common settings
    settingTypes.push(CompanySettingType.AccountingUseDimsInRegistration);

    // Container specific settings
    switch (this.container.toString()) {
      case AccountingRowsContainers.Voucher.toString(): {
        settingTypes.push(CompanySettingType.AccountingUseQuantityInVoucher);
        settingTypes.push(CompanySettingType.AccountingAllowUnbalancedVoucher);
        settingTypes.push(
          CompanySettingType.AccountingAutomaticAccountDistribution
        );
        break;
      }
      case AccountingRowsContainers.SupplierInvoice.toString(): {
        settingTypes.push(
          CompanySettingType.SupplierInvoiceAutomaticAccountDistribution
        );
        settingTypes.push(
          CompanySettingType.SupplierInvoiceUseQuantityInAccountingRows
        );
        break;
      }
      case AccountingRowsContainers.CustomerInvoice.toString(): {
        settingTypes.push(
          CompanySettingType.CustomerInvoiceAutomaticAccountDistribution
        );
        break;
      }
    }
    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(setting => {
        // Common settings
        this.useDimsInRegistration = SettingsUtil.getBoolCompanySetting(
          setting,
          CompanySettingType.AccountingUseDimsInRegistration
        );
        // Container specific settings
        switch (this.container.toString()) {
          case AccountingRowsContainers.Voucher.toString(): {
            this.showQuantityValue = SettingsUtil.getBoolCompanySetting(
              setting,
              CompanySettingType.AccountingUseQuantityInVoucher
            );
            this.allowUnbalancedRows = SettingsUtil.getBoolCompanySetting(
              setting,
              CompanySettingType.AccountingAllowUnbalancedVoucher
            );
            this.useAutomaticAccountDistribution =
              SettingsUtil.getBoolCompanySetting(
                setting,
                CompanySettingType.AccountingAutomaticAccountDistribution
              );
            break;
          }
          case AccountingRowsContainers.SupplierInvoice.toString(): {
            this.showQuantityValue = SettingsUtil.getBoolCompanySetting(
              setting,
              CompanySettingType.SupplierInvoiceUseQuantityInAccountingRows
            );
            this.useAutomaticAccountDistribution =
              SettingsUtil.getBoolCompanySetting(
                setting,
                CompanySettingType.SupplierInvoiceAutomaticAccountDistribution
              );
            break;
          }
          case AccountingRowsContainers.CustomerInvoice.toString(): {
            this.useAutomaticAccountDistribution =
              SettingsUtil.getBoolCompanySetting(
                setting,
                CompanySettingType.CustomerInvoiceAutomaticAccountDistribution
              );
            if (this.overrideDiffValidation()) this.allowUnbalancedRows = true;
            break;
          }
          case AccountingRowsContainers.SupplierInvoiceAttest.toString(): {
            this.allowUnbalancedRows = true;
          }
        }

        this.useAutomaticAccountDistribution =
          SettingsUtil.getBoolCompanySetting(
            settingTypes,
            CompanySettingType.BillingUseQuantityPrices
          );
      })
    );
  }

  public addRowSetFocusedCell() {
    if (this.addRowWithSetAccountDimFocus()) {
      this.addRow();
      this.setFocusedLastRowStandardAccountCell();
    }
  }
  setFocusedLastRowStandardAccountCell() {
    const index = this.grid.api.getLastDisplayedRowIndex();
    this.startEditingAfterDelay(index, 'dim1Id');
  }

  private startEditingAfterDelay(rowIndex: number, colId?: string): void {
    if (!colId || rowIndex < 0) return;
    setTimeout(() => this.startEditingCell(rowIndex, colId), 100);
  }

  startEditingCell(index: number, colKey: string) {
    this.grid.api.startEditingCell({
      rowIndex: index,
      colKey: colKey,
    });
  }

  setFocusedCell(index: number, colKey: string) {
    this.grid.api.setFocusedCell(index, colKey);
  }

  override onFinished(): void {
    this.finalizeAccountingRows();
    this.addRowSetFocusedCell();
  }

  private finalizeAccountingRows() {
    if (
      this.actorId() &&
      this.actorId() !== 0 &&
      !this.currencyService.hasLedgerCurrency
    )
      this.currencyService.loadLedgerCurrency(this.actorId());
    this.calculateAccountBalances(true);

    if (this.getGridRows()) {
      if (this.clearInternalAccounts())
        this.setRowItemAccountsOnAllRows(this.rowsIn(), true);
      this.syncDistributionRows();
    }
    if (
      this.container.toString() ==
        AccountingRowsContainers.SupplierInvoice.toString() ||
      this.container.toString() ==
        AccountingRowsContainers.SupplierInvoiceAttest.toString()
    )
      this.loadInventoryTriggerAccounts();
  }

  private syncDistributionRows() {
    if (this.accountDistributionHelper)
      this.accountDistributionHelper.setAccountingRows(this.getGridRows());
  }

  private checkAccountDistribution(row: AccountingRowDTO): boolean {
    if (this.accountDistributionHelper)
      return this.accountDistributionHelper.checkAccountDistribution(
        row,
        this.parentGuid()
      );
    return false;
  }

  rowsAdded(setRowItemAccountsOnAllRows: boolean) {
    if (setRowItemAccountsOnAllRows) {
      this.setRowItemAccountsOnAllRows(undefined, false);
    }

    this.calculateAccountBalances();
    this.calculateAllRowsAllCurrencyAmounts(
      TermGroup_CurrencyType.TransactionCurrency,
      false
    );
    this.grid.api.redrawRows();

    this.internalIdCounter = this.getGridRows().length;
  }

  private calculateAllRowsAllCurrencyAmounts(
    sourceCurrencyType: TermGroup_CurrencyType,
    refreshGrid = true
  ) {
    if (this.isReadOnly() || this.getGridRows().length === 0) return;

    const observables: Observable<void>[] = [];
    const updatedRows: AccountingRowDTO[] = [];
    this.getGridRows().forEach(rawRow => {
      const row =
        rawRow instanceof AccountingRowDTO
          ? rawRow
          : new AccountingRowDTO(rawRow);
      updatedRows.push(row);
      observables.push(
        this.calculateRowAllCurrencyAmounts(row, sourceCurrencyType)
      );
    });
    forkJoin(observables).subscribe(() => {
      this.setGridRows(updatedRows);
      if (refreshGrid) {
        this.grid.api.redrawRows();
      }
    });
  }

  private calculateAccountBalances(onReady: boolean = false) {
    if (this.getGridRows && this.getGridRows()) {
      const accountBalances: IDecimalKeyValue[] = [];
      Object.assign(accountBalances, this.accountBalances);
      this.getGridRows().forEach(ar => {
        ar.balance = this.getAccountRowBalance(ar, accountBalances, onReady);
        if (onReady) {
          this.setOrgDebetCreditAmount(ar);
        }
      });
    }
  }

  public setOrgDebetCreditAmount(ar: AccountingRowDTO) {
    if (!ar.orgDebetAmount && (ar.voucherRowId || ar.invoiceRowId)) {
      ar.orgDebetAmount = ar.debitAmount;
    }

    if (!ar.orgCreditAmount && (ar.voucherRowId || ar.invoiceRowId)) {
      ar.orgCreditAmount = ar.creditAmount;
    }
  }

  private getAccountRowBalance(
    ar: AccountingRowDTO,
    accountBalances: IDecimalKeyValue[],
    onReady: boolean = false
  ) {
    const accountId = ar.dim1Id;
    if (!accountId) return 0;
    const balance = accountBalances.find(a => a.key === accountId);
    if (!balance) return 0;

    if (!balance.value) balance.value = 0;

    if (ar['orgCreditAmount'] && ar.isModified) {
      const orgCreditAmount: number = ar['orgCreditAmount'];
      balance.value += orgCreditAmount;
    }

    if (ar['orgDebetAmount'] && ar.isModified) {
      const orgDebetAmount: number = ar['orgDebetAmount'];
      balance.value -= orgDebetAmount;
    }

    if (ar.debitAmount && !onReady && ar.isModified)
      balance.value += parseFloat(<any>ar.debitAmount) || 0;

    if (ar.creditAmount && !onReady && ar.isModified)
      balance.value -= parseFloat(<any>ar.creditAmount) || 0;

    return balance.value;
  }

  private setRowItemAccountsOnAllRows(
    newRows: AccountingRowDTO[] = this.getGridRows(),
    setInternalAccountFromAccount: boolean = false
  ) {
    if (this.accountDims?.length > 0) {
      newRows.forEach(row => {
        this.setRowItemAccountsOnRow(row, setInternalAccountFromAccount);
      });
    }
    this.setGridRows(newRows);
  }

  private setRowItemAccountsOnRow(
    rowItem: AccountingRowDTO,
    setInternalAccountFromAccount: boolean
  ) {
    if (rowItem.dim1Id) {
      this.accountingRowHelperService.setRowItemAccounts(
        rowItem,
        this.accountDims,
        this.accountBalances,
        setInternalAccountFromAccount,
        this.getAccount(rowItem, 1)
      );
    } else {
      this.accountingRowHelperService.setRowItemAccounts(
        rowItem,
        this.accountDims,
        this.accountBalances,
        false,
        undefined,
        false
      );
    }
  }

  private updateMandatoryAndAccountNamesOnAllRows() {
    this.getGridRows().forEach(ar => {
      if (ar.dim1Id) {
        this.updateMandatoryAndAccountNames(ar, this.getAccount(ar, 1));
      }
    });
  }

  private getAccount(rowItem: AccountingRowDTO, dimIndex: number): IAccountDTO {
    let account: any;
    if (this.accountDims && this.accountDims[dimIndex - 1])
      account = this.accountDims[dimIndex - 1].accounts.find(
        (acc: IAccountDTO) =>
          acc.accountId === rowItem[`dim${dimIndex}Id` as keyof typeof rowItem]
      );

    return account;
  }

  private updateMandatoryAndAccountNames(
    rowItem: AccountingRowDTO,
    account: IAccountDTO
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
    // Set internal accounts
    if (account != null) {
      this.accountDims.forEach((accDim, i) => {
        const index = i + 1;
        const acc = accDim.accounts.find(
          acc =>
            acc.accountId === rowItem[`dim${index}Id` as keyof typeof rowItem]
        );

        if (acc) {
          // const key = `dim${index}Nr` as keyof AccountingRowDTO;
          // rowItem[key] = acc.accountNr;
          // rowItem['dim' + index + 'Name'] = acc.name;
        }
      });

      if (account.accountInternals != null) {
        account.accountInternals.forEach(ai => {
          const index =
            this.accountDims.findIndex(
              ad => ad.accountDimNr === ai.accountDimNr
            ) + 1; //index is 0 based, our dims are 1 based

          // let key = `dim${index}Id`;
          // rowItem[key as keyof typeof rowItem] = true;
          // rowItem['dim' + index + 'Disabled'] = ai.mandatoryLevel === 1;
          // rowItem['dim' + index + 'Mandatory'] = ai.mandatoryLevel === 2;
          // rowItem['dim' + index + 'Stop'] = ai.mandatoryLevel === 3;
        });
      }
    }
    this.grid.refreshCells();
  }

  override onGridReadyToDefine(grid: GridComponent<AccountingRowDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.options.context.newRow = false;
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      tabToNextCell: this.onTabToNextCell.bind(this),
    });

    this.translate
      .get([
        'common.accountingrows.rownr',
        'common.date',
        'common.quantity',
        'common.unit',
        'common.text',
        'common.amount',
        'common.amountcurrency',
        'common.debit',
        'common.credit',
        'common.debitcurrency',
        'common.creditcurrency',
        'common.debitentcurrency',
        'common.creditentcurrency',
        'common.debitledgercurrency',
        'common.creditledgercurrency',
        'common.user',
        'common.balance',
        'economy.accounting.voucher.voucherseries',
        'economy.accounting.voucher.vatvoucher',
        'core.deleterow',
        'common.accountingrows.missingaccount',
        'common.accountingrows.invalidaccount',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'common.showtransactions',
        'common.accountingsettings.noaccount',
        'common.accountingsettings.account',
        'common.amount',
        'economy.accounting.distributioncode.numberofperiods',
        'economy.accounting.accountdistributionentry.accrual',
        'common.startdate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.addColumnModified('isModified');
        if (this.showGrouping()) {
          this.groupColumn = this.grid.addColumnText('dim1NrName', '', {
            enableGrouping: true,
            enableHiding: false,
            resizable: false,
          });
          this.groupColumn.name = 'namecolumn';
        }

        if (this.showRowNr())
          this.grid.addColumnNumber(
            'rowNr',
            terms['common.accountingrows.rownr'],
            {
              width: 40,
              enableHiding: false,
              editable: false,
            }
          );

        this.accountDims.forEach((dim, i) => {
          if (this.hideStdDim() && i === 0) return;
          const index = i + 1;
          const idField = `dim${index}Id`;
          const nameField: any = `dim${index}Name`;
          this.grid.addColumnAutocomplete<IAccountDTO>(
            idField as StringKeyOfNumberProperty<IAccountingRowDTO>,
            dim.name,
            {
              editable: ($event: any): boolean => {
                const row: AccountingRowDTO = $event.data;
                const propertyKey =
                  `dim${index}Disabled` as keyof AccountingRowDTO;
                const isDisabled = row[propertyKey];
                return !isDisabled && !this.isReadOnly();
              },
              optionIdField: 'accountId',
              optionNameField: 'numberName',
              optionDisplayNameField: nameField,
              scrollable: true,
              source: _ => dim.accounts,
              cellRenderer: TwoValueCellRenderer,
              cellRendererParams: {
                primaryValueKey: `dim${index}Nr`,
                secondaryValueKey: `dim${index}Name`,
              },
              allowNavigationFrom: (
                value: number,
                row: AccountingRowDTO
              ): boolean => {
                return this.allowNavigationFromAutocomplete(value, row, index);
              },

              suppressFilter: true,
              sortable: false,
              flex: 1,
            }
          );
        });

        if (this.showTextValue()) {
          this.grid.addColumnText('text', terms['common.text'], {
            flex: 1,
            editable: !this.isReadOnly(),
          });
        }

        if (this.showAccrualColumns()) {
          const colAccrualHeader = this.grid.addColumnHeader(
            'accrual',
            terms['economy.accounting.accountdistributionentry.accrual'],
            { marryChildren: true }
          );
          this.grid.addColumnNumber(
            'numberOfPeriods',
            terms['economy.accounting.distributioncode.numberofperiods'],
            {
              flex: 1,
              enableHiding: true,
              enableGrouping: true,
              editable: !this.isReadOnly(),
              headerColumnDef: colAccrualHeader,
            }
          );
          this.grid.addColumnDate('startDate', terms['common.startdate'], {
            flex: 1,
            enableHiding: true,
            enableGrouping: true,
            editable: !this.isReadOnly(),
            headerColumnDef: colAccrualHeader,
          });
        }
        if (this.showQuantity()) {
          this.grid.addColumnNumber('quantity', terms['common.quantity'], {
            flex: 1,
            enableHiding: true,
            aggFuncOnGrouping: 'sum',
            enableGrouping: true,
            clearZero: true,
            editable: !this.isReadOnly(),
          });

          this.grid.addColumnText('unit', terms['common.unit'], {
            enableHiding: true,
            flex: 1,
            editable: !this.isReadOnly(),
          });
        }

        if (this.oneColumnAmountValue() || this.debugMode) {
          this.grid.addColumnNumber('amount', terms['common.amount'], {
            enableHiding: false,
            decimals: this.decimals,
            aggFuncOnGrouping: 'sum',
            enableGrouping: true,
            flex: 1,
            editable: !this.isReadOnly(),
          });
        }
        const colAmountHeader = this.grid.addColumnHeader(
          '',
          terms['common.amount'],
          {
            marryChildren: true,
          }
        );
        this.grid.addColumnNumber('debitAmount', terms['common.debit'], {
          flex: 1,
          enableHiding: false,
          decimals: this.decimals,
          aggFuncOnGrouping: 'sum',
          enableGrouping: true,
          editable: !this.isReadOnly(),
          headerColumnDef: colAmountHeader,
        });
        this.grid.addColumnNumber('creditAmount', terms['common.credit'], {
          flex: 1,
          enableHiding: false,
          decimals: this.decimals,
          aggFuncOnGrouping: 'sum',
          enableGrouping: true,
          editable: !this.isReadOnly(),
          headerColumnDef: colAmountHeader,
        });

        if (
          (!this.oneColumnAmountValue() && this.showTransactionCurrency()) ||
          this.debugMode
        ) {
          this.grid.addColumnNumber(
            'debitAmountCurrency',
            terms['common.debitcurrency'],
            {
              enableHiding: false,
              decimals: this.decimals,
              aggFuncOnGrouping: 'sum',
              enableGrouping: true,
              editable: !this.isReadOnly(),
              headerColumnDef: colAmountHeader,
            }
          );
          this.grid.addColumnNumber(
            'creditAmountCurrency',
            terms['common.creditcurrency'],
            {
              enableHiding: false,
              decimals: this.decimals,
              aggFuncOnGrouping: 'sum',
              enableGrouping: true,
              editable: !this.isReadOnly(),
              headerColumnDef: colAmountHeader,
            }
          );
        }
        if (this.showEnterpriseCurrency() || this.debugMode) {
          this.grid.addColumnNumber(
            'debitAmountEntCurrency',
            terms['common.debitentcurrency'],
            {
              enableHiding: false,
              decimals: this.decimals,
              aggFuncOnGrouping: 'sum',
              enableGrouping: true,
              editable: !this.isReadOnly(),
              headerColumnDef: colAmountHeader,
            }
          );
          this.grid.addColumnNumber(
            'creditAmountEntCurrency',
            terms['common.creditentcurrency'],
            {
              enableHiding: false,
              decimals: this.decimals,
              aggFuncOnGrouping: 'sum',
              enableGrouping: true,
              editable: !this.isReadOnly(),
              headerColumnDef: colAmountHeader,
            }
          );
        }
        if (this.showLedgerCurrency() || this.debugMode) {
          this.grid.addColumnNumber(
            'debitAmountLedgerCurrency',
            terms['common.debitledgercurrency'],

            {
              enableHiding: false,
              decimals: this.decimals,
              aggFuncOnGrouping: 'sum',
              enableGrouping: true,
              editable: !this.isReadOnly(),
              headerColumnDef: colAmountHeader,
            }
          );
          this.grid.addColumnNumber(
            'creditAmountLedgerCurrency',
            terms['common.creditledgercurrency'],

            {
              enableHiding: false,
              decimals: this.decimals,
              aggFuncOnGrouping: 'sum',
              enableGrouping: true,
              editable: !this.isReadOnly(),
            }
          );
        }
        if (this.showAttestUser())
          this.grid.addColumnText('attestUserName', terms['common.user']);
        if (this.showBalance()) {
          this.grid.addColumnNumber('balance', terms['common.balance'], {
            enableHiding: false,
            decimals: this.decimals,
            editable: false,
          });
          this.grid.addColumnIcon('', '', {
            iconName: 'search',
            suppressFilter: true,
            onClick: (row: IAccountingRowDTO) => this.searchVouchers(row),
            tooltip: terms['common.showtransactions'],
          });
        }

        this.grid.addColumnIconDelete({
          tooltip: terms['core.deleterow'],
          onClick: r => this.deleteRow(r),
          alignLeft: true,
        });

        this.grid.setTotalGridRowClassCallback(params => {
          if (params?.rowIndex !== 0) return 'error-color';
          return '';
        });

        if (this.showAmountSummary())
          this.grid.addAggregationsRow({
            quantity: AggregationType.Sum,
            amount: AggregationType.Sum,
            debitAmount: AggregationType.Sum,
            creditAmount: AggregationType.Sum,
            debitAmountCurrency: AggregationType.Sum,
            creditAmountCurrency: AggregationType.Sum,
            debitAmountEntCurrency: AggregationType.Sum,
            creditAmountEntCurrency: AggregationType.Sum,
            debitAmountLedgerCurrency: AggregationType.Sum,
            creditAmountLedgerCurrency: AggregationType.Sum,
          });

        this.grid.context.suppressFiltering = this.hideFilter();

        this.grid.dynamicHeight = false;
        this.grid.setNbrOfRowsToShow(2, 12);

        super.finalizeInitGrid({
          hidden: !this.showAmountSummary(),
        });
        this.runRowValidation();
        this.accountingRowsReady.emit(this.parentGuid());
      });
  }

  allowNavigationFromAutocomplete(
    value: any,
    row: AccountingRowDTO,
    colIndex: number
  ): boolean {
    const currentValue = value;
    if (!currentValue || colIndex != 1)
      //if no value, allow it.
      return true;

    const valueHasMatchingAccount = this.accountDims[
      colIndex - 1
    ].accounts.filter(
      acc =>
        acc.state === SoeEntityState.Active &&
        (acc.numberName === currentValue.toString() ||
          acc.accountNr === currentValue.toString())
    );
    if (valueHasMatchingAccount.length)
      //if there is a value and it is valid, allow it.
      return true;

    if (colIndex === 1) {
      // Account number not found, open add account dialog
      this.openAddAccountDialog(row, currentValue.toString());
    }

    return false;
  }
  protected deleteRow(row: IAccountingRowDTO) {
    const rows = this.getGridRows().filter(
      f => f.rowNr != row.rowNr && f.tempRowId != row.tempRowId
    );
    this.renumberedRows(
      rows, // grid data
      row.rowNr, // from rowNr
      true // subtract
    );
    this.setGridRows(rows);
    this.setFormDirty();
  }

  public createDefaultAccountingRow(
    accountId?: number,
    amount?: number,
    isDebit?: boolean
  ): { rowIndex: number; row: any } {
    const row = {} as AccountingRowDTO;

    row.tempRowId = this.internalIdCounter;
    row.tempInvoiceRowId = this.internalIdCounter;
    this.internalIdCounter++;

    row.type = AccountingRowType.AccountingRow;
    row.rowNr = AccountingRowDTO.getNextRowNr(this.getGridRows());
    row.state = SoeEntityState.Active;

    row.isDebitRow = isDebit ? isDebit : true;
    row.isCreditRow = isDebit ? !isDebit : false;
    row.isModified = true;

    row.setDebitAmount(
      TermGroup_CurrencyType.TransactionCurrency,
      isDebit ? (amount ? amount : 0) : 0
    );
    row.setCreditAmount(
      TermGroup_CurrencyType.TransactionCurrency,
      isDebit ? 0 : amount ? amount : 0
    );
    this.calculateRowAllCurrencyAmounts(
      row,
      TermGroup_CurrencyType.TransactionCurrency
    ).subscribe();

    if (
      this.container == AccountingRowsContainers.Voucher &&
      this.getGridRows().length > 0
    ) {
      const prevRow = this.getGridRows()[this.getGridRows().length - 1];
      if (prevRow && prevRow.text) {
        row.text = prevRow.text;
      }
    }

    this.validateAccountingRow(row);
    const accountingRows = row.rowNr
      ? this.renumberedRows(this.getGridRows(), row.rowNr).map(
          r => new AccountingRowDTO(r)
        )
      : this.getGridRows();

    accountingRows.push(new AccountingRowDTO(row));

    this.setGridRows(accountingRows);

    this.setFormDirty();
    this.setRowItemAccountsOnAllRows(undefined, true);
    this.syncDistributionRows();

    return { rowIndex: this.grid.api.getLastDisplayedRowIndex(), row: row };
  }

  public addRow(insertAtCurrentRow = false): {
    rowIndex: number;
    row: AccountingRowDTO;
  } {
    const row = this.createBaseRow();
    let defaultAccount: IAccountDTO | undefined;

    // Apply container-specific setup
    if (this.isSupplierInvoiceAttestContainer()) {
      this.setupSupplierInvoiceAttestRow(row);
      defaultAccount = this.getDefaultAttestAccountIfNeeded(row);
    }

    // Handle insert at current row
    if (insertAtCurrentRow) {
      this.copyTextAndPositionFromSelectedRow(row);
    }

    // Re-number existing rows if inserting at a specific position
    const accountingRows = this.prepareRowsForInsertion(row);

    // Fill in missing properties and account info
    this.finalizeNewRow(row, defaultAccount);

    // Insert row into grid
    accountingRows.push(new AccountingRowDTO(row));
    this.updateGridAfterInsert(accountingRows, row);

    const addedRowIndex = this.getGridRows().findIndex(
      r => r.rowNr === row.rowNr
    );
    this.focusAndEditRow(addedRowIndex, 'dim1Id');

    return { rowIndex: this.grid.api.getLastDisplayedRowIndex(), row };
  }

  private createBaseRow(): AccountingRowDTO {
    this.internalIdCounter = this.getGridRows().reduce(
      (max, item) => Math.max(max, item.tempRowId),
      this.internalIdCounter
    );
    const row = {} as AccountingRowDTO;
    row.tempRowId = ++this.internalIdCounter;
    row.tempInvoiceRowId = this.internalIdCounter;
    row.type = AccountingRowType.AccountingRow;
    row.rowNr = AccountingRowDTO.getNextRowNr(this.getGridRows());
    row.state = SoeEntityState.Active;
    row.dim1Mandatory = true;
    return row;
  }

  private isSupplierInvoiceAttestContainer(): boolean {
    return this.container === AccountingRowsContainers.SupplierInvoiceAttest;
  }

  private setupSupplierInvoiceAttestRow(row: AccountingRowDTO): void {
    row.type = AccountingRowType.SupplierInvoiceAttestRow;
    row.attestUserId = SoeConfigUtil.userId;
    row.attestStatus = SupplierInvoiceAccountRowAttestStatus.New;
    row.isDebitRow = true;
    row.isCreditRow = false;
    row.isModified = true;
  }

  private getDefaultAttestAccountIfNeeded(
    row: AccountingRowDTO
  ): IAccountDTO | undefined {
    if (this.getGridRows().length > 0) return undefined;

    const accountDim = this.accountDims?.[0];
    if (!accountDim?.accounts) return undefined;

    const account = accountDim.accounts.find(
      a => a.accountId === this.defaultAttestRowDebitAccountId()
    );

    row.amount = this.defaultAttestRowAmount();
    this.calculateRowCurrencyAmounts(
      row,
      TermGroup_CurrencyType.BaseCurrency,
      TermGroup_CurrencyType.TransactionCurrency
    ).subscribe();

    return account;
  }

  private copyTextAndPositionFromSelectedRow(row: AccountingRowDTO): void {
    const selectedRow =
      this.grid.getSelectedRows()[0] || this.grid.getCurrentRow();
    if (!selectedRow) return;

    row.text = selectedRow.text;
    row.rowNr = selectedRow.rowNr + 1;
    (row as any).isNew = true;
  }

  private prepareRowsForInsertion(
    newRow: AccountingRowDTO
  ): AccountingRowDTO[] {
    if (newRow.rowNr) {
      return this.renumberedRows(this.getGridRows(), newRow.rowNr).map(
        r => new AccountingRowDTO(r)
      );
    }
    return this.getGridRows();
  }

  private finalizeNewRow(row: AccountingRowDTO, account?: IAccountDTO): void {
    if (!row.rowNr) {
      row.text = this.getTextFromLastRow() ?? '';
      row.rowNr = AccountingRowDTO.getNextRowNr(this.getGridRows());
    }

    this.accountingRowHelperService.setRowItemAccounts(
      row,
      this.accountDims,
      this.accountBalances,
      true,
      account
    );

    this.validateAccountingRow(row);
  }

  private updateGridAfterInsert(
    accountingRows: AccountingRowDTO[],
    row: AccountingRowDTO
  ): void {
    this.setGridRows(accountingRows);
    this.setFormDirty();
  }

  private focusAndEditRow(rowIndex: number, colKey: string): void {
    this.setFocusedCell(rowIndex, colKey);
    this.startEditingAfterDelay(rowIndex, colKey);
  }

  private renumberedRows(
    rows: AccountingRowDTO[],
    fromRowNr: number,
    negative: boolean = false
  ): AccountingRowDTO[] {
    const rowsToRenumber = rows.filter(
      f => f.rowNr >= fromRowNr && !(f as any).isNew
    );
    if (negative)
      rowsToRenumber.forEach(row => {
        row.rowNr--;
      });
    else
      rowsToRenumber.forEach(row => {
        row.rowNr++;
      });
    return [
      ...this.getGridRows().filter(f => f.rowNr < fromRowNr),
      ...rowsToRenumber,
    ];
  }

  private getTextFromLastRow(): string | undefined {
    if (
      this.container == AccountingRowsContainers.Voucher &&
      this.getGridRows().length > 0
    ) {
      const prevRow = this.getGridRows()[this.getGridRows().length - 1];
      return prevRow?.text ?? '';
    }
    return undefined;
  }

  private updateParent() {
    this.accountingRowsChanged.emit(this.getGridRows());
  }

  private calculateRowCurrencyAmounts(
    row: AccountingRowDTO,
    sourceCurrencyType: TermGroup_CurrencyType,
    targetCurrencyType: TermGroup_CurrencyType
  ): Observable<void> {
    if (sourceCurrencyType === targetCurrencyType) return of(void 0);

    if (this.oneColumnAmount()) {
      const amount = this.currencyService.getCurrencyAmount(
        row.getAmount(sourceCurrencyType),
        sourceCurrencyType,
        targetCurrencyType
      );
      row.setAmount(targetCurrencyType, amount);
    } else {
      const debit = this.currencyService.getCurrencyAmount(
        row.getDebitAmount(sourceCurrencyType),
        sourceCurrencyType,
        targetCurrencyType
      );

      const credit = this.currencyService.getCurrencyAmount(
        row.getCreditAmount(sourceCurrencyType),
        sourceCurrencyType,
        targetCurrencyType
      );

      row.setDebitAmount(targetCurrencyType, debit);
      row.setCreditAmount(targetCurrencyType, credit);
    }
    return of(void 0);
  }

  private calculateRowAllCurrencyAmounts(
    row: AccountingRowDTO,
    sourceCurrencyType: TermGroup_CurrencyType
  ): Observable<void> {
    if (this.isReadOnly()) return of(void 0);

    return forkJoin([
      this.calculateRowCurrencyAmounts(
        row,
        sourceCurrencyType,
        TermGroup_CurrencyType.BaseCurrency
      ),
      this.calculateRowCurrencyAmounts(
        row,
        sourceCurrencyType,
        TermGroup_CurrencyType.EnterpriseCurrency
      ),
      this.calculateRowCurrencyAmounts(
        row,
        sourceCurrencyType,
        TermGroup_CurrencyType.LedgerCurrency
      ),
      this.calculateRowCurrencyAmounts(
        row,
        sourceCurrencyType,
        TermGroup_CurrencyType.TransactionCurrency
      ),
    ]).pipe(map(() => void 0));
  }

  initRegenerateRows() {
    const response = this.messageboxService.warning(
      'core.warning',
      'common.accountingrows.regeneraterowswarning'
    );
    response.afterClosed().subscribe((resp: IMessageboxComponentResponse) => {
      if (resp?.result) this.regenerateCoding.emit();
    });
  }

  loadAccountsRows() {
    this.loadAccounts(false).subscribe();
  }

  private loadAccountDistributions(useCache: boolean) {
    // Variables for context-specific settings
    let useInVoucher: boolean | null = null; //must send null or true, should be fixed on the serverside that false could be sent to
    let useInSupplierInvoice: boolean | null = null;
    let useInCustomerInvoice: boolean | null = null;
    let useInImport: boolean | null = null;

    //Is the accountingrowsdirective inside a voucher, supplierinvoice or customerinvoice
    // Set context-specific flags
    switch (this.container.toString()) {
      case AccountingRowsContainers.Voucher.toString():
        useInVoucher = true;
        break;
      case AccountingRowsContainers.SupplierInvoice.toString():
        useInSupplierInvoice = true;
        break;
      case AccountingRowsContainers.CustomerInvoice.toString():
        useInCustomerInvoice = true;
        useInImport = true;
        break;
    }

    return this.accountDistributionService
      .getAccountDistributionHeadsUsedIn(
        undefined,
        TermGroup_AccountDistributionTriggerType.Registration,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined
      )
      .pipe(
        tap(data => {
          let accountDistributionHeads: IAccountDistributionHeadDTO[] = [];
          let accountDistributionHeadsForImport: IAccountDistributionHeadDTO[] =
            [];
          switch (this.container.toString()) {
            case AccountingRowsContainers.Voucher.toString(): {
              accountDistributionHeads = data.filter(y => y.useInVoucher);
              accountDistributionHeadsForImport = data.filter(
                y => !y.useInVoucher && y.useInImport
              );
              break;
            }
            case AccountingRowsContainers.SupplierInvoice.toString(): {
              accountDistributionHeads = data.filter(
                y => y.useInSupplierInvoice
              );
              accountDistributionHeadsForImport = data.filter(
                y => !y.useInSupplierInvoice && y.useInImport
              );
              break;
            }
            case AccountingRowsContainers.CustomerInvoice.toString(): {
              accountDistributionHeads = data.filter(
                y => y.useInCustomerInvoice
              );
              accountDistributionHeadsForImport = data.filter(
                y => !y.useInCustomerInvoice && y.useInImport
              );
              break;
            }
            default:
              accountDistributionHeads = [];
              accountDistributionHeadsForImport = [];
              break;
          }

          this.accountDistributionHelper = new DistributionHelperService(
            accountDistributionHeads,
            accountDistributionHeadsForImport,
            this.useAutomaticAccountDistribution,
            this.container,

            this.distributionHelperDoneCallback.bind(this),
            this.deleteRow.bind(this),
            this.addRow.bind(this)
          );
        })
      );
  }

  private distributionHelperDoneCallback(normalMoveNext: boolean = true) {
    this.updateMandatoryAndAccountNamesOnAllRows();
    this.calculateAccountBalances();
    this.calculateAllRowsAllCurrencyAmounts(
      TermGroup_CurrencyType.BaseCurrency
    );

    const emptyRows = this.getGridRows().filter(i => i.dim1Id == 0);

    if (emptyRows && emptyRows.length > 0) {
      emptyRows.forEach(row => {
        this.deleteRow(row);
      });
    }

    if (!normalMoveNext) {
      this.setFocusedLastRowStandardAccountCell();
    }
  }

  private loadAccountBalances(): Observable<IDecimalKeyValue[]> {
    return this.ayService.ensureAccountYearIsLoaded$(() =>
      this.accountingRowsService
        .getAccountBalances(this.ayService.selectedAccountYearId())
        .pipe(
          tap(x => {
            this.accountBalances = x;
          })
        )
    );
  }

  public reloadAccountBalances(): Observable<IDecimalKeyValue[]> {
    return this.loadAccountBalances().pipe(
      tap(x => {
        this.calculateAccountBalances(false);
      })
    );
  }

  private loadAccounts(useCache: boolean): Observable<IAccountDimSmallDTO[]> {
    return this.performLoad.load$(
      this.coreService
        .getAccountDimsSmall(
          false, // only standard
          this.hideStdDim(), // only internal
          true, // load accounts
          true, // load internal accounts
          true, // load parent
          false, // load inactives
          false, // load inactive dims
          true, // include parent accounts
          useCache // use cache
        )
        .pipe(
          tap(x => {
            this.accountDims = x;
            if (this.hideStdDim()) {
              // Creating empty standard dim, to make indexing easier
              this.accountDims.splice(0, 0, <IAccountDimSmallDTO>{
                accountDimId: 0,
                accountDimNr: 1,
                accounts: <IAccountDTO[]>[],
              });
            }

            this.accountDims.forEach((dim, idx) => {
              if (!dim.accounts) dim.accounts = [];

              if (this.useNoAccount()) {
                //Adding 'No Account'
                dim.accounts.splice(
                  0,
                  0,
                  this.convertAccount(
                    -1,
                    dim.accountDimId,
                    '-',
                    this.terms['common.accountingsettings.noaccount'],
                    this.terms['common.accountingsettings.noaccount']
                  )
                );
              }

              //Adding Empty Account
              dim.accounts.splice(
                0,
                0,
                this.convertAccount(0, dim.accountDimId, '', '', '')
              );
            });
            this.setRowItemAccountsOnAllRows();
          })
        )
    );
  }

  private convertAccount(
    accountId: number,
    accountDimId: number,
    accountNr: string,
    name: string,
    numberName: string
  ): IAccountDTO {
    return <IAccountDTO>{
      accountId,
      accountDimId,
      accountNr,
      name,
      numberName,
    };
  }

  openAddAccountDialog(row: AccountingRowDTO, accountNr: string) {
    const dialogRef = this.dialogService.open(AddAccountDialogComponent, {
      title: `${this.translate.instant('common.accountingrows.addaccount.title')} ${accountNr}`,
      size: 'lg',
      accountNr,
    });

    dialogRef.afterClosed().subscribe((result: AddAccountDialogResultData) => {
      if (!result?.data) return;

      switch (result.type) {
        case AddAccountDialogResultType.Copy:
          this.handleCopyAccount(row, result.data as ISysAccountStdDTO);
          break;

        case AddAccountDialogResultType.New:
          this.handleNewAccount(row, result.data as AccountEditDTO);
          break;

        default:
          // do nothing for cancel or unsupported result types
          break;
      }
    });
  }

  private handleCopyAccount(
    row: AccountingRowDTO,
    sysAccount: ISysAccountStdDTO
  ): void {
    this.performLoadData.load(
      this.accountingService.copySysAccountStd(sysAccount.sysAccountStdId).pipe(
        tap({
          next: (copiedAccount: AccountDTO) => {
            if (!copiedAccount) return;

            this.addNewAccount(0, copiedAccount);
            const gridRow = this.grid
              .getAllRows()
              .find(r => r.rowNr === row.rowNr);

            if (gridRow) {
              gridRow.dim1Id = copiedAccount.accountId;
              gridRow.dim1Error = '';
              this.accountingRowHelperService.setRowItemAccounts(
                gridRow,
                this.accountDims,
                this.accountBalances,
                true,
                copiedAccount
              );
            }

            this.refreshAndNavigateNext();
          },
          error: () => {
            this.messageboxService.error(
              this.translate.instant('common.loadfailed'),
              this.translate.instant('economy.accounting.account')
            );
          },
        })
      )
    );
  }

  private handleNewAccount(
    row: AccountingRowDTO,
    account: AccountEditDTO
  ): void {
    const model: SaveAccountSmallModel = new SaveAccountSmallModel();

    model.accountNr = account.accountNr;
    model.name = account.name;
    model.accountTypeId = account.accountTypeSysTermId;
    model.vatAccountId = account.sysVatAccountId ?? 0;
    model.sruCode1Id = account.sysAccountSruCode1Id ?? 0;

    this.performLoadData.load(
      this.accountingService.saveAccountSmall(model).pipe(
        tap({
          next: (postResult: any) => {
            const newAccount = postResult?.value as AccountDTO;

            if (postResult.success && newAccount?.accountId > 0) {
              this.addNewAccount(0, newAccount);
              const updatedRows = this.getGridRows().map(r => {
                if (r.rowNr === row.rowNr) {
                  r.dim1Id = newAccount.accountId;
                  r.dim1Error = '';
                  this.accountingRowHelperService.setRowItemAccounts(
                    r,
                    this.accountDims,
                    this.accountBalances,
                    true,
                    newAccount
                  );
                }
                return r;
              });

              this.setGridRows(updatedRows, false);
              this.refreshAndNavigateNext();
            } else {
              this.messageboxService.error(
                this.translate.instant('common.savefailed'),
                this.translate.instant('economy.accounting.account')
              );
            }
          },
          error: () => {
            this.messageboxService.error(
              this.translate.instant('common.savefailed'),
              this.translate.instant('economy.accounting.account')
            );
          },
        })
      )
    );
  }

  /**
   * Utility: refresh grid UI and move to next cell.
   */
  private refreshAndNavigateNext(): void {
    this.grid.api.refreshCells();
    this.grid.api.tabToNextCell();
  }

  addNewAccount(dimIndex: number, newAccount: IAccountDTO) {
    this.accountDims[dimIndex].accounts.push(newAccount);
  }

  protected searchVouchers(row: any) {
    const selectReportDialog = this.dialogService.open(
      VoucherSearchDialogComponent,
      {
        title: '',
        size: 'lg',
        dim1Id: row.dim1Id,
        dim1Name: row.dim1Name,
        dim1Nr: row.dim1Nr,
      }
    );

    selectReportDialog.afterClosed().subscribe((result: any) => {
      if (result && result.voucherHeadId) {
        this.openVoucher.emit(result.voucherHeadId);
      }
    });
  }
}
