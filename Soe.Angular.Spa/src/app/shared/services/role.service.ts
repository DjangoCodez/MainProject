import { Injectable } from '@angular/core';
import { SoeHttpClient } from './http.service';
import { Observable } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  byCompanyAsDict,
  byUserAsDict,
} from './generated-service-endpoints/manage/RoleV2.endpoints';

@Injectable({
  providedIn: 'root',
})
export class RoleService {
  constructor(private http: SoeHttpClient) {}

  getRolesByCompanyAsDict(
    addEmptyRow: boolean,
    addEmptyRowAsAll: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      byCompanyAsDict(addEmptyRow, addEmptyRowAsAll)
    );
  }

  getRolesByUserAsDict(
    actorCompanyId: number
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(byUserAsDict(actorCompanyId));
  }
}
