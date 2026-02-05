import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  TermGroup,
  TermGroup_AnnualLeaveTransactionType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAnnualLeaveTransactionGridDTO,
  IAnnualLeaveTransactionEditDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getTermGroupContent } from '@shared/services/generated-service-endpoints/core/Term.endpoints';
import { map, Observable } from 'rxjs';
import { SearchAnnualLeaveTransactionModel } from '../models/annual-leave-balance.model';
import {
  deleteAnnualLeaveTransaction,
  getAnnualLeaveTransaction,
  getAnnualLeaveTransactionGridData,
  saveAnnualLeaveTransaction,
} from '@shared/services/generated-service-endpoints/time/AnnualLeave.endpoints';

@Injectable({
  providedIn: 'root',
})
export class AnnualLeaveBalanceService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    model: new SearchAnnualLeaveTransactionModel(),
  };

  getGrid(
    id?: number,
    additionalProps?: {
      model: SearchAnnualLeaveTransactionModel;
    }
  ): Observable<IAnnualLeaveTransactionGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.post<IAnnualLeaveTransactionGridDTO[]>(
      getAnnualLeaveTransactionGridData(),
      this.getGridAdditionalProps.model
    );
  }

  get(id: number): Observable<IAnnualLeaveTransactionEditDTO> {
    return this.http.get<IAnnualLeaveTransactionEditDTO>(
      getAnnualLeaveTransaction(id)
    );
  }

  save(model: IAnnualLeaveTransactionEditDTO): Observable<any> {
    return this.http.post<IAnnualLeaveTransactionEditDTO>(
      saveAnnualLeaveTransaction(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteAnnualLeaveTransaction(id));
  }

  getTransactionTypes(onlyManually: boolean): Observable<SmallGenericType[]> {
    return this.http
      .get<
        SmallGenericType[]
      >(getTermGroupContent(TermGroup.AnnualLeaveTransactionType, true, false, false))
      .pipe(
        map(data => {
          if (onlyManually) {
            data = data.filter(
              d =>
                d.id ==
                  <number>TermGroup_AnnualLeaveTransactionType.ManuallyEarned ||
                d.id ==
                  <number>TermGroup_AnnualLeaveTransactionType.ManuallySpent
            );
          }
          return data.sort((a, b) => {
            if (a.id < b.id) {
              return -1;
            }
            if (a.id > b.id) {
              return 1;
            }
            return 0;
          });
        })
      );
  }
}
