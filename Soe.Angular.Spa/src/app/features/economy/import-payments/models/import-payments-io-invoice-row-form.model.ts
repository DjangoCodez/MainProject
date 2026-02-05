import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PaymentImportIODTO } from './import-payments.model';
import {
  ImportPaymentIOStatus,
  ImportPaymentType,
} from '@shared/models/generated-interfaces/Enumerations';

interface IImportPaymentsIOInvoiceRowForm {
  validationHandler: ValidationHandler;
  element: PaymentImportIODTO | undefined;
}
export class ImportPaymentsIOInvoiceRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IImportPaymentsIOInvoiceRowForm) {
    super(validationHandler, {
      paymentImportIOId: new SoeTextFormControl(
        element?.paymentImportIOId || 0,
        {
          isIdField: true,
        }
      ),
      typeName: new SoeTextFormControl(element?.typeName || '', {}),
      customer: new SoeTextFormControl(element?.customer || '', {}),
      invoiceSeqnr: new SoeTextFormControl(element?.invoiceSeqnr || '', {}),
      invoiceNr: new SoeTextFormControl(element?.invoiceNr || '', {}),
      invoiceId: new SoeNumberFormControl(element?.invoiceId || undefined, {}),
      paymentRowId: new SoeNumberFormControl(
        element?.paymentRowId || undefined,
        {}
      ),
      ocr: new SoeTextFormControl(element?.ocr || '', {}),
      dueDate: new SoeDateFormControl(element?.dueDate || undefined, {}),
      paidDate: new SoeDateFormControl(element?.paidDate || undefined, {}),
      paymentTypeName: new SoeTextFormControl(
        element?.paymentTypeName || '',
        {}
      ),
      invoiceAmount: new SoeNumberFormControl(element?.invoiceAmount || 0, {}),
      paidAmount: new SoeNumberFormControl(element?.paidAmount || 0, {}),
      restAmount: new SoeNumberFormControl(element?.restAmount || 0, {}),
      statusName: new SoeTextFormControl(element?.statusName || '', {}),
      status: new SoeNumberFormControl(element?.status || 0, {}),
      statusId: new SoeNumberFormControl(
        element?.statusId || ImportPaymentIOStatus.None,
        {}
      ),
      state: new SoeNumberFormControl(element?.state || 0, {}),
      importType: new SoeTextFormControl(
        element?.importType || ImportPaymentType.CustomerPayment,
        { required: true }
      ),
      paymentRowSeqNr: new SoeTextFormControl(
        element?.paymentRowSeqNr || '',
        {}
      ),
      matchCodeId: new SoeSelectFormControl(
        element?.matchCodeId || 0,
        {},
        'economy.import.payment.matchcode'
      ),
      comment: new SoeTextFormControl(element?.comment || '', {}),
      isSelectDisabled: new SoeCheckboxFormControl(
        element?.isSelectDisabled || false,
        {}
      ),
    });
  }

  get paymentImportId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentImportId;
  }
  get typeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.typeName;
  }
  get customer(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.customer;
  }
  get invoiceSeqnr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceNr;
  }
  get ocr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.ocr;
  }
  get invoiceNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceNr;
  }
  get dueDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dueDate;
  }
  get paidDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paidDate;
  }
  get paymentTypeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentTypeName;
  }
  get invoiceAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.invoiceAmount;
  }
  get paidAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.paidAmount;
  }
  get restAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.restAmount;
  }
  get importType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.importType;
  }
  get statusName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.statusName;
  }
  get statusId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.statusId;
  }
  get state(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.state;
  }
  get invoiceId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.invoiceId;
  }
  get paymentRowId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.paymentRowId;
  }
  get paymentRowSeqNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentRowSeqNr;
  }
  get matchCodeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.matchCodeId;
  }
  get comment(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.comment;
  }
  get isSelectDisabled(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isSelectDisabled;
  }
}
