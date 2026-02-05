import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IUnionFeeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IUnionFeesForm {
  validationHandler: ValidationHandler;
  element: IUnionFeeDTO | undefined;
}

export class UnionFeesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IUnionFeesForm) {
    super(validationHandler, {
      unionFeeId: new SoeTextFormControl(element?.unionFeeId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100 },
        'common.name'
      ),
      payrollProductId: new SoeSelectFormControl(
        element?.payrollProductId || undefined
      ),
      payrollPriceTypeIdPercent: new SoeSelectFormControl(
        element?.payrollPriceTypeIdPercent || undefined
      ),
      payrollPriceTypeIdPercentCeiling: new SoeSelectFormControl(
        element?.payrollPriceTypeIdPercentCeiling || undefined
      ),
      payrollPriceTypeIdFixedAmount: new SoeSelectFormControl(
        element?.payrollPriceTypeIdFixedAmount || undefined
      ),
      association: new SoeSelectFormControl(element?.association || 0),
    });
  }

  get isPercentChosen(): boolean {
    return !!this.controls.payrollPriceTypeIdPercent.value;
  }
}
