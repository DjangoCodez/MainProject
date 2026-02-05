import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { FilesLookupDTO, ImportFileDTO } from '@shared/models/file.model';
import {
  SoeEntityImageType,
  SoeEntityType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { ResponseUtil } from '@shared/util/response-util';
import {
  AttachedFile,
  IFileUploader,
} from '@ui/forms/file-upload/file-upload.component';
import { map, Observable, of } from 'rxjs';

export class FileUploader implements IFileUploader {
  public fileLookup = new FilesLookupDTO(SoeEntityType.XEConnectImport, []);
  constructor(private coreService: CoreService) {}
  uploadFile(
    file: AttachedFile
  ): Observable<{ success: boolean; message?: string }> {
    if (!file.binaryContent) return of({ success: false });

    return this.coreService
      .uploadInvoiceFile(
        SoeEntityType.XEConnectImport,
        SoeEntityImageType.Unknown,
        0,
        file.binaryContent,
        file.name || '',
        true
      )
      .pipe(
        map(res => {
          console.log('res', res);
          if (Array.isArray(res)) {
            const extracted = res.length > 1;
            let success = true;
            let message = '';
            res.forEach(r => {
              if (!r.success) {
                success = false;
                message = ResponseUtil.getErrorMessage(r) || '';
              } else {
                const fileCopy = {
                  id: file.id,
                  name: ResponseUtil.getStringValue(r),
                  size: extracted ? undefined : file.size,
                  extension:
                    ResponseUtil.getStringValue(r).split('.').pop() || '',
                };
                this.addFileLookup(r, fileCopy);
              }
            });
            return {
              success,
              errorMessage: message,
            };
          } else {
            if (res.success) {
              this.addFileLookup(res, file);
            }
            return {
              success: res.success,
              errorMessage: ResponseUtil.getErrorMessage(res),
            };
          }
        })
      );
  }

  addFileLookup(res: BackendResponse, file: AttachedFile) {
    this.fileLookup.files.push(
      new ImportFileDTO(
        ResponseUtil.getNumberValue(res),
        ResponseUtil.getStringValue(res),
        file
      )
    );
  }
}
