import { inject, Injectable } from '@angular/core';
import { ISupplierInvoiceInterpretationDTO } from '@shared/models/generated-interfaces/SupplierInvoiceInterpretationDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getEdiEntry,
  getInvoice,
  getProjectsList,
  getScanningInterpretation,
  getSupplierInvoiceCostAllocationRows,
  getSupplierInvoiceImage,
  getSupplierInvoiceImageFromEdi,
  getTimeCodes,
  supplierInvoiceNrAlreadyExist,
} from '@shared/services/generated-service-endpoints/economy/SupplierInvoice.endpoints';
import { forkJoin, map, Observable, of as observableOf, of } from 'rxjs';
import {
  finvoiceEdiEntryToInvoiceDTO,
  scanningToInterpretationClasses,
  scanningToInvoiceDTO,
  scanningToSupplierData,
  toSimpleInvoiceFileDTO,
} from '../models/model-converter';
import {
  IEdiEntryDTO,
  IEmployeeSmallDTO,
  IPaymentInformationViewDTO,
  ISupplierDTO,
  ITimeCodeDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { getPaymentInformationViews } from '@shared/services/generated-service-endpoints/economy/SupplierPaymentMethod.endpoints';
import {
  getOrdersForSupplierInvoiceEdit,
  getSupplier,
} from '@shared/services/generated-service-endpoints/economy/SupplierV2.endpoints';
import { IGenericImageDTO } from '@shared/models/generated-interfaces/GenericImageDTO';
import { TranslateService } from '@ngx-translate/core';
import {
  SupplierInvoiceCostAllocationDTO,
  SupplierInvoiceDTO,
} from '../models/supplier-invoice.model';
import { InvoiceFieldClasses } from '../models/utility-models';
import { SupplierEditInputParameters } from '@features/economy/suppliers/models/edit-parameters.model';
import { ISupplierInvoiceCostAllocationDTO } from '@shared/models/generated-interfaces/SupplierInvoiceCostAllocationDTO';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { getAllEmployeeSmallDTOs } from '@shared/services/generated-service-endpoints/time/EmployeeV2.endpoints';
import { SoeTimeCodeType } from '@shared/models/generated-interfaces/Enumerations';
import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { getInvoiceProductsSmall } from '@shared/services/generated-service-endpoints/billing/InvoiceProduct.endpoints';
import { getProductsSmall } from '@shared/services/generated-service-endpoints/billing/BillingProduct.endpoints';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { orderBy } from 'lodash';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceService {
  private readonly http = inject(SoeHttpClient);
  private readonly translate = inject(TranslateService);

  save(params: any) {
    return this.http.post<BackendResponse>(`/api/supplier-invoice/save`, {});
  }

  delete(id: number) {
    return this.http.delete<BackendResponse>(`/api/supplier-invoice/${id}`);
  }

  get(invoiceId: number): Observable<SupplierInvoiceDTO> {
    return this.http.get(
      getInvoice(
        invoiceId,
        false, // loadProjectRows
        false, // loadOrderRows
        false, // loadProject
        true // loadImage
      )
    );
  }

  getInterpretedInvoice(ediEntryId: number) {
    return this.http.get<ISupplierInvoiceInterpretationDTO>(
      getScanningInterpretation(ediEntryId)
    );
  }

  loadInterpretedInvoice(ediEntryId: number) {
    return this.getInterpretedInvoice(ediEntryId).pipe(
      map(response => {
        return [
          scanningToInvoiceDTO(response),
          scanningToInterpretationClasses(response),
          scanningToSupplierData(response),
        ] as const;
      })
    );
  }

  loadSupplierInvoice(invoiceId: number, loadProject: boolean) {
    return this.http.get<SupplierInvoiceDTO>(
      getInvoice(invoiceId, false, false, loadProject, false)
    );
  }

  loadFinvoiceInvoice(ediEntryId: number) {
    return this.getEdiEntry(ediEntryId, true).pipe(
      map(res => {
        return finvoiceEdiEntryToInvoiceDTO(res);
      })
    );
  }

  getEdiEntry(ediEntryId: number, loadSuppliers: boolean) {
    return this.http.get<IEdiEntryDTO>(getEdiEntry(ediEntryId, loadSuppliers));
  }

  loadSupplier(supplierId: number) {
    return this.http.get<ISupplierDTO>(
      getSupplier(supplierId, false, true, false, false)
    );
  }

  loadPaymentInformation(supplierId: number) {
    return this.http.get<IPaymentInformationViewDTO[]>(
      getPaymentInformationViews(supplierId)
    );
  }

  loadInvoiceImage(invoiceId: number) {
    return this.http
      .get<IGenericImageDTO>(getSupplierInvoiceImage(invoiceId))
      .pipe(map(file => this.convertGenericImageToFile(file)));
  }

  loadEdiImage(ediEntryId: number) {
    return this.http
      .get<IGenericImageDTO>(getSupplierInvoiceImageFromEdi(ediEntryId))
      .pipe(map(file => this.convertGenericImageToFile(file)));
  }

  convertGenericImageToFile(file: IGenericImageDTO) {
    if (!file) return null;

    const invoiceImage = toSimpleInvoiceFileDTO(file);
    if (!invoiceImage.fileName) {
      invoiceImage.fileName =
        this.translate.instant('common.supplierinvoice') +
        invoiceImage.extension;
    }
    return invoiceImage;
  }

  loadInvoiceByStrategy(
    invoiceId?: number,
    scanningEntryId?: number,
    ediEntryId?: number
  ): Observable<
    readonly [
      SupplierInvoiceDTO,
      InvoiceFieldClasses | null,
      SupplierEditInputParameters | null,
    ]
  > {
    if (invoiceId) {
      return forkJoin([
        this.loadSupplierInvoice(invoiceId, true),
        observableOf(null),
        observableOf(null),
      ]);
    }
    if (scanningEntryId && ediEntryId) {
      return this.loadInterpretedInvoice(ediEntryId);
    }
    if (ediEntryId) {
      return forkJoin([
        this.loadFinvoiceInvoice(ediEntryId),
        observableOf(null),
        observableOf(null),
      ]);
    }
    throw new Error('No invoiceId or scanningEntryId or ediEntryId provided');
  }

  invoiceNrIsUnique(actorId: number, invoiceNr: string, invoiceId?: number) {
    return this.http.post<BackendResponse>(supplierInvoiceNrAlreadyExist(), {
      actorId,
      invoiceNr,
      invoiceId,
    });
  }
  getSupplierInvoiceCostAllocationRows(
    invoiceId: number
  ): Observable<SupplierInvoiceCostAllocationDTO[]> {
    return this.http
      .get<
        ISupplierInvoiceCostAllocationDTO[]
      >(getSupplierInvoiceCostAllocationRows(invoiceId))
      .pipe(
        map(rows => {
          const typedRows = rows as SupplierInvoiceCostAllocationDTO[];
          typedRows.map(r => {
            r.projectNrName = `${r.projectNr} ${r.projectName}`;
            r.employeeNrName = `${r.employeeNr} ${r.employeeName}`;
          });
          return typedRows;
        })
      );
  }

  getOrdersForSupplierInvoiceEdit(
    useCache: boolean = false
  ): Observable<ICustomerInvoiceSmallGridDTO[]> {
    return this.http.get<ICustomerInvoiceSmallGridDTO[]>(
      getOrdersForSupplierInvoiceEdit(),
      { useCache }
    );
  }

  getTimeCodes(
    timeCodeType: SoeTimeCodeType,
    active: boolean,
    loadPayrollProducts: boolean
  ): Observable<ITimeCodeDTO[]> {
    return this.http.get<ITimeCodeDTO[]>(
      getTimeCodes(timeCodeType, active, loadPayrollProducts)
    );
  }
  getAllEmployeeSmallDTOs(
    addEmptyRow: boolean,
    concatNumberAndNamebool: boolean,
    getHidden: boolean,
    orderByName: boolean,
    useCache = false
  ): Observable<IEmployeeSmallDTO[]> {
    return this.http.get<IEmployeeSmallDTO[]>(
      getAllEmployeeSmallDTOs(
        addEmptyRow,
        concatNumberAndNamebool,
        getHidden,
        orderByName
      ),
      { useCache }
    );
  }

  getProjectList(
    type: number,
    active?: boolean,
    getHidden?: boolean,
    getFinished?: boolean,
    useCache = false
  ): Observable<IProjectTinyDTO[]> {
    return this.http.get<IProjectTinyDTO[]>(
      getProjectsList(type, active, getHidden, getFinished),
      { useCache }
    );
  }

  getInvoiceProductsSmall(useCache = false): Observable<IProductSmallDTO[]> {
    return this.http.get<IProductSmallDTO[]>(getProductsSmall(), {
      useCache,
    });
  }
}
