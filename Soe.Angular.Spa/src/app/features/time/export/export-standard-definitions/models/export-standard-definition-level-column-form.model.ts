import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ExportDefinitionLevelColumnDTO } from '../../../models/export.model';

interface IExportStandardDefinitionLevelColumnForm {
  validationHandler: ValidationHandler;
  element: ExportDefinitionLevelColumnDTO | undefined;
}
export class ExportStandardDefinitionLevelColumnForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({
    validationHandler,
    element,
  }: IExportStandardDefinitionLevelColumnForm) {
    super(validationHandler, {
      exportDefinitionLevelColumnId: new SoeTextFormControl(
        element?.exportDefinitionLevelColumnId || 0,
        { isIdField: true }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || ''),
      key: new SoeTextFormControl(element?.key || ''),
      defaultValue: new SoeTextFormControl(element?.defaultValue || ''),
      position: new SoeNumberFormControl(element?.position || 0),
      columnLength: new SoeNumberFormControl(element?.columnLength || 0),
      convertValue: new SoeTextFormControl(element?.convertValue || ''),
    });
    this.thisValidationHandler = validationHandler;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get key(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.key;
  }

  get defaultValue(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.defaultValue;
  }

  get position(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.position;
  }

  get columnLength(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.columnLength;
  }

  get convertValue(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.convertValue;
  }
}
