import { inject, Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, of } from 'rxjs';
import { getAttestRoles } from '@shared/services/generated-service-endpoints/manage/AttestRole.endpoints';
import { SoeModule } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAttestRoleDTO,
  IRoleDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { getRoles } from '@shared/services/generated-service-endpoints/manage/Role.endpoints';
import {
  getConnectionRequest,
  getServiceUser,
  getServiceUserGrid,
  saveServiceUser,
} from '@shared/services/generated-service-endpoints/clientmanagement/ClientCompany.endpoints';
import {
  ICompanyConnectionRequestDTO,
  IServiceUserDTO,
} from '@shared/models/generated-interfaces/ServiceUserDTO';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ServiceUserService {
  private readonly http = inject(SoeHttpClient);

  get(id: number): Observable<IServiceUserDTO> {
    return this.http.get<IServiceUserDTO>(getServiceUser(id));
  }

  getGrid() {
    return this.http.get<IServiceUserDTO[]>(getServiceUserGrid(0));
  }

  save(request: IServiceUserDTO) {
    return this.http.post<BackendResponse>(saveServiceUser(), request);
  }

  delete(id: number) {
    return of({} as BackendResponse);
  }

  getConnectionRequest(code: string) {
    return this.http.get<ICompanyConnectionRequestDTO>(
      getConnectionRequest(code)
    );
  }

  getRoles(): Observable<IRoleDTO[]> {
    return this.http.get<IRoleDTO[]>(getRoles(false));
  }

  getAttestRoles() {
    return this.http.get<IAttestRoleDTO[]>(
      getAttestRoles(SoeModule.None, false)
    );
  }
}
