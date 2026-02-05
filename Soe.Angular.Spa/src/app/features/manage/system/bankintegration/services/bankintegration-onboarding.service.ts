import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  getBankintegrationOnboarding,
  getBankintegrationOnboardingGrid,
  sendAuthorizationResponse,
} from '@shared/services/generated-service-endpoints/manage/System.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { ISoeBankerOnboardingDTO } from '@shared/models/generated-interfaces/BankIntegrationDTOs';
import { ISoeBankerAuthorizationRequestModel } from '@shared/models/generated-interfaces/ManageModels';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class BankintegrationOnboardingService {
  private readonly http = inject(SoeHttpClient);

  getGrid(
    id?: number,
    additionalProps?: { type: number }
  ): Observable<ISoeBankerOnboardingDTO[]> {
    return this.http.get<ISoeBankerOnboardingDTO[]>(
      getBankintegrationOnboardingGrid()
    );
  }

  get(id: number): Observable<ISoeBankerOnboardingDTO> {
    return this.http.get<ISoeBankerOnboardingDTO>(
      getBankintegrationOnboarding(id)
    );
  }

  sendAcknowledgement(ids: number[]) {
    return this.http.post<BackendResponse>(sendAuthorizationResponse(), {
      onboardingRequestIds: ids,
    } as ISoeBankerAuthorizationRequestModel);
  }
}
