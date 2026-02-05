import { Injectable } from '@angular/core';
import { ICsrResponseDTO } from '@shared/models/generated-interfaces/CsrResponseDTO';
import { IGetCSRResponseModel } from '@shared/models/generated-interfaces/EconomyModels';
import { IEmployeeCSRExportDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getCsrInquiries,
  getEmployeesForCsrExport,
} from '@shared/services/generated-service-endpoints/time/Csr.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EmployeeCsrExportService {
  constructor(private http: SoeHttpClient) {}

  getEmployeesForCsrExport(year: number): Observable<IEmployeeCSRExportDTO[]> {
    return this.http.get<IEmployeeCSRExportDTO[]>(
      getEmployeesForCsrExport(year)
    );
  }

  getCsrInquiries(model: IGetCSRResponseModel): Observable<ICsrResponseDTO[]> {
    return this.http.post<ICsrResponseDTO[]>(getCsrInquiries(), model);
  }
}
