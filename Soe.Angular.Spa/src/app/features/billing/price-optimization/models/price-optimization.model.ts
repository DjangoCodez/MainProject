import {
  SoeEntityState,
  TermGroup_PurchaseCartStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IChangeCartStateModel,
  IPurchaseCartDTO,
  IPurchaseCartRowDTO,
} from '@shared/models/generated-interfaces/PurchaseCartDTOs';

export class PurchaseCartFilterDTO {
  allItemsSelectionId!: number;
  selectedCartStatusIds: TermGroup_PurchaseCartStatus[] = [
    TermGroup_PurchaseCartStatus.Open,
  ];
}

export class PurchaseCartDTO implements IPurchaseCartDTO {
  name!: string;
  status = TermGroup_PurchaseCartStatus.Open;
  purchaseCartId!: number;
  seqNr!: number;
  priceStrategy!: number;
  description!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;
  selectedWholesellerIds!: number[];
  purchaseCartRows!: PurchaseCartRowDTO[];

  //Extension
  statusName!: string;
  isModified: boolean = false;
}

export class PurchaseCartRowDTO implements IPurchaseCartRowDTO {
  purchaseCartRowId: number = 0;
  purchaseCartId!: number;
  sysProductId!: number;
  productNr!: string;
  productInfo!: string;
  imageUrl!: string;
  type!: number;
  externalId?: number;
  productName!: string;
  sysPricelistHeadId!: number;
  wholesellerNetPriceId!: number;
  sysWholesellerId!: number;
  quantity!: number;
  purchasePrice!: number;
  state!: SoeEntityState;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;

  //Extension
  isModified!: boolean;
  selectedPrice: number = 0;
  wholesalerPrices: { [key: string]: number } = {};
  productNameNumber!: string;

  constructor(
    purchaseCartRowId: number,
    purchaseCartId: number,
    productInfo: string,
    productName: string,
    productNr: string,
    imageUrl: string,
    type: number,
    externalId: number,
    purchasePrice: number,
    quantity: number,
    sysWholesellerId: number,
    sysProductId: number,
    isModified: boolean
  ) {
    this.purchaseCartRowId = purchaseCartRowId;
    this.purchaseCartId = purchaseCartId;
    this.productInfo = productInfo;
    this.productName = productName;
    this.productNr = productNr;
    this.imageUrl = imageUrl;
    this.type = type;
    this.externalId = externalId;
    this.purchasePrice = purchasePrice;
    this.quantity = quantity;
    this.sysWholesellerId = sysWholesellerId;
    this.sysProductId = sysProductId;
    this.isModified = isModified;
  }
}

export class ChangeCartStateModel implements IChangeCartStateModel {
  ids!: number[];
  stateTo!: TermGroup_PurchaseCartStatus;
}
