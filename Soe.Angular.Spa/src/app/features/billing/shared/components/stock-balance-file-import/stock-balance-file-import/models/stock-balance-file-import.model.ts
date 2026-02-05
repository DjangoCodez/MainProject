import { IImportStockBalances } from '@shared/models/generated-interfaces/BillingModels';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class ImportStockBalancesDTO implements IImportStockBalances {
  stockInventoryHeadId!: number;
  wholesellerId!: number;
  stockId!: number;
  createVoucher!: boolean;
  fileName!: string;
  fileString!: string;
  fileData!: number[][];
  fromInventory!: boolean;
}

export class StockBalanceFileImportDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  stockInventoryHeadId!: number;
}
