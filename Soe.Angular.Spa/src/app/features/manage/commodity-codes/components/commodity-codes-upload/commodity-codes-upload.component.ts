import { Component, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { CommodityCodesUploadForm } from '../../models/commodity-codes-upload-form.model';
import { CommodityCodeUploadDTO } from '../../models/commodity-codes.model';
import { SoeFormGroup } from '@shared/extensions';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { tap } from 'rxjs';
import { CommodityCodesService } from '../../services/commodity-codes.service';
import { Perform } from '@shared/util/perform.class';
import { CrudActionTypeEnum } from '@shared/enums';
import { ProgressOptions } from '@shared/services/progress';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-commodity-codes-upload',
  templateUrl: './commodity-codes-upload.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class CommodityCodesUploadComponent extends DialogComponent<DialogData> {
  validationHandler = inject(ValidationHandler);
  service = inject(CommodityCodesService);
  progressService = inject(ProgressService);
  performUploadCommodityCodesFile = new Perform<any>(this.progressService);

  form: CommodityCodesUploadForm = new CommodityCodesUploadForm({
    validationHandler: this.validationHandler,
    element: new CommodityCodeUploadDTO(),
  });
  fileName = this.form.fileName;

  selectedDateOnChange(value?: Date) {
    if (value) {
      this.form?.patchValue({
        year: value.getFullYear(),
        selectedDate: new Date(value.getFullYear(), 0, 1),
      });
      this.form?.selectedDate.setValue(new Date(value.getFullYear(), 0, 1), {
        onlySelf: true,
        emitEvent: false,
      });
    } else {
      this.form?.patchValue({
        year: undefined,
        selectedDate: undefined,
      });
      this.form?.selectedDate.setValue(undefined, {
        onlySelf: true,
        emitEvent: false,
      });
    }
  }

  afterFilesAttached(files: AttachedFile[]) {
    const file = files && files.length > 0 ? files[0] : null;
    if (file) {
      this.form.patchValue({
        fileName: file.name,
        fileString: file.content,
      });
    }
  }

  openFormValidationErrors(): void {
    this.validationHandler.showFormValidationErrors(this.form as SoeFormGroup);
  }
  cancel() {
    this.dialogRef.close(false);
  }
  upload() {
    const obj = new CommodityCodeUploadDTO();
    obj.fileString = this.form?.value.fileString;
    obj.year = this.form?.value.year;
    obj.selectedDate = this.form?.value.selectedDate;
    obj.fileName = this.form?.fileName.value;

    const options: Partial<ProgressOptions> = {};
    options.showToast = false;
    options.showToastOnComplete = false;
    options.showDialogOnComplete = true;

    this.performUploadCommodityCodesFile.crud(
      CrudActionTypeEnum.Save,
      this.service
        .uploadCommodityCodesFile(obj)
        .pipe(
          tap((response: BackendResponse) =>
            this.updateStatesAndEmitChange(response, options)
          )
        ),
      undefined,
      undefined,
      options
    );
  }

  updateStatesAndEmitChange = (
    response: BackendResponse,
    option: ProgressOptions
  ) => {
    if (response.success) {
      option.message = ResponseUtil.getErrorMessage(response);
      this.dialogRef.close(true);
    }
  };
}
