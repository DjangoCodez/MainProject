import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  ICompanyGroupAdministrationDTO,
  ICompanyGroupAdministrationGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteCompanyGroupAdministration,
  getCompanyGroupAdministration,
  getCompanyGroupAdministrationGrid,
  getCompanyGroupMappingHeadsDict,
  getGetChildCompaniesDict,
  saveCompanyGroupAdministration,
} from '@shared/services/generated-service-endpoints/economy/CompanyGroupAdministration.endpoints';
import { Observable } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CompanyGroupAdministrationService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ICompanyGroupAdministrationGridDTO[]> {
    return this.http.get<ICompanyGroupAdministrationGridDTO[]>(
      getCompanyGroupAdministrationGrid(id)
    );
  }

  get(id: number): Observable<ICompanyGroupAdministrationDTO> {
    return this.http.get<ICompanyGroupAdministrationDTO>(
      getCompanyGroupAdministration(id)
    );
  }

  save(model: ICompanyGroupAdministrationDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveCompanyGroupAdministration(),
      model
    );
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(
      deleteCompanyGroupAdministration(id)
    );
  }

  getChildCompanies(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getGetChildCompaniesDict());
  }

  getCompanyGroupMappings(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getCompanyGroupMappingHeadsDict(addEmptyRow)
    );
  }
}
