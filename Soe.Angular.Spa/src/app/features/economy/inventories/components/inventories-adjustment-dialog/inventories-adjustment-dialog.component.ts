import { Component, computed, inject, signal, ViewChild } from '@angular/core';
import { VoucherSeriesTypeService } from '@features/economy/services/voucher-series-type.service';
import { TranslateService } from '@ngx-translate/core';
import { AccountingRowsComponent } from '@shared/components/accounting-rows/accounting-rows/accounting-rows.component';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import {
  AccountingRowType,
  InventoryAccountType,
  SoeEntityState,
  TermGroup_InventoryLogType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IAccountingSettingsRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IVoucherSeriesTypeDTO } from '@shared/models/generated-interfaces/VoucherSeriesDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { CurrencyService } from '@shared/services/currency.service';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { Perform } from '@shared/util/perform.class';
import { InventoryAdjustFunctions } from '@shared/util/Enumerations';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { Observable, tap } from 'rxjs';
import {
  addAdjustmentDateValidator,
  debitCreditBalanceValidationError,
  InventoriesAdjustmentForm,
} from '../../models/inventories-adjustment-form.model';
import {
  InventoriesAdjustmentDialogData,
  InventoryAdjustmentDTO,
  SaveAdjustmentModel,
} from '../../models/inventories.model';
import { InventoriesService } from '../../services/inventories.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  templateUrl: './inventories-adjustment-dialog.component.html',
  providers: [FlowHandlerService, CurrencyService, ValidationHandler],
  standalone: false,
})
export class InventoriesAdjustmentDialogComponent extends DialogComponent<InventoriesAdjustmentDialogData> {
  handler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  validationHandler = inject(ValidationHandler);
  messageboxService = inject(MessageboxService);
  voucherSeriesTypeService = inject(VoucherSeriesTypeService);
  inventoriesService = inject(InventoriesService);
  currencyService = inject(CurrencyService);

  accountingSettings: IAccountingSettingsRowDTO[] = [];
  inventoryBaseAccounts: ISmallGenericType[] = [];
  voucherSeriesTypes: IVoucherSeriesTypeDTO[] = [];

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  performLoad = new Perform<any>(this.progressService);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  performSaveAdjustment = new Perform<any>(this.progressService);

  form: InventoriesAdjustmentForm = new InventoriesAdjustmentForm({
    validationHandler: this.validationHandler,
    element: new InventoryAdjustmentDTO(),
  });

  private terms: TermCollection = {};

  dialogLabel = '';
  inventoryLogType: TermGroup_InventoryLogType =
    TermGroup_InventoryLogType.Discarded;
  adjustmentType = 0;
  isNew = false;
  isMissingStandardAccounts = false;
  adjustmentDate = DateUtil.getToday();
  accountingRows = signal<AccountingRowDTO[]>([]);
  private _accountingRows = signal<AccountingRowDTO[]>([]);
  private _nextTempRowId = 1;
  protected numberOfDecimals: number = 2;
  isTrackChangesAccordionOpen = false;

  @ViewChild(AccountingRowsComponent)
  accountingRowsComponent!: AccountingRowsComponent;

  private debitAggregate = computed(() =>
    this._accountingRows().reduce(
      (sum, row) => sum + (Number(row.debitAmount) || 0),
      0
    )
  );
  private creditAggregate = computed(() =>
    this._accountingRows().reduce(
      (sum, row) => sum + (Number(row.creditAmount) || 0),
      0
    )
  );
  protected isDispose = computed(
    () =>
      this.data?.adjustmentType == InventoryAdjustFunctions.Sold ||
      this.data?.adjustmentType == InventoryAdjustFunctions.Discarded
  );

  constructor(private translationService: TranslateService) {
    super();
    this.handler.execute({
      lookups: [this.loadTerms(), this.loadVoucherSeriesType()],
      onFinished: this.finished.bind(this),
    });
  }

  private finished() {
    this.handleAdjustmentType();
    this.setFormValues();
    this.addFormValidators();
    this.createAccountingRows();
  }

