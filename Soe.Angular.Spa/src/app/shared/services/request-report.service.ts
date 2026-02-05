import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  printAccount,
  printHouseholdTaxDeduction,
  printInvoicesJournal,
  printIOCustomerInvoice,
  printIOVoucher,
  printProjectReport,
  printStockInventory,
  printVoucher,
  printVoucherList,
} from './generated-service-endpoints/report/RequestReport.endpoints';
import { IDownloadFileDTO } from '@shared/models/generated-interfaces/FileUploadDTO';
import { DownloadApiSoeHttpClientService } from './download-api-soe-http-client.service';
import { ReportPrintDTO } from '@shared/models/report-print/report-print.model';
import { ProjectPrintDTO } from '@shared/models/report-print/project-print.model';
import { HouseholdTaxDeductionPrintDTO } from '@shared/models/report-print/household-tax-deduction-print.model';

@Injectable({
  providedIn: 'root',
})
export class RequestReportService {
  constructor(private readonly http: DownloadApiSoeHttpClientService) {}

  printVoucher(voucherHeadId: number): Observable<IDownloadFileDTO> {
    return this.http.get(printVoucher(voucherHeadId, false));
  }

  printVoucherList(voucherHeadIds: number[]): Observable<IDownloadFileDTO> {
    const url = printVoucherList();
    const model = new ReportPrintDTO(voucherHeadIds);

    return this.http.post(url, model);
  }

  printAccount(accountId: number): Observable<IDownloadFileDTO> {
    return this.http.get(printAccount(accountId, false));
  }

  printIOCustomerInvoice(ioIds: number[]): Observable<IDownloadFileDTO> {
    const url = printIOCustomerInvoice();
    const model = new ReportPrintDTO(ioIds);

    return this.http.post(url, model);
  }

  printIOVoucher(ioIds: number[]): Observable<IDownloadFileDTO> {
    const url = printIOVoucher();
    const model = new ReportPrintDTO(ioIds);

    return this.http.post(url, model);
  }

  printStockInventory(
    reportId: number,
    stockInventoryHeadId: number
  ): Observable<IDownloadFileDTO> {
    const url = printStockInventory(reportId, stockInventoryHeadId, false);

    return this.http.get(url);
  }

  printProjectReport(
    reportItem: ProjectPrintDTO
  ): Observable<IDownloadFileDTO> {
    const url = printProjectReport();

    return this.http.post(url, reportItem);
  }

  printInvoicesJournal(ids: number[]): Observable<IDownloadFileDTO> {
    const url = printInvoicesJournal();
    const model = new ReportPrintDTO(ids);

    return this.http.post(url, model);
  }

  printHouseholdTaxDeduction(
    reportItem: HouseholdTaxDeductionPrintDTO
  ): Observable<IDownloadFileDTO> {
    const url = printHouseholdTaxDeduction();

    return this.http.post(url, reportItem);
  }


}
