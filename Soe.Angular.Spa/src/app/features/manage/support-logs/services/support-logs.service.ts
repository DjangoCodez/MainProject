import { Injectable } from '@angular/core';
import { SoeLogType } from '@shared/models/generated-interfaces/Enumerations';
import { ISysLogGridDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getSysLogsGrid,
  getSysLog,
  searchSysLogs,
} from '@shared/services/generated-service-endpoints/manage/SupportV2.endpoints';
import { Observable, of, tap } from 'rxjs';
import { SearchSysLogsDTO, SysLogDTO } from '../models/support-logs.model';

@Injectable({
  providedIn: 'root',
})
export class SupportLogsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    logType: SoeLogType.System_Error_Today,
    showUnique: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      logType: SoeLogType;
      showUnique: boolean;
    }
  ): Observable<ISysLogGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<ISysLogGridDTO[]>(
      getSysLogsGrid(
        this.getGridAdditionalProps.logType,
        this.getGridAdditionalProps.showUnique
      )
    );
  }

  get(id: number): Observable<SysLogDTO> {
    return this.http.get<SysLogDTO>(getSysLog(id)).pipe(
      tap(el => {
        if (el.date)
          el.dateStr = new Date(el.date).toFormattedDate('yyyy-MM-dd HH:mm');

        return of(el);
      })
    );
  }

  searchLogs(searchDto: SearchSysLogsDTO): Observable<ISysLogGridDTO[]> {
    return this.http.post(searchSysLogs(), searchDto);
  }

  save(): Observable<any> {
    return of(true);
  }
  delete(): Observable<any> {
    return of(true);
  }

  getLabelTerm(logType?: SoeLogType) {
    switch (<SoeLogType>logType) {
      case SoeLogType.System_Error_Today:
        return 'manage.support.logs.error';
      case SoeLogType.System_Warning_Today:
        return 'manage.support.logs.warning';
      case SoeLogType.System_Information_Today:
        return 'manage.support.logs.information';
      case SoeLogType.System_Search:
        return 'manage.support.logs.search';
      default:
        return 'manage.support.logs.all';
    }
  }
}
