import { Component, inject } from '@angular/core';
import { HouseholdTaxDeductionApplicantDialogData } from './models/household-tax-deduction-Applicant.model';
import { HouseholdTaxDeductionApplicantForm } from './models/household-tax-deduction-applicant-form.model';
import { ValidationHandler } from '@shared/handlers';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';

@Component({
    selector: 'soe-household-tax-deduction-dialog',
    templateUrl: './household-tax-deduction-dialog.component.html',
    styleUrl: './household-tax-deduction-dialog.component.scss',
    standalone: false
})
export class HouseholdTaxDeductionDialogComponent extends DialogComponent<HouseholdTaxDeductionApplicantDialogData> {
  validationHandler = inject(ValidationHandler);

  form: HouseholdTaxDeductionApplicantForm;

  constructor() {
    super();
    this.form = new HouseholdTaxDeductionApplicantForm({
      validationHandler: this.validationHandler,
      element: this.data.rowToUpdate,
    });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  protected ok(): void {
    this.dialogRef.close(this.form.value);
  }
}
