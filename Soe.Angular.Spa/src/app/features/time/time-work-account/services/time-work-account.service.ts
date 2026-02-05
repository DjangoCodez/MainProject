import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import {
  TimeWorkAccountDTO,
  TimeWorkAccountYearDTO,
} from '../../models/timeworkaccount.model';
import {
  deleteTimeWorkAccount,
  deleteTimeWorkAccountYear,
  getTimeWorkAccount,
  getTimeWorkAccountLastYear,
  getTimeWorkAccountsGrid,
  getTimeWorkAccountYear,
  saveTimeWorkAccount,
  saveTimeWorkAccountYear,
  calculateYearEmployee,
  getCalculationBasis,
  sendSelection,
  getPensionExport,
  generateOutcome,
  getPayrollTimePeriods,
  getPayrollProductIdsByType,
  getPayrollProductsSmall,
  getPaymentDate,
  getTimeAccumulators,
  reverseTransaction,
  deleteTimeWorkAccountYearEmployeeRow,
  generateUnPaidBalance,
  reversePaidBalance,
} from '@shared/services/generated-service-endpoints/time/TimeWorkAccount.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  ITimeWorkAccountYearEmployeeModel,
  ITimeWorkAccountChoiceResultDTO,
  ITimeWorkAccountGridDTO,
  ITimeWorkAccountYearEmployeeBasisDTO,
  ITimeWorkAccountYearEmployeeResultDTO,
  ITimeWorkAccountExportPensionDTO,
  ITimeWorkAccountGenerateOutcomeModel,
  ITimeWorkAccountGenerateOutcomeResultDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { ISelectableTimePeriodDTO } from '@shared/models/generated-interfaces/ReportDataDTO';

@Injectable({
  providedIn: 'root',
})
export class TimeWorkAccountService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeWorkAccountGridDTO[]> {
    return this.http.get<ITimeWorkAccountGridDTO[]>(
      getTimeWorkAccountsGrid(id)
    );
  }

  get(id: number, includeYears = true): Observable<TimeWorkAccountDTO> {
    return this.http
      .get<TimeWorkAccountDTO>(getTimeWorkAccount(id, includeYears))
      .pipe(
        map((data: any) => {
          const obj = new TimeWorkAccountDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  getYear(
    id: number,
    timeWorkAccountId: number,
    loadEmployees = false,
    loadWorkTimeWeek = false
  ): Observable<TimeWorkAccountYearDTO> {
    return this.http
      .get<TimeWorkAccountYearDTO>(
        getTimeWorkAccountYear(
          id,
          timeWorkAccountId,
          loadEmployees,
          loadWorkTimeWeek
        )
      )
      .pipe(
        map((data: any) => {
          const obj = new TimeWorkAccountYearDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  getLastYear(
    timeWorkAccountId: number,
    addYear: boolean
  ): Observable<TimeWorkAccountYearDTO> {
    return this.http
      .get<TimeWorkAccountYearDTO>(
        getTimeWorkAccountLastYear(timeWorkAccountId, addYear)
      )
      .pipe(
        map((data: any) => {
          const obj = new TimeWorkAccountYearDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  getPensionExport(
    model: ITimeWorkAccountYearEmployeeModel
  ): Observable<ITimeWorkAccountExportPensionDTO[]> {
    return this.http.post<ITimeWorkAccountExportPensionDTO[]>(
      getPensionExport(),
      model
    );
  }

  save(model: TimeWorkAccountDTO): Observable<any> {
    return this.http.post<TimeWorkAccountDTO>(saveTimeWorkAccount(), model);
  }

  saveYear(model: TimeWorkAccountYearDTO): Observable<any> {
    return this.http.post<TimeWorkAccountYearDTO>(
      saveTimeWorkAccountYear(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteTimeWorkAccount(id));
  }

  deleteYear(id: number, timeWorkAccountId: number): Observable<any> {
    return this.http.delete(deleteTimeWorkAccountYear(id, timeWorkAccountId));
  }

  calculateEmployee(model: ITimeWorkAccountYearEmployeeModel): Observable<any> {
    return this.http.post<ITimeWorkAccountYearEmployeeResultDTO[]>(
      calculateYearEmployee(),
      model
    );
  }
  sendSelection(model: ITimeWorkAccountYearEmployeeModel): Observable<any> {
    return this.http.post<ITimeWorkAccountChoiceResultDTO[]>(
      sendSelection(),
      model
    );
  }
  loadCalculationBasis(
    timeWorkAccountYearEmployeeId: number,
    employeeId: number
  ): Observable<ITimeWorkAccountYearEmployeeBasisDTO[]> {
    return this.http.get<ITimeWorkAccountYearEmployeeBasisDTO[]>(
      getCalculationBasis(timeWorkAccountYearEmployeeId, employeeId)
    );
  }

  generateOutcome(
    model: ITimeWorkAccountGenerateOutcomeModel
  ): Observable<any> {
    return this.http.post<ITimeWorkAccountGenerateOutcomeResultDTO>(
      generateOutcome(),
      model
    );
  }
  generateUnusedPaidBalance(
    model: ITimeWorkAccountGenerateOutcomeModel
  ): Observable<any> {
    return this.http.post<ITimeWorkAccountGenerateOutcomeResultDTO>(
      generateUnPaidBalance(),
      model
    );
  }
  getAllPayrollTimePeriods(): Observable<ISelectableTimePeriodDTO[]> {
    return this.http.get<ISelectableTimePeriodDTO[]>(getPayrollTimePeriods());
  }

  GetPayrollProductIdsByType(
    level1: number,
    level2: number
  ): Observable<any[]> {
    return this.http.get<any[]>(getPayrollProductIdsByType(level1, level2));
  }

  getPayrollProductsSmall(): Observable<any[]> {
    return this.http.get<IProductSmallDTO[]>(getPayrollProductsSmall());
  }
  getPaymentDate(
    timeWorkAccountYearId: number,
    timeWorkAccountYearEmployeeId: number
  ): Observable<Date> {
    return this.http.get<Date>(
      getPaymentDate(timeWorkAccountYearId, timeWorkAccountYearEmployeeId)
    );
  }
  getTimeAccumulators(): Observable<any[]> {
    return this.http.get<IProductSmallDTO[]>(getTimeAccumulators());
  }
  reverseTransaction(
    model: ITimeWorkAccountYearEmployeeModel
  ): Observable<any> {
    return this.http.post<ITimeWorkAccountGenerateOutcomeResultDTO>(
      reverseTransaction(),
      model
    );
  }
  reversePaidBalance(
    model: ITimeWorkAccountYearEmployeeModel
  ): Observable<any> {
    return this.http.post<ITimeWorkAccountGenerateOutcomeResultDTO>(
      reversePaidBalance(),
      model
    );
  }
  deleteTimeWorkAccountYearEmployeeRow(
    timeWorkAccountYearId: number,
    timeWorkAccountYearEmployeeId: number,
    employeeId: number
  ): Observable<any> {
    return this.http.delete(
      deleteTimeWorkAccountYearEmployeeRow(
        timeWorkAccountYearId,
        timeWorkAccountYearEmployeeId,
        employeeId
      )
    );
  }
}
