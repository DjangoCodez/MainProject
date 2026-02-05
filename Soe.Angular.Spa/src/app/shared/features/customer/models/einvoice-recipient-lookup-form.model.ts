import { ValidationHandler } from '@shared/handlers';
import { SoeFormGroup } from '@shared/extensions';
import { EInvoiceRecipientModelDTO } from './search-einvoice-recipient-dialog.model';

interface IEinvoiceRecipientLookupForm {
  validationHandler: ValidationHandler;
  element: EInvoiceRecipientModelDTO | undefined;
}

export class EinvoiceRecipientLookupForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEinvoiceRecipientLookupForm) {
    super(validationHandler, {
      name: element?.name || '',
      orgNumber: element?.orgNo || '',
      vatNumber: element?.vatNo || '',
      glNumber: element?.gln || '',
    });
  }
}
