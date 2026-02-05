import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IPriceListDTO } from '@shared/models/generated-interfaces/PriceListDTOs';

export class PriceListDTO implements IPriceListDTO {
  priceListId: number;
  productId: number;
  priceListTypeId: number;
  sysPriceListTypeName: string;
  price: number;
  quantity: number;
  startDate!: Date;
  startDateDisplay?: Date;
  stopDate!: Date;
  stopDateDisplay?: Date;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  isModified?: boolean;
  name?: string;
  number?: string;
  purchasePrice?: number;
  newRow?: boolean;

  constructor() {
    this.priceListId = 0;
    this.productId = 0;
    this.priceListTypeId = 0;
    this.sysPriceListTypeName = '';
    this.price = 0;
    this.quantity = 0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.isModified = false;
  }

  static fromServer(r: PriceListDTO): PriceListDTO {
    const max = new Date('9998-01-01');
    const min = new Date('1902-01-01');
    return {
      priceListTypeId: r.priceListTypeId,
      priceListId: r.priceListId,
      productId: r.productId,
      quantity: r.quantity,
      sysPriceListTypeName: '',
      price: r.price,
      modifiedBy: '',
      createdBy: '',
      isModified: false,
      state: r.state,
      name: r.name,
      number: r.number,
      purchasePrice: r.purchasePrice,
      startDate: r.startDate,
      startDateDisplay: r.startDate > min ? r.startDate : undefined,
      stopDate: r.stopDate,
      stopDateDisplay: r.stopDate < max ? r.stopDate : undefined,
    };
  }

  static fromClient(r: PriceListDTO): PriceListDTO {
    const model = {
      priceListTypeId: r.priceListTypeId,
      priceListId: r.priceListId,
      productId: r.productId,
      quantity: r.quantity,
      sysPriceListTypeName: '',
      price: r.price,
      modifiedBy: '',
      createdBy: '',
      isModified: r.isModified ?? false,
      state: r.state,
      name: r.name,
      number: r.number,
      purchasePrice: r.purchasePrice,
    } as PriceListDTO;
    if (r.startDateDisplay) {
      model.startDate = r.startDateDisplay;
    }
    if (r.stopDateDisplay) {
      model.stopDate = r.stopDateDisplay;
    }
    return model;
  }
}
