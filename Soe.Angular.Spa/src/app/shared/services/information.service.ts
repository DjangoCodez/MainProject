import { Injectable } from '@angular/core';
import { CACHE_EXPIRE_LONG, SoeHttpClient } from './http.service';
import {
  getNbrOfUnreadInformations,
  hasNewInformations,
  hasSevereUnreadInformation,
} from './generated-service-endpoints/core/Information.endpoints';
import { Observable } from 'rxjs';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

@Injectable({
  providedIn: 'root',
})
export class InformationService {
  constructor(private http: SoeHttpClient) {}

  getNbrOfUnreadInformations(useCache: boolean): Observable<number> {
    return this.http.get<number>(
      getNbrOfUnreadInformations(SoeConfigUtil.language),
      {
        useCache: useCache,
        cacheOptions: { expires: CACHE_EXPIRE_LONG },
      }
    );
  }

  hasNewInformations(time: string): Observable<boolean> {
    return this.http.get<boolean>(hasNewInformations(time));
  }

  hasSevereUnreadInformation(useCache: boolean): Observable<boolean> {
    return this.http.get<boolean>(
      hasSevereUnreadInformation(SoeConfigUtil.language),
      {
        useCache: useCache,
        cacheOptions: { expires: CACHE_EXPIRE_LONG },
      }
    );
  }
}
