import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs/internal/Observable';
import { ProjectSearchModel } from '../models/select-project-dialog.model';
import { IProjectSearchResultDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { getProjectsBySearch } from '@shared/services/generated-service-endpoints/billing/InvoiceProject.endpoints';
@Injectable({
  providedIn: 'root',
})
export class SelectProjectService {
  constructor(private http: SoeHttpClient) {}

  getProjectsBySearch(
    model: ProjectSearchModel
  ): Observable<IProjectSearchResultDTO[]> {
    return this.http.post<IProjectSearchResultDTO[]>(
      getProjectsBySearch(),
      model
    );
  }
}
