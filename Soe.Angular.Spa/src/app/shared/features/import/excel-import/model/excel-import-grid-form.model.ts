import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IFileDTO,
  IHandleBillingRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IExcelImportForm {
  validationHandler: ValidationHandler;
  element: any | undefined;
}

export class ExcelImportForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IExcelImportForm) {
    super(validationHandler, {
      file: new SoeFormControl<IFileDTO>(
        null,
        { required: true },
        null,
        null,
        'economy.import.sie.file'
      ),
      doNotUpdateWithEmptyValues: new SoeCheckboxFormControl(
        element?.doNotUpdateWithEmptyValues || true
      ),
    });
  }

  get file() {
    return this.controls.file as SoeFormControl<IFileDTO>;
  }

  get doNotUpdateWithEmptyValues(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.doNotUpdateWithEmptyValues;
  }
}
