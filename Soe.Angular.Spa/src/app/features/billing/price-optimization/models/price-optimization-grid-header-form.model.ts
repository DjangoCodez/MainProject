import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseCartFilterDTO } from './price-optimization.model';
import { TermGroup_PurchaseCartStatus } from '@shared/models/generated-interfaces/Enumerations';

interface IPriceOptimizationGridHeaderForm {
  validationHandler: ValidationHandler;
  element: PurchaseCartFilterDTO | undefined;
}
export class PriceOptimizationGridHeaderForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IPriceOptimizationGridHeaderForm) {
    super(validationHandler, {
      allItemsSelectionId: new SoeSelectFormControl(
        element?.allItemsSelectionId || 3
      ),
      selectedCartStatusIds: new SoeSelectFormControl(
        element?.selectedCartStatusIds || [TermGroup_PurchaseCartStatus.Open]
      ),
    });
  }

  get allItemsSelectionId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.allItemsSelectionId;
  }
  get selectedCartStatusIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedCartStatusIds;
  }
}
