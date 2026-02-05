import { Injectable } from '@angular/core';
import { IGetRequestOptions, SoeHttpClient } from './http.service';
import { Observable, tap } from 'rxjs';
import { IDownloadFileDTO } from '@shared/models/generated-interfaces/FileUploadDTO';
import { DownloadUtility } from '@shared/util/download-util';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root',
})
export class DownloadApiSoeHttpClientService {
  constructor(
    private readonly http: SoeHttpClient,
    private readonly messageboxService: MessageboxService,
    private readonly translate: TranslateService
  ) {}

  public get(
    endPoint: string,
    options?: IGetRequestOptions
  ): Observable<IDownloadFileDTO> {
    return this.http.get<IDownloadFileDTO>(endPoint, options).pipe(
      tap((file: IDownloadFileDTO) => {
        this.handleDownload(file);
      })
    );
  }

  public post(
    endPoint: string,
    value: any
  ): Observable<IDownloadFileDTO> {
    return this.http.post<IDownloadFileDTO>(endPoint, value).pipe(
      tap((file: IDownloadFileDTO) => {
        this.handleDownload(file);
      })
    );
  }

  private handleDownload(file: IDownloadFileDTO): void {
    if (file.success && file.fileName && file.content) {
      DownloadUtility.downloadFile(file.fileName, file.fileType, file.content);
    } else if (!file.success && file.errorMessage) {
      this.messageboxService.error(
        this.translate.instant('core.error'),
        file.errorMessage
      );
    }
  }
}
