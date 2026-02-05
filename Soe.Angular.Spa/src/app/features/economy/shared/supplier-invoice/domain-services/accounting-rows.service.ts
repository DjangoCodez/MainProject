import { inject, Injectable } from '@angular/core';
import {
  CompanySettingType,
  SupplierAccountType,
  TermGroup_InvoiceVatType,
} from '@shared/models/generated-interfaces/Enumerations';
import { SupplierInvoiceSettingsService } from '../services/supplier-invoice-settings.service';

// Import extracted modules
import {
  AccountingRowFactory,
  IAccountingRowWithDimensionsDTO,
} from './accounting-row-factory';
import { IAccountingSettingsRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

// ============================================================================
// MAIN SERVICE
// ============================================================================

const DIM_INDEXES = [1, 2, 3, 4, 5, 6] as const;
type DimIndex = (typeof DIM_INDEXES)[number];
type DimKey = `dim${DimIndex}Id`;

type DimObject = Partial<Record<DimKey, number>>;

export interface CreateAccountingRowParams {
  dim1Id?: number;
  type: SupplierAccountType;
  amount: number;
  amountBaseCurrency: number;
  isDebitRow: boolean;
  isVatRow?: boolean;
  isContractorVatRow?: boolean;
  text?: string;

  dim2Id?: number;
  dim3Id?: number;
  dim4Id?: number;
  dim5Id?: number;
  dim6Id?: number;
}
export interface CreateAccountingRowContext {
  accountingRows: IAccountingRowWithDimensionsDTO[];
  voucherDate?: Date;
  vatCodeVatAccountId?: number; // From VatCode
  accountingSettings: IAccountingSettingsRowDTO[];
  isCreditInvoice: boolean;
  // Additional focused parameters
  isInterimInvoice: boolean;
  vatType: TermGroup_InvoiceVatType;
  vatRate: number;
}

export interface GenerateAccountingRowsInput {
  // Amounts - only the transaction currency amounts we actually use
  amountBaseCurrency: number;
  vatAmountBaseCurrency: number;
  amountTransactionCurrency: number;
  vatAmountTransactionCurrency: number;
  costRows: CreateAccountingRowParams[] | null;
}

@Injectable({
  providedIn: 'root',
})
export class InvoiceAccountingRowsService {
  private readonly settingService = inject(SupplierInvoiceSettingsService);

  // ============================================================================
  // PUBLIC API
  // ============================================================================

  /**
   * Creates a new accounting row with the specified parameters
   */
  public createAccountingRow(
    params: CreateAccountingRowParams,
    context: CreateAccountingRowContext
  ): IAccountingRowWithDimensionsDTO {
    const { type, amount, amountBaseCurrency } = params;
    const { isCreditInvoice } = context;

    // Create base row
    const row = AccountingRowFactory.createBaseRow({
      accountingRows: context.accountingRows,
      voucherDate: context.voucherDate,
    });

    // Apply business logic
    this.configureRow(row, params, isCreditInvoice);
    this.setRowAmounts(row, amount, amountBaseCurrency);

    row.dim1Id =
      params.dim1Id ??
      (this.resolveDim1AccountId(
        type,
        context.vatType,
        context.accountingSettings,
        context.isInterimInvoice,
        context.vatCodeVatAccountId
      ) as number);

    for (const i of DIM_INDEXES) {
      if (i === 1) continue; // Dim1 is already set

      row[`dim${i}Id`] =
        params[`dim${i}Id`] ??
        (this.getAccountingSettingAccountId(
          type,
          i,
          context.accountingSettings
        ) as number);
    }

    // Add to accounting rows
    context.accountingRows.push(row);
    return row;
  }

  /**
   * Generates accounting rows for the given input parameters
   */
  public generateAccountingRows(
    input: GenerateAccountingRowsInput,
    context: CreateAccountingRowContext
  ): IAccountingRowWithDimensionsDTO[] {
    const {
      amountBaseCurrency,
      vatAmountBaseCurrency,
      amountTransactionCurrency,
      vatAmountTransactionCurrency,
    } = input;
    this.createAccountingRow(
      {
        type: SupplierAccountType.Credit,
        amount: amountTransactionCurrency,
        amountBaseCurrency: amountBaseCurrency,
        isDebitRow: false,
        isVatRow: false,
        isContractorVatRow: false,
      },
      context
    );

    this.createVatRows(
      vatAmountTransactionCurrency,
      vatAmountBaseCurrency,
      context
    );

    const debitAmount =
      context.vatType === TermGroup_InvoiceVatType.Merchandise
        ? amountTransactionCurrency - vatAmountTransactionCurrency
        : amountTransactionCurrency;

    const debitAmountBaseCurrency =
      context.vatType === TermGroup_InvoiceVatType.Merchandise
        ? amountBaseCurrency - vatAmountBaseCurrency
        : amountBaseCurrency;

    this.createCostRows(
      debitAmount,
      debitAmountBaseCurrency,
      input.costRows,
      context
    );

    return context.accountingRows;
  }

  private createCostRows(
    debitAmount: number,
    debitAmountBaseCurrency: number,
    costRows: CreateAccountingRowParams[] | null,
    context: CreateAccountingRowContext
  ) {
    if (!costRows) {
      this.createAccountingRow(
        {
          type: SupplierAccountType.Debit,
          amount: debitAmount,
          amountBaseCurrency: debitAmountBaseCurrency,
          isDebitRow: true,
        },
        context
      );
      return;
    }

    if (costRows.length === 1) {
      const costRow = costRows[0];
      costRow.amount = debitAmount;
      costRow.amountBaseCurrency = debitAmountBaseCurrency;
      this.createAccountingRow(costRow, context);
      return;
    }

    costRows.forEach(row => {
      this.createAccountingRow(row, context);
    });
  }

  private createVatRows(
    vatAmount: number,
    vatAmountBaseCurrency: number,
    createRowContext: CreateAccountingRowContext
  ) {
    switch (createRowContext.vatType) {
      case TermGroup_InvoiceVatType.Contractor:
      case TermGroup_InvoiceVatType.EU:
      case TermGroup_InvoiceVatType.NonEU:
        return this.createEqualizingVatRows(
          vatAmount,
          vatAmountBaseCurrency,
          createRowContext
        );
      case TermGroup_InvoiceVatType.Merchandise:
        return this.createAccountingRow(
          {
            type: SupplierAccountType.VAT,
            amount: vatAmount,
            amountBaseCurrency: vatAmountBaseCurrency,
            isDebitRow: true,
            isVatRow: true,
          },
          createRowContext
        );
    }
  }

  createEqualizingVatRows(
    vatAmount: number,
    amountBaseCurrency: number,
    context: CreateAccountingRowContext
  ) {
    this.createAccountingRow(
      {
        type: SupplierAccountType.Unknown,
        dim1Id: this.getCreditVatAccountId(context.vatType, context.vatRate),
        amount: vatAmount,
        amountBaseCurrency: amountBaseCurrency,
        isDebitRow: false,
        isVatRow: true,
        isContractorVatRow:
          context.vatType === TermGroup_InvoiceVatType.Contractor,
      },
      context
    );
    this.createAccountingRow(
      {
        type: SupplierAccountType.Unknown,
        dim1Id: this.getDebitVatAccountId(context.vatType),
        amount: vatAmount,
        amountBaseCurrency: amountBaseCurrency,
        isDebitRow: true,
        isVatRow: true,
        isContractorVatRow:
          context.vatType === TermGroup_InvoiceVatType.Contractor,
      },
      context
    );
  }

  // ============================================================================
  // PRIVATE HELPER METHODS
  // ============================================================================

  private configureRow(
    row: IAccountingRowWithDimensionsDTO,
    params: CreateAccountingRowParams,
    isCreditInvoice: boolean
  ): void {
    const { isDebitRow, isVatRow, isContractorVatRow, text } = params;

    const actualIsDebitRow = isCreditInvoice ? !isDebitRow : isDebitRow;
    row.text = text || '';
    row.isDebitRow = actualIsDebitRow;
    row.isCreditRow = !actualIsDebitRow;
    row.isVatRow = isVatRow ?? false;
    row.isContractorVatRow = isContractorVatRow ?? false;
    // Note: isInterimRow would need additional context to determine
  }

  private setRowAmounts(
    row: IAccountingRowWithDimensionsDTO,
    amount: number,
    amountBaseCurrency: number
  ): void {
    const absAmount = Math.abs(amount);
    const absAmountBaseCurrency = Math.abs(amountBaseCurrency);

    row.debitAmountCurrency = row.isDebitRow ? absAmount : 0;
    row.creditAmountCurrency = row.isDebitRow ? 0 : absAmount;
    row.amountCurrency = row.isDebitRow ? absAmount : -absAmount;

    row.debitAmount = row.isDebitRow ? absAmountBaseCurrency : 0;
    row.creditAmount = row.isDebitRow ? 0 : absAmountBaseCurrency;
    row.amount = row.isDebitRow
      ? absAmountBaseCurrency
      : -absAmountBaseCurrency;
  }

  private resolveDim1AccountId(
    accountType: SupplierAccountType,
    vatType: TermGroup_InvoiceVatType,
    supplierSettings: IAccountingSettingsRowDTO[],
    isInterimInvoice: boolean,
    vatCodeVatAccountId?: number
  ) {
    const resolvedAccountType =
      accountType === SupplierAccountType.Debit && isInterimInvoice
        ? SupplierAccountType.Interim
        : accountType;

    const fromSupplierSetting = this.getAccountingSettingAccountId(
      resolvedAccountType,
      1,
      supplierSettings
    );

    if (fromSupplierSetting) return fromSupplierSetting;

    switch (resolvedAccountType) {
      case SupplierAccountType.Debit:
        return this.getDebitPurchaseAccountId(vatType, isInterimInvoice);
      case SupplierAccountType.VAT:
        return vatCodeVatAccountId ?? this.getDebitVatAccountId(vatType);
      case SupplierAccountType.Credit:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountSupplierDebt
        );
      case SupplierAccountType.Interim:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountSupplierInterim
        );
    }
    return undefined;
  }

  /**
   * Gets account ID from accounting settings
   */
  private getAccountingSettingAccountId(
    type: SupplierAccountType,
    dimNr: DimIndex,
    supplierSettings: IAccountingSettingsRowDTO[]
  ) {
    if (!supplierSettings) return undefined;

    const setting = supplierSettings.find(s => s.type === type);
    if (!setting) return undefined;

    return setting ? setting[`account${dimNr}Id`] : undefined;
  }

  /**
   * Gets debit account ID with special handling for interim invoices and VAT types
   */
  private getDebitPurchaseAccountId(
    vatType: TermGroup_InvoiceVatType,
    isInterimInvoice: boolean
  ): number | undefined {
    const interimAccountId = this.settingService.getIntSetting(
      CompanySettingType.AccountSupplierInterim
    );
    if (isInterimInvoice && interimAccountId) {
      return interimAccountId;
    }

    switch (vatType) {
      case TermGroup_InvoiceVatType.Contractor:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountCommonReverseVatPurchase
        );
      case TermGroup_InvoiceVatType.EU:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountCommonVatPurchaseEUImport
        );
      case TermGroup_InvoiceVatType.NonEU:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountCommonVatPurchaseNonEUImport
        );
      default:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountSupplierPurchase
        );
    }
  }

  private getDebitVatAccountId(vatType: TermGroup_InvoiceVatType) {
    switch (vatType) {
      case TermGroup_InvoiceVatType.EU:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountCommonVatReceivableEUImport
        );
      case TermGroup_InvoiceVatType.NonEU:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountCommonVatReceivableNonEUImport
        );
      case TermGroup_InvoiceVatType.Contractor:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountCommonVatReceivableReversed
        );
      case TermGroup_InvoiceVatType.Merchandise:
        return this.settingService.getIntSetting(
          CompanySettingType.AccountCommonVatReceivable
        );
    }
    return undefined;
  }

  private getCreditVatAccountId(
    vatType: TermGroup_InvoiceVatType,
    vatRate: number
  ) {
    switch (vatType) {
      case TermGroup_InvoiceVatType.Contractor:
        switch (vatRate) {
          case 6:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable3Reversed
            );
          case 12:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable2Reversed
            );
          default:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable1Reversed
            );
        }

      case TermGroup_InvoiceVatType.EU:
        switch (vatRate) {
          case 6:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable3EUImport
            );
          case 12:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable2EUImport
            );
          default:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable1EUImport
            );
        }

      case TermGroup_InvoiceVatType.NonEU:
        switch (vatRate) {
          case 6:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable3NonEUImport
            );
          case 12:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable2NonEUImport
            );
          default:
            return this.settingService.getIntSetting(
              CompanySettingType.AccountCommonVatPayable1NonEUImport
            );
        }
    }
    return undefined;
  }
}
