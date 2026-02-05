using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.soe.billing.preferences.productsettings.accounts
{
    public partial class _default : PageBase
    {
        #region Variables

        protected SettingManager sm;
        protected AccountManager am;

        //Accounts
        private Account purchaseAccount;
        private Account salesAccount;
        private Account salesVatFreeAccount;
        private Account accountStockIn;
        private Account accountStockInChange;
        private Account accountStockOut;
        private Account accountStockOutChange;
        private Account accountStockInventory;
        private Account accountStockInventoryChange;
        private Account accountStockLoss;
        private Account accountStockLossChange;
        private Account accountStockTransferChange;
        

        private IEnumerable<AccountDim> accountDims;

        public string stdDimID; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_ProductSettings_Accounts;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("/cssjs/account.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);
            sm = new SettingManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Set data

            AccountDim accountDimStd = am.GetAccountDimStd(SoeCompany.ActorCompanyId);
            if (accountDimStd == null)
                return;

            stdDimID = accountDimStd.AccountDimId.ToString();

            GetAccounts();

            HtmlTableRow tRow;
            HtmlTableCell tCell;
            TextEntry text;
            Text label;
            SelectEntry select;

            #region AccountStd Purchase

            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 1130,
                DefaultTerm = "Standard",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "CreditAccount";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (purchaseAccount != null)
            {
                text.Value = purchaseAccount.AccountNr;
                text.InfoText += purchaseAccount.Name;
            }
            text.OnChange = "accountSearch.searchField('CreditAccount')";
            text.OnKeyUp = "accountSearch.keydown('CreditAccount')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            PurchaseAccountTable.Rows.Add(tRow);

            #endregion

            #region AccountStd Sales

            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 1130,
                DefaultTerm = "Standard",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "DebitAccount";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (salesAccount != null)
            {
                text.Value = salesAccount.AccountNr;
                text.InfoText += salesAccount.Name;
            }
            text.OnChange = "accountSearch.searchField('DebitAccount')";
            text.OnKeyUp = "accountSearch.keydown('DebitAccount')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            SalesAccountTable.Rows.Add(tRow);

            #endregion

            #region AccountStd Sales VatFree

            // DebitAccount
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 1130,
                DefaultTerm = "Standard",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "DebitVatFreeAccount";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (salesVatFreeAccount != null)
            {
                text.Value = salesVatFreeAccount.AccountNr;
                text.InfoText += salesVatFreeAccount.Name;
            }
            text.OnChange = "accountSearch.searchField('DebitVatFreeAccount')";
            text.OnKeyUp = "accountSearch.keydown('DebitVatFreeAccount')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            SalesVatFreeAccountTable.Rows.Add(tRow);

            #endregion

            int counter = 1;
            foreach (AccountDim accountDim in accountDims)
            {
                #region AccountDim

                // Only internal accounts
                if (accountDim.IsStandard)
                    continue;

                var accountInternalsDict = am.GetAccountInternalsDict(accountDim.AccountDimId, SoeCompany.ActorCompanyId, true);

                #region AccountInternal Purchase

                tRow = new HtmlTableRow();
                tRow.Attributes.Add("RowType", "AccountInternal");

                tCell = new HtmlTableCell();
                label = new Text()
                {
                    LabelSetting = accountDim.Name,
                    FitInTable = true,
                };
                tCell.Controls.Add(label);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                select = new SelectEntry();
                select.ID = "CreditAccount" + counter;
                select.HideLabel = true;
                select.FitInTable = true;
                select.DisableSettings = true;
                select.ConnectDataSource(accountInternalsDict);

                int purchaseAccountSettingId = am.GetProductAccountInternalPurchaseSettingId(counter);
                int purchaseAccountValue = sm.GetIntSetting(SettingMainType.Company, purchaseAccountSettingId, UserId, SoeCompany.ActorCompanyId, 0);
                if (accountInternalsDict.ContainsKey(purchaseAccountValue))
                    select.Value = purchaseAccountValue.ToString();

                tCell.Controls.Add(select);
                tRow.Cells.Add(tCell);

                PurchaseAccountTable.Rows.Add(tRow);

                #endregion

                #region AccountInternal Sales

                tRow = new HtmlTableRow();
                tRow.Attributes.Add("RowType", "AccountInternal");

                tCell = new HtmlTableCell();
                label = new Text()
                {
                    LabelSetting = accountDim.Name,
                    FitInTable = true,
                };
                tCell.Controls.Add(label);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                select = new SelectEntry();
                select.ID = "DebitAccount" + counter;
                select.HideLabel = true;
                select.FitInTable = true;
                select.DisableSettings = true;
                select.ConnectDataSource(accountInternalsDict);

                int salesAccountSettingId = am.GetProductAccountInternalSalesSettingId(counter);
                int salesAccountValue = sm.GetIntSetting(SettingMainType.Company, salesAccountSettingId, UserId, SoeCompany.ActorCompanyId, 0);
                if (accountInternalsDict.ContainsKey(salesAccountValue))
                    select.Value = salesAccountValue.ToString();

                tCell.Controls.Add(select);
                tRow.Cells.Add(tCell);

                SalesAccountTable.Rows.Add(tRow);

                #endregion

                #region AccountInternal SalesVatFree

                tRow = new HtmlTableRow();
                tRow.Attributes.Add("RowType", "AccountInternal");

                tCell = new HtmlTableCell();
                label = new Text()
                {
                    LabelSetting = accountDim.Name,
                    FitInTable = true,
                };
                tCell.Controls.Add(label);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                select = new SelectEntry();
                select.ID = "DebitVatFreeAccount" + counter;
                select.HideLabel = true;
                select.FitInTable = true;
                select.DisableSettings = true;
                select.ConnectDataSource(accountInternalsDict);

                int salesVatFreeAccountSettingId = am.GetProductAccountInternalSalesVatFreeSettingId(counter);
                int salesVatFreeAccountValue = sm.GetIntSetting(SettingMainType.Company, salesVatFreeAccountSettingId, UserId, SoeCompany.ActorCompanyId, 0);
                if (accountInternalsDict.ContainsKey(salesVatFreeAccountValue))
                    select.Value = salesVatFreeAccountValue.ToString();

                tCell.Controls.Add(select);
                tRow.Cells.Add(tCell);

                SalesVatFreeAccountTable.Rows.Add(tRow);

                #endregion
                
                #endregion

                counter++;
            }

            #region Stock
            //Todo Terms !
            
            //Out
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 4652,
                DefaultTerm = "Ut lager",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockOut";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockIn != null)
            {
                text.Value = accountStockIn.AccountNr;
                text.InfoText += accountStockIn.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockOut')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockOut')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);

            //Out change
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 4653,
                DefaultTerm = "Ut lagerändring",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockOutChange";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockOutChange != null)
            {
                text.Value = accountStockOutChange.AccountNr;
                text.InfoText += accountStockOutChange.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockOutChange')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockOutChange')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);

            //In
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 4654,
                DefaultTerm = "In lager",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockIn";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockIn != null)
            {
                text.Value = accountStockIn.AccountNr;
                text.InfoText += accountStockIn.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockIn')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockOutIn')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);

            //In change
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 4655,
                DefaultTerm = "In lagerändring",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockInChange";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockInChange != null)
            {
                text.Value = accountStockInChange.AccountNr;
                text.InfoText += accountStockInChange.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockInChange')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockInChange')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);

            //Inventory
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 4656,
                DefaultTerm = "Inventering lager",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockInventory";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockInventory != null)
            {
                text.Value = accountStockInventory.AccountNr;
                text.InfoText += accountStockInventory.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockInventory')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockInventory')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);

            //Inventory Change
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 4657,
                DefaultTerm = "Inventering lagerändring",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockInventoryChange";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockInventoryChange != null)
            {
                text.Value = accountStockInventoryChange.AccountNr;
                text.InfoText += accountStockInventoryChange.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockInventoryChange')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockInventoryChange')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);

            //Loss
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 4658,
                DefaultTerm = "Kassation lager",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockLoss";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockLoss != null)
            {
                text.Value = accountStockLoss.AccountNr;
                text.InfoText += accountStockLoss.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockLoss')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockLoss')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);

            //Loss Change
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 4659,
                DefaultTerm = "Kassation lagerändring",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockLossChange";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockLossChange != null)
            {
                text.Value = accountStockLossChange.AccountNr;
                text.InfoText += accountStockLossChange.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockLossChange')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockLossChange')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);

            //Stocktransfer
            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 7748,
                DefaultTerm = "Lageromföring lagerändring",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "AccountStockTransferChange";
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            if (accountStockTransferChange != null)
            {
                text.Value = accountStockTransferChange.AccountNr;
                text.InfoText += accountStockTransferChange.Name;
            }
            text.OnChange = "accountSearch.searchField('AccountStockTransferChange')";
            text.OnKeyUp = "accountSearch.keydown('AccountStockTransferChange')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            StockAccountTable.Rows.Add(tRow);
            #endregion

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            bool success = true;

            var values = new Dictionary<int, int>();

            //AccountStd
            values.Add((int)CompanySettingType.AccountInvoiceProductPurchase, am.GetAccountStdIdFromAccountNr(F["CreditAccount"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountInvoiceProductSales, am.GetAccountStdIdFromAccountNr(F["DebitAccount"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountInvoiceProductSalesVatFree, am.GetAccountStdIdFromAccountNr(F["DebitVatFreeAccount"], SoeCompany.ActorCompanyId));

            //AccountInternal
            for (int i = 1; i <= 5; i++) //TODO: Hard-coded
            {
                int setting = 0;
                int value = 0;
                string purchase = F["CreditAccount" + i];
                if (purchase != null)
                {
                    if ((Int32.TryParse(purchase, out value)) && value > 0)
                    {
                        setting = am.GetProductAccountInternalPurchaseSettingId(i);
                        values.Add(setting, value);
                    }
                }

                string sales = F["DebitAccount" + i];
                if (sales != null)
                {
                    if ((Int32.TryParse(sales, out value)) && value > 0)
                    {
                        setting = am.GetProductAccountInternalSalesSettingId(i);
                        values.Add(setting, value);
                    }
                }

                string salesVatFree = F["DebitVatFreeAccount" + i];
                if (salesVatFree != null)
                {
                    if ((Int32.TryParse(salesVatFree, out value)) && value > 0)
                    {
                        setting = am.GetProductAccountInternalSalesVatFreeSettingId(i);
                        values.Add(setting, value);
                    }
                }
            }

            //Stock accounts
            values.Add((int)CompanySettingType.AccountStockOut, am.GetAccountStdIdFromAccountNr(F["AccountStockOut"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountStockOutChange, am.GetAccountStdIdFromAccountNr(F["AccountStockOutChange"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountStockIn, am.GetAccountStdIdFromAccountNr(F["AccountStockIn"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountStockInChange, am.GetAccountStdIdFromAccountNr(F["AccountStockInChange"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountStockInventory, am.GetAccountStdIdFromAccountNr(F["AccountStockInventory"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountStockInventoryChange, am.GetAccountStdIdFromAccountNr(F["AccountStockInventoryChange"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountStockLoss, am.GetAccountStdIdFromAccountNr(F["AccountStockLoss"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountStockLossChange, am.GetAccountStdIdFromAccountNr(F["AccountStockLossChange"], SoeCompany.ActorCompanyId));
            values.Add((int)CompanySettingType.AccountStockTransferChange, am.GetAccountStdIdFromAccountNr(F["AccountStockTransferChange"], SoeCompany.ActorCompanyId));

            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, values, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion

        #region Help-methods

        private void GetAccounts()
        {
            //Get AccountDims
            accountDims = am.GetAccountDimsByCompany(SoeCompany.ActorCompanyId);

            // Get AccountStds
            purchaseAccount = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountInvoiceProductPurchase, UserId, SoeCompany.ActorCompanyId, 0));
            salesAccount = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountInvoiceProductSales, UserId, SoeCompany.ActorCompanyId, 0));
            salesVatFreeAccount = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountInvoiceProductSalesVatFree, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockIn = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockIn, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockInChange = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockInChange, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockOut = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockOut, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockOutChange = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockOutChange, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockInventory = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockInventory, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockInventoryChange = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockInventoryChange, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockLoss = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockLoss, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockLossChange = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockLossChange, UserId, SoeCompany.ActorCompanyId, 0));
            accountStockTransferChange = am.GetAccount(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountStockTransferChange, UserId, SoeCompany.ActorCompanyId, 0));
        }

        #endregion
    }
}