  private handleAdjustmentType() {
    const adjustmentType: InventoryAdjustFunctions = this.data.adjustmentType;
    switch (adjustmentType) {
      case InventoryAdjustFunctions.OverWriteOff:
        this.dialogLabel =
          this.terms['economy.inventory.inventories.overwriteoff'];
        this.inventoryLogType = TermGroup_InventoryLogType.OverWriteOff;
        break;
      case InventoryAdjustFunctions.UnderWriteOff:
        this.dialogLabel =
          this.terms['economy.inventory.inventories.underwriteoff'];
        this.inventoryLogType = TermGroup_InventoryLogType.UnderWriteOff;
        break;
      case InventoryAdjustFunctions.WriteUp:
        this.dialogLabel = this.terms['economy.inventory.inventories.writeup'];
        this.inventoryLogType = TermGroup_InventoryLogType.WriteUp;
        break;
      case InventoryAdjustFunctions.WriteDown:
        this.dialogLabel =
          this.terms['economy.inventory.inventories.writedown'];
        this.inventoryLogType = TermGroup_InventoryLogType.WriteDown;
        break;
      case InventoryAdjustFunctions.Discarded:
        this.dialogLabel =
          this.terms['economy.inventory.inventories.discarded'];
        this.inventoryLogType = TermGroup_InventoryLogType.Discarded;
        break;
      case InventoryAdjustFunctions.Sold:
        this.dialogLabel = this.terms['economy.inventory.inventories.sold'];
        this.inventoryLogType = TermGroup_InventoryLogType.Sold;
        break;
    }
  }

  private addFormValidators() {
    this.form.addValidators(
      addAdjustmentDateValidator(
        this.terms[
          'economy.inventory.inventories.adjustment.purchasedatecomparisonerror'
        ]
      )
    );
  }

  private setFormValues() {
    if (this.data) {
      this.form?.patchValue({ amount: 0 });

      if (this.data.inventoryId) {
        this.form?.patchValue({ inventoryId: this.data.inventoryId });
      }
      if (this.data.purchaseDate) {
        this.form?.patchValue({ purchaseDate: this.data.purchaseDate });
      }
      if (this.data.purchaseAmount) {
        if (this.data.adjustmentType == InventoryAdjustFunctions.Discarded) {
          this.form?.patchValue({
            amount: this.data.purchaseAmount,
          });
        }
      }
      if (this.data.accWriteOffAmount) {
        this.form?.patchValue({
          accWriteOffAmount: this.data.accWriteOffAmount,
        });
      }
      if (this.data.accountingSettings) {
        this.accountingSettings = this.data.accountingSettings;
      }
      if (this.data.inventoryBaseAccounts) {
        this.inventoryBaseAccounts = this.data.inventoryBaseAccounts;
      }
      if (this.data.noteText) {
        this.form?.patchValue({ noteText: this.data.noteText });
      }
      this.form?.patchValue({
        noteText: this.dialogLabel + ', ' + this.data.noteText,
        isDisposed: this.isDispose(),
      });
    }
  }

  protected cancel() {
    this.closeDialog();
  }

  private closeDialogWithData(data: BackendResponse): void {
    if (data) {
      this.dialogRef.close(data);
    }
  }

  protected save(): void {
    const model = new SaveAdjustmentModel(
      this.form?.value.inventoryId,
      this.inventoryLogType,
      this.form?.value.voucherSeriesTypeId,
      this.form?.value.amount,
      this.form?.value.adjustmentDate,
      this.form?.value.noteText,
      this.form?.value.accountingRows
    );

    this.performSaveAdjustment.crud(
      CrudActionTypeEnum.Save,
      this.inventoriesService.saveAdjustment(model).pipe(
        tap((returnValue: BackendResponse) => {
          if (returnValue && returnValue.success) {
            ResponseUtil.setDecimalValue(returnValue, this.form?.value.amount);
            // returnValue.decimalValue = this.form?.value.amount;
            this.closeDialogWithData(returnValue);
          } else {
            this.messageboxService.error(
              'core.error',
              'economy.inventory.inventories.adjustment.saveadjustmentnotsucceedederror'
            );
          }
        })
      )
    );
  }

