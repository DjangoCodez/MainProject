import { Injectable } from '@angular/core';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getSysCompanies,
  getSysCompanyDict,
  getSysCompany,
  getSysCompanyByApiKey,
  saveSysCompany,
} from '@shared/services/generated-service-endpoints/manage/SysCompany.endpoints';
import { map, Observable, of } from 'rxjs';
import {
  ISysCompanyDTO,
  SysBankDTO,
  SysCompanyDTO,
} from '../../../models/sysCompany.model';
import { getBankintegrationBanks } from '@shared/services/generated-service-endpoints/manage/System.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SysCompanyService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISysCompanyDTO[]> {
    return this.http.get<ISysCompanyDTO[]>(getSysCompanies(id));
  }

  getSysCompanyDict(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getSysCompanyDict());
  }

  get(id: number): Observable<SysCompanyDTO> {
    return this.http.get<SysCompanyDTO>(getSysCompany(id));
  }

  getSysBanks() {
    return this.http.get<SysBankDTO[]>(getBankintegrationBanks()).pipe(
      map(banks => {
        banks.forEach(bank => {
          bank.nameWithBic = `${bank.name} (${bank.bic})`;
        });
        return banks;
      })
    );
  }

  getSysCompanyByApiKey(
    apiKey: string,
    sysCompDbId: number
  ): Observable<ISysCompanyDTO> {
    return this.http.get<ISysCompanyDTO>(
      getSysCompanyByApiKey(apiKey, sysCompDbId)
    );
  }

  save(sysCompany: SysCompanyDTO): Observable<BackendResponse> {
    return this.http.post(saveSysCompany(), sysCompany);
  }

  delete(id: number): Observable<any> {
    return of(false);
  }
}
