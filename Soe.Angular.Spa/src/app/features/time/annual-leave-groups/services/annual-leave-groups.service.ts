import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  TermGroup,
  TermGroup_AnnualLeaveGroupType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAnnualLeaveGroupDTO,
  IAnnualLeaveGroupGridDTO,
  IAnnualLeaveGroupLimitDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getTermGroupContent } from '@shared/services/generated-service-endpoints/core/Term.endpoints';
import {
  deleteAnnualLeaveGroup,
  getAnnualLeaveGroup,
  getAnnualLeaveGroupLimits,
  getAnnualLeaveGroupsDict,
  getAnnualLeaveGroupsGrid,
  saveAnnualLeaveGroup,
} from '@shared/services/generated-service-endpoints/time/AnnualLeave.endpoints';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AnnualLeaveGroupsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IAnnualLeaveGroupGridDTO[]> {
    return this.http.get<IAnnualLeaveGroupGridDTO[]>(
      getAnnualLeaveGroupsGrid(id)
    );
  }

  get(id: number): Observable<IAnnualLeaveGroupDTO> {
    return this.http.get<IAnnualLeaveGroupDTO>(getAnnualLeaveGroup(id));
  }

  save(model: IAnnualLeaveGroupDTO): Observable<any> {
    return this.http.post<IAnnualLeaveGroupDTO>(saveAnnualLeaveGroup(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteAnnualLeaveGroup(id));
  }

  getTypes(): Observable<SmallGenericType[]> {
    return this.http
      .get<
        SmallGenericType[]
      >(getTermGroupContent(TermGroup.AnnualLeaveGroupType, false, true, false))
      .pipe(
        tap(data => {
          data.sort((a, b) => {
            if (a.id < b.id) {
              return -1;
            }
            if (a.id > b.id) {
              return 1;
            }
            return 0;
          });
        })
      );
  }

  getTypeLimits(
    type: TermGroup_AnnualLeaveGroupType
  ): Observable<IAnnualLeaveGroupLimitDTO[]> {
    return this.http.get<IAnnualLeaveGroupLimitDTO[]>(
      getAnnualLeaveGroupLimits(<number>type)
    );
  }

  getAnnualLeaveGroups(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getAnnualLeaveGroupsDict(addEmptyRow)
    );
  }
}
