import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  SoeTimeCodeType,
  TermGroup_ExpenseType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeCodeSaveDTO,
  ITimeCodePayrollProductDTO,
  ITimeCodeInvoiceProductDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { arrayToFormArray } from '@shared/util/form-util';
import { TimeCodePayrollProductForm } from '@shared/components/time/time-code-payroll-products-grid/models/time-code-payroll-product-form.model';
import { TimeCodeInvoiceProductForm } from '@shared/components/time/time-code-invoice-products-grid/models/time-code-invoice-product-form.model';

interface ITimeCodeAdditionDeductionForm {
  validationHandler: ValidationHandler;
  element: ITimeCodeSaveDTO | undefined;
}

export class TimeCodeAdditionDeductionForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ITimeCodeAdditionDeductionForm) {
    super(validationHandler, {
      type: new SoeTextFormControl(SoeTimeCodeType.AdditionDeduction),
      timeCodeId: new SoeTextFormControl(element?.timeCodeId || 0, {
        isIdField: true,
      }),
      expenseType: new SoeSelectFormControl(
        element?.expenseType || TermGroup_ExpenseType.Mileage
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, maxLength: 20, minLength: 1 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      registrationType: new SoeSelectFormControl(
        element?.registrationType || undefined,
        { required: true, zeroNotAllowed: true },
        'time.time.timecode.registrationtype'
      ),
      fixedQuantity: new SoeNumberFormControl(
        element?.fixedQuantity || 0,
        {
          minValue: 0,
          maxValue: 99999999.9999,
        },
        'time.time.timecode.fixedquantity'
      ),
      hideForEmployee: new SoeCheckboxFormControl(
        element?.hideForEmployee || false
      ),
      showInTerminal: new SoeCheckboxFormControl(
        element?.showInTerminal || false
      ),
      stopAtDateStart: new SoeCheckboxFormControl(
        element?.stopAtDateStart || false
      ),
      stopAtDateStop: new SoeCheckboxFormControl(
        element?.stopAtDateStop || false
      ),
      stopAtPrice: new SoeCheckboxFormControl(element?.stopAtPrice || false),
      stopAtVat: new SoeCheckboxFormControl(element?.stopAtVat || false),
      stopAtComment: new SoeCheckboxFormControl(
        element?.stopAtComment || false
      ),
      commentMandatory: new SoeCheckboxFormControl(
        element?.commentMandatory || false
      ),
      stopAtAccounting: new SoeCheckboxFormControl(
        element?.stopAtAccounting || false
      ),
      payrollProducts: arrayToFormArray(element?.payrollProducts || []),
      invoiceProducts: arrayToFormArray(element?.invoiceProducts || []),
    });

    this.thisValidationHandler = validationHandler;
  }

  get payrollProducts(): FormArray<TimeCodePayrollProductForm> {
    return <FormArray>this.controls.payrollProducts;
  }

  get invoiceProducts(): FormArray<TimeCodeInvoiceProductForm> {
    return <FormArray>this.controls.invoiceProducts;
  }

  customPatchValue(element: ITimeCodeSaveDTO) {
    this.patchValue(element);
    this.patchPayrollProducts(element.payrollProducts);
    this.patchInvoiceProducts(element.invoiceProducts);
  }

  patchPayrollProducts(payrollProducts: ITimeCodePayrollProductDTO[]) {
    this.payrollProducts.clear({ emitEvent: false });
    payrollProducts.forEach(payrollProduct => {
      const payrollProductForm = new TimeCodePayrollProductForm({
        validationHandler: this.thisValidationHandler,
        element: payrollProduct,
      });
      payrollProductForm.customPatchValue(payrollProduct);
      this.payrollProducts.push(payrollProductForm, { emitEvent: false });
    });
    this.payrollProducts.markAsUntouched({
      onlySelf: true,
    });
    this.payrollProducts.markAsPristine({
      onlySelf: true,
    });
    this.payrollProducts.updateValueAndValidity();
  }

  patchInvoiceProducts(invoiceProducts: ITimeCodeInvoiceProductDTO[]) {
    this.invoiceProducts.clear({ emitEvent: false });
    invoiceProducts.forEach(invoiceProduct => {
      const invoiceProductForm = new TimeCodeInvoiceProductForm({
        validationHandler: this.thisValidationHandler,
        element: invoiceProduct,
      });
      invoiceProductForm.customPatchValue(invoiceProduct);
      this.invoiceProducts.push(invoiceProductForm, { emitEvent: false });
    });
    this.invoiceProducts.markAsUntouched({
      onlySelf: true,
    });
    this.invoiceProducts.markAsPristine({
      onlySelf: true,
    });
    this.invoiceProducts.updateValueAndValidity();
  }
}
