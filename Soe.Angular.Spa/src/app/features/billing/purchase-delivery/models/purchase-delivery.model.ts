import {
  IPurchaseDeliveryDTO,
  IPurchaseDeliverySaveDTO,
  IPurchaseDeliverySaveRowDTO,
} from '@shared/models/generated-interfaces/PurchaseDeliveryDTOs ';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IPurchaseDeliveryRowDTO } from '@shared/models/generated-interfaces/PurchaseDeliveryDTOs ';

export class PurchaseDeliveryDTO implements IPurchaseDeliveryDTO {
  purchaseDeliveryId: number;
  deliveryNr: number;
  deliveryDate?: Date;
  supplierId?: number;
  supplierNr: string;
  supplierName: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  purchaseId: number;
  copyQty: boolean;
  finalDelivery!: boolean;
  originDescription: string;
  deliveryType!: number;
  purchaseNr!: string;

  constructor() {
    this.purchaseDeliveryId = 0;
    this.deliveryNr = 0;
    this.supplierNr = '';
    this.supplierName = '';
    this.copyQty = true;
    this.purchaseId = 0;
    this.originDescription = '';
    // this.deliveryDate = new Date();
    this.deliveryType = 99;
  }
}

export class PurchaseDeliveryRowDTO implements IPurchaseDeliveryRowDTO {
  purchaseId: number;
  purchaseNr: string;
  tempRowId: number;
  productNr: string;
  productName: string;
  purchaseDeliveryRowId: number;
  purchaseDeliveryId: number;
  purchaseRowId: number;
  deliveredQuantity: number;
  deliveryDate?: Date;
  purchasePrice?: number;
  purchasePriceCurrency?: number;
  purchaseQuantity: number;
  remainingQuantity: number;
  isLocked?: boolean | undefined;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  stockCode: string;

  //extensions
  isModified: boolean;

  constructor() {
    this.purchaseId = 0;
    this.purchaseNr = '';
    this.tempRowId = 0;
    this.productNr = '';
    this.productName = '';
    this.purchaseDeliveryRowId = 0;
    this.purchaseDeliveryId = 0;
    this.purchaseRowId = 0;
    this.deliveredQuantity = 0;
    this.purchaseQuantity = 0;
    this.remainingQuantity = 0;
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.stockCode = '';
    this.isModified = false;
  }
}

export class PurchaseDeliverySaveDTO implements IPurchaseDeliverySaveDTO {
  purchaseDeliveryId!: number;
  deliveryDate: Date;
  supplierId!: number;
  rows: IPurchaseDeliverySaveRowDTO[];

  constructor() {
    this.deliveryDate = new Date();
    this.rows = [];
  }
}
