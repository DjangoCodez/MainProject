import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { exportVatFile } from '@shared/services/generated-service-endpoints/economy/FinishTaxDeclaration.endpoints';
import { Observable, of } from 'rxjs';
import { FinnishTaxExportDTO } from '../models/finnish-tax-export.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class FinnishTaxExportService {
  constructor(private readonly http: SoeHttpClient) {}

  save(model: FinnishTaxExportDTO): Observable<BackendResponse> {
    return of(<BackendResponse>{ success: false, integerValue: 0 });
  }
  delete(id: number): Observable<BackendResponse> {
    return of(<BackendResponse>{ success: false, integerValue: 0 });
  }

  exportVatFile(model: FinnishTaxExportDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(exportVatFile(), model);
  }
}
