import { Component, inject, signal } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  HouseholdPartialAmountDialogData,
  HouseholdPartialAmountForm,
} from './household-partial-amount-modal.model';
import { ValidationHandler } from '@shared/handlers';
import { HouseholdTaxDeductionService } from '@features/billing/household-tax-deduction/services/household-tax-deduction.service';

@Component({
  templateUrl: './household-partial-amount-modal.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class HouseholdPartialAmountModal extends DialogComponent<HouseholdPartialAmountDialogData> {
  validationHandler = inject(ValidationHandler);
  form: HouseholdPartialAmountForm;
  fullAmount: number = 0;
  service = inject(HouseholdTaxDeductionService);

  // Signals
  protected okButtonDisabled = signal(true);

  constructor() {
    super();

    this.form = new HouseholdPartialAmountForm({
      validationHandler: this.validationHandler,
      amount: this.data?.amount || 0,
      createInvoice: false,
    });

    this.fullAmount = this.data?.amount;
  }

  triggerOk() {
    const row = this.form.getRawValue() as HouseholdPartialAmountDialogData;
    this.dialogRef.close({
      amount: row.amount,
      createInvoice: row.createInvoice,
    });
  }

  amountChanged(value: any) {
    this.okButtonDisabled.set(!value || value > this.fullAmount || value < 1);
  }
}
