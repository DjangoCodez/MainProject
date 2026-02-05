import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IRoleFieldSettingDTO } from '@shared/models/generated-interfaces/FieldSettingDTO';

interface IFieldSettingsRoleRowForm {
  validationHandler: ValidationHandler;
  element: IRoleFieldSettingDTO | undefined;
}
export class FieldSettingsRoleRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IFieldSettingsRoleRowForm) {
    super(validationHandler, {
      roleId: new SoeTextFormControl(element?.roleId || null),
      roleName: new SoeTextFormControl(element?.roleName || ''),
      visible: new SoeSelectFormControl(element?.visible),
      visibleId: new SoeNumberFormControl(null),
    });
  }
}
