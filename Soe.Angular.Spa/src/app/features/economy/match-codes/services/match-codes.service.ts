import { inject, Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { MatchCodeGridDTO, MatchCodeDTO } from '../models/match-codes.model';
import {
  getMatchCodesGrid,
  getMatchCode,
  saveMatchCode,
  deleteMatchCode,
} from '@shared/services/generated-service-endpoints/economy/MatchCode.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class MatchCodeService {
  private readonly http = inject(SoeHttpClient);

  getGrid(id?: number): Observable<MatchCodeGridDTO[]> {
    return this.http
      .get<MatchCodeGridDTO[]>(getMatchCodesGrid(id))
      .pipe(tap(x => x.forEach(x => (x.typeName = x.type))));
  }

  get(id: number): Observable<MatchCodeDTO> {
    return this.http.get<MatchCodeDTO>(getMatchCode(id));
  }

  save(model: MatchCodeDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveMatchCode(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteMatchCode(id));
  }
}
