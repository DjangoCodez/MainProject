import { StockProductDTO } from '@features/billing/stock-balance/models/stock-balance.model';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountingSettingsRowDTO,
  IStockDTO,
  IStockShelfDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class StockDTO implements IStockDTO {
  stockProducts!: StockProductDTO[];
  stockId!: number;
  actorCompanyId!: number;
  code!: string;
  name!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState = 0;
  saldo!: number;
  avgPrice!: number;
  stockProductId?: number;
  purchaseTriggerQuantity!: number;
  purchaseQuantity!: number;
  deliveryLeadTimeDays!: number;
  stockShelfId!: number;
  stockShelfName!: string;
  stockShelves!: IStockShelfDTO[];
  isExternal!: boolean;
  accountingSettings: IAccountingSettingsRowDTO[] = [];
  deliveryAddressId?: number;

  //extended properties
  // warehouseProducts?: StockProductDTO[];
}

export class StockShelfDTO implements IStockShelfDTO {
  stockShelfId!: number;
  stockId!: number;
  code!: string;
  name!: string;
  stockName!: string;
  isDelete!: boolean;
  constructor(
    stockShelfId: number,
    stockId: number,
    code: string,
    name: string,
    stockName: string,
    isDelete: boolean
  ) {
    this.stockShelfId = stockShelfId;
    this.stockId = stockId;
    this.code = code;
    this.name = name;
    this.stockName = stockName;
    this.isDelete = isDelete;
  }
}
