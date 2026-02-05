import { Component, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { DeleteAgreementForm } from '../../../models/delete-agreement.form.model';

export class DeleteAgreementDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  priceLists!: SmallGenericType[];
  wholesellersDict!: SmallGenericType[];
}

@Component({
  selector: 'soe-delete-agreement-dialog',
  templateUrl: './delete-agreement-dialog.component.html',
  styleUrls: ['./delete-agreement-dialog.component.scss'],
  standalone: false,
})
export class DeleteAgreementDialogComponent extends DialogComponent<DeleteAgreementDialogData> {
  validationHandler = inject(ValidationHandler);

  form: DeleteAgreementForm;

  constructor() {
    super();

    this.form = new DeleteAgreementForm(this.validationHandler);
  }

  cancel() {
    this.dialogRef.close();
  }

  protected ok(): void {
    this.dialogRef.close(this.form.value);
  }
}
