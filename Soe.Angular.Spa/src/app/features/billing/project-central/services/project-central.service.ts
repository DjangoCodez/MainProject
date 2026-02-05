import { Injectable } from '@angular/core';
import { IProjectCentralStatusDTO } from '@shared/models/generated-interfaces/ProjectCentralDTOs';
import { IProjectSearchResultDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getProjectCentralStatus } from '@shared/services/generated-service-endpoints/billing/InvoiceProject.endpoints';
import { Observable, of } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ProjectCentralService {
  constructor(private http: SoeHttpClient) {}

  getGrid(
    id?: number,
    additionalProps?: any
  ): Observable<IProjectSearchResultDTO[]> {
    return of([]);
  }

  save(model: IProjectSearchResultDTO): Observable<BackendResponse> {
    return of();
  }

  delete(id: number): Observable<BackendResponse> {
    return of();
  }

  getProjectCentralStatus(
    projectId: number,
    includeChildProjects: boolean,
    loadDetails: boolean,
    from?: Date,
    to?: Date
  ): Observable<IProjectCentralStatusDTO[]> {
    const dateFrom: string = from ? from.toDateString() : '';
    const dateTo: string = to ? to.toDateString() : '';
    return this.http.get<IProjectCentralStatusDTO[]>(
      getProjectCentralStatus(
        projectId,
        includeChildProjects,
        loadDetails,
        dateFrom,
        dateTo
      )
    );
  }
}
