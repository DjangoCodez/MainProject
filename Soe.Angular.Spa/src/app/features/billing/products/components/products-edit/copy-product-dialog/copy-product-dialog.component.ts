import { Component, inject } from '@angular/core';
import {
  CopyProductDialogData,
  CopyProductSettingsDTO,
  CopyProductSettingsForm,
} from '@features/billing/products/models/copy-product-dialog-data.models';
import { ValidationHandler } from '@shared/handlers';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';

@Component({
  selector: 'soe-copy-product-dialog',
  templateUrl: './copy-product-dialog.component.html',
  styleUrls: ['./copy-product-dialog.component.scss'],
  standalone: false,
})
export class CopyProductDialogComponent extends DialogComponent<CopyProductDialogData> {
  private validationHandler = inject(ValidationHandler);
  protected form = new CopyProductSettingsForm({
    validationHandler: this.validationHandler,
    element: new CopyProductSettingsDTO(),
  });

  protected triggerOk(): void {
    this.data.copyProductSetting = this.form.value as CopyProductSettingsDTO;
    this.dialogRef.close(this.data);
  }
}
