import { ISupplierInvoiceProductRowDTO } from '@shared/models/generated-interfaces/SupplierInvoiceProductRowDTO';

export class SupplierInvoiceProductRowDTO
  implements ISupplierInvoiceProductRowDTO
{
  supplierInvoiceProductRowId!: number;
  supplierInvoiceId!: number;
  customerInvoiceRowId?: number;
  customerInvoiceId?: number;
  sellerProductNumber!: string;
  text!: string;
  unitCode!: string;
  quantity!: number;
  priceCurrency!: number;
  amountCurrency!: number;
  vatAmountCurrency!: number;
  vatRate!: number;
  customerInvoiceNumber!: string;
  rowType!: number;
  state!: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  
  // Additional property for UI
  rowTypeIcon?: string;
}

