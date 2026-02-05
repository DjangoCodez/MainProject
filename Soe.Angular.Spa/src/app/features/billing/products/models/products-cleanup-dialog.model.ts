import { IProductCleanupDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class ProductsCleanupDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  originType!: number;
}

export class ProductCleanupDTO implements IProductCleanupDTO {
  productId!: number;
  productNumber!: string;
  productName!: string;
  isExternal!: boolean;
  lastUsedDate!: Date;
  isActive!: boolean;

  //Extended properties
  externalStatus!: string;
}
