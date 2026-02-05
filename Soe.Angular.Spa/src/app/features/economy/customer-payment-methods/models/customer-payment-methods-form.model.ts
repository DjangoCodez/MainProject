import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PaymentMethodDTO } from './customer-payment-methods.model';
import { Validators } from '@angular/forms';

interface ICustomerPaymentMethodsForm {
  validationHandler: ValidationHandler;
  element: PaymentMethodDTO | undefined;
}
export class CustomerPaymentMethodsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICustomerPaymentMethodsForm) {
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
        'economy.common.paymentmethods.importtype'
      ),
      useInCashSales: new SoeCheckboxFormControl(
        element?.useInCashSales || false
      ),

      paymentInformationRowId: new SoeSelectFormControl(
        element?.paymentInformationRowId || undefined,
        {
          required: true,
        },
        'economy.common.paymentmethods.paymentnr'
      ),
      accountId: new SoeSelectFormControl(
        element?.accountId || undefined,
        {
          required: true,
        },
        'economy.common.paymentmethods.accountnr'
      ),
      useRoundingInCashSales: new SoeCheckboxFormControl(
        element?.useRoundingInCashSales || false
      ),
      transactionCode: new SoeNumberFormControl(
        element?.transactionCode || 0,
        {},
        'common.percentage'
      ),
    });

    this.useInCashSales.valueChanges.subscribe(_ => {
      this.updateValidators();
    });
  }

  get paymentMethodId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentMethodId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get sysPaymentMethodId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysPaymentMethodId;
  }

  get useInCashSales(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useInCashSales;
  }

  get paymentInformationRowId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.paymentInformationRowId;
  }
  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }
  get useRoundingInCashSales(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useRoundingInCashSales;
  }
  get transactionCode(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.transactionCode;
  }

  private updateValidators(): void {
    this.sysPaymentMethodId.clearValidators();
    this.paymentInformationRowId.clearValidators();
    if (!this.useInCashSales.value) {
      this.sysPaymentMethodId.addValidators(Validators.required);
      this.paymentInformationRowId.addValidators(Validators.required);
    }
    this.sysPaymentMethodId.updateValueAndValidity();
    this.paymentInformationRowId.updateValueAndValidity();
    this.updateValueAndValidity();
  }
}
