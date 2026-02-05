import { inject, Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  IAttestWorkFlowHeadDTO,
  IAttestWorkFlowTemplateHeadDTO,
  IAttestWorkFlowTemplateRowDTO,
  IFileUploadDTO,
  IPaymentConditionDTO,
  ISupplierGridDTO,
  IUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  CacheSettingsFactory,
  SoeHttpClient,
} from '@shared/services/http.service';
import {
  getPaymentConditions,
  getSmallGenericTypePaymentConditions,
} from '@shared/services/generated-service-endpoints/economy/PaymentCondition.endpoints';
import {
  generateReportForFinvoice,
  getOrdersForSupplierInvoiceEdit,
  getSuppliersDict,
  getSuppliersBySearch,
  getSupplier,
  saveSupplier,
  deleteSupplier,
  updateSuppliersState,
  updateIsPrivatePerson,
  getSupplierForExport,
  getNextSupplierNr,
  transferEdiState,
  transferEdiToInvoices,
  transferEdiToOrder,
  updateEdiEntrys,
  getSuppliers,
} from '@shared/services/generated-service-endpoints/economy/SupplierV2.endpoints';
import { map, Observable, tap } from 'rxjs';
import { SupplierExtendedGridDTO } from '../models/supplier.model';
import { ISearchSuppliersDTO } from '@shared/models/generated-interfaces/SearchSuppliersDTO';
import {
  IUpdateEntityStatesModel,
  IUpdateIsPrivatePerson,
} from '@shared/models/generated-interfaces/CoreModels';
import { SupplierDTO } from '../suppliers/models/supplier.model';
import {
  IGetSupplierCentralCountersAndBalanceModel,
  IInvoicesForProjectCentralModel,
  ISaveSupplierModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { getSmallGenericSysWholesellers } from '@shared/services/generated-service-endpoints/billing/SysWholeseller.endpoints';
import { getAttestWorkFlowGroupsDict } from '@shared/services/generated-service-endpoints/economy/SupplierAttestGroup.endpoints';
import {
  TransferEdiStateModel,
  UpdateEdiEntryDTO,
} from '../imports-invoices-finvoice/models/imports-invoices-finvoice.model';
import {
  saveSupplierFromFinvoice,
  getSupplierCentralCountersAndBalance,
  getInvoicesForProjectCentral,
} from '@shared/services/generated-service-endpoints/economy/SupplierInvoice.endpoints';
import { IChangeStatusGridViewBalanceDTO } from '@shared/models/generated-interfaces/ChangeStatusGridViewDTO';
import {
  getAttestWorkFlowAttestRolesByAttestTransition,
  getAttestWorkFlowHead,
  getAttestWorkFlowHeadFromInvoiceId,
  getAttestWorkFlowHeadFromInvoiceIds,
  getAttestWorkFlowTemplateHeadRows,
  getAttestWorkFlowTemplateHeadsForCompany,
  getAttestWorkFlowUsersByAttestTransition,
} from '@shared/services/generated-service-endpoints/economy/SupplierTemplateHeads.endpoints';
import { TermGroup_AttestEntity } from '@shared/models/generated-interfaces/Enumerations';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import {
  getCustomerEmailAddresses,
  getCustomerReferences,
} from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';
import { getAttestStates } from '@shared/services/generated-service-endpoints/manage/AttestState.endpoints';
import { SupplierInvoiceGridDTO } from '@features/billing/project-central/models/project-central.model';
import { AttestWorkFlowHeadDTO } from '@features/economy/attestation-groups/models/attestation-groups.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SupplierService {
  private readonly http = inject(SoeHttpClient);

  getGridAdditionalProps = {
    onlyActive: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      onlyActive: boolean;
    }
  ): Observable<SupplierExtendedGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http
      .get<
        SupplierExtendedGridDTO[]
      >(getSuppliers(this.getGridAdditionalProps.onlyActive, id))
      .pipe(
        map(suppliers =>
          suppliers.map(supplier => ({
            ...supplier,
            categoriesArray: supplier.categories
              ? supplier.categories.split(',').map(c => c.trim())
              : [],
          }))
        )
      );
  }

  getSupplierDict(
    onlyActive = false,
    addEmptyRow = false,
    useCache = false
  ): Observable<SmallGenericType[]> {
    const options = useCache ? CacheSettingsFactory.medium() : {};
    return this.http.get<SmallGenericType[]>(
      getSuppliersDict(onlyActive, addEmptyRow),
      options
    );
  }

  getSuppliersBySearch(
    data: ISearchSuppliersDTO
  ): Observable<ISupplierGridDTO[]> {
    return this.http.post<ISupplierGridDTO[]>(getSuppliersBySearch(), data);
  }

  getSupplierCentralCountersAndBalance(
    counterTypes: number[],
    supplierId: number
  ): Observable<IChangeStatusGridViewBalanceDTO[]> {
    const model: IGetSupplierCentralCountersAndBalanceModel = {
      counterTypes: counterTypes,
      supplierId: supplierId,
    };
    return this.http.post<IChangeStatusGridViewBalanceDTO[]>(
      getSupplierCentralCountersAndBalance(),
      model
    );
  }

  get(supplierId: number): Observable<SupplierDTO> {
    return this.http.get<SupplierDTO>(
      getSupplier(supplierId, true, true, true, true)
    );
  }

  getSupplier(
    supplierId: number,
    loadActor: boolean,
    loadAccount: boolean,
    loadContactAddresses: boolean,
    loadCategories: boolean
  ): Observable<SupplierDTO> {
    return this.http.get<SupplierDTO>(
      getSupplier(
        supplierId,
        loadActor,
        loadAccount,
        loadContactAddresses,
        loadCategories
      )
    );
  }

  getSupplierForExport(supplierId: number): Observable<SupplierDTO> {
    return this.http.get<SupplierDTO>(getSupplierForExport(supplierId));
  }

  getSupplierInvoicesForProjectCentral(
    model: IInvoicesForProjectCentralModel
  ): Observable<SupplierInvoiceGridDTO[]> {
    return this.http.post<SupplierInvoiceGridDTO[]>(
      getInvoicesForProjectCentral(),
      model
    );
  }

  getNextSupplierNr(): Observable<string> {
    return this.http.get<string>(getNextSupplierNr());
  }

  save(
    head: SupplierDTO,
    data?: { files?: IFileUploadDTO[]; extraFields?: IExtraFieldRecordDTO[] }
  ): Observable<BackendResponse> {
    const model: ISaveSupplierModel = {
      supplier: head,
      files: data?.files || [],
      extraFields: data?.extraFields || [],
    };
    return this.http.post<BackendResponse>(saveSupplier(), model);
  }

  saveSupplierFromFinvoice(ediEntryId: number): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveSupplierFromFinvoice(ediEntryId),
      null
    );
  }

  delete(supplierId: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteSupplier(supplierId));
  }

  updateSuppliersState(
    dict: Record<number, boolean>
  ): Observable<BackendResponse> {
    const model: IUpdateEntityStatesModel = { dict };
    return this.http.post<BackendResponse>(updateSuppliersState(), model);
  }

  updateIsPrivatePerson(
    dict: Record<number, boolean>
  ): Observable<BackendResponse> {
    const model: IUpdateIsPrivatePerson[] = Object.keys(dict).map(key => ({
      id: +key,
      isPrivatePerson: dict[+key],
    }));
    return this.http.post<BackendResponse>(updateIsPrivatePerson(), model);
  }

  getSupplierReferences(
    customerId: number,
    addEmptyRow = false
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getCustomerReferences(customerId, addEmptyRow)
    );
  }

  getSupplierEmails(
    supplierId: number,
    loadContactPersonsEmails: boolean,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getCustomerEmailAddresses(
        supplierId,
        loadContactPersonsEmails,
        addEmptyRow
      )
    );
  }

  getPaymentConditions(
    useCache: boolean = false
  ): Observable<IPaymentConditionDTO[]> {
    return this.http.get<IPaymentConditionDTO[]>(getPaymentConditions(), {
      useCache,
    });
  }

  getSmallGenericTypePaymentConditions(
    addEmptyRow: boolean,
    useCache: boolean = false
  ): Observable<ISmallGenericType[]> {
    const options = useCache ? CacheSettingsFactory.long() : {};
    return this.http.get<ISmallGenericType[]>(
      getSmallGenericTypePaymentConditions(addEmptyRow),
      options
    );
  }

  getSmallGenericSysWholesellers(
    addEmptyRow: boolean,
    useCache: boolean = false
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getSmallGenericSysWholesellers(addEmptyRow),
      { useCache }
    );
  }

  getAttestStates(
    entity: number,
    module: number,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getAttestStates(entity, module, addEmptyRow)
    );
  }

  getAttestWorkFlowGroupsDict(
    addEmptyRow: boolean,
    useCache: boolean = false
  ): Observable<ISmallGenericType[]> {
    const options = useCache ? CacheSettingsFactory.medium() : {};
    return this.http.get<ISmallGenericType[]>(
      getAttestWorkFlowGroupsDict(addEmptyRow),
      options
    );
  }

  getAttestWorkFlowTemplateHeadsForCurrentCompany(
    entity: TermGroup_AttestEntity = TermGroup_AttestEntity.SupplierInvoice
  ): Observable<IAttestWorkFlowTemplateHeadDTO[]> {
    return this.http.get<IAttestWorkFlowTemplateHeadDTO[]>(
      getAttestWorkFlowTemplateHeadsForCompany(entity)
    );
  }

  getAttestWorkFlowTemplateHeadRows(
    templateHeadId: number
  ): Observable<IAttestWorkFlowTemplateRowDTO[]> {
    return this.http.get<IAttestWorkFlowTemplateRowDTO[]>(
      getAttestWorkFlowTemplateHeadRows(templateHeadId)
    );
  }

  getAttestWorkFlowHeadFromInvoiceId(
    invoiceId: number,
    setTypeName: boolean = false,
    loadTemplate: boolean = false,
    loadRows: boolean = false,
    loadRemoved: boolean = false
  ): Observable<AttestWorkFlowHeadDTO | null> {
    return this.http.get<AttestWorkFlowHeadDTO | null>(
      getAttestWorkFlowHeadFromInvoiceId(
        invoiceId,
        setTypeName,
        loadTemplate,
        loadRows,
        loadRemoved
      )
    );
  }

  getAttestWorkFlowHeadFromInvoiceIds(
    invoiceIds: number[]
  ): Observable<AttestWorkFlowHeadDTO[]> {
    return this.http.post<AttestWorkFlowHeadDTO[]>(
      getAttestWorkFlowHeadFromInvoiceIds(),
      invoiceIds
    );
  }

  getAttestWorkFlowHead(
    attestWorkFlowHeadId: number,
    setTypeName: boolean,
    loadRows: boolean
  ): Observable<IAttestWorkFlowHeadDTO> {
    return this.http.get<IAttestWorkFlowHeadDTO>(
      getAttestWorkFlowHead(attestWorkFlowHeadId, setTypeName, loadRows)
    );
  }

  getAttestWorkFlowUsersByAttestTransition(
    attestTransitionId: number
  ): Observable<IUserSmallDTO[]> {
    return this.http.get<IUserSmallDTO[]>(
      getAttestWorkFlowUsersByAttestTransition(attestTransitionId)
    );
  }

  getAttestWorkFlowAttestRolesByAttestTransition(
    attestTransitionId: number
  ): Observable<IUserSmallDTO[]> {
    return this.http
      .get<
        IUserSmallDTO[]
      >(getAttestWorkFlowAttestRolesByAttestTransition(attestTransitionId))
      .pipe(
        tap(roles => {
          return roles.map(r => {
            return <IUserSmallDTO>{
              userId: 0,
              name: r.name,
              attestRoleId: r.attestRoleId,
            };
          });
        })
      );
  }

  getOrdersForSupplierInvoiceEdit(): Observable<
    ICustomerInvoiceSmallGridDTO[]
  > {
    return this.http.get<ICustomerInvoiceSmallGridDTO[]>(
      getOrdersForSupplierInvoiceEdit()
    );
  }

  transferEdiState(ediEntries: TransferEdiStateModel): Observable<any> {
    return this.http.post<BackendResponse>(transferEdiState(), ediEntries);
  }

  updateEdiEntrys(model: UpdateEdiEntryDTO[]): Observable<any> {
    return this.http.post<BackendResponse>(updateEdiEntrys(), model);
  }

  generateReportForFinvoice(ediEntryIds: number[]): Observable<any> {
    return this.http.post<number[]>(
      generateReportForFinvoice(ediEntryIds),
      ediEntryIds
    );
  }

  transferEdiToInvoices(itemsToTransfer: number[]): Observable<any> {
    const model = {
      numbers: itemsToTransfer,
    };
    return this.http.post<BackendResponse>(transferEdiToInvoices(), model);
  }
  transferEdiToOrder(itemsToTransfer: number[]): Observable<any> {
    const model = {
      numbers: itemsToTransfer,
    };
    return this.http.post<number[]>(transferEdiToOrder(), model);
  }
}
