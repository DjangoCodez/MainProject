import { inject, Injectable } from '@angular/core';
import { ISieExportDTO } from '@shared/models/generated-interfaces/SieExportDTO';
import {
  ISieImportDTO,
  ISieImportPreviewDTO,
  ISieImportResultDTO,
  ISieReverseImportDTO,
} from '@shared/models/generated-interfaces/SieImportDTO';
import { IFileDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getSieImportHistory,
  reverseImport,
  sieImport,
  sieImportReadFile,
} from '@shared/services/generated-service-endpoints/economy/Sie.endpoints';
import { map, Observable, of } from 'rxjs';
import { FileImportHeadGridDTO } from '../models/sie-import-history.model';
import { TermGroup_FileImportStatus } from '@shared/models/generated-interfaces/Enumerations';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SieService {
  private http = inject(SoeHttpClient);
  /**
   * @deprecated Not in use. Added to comply with IApiService constraint
   * @param data
   * @returns
   */
  save(data: ISieExportDTO) {
    return of(<BackendResponse>{ success: true });
  }

  /**
   * @deprecated Not in use. Added to comply with IApiService constraint
   * @param id
   * @returns
   */
  delete(id: number): Observable<BackendResponse> {
    return of(<BackendResponse>{ success: true });
  }

  /**
   * @deprecated Not in use. Added to comply with IApiService constraint
   * @param id
   * @returns
   */
  getGrid(
    id?: number | undefined,
    additionalProps?: any
  ): Observable<FileImportHeadGridDTO[]> {
    return of(<FileImportHeadGridDTO[]>{});
  }

  import(model: ISieImportDTO): Observable<ISieImportResultDTO> {
    return this.http.post(sieImport(), model);
  }

  getPreview(file: IFileDTO) {
    return this.http.post<ISieImportPreviewDTO>(sieImportReadFile(), file);
  }

  getImportHistory(): Observable<FileImportHeadGridDTO[]> {
    return this.http.get<FileImportHeadGridDTO[]>(getSieImportHistory()).pipe(
      map(data => {
        const currentMaxId = data.reduce((max, row) => {
          if (
            row.fileImportHeadId > max &&
            ![
              TermGroup_FileImportStatus.Error,
              TermGroup_FileImportStatus.Reversed,
            ].includes(row.status)
          ) {
            return row.fileImportHeadId;
          }
          return max;
        }, -1);

        if (currentMaxId > 0)
          data.find(
            x => x.fileImportHeadId === currentMaxId
          )!.showReverseButton = true;
        return data;
      })
    );
  }

  reverseImport(
    reverseImportDto: ISieReverseImportDTO
  ): Observable<BackendResponse> {
    return this.http.post(reverseImport(), reverseImportDto);
  }
}
