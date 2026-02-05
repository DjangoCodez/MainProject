import { TermGroup_StockTransactionType } from '@shared/models/generated-interfaces/Enumerations';
import {
  IStockProductDTO,
  IStockTransactionDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class StockProductDTO implements IStockProductDTO {
  stockProductId!: number;
  stockId!: number;
  invoiceProductId!: number;
  quantity!: number;
  orderedQuantity!: number;
  reservedQuantity!: number;
  isInInventory!: boolean;
  warningLevel?: number;
  avgPrice!: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  stockShelfId!: number;
  productNumber!: string;
  productName!: string;
  productUnit!: string;
  stockName!: string;
  stockShelfCode!: string;
  stockShelfName!: string;
  stockValue!: number;
  purchaseQuantity!: number;
  purchaseTriggerQuantity!: number;
  purchasedQuantity!: number;
  productGroupCode!: string;
  productGroupName!: string;
  transaction!: StockTransactionDTO;
  transactionPrice!: number;
  productGroupId!: number;
  numberSort!: string;
  productState!: number;

  //extended properties
  isModified: boolean;
  deliveryLeadTimeDays: number;

  constructor() {
    this.isModified = false;
    this.deliveryLeadTimeDays = 0;
  }
}

export class StockTransactionDTO implements IStockTransactionDTO {
  stockTransactionId!: number;
  stockProductId!: number;
  invoiceRowId?: number;
  invoiceId?: number;
  actionType!: TermGroup_StockTransactionType;
  quantity!: number;
  price!: number;
  avgPrice!: number;
  note!: string;
  created?: Date;
  createdBy!: string;
  voucherId!: number;
  voucherNr!: string;
  transactionDate?: Date;
  productNumber!: string;
  productName!: string;
  stockName!: string;
  actionTypeName!: string;
  reservedQuantity!: number;
  productId!: number;
  stockId!: number;
  stockShelfId!: number;
  stockShelfName!: string;
  productUnitConvertId?: number;
  targetStockId!: number;
  purchaseId?: number;
  originType?: number;
  stockInventoryHeadId?: number;
  sourceLabel!: string;
  sourceNr!: string;
  childStockTransaction!: string;

  // Extends
  total!: number;
}
export interface IAutocompleteIStockProductDTO extends IStockProductDTO {
  id: number;
  name: string;
}
