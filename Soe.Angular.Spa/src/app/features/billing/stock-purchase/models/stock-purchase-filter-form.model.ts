import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { StockPurchaseFilterDTO } from './stock-purchase.model';

interface IPurchaseFilterForm {
  validationHandler: ValidationHandler;
  element: StockPurchaseFilterDTO | undefined;
}
export class StockPurchaseFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPurchaseFilterForm) {
    super(validationHandler, {
      purchaseGenerationType: new SoeSelectFormControl(
        element?.purchaseGenerationType || undefined,
        {
          required: true,
        },
        'billing.stock.purchase.suggestionbase'
      ),
      triggerQuantityPercent: new SoeNumberFormControl(
        element?.triggerQuantityPercent || 0
      ),
      stockPlaceIds: new SoeSelectFormControl(element?.stockPlaceIds || []),
      productNrFrom: new SoeTextFormControl(element?.productNrFrom || ''),
      productNrTo: new SoeTextFormControl(element?.productNrTo || ''),
      purchaser: new SoeTextFormControl(element?.purchaser || ''),

      excludeMissingTriggerQuantity: new SoeCheckboxFormControl(
        element?.excludeMissingTriggerQuantity || true
      ),
      excludeMissingPurchaseQuantity: new SoeCheckboxFormControl(
        element?.excludeMissingPurchaseQuantity || true
      ),
      defaultDeliveryAddress: new SoeTextFormControl(
        element?.defaultDeliveryAddress || ''
      ),
    });
  }

  get purchaseGenerationType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.purchaseGenerationType;
  }
  get triggerQuantityPercent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.triggerQuantityPercent;
  }
  get stockPlaceIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.stockPlaceIds;
  }
  get productNrFrom(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productNrFrom;
  }
  get productNrTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productNrTo;
  }
  get purchaser(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.purchaser;
  }
  get excludeMissingTriggerQuantity(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.excludeMissingTriggerQuantity;
  }
  get excludeMissingPurchaseQuantity(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.excludeMissingPurchaseQuantity;
  }
  get defaultDeliveryAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.defaultDeliveryAddress;
  }
}
