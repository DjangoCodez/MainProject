import { Component, inject, signal } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { ImportAgreementForm } from '../../../models/import-agreement.form.model';
import { StringUtil } from '@shared/util/string-util';
import { TranslateService } from '@ngx-translate/core';
import { SoeSupplierAgreementProvider } from '@shared/models/generated-interfaces/Enumerations';

export enum ImportAgreementDialogContainer {
  SupplierAgreement = 1,
  NetPrices = 2,
}

export class ImportAgreementDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  showHeaderInfo?: boolean;
  priceLists!: SmallGenericType[];
  wholesellersDict!: SmallGenericType[];
  container = ImportAgreementDialogContainer.SupplierAgreement;
}

@Component({
  selector: 'soe-import-agreement-dialog',
  templateUrl: './import-agreement-dialog.component.html',
  styleUrls: ['./import-agreement-dialog.component.scss'],
  standalone: false,
})
export class ImportAgreementDialogComponent extends DialogComponent<ImportAgreementDialogData> {
  wholesellerInfoText = signal('');
  showWholesellerText = signal(false);
  validationHandler = inject(ValidationHandler);
  translateService = inject(TranslateService);

  form: ImportAgreementForm;
  files: AttachedFile[] = [];

  constructor() {
    super();
    this.form = new ImportAgreementForm(this.validationHandler);
  }

  allowMultipleFiles(): boolean {
    return this.form.value.wholesellerId === 20;
  }

  afterFilesAttached(files: AttachedFile[]) {
    this.files = files;
  }

  onWholsellerChange(value: number) {
    if (
      this.data.container ===
        ImportAgreementDialogContainer.SupplierAgreement &&
      (value === SoeSupplierAgreementProvider.Solar ||
        value === SoeSupplierAgreementProvider.Ahlsell)
    ) {
      this.wholesellerInfoText.set(
        this.translateService.instant(
          'billing.invoices.supplieragreement.netpriceinfo'
        )
      );
      this.showWholesellerText.set(true);
    } else if (value === 20) {
      this.wholesellerInfoText.set(
        this.translateService.instant(
          'billing.invoices.supplieragreement.lundainfo'
        )
      );
      this.showWholesellerText.set(true);
    } else {
      this.wholesellerInfoText.set(' ');
      this.showWholesellerText.set(false);
    }
  }

  cancel() {
    this.dialogRef.close();
  }

  protected ok(): void {
    this.dialogRef.close({
      ...this.form.value,
      files: this.files.map(file => {
        return {
          name: file.name,
          bytes: StringUtil.base64ToByteArray(file.content ?? ''),
        };
      }),
    });
  }
}
