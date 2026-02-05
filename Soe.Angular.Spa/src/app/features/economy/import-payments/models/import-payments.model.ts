import {
  ISaveCustomerPaymentImportIODTOModel,
  ISavePaymentImportIODTOModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import {
  ImportPaymentIOState,
  ImportPaymentIOStatus,
  ImportPaymentType,
  SoeEntityState,
  TermGroup_BillingType,
  TermGroup_ImportPaymentType,
  TermGroup_SysPaymentMethod,
  TermGroup_SysPaymentType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IPaymentImportRowsDto } from '@shared/models/generated-interfaces/PaymentImportDTO';
import {
  IPaymentImportDTO,
  IPaymentImportIODTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class PaymentImportDTO implements IPaymentImportDTO {
  paymentImportId!: number;
  actorCompanyId?: number;
  batchId!: number;
  sysPaymentTypeId: TermGroup_SysPaymentType = TermGroup_SysPaymentType.Unknown;
  type!: number;
  totalAmount!: number;
  numberOfPayments!: number;
  importDate!: Date;
  filename!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState = SoeEntityState.Active;
  importType: ImportPaymentType = ImportPaymentType.None;
  importPaymentTypeTermId: TermGroup_ImportPaymentType = TermGroup_ImportPaymentType.None;
  statusName!: string;
  transferStatus?: number;
  paymentLabel!: string;
  typeName!: string;
  paymentMethodName!: string;
  transferStateIcon!: string;
  transferStateIconText!: string;
  transferMsg!: string;
  transferStateIconState!: string;
  transferStateIconClass!: string;
  showTransferStatusIcon!: boolean;
}

export class PaymentImportIODTO implements IPaymentImportIODTO {
  paymentImportIOId!: number;
  actorCompanyId!: number;
  batchNr!: number;
  type: TermGroup_BillingType = TermGroup_BillingType.None;
  customerId?: number;
  customer!: string;
  invoiceId?: number;
  invoiceNr!: string;
  invoiceAmount?: number;
  restAmount?: number;
  paidAmount?: number;
  currency!: string;
  invoiceDate?: Date;
  paidDate?: Date;
  dueDate?: Date;
  matchCodeId?: number;
  status: ImportPaymentIOStatus = ImportPaymentIOStatus.None;
  statusId: ImportPaymentIOStatus = ImportPaymentIOStatus.None;
  state: ImportPaymentIOState = ImportPaymentIOState.Open;
  invoiceSeqnr!: string;
  paidAmountCurrency?: number;
  importType?: ImportPaymentType;
  paymentRowId?: number;
  paymentRowSeqNr?: number;
  ocr!: string;
  isSelected!: boolean;
  isFullyPaid!: boolean;
  isVisible!: boolean;
  amountDiff!: number;
  typeName!: string;
  statusName!: string;
  stateName!: string;
  paymentTypeName!: string;
  matchCodeName!: string;
  comment!: string;
  // Extension
  tempRowId!: number;
  isSelectDisabled!: boolean;
  isModified!: boolean;
}

export class SaveCustomerPaymentImportIODTOModel
  implements ISaveCustomerPaymentImportIODTOModel
{
  items!: IPaymentImportIODTO[];
  bulkPayDate!: Date;
  accountYearId!: number;
  paymentMethodId!: number;
  constructor(
    items: IPaymentImportIODTO[],
    bulkPayDate: Date,
    accountYearId: number,
    paymentMethodId: number
  ) {
    this.items = items;
    this.bulkPayDate = bulkPayDate;
    this.accountYearId = accountYearId;
    this.paymentMethodId = paymentMethodId;
  }
}

export class SavePaymentImportIODTOModel
  implements ISavePaymentImportIODTOModel
{
  items!: IPaymentImportIODTO[];
  bulkPayDate!: Date;
  accountYearId!: number;
  constructor(
    items: IPaymentImportIODTO[],
    bulkPayDate: Date,
    accountYearId: number
  ) {
    this.items = items;
    this.bulkPayDate = bulkPayDate;
    this.accountYearId = accountYearId;
  }
}
export class PaymentImportRowsDto implements IPaymentImportRowsDto {
  paymentIOType!: TermGroup_SysPaymentMethod;
  paymentMethodId!: number;
  contents!: number[][];
  base64String!: string;
  fileName!: string;
  batchId!: number;
  paymentImportId!: number;
  importType!: ImportPaymentType;
}

export enum PaymentImportUpdateFunctions {
  UpdatePayment = 1,
  UpdateStatus = 2,
}

export class CrudResponse implements CrudResponse {
  booleanValue!: boolean;
  booleanValue2!: boolean;
  canUserOverride!: boolean;
  dateTimeValue!: string;
  decimalValue!: number;
  errorMessage?: string;
  errorNumber?: number;
  integerValue!: number;
  integerValue2!: number;
  modified!: string;
  objectsAffected!: number;
  success!: boolean;
  successNumber!: number;
  infoMessage!: string;
  stringValue!: string;
}

export enum DefaultDurationSelection {
  All = 99,
}
