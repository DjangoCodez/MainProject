import { Injectable } from '@angular/core';
import {
  ISysPositionDTO,
  ISysPositionGridDTO,
} from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  deleteSysPosition,
  getSysPosition,
  getSysPositions,
  getSysPositionsDict,
  getSysPositionsGrid,
  saveSysPosition,
} from '@shared/services/generated-service-endpoints/core/Positions.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PositionsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISysPositionGridDTO[]> {
    return this.http.get<ISysPositionGridDTO[]>(getSysPositionsGrid(id));
  }

  get(id: number): Observable<ISysPositionDTO> {
    return this.http.get<ISysPositionDTO>(getSysPosition(id));
  }

  getPositions(
    sysCountryId: number,
    sysLanguageId: number
  ): Observable<ISysPositionGridDTO[]> {
    return this.http.get<ISysPositionGridDTO[]>(
      getSysPositions(sysCountryId, sysLanguageId)
    );
  }

  getSysPositionsDict(
    sysCountryId: number,
    sysLanguageId: number,
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getSysPositionsDict(sysCountryId, sysLanguageId, addEmptyRow)
    );
  }

  save(model: ISysPositionDTO): Observable<any> {
    return this.http.post<ISysPositionDTO>(saveSysPosition(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteSysPosition(id));
  }
}
