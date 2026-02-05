import { Injectable } from '@angular/core';
import { IProjectTimeMatrixSaveDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  loadProjectTimeBlockForMatrix,
  saveProjectMatrix,
} from '@shared/services/generated-service-endpoints/billing/ProjectTime.endpoints';
import { map, Observable, of } from 'rxjs';
import { ProjectTimeMatrixDTO } from '../models/project-time-report.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ProjectWeekReportService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ProjectTimeMatrixDTO[]> {
    return of([]);
  }

  loadProjectTimeBlockForMatrix(
    employeeId: number,
    selectedEmployeeId: number,
    dateFrom: string,
    dateTo: string,
    isCopying: boolean
  ): Observable<ProjectTimeMatrixDTO[]> {
    return this.http
      .get<
        ProjectTimeMatrixDTO[]
      >(loadProjectTimeBlockForMatrix(employeeId, selectedEmployeeId, dateFrom, dateTo, isCopying))
      .pipe(
        map((p: ProjectTimeMatrixDTO[]) => {
          p.forEach(item => {
            let nrName = '';
            if (item.invoiceNr) {
              nrName = item.invoiceNr + ' - ';
            }
            nrName += item.customerName ?? '';
            item.invoiceNrName = nrName;
          });
          return p;
        })
      );
  }

  save(model: IProjectTimeMatrixSaveDTO[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveProjectMatrix(), model);
  }
}
