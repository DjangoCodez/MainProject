import {
  ISavePurchaseModel,
  ISavePurchaseStatus,
  ISendPurchaseEmail,
} from '@shared/models/generated-interfaces/BillingModels';
import { SoeOriginStatus } from '@shared/models/generated-interfaces/Enumerations';
import {
  IPurchaseDTO,
  IPurchaseRowDTO,
} from '@shared/models/generated-interfaces/PurchaseDTOs';
import {
  IOriginUserDTO,
  IOriginUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { BehaviorSubject } from 'rxjs';
import { PurchaseRowDTO } from './purchase-rows.model';
import { ISaveUserCompanySettingModel } from '@shared/models/generated-interfaces/TimeModels';

export class PurchaseDTO implements IPurchaseDTO {
  purchaseId!: number;
  purchaseNr!: string;
  purchaseDate?: Date;
  referenceOur!: string;
  referenceOurId!: number;
  referenceYour!: string;
  supplierId?: number;
  supplierEmail!: string;
  supplierCustomerNr!: string;
  defaultDim1AccountId?: number;
  defaultDim2AccountId?: number;
  defaultDim3AccountId?: number;
  defaultDim4AccountId?: number;
  defaultDim5AccountId?: number;
  defaultDim6AccountId?: number;
  purchaseRows!: IPurchaseRowDTO[];
  originUsers!: IOriginUserSmallDTO[];
  participants!: string;
  purchaseLabel!: string;
  contactEComId?: number;
  deliveryConditionId?: number;
  deliveryTypeId?: number;
  paymentConditionId?: number;
  deliveryAddressId?: number;
  deliveryAddress!: string;
  vatType!: number;
  origindescription!: string;
  originStatus: SoeOriginStatus = 0;
  statusName!: string;
  currencyId?: number;
  currencyRate!: number;
  currencyDate!: Date;
  projectId?: number;
  projectNr!: string;
  orderId?: number;
  orderNr!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  totalAmountExVatCurrency!: number;
  totalAmountCurrency!: number;
  vatAmountCurrency!: number;
  wantedDeliveryDate?: Date;
  confirmedDeliveryDate?: Date;
  totalAmount = 0;
  vatAmount = 0;
  stockId?: number;
  stockCode!: string;

  public static getPropertiesToSkipOnSave(): string[] {
    return [
      'created',
      'createdBy',
      'purchaseRows',
      'modified',
      'modifiedBy',
      'originStatusName',
      'projectNr',
      'participants',
      'purchaseRows',
      'originUsers',
      'referenceOurId',
    ];
  }
}

export class PurchaseFilterDTO {
  allItemsSelection!: number;
  selectedPurchaseStatusIds!: number[];
}
export class PurchaseStatusTextDTO {
  lateText!: string;
  restText!: string;
}

export class OriginUserSmallDTO implements IOriginUserSmallDTO {
  originUserId!: number;
  userId!: number;
  main!: boolean;
  name!: string;
  isReady!: boolean;
}

export class PurchaseSetPurchaseDateDTO {
  purchaseDate!: Date;
}
export class PurchaseDateDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  purchaseRows!: BehaviorSubject<PurchaseRowDTO[]>;
  newStatus!: SoeOriginStatus;
  confirmedDeliveryDate!: Date;
  useConfirmed!: boolean;
  purchaseDate?: Date;
}

export class SavePurchaseModel implements ISavePurchaseModel {
  modifiedFields!: Record<string, any>;
  originUsers!: IOriginUserDTO[];
  newRows!: IPurchaseRowDTO[];
  modifiedRows!: Record<string, any>[];
}

export class SendPurchaseEmail implements ISendPurchaseEmail {
  purchaseId!: number;
  purchaseIds!: number[];
  reportId!: number;
  emailTemplateId!: number;
  langId?: number;
  recipients!: number[];
  singleRecipient!: string;
  constructor(purchaseIds: number[], emailTemplateId: number, langId: number) {
    this.purchaseIds = purchaseIds;
    this.emailTemplateId = emailTemplateId;
    this.langId = langId;
  }
}

export class StatusFunctionDTO {
  id!: number;
  label!: string;
  icon!: string;
  constructor(id: number, label: string, icon: string) {
    this.id = id;
    this.label = label;
    this.icon = icon;
  }
}

export class SavePurchaseStatus implements ISavePurchaseStatus {
  purchaseId!: number;
  status!: SoeOriginStatus;
  constructor(purchaseId: number, status: SoeOriginStatus) {
    this.purchaseId = purchaseId;
    this.status = status;
  }
}

export class PurchaseDeliveryAddressesDialogData implements DialogData {
  size?: DialogSize;
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  disableClose?: boolean;
  customerOrderId!: number;
  constructor(title: string, customerOrderId: number) {
    this.title = title;
    this.size = 'md';
    this.customerOrderId = customerOrderId;
  }
}
export enum PurchaseEditSaveFunctions {
  Save = 1,
  SaveAndClose = 2,
}

export enum PurchaseEditPrintFunctions {
  Print = 1,
  EMail = 2,
  ReportDialog = 3,
}

export class ReturnSetPurchaseDateDialog {
  selectedDate!: Date;
  originalDate!: any;
  originalDateSet = false;
  propName!: string;
  purchaseRowsChanges: PurchaseRowDTO[] = [];
  constructor(
    selectedDate: Date,
    originalDate: any,
    originalDateSet: boolean,
    propName: string,
    purchaseRowsChanges: PurchaseRowDTO[]
  ) {
    this.selectedDate = selectedDate;
    this.originalDate = originalDate;
    this.propName = propName;
    this.originalDateSet = originalDateSet;
    this.purchaseRowsChanges = purchaseRowsChanges;
  }
}

export class SaveUserCompanySettingModel
  implements ISaveUserCompanySettingModel
{
  settingMainType: number;
  settingTypeId: number;
  boolValue!: boolean;
  intValue: number;
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
