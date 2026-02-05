using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UserControls.WebForms
{
    public partial class AccountUC : ControlBase, IControlBase
    {
        private int accountId = 0;
        public int SelectedGridValueId
        {
            get { return accountId; }
            set { accountId = value; }
        }

        #region Variables

        private AccountManager am;
        private AccountBalanceManager abm;

        protected Account account;
        protected AccountDim accountDim;
        private int accountDimId;

        //Module specifics
        public bool EnableEconomy { get; set; }

        #endregion

        protected void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();
        }

        private void EnableModuleSpecifics()
        {
            if (PageBase.CTX["Feature"] != null)
            {
                this.PageBase.Feature = (Feature)PageBase.CTX["Feature"];
                switch (this.PageBase.Feature)
                {
                    case Feature.Economy_Accounting_Accounts_Edit:
                        EnableEconomy = true;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(PageBase.ParameterObject);
            abm = new AccountBalanceManager(PageBase.ParameterObject, PageBase.SoeCompany.ActorCompanyId);

            //Mandatory parameters
            if (Int32.TryParse(PageBase.QS["dim"], out accountDimId))
            {
                accountDim = am.GetAccountDim(accountDimId, PageBase.SoeCompany.ActorCompanyId);
                if (accountDim == null)
                {
                    throw new SoeEntityNotFoundException("AccountDim", this.ToString());
                }
            }
            else
            {
                throw new SoeQuerystringException("dim", this.ToString());
            }

            //Optional parameters
            if (accountId != 0)
                account = am.GetAccount(PageBase.SoeCompany.ActorCompanyId, accountId, onlyActive: false, loadAccount: true, loadAccountDim: true, loadAccountMapping: true, loadAccountSru: true);

            #endregion

            #region Populate

            AccountType.ConnectDataSource(PageBase.GetGrpText(TermGroup.AccountType));
            VatAccount.ConnectDataSource(am.GetSysVatAccountCodesDict(true));
            AmountStop.ConnectDataSource(PageBase.GetGrpText(TermGroup.AmountStop));

            Dictionary<int, string> sruCodesDict = am.GetSysAccountSruCodesDict(true);
            AccountSru1.ConnectDataSource(sruCodesDict);
            AccountSru2.ConnectDataSource(sruCodesDict);

            if (accountDim.IsInternal)
            {
                GeneralAccountStd.Visible = false;
                DivAccountStd.Visible = false;
                DivAccountInternal.Visible = true;

                AccountInternalCategories.Populate(true, PageBase.SoeCompany.ActorCompanyId, accountId, true);
            }
            else
            {
                GeneralAccountStd.Visible = true;
                DivAccountStd.Visible = true;
                DivAccountInternal.Visible = false;
            }

            #endregion

            #region Set data

            AccountDim projDim = am.GetProjectAccountDim(PageBase.SoeCompany.ActorCompanyId);
            if (projDim != null && projDim.AccountDimId == accountDimId)
            {
                LinkedToProjectInstruction.DefaultIdentifier = " ";
                LinkedToProjectInstruction.DisableFieldset = true;
                LinkedToProjectInstruction.Instructions = new List<string>()
                {
                    "&nbsp;",
                    PageBase.GetText(3363, "Konteringsnivån är länkat till ett projekt."),
                    PageBase.GetText(3355, "En länkad konteringsnivå administreras via projektbilden."),
                    "&nbsp;",
                };

                DivLinkedToProjectInstruction.Visible = true;
            }

            if (account != null)
            {
                Active.Value = account.State == (int)SoeEntityState.Active ? Boolean.TrueString : Boolean.FalseString;
                Name.Value = account.Name;

                //AccountStd
                if (accountDim.IsStandard)
                {
                    AccountNr.Value = account.AccountNr;
                    ObjectCode.Visible = false;

                    if (account.AccountStd != null)
                    {
                        AccountType.Value = Convert.ToString(account.AccountStd.AccountTypeSysTermId);
                        VatAccount.Value = Convert.ToString(account.AccountStd.SysVatAccountId);
                        AmountStop.Value = Convert.ToString(account.AccountStd.AmountStop);
                        Unit.Value = account.AccountStd.Unit;

                        #region Balance

                        // Get Balances
                        List<AccountBalance> accountBalances = abm.GetAccountBalanceByAccount(account.AccountId, true);

                        HtmlTableRow tRow;
                        HtmlTableCell tCell;
                        Text label;

                        tRow = new HtmlTableRow();

                        tCell = new HtmlTableCell();
                        label = new Text()
                        {
                            TermID = 1502,
                            DefaultTerm = "Redovisningsår",
                            FitInTable = true,
                        };
                        tCell.Controls.Add(label);
                        tCell.Style["Padding"] = "5px";
                        tRow.Cells.Add(tCell);

                        tCell = new HtmlTableCell();
                        label = new Text()
                        {
                            TermID = 1109,
                            DefaultTerm = "Saldo",
                            FitInTable = true,
                        };
                        tCell.Controls.Add(label);
                        tCell.Style["Padding"] = "5px";
                        tRow.Cells.Add(tCell);

                        tCell = new HtmlTableCell();
                        label = new Text()
                        {
                            TermID = 3050,
                            DefaultTerm = "Senast uppdaterad",
                            FitInTable = true,
                        };
                        tCell.Controls.Add(label);
                        tCell.Style["Padding"] = "5px";
                        tRow.Cells.Add(tCell);

                        TableBalance.Rows.Add(tRow);

                        foreach (AccountBalance accountBalance in accountBalances)
                        {
                            tRow = new HtmlTableRow();
                            tCell = new HtmlTableCell();
                            tCell.Controls.Add(new LiteralControl(accountBalance.AccountYear.From.ToString("yyyyMM") + "-" + accountBalance.AccountYear.To.ToString("yyyyMM")));
                            tCell.Style["Padding"] = "5px 10px 5px 5px";
                            tRow.Cells.Add(tCell);

                            tCell = new HtmlTableCell();
                            tCell.Controls.Add(new LiteralControl(accountBalance.Balance.ToString()));
                            tCell.Style["Padding"] = "5px 10px 5px 5px";
                            tCell.Align = "Right";
                            tRow.Cells.Add(tCell);

                            tCell = new HtmlTableCell();
                            tCell.Controls.Add(new LiteralControl((accountBalance.Modified != null ? accountBalance.Modified : accountBalance.Created).ToString()));
                            tCell.Style["Padding"] = "5px 10px 5px 5px";
                            tCell.Align = "Right";
                            tRow.Cells.Add(tCell);

                            TableBalance.Rows.Add(tRow);
                        }

                        if (accountBalances.IsNullOrEmpty())
                            UpdateBalance.Visible = false;

                        #endregion

                        if (Convert.ToBoolean(account.AccountStd.UnitStop))
                            UnitStop.Value = Boolean.TrueString;
                        else
                            UnitStop.Value = Boolean.FalseString;

                        #region AccountSru

                        int i = 1;
                        foreach (AccountSru accountSru in account.AccountStd.AccountSru)
                        {
                            if (i > 2)
                                break;

                            if (i == 1)
                                AccountSru1.Value = Convert.ToString(accountSru.SysAccountSruCodeId);
                            else if (i == 2)
                                AccountSru2.Value = Convert.ToString(accountSru.SysAccountSruCodeId);

                            i++;
                        }

                        #endregion
                    }
                }
                //AccountInternal
                else
                {
                    ObjectCode.Value = account.AccountNr;
                    AccountNr.Visible = false;
                }
            }
            else
            {
                //AccountStd
                if (accountDim.IsStandard)
                {
                    ObjectCode.Visible = false;
                    AccountNr.OnChange = "checkSysAccountStdParent()";
                }
                //AccountInternal
                else
                {
                    AccountNr.Visible = false;
                }

                UnitStop.Value = Boolean.FalseString;
                DivBalance.Visible = false;

            }

            #region AccountMapping

            if (accountDim.IsStandard)
            {
                //AccountMapping
                bool addedAccountDim = false;
                bool addedHeader = false;
                bool addedCheckBoxInfo = false;

                HtmlTableRow tRow;
                HtmlTableCell tCell;
                Text label;
                CheckBoxEntry checkbox;
                SelectEntry select;

                IEnumerable<AccountDim> accountDims = am.GetAccountDimsByCompany(PageBase.SoeCompany.ActorCompanyId);
                foreach (AccountDim ad in accountDims)
                {
                    #region AccountDim

                    if (ad.AccountDimNr != Constants.ACCOUNTDIM_STANDARD)
                    {
                        tRow = new HtmlTableRow();

                        if (!addedHeader)
                        {
                            #region Header

                            //AccountInternal
                            tCell = new HtmlTableCell();
                            label = new Text()
                            {
                                TermID = 1129,
                                DefaultTerm = "Internkonto",
                                FitInTable = true,
                            };
                            tCell.Controls.Add(label);
                            tRow.Cells.Add(tCell);

                            //Standard
                            tCell = new HtmlTableCell();
                            label = new Text()
                            {
                                TermID = 1130,
                                DefaultTerm = "Standard",
                                FitInTable = true,
                            };
                            tCell.Controls.Add(label);
                            tRow.Cells.Add(tCell);

                            //Inactive
                            tCell = new HtmlTableCell();
                            label = new Text()
                            {
                                TermID = 1131,
                                DefaultTerm = "Inaktiverad",
                                FitInTable = true,
                            };
                            tCell.Controls.Add(label);
                            tRow.Cells.Add(tCell);

                            //Mandatory
                            tCell = new HtmlTableCell();
                            label = new Text()
                            {
                                TermID = 1132,
                                DefaultTerm = "Obligatorisk",
                                FitInTable = true,
                            };
                            tCell.Controls.Add(label);
                            tRow.Cells.Add(tCell);

                            tCell.Controls.Add(new LiteralControl("&nbsp;"));
                            tRow.Cells.Add(tCell);

                            TableAccountMapping.Rows.Add(tRow);

                            addedHeader = true;

                            #endregion
                        }

                        AccountMapping accountMapping = null;
                        if (account != null)
                            accountMapping = am.GetAccountMapping(account.AccountId, ad.AccountDimId, PageBase.SoeCompany.ActorCompanyId, false, true, true, true);

                        tRow = new HtmlTableRow();

                        //AccountDim name
                        tCell = new HtmlTableCell();
                        tCell.Controls.Add(new LiteralControl(ad.Name));
                        tRow.Cells.Add(tCell);

                        //Standard
                        tCell = new HtmlTableCell();
                        select = new SelectEntry();
                        select.ID = ad.AccountDimId + "_default";
                        select.HideLabel = true;
                        select.FitInTable = true;
                        select.DisableSettings = true;
                        select.ConnectDataSource(am.GetAccountInternalsDict(ad.AccountDimId, PageBase.SoeCompany.ActorCompanyId, true));
                        select.OnChange = "internalAccountChanged('" + select.ID + "')";
                        if (accountMapping?.AccountInternal != null)
                            select.Value = accountMapping.AccountInternal.AccountId.ToString();
                        tCell.Controls.Add(select);
                        tRow.Cells.Add(tCell);

                        //Inactive
                        tCell = new HtmlTableCell();
                        checkbox = new CheckBoxEntry();
                        checkbox.ID = ad.AccountDimId + "_warn";
                        checkbox.DisableSettings = true;
                        checkbox.FitInTable = true;
                        checkbox.HideLabel = true;
                        if ((accountMapping != null) && (accountMapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn))
                        {
                            checkbox.Value = Boolean.TrueString;
                        }
                        checkbox.OnClick = "checkMandatoryLevel('" + checkbox.ID + "')";
                        tCell.Controls.Add(checkbox);
                        tRow.Cells.Add(tCell);

                        //Mandatory
                        tCell = new HtmlTableCell();
                        checkbox = new CheckBoxEntry();
                        checkbox.ID = ad.AccountDimId + "_mandatory";
                        checkbox.DisableSettings = true;
                        checkbox.FitInTable = true;
                        checkbox.HideLabel = true;
                        if ((accountMapping != null) && (accountMapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory))
                        {
                            checkbox.Value = Boolean.TrueString;
                        }
                        checkbox.OnClick = "checkMandatoryLevel('" + checkbox.ID + "')";
                        tCell.Controls.Add(checkbox);
                        tRow.Cells.Add(tCell);

                        if (!addedCheckBoxInfo)
                        {
                            tCell = new HtmlTableCell();
                            Instruction intruction = new Instruction()
                            {
                                TermID = 3028,
                                DefaultTerm = "Båda kryssrutorna kan ej markeras samtidigt",
                                FitInTable = true,
                            };
                            tCell.Controls.Add(intruction);
                            tRow.Cells.Add(tCell);

                            addedCheckBoxInfo = true;
                        }

                        TableAccountMapping.Rows.Add(tRow);

                        addedAccountDim = true;
                    }

                    #endregion
                }

                if (!addedAccountDim)
                    DivAccountMapping.Visible = false;
            }

            #endregion

            #endregion
        }
    }
}