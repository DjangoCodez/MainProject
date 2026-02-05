import { Injectable } from '@angular/core';
import {
  IAccountDistributionEntryDTO,
  IAccountDistributionHeadDTO,
  IAccountDistributionTraceViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteAccountDistribution,
  getAccountDistributionHead,
  getAccountDistributionHeads,
  getAccountDistributionHeadsUsedIn,
  getAccountDistributionTraceViews,
  saveAccountDistribution,
} from '@shared/services/generated-service-endpoints/economy/AccountDistribution.endpoints';
import { BehaviorSubject, Observable, map } from 'rxjs';
import {
  AccountDistributionHeadDTO,
  AccountDistributionHeadSmallDTO,
} from '../models/account-distribution.model';
import { getAccountDistributionEntriesForSource } from '@shared/services/generated-service-endpoints/economy/AccountDistributionEntry.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AccountDistributionService {
  constructor(protected http: SoeHttpClient) {}
  private accountDistributionHeadName = '';

  private accountDistributionHeadNameSubject = new BehaviorSubject<string>(
    this.accountDistributionHeadName
  );
  readonly accountDistributionHeadName$ =
    this.accountDistributionHeadNameSubject.asObservable();

  changeAccountDistributionHeadName(name: string): void {
    this.accountDistributionHeadNameSubject.next(name);
  }

  getGridAdditionalProps = {
    loadOpen: false,
    loadClosed: false,
    loadEntries: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      loadOpen: boolean;
      loadClosed: boolean;
      loadEntries: boolean;
    }
  ): Observable<AccountDistributionHeadSmallDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<AccountDistributionHeadSmallDTO[]>(
      getAccountDistributionHeads(
        this.getGridAdditionalProps.loadOpen,
        this.getGridAdditionalProps.loadClosed,
        this.getGridAdditionalProps.loadEntries,
        id
      )
    );
  }

  get(id: number): Observable<AccountDistributionHeadDTO> {
    return this.http
      .get<AccountDistributionHeadDTO>(getAccountDistributionHead(id))
      .pipe(
        map(data => {
          const obj = new AccountDistributionHeadDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  getUsedIn(
    type?: number,
    triggerType?: number,
    date?: number,
    useInVoucher?: boolean,
    useInSupplierInvoice?: boolean,
    useInCustomerInvoice?: boolean,
    useInImport?: boolean,
    useInPayrollVoucher?: boolean,
    useInPayrollVacationVoucher?: boolean
  ): Observable<AccountDistributionHeadDTO[]> {
    return this.http.get<AccountDistributionHeadDTO[]>(
      getAccountDistributionHeadsUsedIn(
        type,
        triggerType,
        date,
        useInVoucher,
        useInSupplierInvoice,
        useInCustomerInvoice,
        useInImport,
        useInPayrollVoucher,
        useInPayrollVacationVoucher
      )
    );
  }

  getTraceViews(id: number): Observable<IAccountDistributionTraceViewDTO[]> {
    return this.http.get<IAccountDistributionTraceViewDTO[]>(
      getAccountDistributionTraceViews(id)
    );
  }

  save(model: any): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveAccountDistribution(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteAccountDistribution(id));
  }

  getAccountDistributionEntriesForSource(
    accountDistributionHeadId: number,
    registrationType: number,
    sourceId: number
  ): Observable<IAccountDistributionEntryDTO[]> {
    return this.http.get<IAccountDistributionEntryDTO[]>(
      getAccountDistributionEntriesForSource(
        accountDistributionHeadId,
        registrationType,
        sourceId
      )
    );
  }

  getAccountDistributionHeadsUsedIn(
    type?: number,
    triggerType?: number,
    date?: number,
    useInVoucher?: boolean,
    useInSupplierInvoice?: boolean,
    useInCustomerInvoice?: boolean,
    useInImport?: boolean,
    useInPayrollVoucher?: boolean,
    useInPayrollVacationVoucher?: boolean
  ): Observable<IAccountDistributionHeadDTO[]> {
    return this.http.get<IAccountDistributionHeadDTO[]>(
      getAccountDistributionHeadsUsedIn(
        type,
        triggerType,
        date,
        useInVoucher,
        useInSupplierInvoice,
        useInCustomerInvoice,
        useInImport,
        useInPayrollVoucher,
        useInPayrollVacationVoucher
      )
    );
  }
}
