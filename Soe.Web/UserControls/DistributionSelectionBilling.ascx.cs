using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class DistributionSelectionBilling : ControlBase
    {
        #region Variables

        public NameValueCollection F { get; set; }
        public ReportSelection ReportSelection { get; set; }
        protected string title;

        private ReportManager rm;
        private CategoryManager cm;
        private StockManager sm;

        #endregion

        public void Populate(bool repopulate, SoeReportTemplateType reportTemplateType)
        {
            #region Init

            rm = new ReportManager(PageBase.ParameterObject);

            //Bug: 
            //PageBase.Scripts is null in method Populate() 
            //in a UserControl that lives in a Page that is navigated to from Server.Transfer
            //The script should be added from the calling page in this scenario
            if (PageBase.Scripts != null)
            {
                PageBase.Scripts.Add("/UserControls/DistributionSelectionBilling.js");
            }

            #endregion

            #region Populate

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.ExpenseReport:
                case SoeReportTemplateType.BillingOrderOverview:
                    ShowGuiForOrder();
                    break;
                case SoeReportTemplateType.BillingOffer:
                    ShowGuiForOffer();
                    break;
                case SoeReportTemplateType.BillingContract:
                    ShowGuiForContract();
                    break;
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                    ShowGuiForInvoice();
                    break;
                case SoeReportTemplateType.ProjectStatisticsReport:
                case SoeReportTemplateType.ProjectTimeReport:
                    ShowGuiForProjectStatistics();
                    break;
                case SoeReportTemplateType.TimeProjectReport:
                case SoeReportTemplateType.OriginStatisticsReport:
                    ShowGuiForOriginStatistics();
                    break;
                case SoeReportTemplateType.BillingStatisticsReport:
                    ShowGuiForBillingStatistics();
                    break;
                case SoeReportTemplateType.StockSaldoListReport:
                case SoeReportTemplateType.StockTransactionListReport:
                    ShowGuiForStock();
                    break;
                case SoeReportTemplateType.StockInventoryReport:
                    ShowGuiForStockInventory();
                    break;
                case SoeReportTemplateType.ProductListReport:
                    ShowGuiForProductList();
                    break;

                default:
                    ShowGuiForInvoice();
                    break;
            }

            SortOrder.DataTextField = "value";
            SortOrder.DataValueField = "key";
            SortOrder.DataBind();

            CustomerGroup.DataTextField = "value";
            CustomerGroup.DataValueField = "key";
            CustomerGroup.DataBind();

            StockInventory.DataTextField = "value";
            StockInventory.DataValueField = "key";
            StockInventory.DataBind();

            #endregion

            #region Set data

            if (repopulate && SoeForm != null && SoeForm.PreviousForm != null)
            {
                ShowNotPrinted.Value = SoeForm.PreviousForm["ShowNotPrinted"];
                ShowCopy.Value = SoeForm.PreviousForm["ShowCopy"];
                CustomerNr.PreviousForm = SoeForm.PreviousForm;
                InvoiceNr.PreviousForm = SoeForm.PreviousForm;
                CustomerNr.Value = SoeForm.PreviousForm["CustomerNr"];
                InvoiceNr.Value = SoeForm.PreviousForm["InvoiceNr"];
                SortOrder.Value = SoeForm.PreviousForm["SortOrder"];
                Date.PreviousForm = SoeForm.PreviousForm;
                ProjectNr.PreviousForm = SoeForm.PreviousForm;
                EmployeeNr.PreviousForm = SoeForm.PreviousForm;
                ProjectNr.Value = SoeForm.PreviousForm["ProjectNr"];
                EmployeeNr.Value = SoeForm.PreviousForm["EmployeeNr"];
                CustomerGroup.Value = SoeForm.PreviousForm["CustomerGroup"];
                ProductNr.PreviousForm = SoeForm.PreviousForm;
                ProductNr.Value = SoeForm.PreviousForm["ProductNr"];
                Period.PreviousForm = SoeForm.PreviousForm;
                Period.Value = SoeForm.PreviousForm["Period"];
            }
            else
            {
                //default values
                ShowNotPrinted.Value = Boolean.TrueString;
                ShowCopy.Value = Boolean.FalseString;

                if (ReportSelection != null)
                {
                    #region ReportSelection

                    bool foundSortOrder = false;
                    IEnumerable<ReportSelectionInt> reportSelectionInts = rm.GetReportSelectionInts(ReportSelection.ReportSelectionId);
                    foreach (ReportSelectionInt reportSelectionInt in reportSelectionInts)
                    {
                        switch (reportSelectionInt.ReportSelectionType)
                        {
                            case (int)SoeSelectionData.Int_Billing_SortOrder:
                                SortOrder.Value = reportSelectionInt.SelectFrom.ToString();
                                foundSortOrder = true;
                                break;

                        }

                        if (foundSortOrder)
                            break;
                    }

                    bool foundCustomerNr = false;
                    bool foundInvoiceNr = false;
                    bool foundProjectNr = false;
                    bool foundEmployeeNr = false;
                    IEnumerable<ReportSelectionStr> reportSelectionStrs = rm.GetReportSelectionStrs(ReportSelection.ReportSelectionId);
                    foreach (ReportSelectionStr reportSelectionStr in reportSelectionStrs)
                    {
                        switch (reportSelectionStr.ReportSelectionType)
                        {
                            case (int)SoeSelectionData.Str_Billing_CustomerNr:
                                CustomerNr.ValueFrom = reportSelectionStr.SelectFrom;
                                CustomerNr.ValueTo = reportSelectionStr.SelectTo;
                                foundCustomerNr = true;
                                break;
                            case (int)SoeSelectionData.Str_Billing_InvoiceNr:
                                InvoiceNr.ValueFrom = reportSelectionStr.SelectFrom;
                                InvoiceNr.ValueTo = reportSelectionStr.SelectTo;
                                foundInvoiceNr = true;
                                break;
                            case (int)SoeSelectionData.Str_Billing_ProjectNr:
                                ProjectNr.ValueFrom = reportSelectionStr.SelectFrom;
                                ProjectNr.ValueTo = reportSelectionStr.SelectTo;
                                foundProjectNr = true;
                                break;
                            case (int)SoeSelectionData.Str_Billing_EmployeeNr:
                                EmployeeNr.ValueFrom = reportSelectionStr.SelectFrom;
                                EmployeeNr.ValueTo = reportSelectionStr.SelectTo;
                                foundEmployeeNr = true;
                                break;
                            case (int)SoeSelectionData.Str_Billing_ProductNr:
                                ProductNr.ValueFrom = reportSelectionStr.SelectFrom;
                                ProductNr.ValueTo = reportSelectionStr.SelectTo;
                                break;
                            case (int)SoeSelectionData.Str_Billing_Period:
                                Period.ValueFrom = reportSelectionStr.SelectFrom;
                                Period.ValueTo = reportSelectionStr.SelectTo;
                                break;
                        }

                        if (reportTemplateType == SoeReportTemplateType.ProjectStatisticsReport)
                        {
                            if (foundEmployeeNr && foundCustomerNr && foundProjectNr)
                                break;
                        }
                        else
                        {
                            if (foundCustomerNr && foundInvoiceNr)
                                break;
                        }
                    }

                    #endregion
                }
            }

            #endregion
        }

        public bool Evaluate(SelectionBilling s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return false;

            #region Init

            if (F == null)
                return false;

            if (rm == null)
                rm = new ReportManager(PageBase.ParameterObject);

            #endregion

            #region Validate input and read interval into SelectionBilling

            #region Read from Form

            string dateFrom = F["Date-from-1"];
            string dateTo = F["Date-to-1"];
            bool showNotPrinted = StringUtility.GetBool(F["ShowNotPrinted"]);
            bool showCopy = StringUtility.GetBool(F["ShowCopy"]);
            bool includeClosedOrder = StringUtility.GetBool(F["IncludeClosedOrder"]);
            string customerNrFrom = F["CustomerNr-from-1"];
            string customerNrTo = F["CustomerNr-to-1"];
            string invoiceNrFrom = F["InvoiceNr-from-1"];
            string invoiceNrTo = F["InvoiceNr-to-1"];
            string projectNrFrom = F["ProjectNr-from-1"];
            string projectNrTo = F["ProjectNr-to-1"];
            string employeeNrFrom = F["EmployeeNr-from-1"];
            string employeeNrTo = F["EmployeeNr-to-1"];
            string productNrFrom = F["ProductNr-from-1"];
            string productNrTo = F["ProductNr-to-1"];
            string paymentDateFrom = F["PaymentDate-from-1"];
            string paymentDateTo = F["PaymentDate-to-1"];
            bool invoiceCopyAsOriginal = StringUtility.GetBool(F["InvoiceCopyAsOriginal"]);
            int? budgetId = Convert.ToInt32(F["Budget"]);
            string periodFrom = F["Period-from-1"];
            string periodTo = F["Period-to-1"];

            #endregion

            #region Validate interval

            bool customerNrValid = Validator.ValidateSelectInterval(customerNrFrom, customerNrTo);
            bool invoiceNrValid = Validator.ValidateSelectInterval(invoiceNrFrom, invoiceNrTo);
            bool projectNrValid = Validator.ValidateSelectInterval(projectNrFrom, projectNrTo);
            bool employeeNrValid = Validator.ValidateSelectInterval(employeeNrFrom, employeeNrTo);
            bool productNrValid = Validator.ValidateSelectInterval(productNrFrom, productNrTo);
            bool periodValid = Validator.ValidateSelectInterval(periodFrom, periodTo);
            bool dateValid = Validator.ValidateTextInterval(dateFrom, dateFrom);

            if (!customerNrValid || !invoiceNrValid || !employeeNrValid || !projectNrValid || !productNrValid || !dateValid || !periodValid)
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                return false;
            }

            #endregion

            #region CustomerNr

            s.CustomerNrFrom = customerNrFrom;
            s.CustomerNrTo = customerNrTo;

            #endregion

            #region InvoiceNr

            s.InvoiceNrFrom = invoiceNrFrom;
            s.InvoiceNrTo = invoiceNrTo;

            #endregion

            #region ProjectNr

            s.ProjectNrFrom = projectNrFrom;
            s.ProjectNrTo = projectNrTo;

            #endregion

            #region Period

            s.PeriodFrom = periodFrom;
            s.PeriodTo = periodTo;

            #endregion

            #region EmployeeNr

            s.EmployeeNrFrom = employeeNrFrom;
            s.EmployeeNrTo = employeeNrTo;

            #endregion

            #region ProductNr

            s.ProductNrFrom = productNrFrom;
            s.ProductNrTo = productNrTo;

            #endregion

            s.StockLocationIdFrom = Convert.ToInt32(F["StockLocation-from-1"]);
            s.StockLocationIdTo = Convert.ToInt32(F["StockLocation-to-1"]);
            s.StockShelfIdFrom = Convert.ToInt32(F["StockShelf-from-1"]);
            s.StockShelfIdTo = Convert.ToInt32(F["StockShelf-to-1"]);
            s.StockInventoryId = Convert.ToInt32(F["StockInventory"]);

            #region Date


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

            if (s.DateFrom.HasValue && !s.DateTo.HasValue)
            {
                s.DateTo = CalendarUtility.DATETIME_MAXVALUE;
            }
            else if (s.DateTo.HasValue && !s.DateFrom.HasValue)
            {
                s.DateFrom = CalendarUtility.DATETIME_MINVALUE;
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

            //From
            if (!String.IsNullOrEmpty(paymentDateFrom))
            {
                try
                {
                    s.PaymentDateFrom = Convert.ToDateTime(paymentDateFrom);
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                    s.PaymentDateFrom = null;
                }

                if (s.PaymentDateFrom.HasValue)
                {
                    if (!CalendarUtility.IsDateTimeSqlServerValid(s.PaymentDateFrom.Value))
                    {
                        SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1679, "Felaktigt datum");
                        return false;
                    }
                }
            }

            //To
            if (!String.IsNullOrEmpty(paymentDateTo))
            {
                try
                {
                    s.PaymentDateTo = Convert.ToDateTime(paymentDateTo);
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                    s.PaymentDateTo = null;
                }

                if (s.DateTo.HasValue)
                {
                    if (!CalendarUtility.IsDateTimeSqlServerValid(s.PaymentDateTo.Value))
                    {
                        SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1679, "Felaktigt datum");
                        return false;
                    }
                }
            }

            if (s.PaymentDateFrom.HasValue && !s.PaymentDateTo.HasValue)
            {
                s.PaymentDateTo = CalendarUtility.DATETIME_MAXVALUE;
            }
            else if (s.PaymentDateTo.HasValue && !s.PaymentDateFrom.HasValue)
            {
                s.PaymentDateFrom = CalendarUtility.DATETIME_MINVALUE;
            }
            //Validate
            dateValid = Validator.ValidateDateInterval(s.PaymentDateFrom, s.PaymentDateTo);
            if (!dateValid)
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " +
                                         PageBase.GetText(1448, "Till får inte vara mindre än Från i intervall") + "." +
                                         PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                return false;
            }

            #endregion

            #region Misc

            s.InvoiceCopy = !invoiceCopyAsOriginal;
            s.InvoiceReminder = false;
            s.ShowNotPrinted = showNotPrinted;
            s.ShowCopy = showCopy;
            s.IncludeClosedOrder = includeClosedOrder;
            s.SortOrder = Convert.ToInt32(F["SortOrder"]);
            s.CustomerGroupId = Convert.ToInt32(F["CustomerGroup"]);
            s.IncludeProjectReport2 = true;


            #endregion

            #endregion

            #region Set EvaluatedSelection from SelectionBilling

            SetEvaluated(s, es);

            #endregion

            return true;
        }

        public void SetEvaluated(SelectionBilling s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return;

            if (s.DateFrom.HasValue && s.DateTo.HasValue)
            {
                es.DateFrom = s.DateFrom.Value;
                es.DateTo = s.DateTo.Value;
                es.HasDateInterval = true;
            }

            if (s.PaymentDateFrom.HasValue && s.PaymentDateTo.HasValue)
            {
                es.SB_PaymentDateFrom = s.PaymentDateFrom.Value;
                es.SB_PaymentDateTo = s.PaymentDateTo.Value;
                es.SB_HasPaymentDateInterval = true;
            }

            if ((!String.IsNullOrEmpty(s.CustomerNrFrom)) && (!String.IsNullOrEmpty(s.CustomerNrTo)))
            {
                es.SB_CustomerNrFrom = s.CustomerNrFrom;
                es.SB_CustomerNrTo = s.CustomerNrTo;
                es.SB_HasCustomerNrInterval = true;
            }

            if ((!String.IsNullOrEmpty(s.InvoiceNrFrom)) && (!String.IsNullOrEmpty(s.InvoiceNrTo)))
            {
                es.SB_InvoiceNrFrom = s.InvoiceNrFrom;
                es.SB_InvoiceNrTo = s.InvoiceNrTo;
                es.SB_HasInvoiceNrInterval = true;
            }

            if ((!String.IsNullOrEmpty(s.ProjectNrFrom)) && (!String.IsNullOrEmpty(s.ProjectNrTo)))
            {
                es.SB_ProjectNrFrom = s.ProjectNrFrom;
                es.SB_ProjectNrTo = s.ProjectNrTo;
                es.SB_HasProjectNrInterval = true;
            }

            if ((!String.IsNullOrEmpty(s.EmployeeNrFrom) && (!String.IsNullOrEmpty(s.EmployeeNrTo))))
            {
                es.SB_EmployeeNrFrom = s.EmployeeNrFrom;
                es.SB_EmployeeNrTo = s.EmployeeNrTo;
                es.SB_HasEmployeeNrInterval = true;
            }

            if ((!String.IsNullOrEmpty(s.ProductNrFrom) && (!String.IsNullOrEmpty(s.ProductNrTo))))
            {
                es.SB_ProductNrFrom = s.ProductNrFrom;
                es.SB_ProductNrTo = s.ProductNrTo;
            }

            if ((!String.IsNullOrEmpty(s.PeriodFrom) || (!String.IsNullOrEmpty(s.PeriodTo))))
            {
                es.SB_PeriodFrom = s.PeriodFrom;
                es.SB_PeriodTo = s.PeriodTo;
            }

            es.SB_ShowNotPrinted = s.ShowNotPrinted;
            es.SB_ShowCopy = s.ShowCopy;
            es.SB_InvoiceCopy = s.InvoiceCopy;
            es.SB_InvoiceReminder = s.InvoiceReminder;
            es.SB_SortOrder = s.SortOrder;
            es.SB_CustomerGroupId = (int)s.CustomerGroupId;
            es.SB_IncludeProjectReport2 = s.IncludeProjectReport2;
            es.SB_IncludeClosedOrder = s.IncludeClosedOrder;
            es.SB_StockLocationIdFrom = s.StockLocationIdFrom;
            es.SB_StockLocationIdTo = s.StockLocationIdTo;
            es.SB_StockShelfIdFrom = s.StockShelfIdFrom;
            es.SB_StockShelfIdTo = s.StockShelfIdTo;
            es.SB_StockInventoryId = s.StockInventoryId;
            //Set as evaluated
            es.SB_IsEvaluated = true;
        }

        public void SetDefaultValues(SelectionBilling s)
        {
            //Default values
            s.SortOrder = (int)TermGroup_ReportBillingInvoiceSortOrder.InvoiceNr;
        }

        public void SetDefaultValues(EvaluatedSelection es)
        {
            //Default values
            es.SB_SortOrder = (int)TermGroup_ReportBillingInvoiceSortOrder.InvoiceNr;
        }

        #region GUI methods

        public void ShowGuiForInvoice()
        {
            SortOrder.DataSource = PageBase.GetGrpText((TermGroup.ReportBillingInvoiceSortOrder));
            cm = new CategoryManager(PageBase.ParameterObject);
            CustomerGroup.DataSource = cm.GetCategoriesDict(SoeCategoryType.Customer, PageBase.SoeCompany.ActorCompanyId, true);
            title = PageBase.GetText(1908, "Fakturaurval");

            //Show/Hide
            SetCheckboxesVisible(true);
            CustomerNr.DisableHeader = false;
            ProjectNr.Visible = false;
            EmployeeNr.Visible = false;
            IncludeClosedOrder.Visible = false;
            ProductNr.Visible = false;
            StockLocation.Visible = false;
            StockShelf.Visible = false;
            StockInventory.Visible = false;
            PaymentDate.Visible = false;
            Period.Visible = false;


            //Date intervall needed task 8882
            //Date.Visible = false;
        }

        public void ShowGuiForOrder()
        {
            SortOrder.DataSource = PageBase.GetGrpText(TermGroup.ReportBillingOrderSortOrder);
            cm = new CategoryManager(PageBase.ParameterObject);
            CustomerGroup.DataSource = cm.GetCategoriesDict(SoeCategoryType.Customer, PageBase.SoeCompany.ActorCompanyId, true);
            title = PageBase.GetText(8015, "Orderurval");
            InvoiceNr.TermID = 8013;
            InvoiceNr.DefaultTerm = "Ordernr";

            //Show/Hide
            SetCheckboxesVisible(false);
            CustomerNr.DisableHeader = false;
            ProjectNr.Visible = false;
            EmployeeNr.Visible = false;
            Date.Visible = true;
            IncludeClosedOrder.Visible = true;
            ProductNr.Visible = false;
            StockLocation.Visible = false;
            StockShelf.Visible = false;
            StockInventory.Visible = false;
            Period.Visible = false;
        }

        public void ShowGuiForOffer()
        {
            SortOrder.DataSource = PageBase.GetGrpText(TermGroup.ReportBillingOfferSortOrder);
            title = PageBase.GetText(8016, "Offerturval");
            InvoiceNr.TermID = 8014;
            InvoiceNr.DefaultTerm = "Offertnr";

            //Show/Hide
            CustomerGroup.Visible = false;
            SetCheckboxesVisible(false);
            CustomerNr.DisableHeader = false;
            ProjectNr.Visible = false;
            EmployeeNr.Visible = false;
            Date.Visible = false;
            ProductNr.Visible = false;
            StockLocation.Visible = false;
            StockShelf.Visible = false;
            StockInventory.Visible = false;
            Period.Visible = false;
        }

        public void ShowGuiForContract()
        {
            SortOrder.DataSource = PageBase.GetGrpText(TermGroup.ReportBillingContractSortOrder);
            title = PageBase.GetText(3461, "Avtalsurval");
            InvoiceNr.TermID = 3462;
            InvoiceNr.DefaultTerm = "Avtalsnr";

            //Show/Hide
            SetCheckboxesVisible(false);
            CustomerGroup.Visible = false;
            CustomerNr.DisableHeader = false;
            ProjectNr.Visible = false;
            EmployeeNr.Visible = false;
            Date.Visible = false;
            ProductNr.Visible = false;
            StockLocation.Visible = false;
            StockShelf.Visible = false;
            StockInventory.Visible = false;
            Period.Visible = false;

        }

        public void ShowGuiForProjectStatistics()
        {
            title = PageBase.GetText(9174, "Urval");
            cm = new CategoryManager(PageBase.ParameterObject);
            CustomerGroup.DataSource = cm.GetCategoriesDict(SoeCategoryType.Customer, PageBase.SoeCompany.ActorCompanyId, true);

            //Show/Hide
            SortOrder.Visible = false;
            CustomerGroup.Visible = true;
            SetCheckboxesVisible(false);
            ProjectNr.DisableHeader = false;
            InvoiceNr.Visible = false;
            Date.Visible = true;
            PaymentDate.Visible = false;
            ProductNr.Visible = false;
            StockLocation.Visible = false;
            StockShelf.Visible = false;
            StockInventory.Visible = false;
            Period.Visible = false;

        }

        public void ShowGuiForOriginStatistics()
        {
            title = PageBase.GetText(8027, "Projekturval");
            cm = new CategoryManager(PageBase.ParameterObject);
            CustomerGroup.DataSource = cm.GetCategoriesDict(SoeCategoryType.Customer, PageBase.SoeCompany.ActorCompanyId, true);

            //Show/Hide
            SortOrder.Visible = false;
            CustomerGroup.Visible = true;
            SetCheckboxesVisible(false);
            ProjectNr.DisableHeader = false;
            InvoiceNr.Visible = false;
            Date.Visible = true;
            ProductNr.Visible = false;
            StockLocation.Visible = false;
            StockShelf.Visible = false;
            StockInventory.Visible = false;
            PaymentDate.Visible = true;
            Period.Visible = false;

        }

        public void ShowGuiForBillingStatistics()
        {
            title = PageBase.GetText(8027, "Projekturval");
            cm = new CategoryManager(PageBase.ParameterObject);
            CustomerGroup.DataSource = cm.GetCategoriesDict(SoeCategoryType.Customer, PageBase.SoeCompany.ActorCompanyId, true);

            //Show/Hide
            SortOrder.Visible = false;
            CustomerGroup.Visible = true;
            SetCheckboxesVisible(false);
            ProjectNr.DisableHeader = false;
            InvoiceNr.Visible = false;
            Date.Visible = true;
            ProductNr.Visible = true;
            StockLocation.Visible = false;
            StockShelf.Visible = false;
            StockInventory.Visible = false;
            PaymentDate.Visible = false;
            Period.Visible = false;

        }
        public void ShowGuiForStock()
        {
            SortOrder.DataSource = PageBase.GetGrpText(TermGroup.ReportBillingStockSortOrder);
            sm = new StockManager(PageBase.ParameterObject);
            StockLocation.DataSourceFrom = sm.GetStocksDict(PageBase.SoeCompany.ActorCompanyId, true);
            StockLocation.DataSourceTo = sm.GetStocksDict(PageBase.SoeCompany.ActorCompanyId, true);
            StockShelf.DataSourceFrom = sm.GetStockShelfsDict(PageBase.SoeCompany.ActorCompanyId, true);
            StockShelf.DataSourceTo = sm.GetStockShelfsDict(PageBase.SoeCompany.ActorCompanyId, true);
            title = PageBase.GetText(9296, "Lagerurval");
            //InvoiceNr.TermID = 8013;
            //InvoiceNr.DefaultTerm = "Ordernr";

            //Show/Hide
            SetCheckboxesVisible(false);
            CustomerNr.Visible = false;
            ProjectNr.Visible = false;
            EmployeeNr.Visible = false;
            InvoiceNr.Visible = false;
            CustomerGroup.Visible = false;
            Date.Visible = true;
            //PaymentDate.Visible = true;
            IncludeClosedOrder.Visible = false;
            ProductNr.Visible = true;
            StockLocation.Visible = true;
            StockShelf.Visible = true;
            StockInventory.Visible = false;
            Period.Visible = false;
        }

        public void ShowGuiForProductList()
        {
            //           title = PageBase.GetText(3461, "Avtalsurval");
            //           InvoiceNr.TermID = 3462;
            //           InvoiceNr.DefaultTerm = "Avtalsnr";

            //Show/Hide
            SortOrder.Visible = false;
            SetCheckboxesVisible(false);
            CustomerGroup.Visible = false;
            CustomerNr.DisableHeader = false;
            CustomerNr.Visible = false;
            InvoiceNr.Visible = false;
            ProjectNr.Visible = false;
            EmployeeNr.Visible = false;
            Date.Visible = false;
            ProductNr.Visible = true;
            StockLocation.Visible = false;
            StockShelf.Visible = false;
            StockInventory.Visible = false;
            Period.Visible = false;
        }

        public void ShowGuiForStockInventory()
        {
            SortOrder.DataSource = PageBase.GetGrpText(TermGroup.ReportBillingStockSortOrder);
            sm = new StockManager(PageBase.ParameterObject);
            StockLocation.DataSourceFrom = sm.GetStocksDict(PageBase.SoeCompany.ActorCompanyId, true);
            StockLocation.DataSourceTo = sm.GetStocksDict(PageBase.SoeCompany.ActorCompanyId, true);
            StockShelf.DataSourceFrom = sm.GetStockShelfsDict(PageBase.SoeCompany.ActorCompanyId, true);
            StockShelf.DataSourceTo = sm.GetStockShelfsDict(PageBase.SoeCompany.ActorCompanyId, true);
            StockInventory.DataSource = sm.GetStockInventoriesDict(PageBase.SoeCompany.ActorCompanyId, true);
            title = PageBase.GetText(9296, "Lagerurval");
            //InvoiceNr.TermID = 8013;
            //InvoiceNr.DefaultTerm = "Ordernr";

            //Show/Hide
            SetCheckboxesVisible(false);
            CustomerNr.Visible = false;
            ProjectNr.Visible = false;
            EmployeeNr.Visible = false;
            InvoiceNr.Visible = false;
            CustomerGroup.Visible = false;
            Date.Visible = false;
            IncludeClosedOrder.Visible = false;
            ProductNr.Visible = true;
            StockLocation.Visible = true;
            StockShelf.Visible = true;
            StockInventory.Visible = true;
            Period.Visible = false;
        }

        public void SetCheckboxesVisible(bool option)
        {
            ShowNotPrinted.Visible = option;
            ShowCopy.Visible = option;
            InvoiceCopyAsOriginal.Visible = option;
            IncludeClosedOrder.Visible = option;
        }

        #endregion
    }
}
