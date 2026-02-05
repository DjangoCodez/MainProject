import { Injectable } from '@angular/core';
import { ISignatoryContractPermissionEditItem } from '@shared/models/generated-interfaces/SignatoryContractPermissionEditItem';
import { SoeHttpClient } from '@shared/services/http.service';
import { getPermissionTerms } from '@shared/services/generated-service-endpoints/manage/SignatoryContract.endpoints';
import { Observable } from 'rxjs';

@Injectable()
export class SignatoryContractPermissionsService {

  constructor(private readonly http: SoeHttpClient) { }

  getGrid(
    id?: number
  ): Observable<ISignatoryContractPermissionEditItem[]> {

    return this.http.get<ISignatoryContractPermissionEditItem[]>(
      getPermissionTerms(id!)
    );
  }

}
