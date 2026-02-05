import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ImportBatchDTO } from './connect-importer.model';

interface IConnectImporterForm {
  validationHandler: ValidationHandler;
  element: ImportBatchDTO | undefined;
}
export class ConnectImporterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IConnectImporterForm) {
    super(validationHandler, {
      batchId: new SoeTextFormControl(element?.batchId || '', {
        isIdField: true,
        isNameField: true,
      }),
      importHeadType: new SoeNumberFormControl(
        element?.importHeadType || 0,
        {}
      ),
    });
  }

  get batchId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.batchId;
  }
  get importHeadType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.importHeadType;
  }
}
