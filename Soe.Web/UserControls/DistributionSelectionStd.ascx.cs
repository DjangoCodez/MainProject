using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class DistributionSelectionStd : ControlBase
    {
        #region Variables

        public NameValueCollection F { get; set; }
        public ReportSelection ReportSelection { get; set; }
        public bool DisableDateSelection { get; set; }
        public bool AdjustForOnlyFromYearInterval { get; set; }
        public bool AdjustForOnlyFromPeriodInterval { get; set; }
        public bool ShowOnlyOpenAccountYears { get; set; }

        private ReportManager rm;
        private AccountManager am;

        #endregion

        public void Populate(bool repopulate)
        {
            Populate(repopulate, 0);
        }

        public void Populate(bool repopulate, SoeReportTemplateType reportTemplateType, bool showBudget = false)
        {
            #region Init

            rm = new ReportManager(PageBase.ParameterObject);
            am = new AccountManager(PageBase.ParameterObject);

            //Bug: 
            //PageBase.Scripts is null in method Populate() 
            //in a UserControl that lives in a Page that is navigated to from Server.Transfer
            //The script should be added from the calling page in this scenario
            if (PageBase.Scripts != null)
            {
                PageBase.Scripts.Add("/UserControls/DistributionSelectionStd.js");
            }

            if (AccountYear.OnlyFrom || AccountPeriod.OnlyFrom)
            {
                DisableDateSelection = true;
            }

            if (DisableDateSelection)
            {
                DivDateSelection.Visible = false;
            }

            #endregion

            #region Populate

            Dictionary<int, string> accountYears = am.GetAccountYearsDict(PageBase.SoeCompany.ActorCompanyId, ShowOnlyOpenAccountYears, false, false, true);
            AccountYear.DataSourceFrom = accountYears;
            AccountYear.DataSourceTo = accountYears;

            Dictionary<int, string> budgetHeadsdict = new Dictionary<int, string>();

            BudgetManager bm = new BudgetManager(null);
            List<BudgetHead> budgetHeads = bm.GetBudgetHeads(PageBase.SoeCompany.ActorCompanyId, (int)DistributionCodeBudgetType.AccountingBudget);
            foreach (BudgetHead budgetHead in budgetHeads)
            {
                if (!budgetHeadsdict.ContainsKey(budgetHead.BudgetHeadId))
                    budgetHeadsdict.Add(budgetHead.BudgetHeadId, budgetHead.Name);
            }

            Budget.ConnectDataSource(budgetHeadsdict);



            //Special for only from selection for AccountYear and AccountPeriod
            if (AdjustForOnlyFromYearInterval)
            {
                AccountYear.DisableHeader = true;
                AccountYear.OnlyFrom = true;
            }

            if (AdjustForOnlyFromPeriodInterval)
            {
                AccountPeriod.TermID = 1491;
                AccountPeriod.DefaultTerm = "Redov. till Period";
                AccountPeriod.OnlyFrom = true;
            }

            //Special for TaxAudit
            if (reportTemplateType == SoeReportTemplateType.TaxAudit)
            {
                CreateVatVoucherFlag.Value = "1";
                CreateVatVoucher.Visible = true;
            }

            if ((reportTemplateType != SoeReportTemplateType.ResultReport && reportTemplateType != SoeReportTemplateType.ResultReportV2) || !showBudget)
            {
                ShowBudget.Visible = false;
                Budget.Visible = false;
            }

            if (reportTemplateType == SoeReportTemplateType.ResultReport || reportTemplateType == SoeReportTemplateType.ResultReportV2 ||
                reportTemplateType == SoeReportTemplateType.BalanceReport)
            {
                IncludeExternalVoucherSeries.Visible = true;
                IncludeExternalVoucherSeries.Value = true.ToString();
                IncludeYearEndVoucherSeries.Visible = true;
                IncludeYearEndVoucherSeries.Value = true.ToString();

                var accountingDims = am.GetAccountDimsByCompanyDict(PageBase.SoeCompany.ActorCompanyId, true, false, true);
                AccountDim.ConnectDataSource(accountingDims);
                AccountDim.Visible = true;

                IncludeMissingAccountDim.Visible = true;
                SeparateAccountDim.Visible = true;
            }

            if (reportTemplateType == SoeReportTemplateType.GeneralLedger)
            {
                ProjectReport.Visible = true;
                SeparateAccountDim.Visible = true;
            }

            #endregion

            #region Set data

            if (repopulate && SoeForm != null && SoeForm.PreviousForm != null)
            {
                AccountYear.PreviousForm = SoeForm.PreviousForm;
                AccountPeriod.PreviousForm = SoeForm.PreviousForm;
                Date.PreviousForm = SoeForm.PreviousForm;

                string accountYearIdFrom = SoeForm.PreviousForm["AccountYear-from-1"];
                if (!String.IsNullOrEmpty(accountYearIdFrom))
                {
                    AccountYear getAccountYear = am.GetAccountYear(Convert.ToInt32(accountYearIdFrom));
                    AccountPeriod.DataSourceFrom = am.GetAccountPeriodsInDateIntervalDict(Convert.ToInt32(accountYearIdFrom), getAccountYear.From, getAccountYear.To);
                }

                string accountYearIdTo = SoeForm.PreviousForm["AccountYear-to-1"];
                if (!String.IsNullOrEmpty(accountYearIdTo))
                {
                    AccountYear getAccountYear = am.GetAccountYear(Convert.ToInt32(accountYearIdTo));
                    AccountPeriod.DataSourceTo = am.GetAccountPeriodsInDateIntervalDict(Convert.ToInt32(accountYearIdTo), getAccountYear.From, getAccountYear.To);
                }
            }
            else
            {
                //Get current AccountYear
                AccountYear accountYear = null;
                if (PageBase.CurrentAccountYear != null)
                    accountYear = PageBase.CurrentAccountYear;
                else
                    accountYear = am.GetCurrentAccountYear(PageBase.SoeCompany.ActorCompanyId);

                if (accountYear != null)
                {
                    AccountYear.ValueFrom = accountYear.AccountYearId.ToString();
                    AccountYear.ValueTo = accountYear.AccountYearId.ToString();

                    AccountYear getAccountYear = am.GetAccountYear(accountYear.AccountYearId);
                    Dictionary<int, string> accountPeriods = am.GetAccountPeriodsInDateIntervalDict(accountYear.AccountYearId, getAccountYear.From, getAccountYear.To);

                    AccountPeriod.DataSourceFrom = accountPeriods;
                    AccountPeriod.DataSourceTo = accountPeriods;

                    AccountPeriod firstAccountPeriod = am.GetFirstAccountPeriodInterval(accountYear.AccountYearId, PageBase.SoeCompany.ActorCompanyId, false, getAccountYear.From, getAccountYear.To);
                    AccountPeriod currentAccountPeriod = am.GetCurrentAccountPeriod(accountYear.AccountYearId, PageBase.SoeCompany.ActorCompanyId, getAccountYear.From, getAccountYear.To);
                    if (currentAccountPeriod != null)
                    {
                        AccountPeriod.ValueFrom = firstAccountPeriod.AccountPeriodId.ToString();
                        AccountPeriod.ValueTo = currentAccountPeriod.AccountPeriodId.ToString();
                    }
                }
                if (ReportSelection != null)
                {
                    #region ReportSelection
                    bool foundBudget = false;

                    IEnumerable<ReportSelectionInt> reportSelectionInts = rm.GetReportSelectionInts(ReportSelection.ReportSelectionId);
                    foreach (ReportSelectionInt reportSelectionInt in reportSelectionInts)
                    {
                        switch (reportSelectionInt.ReportSelectionType)
                        {
                            case (int)SoeSelectionData.Int_BudgetId:
                                Budget.Value = reportSelectionInt.SelectFrom.ToString();
                                foundBudget = true;
                                break;
                        }
                        if (foundBudget)
                            break;
                    }
                    #endregion
                }
            }

            #endregion
        }

        public void GetSelectedAccountYearId(bool repopulate, out int accountYearIdFrom, out int accountYearIdTo)
        {
            int from = 0;
            int to = 0;
            if (repopulate && SoeForm != null && SoeForm.PreviousForm != null)
            {
                if (Int32.TryParse(SoeForm.PreviousForm["AccountYear-from-1"], out from))
                    accountYearIdFrom = from;
                else
                    accountYearIdFrom = 0;

                if (Int32.TryParse(SoeForm.PreviousForm["AccountYear-to-1"], out to))
                    accountYearIdTo = to;
                else
                    accountYearIdTo = 0;
            }
            else
            {
                if (Int32.TryParse(AccountYear.ValueFrom, out from))
                    accountYearIdFrom = from;
                else
                    accountYearIdFrom = 0;

                if (Int32.TryParse(AccountYear.ValueTo, out to))
                    accountYearIdTo = to;
                else
                    accountYearIdTo = 0;
            }
        }

        public bool Evaluate(SelectionStd s, EvaluatedSelection es)
        {
            #region Init

            if (F == null || s == null || es == null)
                return false;

            if (rm == null)
                rm = new ReportManager(PageBase.ParameterObject);
            if (am == null)
                am = new AccountManager(PageBase.ParameterObject);

            #endregion

            #region Validate input and read interval into SelectionStd

            #region Read from Form

            bool dateSelection = StringUtility.GetBool(F["DateSelection"]);
            string accountYearFrom = F["AccountYear-from-1"];
            string accountYearTo = F["AccountYear-to-1"];
            string accountPeriodFrom = F["AccountPeriod-from-1"];
            string accountPeriodTo = F["AccountPeriod-to-1"];
            string dateFrom = F["Date-from-1"];
            string dateTo = F["Date-to-1"];
            bool createVatVoucher = StringUtility.GetBool(F["CreateVatVoucher"]);
            int? budget = StringUtility.GetInt(F["Budget"], 0);
            string reportSelectionText = F["ReportSelectionText"];

            if (AccountYear.OnlyFrom)
                accountYearTo = accountYearFrom;
            if (AccountPeriod.OnlyFrom)
                accountPeriodTo = accountPeriodFrom;

            es.ReportSelectionText = reportSelectionText;
            s.IncludeYearEndVouchers = StringUtility.GetBool(F["IncludeYearEndVoucherSeries"]);
            s.IncludeExternalVouchers = StringUtility.GetBool(F["IncludeExternalVoucherSeries"]);
            s.ProjectReport = StringUtility.GetBool(F["ProjectReport"]);
            s.AccountDimId = StringUtility.GetInt(F["AccountDim"]);
            s.IncludeMissingAccountDim = StringUtility.GetBool(F["IncludeMissingAccountDim"]);
            s.SeparateAccountDim = StringUtility.GetBool(F["SeparateAccountDim"]);
            
            if (budget != null && budget != 0)
                s.BudgetId = budget;

            #endregion

            #region Validate interval

            #region Date

            #region Date

            if (dateSelection)
            {
                #region Validate

                //Fix to allow only from or to date's
                if (String.IsNullOrEmpty(dateFrom))
                    dateFrom = CalendarUtility.DATETIME_MINVALUE.ToString();
                if (String.IsNullOrEmpty(dateTo))
                    dateTo = CalendarUtility.DATETIME_MAXVALUE.ToString();

                if (!Validator.ValidateTextInterval(dateFrom, dateFrom))
                {
                    SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                    return false;
                }

                #endregion

                #region From

                if (!String.IsNullOrEmpty(dateFrom))
                {
                    s.DateFrom = Convert.ToDateTime(dateFrom);
                    if (s.DateFrom.HasValue)
                    {
                        if (!CalendarUtility.IsDateTimeSqlServerValid(s.DateFrom.Value))
                        {
                            SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1679, "Felaktigt datum");
                            return false;
                        }

                        //AccountYear
                        AccountYear accountYear = am.GetAccountYear(s.DateFrom.Value, PageBase.SoeCompany.ActorCompanyId);
                        if (accountYear != null)
                        {
                            s.AccountYearFromId = accountYear.AccountYearId;
                            s.AccountYearFromDate = accountYear.From;

                            //AccountPeriod
                            AccountPeriod accountPeriod = am.GetAccountPeriod(accountYear.AccountYearId, s.DateFrom.Value, PageBase.SoeCompany.ActorCompanyId);
                            if (accountPeriod != null)
                            {
                                s.AccountPeriodFromId = accountPeriod.AccountPeriodId;
                                s.AccountPeriodFromDate = accountPeriod.From;
                            }
                        }
                    }
                }

                #endregion

                #region To

                if (!String.IsNullOrEmpty(dateTo))
                {
                    s.DateTo = Convert.ToDateTime(dateTo);
                    if (s.DateTo.HasValue)
                    {
                        if (!CalendarUtility.IsDateTimeSqlServerValid(s.DateTo.Value))
                        {
                            SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1679, "Felaktigt datum");
                            return false;
                        }

                        //AccountYear
                        AccountYear accountYear = am.GetAccountYear(s.DateTo.Value, PageBase.SoeCompany.ActorCompanyId);
                        if (accountYear != null)
                        {
                            s.AccountYearToId = accountYear.AccountYearId;
                            s.AccountYearToDate = accountYear.To;

                            //AccountPeriod
                            AccountPeriod accountPeriod = am.GetAccountPeriod(accountYear.AccountYearId, s.DateTo.Value, PageBase.SoeCompany.ActorCompanyId);
                            if (accountPeriod != null)
                            {
                                s.AccountPeriodToId = accountPeriod.AccountPeriodId;
                                s.AccountPeriodToDate = accountPeriod.To;
                            }
                        }
                    }
                }

                #endregion
            }

            #endregion

            #region AccountYear and AccountPeriod

            if (!dateSelection || (!s.DateFrom.HasValue && !s.DateTo.HasValue))
            {
                #region Validate

                if (!Validator.ValidateSelectInterval(accountYearFrom, accountYearTo) || !Validator.ValidateSelectInterval(accountPeriodFrom, accountPeriodTo))
                {
                    SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                    return false;
                }

                #endregion

                #region From

                if (!String.IsNullOrEmpty(accountYearFrom))
                {
                    //AccountYear
                    AccountYear accountYear = am.GetAccountYear(Convert.ToInt32(accountYearFrom));
                    if (accountYear != null)
                    {
                        s.DateFrom = s.AccountYearFromDate;
                        s.AccountYearFromId = accountYear.AccountYearId;
                        s.AccountYearFromDate = accountYear.From;

                        if (!String.IsNullOrEmpty(accountPeriodFrom))
                        {
                            //AccountPeriod
                            AccountPeriod accountPeriod = am.GetAccountPeriod(Convert.ToInt32(accountPeriodFrom));
                            if (accountPeriod != null)
                            {
                                s.DateFrom = s.AccountPeriodFromDate;
                                s.AccountPeriodFromId = accountPeriod.AccountPeriodId;
                                s.AccountPeriodFromDate = accountPeriod.From;
                            }
                        }
                    }
                }

                #endregion

                #region To

                if (!String.IsNullOrEmpty(accountYearTo))
                {
                    //AccountYear
                    AccountYear accountYear = am.GetAccountYear(Convert.ToInt32(accountYearTo));
                    if (accountYear != null)
                    {
                        s.DateTo = s.AccountYearToDate;
                        s.AccountYearToId = accountYear.AccountYearId;
                        s.AccountYearToDate = accountYear.To;

                        if (!String.IsNullOrEmpty(accountPeriodTo))
                        {
                            //AccountPeriod
                            AccountPeriod accountPeriod = am.GetAccountPeriod(Convert.ToInt32(accountPeriodTo));
                            if (accountPeriod != null)
                            {
                                s.DateTo = s.AccountPeriodToDate;
                                s.AccountPeriodToId = accountPeriod.AccountPeriodId;
                                s.AccountPeriodToDate = accountPeriod.To;
                            }
                        }
                    }
                }

                //SysReportTemplate reportTemplate = rm.GetSysReportTemplate(es.ReportTemplateId);
                //if (reportTemplate != null && reportTemplate.SysReportTemplateTypeId == (int)SoeReportTemplateType.GeneralLedger)
                //{
                //    if (accountYearTo != accountYearFrom)
                //    {
                //        SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(4819, "Perioder måste vara inom samma redovisningsår när du skriver ut huvudbok");
                //        return false;
                //    }
                //}

                #endregion
            }

            #endregion

            #region Validate

            if (!Validator.ValidateDateInterval(s.DateFrom, s.DateTo))
            {
                SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1448, "Till får inte vara mindre än Från i intervall");
                return false;
            }

            #endregion

            #endregion

            #region TaxAudit

            s.CreateVatVoucher = createVatVoucher;

            #endregion

            #endregion

            #endregion

            #region Set EvaluatedSelection from SelectionVoucher

            SetEvaluated(s, es, dateSelection);

            #endregion

            return true;
        }

        public void SetEvaluated(SelectionStd s, EvaluatedSelection es, bool dateSelection)
        {
            if (s == null || es == null)
                return;

            #region Date

            if (dateSelection)
            {
                if (s.DateFrom.HasValue && s.DateTo.HasValue)
                {
                    es.DateFrom = s.DateFrom.Value;
                    es.DateTo = s.DateTo.Value;
                }

                es.HasDateInterval = s.DateFrom.HasValue && s.DateTo.HasValue;
            }
            else
            {
                if (s.AccountPeriodFromDate.HasValue)
                    es.DateFrom = s.AccountPeriodFromDate.Value;
                else if (s.AccountYearFromDate.HasValue)
                    es.DateFrom = s.AccountYearFromDate.Value;

                if (s.AccountPeriodToDate.HasValue)
                    es.DateTo = s.AccountPeriodToDate.Value;
                else if (s.AccountYearToDate.HasValue)
                    es.DateTo = s.AccountYearToDate.Value;

                es.HasDateInterval = s.AccountPeriodFromDate != null || s.AccountYearFromDate != null || s.AccountPeriodToDate != null || s.AccountYearToDate != null;
            }

            #endregion

            #region AccountYear and AccountPeriod

            if (s.AccountYearFromDate.HasValue)
            {
                es.SSTD_AccountYearFromText = GetShortDateStringFromCulture(s.AccountYearFromDate.Value);
                es.SSTD_LongAccountYearFromText = GetLongDateStringFromCulture(s.AccountYearFromDate.Value);
            }
            if (s.AccountYearToDate.HasValue)
            {
                es.SSTD_AccountYearToText = GetShortDateStringFromCulture(s.AccountYearToDate.Value);
                es.SSTD_LongAccountYearToText = GetLongDateStringFromCulture(s.AccountYearToDate.Value);
            }
            es.SSTD_HasAccountYearText = !String.IsNullOrEmpty(es.SSTD_AccountYearFromText) && !String.IsNullOrEmpty(es.SSTD_AccountYearToText);

            if (s.AccountPeriodFromDate.HasValue)
                es.SSTD_AccountPeriodFromText = GetShortDateStringFromCulture(s.AccountPeriodFromDate.Value);
            if (s.AccountPeriodToDate.HasValue)
                es.SSTD_AccountPeriodToText = GetShortDateStringFromCulture(s.AccountPeriodToDate.Value);
            es.SSTD_HasAccountPeriodText = !String.IsNullOrEmpty(es.SSTD_AccountPeriodFromText) && !String.IsNullOrEmpty(es.SSTD_AccountPeriodToText);

            if (s.AccountYearFromId == s.AccountYearToId)
            {
                es.SSTD_AccountYearId = s.AccountYearToId;
                es.SSTD_IsSameYear = true;
            }
            else
            {
                es.SSTD_AccountYearId = s.AccountYearFromId;
            }
            #endregion

            #region PreviousAccountYear
            if (s.AccountYearFromDate.HasValue)
            {
                AccountYear accountYear = am.GetAccountYear(s.AccountYearFromId, false);
                AccountYear previousaccountYear = am.GetPreviousAccountYear(accountYear, false);
                if (previousaccountYear != null)
                {
                    es.SSTD_PreviousAccountYearFromText = GetLongDateStringFromCulture(previousaccountYear.From.Date);
                    es.SSTD_PreviousAccountYearToText = GetLongDateStringFromCulture(previousaccountYear.To.Date);
                }
            }

            #endregion

            //Budget

            if (s.BudgetId != null)
                es.SSTD_BudgetId = s.BudgetId;

            //TaxAudit
            es.SSTD_CreateVatVoucher = s.CreateVatVoucher;

            //Year-end and external vouchers
            es.SSTD_IncludeYearEndVouchers = s.IncludeYearEndVouchers;
            es.SSTD_IncludeExternalVouchers = s.IncludeExternalVouchers;

            //Project report
            es.SSTD_ProjectReport = s.ProjectReport;
            es.SSTD_AccountDimId = s.AccountDimId;
            es.SSTD_IncludeMissingAccountDim = s.IncludeMissingAccountDim;
            es.SSTD_SeparateAccountDim = s.SeparateAccountDim;
            
            //Set as evaluated
            es.SSTD_IsEvaluated = true;
        }

        private string GetShortDateStringFromCulture(DateTime dateTime)
        {
            string dateString = dateTime.ToString("yyyyMM");

            if (CultureInfo.CurrentCulture != null)
            {
                switch (CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                {
                    case "da":
                        dateString = dateTime.ToString("MM.yyyy");
                        break;
                    case "fi":
                        dateString = dateTime.ToString("M.yyyy");
                        break;
                    case "nb":
                        dateString = dateTime.ToString("MM.yyyy");
                        break;
                    case "en":
                        dateString = dateTime.ToString("M/yyyy");
                        break;
                }
            }

            return dateString;
        }

        private string GetLongDateStringFromCulture(DateTime dateTime)
        {
            string dateString = dateTime.ToString("yyyyMMdd");

            if (CultureInfo.CurrentCulture != null)
            {
                switch (CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                {
                    case "da":
                        dateString = dateTime.ToString("dd.MM.yyyy");
                        break;
                    case "fi":
                        dateString = dateTime.ToString("dd.MM.yyyy");
                        break;
                    case "nb":
                        dateString = dateTime.ToString("dd.MM.yyyy");
                        break;
                    case "en":
                        dateString = dateTime.ToString("d/M/yyyy");
                        break;
                }
            }

            return dateString;
        }


    }
}