import { ValidationHandler } from '@shared/handlers';
import { SysCompanySettingDTO } from '../../../models/sysCompany.model';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface ISysCompanySettingForm {
  validationHandler: ValidationHandler;
  element: SysCompanySettingDTO | undefined;
}

export class SysCompanySettingForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISysCompanySettingForm) {
    super(validationHandler, {
      sysCompanySettingId: new SoeTextFormControl(
        element?.sysCompanySettingId || 0,
        {
          isIdField: true,
        }
      ),
      sysCompanyId: new SoeTextFormControl(element?.sysCompanyId || 0),
      settingType: new SoeNumberFormControl(
        element?.settingType || undefined,
        { required: true },
        'common.settingtype'
      ),
      stringValue: new SoeTextFormControl(element?.stringValue || ''),
      intValue: new SoeNumberFormControl(element?.intValue || undefined),
      boolValue: new SoeCheckboxFormControl(element?.boolValue || undefined),
      decimalValue: new SoeNumberFormControl(
        element?.decimalValue || undefined
      ),
    });
  }

  get sysCompanySettingId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysCompanySettingId;
  }

  get sysCompanyId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysCompanyId;
  }

  get settingType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.settingType;
  }

  get stringValue(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stringValue;
  }

  get intValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.intValue;
  }

  get boolValue(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.boolValue;
  }

  get decimalValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.decimalValue;
  }
}
