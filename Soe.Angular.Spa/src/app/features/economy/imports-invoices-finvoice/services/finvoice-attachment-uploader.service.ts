import { inject, Injectable } from '@angular/core';
import {
  AttachedFile,
  IFileUploader,
} from '@ui/forms/file-upload/file-upload.component';
import { map, Observable, of, take } from 'rxjs';
import { ImportsInvoicesFinvoiceService } from './imports-invoices-finvoice.service';
import { SoeEntityType } from '@shared/models/generated-interfaces/Enumerations';
import { FInvoiceModel } from '../models/imports-invoices-finvoice.model';
import { ResponseUtil } from '@shared/util/response-util';

@Injectable()
export class FInvoiceAttachmentUploaderService implements IFileUploader {
  readonly service = inject(ImportsInvoicesFinvoiceService);
  uploadFile(
    file: AttachedFile
  ): Observable<{ success: boolean; errorMessage?: string }> {
    if (file.content && file.name) {
      const model: FInvoiceModel = {
        fileName: file.name ?? '',
        fileString: file.content ?? '',
        extention: file.extension ?? '',
        entity: SoeEntityType.None,
      };
      return this.service.attacheFile(model).pipe(
        take(1),
        map(res => {
          if (res.success) {
            return {
              success: true,
            };
          }
          return {
            success: false,
            errorMessage: ResponseUtil.getErrorMessage(res),
          };
        })
      );
    }
    return of({ success: false });
  }
}
