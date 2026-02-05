import { Injectable } from '@angular/core';
import {
  deleteTimeWorkReductionReconciliation,
  deleteTimeWorkReductionReconciliationYear,
  getTimeAccumulatorsForReductionDict,
  getTimeWorkReduction,
  getTimeWorkReductionReconciliationEmployee,
  getTimeWorkReductionsGrid,
  saveTimeWorkReductionReconciliation,
  saveTimeWorkReductionReconciliationYear,
  calculateYearEmployee,
  generateOutcome,
  reverseTransactions,
  getPensionExport,
} from '@shared/services/generated-service-endpoints/time/TimeWorkReduction.endpoints';
import { Observable } from 'rxjs';
import {
  ITimeWorkReductionReconciliationDTO,
  ITimeWorkReductionReconciliationEmployeeDTO,
  ITimeWorkReductionReconciliationGridDTO,
  ITimeWorkReductionReconciliationYearDTO,
  ITimeWorkReductionReconciliationEmployeeModel,
  ITimeWorkReductionReconciliationGenerateOutcomeModel,
  ITimeWorkReductionExportPensionDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Injectable({
  providedIn: 'root',
})
export class TimeWorkReductionService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeWorkReductionReconciliationGridDTO[]> {
    return this.http.get<ITimeWorkReductionReconciliationGridDTO[]>(
      getTimeWorkReductionsGrid(id)
    );
  }

  get(id: number): Observable<ITimeWorkReductionReconciliationDTO> {
    return this.http.get<ITimeWorkReductionReconciliationDTO>(
      getTimeWorkReduction(id)
    );
  }

  getTimeAccumulatorsForReductionDict(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getTimeAccumulatorsForReductionDict()
    );
  }
  getEmployees(
    yearId: number
  ): Observable<ITimeWorkReductionReconciliationEmployeeDTO[]> {
    return this.http.get<ITimeWorkReductionReconciliationEmployeeDTO[]>(
      getTimeWorkReductionReconciliationEmployee(yearId)
    );
  }

  save(model: ITimeWorkReductionReconciliationDTO): Observable<any> {
    return this.http.post<ITimeWorkReductionReconciliationDTO>(
      saveTimeWorkReductionReconciliation(),
      model
    );
  }
  saveYear(model: ITimeWorkReductionReconciliationYearDTO): Observable<any> {
    return this.http.post<ITimeWorkReductionReconciliationYearDTO>(
      saveTimeWorkReductionReconciliationYear(),
      model
    );
  }
  delete(id: number): Observable<any> {
    return this.http.delete(deleteTimeWorkReductionReconciliation(id));
  }
  deleteYear(id: number): Observable<any> {
    return this.http.delete(deleteTimeWorkReductionReconciliationYear(id));
  }

  calculate(
    model: ITimeWorkReductionReconciliationEmployeeModel
  ): Observable<any> {
    return this.http.post<ITimeWorkReductionReconciliationEmployeeModel>(
      calculateYearEmployee(),
      model
    );
  }

  generateOutcome(
    model: ITimeWorkReductionReconciliationGenerateOutcomeModel
  ): Observable<any> {
    return this.http.post<ITimeWorkReductionReconciliationGenerateOutcomeModel>(
      generateOutcome(),
      model
    );
  }
  reverseTransactions(
    model: ITimeWorkReductionReconciliationGenerateOutcomeModel
  ): Observable<any> {
    return this.http.post<ITimeWorkReductionReconciliationGenerateOutcomeModel>(
      reverseTransactions(),
      model
    );
  }
  getPensionExport(
    model: ITimeWorkReductionReconciliationEmployeeModel
  ): Observable<ITimeWorkReductionExportPensionDTO[]> {
    return this.http.post<ITimeWorkReductionExportPensionDTO[]>(
      getPensionExport(),
      model
    );
  }
}
