import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  IHouseholdTaxDeductionGridViewDTO,
  IHouseholdTaxDeductionApplicantDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  getHouseholdTaxDeductionRows,
  getHouseholdTaxDeductionRowInfo,
  getHouseholdTaxDeductionRowForEdit,
  saveHouseholdTaxReceived,
  saveHouseholdTaxPartiallyApproved,
  saveHouseholdTaxApplied,
  saveHouseholdTaxDenied,
  deleteHouseholdTaxDeductionRow,
  getHouseholdTaxDeductionRowsApply,
  getHouseholdTaxDeductionRowsApplied,
  getHouseholdTaxDeductionRowsReceived,
  getHouseholdTaxDeductionRowsDenied,
  getLastUsedSequenceNumber,
  getHouseholdTaxDeductionPrintUrl,
  saveHouseholdTaxDeductionRowForEdit,
  saveHouseholdTaxWithdrawApplied,
} from '@shared/services/generated-service-endpoints/billing/HouseholdTaxDeduction.endpoints';
import { Observable } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class HouseholdTaxDeductionService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    classification: 0,
    taxDeductionType: 0,
  };
  getGrid(
    id?: number,
    additionalProps?: { classification: number; taxDeductionType: number }
  ): Observable<IHouseholdTaxDeductionGridViewDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IHouseholdTaxDeductionGridViewDTO[]>(
      getHouseholdTaxDeductionRows(
        this.getGridAdditionalProps.classification,
        this.getGridAdditionalProps.taxDeductionType
      )
    );
  }

  getGridApply(): Observable<IHouseholdTaxDeductionGridViewDTO[]> {
    return this.http.get<IHouseholdTaxDeductionGridViewDTO[]>(
      getHouseholdTaxDeductionRowsApply()
    );
  }

  getGridApplied(): Observable<IHouseholdTaxDeductionGridViewDTO[]> {
    return this.http.get<IHouseholdTaxDeductionGridViewDTO[]>(
      getHouseholdTaxDeductionRowsApplied()
    );
  }

  getGridReceived(): Observable<IHouseholdTaxDeductionGridViewDTO[]> {
    return this.http.get<IHouseholdTaxDeductionGridViewDTO[]>(
      getHouseholdTaxDeductionRowsReceived()
    );
  }

  getGridDenied(): Observable<IHouseholdTaxDeductionGridViewDTO[]> {
    return this.http.get<IHouseholdTaxDeductionGridViewDTO[]>(
      getHouseholdTaxDeductionRowsDenied()
    );
  }

  getLastUsedSequenceNumber(entityName: string): Observable<any> {
    return this.http.get<string>(getLastUsedSequenceNumber(entityName));
  }

  getInfo(invoiceId: number, customerInvoiceRowId: number): Observable<any> {
    return this.http.get<string>(
      getHouseholdTaxDeductionRowInfo(invoiceId, customerInvoiceRowId)
    );
  }

  getHouseholdRowForEdit(
    customerInvoiceRowId: number
  ): Observable<IHouseholdTaxDeductionApplicantDTO> {
    return this.http.get<IHouseholdTaxDeductionApplicantDTO>(
      getHouseholdTaxDeductionRowForEdit(customerInvoiceRowId)
    );
  }

  saveRecieved(
    rowIds: number[],
    receivedDate: Date
  ): Observable<BackendResponse> {
    return this.http.post(saveHouseholdTaxReceived(), {
      idsToUpdate: rowIds,
      bulkDate: receivedDate,
    });
  }

  savePartiallyApproved(
    id: number,
    amount: number,
    receivedDate: Date
  ): Observable<BackendResponse> {
    return this.http.post(saveHouseholdTaxPartiallyApproved(), {
      idsToUpdate: [id],
      amount: amount,
      bulkDate: receivedDate,
    });
  }

  saveApplied(ids: number[]): Observable<BackendResponse> {
    return this.http.post(saveHouseholdTaxApplied(), { idsToUpdate: ids });
  }

  saveHouseholdRowForEdit(
    applicant: IHouseholdTaxDeductionApplicantDTO
  ): Observable<BackendResponse> {
    return this.http.post(saveHouseholdTaxDeductionRowForEdit(), applicant);
  }

  saveDenied(
    invoiceId: number,
    rowId: number,
    deniedDate: Date
  ): Observable<BackendResponse> {
    return this.http.post(saveHouseholdTaxDenied(), {
      customerInvoiceId: invoiceId,
      customerInvoiceRowId: rowId,
      bulkDate: deniedDate,
    });
  }

  withdrawApplied(ids: number[]): Observable<BackendResponse> {
    return this.http.post(saveHouseholdTaxWithdrawApplied(), {
      idsToUpdate: ids,
    });
  }

  getHouseholdTaxDeductionPrintUrl(
    customerInvoiceRowIds: number[],
    reportId: number,
    sysReportTemplateTypeId: number,
    nextSequenceNumber: number,
    useGreen: boolean
  ): Observable<string> {
    return this.http.post(getHouseholdTaxDeductionPrintUrl(), {
      customerInvoiceRowIds: customerInvoiceRowIds,
      reportId: reportId,
      sysReportTemplateTypeId: sysReportTemplateTypeId,
      nextSequenceNumber: nextSequenceNumber,
      useGreen: useGreen,
    });
  }

  delete(rowId: number): Observable<BackendResponse> {
    return this.http.delete(deleteHouseholdTaxDeductionRow(rowId));
  }
}
