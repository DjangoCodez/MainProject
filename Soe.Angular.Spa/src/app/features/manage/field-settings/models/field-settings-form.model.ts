import { FormArray, FormGroup } from '@angular/forms';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IFieldSettingDTO } from '@shared/models/generated-interfaces/FieldSettingDTO';
import { FieldSettingsRoleRowForm } from './field-settings-role-row-form.model';

interface IFieldSettingsForm {
  validationHandler: ValidationHandler;
  element: IFieldSettingDTO | undefined;
}
export class FieldSettingsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IFieldSettingsForm) {
    super(validationHandler, {
      fieldId: new SoeTextFormControl(element?.fieldId || 0, {
        isIdField: true,
      }),
      formId: new SoeTextFormControl(element?.formId || 0, {}),
      formName: new SoeTextFormControl(
        element?.formName || '',
        {
          isNameField: true,
          required: false,
          disabled: true,
        },
        'core.function'
      ),
      fieldName: new SoeTextFormControl(
        element?.fieldName || '',
        {
          required: false,
          disabled: true,
        },
        'common.field'
      ),
      companySettingVisibleId: new SoeSelectFormControl(
        null,
        {
          required: false,
        },
        'manage.preferences.fieldsettings.shown'
      ),
      companySetting: new FormGroup({
        visible: new SoeSelectFormControl(
          element?.companySetting.visible || null,
          {
            required: false,
          },
          'manage.preferences.fieldsettings.shown'
        ),
      }),
      roleSettings: new FormArray<FieldSettingsRoleRowForm>([]),
    });

    this.thisValidationHandler = validationHandler;
  }

  customPatch(value: IFieldSettingDTO | undefined) {
    if (value) {
      this.patchValue(value, { emitEvent: false });
      this.roleSettings.clear();
      value.roleSettings.forEach(r => {
        this.roleSettings.push(
          new FieldSettingsRoleRowForm({
            validationHandler: this.thisValidationHandler,
            element: r,
          })
        );
      });

      this.updateValueAndValidity();
    }
  }

  get roleSettings(): FormArray<FieldSettingsRoleRowForm> {
    return <FormArray>this.controls.roleSettings;
  }
}
