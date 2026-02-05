import { BalanceListPrintDTO } from "../../Common/Models/RequestReports/BalanceListPrintDTO";
import { HouseholdTaxDeductionPrintDTO } from "../../Common/Models/RequestReports/HouseholdTaxDeductionPrintDTO";
import { ProjectPrintDTO } from "../../Common/Models/RequestReports/ProjectPrintDTO";
import { ReportPrintDTO } from "../../Common/Models/RequestReports/ReportPrintDTO";
import { ICustomerInvoicePrintDTO, IDownloadFileDTO, IProjectTimeBookPrintDTO } from "../../Scripts/TypeLite.Net4";
import { Constants } from "../../Util/Constants";
import { IRequestReportApiService } from "./RequestReportApiService";

export interface IRequestReportService {
  // GET
    printVoucher(id: number): ng.IPromise<IDownloadFileDTO>;
    printVoucherList(ids: number[]): ng.IPromise<IDownloadFileDTO>;
    printAccount(id: number): ng.IPromise<IDownloadFileDTO>;
    printSupplierBalanceList(
    reportItem: BalanceListPrintDTO
    ): ng.IPromise<IDownloadFileDTO>;
    printCustomerBalanceList(ids: number[]): ng.IPromise<IDownloadFileDTO>;
    printInvoicesJournal(reportId: number, ids: number[]): ng.IPromise<IDownloadFileDTO>;
    printIOCustomerInvoice(ids: number[]): ng.IPromise<IDownloadFileDTO>;
    printIOVoucher(ids: number[]): ng.IPromise<IDownloadFileDTO>;
    printHouseholdTaxDeduction(reportItem: HouseholdTaxDeductionPrintDTO): ng.IPromise<IDownloadFileDTO>;
    printCustomerInvoice(reportItem: ICustomerInvoicePrintDTO): ng.IPromise<IDownloadFileDTO>;

    // POST
    printProjectReport(reportItem: ProjectPrintDTO): ng.IPromise<IDownloadFileDTO>;
    printProjectTimebookReport(reportItem: IProjectTimeBookPrintDTO): ng.IPromise<IDownloadFileDTO>;
}

export class RequestReportService implements IRequestReportService {
  //@ngInject
  constructor(
    private readonly requestReportApiService: IRequestReportApiService
  ) {}

  // GET
  printVoucher(id: number): ng.IPromise<IDownloadFileDTO> {
    const url = Constants.WEBAPI_REQUEST_REPORT_VOUCHER + id;

    return this.requestReportApiService.get(url);
  }

  printAccount(id: number): ng.IPromise<IDownloadFileDTO> {
    const url = Constants.WEBAPI_REQUEST_REPORT_ACCOUNT + id;

    return this.requestReportApiService.get(url);
  }

  // POST
  printProjectReport(
    reportItem: ProjectPrintDTO
  ): ng.IPromise<IDownloadFileDTO> {
    return this.requestReportApiService.post(
      Constants.WEBAPI_REQUEST_REPORT_PROJECT,
      reportItem
    );
  }

    printProjectTimebookReport(
        reportItem: IProjectTimeBookPrintDTO
    ): ng.IPromise<IDownloadFileDTO> {
        return this.requestReportApiService.post(Constants.WEBAPI_REQUEST_REPORT_PROJECT_TIMEBOOK,
            reportItem
        );
    }

  printVoucherList(ids: number[]): ng.IPromise<IDownloadFileDTO> {
    const value: ReportPrintDTO = new ReportPrintDTO(ids);

    return this.requestReportApiService.post(
      Constants.WEBAPI_REQUEST_REPORT_VOUCHER_LIST,
      value
    );
  }

  printSupplierBalanceList(
    reportItem: BalanceListPrintDTO
  ): ng.IPromise<IDownloadFileDTO> {
    return this.requestReportApiService.post(
      Constants.WEBAPI_REQUEST_REPORT_SUPPLIER_BALANCE_LIST,
      reportItem
    );
  }

  printCustomerBalanceList(ids: number[]): ng.IPromise<IDownloadFileDTO> {
    const value: ReportPrintDTO = new ReportPrintDTO(ids);

    return this.requestReportApiService.post(
      Constants.WEBAPI_REQUEST_REPORT_CUSTOMER_BALANCE_LIST,
      value
    );
  }

    printInvoicesJournal(reportId: number, ids: number[]): ng.IPromise<IDownloadFileDTO> {
        const value: ReportPrintDTO = new ReportPrintDTO(ids);
        value.reportId = reportId;

    return this.requestReportApiService.post(
      Constants.WEBAPI_REQUEST_REPORT_INVOICES_JOURNAL,
      value
    );
  }

  printIOCustomerInvoice(ids: number[]): ng.IPromise<IDownloadFileDTO> {
    const value: ReportPrintDTO = new ReportPrintDTO(ids);

    return this.requestReportApiService.post(
      Constants.WEBAPI_REQUEST_REPORT_IO_CUSTOMER_INVOICE,
      value
    );
  }

  printIOVoucher(ids: number[]): ng.IPromise<IDownloadFileDTO> {
    const value: ReportPrintDTO = new ReportPrintDTO(ids);

    return this.requestReportApiService.post(
      Constants.WEBAPI_REQUEST_REPORT_IO_VOUCHER,
      value
    );
  }

  printHouseholdTaxDeduction(
    reportItem: HouseholdTaxDeductionPrintDTO
  ): ng.IPromise<IDownloadFileDTO> {
    return this.requestReportApiService.post(
      Constants.WEBAPI_REQUEST_REPORT_HOUSEHOLD_TAX_DEDUCTION,
      reportItem
    );
    }

    printCustomerInvoice(reportItem: ICustomerInvoicePrintDTO): ng.IPromise<IDownloadFileDTO> {
        return this.requestReportApiService.post(
            Constants.WEBAPI_REQUEST_REPORT_HOUSEHOLD_CUSTOMERINVOICE,
            reportItem, 
            reportItem?.queue ?? true,
        );
    }
}
