import { Component, inject, signal } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  HouseholdSequenceNumberDialogData,
  HouseholdSequenceNumberForm,
} from './household-sequence-number-modal.model';
import { ValidationHandler } from '@shared/handlers';
import { HouseholdTaxDeductionService } from '@features/billing/household-tax-deduction/services/household-tax-deduction.service';

@Component({
  templateUrl: './household-sequence-number-modal.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class HouseholdSequenceNumberModal extends DialogComponent<HouseholdSequenceNumberDialogData> {
  validationHandler = inject(ValidationHandler);
  form: HouseholdSequenceNumberForm;
  lastSequenceNumber: number = 0;
  service = inject(HouseholdTaxDeductionService);

  // Signals
  protected okButtonDisabled = signal(true);

  constructor() {
    super();

    this.form = new HouseholdSequenceNumberForm({
      validationHandler: this.validationHandler,
      sequenceNumber: 0,
    });

    this.service
      .getLastUsedSequenceNumber(this.data.entityName)
      .subscribe(sequenceNumber => {
        this.lastSequenceNumber = sequenceNumber;
        this.form.patchValue({
          sequenceNumber: sequenceNumber ? sequenceNumber + 1 : 1,
        });
        this.onSequenceNumberChange(this.lastSequenceNumber);
      });
  }

  triggerOk() {
    const row = this.form.getRawValue() as HouseholdSequenceNumberDialogData;
    this.dialogRef.close(row.sequenceNumber);
  }

  onSequenceNumberChange(value: any) {
    this.okButtonDisabled.set(!value || value < this.lastSequenceNumber);
  }
}
