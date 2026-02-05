import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ChangeIntrastatCodeDTO } from './change-intrastat-code.model';

interface IChangeIntrastatCodeForm {
  validationHandler: ValidationHandler;
  element: ChangeIntrastatCodeDTO | undefined;
}
export class ChangeIntrastatCodeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IChangeIntrastatCodeForm) {
    super(validationHandler, {
      code: new SoeSelectFormControl(element?.code || 0),
      transactionType: new SoeSelectFormControl(element?.transactionType || 0),
      country: new SoeSelectFormControl(element?.country || 0),
    });
  }

  get code(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.code;
  }
  get transactionType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.transactionType;
  }
  get country(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.country;
  }
}
