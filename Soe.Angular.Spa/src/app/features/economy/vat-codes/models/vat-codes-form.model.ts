import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { VatCodeDTO } from '../../models/vat-code.model';

interface IVatCodeForm {
  validationHandler: ValidationHandler;
  element: VatCodeDTO | undefined;
}
export class VatCodeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IVatCodeForm) {
    super(validationHandler, {
      vatCodeId: new SoeTextFormControl(element?.vatCodeId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 80, minLength: 1 },
        'common.name'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, maxLength: 20, minLength: 1 },
        'common.code'
      ),
      percent: new SoeNumberFormControl(
        element?.percent || 0,
        {
          required: true,
          minValue: 0,
          maxValue: 100,
          decimals: 1,
          minDecimals: 0,
          maxDecimals: 1,
        },
        'common.percentage'
      ),
      accountId: new SoeSelectFormControl(
        element?.accountId,
        { required: true },
        'economy.accounting.vatcode.account'
      ),
      purchaseVATAccountId: new SoeSelectFormControl(
        element?.purchaseVATAccountId || 0
      ),
      accountSysVatRate: new SoeTextFormControl(''),
      purchaseVATAccountSysVatRate: new SoeTextFormControl(''),
    });
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get percent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.percent;
  }

  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }

  get purchaseVATAccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.purchaseVATAccountId;
  }
}
