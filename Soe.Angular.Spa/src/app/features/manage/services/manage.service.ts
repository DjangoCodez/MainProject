import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import { getUserAttestTransitions } from '@shared/services/generated-service-endpoints/manage/AttestTransition.endpoints';
import { IAttestTransitionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Injectable({
  providedIn: 'root',
})
export class ManageService {
  constructor(private http: SoeHttpClient) {}

  //#region AttestTransitionService
  getUserAttestTransitions(
    entity: number,
    dateFrom: number,
    dateTo: number
  ): Observable<IAttestTransitionDTO[]> {
    return this.http.get<IAttestTransitionDTO[]>(
      getUserAttestTransitions(entity, dateFrom, dateTo)
    );
  }
  //#endregion
}
