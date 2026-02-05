import {
  IStockInventoryFilterDTO,
  IStockInventoryHeadDTO,
  IStockInventoryRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
export class StockInventoryHeadDTO implements IStockInventoryHeadDTO {
  stockInventoryHeadId: number;
  actorCompanyId: number;
  inventoryStart?: Date;
  inventoryStop?: Date;
  headerText: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  stockId: number;
  stockName: string;
  stockInventoryRows: IStockInventoryRowDTO[];
  stockCode: string;

  //Extends
  inventoryStartStr: string;
  inventoryStopStr: string;

  constructor() {
    this.stockInventoryHeadId = 0;
    this.actorCompanyId = 0;
    this.headerText = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.stockId = 0;
    this.stockName = '';
    this.stockInventoryRows = [];
    this.stockCode = '';

    this.inventoryStartStr = '';
    this.inventoryStopStr = '';
  }
}

export class StockInventoryRowDTO implements IStockInventoryRowDTO {
  stockInventoryRowId: number;
  stockInventoryHeadId: number;
  stockProductId: number;
  startingSaldo: number;
  inventorySaldo: number;
  difference: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  productNumber: string;
  productName: string;
  unit: string;
  avgPrice: number;
  shelfId: number;
  shelfCode: string;
  shelfName: string;
  orderedQuantity: number;
  reservedQuantity: number;
  productGroupId?: number;
  productGroupCode: string;
  productGroupName: string;
  transactionDate?: Date;

  constructor() {
    this.stockInventoryRowId = 0;
    this.stockInventoryHeadId = 0;
    this.stockProductId = 0;
    this.startingSaldo = 0;
    this.inventorySaldo = 0;
    this.difference = 0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.productNumber = '';
    this.productName = '';
    this.unit = '';
    this.avgPrice = 0.0;
    this.shelfId = 0;
    this.shelfCode = '';
    this.shelfName = '';
    this.orderedQuantity = 0;
    this.reservedQuantity = 0;
    this.productGroupCode = '';
    this.productGroupName = '';
  }
}

export class StockInventoryFilterDTO implements IStockInventoryFilterDTO {
  stockId!: number;
  shelfIds!: number[];
  productGroupIds!: number[];
  productNrFrom!: string;
  productNrTo!: string;

  //Extention
  productNrFromId!: number;
  productNrToId!: number;
}
