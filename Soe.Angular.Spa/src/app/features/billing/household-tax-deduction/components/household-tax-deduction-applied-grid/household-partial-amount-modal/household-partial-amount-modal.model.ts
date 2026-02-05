import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export class HouseholdPartialAmountDialogData implements DialogData {
  title: string;
  amount: number;
  createInvoice: boolean;
  size?: DialogSize;

  constructor() {
    this.title = 'billing.invoices.householddeduction.setseqnrdialogheader';
    this.amount = 0;
    this.createInvoice = false;
    this.size = 'sm';
  }
}

interface IHouseholdPartialAmountForm {
  validationHandler: ValidationHandler;
  amount?: number;
  createInvoice?: boolean;
}

export class HouseholdPartialAmountForm extends SoeFormGroup {
  constructor({
    validationHandler,
    amount,
    createInvoice,
  }: IHouseholdPartialAmountForm) {
    super(validationHandler, {
      amount: new SoeNumberFormControl(amount || 0, {
        maxDecimals: 2,
      }),
      createInvoice: new SoeCheckboxFormControl(createInvoice || false),
    });
  }
}
