import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  ITimeBreakTemplateGridDTONew,
  ITimeBreakTemplateDTONew,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import {
  deleteTimeBreakTemplate,
  getTimeBreakTemplate,
  getTimeBreakTemplatesGrid,
  saveTimeBreakTemplate,
  validateTimeBreakTemplate,
} from '@shared/services/generated-service-endpoints/time/TimeBreakTemplate.endpoints';

@Injectable({
  providedIn: 'root',
})
export class TimeBreakTemplatesService {
  private refreshGridSubject = new Subject<void>();
  refreshGrid$ = this.refreshGridSubject.asObservable();

  private gridDataSubject = new Subject<ITimeBreakTemplateGridDTONew[]>();
  gridData$ = this.gridDataSubject.asObservable();

  constructor(private http: SoeHttpClient) {}

  triggerRefresh() {
    this.refreshGridSubject.next();
  }

  emitGridData(data: ITimeBreakTemplateGridDTONew[]) {
    this.gridDataSubject.next(data);
  }

  getGrid(id?: number): Observable<ITimeBreakTemplateGridDTONew[]> {
    return this.http.get<ITimeBreakTemplateGridDTONew[]>(
      getTimeBreakTemplatesGrid(id)
    );
  }

  get(id: number): Observable<ITimeBreakTemplateDTONew> {
    return this.http.get<ITimeBreakTemplateDTONew>(getTimeBreakTemplate(id));
  }

  validate(model: ITimeBreakTemplateDTONew): Observable<IActionResult> {
    return this.http.post<IActionResult>(validateTimeBreakTemplate(), model);
  }

  save(model: ITimeBreakTemplateDTONew): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveTimeBreakTemplate(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteTimeBreakTemplate(id));
  }
}
