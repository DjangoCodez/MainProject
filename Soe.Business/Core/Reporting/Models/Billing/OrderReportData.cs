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
    public class OrderReportData : BillingReportDataManager, IReportDataModel
    {
        private readonly OrderReportDataInput _reportDataInput;
        private readonly OrderReportDataOutput _reportDataOutput;

        //Edit this if we add address columns to order analysis.
        private bool LoadAddresses => false;
        private bool LoadInternalAccounts => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_OrderAnalysisMatrixColumns.AccountInternalName1 ||
            a.Column == TermGroup_OrderAnalysisMatrixColumns.AccountInternalName2 ||
            a.Column == TermGroup_OrderAnalysisMatrixColumns.AccountInternalName3 ||
            a.Column == TermGroup_OrderAnalysisMatrixColumns.AccountInternalName4 ||
            a.Column == TermGroup_OrderAnalysisMatrixColumns.AccountInternalName5
        );

        public OrderReportData(ParameterObject parameterObject, OrderReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new OrderReportDataOutput(reportDataInput);
        }

        public OrderReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionOrderDateFrom, out DateTime selectionOrderDateTo, "orderDate"))
                return new ActionResult(false);
            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDeliveryDateFrom, out DateTime selectionDeliveryDateTo, "deliveryDate"))
                return new ActionResult(false);

            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds1, "accountInternalName1");
            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds2, "accountInternalName2");
            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds3, "accountInternalName3");
            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds4, "accountInternalName4");
            TryGetAccountDim(reportResult, out _, out List<int?> selectionAccountIds5, "accountInternalName5");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeShowOpen, "showOpen");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeShowClosed, "showClosed");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeViewMy, "viewMy");

            var addOrderFromDates = false;
            var addOrderToDates = false;
            var addDeliveryFromDates = false;
            var addDeliveryToDates = false;

            #endregion

            if (!(selectionOrderDateFrom.Date == CalendarUtility.DATETIME_MINVALUE && selectionOrderDateTo.Date == CalendarUtility.DATETIME_MAXVALUE))
            {
                if (selectionOrderDateFrom.Date != CalendarUtility.DATETIME_MINVALUE)
                {
                    addOrderFromDates = true;
                    if (selectionOrderDateTo.Date == CalendarUtility.DATETIME_MAXVALUE)
                    {
                        selectionOrderDateTo = selectionOrderDateFrom.Date;
                    }
                }

                if (selectionOrderDateTo.Date != CalendarUtility.DATETIME_MAXVALUE)
                {
                    addOrderToDates = true;
                    if (selectionOrderDateFrom.Date == CalendarUtility.DATETIME_MINVALUE)
                    {
                        selectionOrderDateFrom = selectionOrderDateTo.Date;
                        addOrderFromDates = true;
                    }
                }
            }

            if (!(selectionDeliveryDateFrom.Date == CalendarUtility.DATETIME_MINVALUE && selectionDeliveryDateTo.Date == CalendarUtility.DATETIME_MAXVALUE))
            {
                if (selectionDeliveryDateFrom.Date != CalendarUtility.DATETIME_MINVALUE)
                {
                    addDeliveryFromDates = true;
                    if (selectionDeliveryDateTo.Date == CalendarUtility.DATETIME_MAXVALUE)
                    {
                        selectionDeliveryDateTo = selectionDeliveryDateFrom.Date;
                    }
                }

                if (selectionDeliveryDateTo.Date != CalendarUtility.DATETIME_MAXVALUE)
                {
                    addDeliveryToDates = true;
                    if (selectionDeliveryDateFrom.Date == CalendarUtility.DATETIME_MINVALUE)
                    {
                        selectionDeliveryDateFrom = selectionDeliveryDateTo.Date;
                        addDeliveryFromDates = true;
                    }
                }
            }

            #region Build XML

            using (CompEntities entities = new CompEntities())
            {
                var filterModel = new Dictionary<string, object>();

                if (addOrderFromDates)
                    filterModel.Add("invoiceDateFrom", selectionOrderDateFrom);

                if (addOrderToDates)
                    filterModel.Add("invoiceDateTo", selectionOrderDateTo);

                if (addDeliveryFromDates)
                    filterModel.Add("deliveryDateFrom", selectionDeliveryDateFrom);

                if (addDeliveryToDates)
                    filterModel.Add("deliveryDateTo", selectionDeliveryDateTo);

                filterModel.Add("showOpen", selectionIncludeShowOpen);
                filterModel.Add("showClosed", selectionIncludeShowClosed);
                filterModel.Add("onlyMine", selectionIncludeViewMy);

                var orders = InvoiceManager.GetCustomerInvoicesForAnalys(entities, SoeOriginType.Order, this.ActorCompanyId, filterModel, LoadInternalAccounts, LoadAddresses);

                if (selectionAccountIds1.Count > 0)
                    orders = orders.Where(x => selectionAccountIds1.Contains(x.DefaultDim2AccountId)).ToList();
                if (selectionAccountIds2.Count > 0)
                    orders = orders.Where(x => selectionAccountIds2.Contains(x.DefaultDim3AccountId)).ToList();
                if (selectionAccountIds3.Count > 0)
                    orders = orders.Where(x => selectionAccountIds3.Contains(x.DefaultDim4AccountId)).ToList();
                if (selectionAccountIds4.Count > 0)
                    orders = orders.Where(x => selectionAccountIds4.Contains(x.DefaultDim5AccountId)).ToList();
                if (selectionAccountIds5.Count > 0)
                    orders = orders.Where(x => selectionAccountIds5.Contains(x.DefaultDim6AccountId)).ToList();

                #region Output File Content

                foreach (var order in orders)
                {
                    try
                    {
                        var item = new OrderItem();
                        item.CustomerNumber = order.ActorCustomerNr;
                        item.CustomerName = order.ActorCustomerName;
                        item.OrderNumber = order.InvoiceNr;
                        item.OrderDate = order.InvoiceDate;
                        item.DeliveryDate = order.DeliveryDate;
                        item.ProjectNumber = order.ProjectNr;
                        item.ProjectName = order.ProjectName;
                        item.AmountExVAT = order.TotalAmountExVat;
                        item.ToInvoiceExVAT = order.RemainingAmountExVat;
                        item.AccountInternalName2 = order.DefaultDim2AccountName;
                        item.AccountInternalName3 = order.DefaultDim3AccountName;
                        item.AccountInternalName4 = order.DefaultDim4AccountName;
                        item.AccountInternalName5 = order.DefaultDim5AccountName;
                        item.AccountInternalName6 = order.DefaultDim6AccountName;
                        item.OurReference = order.ReferenceOur;
                        item.SalesPriceList = order.PriceListName;
                        item.AssignmentType = order.OrderTypeName;
                        item.ReadyStateMy = order.OriginReadyUserCount;
                        item.ReadyStateAll = order.OriginUserCount;
                        item.Created = order.Created;
                        item.CreatedBy = order.InvoiceDeliveryProvider;
                        item.Changed = order.LastCreatedReminder;
                        item.ChangedBy = order.DeliverDateText;

                        _reportDataOutput.OrderItems.Add(item);
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

    public class OrderReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_OrderAnalysisMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public OrderReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_OrderAnalysisMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_OrderAnalysisMatrixColumns.Unknown;
        }
    }

    public class OrderReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<OrderReportDataField> Columns { get; set; }

        public OrderReportDataInput(CreateReportResult reportresult, List<OrderReportDataField> columns)
        {
            this.ReportResult = reportresult;
            this.Columns = columns;
        }
    }

    public class OrderReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public OrderReportDataInput Input { get; set; }

        public OrderReportDataOutput(OrderReportDataInput input)
        {
            this.OrderItems = new List<OrderItem>();
            this.Input = input;
        }
    }
}
