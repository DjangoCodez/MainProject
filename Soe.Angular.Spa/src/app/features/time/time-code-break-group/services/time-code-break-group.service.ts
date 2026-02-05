import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ITimeCodeBreakGroupDTO,
  ITimeCodeBreakGroupGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deleteTimeCodeBreakGroup,
  getTimeCodeBreakGroupsGrid,
  getTimeCodeBreakGroup,
  saveTimeCodeBreakGroup,
} from '@shared/services/generated-service-endpoints/time/TimeCodeBreakGroup.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class TimeCodeBreakGroupService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeCodeBreakGroupGridDTO[]> {
    return this.http.get<ITimeCodeBreakGroupGridDTO[]>(
      getTimeCodeBreakGroupsGrid(id)
    );
  }

  get(id: number): Observable<ITimeCodeBreakGroupDTO> {
    return this.http.get<ITimeCodeBreakGroupDTO>(getTimeCodeBreakGroup(id));
  }

  save(model: ITimeCodeBreakGroupDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveTimeCodeBreakGroup(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteTimeCodeBreakGroup(id));
  }
}