  protected createAccountingRows() {
    this.form.accountingRows.clear();

    switch (this.data.adjustmentType) {
      case InventoryAdjustFunctions.OverWriteOff:
        this.createAccountingRow(
          InventoryAccountType.OverWriteOff,
          0,
          true,
          this.form?.value.amount
        );
        this.createAccountingRow(
          InventoryAccountType.AccOverWriteOff,
          0,
          false,
          this.form?.value.amount
        );
        break;
      case InventoryAdjustFunctions.UnderWriteOff:
        this.createAccountingRow(
          InventoryAccountType.AccOverWriteOff,
          0,
          true,
          this.form?.value.amount
        );
        this.createAccountingRow(
          InventoryAccountType.OverWriteOff,
          0,
          false,
          this.form?.value.amount
        );
        break;
      case InventoryAdjustFunctions.WriteUp:
        this.createAccountingRow(
          InventoryAccountType.AccWriteUp,
          0,
          true,
          this.form?.value.amount
        );
        this.createAccountingRow(
          InventoryAccountType.WriteUp,
          0,
          false,
          this.form?.value.amount
        );
        break;
      case InventoryAdjustFunctions.WriteDown:
        this.createAccountingRow(
          InventoryAccountType.WriteDown,
          0,
          true,
          this.form?.value.amount
        );
        this.createAccountingRow(
          InventoryAccountType.AccWriteDown,
          0,
          false,
          this.form?.value.amount
        );
        break;
      case InventoryAdjustFunctions.Discarded:
        {
          this.createAccountingRow(
            InventoryAccountType.Inventory,
            0,
            false,
            this.data.purchaseAmount
          );
          this.createAccountingRow(
            InventoryAccountType.AccWriteOff,
            0,
            true,
            this.data.accWriteOffAmount
          );

          const diffAmount =
            this.data.purchaseAmount - this.data.accWriteOffAmount;

          if (diffAmount < 0)
            this.createAccountingRow(
              InventoryAccountType.SalesProfit,
              0,
              false,
              Math.abs(diffAmount)
            );
          else if (diffAmount > 0)
            this.createAccountingRow(
              InventoryAccountType.SalesLoss,
              0,
              true,
              Math.abs(diffAmount)
            );
        }
        break;
      case InventoryAdjustFunctions.Sold:
        {
          this.createAccountingRow(
            InventoryAccountType.Inventory,
            0,
            false,
            this.data.purchaseAmount
          );
          this.createAccountingRow(
            InventoryAccountType.AccWriteOff,
            0,
            true,
            this.data.accWriteOffAmount
          );
          this.createAccountingRow(
            InventoryAccountType.Sales,
            0,
            true,
            this.form?.value.amount
          );

          const salesProfitAmount: number =
            this.form?.value.amount -
            (this.data.purchaseAmount - this.data.accWriteOffAmount);

          if (salesProfitAmount > 0)
            this.createAccountingRow(
              InventoryAccountType.SalesProfit,
              0,
              false,
              salesProfitAmount
            );
          else if (salesProfitAmount <= 0)
            this.createAccountingRow(
              InventoryAccountType.SalesLoss,
              0,
              true,
              salesProfitAmount
            );
        }
        break;
    }

    if (this.isMissingStandardAccounts && this.isNew)
      this.messageboxService.information(
        this.terms['core.info'],
        this.terms[
          'economy.inventory.inventories.adjustment.standardaccountsmissing'
        ]
      );

    this.accountingRows.set([...this.form.accountingRows.value]);
    this.isNew = false;
  }

  private createAccountingRow(
    type: InventoryAccountType,
    accountId: number,
    isDebitRow: boolean,
    amount: number
  ) {
    const row = {} as AccountingRowDTO;
    row.type = AccountingRowType.AccountingRow;
    row.invoiceAccountRowId = 0;
    row.amountCurrency = isDebitRow ? Math.abs(amount) : -Math.abs(amount);
    row.debitAmountCurrency = isDebitRow ? Math.abs(amount) : 0;
    row.creditAmountCurrency = isDebitRow ? 0 : Math.abs(amount);

    row.amount = isDebitRow ? Math.abs(amount) : -Math.abs(amount);
    row.debitAmount = isDebitRow ? Math.abs(amount) : 0;
    row.creditAmount = isDebitRow ? 0 : Math.abs(amount);

    //Rounding
    row.amountCurrency = NumberUtil.round(
      row.amountCurrency,
      this.numberOfDecimals
    );
    row.debitAmountCurrency = NumberUtil.round(
      row.debitAmountCurrency,
      this.numberOfDecimals
    );
    row.creditAmountCurrency = NumberUtil.round(
      row.creditAmountCurrency,
      this.numberOfDecimals
    );
    row.amount = NumberUtil.round(row.amount, this.numberOfDecimals);
    row.debitAmount = NumberUtil.round(row.debitAmount, this.numberOfDecimals);
    row.creditAmount = NumberUtil.round(
      row.creditAmount,
      this.numberOfDecimals
    );

    row.quantity = undefined;
    row.isCreditRow = !isDebitRow;
    row.isDebitRow = isDebitRow;
    row.isVatRow = false;
    row.isContractorVatRow = false;
    row.isInterimRow = false;
    row.state = SoeEntityState.Active;
    row.isModified = false;
    row.text = this.form?.noteText.value;

    // Set accounts
    const rowItem = this.data.accountingSettings.find(x => x.type == type);
    const baseAccount = this.data.inventoryBaseAccounts.find(y => y.id == type);

    if (rowItem?.account1Id) {
      row.dim1Id = rowItem.account1Id;
      row.dim2Id = rowItem.account2Id;
      row.dim3Id = rowItem.account3Id;
      row.dim4Id = rowItem.account4Id;
      row.dim5Id = rowItem.account5Id;
      row.dim6Id = rowItem.account6Id;
    } else {
      row.dim1Id = baseAccount != null ? Number(baseAccount?.name) : accountId;
      row.dim1Nr = '';
      row.dim1Name = '';
      row.dim1Mandatory = true;
      row.dim2Id = 0;
      row.dim2Nr = '';
      row.dim2Name = '';
      row.dim3Id = 0;
      row.dim3Nr = '';
      row.dim3Name = '';
      row.dim4Id = 0;
      row.dim4Nr = '';
      row.dim4Name = '';
      row.dim5Id = 0;
      row.dim5Nr = '';
      row.dim5Name = '';
      row.dim6Id = 0;
      row.dim6Nr = '';
      row.dim6Name = '';
    }

    row.rowNr = this.form?.value.accountingRows.length + 1;
    row.tempRowId = this._nextTempRowId;
    this._nextTempRowId += 1;
    this.form?.patchAccountingRow(row, true);
    if (row.dim1Id == 0) this.isMissingStandardAccounts = true;
  }

