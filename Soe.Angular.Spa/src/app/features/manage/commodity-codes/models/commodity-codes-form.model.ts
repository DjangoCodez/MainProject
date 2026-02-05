import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CommodityCodeDTO } from './commodity-codes.model';

interface ICommodityCodesForm {
  validationHandler: ValidationHandler;
  element: CommodityCodeDTO | undefined;
}
export class CommodityCodesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICommodityCodesForm) {
    super(validationHandler, {
      sysIntrastatCodeId: new SoeTextFormControl(
        element?.sysIntrastatCodeId || 0,
        {
          isIdField: true,
        }
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { isNameField: true, required: true, maxLength: 80, minLength: 1 },
        'common.code'
      ),
      text: new SoeTextFormControl(
        element?.text || '',
        { maxLength: 50 },
        'common.description'
      ),
    });
  }

  get sysIntrastatCodeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysIntrastatCodeId;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get text(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.text;
  }
}
