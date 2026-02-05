import { ITransferSupplierInvoiceRowsToOrderModel } from '@shared/models/generated-interfaces/EconomyModels';

export class TransferSupplierProductRowsModel
  implements ITransferSupplierInvoiceRowsToOrderModel
{
  customerInvoiceId!: number;
  supplierInvoiceId!: number;
  wholesellerId!: number;
  supplierInvoiceProductRowIds!: number[];
}

