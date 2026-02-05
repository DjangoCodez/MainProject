using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class DistributionSelectionAccount : ControlBase
    {
        #region Variables

        private ReportManager rm;
        private AccountManager am;

        public NameValueCollection F { get; set; }
        public ReportSelection ReportSelection { get; set; }
        public bool OnlyAccountDimStd { get; set; }

        #endregion

        public void Populate(bool repopulate)
        {
            #region Init

            rm = new ReportManager(PageBase.ParameterObject);
            am = new AccountManager(PageBase.ParameterObject);

            #endregion

            #region Populate

            Account.Labels = am.GetAccountDimsByCompanyDict(PageBase.SoeCompany.ActorCompanyId, false, OnlyAccountDimStd, false);
            
            #endregion

            #region Set data

            if (repopulate)
            {
                Account.PreviousForm = SoeForm.PreviousForm;
            }
            else
            {
                if (ReportSelection != null)
                {
                    #region ReportSelection

                    int pos = 0;
                    var reportSelectionStrs = rm.GetReportSelectionStrs(ReportSelection.ReportSelectionId);
                    foreach (var reportSelectionStr in reportSelectionStrs)
                    {
                        switch (reportSelectionStr.ReportSelectionType)
                        {
                            case (int)SoeSelectionData.Str_Account:
                                Account.AddLabelValue(pos, reportSelectionStr.SelectGroup.ToString());
                                Account.AddValueFrom(pos, reportSelectionStr.SelectFrom);
                                Account.AddValueTo(pos, reportSelectionStr.SelectTo);
                                break;
                        }

                        pos++;
                        if (pos == Account.NoOfIntervals)
                            break;
                    }

                    #endregion
                }
            }

            #endregion
        }

        public bool Evaluate(SelectionAccount s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return false;

            #region Init

            if (F == null)
                return false;

            if (rm == null)
                rm = new ReportManager(PageBase.ParameterObject);
            if (am == null)
                am = new AccountManager(PageBase.ParameterObject);

            #endregion

            #region Validate input and read into SelectionAccount

            #region Read from Form

            int accountIntervalCounter;
            if (Int32.TryParse(F["Account-intervalcounter"], out accountIntervalCounter))
            {
                for (int i = 1; i <= accountIntervalCounter; i++)
                {
                    string dim = F["Account-label-" + i];
                    string from = F["Account-from-" + i];
                    string to = F["Account-to-" + i];

                    if (!Validator.ValidateTextInterval(from, to))
                    {
                        SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1449, "Ange både Från och Till på intervall, eller utelämna båda");
                        return false;
                    }
                }
            }

            #endregion

            #region Accounts

            Collection<FormIntervalEntryItem> formIntervalEntryItems = Account.GetData(F);
            foreach (FormIntervalEntryItem formIntervalEntryItem in formIntervalEntryItems)
            {
                //Read and evaluate interval
                AccountIntervalDTO accountInterval = new AccountIntervalDTO();

                AccountDim accountDim = am.GetAccountDim(formIntervalEntryItem.LabelType, PageBase.SoeCompany.ActorCompanyId);
                if (accountDim != null)
                {
                    accountInterval.AccountDimId = accountDim.AccountDimId;

                    if (!String.IsNullOrEmpty(formIntervalEntryItem.From))
                        accountInterval.AccountNrFrom = formIntervalEntryItem.From;

                    if (!String.IsNullOrEmpty(formIntervalEntryItem.To))
                        accountInterval.AccountNrTo = formIntervalEntryItem.To;

                    if (Validator.ValidateStringInterval(accountInterval.AccountNrFrom, accountInterval.AccountNrTo))
                    {
                        if (!String.IsNullOrEmpty(accountInterval.AccountNrFrom))
                            s.AddAccountInterval(accountInterval);
                    }
                    else
                    {
                        SoeForm.MessageWarning = PageBase.GetText(1450, "Felaktigt urval") + ". " + PageBase.GetText(1448, "Till får inte vara mindre än Från i intervall");
                        return false;
                    }
                }
            }

            #endregion

            #endregion

            #region Set EvaluatedSelection from SelectionAccount

            SetEvaluated(s, es);

            #endregion

            return true;
        }

        public void SetEvaluated(SelectionAccount s, EvaluatedSelection es)
        {
            if (s == null || es == null)
                return;

            if (s.AccountIntervals.Count > 0)
            {
                es.SA_AccountIntervals = s.AccountIntervals;
                es.SA_HasAccountInterval = true;
            }

            //Set as evaluated
            es.SA_IsEvaluated = true;
        }
    }
}
