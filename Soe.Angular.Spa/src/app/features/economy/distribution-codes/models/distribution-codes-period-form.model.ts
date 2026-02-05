import { ValidationHandler } from '@shared/handlers';
import { DistributionCodePeriodDTO } from './distribution-codes.model';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface IDistributionCodePeriodForm {
  validationHandler: ValidationHandler;
  element: DistributionCodePeriodDTO | undefined;
}

export class DistributionCodePeriodForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDistributionCodePeriodForm) {
    super(validationHandler, {
      distributionCodePeriodId: new SoeTextFormControl(
        element?.distributionCodePeriodId || 0,
        { isIdField: true }
      ),
      number: new SoeTextFormControl(element?.number || 0),
      percent: new SoeTextFormControl(element?.percent || 0),
      comment: new SoeTextFormControl(element?.comment || ''),
      periodSubTypeName: new SoeTextFormControl(
        element?.periodSubTypeName || ''
      ),
      parentToDistributionCodePeriodId: new SoeSelectFormControl(
        element?.parentToDistributionCodePeriodId || undefined
      ),
    });
  }
}
