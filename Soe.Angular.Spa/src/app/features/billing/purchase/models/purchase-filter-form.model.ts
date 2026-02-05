import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseFilterDTO } from './purchase.model';
import { SoeOriginStatus } from '@shared/models/generated-interfaces/Enumerations';

interface IPurchaseFilterForm {
  validationHandler: ValidationHandler;
  element: PurchaseFilterDTO | undefined;
}
export class PurchaseFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPurchaseFilterForm) {
    super(validationHandler, {
      allItemsSelection: new SoeSelectFormControl(
        element?.allItemsSelection || 99
      ),
      selectedPurchaseStatusIds: new SoeSelectFormControl(
        element?.selectedPurchaseStatusIds || [
          SoeOriginStatus.Origin,
          SoeOriginStatus.PurchaseDone,
          SoeOriginStatus.PurchaseSent,
          SoeOriginStatus.PurchaseAccepted,
        ]
      ),
    });
  }

  get allItemsSelection(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.allItemsSelection;
  }
  get selectedPurchaseStatusIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedPurchaseStatusIds;
  }
}
