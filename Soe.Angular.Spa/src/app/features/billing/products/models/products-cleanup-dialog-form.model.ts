import { ValidationHandler } from '@shared/handlers';
import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { IProductCleanupDTO } from '@shared/models/generated-interfaces/ProductDTOs';

interface IProductCleanupDialogForm {
  validationHandler: ValidationHandler;
  element: IProductCleanupDTO;
}
export class ProductCleanupDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IProductCleanupDialogForm) {
    super(validationHandler, {
      lastUsedDate: new SoeDateFormControl(element.lastUsedDate || null),
    });
  }

  get lastUsedDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.lastUsedDate;
  }
}
