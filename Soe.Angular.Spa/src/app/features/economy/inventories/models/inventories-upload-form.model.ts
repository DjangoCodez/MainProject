import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InventoryUploadDTO } from './inventories.model';

interface IInventoriesUploadForm {
  validationHandler: ValidationHandler;
  element: InventoryUploadDTO | undefined;
}
export class InventoriesUploadForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IInventoriesUploadForm) {
    super(validationHandler, {
      year: new SoeTextFormControl(element?.year || new Date().getFullYear(), {
        required: true,
      }),
      fileString: new SoeTextFormControl(element?.fileString || ''),
      fileName: new SoeTextFormControl(element?.fileName || '', {
        required: true,
      }),
    });
  }

  get year(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.year;
  }

  get fileString(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileString;
  }

  get fileName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileName;
  }
}
