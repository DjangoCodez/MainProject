import { IProductUnitFileModel } from '@shared/models/generated-interfaces/BillingModels';
import { DialogData } from '@ui/dialog/models/dialog';

export interface ProductUnitConversionDialogData extends DialogData {
  productIds: number[];
}
export class ProductUnitFileModel implements IProductUnitFileModel {
  productIds: number[] = [];
  fileData: number[][] = [];

  constructor(ids: number[], fileData: unknown[]) {
    this.productIds = ids;
    this.fileData = <number[][]>fileData;
  }
}
