import { inject, Injectable } from '@angular/core';
import {
  AttachedFile,
  IFileUploader,
} from '@ui/forms/file-upload/file-upload.component';
import { map, Observable, of, take } from 'rxjs';
import { CoreService } from '@shared/services/core.service';
import { SoeEntityType } from '@shared/models/generated-interfaces/Enumerations';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Injectable()
export class FInvoiceFileUploaderService implements IFileUploader {
  readonly coreService = inject(CoreService);

  private dataStorageIds: number[] = [];

  public reset() {
    this.dataStorageIds = [];
  }
  public popDataStorageIds() {
    const ids = this.dataStorageIds;
    this.dataStorageIds = [];
    return ids;
  }

  uploadFile(
    file: AttachedFile
  ): Observable<{ success: boolean; errorMessage?: string }> {
    if (file.binaryContent && file.name) {
      return this.coreService
        .uploadInvoiceFileByEntityType(
          SoeEntityType.None,
          new Uint8Array(file.binaryContent),
          file.name ?? ''
        )
        .pipe(
          take(1),
          map((res: BackendResponse) => {
            res.success &&
              this.dataStorageIds.push(ResponseUtil.getNumberValue(res));
            return {
              success: res.success,
              errorMessage: ResponseUtil.getErrorMessage(res),
            };
          })
        );
    }

    return of({ success: false });
  }
}
