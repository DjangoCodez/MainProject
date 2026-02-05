import { Injectable } from '@angular/core';
import {
  IEmployeeGroupTimeLeisureCodeDTO,
  IEmployeeGroupTimeLeisureCodeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteEmployeeGroupTimeLeisureCode,
  getEmployeeGroupTimeLeisureCode,
  getEmployeeGroupTimeLeisureCodesGrid,
  saveEmployeeGroupTimeLeisureCode,
} from '@shared/services/generated-service-endpoints/time/TimeLeisureCode.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LeisureCodesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IEmployeeGroupTimeLeisureCodeGridDTO[]> {
    return this.http.get<IEmployeeGroupTimeLeisureCodeGridDTO[]>(
      getEmployeeGroupTimeLeisureCodesGrid(id)
    );
  }

  get(id: number): Observable<IEmployeeGroupTimeLeisureCodeDTO> {
    return this.http.get<IEmployeeGroupTimeLeisureCodeDTO>(
      getEmployeeGroupTimeLeisureCode(id)
    );
  }

  save(model: IEmployeeGroupTimeLeisureCodeDTO): Observable<any> {
    return this.http.post<IEmployeeGroupTimeLeisureCodeDTO>(
      saveEmployeeGroupTimeLeisureCode(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteEmployeeGroupTimeLeisureCode(id));
  }
}
