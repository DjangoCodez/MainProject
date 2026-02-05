import { ValidationHandler } from '@shared/handlers';
import { SearchInvoiceProductDialogData } from './search-invoice-product-dialog.models';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface ISearchInvoiceProductForm {
  validationHandler: ValidationHandler;
  element?: SearchInvoiceProductDialogData;
}

export class SearchInvoiceProductForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISearchInvoiceProductForm) {
    super(validationHandler, {
      number: new SoeTextFormControl(element?.number || ''),
      priceListTypeId: new SoeSelectFormControl(element?.priceListTypeId || 0),
      quantity: new SoeNumberFormControl(element?.quantity || undefined),
    });
    this.number.disable();
  }

  get number(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.number;
  }

  get priceListTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.priceListTypeId;
  }

  get quantity(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.quantity;
  }
}
