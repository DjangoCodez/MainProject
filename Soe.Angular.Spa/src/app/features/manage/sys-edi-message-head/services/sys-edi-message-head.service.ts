import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  ISysEdiMessageHeadDTO,
  ISysEdiMessageHeadGridDTO,
  SysEdiMessageHeadDTO,
  SysEdiMessageHeadStatus,
} from '../models/sys-edi-message-head.model';
import { Observable, of } from 'rxjs';
import {
  getSysEdiMessageHead,
  getSysEdiMessageHeadMsg,
  sysEdiMessageGridHead,
  sysEdiMessageHead,
  sysEdiMessagesGrid,
} from '@shared/services/generated-service-endpoints/manage/EdiMessage.endpoints';

@Injectable({
  providedIn: 'root',
})
export class SysEdiMessageHeadService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISysEdiMessageHeadGridDTO[]> {
    return this.http.get<ISysEdiMessageHeadGridDTO[]>(
      sysEdiMessageGridHead(SysEdiMessageHeadStatus.Handled, 30000, false, id)
    );
  }

  getGridFilter(
    open: boolean,
    closed: boolean,
    raw: boolean,
    missingCompanyId: boolean
  ): Observable<ISysEdiMessageHeadGridDTO[]> {
    return this.http.get<ISysEdiMessageHeadGridDTO[]>(
      sysEdiMessagesGrid(open, closed, raw, missingCompanyId)
    );
  }

  get(id: number): Observable<SysEdiMessageHeadDTO> {
    return this.http.get<SysEdiMessageHeadDTO>(getSysEdiMessageHead(id));
  }

  save(model: SysEdiMessageHeadDTO): Observable<any> {
    return this.http.post<SysEdiMessageHeadDTO>(sysEdiMessageHead(), model);
  }
  searchForMissingSysCompanyId(): Observable<ISysEdiMessageHeadGridDTO[]> {
    return this.http.get<ISysEdiMessageHeadGridDTO[]>(
      sysEdiMessagesGrid(false, true, false, true)
    );
  }

  getSysEdiMessageHeadMsg(sysEdiMessageHeadId: number): Observable<any> {
    return this.http.get<any>(getSysEdiMessageHeadMsg(sysEdiMessageHeadId));
  }
  delete(id: number): Observable<any> {
    return of();
  }
}
