import {
  ICompTermDTO,
  IProductUnitSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class ProductUnitSmallDTO implements IProductUnitSmallDTO {
  productUnitId: number;
  code: string;
  name: string;

  constructor() {
    this.productUnitId = 0;
    this.code = '';
    this.name = '';
  }
}

export class ProductUnitModel {
  productUnit!: ProductUnitSmallDTO;
  translations!: ICompTermDTO[];
}

export class ProductUnitDTO {
  productUnitId: number;
  code: string;
  name: string;
  translations: ICompTermDTO[];

  constructor() {
    this.productUnitId = 0;
    this.code = '';
    this.name = '';
    this.translations = [];
  }
}
