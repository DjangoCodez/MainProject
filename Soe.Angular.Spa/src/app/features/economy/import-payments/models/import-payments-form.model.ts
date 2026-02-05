import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PaymentImportDTO, PaymentImportIODTO } from './import-payments.model';
import { FormArray } from '@angular/forms';
import { ImportPaymentsIOInvoiceRowForm } from './import-payments-io-invoice-row-form.model';
import { ImportPaymentType } from '@shared/models/generated-interfaces/Enumerations';

interface IImportPaymentsForm {
  validationHandler: ValidationHandler;
  element: PaymentImportDTO | undefined;
}
export class ImportPaymentsForm extends SoeFormGroup {
  paymentImportIOInvoiceValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IImportPaymentsForm) {
    super(validationHandler, {
      paymentImportId: new SoeTextFormControl(element?.paymentImportId || 0, {
        isIdField: true,
      }),
      batchId: new SoeNumberFormControl(element?.batchId || 0, {
        isNameField: true,
      }),

      type: new SoeSelectFormControl(
        element?.type,
        { required: true, zeroNotAllowed: true },
        'economy.import.payment.paymentmethodnew'
      ),

      importType: new SoeSelectFormControl<ImportPaymentType>(
        element?.importType || ImportPaymentType.CustomerPayment,
        { required: true, zeroNotAllowed: true },
        'economy.import.payment.importpaymenttype'
      ),
      sysPaymentTypeId: new SoeTextFormControl(
        element?.sysPaymentTypeId || 0,
        {}
      ),

      totalAmount: new SoeNumberFormControl(element?.totalAmount || 0, {
        maxDecimals: 2,
      }),
      importDate: new SoeDateFormControl(
        element?.importDate || new Date(),
        {
          required: true,
        },
        'common.date'
      ),
      paymentLabel: new SoeTextFormControl(element?.paymentLabel || '', {
        maxLength: 512,
      }),
      filename: new SoeTextFormControl(element?.filename || ''),
      importedIoInvoices: new FormArray<ImportPaymentsIOInvoiceRowForm>([]),
    });
    this.paymentImportIOInvoiceValidationHandler = validationHandler;
  }

  get paymentImportId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentImportId;
  }
  get batchId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.batchId;
  }
  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get importType(): SoeSelectFormControl<ImportPaymentType> {
    return <SoeSelectFormControl<ImportPaymentType>>this.controls.importType;
  }
  get sysPaymentTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.sysPaymentTypeId;
  }
  get totalAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.totalAmount;
  }
  get importDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.importDate;
  }
  get paymentLabel(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentLabel;
  }
  get filename(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.filename;
  }
  get importedIoInvoices(): FormArray<ImportPaymentsIOInvoiceRowForm> {
    return <FormArray>this.controls.importedIoInvoices;
  }

  setDirtyOnPaymentRowChange(rowId: number) {
    this.markAsDirty();
  }

  customPaymentImportIOPatchValue(importedIoInvoices: PaymentImportIODTO[]) {
    (this.controls.importedIoInvoices as FormArray).clear();
    if (importedIoInvoices) {
      for (const importedIoInvoice of importedIoInvoices) {
        if (this.isCopy) {
          importedIoInvoice.paymentImportIOId = 0;
        }
        const row = new ImportPaymentsIOInvoiceRowForm({
          validationHandler: this.paymentImportIOInvoiceValidationHandler,
          element: importedIoInvoice,
        });
        (this.controls.importedIoInvoices as FormArray).push(row, {
          emitEvent: false,
        });
      }
      this.importedIoInvoices.updateValueAndValidity();
    }
  }
}
