import { Injectable } from '@angular/core';
import { IFilterExpensesModel } from '@shared/models/generated-interfaces/BillingModels';
import { IExpenseRowGridDTO } from '@shared/models/generated-interfaces/ExpenseDTO';
import { IEmployeeTimeCodeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getExpenseRows,
  getExpenseRowsForGridFiltered,
} from '@shared/services/generated-service-endpoints/billing/ProjectExpense.endpoints';
import { getEmployeesForTimeProjectRegistrationSmall } from '@shared/services/generated-service-endpoints/billing/ProjectTime.endpoints';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProjectExpenseService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IExpenseRowGridDTO[]> {
    return of([]);
  }

  //#region Expenses
  getExpenseRows(
    invoiceId: number,
    customerInvoiceRowId: number = 0
  ): Observable<IExpenseRowGridDTO[]> {
    return this.http.get<IExpenseRowGridDTO[]>(
      getExpenseRows(invoiceId, customerInvoiceRowId)
    );
  }

  getExpenseRowsFiltered(
    model: IFilterExpensesModel
  ): Observable<IExpenseRowGridDTO[]> {
    return this.http.post<IExpenseRowGridDTO[]>(
      getExpenseRowsForGridFiltered(),
      model
    );
  }

  //#endregion

  getEmployeesForTimeProjectRegistrationSmall(
    projectId: number,
    fromDate: string = '',
    toDate: string = ''
  ): Observable<IEmployeeTimeCodeDTO[]> {
    return this.http.get<IEmployeeTimeCodeDTO[]>(
      getEmployeesForTimeProjectRegistrationSmall(projectId, fromDate, toDate)
    );
  }
}
