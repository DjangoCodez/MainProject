import { IIntrastatTransactionDTO } from '@shared/models/generated-interfaces/CommodityCodeDTO';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class ChangeIntrastatCodeDTO {
  code!: number;
  transactionType!: number;
  country!: number;
}

export class IntrastatTransactionDTO implements IIntrastatTransactionDTO {
  intrastatTransactionId: number;
  originId: number;
  intrastatCodeId: number;
  intrastatTransactionType: number;
  productUnitId?: number;
  sysCountryId?: number;
  netWeight?: number;
  otherQuantity: string;
  notIntrastat: boolean;
  amount?: number;
  state: SoeEntityState;
  customerInvoiceRowId?: number;
  rowNr: number;
  productNr: string;
  productName: string;
  quantity: number;
  productUnitCode: string;

  isModified!: boolean;

  constructor() {
    this.intrastatTransactionId = 0;
    this.originId = 0;
    this.intrastatCodeId = 0;
    this.intrastatTransactionType = 0;
    this.otherQuantity = '';
    this.notIntrastat = false;
    this.state = SoeEntityState.Active;
    this.rowNr = 0;
    this.productNr = '';
    this.productName = '';
    this.quantity = 0;
    this.productUnitCode = '';
  }
}

export class ChangeIntrastatCodeDialogData implements DialogData {
  title!: string;
  content?: string;
  size?: DialogSize;
}
