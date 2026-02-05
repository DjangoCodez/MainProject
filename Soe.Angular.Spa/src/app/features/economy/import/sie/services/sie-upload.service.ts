import { inject, Injectable } from '@angular/core';
import { AttachedFile, IFileUploader } from '@ui/forms/file-upload/file-upload.component';
import { map, Observable, of } from 'rxjs';
import { SieService } from './sie.service';
import { IFileDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISieImportPreviewDTO } from '@shared/models/generated-interfaces/SieImportDTO';

@Injectable()
export class SieUploadService implements IFileUploader {
  sieService = inject(SieService);
  filePreview?: ISieImportPreviewDTO;
  file?: IFileDTO;

  uploadFile(
    file: AttachedFile
  ): Observable<{ success: boolean; errorMessage?: string }> {
    this.filePreview = undefined;
    this.file = undefined;
    if (file.binaryContent && file.name) {
      this.file = {
        bytes: Array.from(new Uint8Array(file.binaryContent)),
        name: file.name,
      };
      return this.sieService.getPreview(this.file).pipe(
        map(preview => {
          this.filePreview = preview;
          return {
            success: true,
          };
        })
      );
    }
    return of({ success: false });
  }
}
