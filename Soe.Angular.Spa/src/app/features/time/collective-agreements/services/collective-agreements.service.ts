import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  IEmployeeCollectiveAgreementDTO,
  IEmployeeCollectiveAgreementGridDTO,
} from '@shared/models/generated-interfaces/EmployeeCollectiveAgreementDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import {
  deleteEmployeeCollectiveAgreement,
  getEmployeeCollectiveAgreement,
  getEmployeeCollectiveAgreements,
  getEmployeeCollectiveAgreementsDict,
  getEmployeeCollectiveAgreementsGrid,
  saveEmployeeCollectiveAgreement,
} from '@shared/services/generated-service-endpoints/time/EmployeeCollectiveAgreement.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CollectiveAgreementsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IEmployeeCollectiveAgreementGridDTO[]> {
    return this.http.get<IEmployeeCollectiveAgreementGridDTO[]>(
      getEmployeeCollectiveAgreementsGrid(id)
    );
  }

  get(id: number): Observable<IEmployeeCollectiveAgreementDTO> {
    return this.http.get<IEmployeeCollectiveAgreementDTO>(
      getEmployeeCollectiveAgreement(id)
    );
  }

  getEmployeeCollectiveAgreementsDict(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getEmployeeCollectiveAgreementsDict(addEmptyRow)
    );
  }

  save(model: IEmployeeCollectiveAgreementDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveEmployeeCollectiveAgreement(),
      model
    );
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteEmployeeCollectiveAgreement(id));
  }
}
