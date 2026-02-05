import { Injectable } from '@angular/core';
import {
  IPayrollLevelDTO,
  IPayrollLevelGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deletePayrollLevel,
  getPayrollLevel,
  getPayrollLevelsGrid,
  savePayrollLevel,
} from '@shared/services/generated-service-endpoints/time/PayrollLevel.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PayrollLevelsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IPayrollLevelGridDTO[]> {
    return this.http.get<IPayrollLevelGridDTO[]>(getPayrollLevelsGrid(id));
  }

  get(id: number): Observable<IPayrollLevelDTO> {
    return this.http.get<IPayrollLevelDTO>(getPayrollLevel(id));
  }

  save(model: IPayrollLevelDTO): Observable<any> {
    return this.http.post<IPayrollLevelDTO>(savePayrollLevel(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deletePayrollLevel(id));
  }
}
