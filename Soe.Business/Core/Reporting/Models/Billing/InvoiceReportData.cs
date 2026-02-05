using SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Billing
{
    public class InvoiceReportData : BillingReportDataManager, IReportDataModel
    {
        private readonly InvoiceReportDataInput _reportDataInput;
        private readonly InvoiceReportDataOutput _reportDataOutput;

        public InvoiceReportData(ParameterObject parameterObject, InvoiceReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new InvoiceReportDataOutput(reportDataInput);
        }

        //Edit this if we add address columns to order analysis.
        private bool loadAddresses => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_InvoiceAnalysisMatrixColumns.DeliveryAddress ||
            a.Column == TermGroup_InvoiceAnalysisMatrixColumns.InvoiceAddress
        );
        private bool loadInternalAccounts => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName1 ||
            a.Column == TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName2 ||
            a.Column == TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName3 ||
            a.Column == TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName4 ||
            a.Column == TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName5
        );

        public InvoiceReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionInvoiceDateFrom, out DateTime selectionInvoiceDateTo, "invoiceDate"))
                return new ActionResult(false);
            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDueDateFrom, out DateTime selectionDueDateTo, "dueDate"))
                return new ActionResult(false);

            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds1, "accountInternalName1");
            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds2, "accountInternalName2");
            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds3, "accountInternalName3");
            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds4, "accountInternalName4");
            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds5, "accountInternalName5");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeShowOpen, "showOpen");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeShowClosed, "showClosed");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeViewMy, "viewMy");
            TryGetBoolFromSelection(reportResult, out bool includePreliminaryInvoices, "showPreliminaryInvoices");

            var addInvoiceFromDates = false;
            var addInvoiceToDates = false;
            var addDueFromDates = false;
            var addDueToDates = false;

            #endregion

            if (!(selectionInvoiceDateFrom.Date == CalendarUtility.DATETIME_MINVALUE && selectionInvoiceDateTo.Date == CalendarUtility.DATETIME_MAXVALUE))
            {
                if (selectionInvoiceDateFrom.Date != CalendarUtility.DATETIME_MINVALUE)
                {
                    addInvoiceFromDates = true;
                    if (selectionInvoiceDateTo.Date == CalendarUtility.DATETIME_MAXVALUE)
                    {
                        selectionInvoiceDateTo = selectionInvoiceDateFrom.Date;
                    }
                }

                if (selectionInvoiceDateTo.Date != CalendarUtility.DATETIME_MAXVALUE)
                {
                    addInvoiceToDates = true;
                    if (selectionInvoiceDateFrom.Date == CalendarUtility.DATETIME_MINVALUE)
                    {
                        selectionInvoiceDateFrom = selectionInvoiceDateTo.Date;
                        addInvoiceFromDates = true;
                    }
                }
            }

            if (!(selectionDueDateFrom.Date == CalendarUtility.DATETIME_MINVALUE && selectionDueDateTo.Date == CalendarUtility.DATETIME_MAXVALUE))
            {
                if (selectionDueDateFrom.Date != CalendarUtility.DATETIME_MINVALUE)
                {
                    addDueFromDates = true;
                    if (selectionDueDateTo.Date == CalendarUtility.DATETIME_MAXVALUE)
                    {
                        selectionDueDateTo = selectionDueDateFrom.Date;
                    }
                }

                if (selectionDueDateTo.Date != CalendarUtility.DATETIME_MAXVALUE)
                {
                    addDueToDates = true;
                    if (selectionDueDateFrom.Date == CalendarUtility.DATETIME_MINVALUE)
                    {
                        selectionDueDateFrom = selectionDueDateTo.Date;
                        addDueFromDates = true;
                    }
                }
            }

            #region Build XML

            using (CompEntities entities = new CompEntities())
            {
                var filterModel = new Dictionary<string, object>();

                if (addInvoiceFromDates)
                    filterModel.Add("invoiceDateFrom", selectionInvoiceDateFrom);

                if (addInvoiceToDates)
                    filterModel.Add("invoiceDateTo", selectionInvoiceDateTo);

                if (addDueFromDates)
                    filterModel.Add("dueDateFrom", selectionDueDateFrom);

                if (addDueToDates)
                    filterModel.Add("dueDateTo", selectionDueDateTo);

                filterModel.Add("showOpen", selectionIncludeShowOpen);
                filterModel.Add("showClosed", selectionIncludeShowClosed);
                filterModel.Add("onlyMine", selectionIncludeViewMy);
                filterModel.Add("includePreliminaryInvoices", includePreliminaryInvoices);

                var customerInvoices = InvoiceManager.GetCustomerInvoicesForAnalys(entities, SoeOriginType.CustomerInvoice, this.ActorCompanyId, filterModel, loadInternalAccounts, loadAddresses);

                if (selectionAccountIds1.Count > 0)
                    customerInvoices = customerInvoices.Where(x => selectionAccountIds1.Contains(x.DefaultDim2AccountId)).ToList();
                if (selectionAccountIds2.Count > 0)
                    customerInvoices = customerInvoices.Where(x => selectionAccountIds2.Contains(x.DefaultDim3AccountId)).ToList();
                if (selectionAccountIds3.Count > 0)
                    customerInvoices = customerInvoices.Where(x => selectionAccountIds3.Contains(x.DefaultDim4AccountId)).ToList();
                if (selectionAccountIds4.Count > 0)
                    customerInvoices = customerInvoices.Where(x => selectionAccountIds4.Contains(x.DefaultDim5AccountId)).ToList();
                if (selectionAccountIds5.Count > 0)
                    customerInvoices = customerInvoices.Where(x => selectionAccountIds5.Contains(x.DefaultDim6AccountId)).ToList();

                #region Output File Content

                foreach (var customerInvoice in customerInvoices)
                {
                    try
                    {
                        var item = new InvoiceItem();
                        item.CustomerNumber = customerInvoice.ActorCustomerNr;
                        item.CustomerName = customerInvoice.ActorCustomerName;
                        item.InvoiceNumber = customerInvoice.InvoiceNr;
                        item.InvoiceDate = customerInvoice.InvoiceDate;
                        item.DeliveryDate = customerInvoice.DeliveryDate;
                        item.OrderDate = customerInvoice.OrderDate;
                        item.DueDate = customerInvoice.DueDate;
                        item.ProjectNumber = customerInvoice.ProjectNr;
                        item.ProjectName = customerInvoice.ProjectName;
                        item.AmountExVAT = customerInvoice.TotalAmountExVat;
                        item.ToInvoiceExVAT = customerInvoice.RemainingAmountExVat;
                        item.AccountInternalName2 = customerInvoice.DefaultDim2AccountId.HasValue ? customerInvoice.DefaultDim2AccountName : "";
                        item.AccountInternalName3 = customerInvoice.DefaultDim3AccountId.HasValue ? customerInvoice.DefaultDim3AccountName : "";
                        item.AccountInternalName4 = customerInvoice.DefaultDim4AccountId.HasValue ? customerInvoice.DefaultDim4AccountName : "";
                        item.AccountInternalName5 = customerInvoice.DefaultDim5AccountId.HasValue ? customerInvoice.DefaultDim5AccountName : "";
                        item.AccountInternalName6 = customerInvoice.DefaultDim6AccountId.HasValue ? customerInvoice.DefaultDim6AccountName : "";
                        item.OriginDescription = customerInvoice.InternalText;
                        item.InvoiceType = customerInvoice.BillingTypeName;
                        item.Currency = customerInvoice.CurrencyCode;
                        item.InvoiceLabel = customerInvoice.InvoiceLabel;
                        item.SalesPriceList = customerInvoice.PriceListName;
                        item.InvoiceAddress = customerInvoice.BillingAddress;
                        item.DeliveryAddress = customerInvoice.DeliveryAddress;
                        item.VATType = customerInvoice.VATType;
                        item.Status = customerInvoice.StatusName;
                        item.OurReference = customerInvoice.ReferenceOur;
                        item.Created = customerInvoice.Created;
                        item.CreatedBy = customerInvoice.CreatedBy;
                        item.Changed = customerInvoice.Modified;
                        item.ChangedBy = customerInvoice.ModifiedBy;

                        _reportDataOutput.InvoiceItems.Add(item);
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);
                    }


                }
                #endregion

            }

            #endregion

            return new ActionResult();
        }

    }

    public class InvoiceReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_InvoiceAnalysisMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public InvoiceReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_InvoiceAnalysisMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_InvoiceAnalysisMatrixColumns.Unknown;
        }
    }

    public class InvoiceReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<InvoiceReportDataField> Columns { get; set; }

        public InvoiceReportDataInput(CreateReportResult reportresult, List<InvoiceReportDataField> columns)
        {
            this.ReportResult = reportresult;
            this.Columns = columns;
        }
    }

    public class InvoiceReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; }
        public InvoiceReportDataInput Input { get; set; }

        public InvoiceReportDataOutput(InvoiceReportDataInput input)
        {
            this.InvoiceItems = new List<InvoiceItem>();
            this.Input = input;
        }
    }
}
