import { Injectable } from '@angular/core';
import { ISignatoryContractDTO } from '@shared/models/generated-interfaces/SignatoryContractDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import { getSignatoryContractSubContract } from '@shared/services/generated-service-endpoints/manage/SignatoryContract.endpoints';
import { Observable } from 'rxjs';

@Injectable()
export class SubSignatoryContractService {

  constructor(private readonly http: SoeHttpClient) { }

  getGrid(
    id?: number,
    additionalProps?: any
  ): Observable<ISignatoryContractDTO[]> {
    const signatoryContractParentId = additionalProps?.signatoryContractParentId;
    return this.http.get<ISignatoryContractDTO[]>(
      getSignatoryContractSubContract(signatoryContractParentId)
    );
  }
}
