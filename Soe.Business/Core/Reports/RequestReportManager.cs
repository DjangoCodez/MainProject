namespace SoftOne.Soe.Business.Core.Reports
{
    using SoftOne.Soe.Business.Util;
    using SoftOne.Soe.Business.Util.Converter;
    using SoftOne.Soe.Common.DTO;
    using SoftOne.Soe.Common.DTO.Reports;
    using SoftOne.Soe.Common.Util;
    using SoftOne.Soe.Data;
    using SoftOne.Soe.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class RequestReportManager : ManagerBase
    {
        #region Variables

        private readonly ReportManager rm;
        private readonly ReportDataManager rdm;

        #endregion

        #region Ctor

        public RequestReportManager( ParameterObject parameterObject, ReportManager rm, ReportDataManager rdm) : base(parameterObject) 
        {
            this.rm = rm;
            this.rdm = rdm;
        }

        #endregion

        #region Print

        private DownloadFileDTO GetNoReportError()
        {
            return new DownloadFileDTO
            {
                Success = false,
                ErrorMessage = GetText(1336, "Rapport hittades inte")
            };
        }


        private int GetReportId(CompanySettingType companySettingType, SoeReportTemplateType reportTemplatetype, bool checkRolePermission)
        {
            int? roleId = null;

            if (checkRolePermission)
            {
                roleId = base.RoleId;
            }

            int reportId = rm.GetCompanySettingReportId(
                SettingMainType.Company,
                companySettingType,
                reportTemplatetype,
                base.ActorCompanyId,
                base.UserId,
                roleId);

            return reportId;
        }

        private DownloadFileDTO GetDownloadFile(ReportJobDefinitionDTO reportJobDefinitionDTO, bool queue, bool asBinary = false, bool internalPrint = false)
        {
            ReportPrintoutDTO reportResult = ReportDataManager
                .PrintMigratedReportDTO(
                    reportJobDefinitionDTO,
                    base.ActorCompanyId,
                    base.UserId,
                    base.RoleId,
                    forcePrint: !queue,
                    skipApiInternal: !queue,
                    internalPrint: internalPrint);

            DownloadFileDTO downloadFileDTO;
            if (reportResult == null)
            {
                downloadFileDTO = new DownloadFileDTO
                {
                    Success = false,
                    ErrorMessage = GetText(5947, "Utskrift misslyckades")
                };
            }
            else
            {
                string fileName = reportResult.ReportFileName + reportResult.ReportFileType;
                bool success = queue || reportResult.Data != null;
                string errorMessage = string.Empty;

                if (!success)
                {
                    errorMessage = GetText(5947, "Utskrift misslyckades") + "\n" + $"{reportResult.Status}" +
                        $":{reportResult.ResultMessage}" +
                        $":{reportResult.ResultMessageDetails}";
                }
                downloadFileDTO = new DownloadFileDTO
                {
                    Success = success,
                    FileName = fileName,
                    FileType = WebUtil.GetContentType(fileName),
                    ErrorMessage = errorMessage,
                    Content = ""
                };

                if (reportResult.Data != null)
                {
                    if (asBinary)
                    {
                        downloadFileDTO.BinaryData = reportResult.Data;
                    }
                    else
                    {
                        downloadFileDTO.Content = Convert.ToBase64String(reportResult.Data);
                    }
                }
            }

            return downloadFileDTO;
        }

        private TermGroup_ReportExportType GetExportType(ReportPrintDTO printDTO)
        {
           if (printDTO.exportType != TermGroup_ReportExportType.Unknown)
            {
                return printDTO.exportType;
            }

            var exportType = rm.GetReportExportType(printDTO.ReportId, base.ActorCompanyId);
            if (exportType == TermGroup_ReportExportType.Unknown)
            {
                return TermGroup_ReportExportType.Pdf;
            }
            else
            {
                return exportType;
            }
        }

        public DownloadFileDTO PrintProjectReport(ProjectPrintDTO dto)
        {
            ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                dto.ReportId, 
                dto.SysReportTemplateTypeId,
                GetExportType(dto));

            reportJobDefinitionDTO.Selections.Add(
                new IdListSelectionDTO(
                    dto.Ids, 
                    "IdListSelectionDTO", 
                    "selectedProjectIds"));
            
            reportJobDefinitionDTO.Selections.Add(
                new BoolSelectionDTO(
                    dto.IncludeChildProjects, 
                    "BoolSelectionDTO", 
                    "includeChildProjects"));

            if (dto.DateFrom.HasValue || dto.DateTo.HasValue)
            {
                reportJobDefinitionDTO.Selections.Add(
                new DateRangeSelectionDTO
                {
                    From = dto.DateFrom.HasValue ? dto.DateFrom.Value : CalendarUtility.DATETIME_MINVALUE,
                    To = dto.DateTo.HasValue ? dto.DateTo.Value : CalendarUtility.DATETIME_MAXVALUE,
                    Key = "transactionDateRange",
                    TypeName = "DateRangeSelectionDTO",
                });
            }

            DownloadFileDTO downloadFile = GetDownloadFile(reportJobDefinitionDTO, dto.Queue);
            return downloadFile;
        }

        public DownloadFileDTO PrintProjectTimeBook(ProjectTimeBookPrintDTO dto)
        {

            var sysReportTemplateType = SoeReportTemplateType.TimeProjectReport;
            dto.ReportId = GetReportId(CompanySettingType.BillingDefaultTimeProjectReportTemplate, sysReportTemplateType, false);

            ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                dto.ReportId,
                sysReportTemplateType,
                GetExportType(dto));

            //SB_IncludeOnlyInvoiced

            reportJobDefinitionDTO.Selections.Add(
                new IdListSelectionDTO(
                    new List<int>() { dto.InvoiceId },
                    "IdListSelectionDTO",
                    "invoiceIds"));

            reportJobDefinitionDTO.Selections.Add(
                new IdListSelectionDTO(
                    new List<int>() { dto.ProjectId },
                    "IdListSelectionDTO",
                    "selectedProjectIds"));

            reportJobDefinitionDTO.Selections.Add(
                  new BoolSelectionDTO(
                      true,
                      "BoolSelectionDTO",
                      "includeProjectReport"));

            reportJobDefinitionDTO.Selections.Add(
                  new BoolSelectionDTO(
                      dto.IncludeOnlyInvoiced,
                      "BoolSelectionDTO",
                      "includeOnlyInvoiced"));

            if (dto.DateFrom.HasValue || dto.DateTo.HasValue)
            {
                reportJobDefinitionDTO.Selections.Add(
                new DateRangeSelectionDTO
                {
                    From = dto.DateFrom.HasValue ? dto.DateFrom.Value : CalendarUtility.DATETIME_MINVALUE,
                    To = dto.DateTo.HasValue ? dto.DateTo.Value : CalendarUtility.DATETIME_MAXVALUE,
                    Key = "transactionDateRange",
                    TypeName = "DateRangeSelectionDTO",
                });
            }

            DownloadFileDTO downloadFile = GetDownloadFile(reportJobDefinitionDTO, dto.Queue, dto.ReturnAsBinary);
            return downloadFile;


        }

        public DownloadFileDTO PrintVoucherList(ReportPrintDTO model)
        {
            CompanySettingType companySettingType = CompanySettingType.AccountingDefaultVoucherList;

            DownloadFileDTO downloadFile = PrintVouchers(model, companySettingType);
            return downloadFile;
        }

        public DownloadFileDTO PrintVoucherDefaultAccountingOrder(int voucherHead, bool queue)
        {
            CompanySettingType companySettingType = CompanySettingType.AccountingDefaultAccountingOrder;
            List<int> voucherHeadIds = new List<int> { voucherHead };

            ReportPrintDTO model = new ReportPrintDTO
            {
                Ids = voucherHeadIds,
                Queue = queue
            };

            DownloadFileDTO downloadFile = PrintVouchers(
                model, companySettingType);
            return downloadFile;
        }

        private DownloadFileDTO PrintVouchers(ReportPrintDTO model, CompanySettingType companySettingType)
        {
            DownloadFileDTO downloadFile;
            SoeReportTemplateType reportTemplatetype = SoeReportTemplateType.VoucherList;
            int reportId = GetReportId(
                companySettingType, reportTemplatetype, false);

            if (reportId > 0)
            {
                ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                    reportId,
                    reportTemplatetype,
                    TermGroup_ReportExportType.Pdf);

                reportJobDefinitionDTO.Selections.Add(
                    new IdListSelectionDTO(
                        model.Ids,
                        "IdListSelectionDTO",
                        "voucherHeadIds"));

                reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                        true,
                        "BoolSelectionDTO",
                        "isAccountingOrder"));

                downloadFile = GetDownloadFile(
                    reportJobDefinitionDTO,
                    model.Queue);
            }
            else
            {
                downloadFile = GetNoReportError();
            }

            return downloadFile;

        }

        public DownloadFileDTO PrintAccount(int accountId, bool queue)
        {
            DownloadFileDTO downloadFile;
            SoeReportTemplateType reportTemplatetype = SoeReportTemplateType.GeneralLedger;

            // Used specifically from "Kontoanalysen" in account edit page
            int reportId = GetReportId(
                CompanySettingType.AccountingDefaultAnalysisReport,
                reportTemplatetype, 
                true);
            Account account = AccountManager.GetAccount(
                base.ActorCompanyId,
                accountId,
                onlyActive: false);

            if (reportId > 0 && account != null)
            {
                ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                    reportId,
                    reportTemplatetype,
                    TermGroup_ReportExportType.Pdf);

                AccountFilterSelectionDTO filter = new AccountFilterSelectionDTO 
                {
                    Id = account.AccountDimId,
                    From = account.AccountNr,
                    To = account.AccountNr
                };

                AccountFilterSelectionsDTO selectionsDTO = new AccountFilterSelectionsDTO
                {
                    Filters = new List<AccountFilterSelectionDTO> { filter },
                    Key = "namedFilterRanges"
                };

                reportJobDefinitionDTO.Selections.Add(selectionsDTO);

                downloadFile = GetDownloadFile(
                    reportJobDefinitionDTO, queue);
            }
            else if (reportId <= 0)
            {
                downloadFile = GetNoReportError();
            }
            else
            {
                downloadFile = new DownloadFileDTO
                {
                    Success = false,
                    ErrorMessage = GetText(1481, "Konto hittades inte")
                };
            }

            return downloadFile;
        }

        private static ReportJobDefinitionDTO GetReportJobForBalanceList(
            int reportId,
            SoeReportTemplateType reportTemplatetype,
            List<int> invoiceIds,
            bool showPreliminaryInvoices,
            List<int> paymentRowIds,
            TermGroup_ReportLedgerInvoiceSelection ledgerInvoiceSelection, DateTime? dateTo)
        {
            ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                reportId,
                reportTemplatetype,
                TermGroup_ReportExportType.Pdf);

            reportJobDefinitionDTO.Selections.Add(
                new IdListSelectionDTO(
                    invoiceIds,
                    "IdListSelectionDTO",
                    "invoiceIds"));

            reportJobDefinitionDTO.Selections.Add(
                new IdSelectionDTO(
                    (int)ledgerInvoiceSelection,
                    "invoiceSelection"));

            reportJobDefinitionDTO.Selections.Add(
                new IdSelectionDTO(
                    (int)TermGroup_ReportLedgerDateRegard.InvoiceDate,
                    "dateRegard"));

            reportJobDefinitionDTO.Selections.Add(
                new IdSelectionDTO(
                    (int)SoeReportSortOrder.ActorName,
                    "sortOrder"));

            reportJobDefinitionDTO.Selections.Add(
                new BoolSelectionDTO(
                    showPreliminaryInvoices,
                    "BoolSelectionDTO",
                    "showPreliminaryInvoices"));

            if (paymentRowIds?.Any() == true)
            {
                reportJobDefinitionDTO.Selections.Add(
                    new IdListSelectionDTO(
                        paymentRowIds,
                        "IdListSelectionDTO",
                        "paymentRowIds"));
            }


            if (dateTo.HasValue)
            {
                reportJobDefinitionDTO.Selections.Add(
                new DateRangeSelectionDTO
                {
                    From = CalendarUtility.DATETIME_MINVALUE,
                    To = dateTo.Value,
                    Key = "transactionDateRange",
                    TypeName = "DateRangeSelectionDTO",
                });
            }

            return reportJobDefinitionDTO;
        }

        public DownloadFileDTO PrintBalanceList(BalanceListPrintDTO model, SoeReportTemplateType reportTemplatetype)
        {
            int reportId = GetReportId(model.CompanySettingType, reportTemplatetype, false);

            DownloadFileDTO downloadFile;
            if (reportId > 0)
            {
                DateTime? date = reportTemplatetype == SoeReportTemplateType.SupplierBalanceList ? null : DateTime.Today;
				ReportJobDefinitionDTO reportJobDefinitionDTO = GetReportJobForBalanceList(
                    reportId,
                    reportTemplatetype,
                    model.Ids,
                    showPreliminaryInvoices: false,
                    model.PaymentRowIds, 
                    TermGroup_ReportLedgerInvoiceSelection.NotPayedAndPartlyPayed,
					date);

                downloadFile = GetDownloadFile(reportJobDefinitionDTO, model.Queue);
            }
            else
            {
                downloadFile = GetNoReportError();
            }

            return downloadFile;
        }

        public DownloadFileDTO PrintInvoicesJournal(ReportPrintDTO model)
        {
            var report = rm.GetReport(model.ReportId, ActorCompanyId, loadSysReportTemplateType: true);

            DownloadFileDTO downloadFile;
            if (report != null)
            {
                ReportJobDefinitionDTO reportJobDefinitionDTO = GetReportJobForBalanceList(
                    report.ReportId,
                    (SoeReportTemplateType)report.SysReportTemplateTypeId,
                    model.Ids,
                    showPreliminaryInvoices: true,
                    paymentRowIds: null,
                    ledgerInvoiceSelection: TermGroup_ReportLedgerInvoiceSelection.All, null);

                downloadFile = GetDownloadFile(reportJobDefinitionDTO, model.Queue);
            }
            else
            {
                downloadFile = GetNoReportError();
            }

            return downloadFile;
        }

        public DownloadFileDTO PrintIOVoucher(ReportPrintDTO model)
        {
            SoeReportTemplateType reportTemplatetype
                = SoeReportTemplateType.IOVoucher;


            Report report = rm.GetSettingOrStandardReport(
                SettingMainType.Company,
                CompanySettingType.Unknown,
                reportTemplatetype,
                SoeReportType.CrystalReport,
                ActorCompanyId,
                UserId,
                RoleId);

            DownloadFileDTO downloadFile;
            if (report != null)
            {
                ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                    report.ReportId,
                    reportTemplatetype,
                    TermGroup_ReportExportType.Pdf);

                reportJobDefinitionDTO.Selections.Add(
                    new IdListSelectionDTO(
                        model.Ids,
                        "IdListSelectionDTO",
                        "voucherHeadIOIds"));

                downloadFile = GetDownloadFile(
                    reportJobDefinitionDTO, model.Queue);
            }
            else
            {
                downloadFile = GetNoReportError();
            }

            return downloadFile;
        }

        public DownloadFileDTO PrintIOCustomerInvoice(ReportPrintDTO model)
        {
            SoeReportTemplateType reportTemplatetype
                = SoeReportTemplateType.IOCustomerInvoice;


            Report report = rm.GetSettingOrStandardReport(
                SettingMainType.Company,
                CompanySettingType.Unknown,
                reportTemplatetype,
                SoeReportType.CrystalReport,
                ActorCompanyId,
                UserId,
                RoleId);


            DownloadFileDTO downloadFile;
            if (report != null)
            {
                ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                    report.ReportId,
                    reportTemplatetype,
                    TermGroup_ReportExportType.Pdf);

                reportJobDefinitionDTO.Selections.Add(
                    new IdListSelectionDTO(
                        model.Ids,
                        "IdListSelectionDTO",
                        "customerInvoiceHeadIOIds"));

                downloadFile = GetDownloadFile(
                    reportJobDefinitionDTO, model.Queue);
            }
            else
            {
                downloadFile = GetNoReportError();
            }

            return downloadFile;
        }

        public DownloadFileDTO PrintInventoryReport(int reportId, int stockInventoryHeadId, bool queue)
        {
            ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                reportId,
                SoeReportTemplateType.StockInventoryReport,
                TermGroup_ReportExportType.Pdf);

            reportJobDefinitionDTO.Selections.Add(
                new IdSelectionDTO(stockInventoryHeadId, "stockInventory"));
            reportJobDefinitionDTO.Selections.Add(
                new IdSelectionDTO(4, "sortOrder"));

            DownloadFileDTO downloadFile = GetDownloadFile(
                reportJobDefinitionDTO, queue);

            return downloadFile;

        }

        public DownloadFileDTO PrintHouseholdTaxDeduction(HouseholdTaxDeductionPrintDTO model)
        {
            Report report = rm.GetSettingOrStandardReport(
                SettingMainType.Company,
                CompanySettingType.BillingDefaultHouseholdDeductionTemplate,
                SoeReportTemplateType.HousholdTaxDeduction,
                SoeReportType.CrystalReport,
                ActorCompanyId,
                UserId,
                RoleId);



            DownloadFileDTO downloadFile;
            if (report != null)
            {
                ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                    report.ReportId,
                    model.SysReportTemplateTypeId,
                    TermGroup_ReportExportType.Pdf);


                reportJobDefinitionDTO.Selections.Add(
                    new IdSelectionDTO((int)TermGroup_ReportBillingInvoiceSortOrder.InvoiceNr, 
                    "sortOrder"));

                reportJobDefinitionDTO.Selections.Add(
                    new IdSelectionDTO(ActorCompanyId,
                    "companyId"));

                reportJobDefinitionDTO.Selections.Add(
                    new IdSelectionDTO(model.SequenceNumber,
                    "sequenceNumber"));

                reportJobDefinitionDTO.Selections.Add(
                    new IdListSelectionDTO(
                        model.Ids,
                        "IdListSelectionDTO",
                        "customerInvoiceRowIds"));

                reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                        model.UseGreen,
                        "BoolSelectionDTO",
                        "useGreen"));

                reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                        true,
                        "BoolSelectionDTO",
                        "useInputSeqNbr"));


                downloadFile = GetDownloadFile(
                    reportJobDefinitionDTO, model.Queue);
            }
            else
            {
                downloadFile = GetNoReportError();
            }

            return downloadFile;
        }

        public DownloadFileDTO PrintCustomerInvoice(CustomerInvoicePrintDTO model, bool internalPrint)
        {
            CompanySettingType GetReportSettingType(OrderInvoiceRegistrationType type)
            {
                switch (type)
                {
                    case OrderInvoiceRegistrationType.Contract:
                        return CompanySettingType.BillingDefaultContractTemplate;
                    case OrderInvoiceRegistrationType.Offer:
                        return CompanySettingType.BillingDefaultOfferTemplate;
                    case OrderInvoiceRegistrationType.Order:
                        return CompanySettingType.BillingDefaultOrderTemplate;
                    case OrderInvoiceRegistrationType.Invoice:
                        return CompanySettingType.BillingDefaultInvoiceTemplate;
                    default:
                        return CompanySettingType.Unknown;
                }
            }

            SoeReportTemplateType GetReportTemplateType(OrderInvoiceRegistrationType type, bool asReminder)
            {
                switch (type)
                {
                    case OrderInvoiceRegistrationType.Contract:
                        return SoeReportTemplateType.BillingContract;
                    case OrderInvoiceRegistrationType.Offer:
                        return SoeReportTemplateType.BillingOffer;
                    case OrderInvoiceRegistrationType.Order:
                        return SoeReportTemplateType.BillingOrder;
                    case OrderInvoiceRegistrationType.Invoice:
                        return asReminder ? SoeReportTemplateType.BillingInvoiceReminder : SoeReportTemplateType.BillingInvoice;
                    default:
                        return SoeReportTemplateType.Unknown;
                }
            }

            var standardReport = model.ReportId > 0 ? rm.GetReport(model.ReportId, ActorCompanyId, loadSysReportTemplateType: true) : rm.GetSettingOrStandardReport(
                                                                                                    SettingMainType.Company,
                                                                                                    GetReportSettingType(model.OrderInvoiceRegistrationType),
                                                                                                    GetReportTemplateType(model.OrderInvoiceRegistrationType, model.AsReminder),
                                                                                                    SoeReportType.CrystalReport,
                                                                                                    ActorCompanyId,
                                                                                                    UserId,
                                                                                                    RoleId);

            if (model.exportType == TermGroup_ReportExportType.Unknown)
            {
                model.exportType = (TermGroup_ReportExportType)standardReport.ExportType;
            }

            DownloadFileDTO downloadFile;
            if (standardReport != null)
            {
                ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(
                    standardReport.ReportId,
                    (SoeReportTemplateType)standardReport.SysReportTemplateTypeId,
                    GetExportType(model));

                reportJobDefinitionDTO.Selections.Add(
                    new IdListSelectionDTO(
                        model.Ids,
                        "IdListSelectionDTO",
                        "invoiceIds"));

                reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                        model.PrintTimeReport,
                        "BoolSelectionDTO",
                        "printTimeReport"));

                reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                        model.IncludeOnlyInvoiced,
                        "BoolSelectionDTO",
                        "includeOnlyInvoiced"));

                reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                        true,
                        "BoolSelectionDTO",
                        "showCopies"));

                reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                        true,
                        "BoolSelectionDTO",
                        "showAnyUnprinted"));

                reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                        !model.InvoiceCopy,
                        "BoolSelectionDTO",
                        "showCopiesOfOriginal"));

                reportJobDefinitionDTO.Selections.Add(
                new BoolSelectionDTO(
                    model.OrderInvoiceRegistrationType == OrderInvoiceRegistrationType.Order,
                    "BoolSelectionDTO",
                    "includeclosedorder"));

                reportJobDefinitionDTO.Selections.Add(
                new BoolSelectionDTO(
                   true,
                    "BoolSelectionDTO",
                    "includedrafts"));

                reportJobDefinitionDTO.Selections.Add(
                    new IdSelectionDTO(model.ReportLanguageId ?? 0,
                    "languageId"));

                if (model.AsReminder)
                {
                    reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                       true,
                        "BoolSelectionDTO",
                        "asreminder"));
                }

                var attachments = new List<KeyValuePair<string, byte[]>>();

                // Handle attachments
                if (!model.ChecklistIds.IsNullOrEmpty())
                {
                    var checkListInvoiceId = model.Ids[0];
                    var chm = new ChecklistManager(this.parameterObject);

                    //if invoice checklist are saved on order....
                    if (model.OrderInvoiceRegistrationType == OrderInvoiceRegistrationType.Invoice)
                    {
                        var checkListHeadRecord = chm.GetChecklistHeadRecord(model.ChecklistIds.First(), base.ActorCompanyId);
                        checkListInvoiceId = checkListHeadRecord == null ? model.Ids[0] : checkListHeadRecord.RecordId;
                    }

                    var checklists = chm.GetChecklistAsDocuments(rm, rdm, checkListInvoiceId, model.ChecklistIds.ToList(), base.ActorCompanyId);
                    if (checklists.Any())
                    {
                        attachments.AddRange(checklists);
                    }
                }

                if (!model.AttachmentIds.IsNullOrEmpty() && reportJobDefinitionDTO.ExportType == TermGroup_ReportExportType.Pdf)
                {
                    var invoiceAttachments = InvoiceDistributionManager.GetInvoiceDocuments(model.Ids[0], base.ActorCompanyId, model.OrderInvoiceRegistrationType == OrderInvoiceRegistrationType.Order ? SoeOriginType.Order : SoeOriginType.CustomerInvoice, model.AttachmentIds.ToList(), true, false);
                    invoiceAttachments.ForEach(a =>
                    {
                        if (FileUtil.IsImageFile(a.Item2))
                            attachments.Add(new KeyValuePair<string, byte[]>(Path.GetFileNameWithoutExtension(a.Item2) + ".pdf", PdfConvertConnector.ConvertImageToPdf(a.Item3, a.Item2, false)));
                        else if (FileUtil.GetFileType(a.Item2) == SoeFileType.Pdf)
                            attachments.Add(new KeyValuePair<string, byte[]>(a.Item2, a.Item3));
                    });
                }

                if (attachments.Any())
                {
                    reportJobDefinitionDTO.Selections.Add(
                    new BoolSelectionDTO(
                       true,
                        "BoolSelectionDTO",
                        "mergepdfs"));

                    reportJobDefinitionDTO.Selections.Add(
                    new AttachmentsListSelectionDTO(attachments,
                    "AttachmentsListSelectionDTO",
                    "attachments"));
                }


                downloadFile = GetDownloadFile(
                    reportJobDefinitionDTO, model.Queue && model.Ids.Count == 1 && (model.AttachmentIds.IsNullOrEmpty()) && (model.ChecklistIds.IsNullOrEmpty()), model.ReturnAsBinary, internalPrint);

            }
            else
            {
                downloadFile = GetNoReportError();
            }

            return downloadFile;
        }

        #endregion
    }
}
