import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ImportPriceListUploadDTO } from './import-price-list.model';

interface IImportPriceListUploadForm {
  validationHandler: ValidationHandler;
  element: ImportPriceListUploadDTO | undefined;
}
export class ImportPriceListUploadForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IImportPriceListUploadForm) {
    super(validationHandler, {
      fileName: new SoeTextFormControl(
        element?.fileName || '',
        {
          required: true,
        },
        'manage.system.import.price.list.file'
      ),

      providerId: new SoeSelectFormControl(
        element?.providerId,
        {
          required: true,
        },
        'manage.system.import.price.list.supplier'
      ),
    });
  }

  get providerId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.providerId;
  }

  get fileName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileName;
  }
}
