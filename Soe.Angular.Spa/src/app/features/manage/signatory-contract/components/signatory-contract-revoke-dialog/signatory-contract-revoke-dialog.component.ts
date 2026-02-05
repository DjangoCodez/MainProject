import { Component, inject } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { ValidationHandler } from '@shared/handlers';
import { SignatoryContractRevokeForm } from '../../models/signatory-contract-revoke-form';
import { SignatoryContractRevokeDTO } from '../../models/signatory-contract-revoke-dto';

@Component({
  selector: 'soe-signatory-contract-revoke-dialog',
  standalone: false,
  templateUrl: './signatory-contract-revoke-dialog.component.html',
})
export class SignatoryContractRevokeDialogComponent
  extends DialogComponent<DialogData> 
{
  private readonly validationHandler = inject(ValidationHandler);

  protected readonly form: SignatoryContractRevokeForm;

  constructor() {
    super();
    const element = new SignatoryContractRevokeDTO();
    this.form = new SignatoryContractRevokeForm({
      validationHandler: this.validationHandler,
      element: element,
    });

  }

  protected ok(): void {
    this.dialogRef.close(this.form.revokedReason.value);
  }

}
