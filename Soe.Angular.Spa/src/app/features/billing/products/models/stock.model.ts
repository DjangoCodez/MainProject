import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountingSettingsRowDTO,
  IStockDTO,
  IStockProductDTO,
  IStockShelfDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class StockDTO implements IStockDTO {
  stockId: number;
  code: string;
  name: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  saldo: number;
  avgPrice: number;
  stockProductId?: number;
  purchaseTriggerQuantity: number;
  purchaseQuantity: number;
  deliveryLeadTimeDays: number;
  stockShelfId: number;
  stockShelfName: string;
  stockShelves: IStockShelfDTO[] = [];
  accountingSettings: IAccountingSettingsRowDTO[] = [];
  isExternal!: boolean;
  deliveryAddressId?: number;
  stockProducts: IStockProductDTO[];

  constructor() {
    this.stockId = 0;
    this.code = '';
    this.name = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.saldo = 0;
    this.avgPrice = 0;
    this.purchaseTriggerQuantity = 0;
    this.purchaseQuantity = 0;
    this.deliveryLeadTimeDays = 0;
    this.stockShelfId = 0;
    this.stockShelfName = '';
    this.stockProducts = [];
  }
}

export class StockShelfDTO implements IStockShelfDTO {
  stockShelfId!: number;
  stockId!: number;
  code: string = '';
  name: string = '';
  stockName: string = '';
  isDelete!: boolean;
  shelfName: string = '';
}
