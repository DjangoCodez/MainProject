import { Injectable } from '@angular/core';
import {
  deleteTimeWorkReductionReconciliationYear,
  getTimeWorkReduction,
  getTimeWorkReductionsGrid,
  saveTimeWorkReductionReconciliationYear,
} from '@shared/services/generated-service-endpoints/time/TimeWorkReduction.endpoints';
import { Observable } from 'rxjs';
import { ITimeWorkReductionReconciliationYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';

@Injectable({
  providedIn: 'root',
})
export class TimeWorkReductionYearService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeWorkReductionReconciliationYearDTO[]> {
    return this.http.get<ITimeWorkReductionReconciliationYearDTO[]>(
      getTimeWorkReductionsGrid(id)
    );
  }

  get(id: number): Observable<ITimeWorkReductionReconciliationYearDTO> {
    return this.http.get<ITimeWorkReductionReconciliationYearDTO>(
      getTimeWorkReduction(id)
    );
  }

  saveYear(model: ITimeWorkReductionReconciliationYearDTO): Observable<any> {
    return this.http.post<ITimeWorkReductionReconciliationYearDTO>(
      saveTimeWorkReductionReconciliationYear(),
      model
    );
  }

  deleteYear(id: number): Observable<any> {
    return this.http.delete(deleteTimeWorkReductionReconciliationYear(id));
  }
}
