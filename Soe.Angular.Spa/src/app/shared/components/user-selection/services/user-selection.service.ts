import { Injectable } from '@angular/core';
import {
  BackendResponse,
  ServiceResponse,
} from '@shared/interfaces/backend-response.interface';
import { UserSelectionType } from '@shared/models/generated-interfaces/Enumerations';
import { IUserSelectionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  deleteUserSelection,
  getUserSelection,
  getUserSelections,
  getUserSelectionsDict,
  saveUserSelection,
} from '@shared/services/generated-service-endpoints/manage/UserSelection.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UserSelectionService {
  constructor(private http: SoeHttpClient) {}

  getUserSelections(type: UserSelectionType): Observable<IUserSelectionDTO[]> {
    return this.http.get<IUserSelectionDTO[]>(getUserSelections(type));
  }

  getUserSelectionsDict(
    type: UserSelectionType
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getUserSelectionsDict(type));
  }

  getUserSelection(userSelectionId: number): Observable<IUserSelectionDTO> {
    return this.http.get<IUserSelectionDTO>(getUserSelection(userSelectionId));
  }

  saveUserSelection(model: IUserSelectionDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveUserSelection(), model);
  }

  deleteUserSelection(userSelectionId: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(
      deleteUserSelection(userSelectionId)
    );
  }
}
