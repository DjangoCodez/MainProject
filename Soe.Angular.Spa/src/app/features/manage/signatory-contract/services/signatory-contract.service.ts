import { Injectable } from '@angular/core';
import { ISignatoryContractGridDTO } from '@shared/models/generated-interfaces/SignatoryContractGridDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getSignatoryContractsGrid,
  getSignatoryContract,
  saveSignatoryContract,
  revokeSignatoryContract,
} from '@shared/services/generated-service-endpoints/manage/SignatoryContract.endpoints';
import { Observable, of } from 'rxjs';
import { SignatoryContractDTO } from '../models/signatory-contract-edit-dto.model';
import { SignatoryContractRevokeDTO } from '../models/signatory-contract-revoke-dto';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable()
export class SignatoryContractService {
  constructor(private readonly http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISignatoryContractGridDTO[]> {
    return this.http.get<ISignatoryContractGridDTO[]>(
      getSignatoryContractsGrid(id)
    );
  }

  get(id: number): Observable<SignatoryContractDTO> {
    return this.http.get<SignatoryContractDTO>(getSignatoryContract(id));
  }

  save(item: SignatoryContractDTO): Observable<BackendResponse> {
    return this.http.post(saveSignatoryContract(), item);
  }

  delete(id: number): Observable<BackendResponse> {
    return of();
  }

  revoke(item: SignatoryContractRevokeDTO): Observable<BackendResponse> {
    return this.http.post(
      revokeSignatoryContract(item.signatoryContractId),
      item
    );
  }
}
