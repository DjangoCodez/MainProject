import { Injectable, signal } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteBudget,
  getBudget,
  saveBudgetHead,
  createForecast,
  getProjectBudgetChangeLogPerRow,
  updateBudgetForecastResult,
  migrateProjectBudgetHead,
} from '@shared/services/generated-service-endpoints/billing/ProjectBudget.endpoints';
import { map, Observable } from 'rxjs';
import {
  BudgetHeadProjectDTO,
  BudgetRowProjectChangeLogDTO,
} from '../models/project-budget.model';
import { DistributionCodeBudgetType } from '@shared/models/generated-interfaces/Enumerations';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ProjectBudgetService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    budgetType: 0,
    actorId: 0,
  };

  get(
    budgetHeadId: number,
    loadRows: boolean = true,
    isUseCache: boolean = false
  ): Observable<BudgetHeadProjectDTO> {
    return this.http
      .get<BudgetHeadProjectDTO>(getBudget(budgetHeadId, loadRows), {
        useCache: isUseCache,
      })
      .pipe(
        map(data => {
          const obj = new BudgetHeadProjectDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  getChangeLog(
    budgetHeadId: number
  ): Observable<BudgetRowProjectChangeLogDTO[]> {
    return this.http
      .get<
        BudgetRowProjectChangeLogDTO[]
      >(getProjectBudgetChangeLogPerRow(budgetHeadId))
      .pipe(
        map(data => {
          return data.map(item => {
            const obj = new BudgetRowProjectChangeLogDTO();
            Object.assign(obj, item);
            return obj;
          });
        })
      );
  }

  createForecast(fromBudgetHeadId: number): Observable<BudgetHeadProjectDTO> {
    return this.http
      .get<BudgetHeadProjectDTO>(createForecast(fromBudgetHeadId))
      .pipe(
        map(data => {
          const obj = new BudgetHeadProjectDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  updateForecastResult(budgetHeadId: number): Observable<BackendResponse> {
    return this.http.get<BackendResponse>(
      updateBudgetForecastResult(budgetHeadId)
    );
  }

  migrateBudgetHead(budgetHeadId: number): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      migrateProjectBudgetHead(budgetHeadId),
      budgetHeadId
    );
  }

  save(model: BudgetHeadProjectDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveBudgetHead(), model);
  }

  delete(budgetHeadId: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteBudget(budgetHeadId));
  }

  getBudgetTabLabel(budgetTypeId: DistributionCodeBudgetType): string {
    switch (budgetTypeId) {
      case DistributionCodeBudgetType.ProjectBudgetForecast:
        return 'billing.projects.budget.forecast';
      case DistributionCodeBudgetType.ProjectBudgetIB:
        return 'billing.projects.list.ib';
      case DistributionCodeBudgetType.ProjectBudgetExtended:
      default:
        return 'billing.projects.list.budget';
    }
  }
}
