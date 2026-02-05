import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  IPositionDTO,
  IPositionGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deletePosition,
  getPosition,
  getPositions,
  getPositionsDict,
  getPositionsGrid,
  savePosition,
  updateAndLinkSysPositionGrid,
  updatePositionGrid,
  updateSysPositionGrid,
} from '@shared/services/generated-service-endpoints/time/EmployeePosition.endpoints';
import { Observable } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class EmployeePositionService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = { loadSkills: false };
  getGrid(
    id?: number,
    additionalProps?: { loadSkills: boolean }
  ): Observable<IPositionGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IPositionGridDTO[]>(
      getPositionsGrid(this.getGridAdditionalProps.loadSkills, id)
    );
  }

  get(id: number, loadSkills: boolean): Observable<IPositionDTO> {
    return this.http.get<IPositionDTO>(getPosition(id, loadSkills));
  }

  getPositions(loadSkills: boolean): Observable<IPositionDTO> {
    return this.http.get<IPositionDTO>(getPositions(loadSkills));
  }

  getPositionsDict(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getPositionsDict(addEmptyRow));
  }

  save(model: IPositionDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(savePosition(), model);
  }

  updatePositionGrid(model: IPositionDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(updatePositionGrid(), model);
  }

  updateAndLinkSysPositionGrid(
    model: IPositionDTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      updateAndLinkSysPositionGrid(),
      model
    );
  }

  updateSysPositionGrid(model: IPositionDTO[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(updateSysPositionGrid(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deletePosition(id));
  }
}
