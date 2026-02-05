import { DialogData } from '@ui/dialog/models/dialog';
import { AddInvoiceToAttestFlowInvoice } from './add-invoice-to-attest-flow-invoice.model';

export interface AddInvoiceToAttestFlowDialogData extends DialogData {
  supplierInvoices: AddInvoiceToAttestFlowInvoice[];
}
