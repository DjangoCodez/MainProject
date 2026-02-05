import { ValidationHandler } from '@shared/handlers';
import { SysCompanyUniqueValueDTO } from '../../../models/sysCompany.model';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface ISysCompanyUniqueValueDTO {
  validationHandler: ValidationHandler;
  element: SysCompanyUniqueValueDTO | undefined;
}

export class SysCompanyUniqueValueForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISysCompanyUniqueValueDTO) {
    super(validationHandler, {
      sysCompanyUniqueValueId: new SoeNumberFormControl(
        element?.sysCompanyUniqueValueId || 0,
        { isIdField: true }
      ),
      uniqueValueType: new SoeNumberFormControl(
        element?.uniqueValueType || undefined,
        { required: true },
        'manage.system.syscompany.uniquevaluetype'
      ),
      value: new SoeTextFormControl(
        element?.value || '',
        { required: true },
        'manage.system.syscompany.value'
      ),
    });
  }

  get sysCompanyUniqueValueId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysCompanyUniqueValueId;
  }

  get uniqueValueType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.uniqueValueType;
  }

  get uniqueValue(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.value;
  }
}
