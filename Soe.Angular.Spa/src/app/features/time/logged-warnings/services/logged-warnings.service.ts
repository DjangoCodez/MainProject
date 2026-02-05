import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import { IWorkRuleBypassLogGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { getWorkRuleBypassLogGrid } from '@shared/services/generated-service-endpoints/time/WorkRuleBypassLog.endpoints';

@Injectable({
  providedIn: 'root',
})
export class LoggedWarningsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(
    id?: number,
    additionalProps?: any
  ): Observable<IWorkRuleBypassLogGridDTO[]> {
    const dateSelection = additionalProps?.dateSelection || 1;
    return this.http.get<IWorkRuleBypassLogGridDTO[]>(
      getWorkRuleBypassLogGrid(dateSelection, id)
    );
  }
}
