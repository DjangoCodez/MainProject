import {
  IDeleteDistributionEntryModel,
  ITransferAccountDistributionEntryToVoucherModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import {
  TermGroup_AccountDistributionTriggerType,
  TermGroup_AccountDistributionPeriodType,
  TermGroup_AccountDistributionRegistrationType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDistributionEntryDTO,
  IAccountDistributionEntryRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class TransferToAccountDistributionEntryDTO {
  accountDistributionEntryDTOs!: IAccountDistributionEntryDTO[];
  periodDate!: string;
  accountDistributionType!: number;
}

export class TransferAccountDistributionEntryToVoucherDTO
  implements ITransferAccountDistributionEntryToVoucherModel
{
  accountDistributionEntryDTOs!: IAccountDistributionEntryDTO[];
  accountDistributionType!: number;
  periodDate!: Date;
}

export class AccountDistributionEntryDTO
  implements IAccountDistributionEntryDTO
{
  accountDistributionEntryId: number;
  actorCompanyId: number;
  accountDistributionHeadId?: number;
  accountDistributionHeadName: string;
  triggerType: TermGroup_AccountDistributionTriggerType;
  periodType: TermGroup_AccountDistributionPeriodType;
  date: Date;
  voucherHeadId?: number;
  supplierInvoiceId?: number;
  inventoryId?: number;
  state: number;
  registrationType: TermGroup_AccountDistributionRegistrationType;
  sourceVoucherHeadId?: number;
  sourceVoucherNr: number;
  sourceSupplierInvoiceId?: number;
  sourceSupplierInvoiceSeqNr?: number;
  sourceCustomerInvoiceId?: number;
  sourceCustomerInvoiceSeqNr?: number;
  invoiceNr: string;
  sourceRowId?: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  isSelected: boolean;
  isSelectEnable: boolean;
  periodError: boolean;
  voucherSeriesTypeId?: number;
  typeName: string;
  inventoryName: string;
  inventoryNr: string;
  status: string;
  voucherNr?: number;
  accountYearId?: number;
  amount: number;
  writeOffAmount: number;
  writeOffYear: number;
  writeOffTotal: number;
  writeOffSum: number;
  currentAmount: number;
  detailVisible: boolean;
  rowId: number;
  accountDistributionEntryRowDTO: IAccountDistributionEntryRowDTO[];
  categories: string;
  inventoryPurchaseDate?: Date;
  inventoryWriteOffDate?: Date;
  inventoryDescription: string;
  inventoryNotes: string;
  notesIcon: string;
  sourceSeqNr: number;
  isReversal: boolean;

  constructor() {
    this.accountDistributionEntryId = 0;
    this.actorCompanyId = 0;
    this.accountDistributionHeadName = '';
    this.triggerType = TermGroup_AccountDistributionTriggerType.None;
    this.periodType = TermGroup_AccountDistributionPeriodType.Unknown;
    this.date = new Date();
    this.state = 0;
    this.registrationType = TermGroup_AccountDistributionRegistrationType.None;
    this.invoiceNr = '';
    this.isSelected = false;
    this.isSelectEnable = false;
    this.periodError = false;
    this.typeName = '';
    this.inventoryName = '';
    this.inventoryNr = '';
    this.status = '';
    this.sourceVoucherNr = 0;
    this.amount = 0;
    this.writeOffAmount = 0;
    this.writeOffYear = 0;
    this.writeOffTotal = 0;
    this.writeOffSum = 0;
    this.currentAmount = 0;
    this.detailVisible = false;
    this.rowId = 0;
    this.accountDistributionEntryRowDTO = [];
    this.categories = '';
    this.inventoryDescription = '';
    this.inventoryNotes = '';
    this.notesIcon = '';
    this.sourceSeqNr = 0;
    this.isReversal = false;
  }
}

export class DeleteDistributionEntryModel
  implements IDeleteDistributionEntryModel
{
  accountDistributionEntryDTOs!: IAccountDistributionEntryDTO[];
  accountDistributionType!: number;
}

export class AccountDistributionEntryRowDTO
  implements IAccountDistributionEntryRowDTO
{
  accountDistributionEntryRowId: number;
  accountDistributionEntryId: number;
  debitAmount: number;
  creditAmount: number;
  debitAmountCurrency: number;
  creditAmountCurrency: number;
  debitAmountEntCurrency: number;
  creditAmountEntCurrency: number;
  debitAmountLedgerCurrency: number;
  creditAmountLedgerCurrency: number;
  dim1Id: number;
  dim1Nr: string;
  dim1Name: string;
  dim1DimName: string;
  sameBalance: number;
  oppositeBalance: number;
  dim2Id: number;
  dim2Nr: string;
  dim2Name: string;
  dim2DimName: string;
  dim3Id: number;
  dim3Nr: string;
  dim3Name: string;
  dim3DimName: string;
  dim4Id: number;
  dim4Nr: string;
  dim4Name: string;
  dim4DimName: string;
  dim5Id: number;
  dim5Nr: string;
  dim5Name: string;
  dim5DimName: string;
  dim6Id: number;
  dim6Nr: string;
  dim6Name: string;
  dim6DimName: string;

  constructor(data: IAccountDistributionEntryRowDTO) {
    this.accountDistributionEntryRowId = data.accountDistributionEntryRowId;
    this.accountDistributionEntryId = data.accountDistributionEntryId;
    this.debitAmount = data.debitAmount;
    this.creditAmount = data.creditAmount;
    this.debitAmountCurrency = data.debitAmountCurrency;
    this.creditAmountCurrency = data.creditAmountCurrency;
    this.debitAmountEntCurrency = data.debitAmountEntCurrency;
    this.creditAmountEntCurrency = data.creditAmountEntCurrency;
    this.debitAmountLedgerCurrency = data.debitAmountLedgerCurrency;
    this.creditAmountLedgerCurrency = data.creditAmountLedgerCurrency;
    this.dim1Id = data.dim1Id;
    this.dim1Nr = data.dim1Nr;
    this.dim1Name = data.dim1Name;
    this.dim1DimName = data.dim1DimName;
    this.sameBalance = data.sameBalance;
    this.oppositeBalance = data.oppositeBalance;
    this.dim2Id = data.dim2Id;
    this.dim2Nr = data.dim2Nr;
    this.dim2Name = data.dim2Name;
    this.dim2DimName = data.dim2DimName;
    this.dim3Id = data.dim3Id;
    this.dim3Nr = data.dim3Nr;
    this.dim3Name = data.dim3Name;
    this.dim3DimName = data.dim3DimName;
    this.dim4Id = data.dim4Id;
    this.dim4Nr = data.dim4Nr;
    this.dim4Name = data.dim4Name;
    this.dim4DimName = data.dim4DimName;
    this.dim5Id = data.dim5Id;
    this.dim5Nr = data.dim5Nr;
    this.dim5Name = data.dim5Name;
    this.dim5DimName = data.dim5DimName;
    this.dim6Id = data.dim6Id;
    this.dim6Nr = data.dim6Nr;
    this.dim6Name = data.dim6Name;
    this.dim6DimName = data.dim6DimName;
  }

  get dim1NrName(): string {
    return `${this.dim1Nr} ${this.dim1Name ?? ''}`.trim();
  }

  get dim2NrName(): string {
    return `${this.dim2Nr ?? ''} ${this.dim2Name ?? ''}`.trim();
  }

  get dim3NrName(): string {
    return `${this.dim3Nr ?? ''} ${this.dim3Name ?? ''}`.trim();
  }

  get dim4NrName(): string {
    return `${this.dim4Nr ?? ''} ${this.dim4Name ?? ''}`.trim();
  }

  get dim5NrName(): string {
    return `${this.dim5Nr ?? ''} ${this.dim5Name ?? ''}`.trim();
  }

  get dim6NrName(): string {
    return `${this.dim6Nr ?? ''} ${this.dim6Name ?? ''}`.trim();
  }
}
