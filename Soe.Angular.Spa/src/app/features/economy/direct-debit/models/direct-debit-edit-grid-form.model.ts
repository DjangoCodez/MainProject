import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InvoiceExportIODTO } from './direct-debit.model';

interface IDirectDebitEditGridForm {
  validationHandler: ValidationHandler;
  element: InvoiceExportIODTO | undefined;
}

export class DirectDebitEditGridForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDirectDebitEditGridForm) {
    super(validationHandler, {
      invoiceExportIOId: new SoeTextFormControl(
        element?.invoiceExportIOId || 0,
        {
          isIdField: true,
        }
      ),
      invoiceExportId: new SoeSelectFormControl(element?.invoiceExportId || 0),
      typeName: new SoeTextFormControl(element?.invoiceType),
      invoiceNo: new SoeTextFormControl(element?.invoiceNr),
      customerName: new SoeTextFormControl(element?.customerName),
      invoiceAmount: new SoeTextFormControl(element?.invoiceAmount),
      invoiceDate: new SoeDateFormControl(element?.invoiceDate),
      paymentDate: new SoeDateFormControl(element?.dueDate),
      bankAccount: new SoeTextFormControl(element?.bankAccount),
    });
  }

  get invoiceExportIOId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceExportIOId;
  }

  get invoiceExportId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceExportId;
  }

  get typeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.typeName;
  }

  get invoiceNo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceNo;
  }

  get customerName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.customerName;
  }

  get invoiceAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceAmount;
  }

  get invoiceDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.invoiceDate;
  }

  get paymentDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.paymentDate;
  }

  get selectedTotal(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.selectedTotal;
  }
}
