import { Injectable, signal } from '@angular/core';
import {
  IBudgetHeadFlattenedDTO,
  IBudgetHeadGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteBudget,
  getBalanceChangePerPeriod,
  getBalanceChangeResult,
  getBudget,
  getBudgetList,
  saveBudgetHead,
} from '@shared/services/generated-service-endpoints/economy/Budget.endpoints';
import { map, Observable } from 'rxjs';
import {
  BudgetHeadFlattenedDTO,
  BudgetRowFlattenedDTO,
} from '../models/budget.model';
import { IGetResultPerPeriodModel } from '@shared/models/generated-interfaces/EconomyModels';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class BudgetService {
  constructor(private http: SoeHttpClient) {}

  useDim2 = signal(false);

  getGridAdditionalProps = {
    budgetType: 0,
    actorId: 0,
  };
  getGrid(
    id?: number,
    additionalProps?: { budgetType: number; actorId: number }
  ): Observable<IBudgetHeadGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IBudgetHeadGridDTO[]>(
      getBudgetList(
        this.getGridAdditionalProps.budgetType,
        this.getGridAdditionalProps.actorId,
        id
      )
    );
  }

  get(
    budgetHeadId: number,
    loadRows: boolean = true,
    isUseCache: boolean = false
  ): Observable<BudgetHeadFlattenedDTO> {
    return this.http
      .get<BudgetHeadFlattenedDTO>(getBudget(budgetHeadId, loadRows), {
        useCache: isUseCache,
      })
      .pipe(
        map(data => {
          const obj = new BudgetHeadFlattenedDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  save(model: IBudgetHeadFlattenedDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveBudgetHead(), model);
  }

  delete(budgetHeadId: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteBudget(budgetHeadId));
  }

  getBalanceChangePerPeriod(
    values: IGetResultPerPeriodModel
  ): Observable<BudgetRowFlattenedDTO[]> {
    return this.http.post(getBalanceChangePerPeriod(), values);
  }

  getBalanceChangeResult(key: string): Observable<BudgetRowFlattenedDTO[]> {
    return this.http
      .get<BudgetRowFlattenedDTO[]>(getBalanceChangeResult(key))
      .pipe(
        map(rows => {
          return rows.map(row => {
            const obj = new BudgetRowFlattenedDTO();
            Object.assign(obj, row);
            obj.fixDates();
            return obj;
          });
        })
      );
  }
}
