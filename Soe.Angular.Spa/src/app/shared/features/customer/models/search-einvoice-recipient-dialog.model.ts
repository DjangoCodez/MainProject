import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import {
  IEInvoiceRecipientSearchDTO,
  IEInvoiceRecipientSearchResultDTO,
} from '@shared/models/generated-interfaces/EInvoiceRecipentDTO';

export class SearchEinvoiceRecipientDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  originType!: number;
  einvoiceRecipientValue!: number;
}

export class EInvoiceRecipientModelDTO
  implements IEInvoiceRecipientSearchResultDTO
{
  companyId!: string;
  name!: string;
  orgNo!: string;
  vatNo!: string;
  gln!: string;
}

export class EInvoiceRecipientSearchDTO implements IEInvoiceRecipientSearchDTO {
  partyId: string;
  receiveElectronicInvoiceCapability!: boolean;
  name!: string;
  gln!: string;
  orgNo!: string;
  vatNo!: string;

  constructor() {
    this.partyId = '';
  }
}
