import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { FinvoiceGridFilterDTO } from './imports-invoices-finvoice.model';

interface IFinvoiceFilterGridForm {
  validationHandler: ValidationHandler;
  element: FinvoiceGridFilterDTO;
}

export class FinvoiveFilterGridForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IFinvoiceFilterGridForm) {
    super(validationHandler, {
      allItemsSelection: new SoeSelectFormControl(
        element.allItemsSelection || 1
      ),
      showOnlyUnHandled: new SoeCheckboxFormControl(
        element.showOnlyUnHandled || false
      ),
    });
  }

  get allItemsSelection() {
    return <SoeSelectFormControl>this.controls.item;
  }

  get showOnlyUnHandled() {
    return <SoeCheckboxFormControl>this.controls.showOnlyUnHandled;
  }
}
