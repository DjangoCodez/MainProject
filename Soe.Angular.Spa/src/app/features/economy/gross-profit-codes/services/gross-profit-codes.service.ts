import { Injectable } from '@angular/core';
import { IGrossProfitCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteGrossProfitCode,
  getGrossProfitCode,
  getGrossProfitCodesByYear,
  getGrossProfitCodesGrid,
  saveGrossProfitCode,
} from '@shared/services/generated-service-endpoints/economy/GrossProfitCode.endpoints';
import { Observable } from 'rxjs';
import { GrossProfitCodeDTO } from '../models/gross-profit-codes.model';

@Injectable({
  providedIn: 'root',
})
export class GrossProfitCodesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IGrossProfitCodeGridDTO[]> {
    return this.http.get<IGrossProfitCodeGridDTO[]>(
      getGrossProfitCodesGrid(id)
    );
  }

  get(id: number): Observable<GrossProfitCodeDTO> {
    return this.http.get<GrossProfitCodeDTO>(getGrossProfitCode(id));
  }

  getGrossProfitCodesByYear(
    accountYearId: number
  ): Observable<GrossProfitCodeDTO[]> {
    return this.http.get<GrossProfitCodeDTO[]>(
      getGrossProfitCodesByYear(accountYearId)
    );
  }

  save(model: GrossProfitCodeDTO): Observable<any> {
    return this.http.post<GrossProfitCodeDTO>(saveGrossProfitCode(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteGrossProfitCode(id));
  }
}
