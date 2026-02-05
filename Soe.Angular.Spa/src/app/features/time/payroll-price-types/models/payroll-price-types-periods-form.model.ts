import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IPayrollPriceTypePeriodDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IPayrollPriceTypesPeriodsForm {
  validationHandler: ValidationHandler;
  element: IPayrollPriceTypePeriodDTO | undefined;
}

export class PayrollPriceTypesPeriodsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPayrollPriceTypesPeriodsForm) {
    super(validationHandler, {
      payrollPriceTypePeriodId: new SoeTextFormControl(
        element?.payrollPriceTypePeriodId || 0,
        { isIdField: true }
      ),
      fromDate: new SoeDateFormControl(
        element?.fromDate || undefined,
        { required: true },
        'common.fromdate'
      ),
      amount: new SoeNumberFormControl(
        element?.amount || 0,
        {
          minValue: -99999999.9999,
          maxValue: 99999999.9999,
        },
        'common.amount'
      ),
    });
  }

  customPatchValue(element: IPayrollPriceTypePeriodDTO) {
    this.patchValue(element);
  }
}
