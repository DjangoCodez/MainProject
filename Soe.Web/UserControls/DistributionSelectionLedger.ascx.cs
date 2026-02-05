using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class DistributionSelectionLedger : ControlBase
    {
        #region Variables

        public NameValueCollection F { get; set; }
        public ReportSelection ReportSelection { get; set; }
        public SoeSelectionType SelectionType { get; set; } //Defines the selection type (ex: customer or supplier)
        public SoeReportTemplateType ReportTemplateType { get; set; } //Further defines the selection for specific reports (ex: Balancelist, Invoicejournal, Paymentjournal)
        public bool DisableAllButNr { get; set; }
        protected string SelectionTitle { get; set; }

        private ReportManager rm;

        #endregion

        public void Populate(bool repopulate)
        {
            rm = new ReportManager(PageBase.ParameterObject);
            var sm = new SettingManager(PageBase.ParameterObject);
            //Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> supplierSettingsDict = sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Supplier, PageBase.ParameterObject.ActorCompanyId);

            #region Init

            if (DisableAllButNr)
            {
                InvoiceSelection.Visible = false;
                InvoiceSeqNr.Visible = false;
                Date.Visible = false;
                DateRegard.Visible = false;
                SortOrder.Visible = false;
                ShowVoucher.Visible = false;
                ShowPendingPaymentsInReport.Visible = false;
            }
            if (!IsSupplierLedger())
                ShowPendingPaymentsInReport.Visible = false;
            #endregion

            #region Environment

            SetupEnvironment();

            #endregion

            #region Populate

            //InvoiceSelection
            Dictionary<int, string> invoiceSelectionDict = PageBase.GetGrpText(TermGroup.ReportLedgerInvoiceSelection);
            if (IsPaymentJournal())
            {
                if (invoiceSelectionDict.ContainsKey((int)TermGroup_ReportLedgerInvoiceSelection.All))
                    invoiceSelectionDict.Remove((int)TermGroup_ReportLedgerInvoiceSelection.All);
                if (invoiceSelectionDict.ContainsKey((int)TermGroup_ReportLedgerInvoiceSelection.NotPayed))
                    invoiceSelectionDict.Remove((int)TermGroup_ReportLedgerInvoiceSelection.NotPayed);
                if (invoiceSelectionDict.ContainsKey((int)TermGroup_ReportLedgerInvoiceSelection.NotPayedAndPartlyPayed))
                    invoiceSelectionDict.Remove((int)TermGroup_ReportLedgerInvoiceSelection.NotPayedAndPartlyPayed);
            }

            InvoiceSelection.ConnectDataSource(invoiceSelectionDict);

            //SortOrder
            Dictionary<int, string> sortOrderDict = new Dictionary<int, string>();
            if (IsSupplierLedger())
                sortOrderDict = PageBase.GetGrpText(TermGroup.ReportSupplierLedgerSortOrder);
            else if (IsCustomerLedger())
                sortOrderDict = PageBase.GetGrpText(TermGroup.ReportCustomerLedgerSortOrder);

            SortOrder.ConnectDataSource(sortOrderDict);

            //DateRegard
            Dictionary<int, string> dateRegardDict = PageBase.GetGrpText(TermGroup.ReportLedgerDateRegard);
            if (IsPaymentJournal())
            {
                if (dateRegardDict.ContainsKey((int)TermGroup_ReportLedgerDateRegard.InvoiceDate))
                    dateRegardDict.Remove((int)TermGroup_ReportLedgerDateRegard.InvoiceDate);
                if (dateRegardDict.ContainsKey((int)TermGroup_ReportLedgerDateRegard.VoucherDate))
                    dateRegardDict.Remove((int)TermGroup_ReportLedgerDateRegard.VoucherDate);
                if (dateRegardDict.ContainsKey((int)TermGroup_ReportLedgerDateRegard.DueDate))
                    dateRegardDict.Remove((int)TermGroup_ReportLedgerDateRegard.DueDate);
            }

            DateRegard.ConnectDataSource(dateRegardDict);

            if (dateRegardDict.ContainsKey((int)TermGroup_ReportLedgerDateRegard.InvoiceDate))
                DateRegard.Value = ((int)TermGroup_ReportLedgerDateRegard.InvoiceDate).ToString();
            else if (dateRegardDict.ContainsKey((int)TermGroup_ReportLedgerDateRegard.PaymentDate))
                DateRegard.Value = ((int)TermGroup_ReportLedgerDateRegard.PaymentDate).ToString();

            #endregion

            #region Set data

            //Default values
            if (IsSupplierLedger())
                // Show pending payments in saldolistan(?)
                ShowPendingPaymentsInReport.Value = sm.GetSettingFromDict(supplierSettingsDict, (int)CompanySettingType.SupplierInvoiceReportShowPendingPayments, (int)SettingDataType.Boolean);
            if (IsPaymentJournal())
                InvoiceSelection.Value = ((int)TermGroup_ReportLedgerInvoiceSelection.FullyPayedAndPartlyPayed).ToString();
            else
                InvoiceSelection.Value = ((int)TermGroup_ReportLedgerInvoiceSelection.NotPayed).ToString();

            if (repopulate && SoeForm != null && SoeForm.PreviousForm != null)
            {
                if (IsSupplierLedger())
                    SupplierNr.PreviousForm = SoeForm.PreviousForm;
                else if (IsCustomerLedger())
                    CustomerNr.PreviousForm = SoeForm.PreviousForm;
                InvoiceSeqNr.PreviousForm = SoeForm.PreviousForm;
                Date.PreviousForm = SoeForm.PreviousForm;
                InvoiceSelection.Value = SoeForm.PreviousForm["InvoiceType"];
                SortOrder.Value = SoeForm.PreviousForm["SortOrder"];
                DateRegard.Value = SoeForm.PreviousForm["DateRegard"];
            }
            else
            {
                if (ReportSelection != null)
                {
                    #region ReportSelection

                    bool foundInvoiceSeqNr = false;
                    bool foundDateRegard = false;
                    bool foundSortOrder = false;
                    bool foundInvoiceSelection = false;
                    IEnumerable<ReportSelectionInt> reportSelectionInts = rm.GetReportSelectionInts(ReportSelection.ReportSelectionId);
                    foreach (ReportSelectionInt reportSelectionInt in reportSelectionInts)
                    {
                        switch (reportSelectionInt.ReportSelectionType)
                        {
                            case (int)SoeSelectionData.Int_Ledger_InvoiceSeqNr:
                                InvoiceSeqNr.ValueFrom = reportSelectionInt.SelectFrom.ToString();
                                InvoiceSeqNr.ValueTo = reportSelectionInt.SelectTo.ToString();
                                foundInvoiceSeqNr = true;
                                break;
                            case (int)SoeSelectionData.Int_Ledger_DateRegard:
                                DateRegard.Value = reportSelectionInt.SelectFrom.ToString();
                                foundDateRegard = true;
                                break;
                            case (int)SoeSelectionData.Int_Ledger_SortOrder:
                                SortOrder.Value = reportSelectionInt.SelectFrom.ToString();
                                foundSortOrder = true;
                                break;
                            case (int)SoeSelectionData.Int_Ledger_InvoiceSelection:
                                InvoiceSelection.Value = reportSelectionInt.SelectFrom.ToString();
                                foundInvoiceSelection = true;
                                break;
                        }

                        if (foundInvoiceSeqNr && foundDateRegard && foundSortOrder && foundInvoiceSelection)
                            break;
                    }

                    bool foundActorNr = false;
                    IEnumerable<ReportSelectionStr> reportSelectionStrs = rm.GetReportSelectionStrs(ReportSelection.ReportSelectionId);
                    foreach (ReportSelectionStr reportSelectionStr in reportSelectionStrs)
                    {
                        switch (reportSelectionStr.ReportSelectionType)
                        {
                            case (int)SoeSelectionData.Str_Ledger_ActorNr:
                                if (IsSupplierLedger())
                                {
                                    SupplierNr.ValueFrom = reportSelectionStr.SelectFrom;
                                    SupplierNr.ValueTo = reportSelectionStr.SelectTo;
                                    foundActorNr = true;
                                }
                                else if (IsCustomerLedger())
                                {
                                    CustomerNr.ValueFrom = reportSelectionStr.SelectFrom;
                                    CustomerNr.ValueTo = reportSelectionStr.SelectTo;
                                    foundActorNr = true;
                                }
                                break;
                        }

                        if (foundActorNr)
                            break;
                    }

                    #endregion
                }
            }

            #endregion
        }

        public bool Evaluate(SelectionLedger s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return false;

            #region Init

            if (F == null)
                return false;

            if (rm == null)
                rm = new ReportManager(PageBase.ParameterObject);

            #endregion

            #region Validate input and read interval into SelectionLedger

            #region Read from Form

            string dateFrom = F["Date-from-1"];
            string dateTo = F["Date-to-1"];
            string supplierNrFrom = F["SupplierNr-from-1"];
            string supplierNrTo = F["SupplierNr-to-1"];
            string customerNrFrom = F["CustomerNr-from-1"];
            string customerNrTo = F["CustomerNr-to-1"];
            string invoiceSeqNrFrom = F["InvoiceSeqNr-from-1"];
            string invoiceSeqNrTo = F["InvoiceSeqNr-to-1"];
            string actorNrFrom = "";
            string actorNrTo = "";            
            if (IsSupplierLedger())
            {
                actorNrFrom = supplierNrFrom;
                actorNrTo = supplierNrTo;
            }
            else if (IsCustomerLedger())
            {
                actorNrFrom = customerNrFrom;
                actorNrTo = customerNrTo;
            }

            #endregion

            #region Validate interval

            bool actorValid = Validator.ValidateSelectInterval(actorNrFrom, actorNrTo);
            bool invoiceSeqNrValid = Validator.ValidateSelectInterval(invoiceSeqNrFrom, invoiceSeqNrTo);
            bool dateValid = Validator.ValidateTextInterval(dateFrom, dateFrom);
            if (!actorValid || !invoiceSeqNrValid || !dateValid)
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                return false;
            }

            #endregion

            #region InvoiceSeqNr

            //From 
            int from;
            if (Int32.TryParse(invoiceSeqNrFrom, out from))
            {
                s.InvoiceSeqNrFrom = from;
            }

            //To
            int to;
            if (Int32.TryParse(invoiceSeqNrTo, out to))
            {
                s.InvoiceSeqNrTo = to;
            }

            //Validate
            invoiceSeqNrValid = Validator.ValidateNumericInterval(s.InvoiceSeqNrFrom, s.InvoiceSeqNrTo);
            if (!invoiceSeqNrValid)
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1448, "Till får inte vara mindre än Från i intervall");
                return false;
            }

            #endregion

            #region Date

            //Fix to allow only from or to date's
            if (String.IsNullOrEmpty(dateFrom))
                dateFrom = CalendarUtility.DATETIME_MINVALUE.ToString();
            if (String.IsNullOrEmpty(dateTo))
                dateTo = CalendarUtility.DATETIME_MAXVALUE.ToString();

            //From
            if (!String.IsNullOrEmpty(dateFrom))
            {
                try
                {
                    s.DateFrom = Convert.ToDateTime(dateFrom);
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                    s.DateFrom = null;
                }

                if (s.DateFrom.HasValue)
                {
                    if (!CalendarUtility.IsDateTimeSqlServerValid(s.DateFrom.Value))
                    {
                        SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1679, "Felaktigt datum");
                        return false;
                    }
                }
            }

            //To
            if (!String.IsNullOrEmpty(dateTo))
            {
                try
                {
                    s.DateTo = Convert.ToDateTime(dateTo);
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                    s.DateTo = null;
                }

                if (s.DateTo.HasValue)
                {
                    if (!CalendarUtility.IsDateTimeSqlServerValid(s.DateTo.Value))
                    {
                        SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1679, "Felaktigt datum");
                        return false;
                    }
                }
            }

            //Validate
            dateValid = Validator.ValidateDateInterval(s.DateFrom, s.DateTo);
            if (!dateValid)
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " +
                                         PageBase.GetText(1448, "Till får inte vara mindre än Från i intervall") + "." +
                                         PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                return false;
            }

            #endregion

            #region Misc

            s.ActorNrFrom = actorNrFrom;
            s.ActorNrTo = actorNrTo;
            s.InvoiceSelection = Convert.ToInt32(F["InvoiceSelection"]);
            s.DateRegard = Convert.ToInt32(F["DateRegard"]);
            s.SortOrder = Convert.ToInt32(F["SortOrder"]);
            s.ShowVoucher = StringUtility.GetBool(F["ShowVoucher"]);
            s.ShowPendingPaymentsInReport = StringUtility.GetBool(F["ShowPendingPaymentsInReport"]);
            s.ShowPreliminaryInvoices = StringUtility.GetBool(F["ShowPreliminaryInvoices"]);
            s.IncludeCashSalesInvoices = StringUtility.GetBool(F["IncludeCashSalesInvoices"]);
            #endregion

            #endregion

            #region Set EvaluatedSelection from SelectionLedger

            SetEvaluated(s, es);

            #endregion

            return true;
        }

        public void SetEvaluated(SelectionLedger s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return;

            if (s.DateFrom.HasValue && s.DateTo.HasValue)
            {
                es.DateFrom = s.DateFrom.Value;
                es.DateTo = s.DateTo.Value;
                es.HasDateInterval = true;
            }

            if (!String.IsNullOrEmpty(s.ActorNrFrom) && (!String.IsNullOrEmpty(s.ActorNrTo)))
            {
                es.SL_ActorNrFrom = s.ActorNrFrom;
                es.SL_ActorNrTo = s.ActorNrTo;
                es.SL_HasActorNrInterval = true;
            }

            if (s.InvoiceSeqNrFrom.HasValue && s.InvoiceSeqNrTo.HasValue)
            {
                es.SL_InvoiceSeqNrFrom = s.InvoiceSeqNrFrom.Value;
                es.SL_InvoiceSeqNrTo = s.InvoiceSeqNrTo.Value;
                es.SL_HasInvoiceSeqNrInterval = true;
            }

            es.SL_InvoiceSelection = s.InvoiceSelection;
            es.SL_DateRegard = s.DateRegard;
            es.SL_SortOrder = s.SortOrder;
            es.SL_ShowVoucher = s.ShowVoucher;
            es.SL_ShowPendingPaymentsInReport = s.ShowPendingPaymentsInReport;
            es.SL_ShowPreliminaryInvoices = s.ShowPreliminaryInvoices;
            es.SL_IncludeCashSalesInvoices = s.IncludeCashSalesInvoices;

            //Set as evaluated
            es.SL_IsEvaluated = true;
        }

        public void SetDefaultValues(SelectionLedger s)
        {
            //Default values
            s.ShowVoucher = false;
            s.SortOrder = (int)SoeReportSortOrder.ActorName;
            s.DateRegard = (int)TermGroup_ReportLedgerDateRegard.InvoiceDate;
            s.InvoiceSelection = (int)TermGroup_ReportLedgerInvoiceSelection.All;
        }

        public void SetDefaultValues(EvaluatedSelection es)
        {
            //Default values
            es.SL_ShowVoucher = false;
            es.SL_SortOrder = (int)SoeReportSortOrder.ActorName;
            es.SL_DateRegard = (int)TermGroup_ReportLedgerDateRegard.InvoiceDate;
            es.SL_InvoiceSelection = (int)TermGroup_ReportLedgerInvoiceSelection.All;
        }

        #region Help-methods

        private void SetupEnvironment()
        {
            #region SelectionType

            if (IsSupplierLedger())
            {
                //Hide fields
                CustomerNr.Visible = false;

                //Set lables
                SelectionTitle = PageBase.GetText(1808, "Urval leverantörsreskontra");
            }
            else if (IsCustomerLedger())
            {
                //Hide fields
                SupplierNr.Visible = false;

                //Set labels
                SelectionTitle = PageBase.GetText(1992, "Urval kundreskontra");
            }

            #endregion

            #region ReportTemplateType

            if (IsBalanceList())
            {
                //Hide fields
                ShowPreliminaryInvoices.Visible = false;
                IncludeCashSalesInvoices.Visible = false;
            }
            else if (IsInvoiceJournal())
            {
                //Hide fields
                ShowVoucher.Visible = false;
                ShowPendingPaymentsInReport.Visible = false;
            }
            else if (IsPaymentJournal())
            {
                //Hide fields
                ShowVoucher.Visible = false;
                InvoiceSeqNr.Visible = false;
                ShowPreliminaryInvoices.Visible = false;
                ShowPendingPaymentsInReport.Visible = false;
            }
            else
            {
                ShowPreliminaryInvoices.Visible = false;
                ShowPendingPaymentsInReport.Visible = false;
            }

            #endregion
        }

        private bool IsSupplierLedger()
        {
            return SelectionType == SoeSelectionType.Ledger_Supplier ||
                   SelectionType == SoeSelectionType.Ledger_Supplier_ExcludeAllButNr;
        }

        private bool IsCustomerLedger()
        {
            return SelectionType == SoeSelectionType.Ledger_Customer ||
                   SelectionType == SoeSelectionType.Ledger_Customer_ExcludeAllButNr;
        }

        private bool IsBalanceList()
        {
            return ReportTemplateType == SoeReportTemplateType.CustomerBalanceList ||
                   ReportTemplateType == SoeReportTemplateType.SupplierBalanceList;
        }

        private bool IsInvoiceJournal()
        {
            return ReportTemplateType == SoeReportTemplateType.CustomerInvoiceJournal ||
                   ReportTemplateType == SoeReportTemplateType.SupplierInvoiceJournal;
        }

        private bool IsPaymentJournal()
        {
            return ReportTemplateType == SoeReportTemplateType.CustomerPaymentJournal ||
                   ReportTemplateType == SoeReportTemplateType.SupplierPaymentJournal;
        }

        #endregion
    }
}