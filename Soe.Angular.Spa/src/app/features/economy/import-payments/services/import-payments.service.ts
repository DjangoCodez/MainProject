import { Injectable } from '@angular/core';
import {
  IMatchCodeDTO,
  IPaymentImportDTO,
  IPaymentMethodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getPaymentImports,
  getPaymentMethodsDict,
  savePaymentImportHeader,
  getPaymentMethodsForImport,
  getPaymentImport,
  getImportedIoInvoices,
  updateCustomerPaymentImportIODTOS,
  updatePaymentImportIODTOS,
  updatePaymentImportIODTOSStatus,
  startPaymentImport,
  getSysPaymentTypes,
  deletePaymentImportHeader,
  deletePaymentImportIO,
  updatePaymentImportIO,
  savePaymentImportIOs,
} from '@shared/services/generated-service-endpoints/economy/ImportPayment.endpoints';
import { BehaviorSubject, map, mergeMap, Observable, of } from 'rxjs';
import {
  PaymentImportDTO,
  PaymentImportIODTO,
  PaymentImportRowsDto,
  SaveCustomerPaymentImportIODTOModel,
  SavePaymentImportIODTOModel,
} from '../models/import-payments.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { getMatchCodes } from '@shared/services/generated-service-endpoints/economy/MatchCode.endpoints';
import {
  ISaveCustomerPaymentImportIODTOModel,
  ISavePaymentImportIODTOModel,
  IPaymentMethodsGetModel
} from '@shared/models/generated-interfaces/EconomyModels';
import { getInvoiceForPayment } from '@shared/services/generated-service-endpoints/economy/CustomerPaymentMethod.endpoints';
import { ICustomerInvoiceDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import {
  ImportPaymentType,
  SoeOriginType,
  TermGroup_PaymentTransferStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { TranslateService } from '@ngx-translate/core';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ImportPaymentsService {
  constructor(
    private http: SoeHttpClient,
    private translate: TranslateService
  ) {}

  durationSelection = 0;
  paymentMethodsDict: ISmallGenericType[] = [];

  private paymentMethodsDictSubject = new BehaviorSubject<ISmallGenericType[]>(
    this.paymentMethodsDict
  );
  private durationSelectionSubject = new BehaviorSubject<number>(
    this.durationSelection
  );

  readonly durationSelection$ = this.durationSelectionSubject.asObservable();
  readonly paymentMethodsDict$ = this.paymentMethodsDictSubject.asObservable();

  setDurationSelectionSubject(durationSelection: number) {
    this.durationSelection = durationSelection;
    this.durationSelectionSubject.next(durationSelection);
  }

  setPaymentMethodsDictSubjectSubject(paymentMethodsDict: ISmallGenericType[]) {
    this.paymentMethodsDictSubject.next(paymentMethodsDict);
  }


  getGrid(
    id?: number,
    additionalProps?: { allItemsSelection: number }
  ): Observable<PaymentImportDTO[]> {
    return this.http
      .get<
        PaymentImportDTO[]
      >(getPaymentImports(additionalProps?.allItemsSelection ?? 0, id))
      .pipe(
        map((data) => {
          data.forEach(row => {
            this.setTransferIcon(row);
          });
          return data;
        }),
        mergeMap(data => {
          const originTypes =
            [SoeOriginType.CustomerPayment, SoeOriginType.SupplierPayment];
          const model: IPaymentMethodsGetModel = {
            originTypeIds: originTypes,
            addEmptyRow: false
          };
          return this.getPaymentMethodsDict(model).pipe(
            map(paymentMethodsDict => {
              data.forEach(row => {
                this.setTypeName(row, paymentMethodsDict);
              });
              data.sort((a, b) => b.batchId - a.batchId);
              return data;
            })
          );
        })
      );
  }

  get(paymentImportId: number): Observable<IPaymentImportDTO> {
    return this.http.get<IPaymentImportDTO>(
      getPaymentImport(paymentImportId)
    );
  }

  save(model: PaymentImportDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(savePaymentImportHeader(), model);
  }

  savePaymentImportRow(model: PaymentImportRowsDto): Observable<any> {
    return this.http.post<PaymentImportRowsDto>(startPaymentImport(), model);
  }

  delete(id: number): Observable<any> {
    return of();
  }

  deletePaymentImportIOInvoices(
    batchId: number,
    paymentType: number
  ): Observable<any> {
    return this.http.delete<any>(
      deletePaymentImportHeader(batchId, paymentType)
    );
  }

  getPaymentMethodsDict(
    model: IPaymentMethodsGetModel
  ): Observable<ISmallGenericType[]> {
    return this.http.post<ISmallGenericType[]>(
      getPaymentMethodsDict(),
      model
    );
  }

  getPaymentMethodsForImport(
    originTypeId: number
  ): Observable<IPaymentMethodDTO[]> {
    return this.http.get<IPaymentMethodDTO[]>(
      getPaymentMethodsForImport(originTypeId)
    );
  }

  getSysPaymentTypeDict(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getSysPaymentTypes());
  }

  getImportedIoInvoices(
    batchId: number,
    importType: number
  ): Observable<PaymentImportIODTO[]> {
    return this.http.get<PaymentImportIODTO[]>(
      getImportedIoInvoices(batchId, importType)
    );
  }

  getMatchCodes(
    matchCodeType: number,
    addEmptyRow: boolean
  ): Observable<IMatchCodeDTO[]> {
    return this.http.get<IMatchCodeDTO[]>(
      getMatchCodes(matchCodeType, addEmptyRow)
    );
  }

  updateCustomerPaymentImportIODTOS(
    model: SaveCustomerPaymentImportIODTOModel
  ): Observable<any> {
    return this.http.post<ISaveCustomerPaymentImportIODTOModel>(
      updateCustomerPaymentImportIODTOS(),
      model
    );
  }
  updatePaymentImportIO(model: PaymentImportIODTO): Observable<any> {
    return this.http.post<ISavePaymentImportIODTOModel>(
      updatePaymentImportIO(),
      model
    );
  }

  updatePaymentImportIODTOS(
    model: SavePaymentImportIODTOModel
  ): Observable<any> {
    return this.http.post<ISavePaymentImportIODTOModel>(
      updatePaymentImportIODTOS(),
      model
    );
  }

  updatePaymentImportIODTOSStatus(
    model: SavePaymentImportIODTOModel
  ): Observable<any> {
    return this.http.post<ISavePaymentImportIODTOModel>(
      updatePaymentImportIODTOSStatus(),
      model
    );
  }

  savePaymentImportIOs(model: PaymentImportIODTO[]): Observable<any> {
    return this.http.post<PaymentImportIODTO[]>(savePaymentImportIOs(), model);
  }

  deletePaymentImportIORow(paymentImportIOId: number): Observable<any> {
    return this.http.delete<any>(deletePaymentImportIO(paymentImportIOId));
  }

  getInvoiceForPayment(invoiceId: number): Observable<ICustomerInvoiceDTO> {
    return this.http.get<ICustomerInvoiceDTO>(getInvoiceForPayment(invoiceId));
  }

  private setTransferIcon(row: PaymentImportDTO) {
    row.showTransferStatusIcon = true;
    switch (row.transferStatus) {
      case TermGroup_PaymentTransferStatus.Transfered:
        row.transferStateIconText = this.translate.instant(
          'economy.import.payment.download'
        );
        row.transferStateIcon = 'cloud-download'; //'fal fa-cloud-download warningColor'
        row.transferStateIconClass = 'warningColor';
        break;
      case TermGroup_PaymentTransferStatus.Completed:
        row.transferStateIconText = this.translate.instant(
          'economy.import.payment.download'
        );
        row.transferStateIcon = 'cloud-download'; //'fal fa-cloud-download okColor'
        row.transferStateIconClass = 'okColor';
        break;
      case TermGroup_PaymentTransferStatus.AvaloError:
      case TermGroup_PaymentTransferStatus.SoftoneError:
      case TermGroup_PaymentTransferStatus.BankError:
        row.transferStateIcon = 'exclamation-triangle'; //'fal fa-exclamation-triangle errorColor'
        row.transferStateIconText = row.statusName;
        row.transferStateIconClass = 'errorColor';
        break;
      default:
        row.transferStateIcon = '';
        row.transferStateIconState = '';
        row.showTransferStatusIcon = false;
        break;
    }
  }

  private setTypeName(
    row: PaymentImportDTO,
    paymentMethodsDict: ISmallGenericType[]
  ) {
    if (paymentMethodsDict) {
      const paymentMethod = paymentMethodsDict.find(x => x.id === row.type);
      if (paymentMethod) {
        row.typeName = paymentMethod.name;
      }
    }
  }
}
