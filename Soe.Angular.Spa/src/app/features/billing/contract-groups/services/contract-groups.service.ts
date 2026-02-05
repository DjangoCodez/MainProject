import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import {
  ContractGroupDTO,
  ContractGroupExtendedGridDTO,
} from '../models/contract-groups.model';
import {
  deleteContractGroup,
  getContractGroup,
  getContractGroups,
  saveContractGroup,
} from '@shared/services/generated-service-endpoints/billing/ContractGroup.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ContractGroupsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ContractGroupExtendedGridDTO[]> {
    return this.http.get<ContractGroupExtendedGridDTO[]>(getContractGroups(id));
  }

  get(id: number): Observable<ContractGroupDTO> {
    return this.http.get<ContractGroupDTO>(getContractGroup(id));
  }

  save(model: ContractGroupDTO): Observable<BackendResponse> {
    return this.http.post(saveContractGroup(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteContractGroup(id));
  }
}
