import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, map } from 'rxjs';
import { CompanyGroupMappingHeadDTO } from '../models/company-group-mappings.model';
import {
  checkCompanyGroupMappingHeadNumberIsExists,
  deleteCompanyGroupMappingHead,
  getCompanyGroupMappingHead,
  getCompanyGroupMappingHeads,
  saveCompanyGroupMappingHead,
} from '@shared/services/generated-service-endpoints/economy/CompanyGroupMappings.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CompanyGroupMappingsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<CompanyGroupMappingHeadDTO[]> {
    return this.http.get<CompanyGroupMappingHeadDTO[]>(
      getCompanyGroupMappingHeads(id)
    );
  }

  get(id: number): Observable<CompanyGroupMappingHeadDTO> {
    return this.http
      .get<CompanyGroupMappingHeadDTO>(getCompanyGroupMappingHead(id))
      .pipe(
        map(data => {
          const obj = new CompanyGroupMappingHeadDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  save(model: CompanyGroupMappingHeadDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveCompanyGroupMappingHead(),
      model
    );
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteCompanyGroupMappingHead(id));
  }

  isCompanyGroupMappingHeadNumberExists(
    companyGroupMappingHeadId: number,
    CompanyGroupMappingHeadNumber: number
  ): Observable<boolean> {
    return this.http.get<boolean>(
      checkCompanyGroupMappingHeadNumberIsExists(
        companyGroupMappingHeadId,
        CompanyGroupMappingHeadNumber
      )
    );
  }
}
