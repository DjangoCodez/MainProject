import { Component, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ISupplierAgreementDTO } from '@shared/models/generated-interfaces/SupplierAgreementDTOs';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { SupplierAgreementForm } from '../../../models/supplier-agreement.form.model';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';

export class SupplierAgreementDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  rowToUpdate?: ISupplierAgreementDTO;
  priceLists!: SmallGenericType[];
  wholesellersDict!: SmallGenericType[];
  codeTypes!: SmallGenericType[];
}

@Component({
  selector: 'soe-supplier-agreement-dialog',
  templateUrl: './supplier-agreement-dialog.component.html',
  styleUrls: ['./supplier-agreement-dialog.component.scss'],
  standalone: false,
})
export class SupplierAgreementDialogComponent extends DialogComponent<SupplierAgreementDialogData> {
  validationHandler = inject(ValidationHandler);

  form: SupplierAgreementForm;

  constructor() {
    super();

    this.form = new SupplierAgreementForm({
      validationHandler: this.validationHandler,
      element: this.data.rowToUpdate,
      wholesellersDict: this.data.wholesellersDict,
    });
  }

  cancel() {
    this.dialogRef.close();
  }

  delete() {
    this.dialogRef.close({
      ...this.form.getRawValue(),
      state: SoeEntityState.Deleted,
    });
  }

  protected ok(): void {
    this.dialogRef.close(this.form.getRawValue());
  }
}
