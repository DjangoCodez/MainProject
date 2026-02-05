import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISaveInventoryNotesModel } from '@shared/models/generated-interfaces/EconomyModels';

interface IInventoryNotesForm {
  validationHandler: ValidationHandler;
  element: ISaveInventoryNotesModel | undefined;
}
export class InventoryNotesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IInventoryNotesForm) {
    super(validationHandler, {
      inventoryId: new SoeTextFormControl(element?.inventoryId || 0, {
        isIdField: true,
      }),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 255 },
        'common.description'
      ),
      notes: new SoeTextFormControl(
        element?.notes || '',
        undefined,
        'common.note'
      ),
    });
  }

  get inventoryNotes(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.inventoryNotes;
  }

  get inventoryDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.inventoryDescription;
  }
}
