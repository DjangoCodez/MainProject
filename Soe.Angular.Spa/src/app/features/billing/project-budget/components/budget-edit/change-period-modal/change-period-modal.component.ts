import { Component, inject, signal } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  ChangePeriodDialogData,
  ChangePeriodForm,
} from './change-period-modal.model';
import { ValidationHandler } from '@shared/handlers';
import { HouseholdTaxDeductionService } from '@features/billing/household-tax-deduction/services/household-tax-deduction.service';

@Component({
  templateUrl: './change-period-modal.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class ChangePeriodModal extends DialogComponent<ChangePeriodDialogData> {
  validationHandler = inject(ValidationHandler);
  form: ChangePeriodForm;
  fullAmount: number = 0;
  periodTypes = this.data?.periodTypes || [];

  // Signals
  protected okButtonDisabled = signal(true);

  constructor() {
    super();

    this.form = new ChangePeriodForm({
      validationHandler: this.validationHandler,
      fromDate: this.data?.fromDate || new Date(),
      toDate: this.data?.toDate || new Date(),
      periodType: this.data?.periodType || 0,
    });
    this.periodTypes = this.data?.periodTypes || [];

    this.valueChanged();
  }

  triggerOk() {
    const row = this.form.getRawValue() as ChangePeriodDialogData;
    this.dialogRef.close({
      fromDate: row.fromDate,
      toDate: row.toDate,
      periodType: row.periodType,
    });
  }

  valueChanged() {
    this.okButtonDisabled.set(
      !this.form.fromDate.value ||
        !this.form.toDate.value ||
        !this.form.periodType.value
    );
  }
}
