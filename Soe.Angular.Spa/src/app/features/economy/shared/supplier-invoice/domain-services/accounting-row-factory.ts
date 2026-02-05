import {
  AccountingRowType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountingRowDTO,
  IAccountDimensionsDTO,
} from '@shared/models/generated-interfaces/AccountingRowDTO';

// ============================================================================
// INTERFACES
// ============================================================================

export interface IAccountingRowWithDimensionsDTO
  extends IAccountingRowDTO,
    IAccountDimensionsDTO {}

export interface AccountingRowFactoryParams {
  accountingRows: IAccountingRowWithDimensionsDTO[];
  voucherDate?: Date;
}

// ============================================================================
// ACCOUNTING ROW FACTORY
// ============================================================================

export class AccountingRowFactory {
  static createBaseRow(
    params: AccountingRowFactoryParams
  ): IAccountingRowWithDimensionsDTO {
    const nextRowNr = this.getNextRowNr(params.accountingRows);
    const nextTempId = params.accountingRows.length + 1;

    return {
      // Core properties
      type: AccountingRowType.AccountingRow,
      invoiceRowId: 0,
      invoiceAccountRowId: 0,
      tempRowId: nextTempId,
      tempInvoiceRowId: nextTempId,
      parentRowId: 0,
      invoiceId: 0,
      rowNr: nextRowNr,
      productRowNr: 0,
      productName: '',
      quantity: undefined,
      text: '',
      date: params.voucherDate ?? new Date(),
      unit: '',

      // Amount properties
      quantityStop: false,
      rowTextStop: false,
      amountStop: 0,
      debitAmount: 0,
      debitAmountCurrency: 0,
      debitAmountEntCurrency: 0,
      debitAmountLedgerCurrency: 0,
      creditAmount: 0,
      creditAmountCurrency: 0,
      creditAmountEntCurrency: 0,
      creditAmountLedgerCurrency: 0,
      amount: 0,
      amountCurrency: 0,
      amountEntCurrency: 0,
      amountLedgerCurrency: 0,
      balance: 0,

      // Split properties
      splitType: 0,
      splitValue: 0,
      splitPercent: 0,
      accountDistributionHeadId: 0,
      accountDistributionNbrOfPeriods: 0,
      accountDistributionStartDate: undefined,

      // Other properties
      inventoryId: 0,
      attestStatus: 0,
      attestUserId: undefined,
      attestUserName: '',

      // Row type flags
      isCreditRow: false,
      isDebitRow: false,
      isVatRow: false,
      isContractorVatRow: false,
      isCentRoundingRow: false,
      isInterimRow: false,
      isTemplateRow: false,
      isClaimRow: false,
      isHouseholdRow: false,
      voucherRowMergeType: 0,
      mergeSign: 0,
      isModified: false,
      isDeleted: false,
      isProcessed: false,
      isManuallyAdjusted: false,
      state: SoeEntityState.Active,

      // Dimension properties
      ...this.createEmptyDimensions(),
    };
  }

  private static createEmptyDimensions(): IAccountDimensionsDTO {
    return {
      dim1Id: 0,
      dim1Nr: '',
      dim1Name: '',
      dim1Disabled: false,
      dim1Mandatory: false,
      dim1Stop: false,
      dim1ManuallyChanged: false,
      dim2Id: 0,
      dim2Nr: '',
      dim2Name: '',
      dim2Disabled: false,
      dim2Mandatory: false,
      dim2Stop: false,
      dim2ManuallyChanged: false,
      dim3Id: 0,
      dim3Nr: '',
      dim3Name: '',
      dim3Disabled: false,
      dim3Mandatory: false,
      dim3Stop: false,
      dim3ManuallyChanged: false,
      dim4Id: 0,
      dim4Nr: '',
      dim4Name: '',
      dim4Disabled: false,
      dim4Mandatory: false,
      dim4Stop: false,
      dim4ManuallyChanged: false,
      dim5Id: 0,
      dim5Nr: '',
      dim5Name: '',
      dim5Disabled: false,
      dim5Mandatory: false,
      dim5Stop: false,
      dim5ManuallyChanged: false,
      dim6Id: 0,
      dim6Nr: '',
      dim6Name: '',
      dim6Disabled: false,
      dim6Mandatory: false,
      dim6Stop: false,
      dim6ManuallyChanged: false,
    };
  }

  private static getNextRowNr(
    accountingRows: IAccountingRowWithDimensionsDTO[]
  ): number {
    if (accountingRows.length === 0) return 1;
    const maxRowNr = Math.max(...accountingRows.map(row => row.rowNr));
    return maxRowNr + 1;
  }
}
