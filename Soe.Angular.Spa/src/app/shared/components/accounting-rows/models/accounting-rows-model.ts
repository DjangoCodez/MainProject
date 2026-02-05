import {
  IAccountDimensionsDTO,
  IAccountingRowDTO,
} from '@shared/models/generated-interfaces/AccountingRowDTO';
import {
  AccountingRowType,
  SoeEntityState,
  TermGroup_CurrencyType,
  VoucherRowMergeType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDTO,
  IAccountDimDTO,
  IAccountDimSmallDTO,
  IAccountInternalDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { NumberUtil } from '@shared/util/number-util';
import { BehaviorSubject } from 'rxjs';

export enum AmountStop {
  DebitAmountStop = 1,
  CreditAmountStop = 2,
}

export type SteppingRuleFunction = (row: AccountingRowDTO) => boolean;

export interface SteppingRules {
  [key: string]: SteppingRuleFunction;
}

export class AccountDimSmallDTO implements IAccountDimSmallDTO {
  mandatoryInOrder!: boolean;
  mandatoryInCustomerInvoice!: boolean;
  accountDimId!: number;
  accountDimNr!: number;
  name!: string;
  parentAccountDimId?: number;
  linkedToShiftType!: boolean;
  accounts!: AccountDTO[];
  level!: number;
  isAboveCompanyStdSetting!: boolean;
  currentSelectableAccounts!: IAccountDTO[];
}

export class AccountDTO implements IAccountDTO {
  accountId: number;
  accountDimId: number;
  accountNr: string;
  parentAccountId?: number;
  name: string;
  description: string;
  externalCode: string;
  hierarchyOnly: boolean;
  accountTypeSysTermId: number;
  hierarchyNotOnSchedule: boolean;
  accountDim!: IAccountDimDTO;
  accountDimNr: number;
  numberName: string;
  dimNameNumberAndName: string;
  amountStop: number;
  unitStop: boolean;
  unit: string;
  rowTextStop: boolean;
  grossProfitCode: number[];
  attestWorkFlowHeadId?: number;
  state: SoeEntityState;
  isAccrualAccount: boolean;
  accountInternals: IAccountInternalDTO[];
  accountIdWithDelimeter: string;
  isAbstract: boolean;
  hasVirtualParent: boolean;
  virtualParentAccountId?: number;
  parentAccounts: IAccountDTO[];
  parentHierachy!: Record<number, string>;
  noOParentHierachys: number;
  hierachyId: string;
  hierachyName: string;
  accountHierarchyUniqueId: string;

  constructor() {
    this.accountId = 0;
    this.accountDimId = 0;
    this.accountNr = '';
    this.name = '';
    this.description = '';
    this.externalCode = '';
    this.hierarchyOnly = false;
    this.accountTypeSysTermId = 0;
    this.hierarchyNotOnSchedule = false;
    this.accountDimNr = 0;
    this.numberName = '';
    this.dimNameNumberAndName = '';
    this.amountStop = 0;
    this.unitStop = false;
    this.unit = '';
    this.rowTextStop = false;
    this.grossProfitCode = [];
    this.state = SoeEntityState.Active;
    this.isAccrualAccount = false;
    this.accountInternals = [];
    this.accountIdWithDelimeter = '';
    this.isAbstract = false;
    this.hasVirtualParent = false;
    this.parentAccounts = [];
    this.noOParentHierachys = 0;
    this.hierachyId = '';
    this.hierachyName = '';
    this.accountHierarchyUniqueId = '';
  }
}

export class AccountingRowDTO
  implements IAccountingRowDTO, IAccountDimensionsDTO
{
  type!: AccountingRowType;
  invoiceRowId!: number;
  invoiceAccountRowId!: number;
  tempRowId!: number;
  tempInvoiceRowId!: number;
  parentRowId!: number;
  voucherRowId?: number;
  invoiceId!: number;
  rowNr!: number;
  productRowNr!: number;
  productName!: string;
  quantity?: number;
  text!: string;
  date?: Date;
  unit!: string;
  quantityStop!: boolean;
  rowTextStop!: boolean;
  amountStop!: number;
  debitAmount!: number;
  debitAmountCurrency!: number;
  debitAmountEntCurrency!: number;
  debitAmountLedgerCurrency!: number;
  creditAmount!: number;
  creditAmountCurrency!: number;
  creditAmountEntCurrency!: number;
  creditAmountLedgerCurrency!: number;
  amount!: number;
  amountCurrency!: number;
  amountEntCurrency!: number;
  amountLedgerCurrency!: number;
  balance!: number;
  splitType!: number;
  splitValue!: number;
  splitPercent!: number;
  accountDistributionHeadId!: number;
  accountDistributionNbrOfPeriods!: number;
  accountDistributionStartDate?: Date;
  inventoryId!: number;
  attestStatus!: number;
  attestUserId?: number;
  attestUserName!: string;
  isCreditRow!: boolean;
  isDebitRow!: boolean;
  isVatRow!: boolean;
  isContractorVatRow!: boolean;
  isCentRoundingRow!: boolean;
  isInterimRow!: boolean;
  isTemplateRow!: boolean;
  isClaimRow!: boolean;
  isHouseholdRow!: boolean;
  voucherRowMergeType!: VoucherRowMergeType;
  mergeSign!: number;
  isModified!: boolean;
  isDeleted!: boolean;
  isProcessed!: boolean;
  isManuallyAdjusted!: boolean;
  state!: SoeEntityState;
  startDate?: Date;
  numberOfPeriods?: number;

  dim1Id!: number;
  dim1Nr!: string;
  dim1Name!: string;
  dim1Disabled!: boolean;
  dim1Mandatory!: boolean;
  dim1Stop!: boolean;
  dim1ManuallyChanged!: boolean;
  dim2Id!: number;
  dim2Nr!: string;
  dim2Name!: string;
  dim2Disabled!: boolean;
  dim2Mandatory!: boolean;
  dim2Stop!: boolean;
  dim2ManuallyChanged!: boolean;
  dim3Id!: number;
  dim3Nr!: string;
  dim3Name!: string;
  dim3Disabled!: boolean;
  dim3Mandatory!: boolean;
  dim3Stop!: boolean;
  dim3ManuallyChanged!: boolean;
  dim4Id!: number;
  dim4Nr!: string;
  dim4Name!: string;
  dim4Disabled!: boolean;
  dim4Mandatory!: boolean;
  dim4Stop!: boolean;
  dim4ManuallyChanged!: boolean;
  dim5Id!: number;
  dim5Nr!: string;
  dim5Name!: string;
  dim5Disabled!: boolean;
  dim5Mandatory!: boolean;
  dim5Stop!: boolean;
  dim5ManuallyChanged!: boolean;
  dim6Id!: number;
  dim6Nr!: string;
  dim6Name!: string;
  dim6Disabled!: boolean;
  dim6Mandatory!: boolean;
  dim6Stop!: boolean;
  dim6ManuallyChanged!: boolean;

  // Extensions
  isAccrualAccount!: boolean;
  voucherHeadId!: number;
  isReadOnly!: boolean;
  isAttestReadOnly!: boolean;

  //used to show errors in the grid.
  dim1Error!: string;
  dim2Error!: string;
  dim3Error!: string;
  dim4Error!: string;
  dim5Error!: string;
  dim6Error!: string;

  orgCreditAmount!: number;
  orgDebetAmount!: number;

  constructor(obj: AccountingRowDTO) {
    this.type = obj.type;
    this.invoiceRowId = obj.invoiceRowId;
    this.invoiceAccountRowId = obj.invoiceAccountRowId;
    this.tempRowId = obj.tempRowId;
    this.tempInvoiceRowId = obj.tempInvoiceRowId;
    this.parentRowId = obj.parentRowId;
    this.voucherRowId = obj?.voucherRowId;
    this.invoiceId = obj.invoiceId;
    this.rowNr = obj.rowNr;
    this.productRowNr = obj.productRowNr;
    this.productName = obj.productName;
    this.quantity = obj.quantity;
    this.text = obj.text;
    this.date = obj.date;
    this.unit = obj.unit;
    this.quantityStop = obj.quantityStop;
    this.rowTextStop = obj.rowTextStop;
    this.amountStop = obj.amountStop;
    this.debitAmount = obj.debitAmount;
    this.debitAmountCurrency = obj.debitAmountCurrency;
    this.debitAmountEntCurrency = obj.debitAmountEntCurrency;
    this.debitAmountLedgerCurrency = obj.debitAmountLedgerCurrency;
    this.creditAmount = obj.creditAmount;
    this.creditAmountCurrency = obj.creditAmountCurrency;
    this.creditAmountEntCurrency = obj.creditAmountEntCurrency;
    this.creditAmountLedgerCurrency = obj.creditAmountLedgerCurrency;
    this.amount = obj.amount;
    this.amountCurrency = obj.amountCurrency;
    this.amountEntCurrency = obj.amountEntCurrency;
    this.amountLedgerCurrency = obj.amountLedgerCurrency;
    this.balance = obj.balance;
    this.splitType = obj.splitType;
    this.splitValue = obj.splitValue;
    this.splitPercent = obj.splitPercent;
    this.accountDistributionHeadId = obj.accountDistributionHeadId;
    this.accountDistributionNbrOfPeriods = obj.accountDistributionNbrOfPeriods;
    this.accountDistributionStartDate = obj.accountDistributionStartDate;
    this.inventoryId = obj.inventoryId;
    this.attestStatus = obj.attestStatus;
    this.attestUserId = obj.attestUserId;
    this.attestUserName = obj.attestUserName;
    this.isCreditRow = obj.isCreditRow;
    this.isDebitRow = obj.isDebitRow;
    this.isVatRow = obj.isVatRow;
    this.isContractorVatRow = obj.isContractorVatRow;
    this.isCentRoundingRow = obj.isCentRoundingRow;
    this.isInterimRow = obj.isInterimRow;
    this.isTemplateRow = obj.isTemplateRow;
    this.isClaimRow = obj.isClaimRow;
    this.isHouseholdRow = obj.isHouseholdRow;
    this.voucherRowMergeType = obj.voucherRowMergeType;
    this.mergeSign = obj.mergeSign;
    this.isModified = obj.isModified;
    this.isDeleted = obj.isDeleted;
    this.isProcessed = obj.isProcessed;
    this.isManuallyAdjusted = obj.isManuallyAdjusted;
    this.state = obj.state;
    this.dim1Id = obj.dim1Id;
    this.dim1Nr = obj.dim1Nr;
    this.dim1Name = obj.dim1Name;
    this.dim1Disabled = obj.dim1Disabled;
    this.dim1Mandatory = obj.dim1Mandatory;
    this.dim1Stop = obj.dim1Stop;
    this.dim1ManuallyChanged = obj.dim1ManuallyChanged;
    this.dim2Id = obj.dim2Id;
    this.dim2Nr = obj.dim2Nr;
    this.dim2Name = obj.dim2Name;
    this.dim2Disabled = obj.dim2Disabled;
    this.dim2Mandatory = obj.dim2Mandatory;
    this.dim2Stop = obj.dim2Stop;
    this.dim2ManuallyChanged = obj.dim2ManuallyChanged;
    this.dim3Id = obj.dim3Id;
    this.dim3Nr = obj.dim3Nr;
    this.dim3Name = obj.dim3Name;
    this.dim3Disabled = obj.dim3Disabled;
    this.dim3Mandatory = obj.dim3Mandatory;
    this.dim3Stop = obj.dim3Stop;
    this.dim3ManuallyChanged = obj.dim3ManuallyChanged;
    this.dim4Id = obj.dim4Id;
    this.dim4Nr = obj.dim4Nr;
    this.dim4Name = obj.dim4Name;
    this.dim4Disabled = obj.dim4Disabled;
    this.dim4Mandatory = obj.dim4Mandatory;
    this.dim4Stop = obj.dim4Stop;
    this.dim4ManuallyChanged = obj.dim4ManuallyChanged;
    this.dim5Id = obj.dim5Id;
    this.dim5Nr = obj.dim5Nr;
    this.dim5Name = obj.dim5Name;
    this.dim5Disabled = obj.dim5Disabled;
    this.dim5Mandatory = obj.dim5Mandatory;
    this.dim5Stop = obj.dim5Stop;
    this.dim5ManuallyChanged = obj.dim5ManuallyChanged;
    this.dim6Id = obj.dim6Id;
    this.dim6Nr = obj.dim6Nr;
    this.dim6Name = obj.dim6Name;
    this.dim6Disabled = obj.dim6Disabled;
    this.dim6Mandatory = obj.dim6Mandatory;
    this.dim6Stop = obj.dim6Stop;
    this.dim6ManuallyChanged = obj.dim6ManuallyChanged;
    this.isAccrualAccount = obj.isAccrualAccount;
    this.voucherHeadId = obj.voucherHeadId;
    this.isReadOnly = obj.isReadOnly;
    this.isAttestReadOnly = obj.isAttestReadOnly;
    this.dim1Error = obj.dim1Error;
    this.dim2Error = obj.dim2Error;
    this.dim3Error = obj.dim3Error;
    this.dim4Error = obj.dim4Error;
    this.dim5Error = obj.dim5Error;
    this.dim6Error = obj.dim6Error;
    this.orgCreditAmount = obj.orgCreditAmount;
    this.orgDebetAmount = obj.orgDebetAmount;
    this.startDate = obj.startDate;
    this.numberOfPeriods = obj.numberOfPeriods;
  }

  public static invertAmounts(rows: IAccountingRowDTO[]) {
    rows.forEach(row => {
      row.isDebitRow = !row.isDebitRow;
      row.isCreditRow = !row.isCreditRow;

      const originalDebitAmount = row.debitAmount;
      row.debitAmount = row.creditAmount;
      row.creditAmount = originalDebitAmount;

      const originalDebitAmountCurrency = row.debitAmountCurrency;
      row.debitAmountCurrency = row.creditAmountCurrency;
      row.creditAmountCurrency = originalDebitAmountCurrency;

      const originalDebitAmountEntCurrency = row.debitAmountEntCurrency;
      row.debitAmountEntCurrency = row.creditAmountEntCurrency;
      row.creditAmountEntCurrency = originalDebitAmountEntCurrency;

      const originalDebitAmountLedgerCurrency = row.debitAmountLedgerCurrency;
      row.debitAmountLedgerCurrency = row.creditAmountLedgerCurrency;
      row.creditAmountLedgerCurrency = originalDebitAmountLedgerCurrency;
    });
  }

  public static clearRowIds(rows: IAccountingRowDTO[], keepTempIds: boolean) {
    let counter: number = 1;
    rows.forEach(row => {
      row.invoiceRowId = 0;
      row.invoiceAccountRowId = 0;
      row.isModified = true;
      if (!row.rowNr) row.rowNr = counter;

      if (!keepTempIds) {
        row.tempRowId = 0;
        row.tempInvoiceRowId = 0;
      }
      counter++;
    });
  }
  public static getNextRowNr(rows: IAccountingRowDTO[]) {
    let rowNr = 0;
    const maxRow = NumberUtil.max(rows, 'rowNr');
    if (maxRow) rowNr = maxRow;

    return rowNr + 1;
  }

  public getAmount(currencyType: TermGroup_CurrencyType) {
    let amount: number = 0;

    switch (currencyType) {
      case TermGroup_CurrencyType.BaseCurrency:
        if (typeof this.amount != 'undefined') amount = this.amount;
        else if (typeof this.amountCurrency != 'undefined')
          amount = this.amountCurrency;
        break;
      case TermGroup_CurrencyType.EnterpriseCurrency:
        if (typeof this.amount != 'undefined') amount = this.amount;
        else if (typeof this.amountEntCurrency != 'undefined')
          amount = this.amountEntCurrency;
        break;
      case TermGroup_CurrencyType.LedgerCurrency:
        if (typeof this.amount != 'undefined') amount = this.amount;
        else if (typeof this.amountLedgerCurrency != 'undefined')
          amount = this.amountLedgerCurrency;
        break;
      case TermGroup_CurrencyType.TransactionCurrency:
        if (typeof this.amountCurrency != 'undefined')
          amount = this.amountCurrency;
        else if (typeof this.amount != 'undefined') amount = this.amount;
        break;
    }

    return amount;
  }

  public getDebitAmount(currencyType: TermGroup_CurrencyType) {
    switch (currencyType) {
      case TermGroup_CurrencyType.BaseCurrency:
        return this.debitAmount;
      case TermGroup_CurrencyType.EnterpriseCurrency:
        return this.debitAmountEntCurrency;
      case TermGroup_CurrencyType.LedgerCurrency:
        return this.debitAmountLedgerCurrency;
      case TermGroup_CurrencyType.TransactionCurrency:
        return this.debitAmountCurrency;
      default:
        return 0;
    }
  }

  public getCreditAmount(currencyType: TermGroup_CurrencyType) {
    switch (currencyType) {
      case TermGroup_CurrencyType.BaseCurrency:
        return this.creditAmount;
      case TermGroup_CurrencyType.EnterpriseCurrency:
        return this.creditAmountEntCurrency;
      case TermGroup_CurrencyType.LedgerCurrency:
        return this.creditAmountLedgerCurrency;
      case TermGroup_CurrencyType.TransactionCurrency:
        return this.creditAmountCurrency;
      default:
        return 0;
    }
  }

  public setAmount(currencyType: TermGroup_CurrencyType, amount: number) {
    switch (currencyType) {
      case TermGroup_CurrencyType.BaseCurrency:
        this.amount = amount;
        break;
      case TermGroup_CurrencyType.EnterpriseCurrency:
        this.amountEntCurrency = amount;
        break;
      case TermGroup_CurrencyType.LedgerCurrency:
        this.amountLedgerCurrency = amount;
        break;
      case TermGroup_CurrencyType.TransactionCurrency:
        this.amountCurrency = amount;
        break;
    }

    if (amount < 0) this.setCreditAmount(currencyType, Math.abs(amount), false);
    else this.setDebitAmount(currencyType, amount, false);
  }

  public setDebitAmount(
    currencyType: TermGroup_CurrencyType,
    amount: number,
    updateAmount: boolean = true
  ) {
    switch (currencyType) {
      case TermGroup_CurrencyType.BaseCurrency:
        this.debitAmount = amount;
        break;
      case TermGroup_CurrencyType.EnterpriseCurrency:
        this.debitAmountEntCurrency = amount;
        break;
      case TermGroup_CurrencyType.LedgerCurrency:
        this.debitAmountLedgerCurrency = amount;
        break;
      case TermGroup_CurrencyType.TransactionCurrency:
        this.debitAmountCurrency = amount;
        break;
    }

    if (updateAmount) this.updateAmount();
  }

  public setCreditAmount(
    currencyType: TermGroup_CurrencyType,
    amount: number,
    updateAmount: boolean = true
  ) {
    switch (currencyType) {
      case TermGroup_CurrencyType.BaseCurrency:
        this.creditAmount = amount;
        break;
      case TermGroup_CurrencyType.EnterpriseCurrency:
        this.creditAmountEntCurrency = amount;
        break;
      case TermGroup_CurrencyType.LedgerCurrency:
        this.creditAmountLedgerCurrency = amount;
        break;
      case TermGroup_CurrencyType.TransactionCurrency:
        this.creditAmountCurrency = amount;
        break;
    }

    if (updateAmount) this.updateAmount();
  }

  public updateAmount() {
    this.amount = (this.debitAmount ?? 0) - (this.creditAmount ?? 0);
    this.amountCurrency =
      (this.debitAmountCurrency ?? 0) - (this.creditAmountCurrency ?? 0);
    this.amountEntCurrency =
      (this.debitAmountEntCurrency ?? 0) - (this.creditAmountEntCurrency ?? 0);
    this.amountLedgerCurrency =
      (this.debitAmountLedgerCurrency ?? 0) -
      (this.creditAmountLedgerCurrency ?? 0);
  }
}
