import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseDeliveryDTO } from './purchase-delivery.model';

interface IPurchaseDeliveryForm {
  validationHandler: ValidationHandler;
  element: PurchaseDeliveryDTO | undefined;
}
export class PurchaseDeliveryForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IPurchaseDeliveryForm) {
    super(validationHandler, {
      purchaseDeliveryId: new SoeTextFormControl(
        element?.purchaseDeliveryId || 0,
        {
          isIdField: true,
        }
      ),
      deliveryNr: new SoeTextFormControl(element?.deliveryNr || '', {
        isNameField: true,
      }),
      deliveryDate: new SoeDateFormControl(element?.deliveryDate || new Date()),
      supplierId: new SoeSelectFormControl(element?.supplierId || null),
      purchaseId: new SoeSelectFormControl(element?.purchaseId || 0),

      originDescription: new SoeTextFormControl(
        element?.originDescription || ''
      ),
      finalDelivery: new SoeCheckboxFormControl(
        false,
        {},
        'billing.purchase.delivery.finaldelivery'
      ),
      copyQty: new SoeCheckboxFormControl(
        element?.copyQty || true,
        {},
        'billing.purchase.delivery.copyqty'
      ),

      //grid filter
      deliveryType: new SoeSelectFormControl(element?.deliveryType || 99),
    });

    this.thisValidationHandler = validationHandler;
  }

  get purchaseDeliveryId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.purchaseDeliveryId;
  }

  get deliveryNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.deliveryNr;
  }

  get originDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.originDescription;
  }

  get supplierId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.supplierId;
  }

  get purchaseId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.purchaseId;
  }

  get deliveryDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.deliveryDate;
  }

  get copyQty(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.copyQty;
  }

  get finalDelivery(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.finalDelivery;
  }

  //grid filter
  get deliveryType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.deliveryType;
  }
}
