import { FormArray } from '@angular/forms';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IEmployeeGroupTimeLeisureCodeDTO,
  IEmployeeGroupTimeLeisureCodeSettingDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { LeisureCodeSettingsForm } from './leisure-code-settings-form.model';
import { SettingDataType } from '@shared/models/generated-interfaces/Enumerations';

interface ILeisureCodesForm {
  validationHandler: ValidationHandler;
  element: IEmployeeGroupTimeLeisureCodeDTO | undefined;
}
export class LeisureCodesForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ILeisureCodesForm) {
    super(validationHandler, {
      employeeGroupTimeLeisureCodeId: new SoeNumberFormControl(
        element?.employeeGroupTimeLeisureCodeId || 0,
        {
          isIdField: true,
        }
      ),
      employeeGroupId: new SoeSelectFormControl(
        element?.employeeGroupId || '',
        { required: false },
        'time.employee.employeegroup.employeegroup'
      ),
      timeLeisureCodeId: new SoeSelectFormControl(
        element?.timeLeisureCodeId || '',
        { required: true },
        'time.schedule.leisurecode.leisurecodetype'
      ),
      dateFrom: new SoeDateFormControl(
        element?.dateFrom || '',
        { required: true },
        'time.schedule.leisurecode.datefrom'
      ),
      settings: new FormArray<LeisureCodeSettingsForm>([]),
    });

    this.thisValidationHandler = validationHandler;
  }

  customPatchValue(element: IEmployeeGroupTimeLeisureCodeDTO) {
    this.patchValue(element);

    if (!element.dateFrom) this.dateFrom.patchValue('');
    if (!element.employeeGroupId) this.employeeGroupId.patchValue('');

    if (element.employeeGroupId == 0)
      this.controls.employeeGroupId.setValue(null);

    // Settings
    this.settings.clear({ emitEvent: false });
    element.settings.forEach(r => {
      const settingsForm = new LeisureCodeSettingsForm({
        validationHandler: this.thisValidationHandler,
        element: r,
      });

      settingsForm.patchValue(r);
      //this.setValues(settingsForm);
      this.settings.push(settingsForm, { emitEvent: false });
    });
    this.settings.markAsUntouched({ onlySelf: true });
    this.settings.markAsPristine({ onlySelf: true });
    this.settings.updateValueAndValidity();
  }

  addSettingForm(settingForm: LeisureCodeSettingsForm | undefined) {
    //this.setValues(settingForm);
    this.settings.push(
      settingForm ??
        new LeisureCodeSettingsForm({
          validationHandler: this.thisValidationHandler,
          element: {} as IEmployeeGroupTimeLeisureCodeSettingDTO,
        })
    );
  }

  removeSettingForm(settingForm: LeisureCodeSettingsForm) {
    this.settings.value.forEach((el: LeisureCodeSettingsForm, i: number) => {
      el.type === settingForm.controls.type.value && this.settings.removeAt(i);
    });
  }

  setValues(settingForm: LeisureCodeSettingsForm | undefined) {
    if (settingForm) {
      if (settingForm.controls.dataType.value == SettingDataType.String)
        settingForm.controls.settingValue.setValue(
          settingForm.controls.strData.value
        );
      else if (settingForm.controls.dataType.value == SettingDataType.Integer)
        settingForm.controls.settingValue.setValue(
          settingForm.controls.intData.value
        );
      else if (settingForm.controls.dataType.value == SettingDataType.Decimal)
        settingForm.controls.settingValue.setValue(
          settingForm.controls.decimalData.value
        );
      else if (settingForm.controls.dataType.value == SettingDataType.Boolean)
        settingForm.controls.settingValue.setValue(
          settingForm.controls.boolData.value
        );
      else if (settingForm.controls.dataType.value == SettingDataType.Date)
        settingForm.controls.settingValue.setValue(
          settingForm.controls.dateData.value
        );
      else if (settingForm.controls.dataType.value == SettingDataType.Time)
        settingForm.controls.settingValue.setValue(
          settingForm.controls.timeData.value
        );
    }
  }

  get employeeGroupimeLeisureCodeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.employeeGroupimeLeisureCodeId;
  }

  get employeeGroupId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.employeeGroupId;
  }

  get timeLeisureCodeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.timeLeisureCodeId;
  }

  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }

  get settings(): FormArray<LeisureCodeSettingsForm> {
    return <FormArray>this.controls.settings;
  }
}
