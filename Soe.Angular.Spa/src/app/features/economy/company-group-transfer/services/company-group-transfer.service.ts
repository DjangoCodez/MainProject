import { inject, Injectable } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { ICompanyGroupTransferModel } from '@shared/models/generated-interfaces/EconomyModels';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  companyGroupTransfer,
  deleteCompanyGroupTransfer,
  getCompanyGroupVoucherHistory,
} from '@shared/services/generated-service-endpoints/economy/CompanyGroupTransfer.endpoints';
import { Observable } from 'rxjs';
import { CompanyGroupTransferHeadDTO } from '../models/company-group-transfer.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CompanyGroupTransferService {
  validationHandler = inject(ValidationHandler);

  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    accountYearId: 0,
    transferType: 0,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      accountYearId: number;
      transferType: number;
    }
  ): Observable<CompanyGroupTransferHeadDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<CompanyGroupTransferHeadDTO[]>(
      getCompanyGroupVoucherHistory(
        this.getGridAdditionalProps.accountYearId,
        this.getGridAdditionalProps.transferType
      )
    );
  }

  save(model: ICompanyGroupTransferModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(companyGroupTransfer(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(deleteCompanyGroupTransfer(id), id);
  }
}
