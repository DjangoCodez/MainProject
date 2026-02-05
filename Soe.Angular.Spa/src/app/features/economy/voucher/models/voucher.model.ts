import { IAccountingRowDTO } from '@shared/models/generated-interfaces/AccountingRowDTO';
import {
  ICalculateAccountBalanceForAccountsFromVoucherModel,
  IEditVoucherNrModel,
  ISaveVoucherModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import {
  InvoiceAttachmentSourceType,
  SoeDataStorageRecordType,
  SoeEntityState,
  TermGroup_AccountStatus,
  TermGroup_VoucherHeadSourceType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDTO,
  IAccountInternalDTO,
  IFileUploadDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  IVoucherGridDTO,
  IVoucherHeadDTO,
} from '@shared/models/generated-interfaces/VoucherHeadDTOs';
import { IVoucherRowDTO } from '@shared/models/generated-interfaces/VoucherRowDTOs';
import { DateUtil } from '@shared/util/date-util';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { ISaveUserCompanySettingModel } from '@shared/models/generated-interfaces/TimeModels';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

export class VoucherHeadDTO implements IVoucherHeadDTO {
  voucherHeadId!: number;
  voucherSeriesId!: number;
  accountPeriodId!: number;
  actorCompanyId!: number;
  voucherNr!: number;
  date!: Date;
  text!: string;
  template!: boolean;
  typeBalance!: boolean;
  vatVoucher!: boolean;
  companyGroupVoucher!: boolean;
  status!: TermGroup_AccountStatus;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  note!: string;
  sourceType!: TermGroup_VoucherHeadSourceType;
  voucherSeriesTypeId!: number;
  voucherSeriesTypeName!: string;
  voucherSeriesTypeNr!: number;
  sourceTypeName!: string;
  rows!: IVoucherRowDTO[];
  isSelected!: boolean;
  accountIds!: number[];
  accountIdsHandled!: boolean;
  accountYearId!: number;
  budgetAccountId!: number;

  //Extension
  templateId!: number;
}

export class VoucherGridFilterDTO {
  voucherSeriesTypeId!: number;
  accountYearId!: number;
}

export class VoucherGridDTO implements IVoucherGridDTO {
  voucherHeadId!: number;
  voucherNr!: number;
  date!: Date;
  text!: string;
  vatVoucher!: boolean;
  voucherSeriesTypeId!: number;
  voucherSeriesTypeName!: string;
  sourceType!: number;
  sourceTypeName!: string;
  modified?: Date;
  hasHistoryRows!: boolean;
  isSelected!: boolean;
  hasDocuments!: boolean;
  hasNoRows!: boolean;
  hasUnbalancedRows!: boolean;

  expander!: string;

  modifiedTooltip!: string;
  modifiedIconValue!: string;
  modifiedIconClass!: string;

  hasDocumentsTooltip!: string;
  hasDocumentsIconValue!: string;

  accRowsIconValue: string | undefined;
  accRowsIconClass!: string;
  accRowsText: string | undefined;

  fixDates() {
    this.date = DateUtil.parseDateOrJson(this.date) || DateUtil.getToday();
    this.modified = DateUtil.parseDateOrJson(this.modified);
  }
}

export class VoucherRowDTO implements IVoucherRowDTO {
  voucherRowId!: number;
  voucherHeadId!: number;
  parentRowId?: number;
  accountDistributionHeadId?: number;
  date?: Date;
  text!: string;
  quantity?: number;
  amount!: number;
  amountEntCurrency!: number;
  merged!: boolean;
  state: SoeEntityState = 0;
  voucherNr!: number;
  voucherSeriesTypeNr!: number;
  voucherSeriesTypeName!: string;
  tempRowId!: number;
  dim1Id!: number;
  dim1Nr!: string;
  dim1Name!: string;
  dim1UnitStop!: boolean;
  dim1AmountStop!: number;
  dim2Id!: number;
  dim2Nr!: string;
  dim2Name!: string;
  dim3Id!: number;
  dim3Nr!: string;
  dim3Name!: string;
  dim4Id!: number;
  dim4Nr!: string;
  dim4Name!: string;
  dim5Id!: number;
  dim5Nr!: string;
  dim5Name!: string;
  dim6Id!: number;
  dim6Nr!: string;
  dim6Name!: string;
  rowNr?: number;
  sysVatAccountId?: number;
  dim1AccountType!: number;
  accountInternalDTO_forReports!: IAccountInternalDTO[];
  amountCredit: number = 0;
  amountDebet: number = 0;
  startDate?: Date;
  numberOfPeriods?: number;

  public static toAccountingRowDTO(row: IVoucherRowDTO): AccountingRowDTO {
    const dto: AccountingRowDTO = {} as AccountingRowDTO;

    dto.invoiceRowId = row.voucherRowId;
    dto.tempInvoiceRowId = row.voucherRowId;
    dto.tempRowId = row.voucherRowId;
    dto.voucherHeadId = row.voucherHeadId;
    dto.accountDistributionHeadId = row.accountDistributionHeadId
      ? row.accountDistributionHeadId
      : 0;
    dto.parentRowId = row.parentRowId ? row.parentRowId : 0;
    dto.date = row.date;
    dto.text = row.text;
    dto.quantity = row.quantity;
    dto.amount = row.amount;
    dto.amountEntCurrency = row.amountEntCurrency;
    dto.creditAmount = row.amount < 0 ? Math.abs(row.amount) : 0;
    dto.creditAmountEntCurrency =
      row.amountEntCurrency < 0 ? Math.abs(row.amountEntCurrency) : 0;
    dto.creditAmountCurrency = row.amount < 0 ? Math.abs(row.amount) : 0; // CreditAmountCurrency missing in VoucherRow
    dto.creditAmountLedgerCurrency = row.amount < 0 ? Math.abs(row.amount) : 0; // CreditAmountLedgerCurrency missing in VoucherRow
    dto.debitAmount = row.amount > 0 ? row.amount : 0;
    dto.debitAmountEntCurrency =
      row.amountEntCurrency > 0 ? row.amountEntCurrency : 0;
    dto.debitAmountCurrency = row.amount > 0 ? row.amount : 0; // DebitAmountCurrency missing in VoucherRow
    dto.debitAmountLedgerCurrency = row.amount > 0 ? row.amount : 0; // CreditAmountLedgerCurrency missing in VoucherRow
    dto.isCreditRow = row.amount < 0;
    dto.isDebitRow = row.amount > 0;
    dto.isTemplateRow = false;
    dto.state = row.state;
    dto.rowNr = row.rowNr ? row.rowNr : 0;
    dto.startDate = row.startDate;
    dto.numberOfPeriods = row.numberOfPeriods;

    // Standard account
    dto.dim1Id = row.dim1Id;
    dto.dim1Nr = row.dim1Nr;
    dto.dim1Name = row.dim1Name;
    dto.dim1Disabled = false;
    dto.dim1Mandatory = true;
    dto.quantityStop = row.dim1UnitStop;
    dto.amountStop = row.dim1AmountStop;

    // Internal accounts (dim 2-6)
    dto.dim2Id = row.dim2Id;
    dto.dim2Nr = row.dim2Nr ? row.dim2Nr : '';
    dto.dim2Name = row.dim2Name ? row.dim2Name : '';
    dto.dim3Id = row.dim3Id;
    dto.dim3Nr = row.dim3Nr ? row.dim3Nr : '';
    dto.dim3Name = row.dim3Name ? row.dim3Name : '';
    dto.dim4Id = row.dim4Id;
    dto.dim4Nr = row.dim4Nr ? row.dim4Nr : '';
    dto.dim4Name = row.dim4Name ? row.dim4Name : '';
    dto.dim5Id = row.dim5Id;
    dto.dim5Nr = row.dim5Nr ? row.dim5Nr : '';
    dto.dim5Name = row.dim5Name ? row.dim5Name : '';
    dto.dim6Id = row.dim6Id;
    dto.dim6Nr = row.dim6Nr ? row.dim6Nr : '';
    dto.dim6Name = row.dim6Name ? row.dim6Name : '';

    return dto;
  }

  public static toAccountingRowDTOs(
    rows: IVoucherRowDTO[]
  ): AccountingRowDTO[] {
    const dtos: AccountingRowDTO[] = [];

    rows
      .sort((a, b) =>
        (a.rowNr ? a.rowNr : 0) < (b.rowNr ? b.rowNr : 0)
          ? 1
          : a.rowNr === b.rowNr
            ? 0
            : -1
      )
      .forEach(row => {
        dtos.push(this.toAccountingRowDTO(row));
      });

    return dtos;
  }
}

export class AccountInternalDTO implements IAccountInternalDTO {
  accountId!: number;
  accountNr!: string;
  name!: string;
  accountDimId!: number;
  accountDimNr!: number;
  sysSieDimNr?: number;
  sysSieDimNrOrAccountDimNr!: number;
  mandatoryLevel!: number;
  useVatDeduction!: boolean;
  vatDeduction!: number;
  account!: IAccountDTO;
}

export class EditVoucherNrModel implements IEditVoucherNrModel {
  voucherHeadId!: number;
  newVoucherNr!: number;
  constructor(voucherHeadId: number, newVoucherNr: number) {
    this.voucherHeadId = voucherHeadId;
    this.newVoucherNr = newVoucherNr;
  }
}
export class FileUploadDTO implements IFileUploadDTO {
  id?: number;
  imageId?: number;
  invoiceAttachmentId?: number;
  isSupplierInvoice!: boolean;
  fileName!: string;
  description!: string;
  isDeleted!: boolean;
  includeWhenDistributed!: boolean;
  includeWhenTransfered!: boolean;
  dataStorageRecordType?: SoeDataStorageRecordType;
  sourceType?: InvoiceAttachmentSourceType;

  // fileRecordId: number;
  // recordId: number;
  // entity: SoeEntityType;
  // created?: Date;
  // createdBy: string;
  // modified?: Date;
  // modifiedBy: string;
  // fileId: number;
  // fileName: string;
  // description: string;
  // extension: string;
  // fileSize?: number;
  // actorCompanyId: number;
  // type: SoeDataStorageRecordType;
  // data: number[];

  // // Extensions
  // isModified: boolean | undefined;

  static fromCrudResponse(response: BackendResponse): FileUploadDTO {
    return {
      ...new FileUploadDTO(),
      id: ResponseUtil.getEntityId(response),
      description: ResponseUtil.getStringValue(response),
      fileName: ResponseUtil.getStringValue(response),
    };
  }
}

export class SaveVoucherModel implements ISaveVoucherModel {
  voucherHead!: IVoucherHeadDTO;
  accountingRows!: IAccountingRowDTO[];
  householdRowIds!: number[];
  revertVatVoucherId?: number;
  files!: IFileUploadDTO[];
  constructor(
    voucherHead: IVoucherHeadDTO,
    accountingRows: IAccountingRowDTO[],
    householdRowIds: number[],
    files: FileUploadDTO[],
    revertVatVoucherId: number
  ) {
    this.voucherHead = voucherHead;
    this.accountingRows = accountingRows;
    this.householdRowIds = householdRowIds;
    this.files = files;
    this.revertVatVoucherId = revertVatVoucherId;
  }
}
export class CalculateAccountBalanceForAccountsFromVoucherModel
  implements ICalculateAccountBalanceForAccountsFromVoucherModel
{
  accountYearId!: number;
  /**
   *
   */
  constructor(accountYearId: number) {
    this.accountYearId = accountYearId;
  }
}

export class SaveUserCompanySettingModel
  implements ISaveUserCompanySettingModel
{
  settingMainType!: number;
  settingTypeId!: number;
  boolValue!: boolean;
  intValue!: number;
  stringValue!: string;
  constructor(
    settingMainType: number,
    settingTypeId: number,
    intValue: number
  ) {
    this.settingMainType = settingMainType;
    this.settingTypeId = settingTypeId;
    this.intValue = intValue;
  }
}
