import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InventoryFilterDTO } from './inventories.model';

interface IInventoriesFilterForm {
  validationHandler: ValidationHandler;
  element: InventoryFilterDTO | undefined;
}
export class InventoriesFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IInventoriesFilterForm) {
    super(validationHandler, {
      //Grid Selection
      selectedStatusIds: new SoeSelectFormControl(
        element?.selectedStatusIds || []
      ),
    });
  }

  get selectedStatusIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedStatusIds;
  }
}