  private loadVoucherSeriesType(): Observable<void> {
    return this.performLoad.load$(
      this.voucherSeriesTypeService.getGrid().pipe(
        tap(voucherSeriesTypes => {
          this.voucherSeriesTypes = voucherSeriesTypes;
        })
      )
    );
  }

  private loadTerms(): Observable<void> {
    return this.performLoad.load$(
      this.translationService
        .get([
          'economy.inventory.inventories.overwriteoff',
          'economy.inventory.inventories.underwriteoff',
          'economy.inventory.inventories.writeup',
          'economy.inventory.inventories.writedown',
          'economy.inventory.inventories.discarded',
          'economy.inventory.inventories.sold',
          'common.amount',
          'common.note',
          'economy.inventory.inventories.adjustmentdate',
          'core.warning',
          'core.info',
          'economy.inventory.inventories.adjustment.amountmissing',
          'economy.inventory.inventories.adjustment.datemissing',
          'economy.inventory.inventories.adjustment.voucherseriemissing',
          'economy.inventory.inventories.adjustment.purchasedatecomparisonerror',
          'economy.inventory.inventories.adjustment.saveadjustmentnotsucceedederror',
          'economy.inventory.inventories.adjustment.standardaccountsmissing',
          'economy.accounting.voucher.unbalancedrows',
        ])
        .pipe(
          tap(terms => {
            this.terms = terms;
          })
        )
    );
  }

  protected amountChanged(newValue: number) {
    // If we'd want this type of functionality for Discarding and/or Selling, we'd need separate calculations for them.
    if (this.isDispose()) return;

    this._accountingRows.update(rows => {
      rows.forEach(row => {
        if (row.isDebitRow) {
          if (row.debitAmount != 0)
            row.debitAmount = NumberUtil.round(
              (row.debitAmount / this.debitAggregate()) * newValue,
              this.numberOfDecimals
            );
          else if (rows.length == 2) row.debitAmount = newValue;
        } else if (row.isCreditRow) {
          if (row.creditAmount != 0)
            row.creditAmount = NumberUtil.round(
              (row.creditAmount / this.creditAggregate()) * newValue,
              this.numberOfDecimals
            );
          else if (rows.length == 2) row.creditAmount = newValue;
        }
        // Rows created manually.
        else {
          if (row.debitAmount != 0)
            row.debitAmount = NumberUtil.round(
              (row.debitAmount / this.debitAggregate()) * newValue,
              this.numberOfDecimals
            );
          if (row.creditAmount != 0)
            row.creditAmount = NumberUtil.round(
              (row.creditAmount / this.creditAggregate()) * newValue,
              this.numberOfDecimals
            );
        }
      });
      return [...rows];
    });
    this.updateChild();
  }

  private updateChild() {
    this.accountingRows.set([...this._accountingRows()]);
  }

  protected accountingRowsChanged(accountingRows: AccountingRowDTO[]) {
    this._accountingRows.set(accountingRows);
    this.form?.patchAccountingRows(accountingRows);
  }

  protected hasDebitCreditBalanceError(hasError: boolean) {
    this.form.clearValidators();
    if (hasError) {
      this.form.addValidators(debitCreditBalanceValidationError());
    }
    this.form.updateValueAndValidity();
  }
}
