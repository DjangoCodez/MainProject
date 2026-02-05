import { Component, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SoeFormGroup } from '@shared/extensions';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { Perform } from '@shared/util/perform.class';
import { ProgressOptions } from '@shared/services/progress';
import { ImportPriceListService } from '../../services/import-price-list.service';
import {
  ImportPriceListUploadDTO,
  SysPriceListImportDTO,
} from '../../models/import-price-list.model';
import { ImportPriceListUploadForm } from '../../models/import-price-list-upload-form.model';
import { CrudActionTypeEnum } from '@shared/enums';
import { tap } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { ISysPricelistProviderDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

export class SysPriceImportDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  providers!: ISysPricelistProviderDTO[];
}

@Component({
  selector: 'soe-import-price-list-upload',
  templateUrl: './import-price-list-upload.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class ImportPriceListUploadComponent extends DialogComponent<SysPriceImportDialogData> {
  validationHandler = inject(ValidationHandler);
  service = inject(ImportPriceListService);
  progressService = inject(ProgressService);
  toasterService = inject(ToasterService);
  messageboxService = inject(MessageboxService);
  translate = inject(TranslateService);
  performUploadImportPriceListFile = new Perform<any>(this.progressService);

  file?: AttachedFile;
  form: ImportPriceListUploadForm = new ImportPriceListUploadForm({
    validationHandler: this.validationHandler,
    element: new ImportPriceListUploadDTO(),
  });
  fileName = this.form.fileName;

  constructor() {
    super();
    this.setProviders();
  }

  setProviders() {
    this.form.patchValue({
      providerId:
        this.data.providers.length > 0 ? this.data.providers[0].id : null,
    });
  }

  afterFilesAttached(files: AttachedFile[]) {
    const file = files && files.length > 0 ? files[0] : null;
    if (file) {
      this.file = file;
      this.form.patchValue({
        fileName: file.name,
      });
    } else {
      this.form.patchValue({
        fileName: null,
      });
      this.file = undefined;
    }
    this.form.markAsDirty();
    this.form.updateValueAndValidity();
  }

  openFormValidationErrors(): void {
    this.validationHandler.showFormValidationErrors(this.form as SoeFormGroup);
  }
  cancel() {
    this.dialogRef.close(false);
  }
  upload() {
    if (this.file && this.file.binaryContent && this.file.name) {
      const file = {
        bytes: Array.from(new Uint8Array(this.file.binaryContent)),
        name: this.file.name,
      };
      const obj = new SysPriceListImportDTO();
      obj.file = file;
      obj.provider = this.form?.providerId.value;

      const options: Partial<ProgressOptions> = {};
      options.showToast = false;
      options.showToastOnComplete = false;
      options.showDialogOnComplete = true;
      this.performUploadImportPriceListFile.crud(
        CrudActionTypeEnum.Save,
        this.service.uploadImportPriceListFile(obj).pipe(
          tap((response: BackendResponse) => {
            if (response.success) {
              // Handle success case
              this.toasterService.success(
                this.translate.instant(
                  'manage.system.import.price.list.success'
                ),
                this.translate.instant('common.status')
              );
            } else {
              // Handle error case
              this.messageboxService.error(
                this.translate.instant('core.error'),
                ResponseUtil.getErrorMessage(response) || ''
              );
            }
            this.dialogRef.close(response.success);
          })
        ),
        undefined,
        undefined,
        options
      );
    }
  }
}
