import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  getMessageGroupsDict,
  getMessageGroup,
  saveMessageGroup,
  deleteMessageGroup,
} from '@shared/services/generated-service-endpoints/manage/MessageGroup.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class MessageGroupService {
  constructor(private http: SoeHttpClient) {}

  getDict(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getMessageGroupsDict());
  }

  get(id: number): Observable<any> {
    return this.http.get<any>(getMessageGroup(id));
  }

  save(model: any): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveMessageGroup(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteMessageGroup(id));
  }
}
