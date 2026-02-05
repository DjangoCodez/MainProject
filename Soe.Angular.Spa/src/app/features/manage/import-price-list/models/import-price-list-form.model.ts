import {
  SoeFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IFileDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ImportPriceListDTO } from './import-price-list.model';

interface IImportPriceListForm {
  validationHandler: ValidationHandler;
  element: ImportPriceListDTO | undefined;
}
export class ImportPriceListForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IImportPriceListForm) {
    super(validationHandler, {
      supplierId: new SoeSelectFormControl(
        element?.supplierId || '',
        { required: true },
        'manage.system.importpricelist.supplier'
      ),
    });
  }

  get supplierId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.supplierId;
  }

  get file() {
    return this.controls.file as SoeFormControl<IFileDTO>;
  }
}
