import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  signatoryContractAuthorize,
  signatoryContractAuthenticate,
} from '@shared/services/generated-service-endpoints/manage/SignatoryContract.endpoints';
import { IGetPermissionResultDTO } from '@shared/models/generated-interfaces/GetPermissionResultDTO';
import { IAuthenticationResponseDTO } from '@shared/models/generated-interfaces/AuthenticationResponseDTO';
import { IAuthorizeRequestDTO } from '@shared/models/generated-interfaces/AuthorizeRequestDTO';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable()
export class SignatoryContractAuthService {
  constructor(private readonly http: SoeHttpClient) {}

  authorize(
    authorizeRequestDTO: IAuthorizeRequestDTO
  ): Observable<IGetPermissionResultDTO> {
    return this.http.post<IGetPermissionResultDTO>(
      signatoryContractAuthorize(),
      authorizeRequestDTO
    );
  }

  authenticate(
    authenticationResponse: IAuthenticationResponseDTO
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      signatoryContractAuthenticate(),
      authenticationResponse
    );
  }
}
