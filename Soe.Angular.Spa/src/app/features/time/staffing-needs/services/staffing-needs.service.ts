import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { getStaffingNeedsLocationGroupsDict } from '@shared/services/generated-service-endpoints/time/StaffingNeeds.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';

@Injectable({
  providedIn: 'root',
})
export class StaffingNeedsService {
  constructor(private http: SoeHttpClient) {}

  getStaffingNeedsLocationGroupsDict(
    addEmptyRow: boolean,
    includeAccountName: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get(
      getStaffingNeedsLocationGroupsDict(addEmptyRow, includeAccountName)
    );
  }
}
