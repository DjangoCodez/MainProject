import { ValidationHandler } from '@shared/handlers';
import { DistributionSalesEuFilterDTO } from './distribution-sales-eu.model';
import {
  SoeFormGroup,
  SoeRadioFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';

interface IDistributionSalesEuGridFilterForm {
  validationHandler: ValidationHandler;
  element: DistributionSalesEuFilterDTO;
}

export class DistributionSalesEuGridFilterForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IDistributionSalesEuGridFilterForm) {
    super(validationHandler, {
      accountYear: new SoeSelectFormControl(element?.accountYear || 0),
      reportPeriod: new SoeRadioFormControl(element?.reportPeriod || 0),
      fromInterval: new SoeSelectFormControl(element?.fromInterval || 0),
      toInterval: new SoeSelectFormControl(element?.toInterval || 0),
    });
  }

  get accountYear(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountYear;
  }

  get reportPeriod(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.reportPeriod;
  }

  get fromInterval(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.fromInterval;
  }

  get toInterval(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.toInterval;
  }
}
