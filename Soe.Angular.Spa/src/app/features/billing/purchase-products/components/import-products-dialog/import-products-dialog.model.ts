import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export class ImportSupplierProductDialogDTO {
  supplierId?: number;
}

interface IImportSupplierProductDialogForm {
  validationHandler: ValidationHandler;
  element: ImportSupplierProductDialogDTO;
}

export class ImportSupplierProductDialogForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IImportSupplierProductDialogForm) {
    super(validationHandler, {
      supplierId: new SoeSelectFormControl(element?.supplierId || undefined),
    });
  }

  get supplierId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.supplierId;
  }
}
