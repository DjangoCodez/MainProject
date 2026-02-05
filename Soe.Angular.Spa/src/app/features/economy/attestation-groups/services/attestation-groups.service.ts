import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { map, Observable } from 'rxjs';
import {
  AttestGroupGridDTO,
  AttestWorkFlowHeadDTO,
} from '../models/attestation-groups.model';
import {
  deleteAttestWorkFlow,
  deleteAttestWorkFlows,
  getAttestWorkFlowGroup,
  getAttestWorkFlowGroups,
  saveAttestWorkFlow,
  saveAttestWorkFlowForInvoices,
  saveAttestWorkFlowMultiple,
} from '@shared/services/generated-service-endpoints/economy/SupplierAttestGroup.endpoints';
import {
  ISaveAttestWorkFlowForInvoicesModel,
  ISaveAttestWorkFlowForMultipleInvoicesModel,
} from '@shared/models/generated-interfaces/CoreModels';
import { IAttestWorkFlowHeadDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AttestationGroupsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps: {
    addEmptyRow: boolean;
    attestWorkFlowHeadId?: number;
  } = {
    addEmptyRow: false,
    attestWorkFlowHeadId: undefined,
  };
  getGrid(
    id?: number,
    additionalProps?: { addEmptyRow: boolean; attestWorkFlowHeadId?: number }
  ): Observable<AttestGroupGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http
      .get<
        AttestGroupGridDTO[]
      >(getAttestWorkFlowGroups(this.getGridAdditionalProps.addEmptyRow, id))
      .pipe(
        map((data: AttestGroupGridDTO[]) => {
          data.forEach(item => {
            item.attestGroupName = item.name;
          });
          return data;
        })
      );
  }

  get(attestWorkFlowHeadId: number): Observable<AttestWorkFlowHeadDTO> {
    return this.http.get<AttestWorkFlowHeadDTO>(
      getAttestWorkFlowGroup(attestWorkFlowHeadId)
    );
  }

  save(data: AttestWorkFlowHeadDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveAttestWorkFlow(), data);
  }

  delete(attestWorkFlowHeadId: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(
      deleteAttestWorkFlow(attestWorkFlowHeadId)
    );
  }

  deleteAttestWorkFlows(attestWorkFlowHeadIds: number[]) {
    const idsString = attestWorkFlowHeadIds.join(',');
    return this.http.delete<BackendResponse>(deleteAttestWorkFlows(idsString));
  }

  saveAttestWorkFlowForInvoices(model: ISaveAttestWorkFlowForInvoicesModel) {
    return this.http.post<BackendResponse>(
      saveAttestWorkFlowForInvoices(),
      model
    );
  }

  saveAttestWorkFlowMultiple(
    attestWorkFlowHead: IAttestWorkFlowHeadDTO,
    invoiceIds: number[]
  ) {
    const model: ISaveAttestWorkFlowForMultipleInvoicesModel = {
      attestWorkFlowHead,
      invoiceIds,
    };
    return this.http.post<BackendResponse>(saveAttestWorkFlowMultiple(), model);
  }
}
