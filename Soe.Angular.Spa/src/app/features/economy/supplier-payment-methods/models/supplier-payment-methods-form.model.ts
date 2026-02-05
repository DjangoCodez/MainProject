import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PaymentMethodDTO } from './supplier-payment-methods.model';

interface ISupplierPaymentMethodsForm {
  validationHandler: ValidationHandler;
  element: PaymentMethodDTO | undefined;
}

export class SupplierPaymentMethodsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISupplierPaymentMethodsForm) {
    super(validationHandler, {
      paymentMethodId: new SoeTextFormControl(element?.paymentMethodId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      sysPaymentMethodId: new SoeSelectFormControl(
        element?.sysPaymentMethodId || undefined,
        {
          required: true,
        },
        'economy.common.paymentmethods.exporttype'
      ),
      payerBankId: new SoeTextFormControl(
        element?.payerBankId || '',
        { maxLength: 20 },
        'economy.common.paymentmethods.payerbankid'
      ),

      paymentInformationRowId: new SoeSelectFormControl(
        element?.paymentInformationRowId || undefined,
        {
          required: true,
        },
        'economy.common.paymentmethods.paymentnr'
      ),
      customerNr: new SoeTextFormControl(
        element?.customerNr || '',
        { maxLength: 100 },
        'economy.common.paymentmethods.customernr'
      ),

      accountId: new SoeSelectFormControl(
        element?.accountId || undefined,
        {
          required: true,
        },
        'economy.common.paymentmethods.accountnr'
      ),
      paymentType: new SoeTextFormControl(element?.paymentType || 0, {}),
    });
  }

  get paymentMethodId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentMethodId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get payerBankId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.payerBankId;
  }
  get sysPaymentMethodId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysPaymentMethodId;
  }
  get paymentInformationRowId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.paymentInformationRowId;
  }
  get customerNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.customerNr;
  }
  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }
  get paymentType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentType;
  }
}
