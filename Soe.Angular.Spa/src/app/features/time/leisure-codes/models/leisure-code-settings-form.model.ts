import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmployeeGroupTimeLeisureCodeSettingDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ILeisureCodeSettingsForm {
  validationHandler: ValidationHandler;
  element: IEmployeeGroupTimeLeisureCodeSettingDTO | undefined;
}
export class LeisureCodeSettingsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ILeisureCodeSettingsForm) {
    super(validationHandler, {
      employeeGroupTimeLeisureCodeSettingId: new SoeNumberFormControl(
        element?.employeeGroupTimeLeisureCodeSettingId || 0,
        {
          isIdField: true,
        }
      ),
      type: new SoeSelectFormControl(element?.type || ''),
      name: new SoeTextFormControl(element?.name || ''),
      dataType: new SoeNumberFormControl(element?.dataType || ''),
      strData: new SoeTextFormControl(element?.strData || ''),
      intData: new SoeNumberFormControl(element?.intData || ''),
      decimalData: new SoeNumberFormControl(element?.decimalData || ''),
      boolData: new SoeCheckboxFormControl(element?.boolData || ''),
      dateData: new SoeDateFormControl(element?.dateData || ''),
      timeData: new SoeTextFormControl(element?.timeData || ''),
      settingValue: new SoeTextFormControl(element?.settingValue || ''),
    });
  }

  get employeeGroupTimeLeisureCodeSettingId(): SoeNumberFormControl {
    return <SoeNumberFormControl>(
      this.controls.employeeGroupTimeLeisureCodeSettingId
    );
  }

  get typeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.typeName;
  }

  get settingValue(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.settingValue;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get dataTypeControl(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dataType;
  }

  get strData(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.strData;
  }

  get intData(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.intData;
  }

  get decimalData(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.decimalData;
  }

  get boolData(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.boolData;
  }

  get dateData(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateData;
  }

  get timeData(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeData;
  }
}
