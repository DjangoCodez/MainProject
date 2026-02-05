import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { IAccountingRowDTO } from '@shared/models/generated-interfaces/AccountingRowDTO';
import {
  ICustomerInvoicesGridModel,
  ISaveAdjustmentModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import {
  TermGroup_InventoryStatus,
  TermGroup_InventoryWriteOffMethodPeriodType,
  SoeEntityState,
  TermGroup_InventoryLogType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountSmallDTO,
  IAccountingSettingsRowDTO,
  IFileUploadDTO,
  IInventoryDTO,
  IInventoryGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { InventoryAdjustFunctions } from '@shared/util/Enumerations';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class InventoryDTO implements IInventoryDTO {
  inventoryId: number;
  actorCompanyId: number;
  parentId?: number;
  inventoryWriteOffMethodId: number;
  voucherSeriesTypeId: number;
  supplierInvoiceId?: number;
  customerInvoiceId?: number;
  inventoryNr: string;
  name: string;
  description: string;
  notes: string;
  status: TermGroup_InventoryStatus;
  purchaseDate?: Date;
  writeOffDate?: Date;
  purchaseAmount: number;
  writeOffAmount: number;
  writeOffSum: number;
  writeOffRemainingAmount: number;
  accWriteOffAmount!: number;
  endAmount: number;
  periodType: TermGroup_InventoryWriteOffMethodPeriodType;
  periodValue: number;
  writeOffPeriods: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  // Should be able to remove these after migration to Angular.
  inventoryAccounts!: { [key: number]: IAccountSmallDTO };
  accWriteOffAccounts!: { [key: number]: IAccountSmallDTO };
  writeOffAccounts!: { [key: number]: IAccountSmallDTO };
  accOverWriteOffAccounts!: { [key: number]: IAccountSmallDTO };
  overWriteOffAccounts!: { [key: number]: IAccountSmallDTO };
  accWriteDownAccounts!: { [key: number]: IAccountSmallDTO };
  writeDownAccounts!: { [key: number]: IAccountSmallDTO };
  accWriteUpAccounts!: { [key: number]: IAccountSmallDTO };
  writeUpAccounts!: { [key: number]: IAccountSmallDTO };
  // <--
  accountingSettings: IAccountingSettingsRowDTO[] = [];
  categoryIds: number[];
  parentName: string;
  statusName: string;
  supplierInvoiceInfo: string;
  customerInvoiceInfo: string;
  inventoryFiles!: IFileUploadDTO[];
  showPreliminary: boolean;

  info: string;

  constructor() {
    this.inventoryId = 0;
    this.actorCompanyId = 0;
    this.inventoryWriteOffMethodId = 0;
    this.voucherSeriesTypeId = 0;
    this.inventoryNr = '';
    this.name = '';
    this.description = '';
    this.notes = '';
    this.status = TermGroup_InventoryStatus.Active;
    this.purchaseAmount = 0;
    this.writeOffAmount = 0.0;
    this.writeOffSum = 0.0;
    this.writeOffRemainingAmount = 0.0;
    this.endAmount = 0;
    this.periodType = TermGroup_InventoryWriteOffMethodPeriodType.Period;
    this.periodValue = 0;
    this.writeOffPeriods = 0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.categoryIds = [];
    this.parentName = '';
    this.statusName = '';
    this.supplierInvoiceInfo = '';
    this.customerInvoiceInfo = '';
    this.showPreliminary = true;

    this.info = '';
  }
}
export class InventoryGridDTO implements IInventoryGridDTO {
  inventoryId: number;
  inventoryNr: string;
  name: string;
  description: string;
  status: number;
  statusName: string;
  purchaseDate?: Date;
  purchaseAmount: number;
  writeOffAmount: number;
  writeOffRemainingAmount: number;
  writeOffSum: number;
  accWriteOffAmount!: number;
  endAmount: number;
  inventoryWriteOffMethodId: number;
  inventoryAccountNr: string;
  inventoryAccountName: string;
  categories: string;
  inventoryAccountNumberName!: string;
  inventoryWriteOffMethod!: string;

  constructor() {
    this.inventoryId = 0;
    this.inventoryNr = '';
    this.name = '';
    this.description = '';
    this.status = 0;
    this.statusName = '';
    this.purchaseAmount = 0;
    this.writeOffAmount = 0;
    this.writeOffRemainingAmount = 0;
    this.writeOffSum = 0;
    this.endAmount = 0;
    this.inventoryWriteOffMethodId = 0;
    this.inventoryAccountNr = '';
    this.inventoryAccountName = '';
    this.categories = '';
  }
}

export class InventoriesAdjustmentDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  inventoryId!: number;
  purchaseDate!: Date;
  purchaseAmount!: number;
  accWriteOffAmount!: number;
  adjustmentType!: number;
  accountingSettings!: IAccountingSettingsRowDTO[];
  inventoryBaseAccounts!: ISmallGenericType[];
  noteText!: string;
  hideFooter?: boolean;
}

export class InventoryAdjustmentDTO {
  inventoryId!: number;
  purchaseDate?: Date;
  adjustmentDate?: Date;
  amount!: number;
  writeOffAmount!: number;
  voucherSeriesTypeId!: number | undefined;
  adjustmentType!: InventoryAdjustFunctions;
  noteText!: string;
  accountingRows!: AccountingRowDTO[];
}

export class SaveAdjustmentModel implements ISaveAdjustmentModel {
  inventoryId!: number;
  type!: TermGroup_InventoryLogType;
  voucherSeriesTypeId!: number;
  amount!: number;
  date!: Date;
  note!: string;
  accountRowItems!: IAccountingRowDTO[];

  constructor(
    inventoryId: number,
    type: TermGroup_InventoryLogType,
    voucherSeriesTypeId: number,
    amount: number,
    date: Date,
    note: string,
    accountRowItems: IAccountingRowDTO[]
  ) {
    this.inventoryId = inventoryId;
    this.type = type;
    this.voucherSeriesTypeId = voucherSeriesTypeId;
    this.amount = amount;
    this.date = date;
    this.note = note;
    this.accountRowItems = accountRowItems;
  }
}

export class InventoryUploadDTO {
  year!: number;
  fileString!: string;
  selectedDate!: Date;
  fileName!: string;
}

export class CustomerInvoicesGridModel implements ICustomerInvoicesGridModel {
  classification: number;
  originType: number;
  loadOpen: boolean;
  loadClosed: boolean;
  onlyMine: boolean;
  loadActive: boolean;
  allItemsSelection: number;
  billing: boolean;
  modifiedIds: number[];
  constructor(
    classification: number,
    originType: number,
    loadOpen: boolean,
    loadClosed: boolean,
    onlyMine: boolean,
    loadActive: boolean,
    allItemsSelection: number,
    billing: boolean,
    modifiedIds: number[]
  ) {
    this.classification = classification;
    this.originType = originType;
    this.loadOpen = loadOpen;
    this.loadClosed = loadClosed;
    this.onlyMine = onlyMine;
    this.loadActive = loadActive;
    this.allItemsSelection = allItemsSelection;
    this.billing = billing;
    this.modifiedIds = modifiedIds;
  }
}

export class InventoriesFileUploadDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  type!: number;
  entity!: number;
  recordId!: number;
}

export class InventoryFilterDTO {
  selectedStatusIds!: number[];
}

export class SaveInventoryModel {
  inventory!: InventoryDTO;
  categoryRecords: unknown[] = [];
}
