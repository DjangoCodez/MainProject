import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ExportDTO } from './export.model';

interface IExportForm {
  validationHandler: ValidationHandler;
  element: ExportDTO | undefined;
}
export class ExportForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IExportForm) {
    super(validationHandler, {
      exportId: new SoeTextFormControl(element?.exportId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      exportDefinitionId: new SoeSelectFormControl(
        0,
        { required: true, zeroNotAllowed: true },
        'common.export.export.exportdefinition'
      ),
      specialFunctionality: new SoeTextFormControl(
        '',
        { maxLength: 512 },
        'common.export.export.specialfunctionality'
      ),
      module: new SoeTextFormControl(element?.module || 0, {}),
    });
  }

  get exportId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.exportId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
}
