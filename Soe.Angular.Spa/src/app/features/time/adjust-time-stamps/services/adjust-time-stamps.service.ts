import { Injectable } from '@angular/core';
import { TimeStampEntryDTO } from '@shared/components/time/time-stamp-details-dialog/models/time-stamp-details.form.model';
import { ITimeStampEntryDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISearchTimeStampModel } from '@shared/models/generated-interfaces/TimeModels';
import { IUserAgentClientInfoDTO } from '@shared/models/generated-interfaces/UserAgentClientInfoDTO';
import {
  getTimeStamp,
  getTimeStampEntryUserAgentClientInfo,
  saveTimeStamps,
  searchTimeStamps,
} from '@shared/services/generated-service-endpoints/time/TimeStampV2.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AdjustTimeStampsService {
  constructor(private http: SoeHttpClient) {}
  getGrid(id?: number): Observable<any[]> {
    return new Observable(observer => {
      observer.next([]);
      observer.complete();
    });
  }
  searchTimeStamps(model: ISearchTimeStampModel): Observable<any[]> {
    return this.http.post<any[]>(searchTimeStamps(), model);
  }
  saveAdjustedTimeStampEntries(entries: ITimeStampEntryDTO[]): Observable<any> {
    return this.http.post<ITimeStampEntryDTO>(saveTimeStamps(), entries);
  }

  getTimeStamp(id: number): Observable<TimeStampEntryDTO> {
    return this.http.get<TimeStampEntryDTO>(getTimeStamp(id));
  }

  getTimeStampEntryUserAgentClientInfo(
    id: number
  ): Observable<IUserAgentClientInfoDTO> {
    return this.http.get<IUserAgentClientInfoDTO>(
      getTimeStampEntryUserAgentClientInfo(id)
    );
  }
}
