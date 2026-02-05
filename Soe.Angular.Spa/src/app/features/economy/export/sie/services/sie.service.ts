import { Injectable } from '@angular/core';
import { ISieExportDTO } from '@shared/models/generated-interfaces/SieExportDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import { sieExport } from '@shared/services/generated-service-endpoints/economy/Sie.endpoints';
import { Observable, of } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SieService {
  constructor(private readonly http: SoeHttpClient) {}

  /**
   * @deprecated Not in use. Added to comply with IApiService constraint
   * @param data
   * @returns
   */
  save(data: ISieExportDTO): Observable<BackendResponse> {
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

  export(model: ISieExportDTO): Observable<BackendResponse> {
    return this.http.post(sieExport(), model);
  }
}
