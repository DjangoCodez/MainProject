using System;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Core;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System.Text;

namespace SoftOne.Soe.Web.soe.economy.import.sie
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am;
        private VoucherManager vm;

        private SieImportContainer ic;
        private int importType;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.HasAngularHost = true;
            this.Feature = Feature.Economy_Import_Sie;
            base.Page_Init(sender, e);
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);
            vm = new VoucherManager(ParameterObject);
            ic = new SieImportContainer();

            //Mandatory parameters
            if (Int32.TryParse(QS["type"], out importType))
            {
                switch (importType)
                {
                    case (int)SieImportType.Account:
                        #region Account

                        this.Feature = Feature.Economy_Import_Sie_Account;
                        Form1.SetTabHeaderText(1, GetText(1390, "SIE import") + " " + GetText(1258, "Konto"));

                        DivAccountYearSelection.Visible = false;
                        DivAccountSelection.Visible = true;
                        DivVoucherSelection.Visible = false;
                        DivAccountBalanceSelection.Visible = false;
                        DivImportTypeSelection.Visible = false;

                        #endregion
                        break;
                    case (int)SieImportType.Voucher:
                        #region Voucher

                        this.Feature = Feature.Economy_Import_Sie_Voucher;
                        Form1.SetTabHeaderText(1, GetText(1390, "SIE import") + " " + GetText(1259, "Verifikat"));

                        DivAccountYearSelection.Visible = true;
                        DivAccountSelection.Visible = false;
                        DivVoucherSelection.Visible = true;
                        DivAccountBalanceSelection.Visible = false;
                        DivImportTypeSelection.Visible = false;

                        #endregion
                        break;
                    case (int)SieImportType.AccountBalance:
                        #region AccountBalance

                        this.Feature = Feature.Economy_Import_Sie_AccountBalance;
                        Form1.SetTabHeaderText(1, GetText(1390, "SIE import") + " " + GetText(1391, "Ingående balans"));

                        DivAccountYearSelection.Visible = true;
                        DivAccountSelection.Visible = false;
                        DivVoucherSelection.Visible = false;
                        DivAccountBalanceSelection.Visible = true;
                        DivImportTypeSelection.Visible = false;

                        NotImplementedInstruction.HeaderText = GetText(1804, "Information");
                        NotImplementedInstruction.Numeric = true;
                        NotImplementedInstruction.Instructions = new List<string>()
                        {
                            GetText(1680, "Import av ingående balans för objekt (#OIB) stöds ej")
                        };

                        #endregion
                        break;
                    case (int)SieImportType.Account_Voucher_AccountBalance:
                        #region Account_Voucher_AccountBalance

                        this.Feature = Feature.Economy_Import_Sie;

                        CheckBoxSelectAccount.ReadOnly = true;
                        CheckBoxSelectVoucher.ReadOnly = true;
                        CheckBoxSelectAccountBalance.ReadOnly = true;

                        string headerText = GetText(1390, "SIE import");
                        if (HasRolePermission(Feature.Economy_Import_Sie_Account, Permission.Modify))
                        {
                            headerText += " " + GetText(1258, "Konto");
                            CheckBoxSelectAccount.ReadOnly = false;
                        }
                        if (HasRolePermission(Feature.Economy_Import_Sie_Voucher, Permission.Modify))
                        {
                            headerText += " " + GetText(1259, "Verifikat");
                            CheckBoxSelectVoucher.ReadOnly = false;
                        }
                        if (HasRolePermission(Feature.Economy_Import_Sie_AccountBalance, Permission.Modify))
                        {
                            headerText += " " + GetText(1391, "Ingående balans");
                            CheckBoxSelectAccountBalance.ReadOnly = false;
                        }

                        Form1.SetTabHeaderText(1, headerText);
                        DivImportTypeSelection.Visible = true;
                        //DivAccountYearSelection.Visible = true;

                        NotImplementedInstruction.HeaderText = GetText(1804, "Information");
                        NotImplementedInstruction.Numeric = true;
                        NotImplementedInstruction.Instructions = new List<string>()
                        {
                            GetText(1680, "Import av ingående balans för objekt (#OIB) stöds ej")
                        };

                        #endregion
                        break;
                }
            }
            else
            {
                RedirectToModuleRoot();
                return;
            }

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath + "?type=" + importType, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            SoeGrid1.Title = GetText(5426, "Resultat");

            AccountYear currentAccountYear = am.GetCurrentAccountYear(SoeCompany.ActorCompanyId);

            Dictionary<int, string> accountYearsDict = am.GetAccountYearsDict(SoeCompany.ActorCompanyId, false, false, false, true);
            Dictionary<int, string> voucherSeriesTypesDict = vm.GetVoucherSeriesTypesDict(SoeCompany.ActorCompanyId, false, true);
            Dictionary<int, string> voucherSeriesDict = new Dictionary<int, string>();
            if (currentAccountYear != null)
                voucherSeriesDict = vm.GetVoucherSeriesByYearDict(currentAccountYear.AccountYearId, SoeCompany.ActorCompanyId, true, false);

            #endregion

            #region Populate

            bool repopulate = Mode == SoeFormMode.Repopulate;

            //AccountYear
            AccountYear.ConnectDataSource(accountYearsDict);

            //VoucherSeries default
            VoucherSeries.DataSource = voucherSeriesDict;
            VoucherSeries.DataTextField = "value";
            VoucherSeries.DataValueField = "key";
            VoucherSeries.DataBind();

            //VoucherSeries mapping
            VoucherSeriesTypesMapping.Labels = voucherSeriesTypesDict;
            VoucherSeriesDelete.Labels = voucherSeriesTypesDict;

            #endregion

            #region Set data

            if (repopulate)
            {
                //VoucherSeriesTypesMapping
                VoucherSeriesTypesMapping.PreviousForm = PreviousForm;

                //VoucherSeriesDelete
                VoucherSeriesDelete.PreviousForm = PreviousForm;
            }
            else
            {
                //Default values
                OverrideVoucherSeries.Value = null;
                OverrideVoucherSeries.ReadOnly = true;
                OverrideNameConflicts.Value = Boolean.FalseString;
                ApproveEmptyAccountNames.Value = Boolean.FalseString;
                ImportAccountStd.Value = Boolean.TrueString;
                ImportAccountInternal.Value = Boolean.TrueString;
                ImportAsUtf8.Value = Boolean.FalseString;

                //AccountYear
                if (currentAccountYear != null)
                    AccountYear.Value = currentAccountYear.AccountYearId.ToString();
            }

            #endregion

            #region Actions

            TryImport();

            if (!String.IsNullOrEmpty(ic.Message))
                RedirectToSelf(ic.Message, ic.MessageRequireRepopulate);

            #endregion

            #region MessageFromSelf

            List<SieImportItemBase> conflicts = null;

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SUCCESS")
                    Form1.MessageSuccess = GetText(1168, "SIE import klar");
                if (MessageFromSelf == "SUCCESS_WITHCONFLICTS")
                    Form1.MessageWarning = GetText(1168, "SIE import klar") + ". " + GetText(1169, "Konflikter uppstod");
                if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(1170, "SIE import misslyckades");
                if (MessageFromSelf == "FAILED_WITHCONFLICTS")
                    Form1.MessageError = GetText(1170, "SIE import misslyckades") + ". " + GetText(1169, "Konflikter uppstod");
                if (MessageFromSelf == "FAILED_ACCOUNT")
                    Form1.MessageError = GetText(1170, "SIE import misslyckades") + ". " + GetText(1643, "Konton kunde inte läsas");
                if (MessageFromSelf == "FAILED_ACCOUNT_WITHCONFLICTS")
                    Form1.MessageError = GetText(1170, "SIE import misslyckades") + ". " + GetText(1169, "Konflikter uppstod") + ". " + GetText(1643, "Konton kunde inte läsas");
                if (MessageFromSelf == "FAILED_VOUCHER")
                    Form1.MessageError = GetText(1170, "SIE import misslyckades") + ". " + GetText(1206, "Verifikat kunde inte läsas in");
                if (MessageFromSelf == "FAILED_VOUCHER_WITHCONFLICTS")
                    Form1.MessageError = GetText(1170, "SIE import misslyckades") + ". " + GetText(1169, "Konflikter uppstod") + ". " + GetText(1206, "Verifikat kunde inte läsas in");
                if (MessageFromSelf == "FAILED_ACCOUNTBALANCE")
                    Form1.MessageError = GetText(1170, "SIE import misslyckades") + ". " + GetText(1644, "Ingående balanser kunde inte läsas in");
                if (MessageFromSelf == "FAILED_ACCOUNTBALANCE_WITHCONFLICTS")
                    Form1.MessageError = GetText(1170, "SIE import misslyckades") + ". " + GetText(1169, "Konflikter uppstod") + ". " + GetText(1644, "Ingående balanser kunde inte läsas in");
                if (MessageFromSelf == "FILENOTFOUND")
                    Form1.MessageWarning = GetText(1179, "Filen hittades inte");
                if (MessageFromSelf == "ACCOUNTYEAR_MANDATORY")
                    Form1.MessageWarning = GetText(1639, "Du måste ange redovisningsår");

                string avab_message = "";
                if (MessageFromSelf.Contains("FAILED_AVAB_NO_IMPORTTYPE_SELECTED"))
                    avab_message += " " + GetText(9250, "Ingen importtyp vald") + ".";
                if (MessageFromSelf.Contains("FAILED_AVAB_ACCOUNTYEAR_MANDATORY"))
                    avab_message += " " + GetText(1639, "Du måste ange redovisningsår.");

                if (MessageFromSelf.Contains("SUCCESS_AVAB_ACCOUNT_IMPORT"))
                    avab_message += " " + GetText(9251, "Konto import klar") + ".";
                if (MessageFromSelf.Contains("FAILED_AVAB_ACCOUNT_IMPORT"))
                    avab_message += " " + GetText(9252, "Konto import misslyckades") + ".";
                if (MessageFromSelf.Contains("FAILED_AVAB_ACCOUNT_NOT_IMPORTED_PARSE_CONFLICTS"))
                    avab_message += " " + GetText(9253, "Konto inte importerat, kunde inte läsas in") + ".";
                if (MessageFromSelf.Contains("SUCCESS_AVAB_VOUCHER_IMPORT"))
                    avab_message += " " + GetText(9254, "Verifikat import klar") + ".";
                if (MessageFromSelf.Contains("FAILED_AVAB_VOUCHER_IMPORT"))
                    avab_message += " " + GetText(9255, "Verifikat import misslyckades") + ".";
                if (MessageFromSelf.Contains("FAILED_AVAB_VOUCHER_NOT_IMPORTED_ACCOUNT_CONFLICTS"))
                    avab_message += " " + GetText(9256, "Verifikat inte importerade, konflikter i Konto importen") + ".";
                if (MessageFromSelf.Contains("FAILED_AVAB_VOUCHER_NOT_IMPORTED_PARSE_CONFLICTS"))
                    avab_message += " " + GetText(9257, "Verifikat inte importerade, kunde inte läsas in") + ".";
                if (MessageFromSelf.Contains("SUCCESS_AVAB_BALANCE_IMPORT"))
                    avab_message += " " + GetText(9258, "Ingående balans import klar") + ".";
                if (MessageFromSelf.Contains("FAILED_AVAB_BALANCE_IMPORT"))
                    avab_message += " " + GetText(9259, "Ingående balans import misslyckades") + ".";
                if (MessageFromSelf.Contains("FAILED_AVAB_ACCOUNT_BALANCE_NOT_IMPORTED_PARSE_CONFLICTS"))
                    avab_message += " " + GetText(9260, "Ingående balans inte importerad, kunde inte läsas in") + ".";

                if (avab_message != "")
                {
                    Form1.MessageError = avab_message;
                    Form1.MessageSuccess = avab_message;
                }

                conflicts = Session[Constants.SESSION_DOWNLOAD_SIE_CONFLICTS] as List<SieImportItemBase>;
            }

            #region Conflicts

            SoeGrid1.Visible = !conflicts.IsNullOrEmpty();
            SoeGrid1.DataSource = conflicts;
            SoeGrid1.RowDataBound += SoeGrid1_RowDataBound;
            SoeGrid1.DataBind();

            Session[Constants.SESSION_DOWNLOAD_SIE_CONFLICTS] = null;

            #endregion

            #endregion
        }

        #region Action-methods

        private void TryImport()
        {
            if (Request.Form["action"] == "upload" && Session[Constants.SESSION_ACTION_DOUBLECLICK] == null)
            {
                Session[Constants.SESSION_DOWNLOAD_SIE_CONFLICTS] = null;
                Session[Constants.SESSION_ACTION_DOUBLECLICK] = 1;

                int accountYearId = Convert.ToInt32(F["AccountYear"]);
                if (importType == (int)SieImportType.Account || accountYearId > 0 || importType == (int)SieImportType.Account_Voucher_AccountBalance)
                {
                    bool bSuccess = false;
                    HttpPostedFile file = Request.Files["File"];
                    HttpPostedFile fileAccount = Request.Files["File"];
                    HttpPostedFile fileVoucher = Request.Files["File"];
                    HttpPostedFile fileAccountBalance = Request.Files["File"];
                    if (file != null && file.ContentLength > 0)
                    {
                        bSuccess = Import(file, fileAccount, fileVoucher, fileAccountBalance);
                        if (bSuccess && importType != (int)SieImportType.Account_Voucher_AccountBalance)
                        {
                            ic.Message = ic.HasConflicts() ? "SUCCESS_WITHCONFLICTS" : "SUCCESS";
                            ic.MessageRequireRepopulate = false;
                        }
                        else
                        {
                            switch (importType)
                            {
                                case (int)SieImportType.Account:
                                    ic.Message = ic.HasConflicts() ? "FAILED_ACCOUNT_WITHCONFLICTS" : "FAILED_ACCOUNT";
                                    break;
                                case (int)SieImportType.Voucher:
                                    ic.Message = ic.HasConflicts() ? "FAILED_VOUCHER_WITHCONFLICTS" : "FAILED_VOUCHER";
                                    break;
                                case (int)SieImportType.AccountBalance:
                                    ic.Message = ic.HasConflicts() ? "FAILED_ACCOUNTBALANCE_WITHCONFLICTS" : "FAILED_ACCOUNTBALANCE";
                                    break;
                                case (int)SieImportType.Account_Voucher_AccountBalance:
                                    //SUCCESS
                                    if (ic.AvabSuccessImportAccount)
                                        ic.Message += "SUCCESS_AVAB_ACCOUNT_IMPORT";
                                    if (ic.AvabSuccessImportVoucher)
                                        ic.Message += "SUCCESS_AVAB_VOUCHER_IMPORT";
                                    if (ic.AvabSuccessImportAccountBalance)
                                        ic.Message += "SUCCESS_AVAB_BALANCE_IMPORT";
                                    //FAILED
                                    if (ic.AvabFailedImportAccount)
                                        ic.Message += "FAILED_AVAB_ACCOUNT_IMPORT";
                                    if (ic.AvabFiledImportVoucher)
                                        ic.Message += "FAILED_AVAB_VOUCHER_IMPORT";
                                    if (ic.AvabFailedImportAccountBalance)
                                        ic.Message += "FAILED_AVAB_BALANCE_IMPORT";
                                    //ERRORMESSAGE GENERAL
                                    if (ic.AvabErrorMessageGeneral != null)
                                    {
                                        if (ic.AvabErrorMessageGeneral.Contains("NO_IMPORTTYPE_SELECTED"))
                                            ic.Message += "FAILED_AVAB_NO_IMPORTTYPE_SELECTED";
                                        if (ic.AvabErrorMessageGeneral.Contains("ACCOUNTYEAR_MANDATORY"))
                                            ic.Message += "FAILED_AVAB_ACCOUNTYEAR_MANDATORY";
                                        if (ic.AvabErrorMessageGeneral.Contains("VOUCHER_NOT_IMPORTED_ACCOUNT_CONFLICTS"))
                                            ic.Message += "FAILED_AVAB_VOUCHER_NOT_IMPORTED_ACCOUNT_CONFLICTS";
                                        if (ic.AvabErrorMessageGeneral.Contains("VOUCHER_NOT_IMPORTED_PARSE_CONFLICTS"))
                                            ic.Message += "FAILED_AVAB_VOUCHER_NOT_IMPORTED_PARSE_CONFLICTS";
                                        if (ic.AvabErrorMessageGeneral.Contains("ACCOUNT_NOT_IMPORTED_PARSE_CONFLICTS"))
                                            ic.Message += "FAILED_AVAB_ACCOUNT_NOT_IMPORTED_PARSE_CONFLICTS";
                                        if (ic.AvabErrorMessageGeneral.Contains("ACCOUNT_BALANCE_NOT_IMPORTED_PARSE_CONFLICTS"))
                                            ic.Message += "FAILED_AVAB_ACCOUNT_BALANCE_NOT_IMPORTED_PARSE_CONFLICTS";
                                    }
                                    //DEFAULT
                                    if (ic.Message == "")
                                    {
                                        if (bSuccess)
                                            ic.Message = ic.HasConflicts() ? "SUCCESS_WITHCONFLICTS" : "SUCCESS";
                                        else
                                            ic.Message = ic.HasConflicts() ? "FAILED_WITHCONFLICTS" : "FAILED";
                                    }

                                    break;
                                default:
                                    ic.Message = ic.HasConflicts() ? "FAILED_WITHCONFLICTS" : "FAILED";
                                    break;
                            }
                            ic.MessageRequireRepopulate = true;
                        }
                    }
                    else
                    {
                        ic.Message = "FILENOTFOUND";
                        ic.MessageRequireRepopulate = true;
                    }
                }
                else
                {
                    ic.Message = "ACCOUNTYEAR_MANDATORY";
                    ic.MessageRequireRepopulate = true;
                }

                Session[Constants.SESSION_DOWNLOAD_SIE_CONFLICTS] = ic.GetConflicts();
                Session[Constants.SESSION_ACTION_DOUBLECLICK] = null;
            }
        }

        private bool Import(HttpPostedFile file, HttpPostedFile fileAccount, HttpPostedFile fileVoucher, HttpPostedFile fileAccountBalance)
        {
            bool success = false;

            try
            {
                ic.ImportAsUtf8 = StringUtility.GetBool(F["ImportAsUtf8"]);

                if (ic.ImportAsUtf8)
                {
                    ic.StreamReader = new StreamReader(file.InputStream, Encoding.UTF8);
                }
                else
                {
                    ic.StreamReader = new StreamReader(file.InputStream, Constants.ENCODING_IBM437);
                }
                MemoryStream ms = new MemoryStream();

                if (importType == (int)SieImportType.Account_Voucher_AccountBalance)
                    file.InputStream.CopyTo(ms); //Important, only copy if this import type!                

                if (ic.StreamReader != null)
                {
                    ic.UserId = UserId;
                    ic.ActorCompanyId = SoeCompany.ActorCompanyId;
                    ic.ImportType = (SieImportType)importType;
                    ic.AllowNotOpenAccountYear = StringUtility.GetBool(F["AllowNotOpenAccountYear"]);

                    bool bAccount = false;
                    bool bVoucher = false;
                    bool bAccountBalance = false;

                    switch (importType)
                    {
                        case (int)SieImportType.Account:
                            bAccount = true;
                            SetSieContainerForAccount(ref ic);
                            SieManager sm1 = new SieManager(ParameterObject);
                            success = sm1.Import(ic, bAccount, bVoucher, bAccountBalance);
                            break;
                        case (int)SieImportType.Voucher:
                            bVoucher = true;
                            SetSieContainerForVoucher(ref ic);
                            SieManager sm2 = new SieManager(ParameterObject);
                            success = sm2.Import(ic, bAccount, bVoucher, bAccountBalance, StringUtility.GetBool(F["UseAccountDistribution"]));
                            break;
                        case (int)SieImportType.AccountBalance:
                            bAccountBalance = true;
                            SetSieContainerForAccountBalance(ref ic);
                            SieManager sm3 = new SieManager(ParameterObject);
                            success = sm3.Import(ic, bAccount, bVoucher, bAccountBalance);
                            break;
                        case (int)SieImportType.Account_Voucher_AccountBalance:
                            if (StringUtility.GetBool(F["CheckBoxSelectAccount"]))
                            {
                                fileAccount.InputStream.Position = ms.Position = 0;
                                if (ic.ImportAsUtf8)
                                {
                                    ic.StreamReaderAccount = new StreamReader(fileAccount.InputStream, Encoding.UTF8);
                                }
                                else
                                {
                                    ic.StreamReaderAccount = new StreamReader(fileAccount.InputStream, Constants.ENCODING_IBM437);
                                }
                                SetSieContainerForAccount(ref ic);
                                SieManager sm4 = new SieManager(ParameterObject);
                                success = sm4.Import(ic, true, false, false, StringUtility.GetBool(F["UseAccountDistribution"]));
                            }
                            if (StringUtility.GetBool(F["CheckBoxSelectVoucher"]))
                            {
                                fileVoucher.InputStream.Position = ms.Position = 0;
                                if (ic.ImportAsUtf8)
                                {
                                    ic.StreamReaderVoucher = new StreamReader(fileVoucher.InputStream, Encoding.UTF8);
                                }
                                else
                                {
                                    ic.StreamReaderVoucher = new StreamReader(fileVoucher.InputStream, Constants.ENCODING_IBM437);
                                }
                                bVoucher = true;
                                SetSieContainerForVoucher(ref ic);
                                SieManager sm5 = new SieManager(ParameterObject);
                                success = sm5.Import(ic, false, bVoucher, false, StringUtility.GetBool(F["UseAccountDistribution"]));
                            }
                            if (StringUtility.GetBool(F["CheckBoxSelectAccountBalance"]))
                            {
                                fileAccountBalance.InputStream.Position = ms.Position = 0;
                                if (ic.ImportAsUtf8)
                                {
                                    ic.StreamReaderAccountBalance = new StreamReader(fileAccountBalance.InputStream, Encoding.UTF8);
                                }
                                else
                                {
                                    ic.StreamReaderAccountBalance = new StreamReader(fileAccountBalance.InputStream, Constants.ENCODING_IBM437);
                                }
                                bAccountBalance = true;
                                SetSieContainerForAccountBalance(ref ic);
                                SieManager sm6 = new SieManager(ParameterObject);
                                success = sm6.Import(ic, false, false, bAccountBalance, StringUtility.GetBool(F["UseAccountDistribution"]));
                            }
                            break;
                    }
                }
            }
            finally
            {
                ic.CloseStream();
                if ((SieImportType)importType == SieImportType.Account_Voucher_AccountBalance)
                {
                    ic.CloseStreamAccount();
                    ic.CloseStreamVoucher();
                    ic.CloseStreamAccountBalance();
                }
            }

            return success;
        }

        private void SetSieContainerForAccount(ref SieImportContainer ic)
        {
            ic.OverwriteNameConflicts = StringUtility.GetBool(F["OverrideNameConflicts"]);
            ic.ApproveEmptyAccountNames = StringUtility.GetBool(F["ApproveEmptyAccountNames"]);
            ic.ImportAccountStd = StringUtility.GetBool(F["ImportAccountStd"]);
            ic.ImportAccountInternal = StringUtility.GetBool(F["ImportAccountInternal"]);
            ic.EmptyAccountName = "[" + GetText(1675, "Kontonamn saknas") + "]";
        }

        private void SetSieContainerForVoucher(ref SieImportContainer ic)
        {
            ic.ImportAsUtf8 = StringUtility.GetBool(F["ImportAsUtf8"]);
            ic.AccountYearId = Convert.ToInt32(F["AccountYear"]);
            int voucherSeriesId = Convert.ToInt32(F["VoucherSeries"]);
            if (voucherSeriesId > 0)
            {
                ic.DefaultVoucherSeriesId = voucherSeriesId;
                ic.OverrideVoucherSeries = StringUtility.GetBool(F["OverrideVoucherSeries"]);
            }

            var voucherSeriesMappingItems = VoucherSeriesTypesMapping.GetData(F);
            if (voucherSeriesMappingItems != null && voucherSeriesMappingItems.Count > 0)
            {
                ic.VoucherSeriesTypesMappingDict = new Dictionary<string, int>();
                foreach (var item in voucherSeriesMappingItems)
                {
                    ic.VoucherSeriesTypesMappingDict.Add(item.From.ToLower(), item.LabelType);
                }
            }
            ic.SkipAlreadyExistingVouchers = StringUtility.GetBool(F["SkipAlreadyExistingVouchers"]);
            ic.OverrideVoucherDeletes = StringUtility.GetBool(F["OverrideVoucherSeriesDelete"]);
            var voucherSeriesDeleteItems = VoucherSeriesDelete.GetData(F);
            if (voucherSeriesDeleteItems != null && voucherSeriesDeleteItems.Count > 0)
            {
                ic.VoucherSeriesDeleteDict = new Dictionary<int, bool>();
                foreach (var item in voucherSeriesDeleteItems)
                {
                    if (item.Checked)
                        ic.VoucherSeriesDeleteDict.Add(item.LabelType, item.Checked);
                }
            }
        }

        private void SetSieContainerForAccountBalance(ref SieImportContainer ic)
        {
            ic.AccountYearId = Convert.ToInt32(Request.Form["AccountYear"]);
            ic.OverrideAccountBalance = StringUtility.GetBool(F["OverrideAccountBalance"]);
            ic.UseUBInsteadOfIB = StringUtility.GetBool(F["UseUBInsteadOfIB"]);
        }

        #endregion

        #region Events

        private void SoeGrid1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            SieImportItemBase sieImportItem = ((e.Row.DataItem) as SieImportItemBase);
            if (sieImportItem != null)
            {
                e.Row.VerticalAlign = VerticalAlign.Top;

                #region Values

                PlaceHolder phValues = (PlaceHolder)e.Row.FindControl("phValues");
                if (phValues != null)
                {
                    Label lbl = new Label();
                    switch (sieImportItem.Label)
                    {
                        case Constants.SIE_LABEL_DIM:
                            SieAccountDimItem accountDimItem = sieImportItem as SieAccountDimItem;
                            if (accountDimItem != null)
                            {
                                lbl.Text = GetText(1173, "Dimension") + ":" + accountDimItem.AccountDimNr + ", " +
                                            GetText(1190, "Namn") + ":" + accountDimItem.Name;
                            }
                            break;
                        case Constants.SIE_LABEL_KONTO:
                            SieAccountStdItem accountStdItem = sieImportItem as SieAccountStdItem;
                            if (accountStdItem != null)
                            {
                                lbl.Text = GetText(1166, "Kontonr") + ":" + accountStdItem.AccountNr + ", " +
                                            GetText(1167, "Namn") + ":" + accountStdItem.Name + ", " +
                                            GetText(1165, "Kontotyp") + ":" +
                                            (accountStdItem.AccountType != null ? TextService.GetText(Convert.ToInt32(accountStdItem.AccountType), (int)TermGroup.AccountType) : String.Empty);
                            }
                            break;
                        case Constants.SIE_LABEL_OBJEKT:
                            SieAccountInternalItem accountInternalItem = sieImportItem as SieAccountInternalItem;
                            if (accountInternalItem != null)
                            {
                                lbl.Text = GetText(1173, "Dimension") + ":" + accountInternalItem.AccountDimNr + ", " +
                                            GetText(1188, "Objektkod") + ":" + accountInternalItem.ObjectCode + ", " +
                                            GetText(1189, "Objektnamn") + ":" + accountInternalItem.Name;
                            }
                            break;
                        case Constants.SIE_LABEL_IB:
                            SieAccountStdInBalanceItem accountStdInBalanceItem = sieImportItem as SieAccountStdInBalanceItem;
                            if (accountStdInBalanceItem != null)
                            {
                                lbl.Text = GetText(1394, "År") + ":" + accountStdInBalanceItem.AccountYear + ", " +
                                            GetText(1395, "Kontonr") + ":" + accountStdInBalanceItem.AccountNr + ", " +
                                            GetText(1396, "Saldo") + ":" + accountStdInBalanceItem.Balance + ", " +
                                            GetText(1397, "Kvantitet") + ":" + accountStdInBalanceItem.Quantity != null ? Convert.ToDecimal(accountStdInBalanceItem.Quantity).ToString() : String.Empty;
                            }
                            break;
                        case Constants.SIE_LABEL_VER:
                            SieVoucherItem voucherItem = sieImportItem as SieVoucherItem;
                            if (voucherItem != null)
                            {
                                lbl.Text = GetText(1198, "Serie") + ":" + voucherItem.VoucherSeriesTypeNr + ", " +
                                            GetText(1199, "Nr") + ":" + voucherItem.VoucherNr + ", " +
                                            GetText(1200, "Datum") + ":" + voucherItem.VoucherDate + ", " +
                                            GetText(1201, "Text") + ":" + voucherItem.Text + ", " +
                                            GetText(1202, "Balans") + ":" + voucherItem.Balance;
                            }
                            break;
                    }
                    phValues.Controls.Add(lbl);
                }

                #endregion

                #region Conflict

                PlaceHolder phConflict = (PlaceHolder)e.Row.FindControl("phConflict");
                if (phConflict != null)
                {
                    Label lbl = new Label();
                    foreach (SieConflictItem sieConflictItem in sieImportItem.Conflicts)
                    {
                        string conflict = "";
                        switch (sieConflictItem.Conflict)
                        {
                            //General
                            case SieConflict.Exception:
                                conflict = sieConflictItem.StrData;
                                break;

                            //Import
                            case SieConflict.Import_NameConflict:
                                INameItem nameItem = sieImportItem as INameItem;
                                if (nameItem != null)
                                    conflict = GetText(1176, "Namnkonflikt");
                                conflict = conflict + " [" + nameItem.OriginalName + "-->" + nameItem.Name + "]";
                                break;
                            case SieConflict.Import_AddFailed:
                                conflict = GetText(1184, "Kunde inte lägga till");
                                break;
                            case SieConflict.Import_UpdateFailed:
                                conflict = GetText(1185, "Kunde inte uppdatera");
                                break;

                            //Syntax
                            case SieConflict.Import_InvalidLine:
                                conflict = GetText(1196, "Felaktig syntax") + " [" + sieConflictItem.Line + "]";
                                break;

                            //MandatoryFieldMissing
                            case SieConflict.Import_MandatoryFieldMissing_Dim_dimensionsnr:
                                conflict = GetText(1208, "Obligatoriska fältet dimensionsnr saknas på dimension");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Dim_namn:
                                conflict = GetText(1209, "Obligatoriska fältet namn saknas på dimension");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Objekt_dimensionsnr:
                                conflict = GetText(1210, "Obligatoriska fältet dimensionsnr saknas på dimension");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Objekt_objektkod:
                                conflict = GetText(1211, "Obligatoriska fältet objektkod saknas på objekt");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Objekt_objektnamn:
                                conflict = GetText(1212, "Obligatoriska fältet objektnamn saknas på objekt");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Konto_kontonr:
                                conflict = GetText(1213, "Obligatoriska fältet kontonr saknas på konto");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Konto_kontonamn:
                                conflict = GetText(1214, "Obligatoriska fältet namn saknas på konto");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Ktyp_kontotyp:
                                conflict = GetText(1215, "Obligatoriska fältet kontotyp saknas");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Ver_verdatum:
                                conflict = GetText(1197, "Obligatoriska fältet verifikatdatum saknas på verifikat");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Trans_kontonr:
                                conflict = GetText(1203, "Obligatoriska fältet kontonr saknas på transaktion");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_Trans_belopp:
                                conflict = GetText(1204, "Obligatoriska fältet belopp saknas på transaktion");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountyear:
                                conflict = GetText(1484, "Obligatoriska fältet redovisningsår saknas på ingående balans");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_AccountBalance_accountnr:
                                conflict = GetText(1485, "Obligatoriska fältet kontonr saknas på ingående balans");
                                break;
                            case SieConflict.Import_MandatoryFieldMissing_AccountBalance_balance:
                                conflict = GetText(1486, "Obligatoriska fältet saldo saknas på ingående balans");
                                break;

                            case SieConflict.Import_DimNotFound:
                                conflict = GetText(1177, "Kontodimension hittades inte");
                                conflict = sieConflictItem.IntData.HasValue ? conflict + " [" + sieConflictItem.IntData + "]" : conflict;
                                break;

                            //AccountInternal
                            case SieConflict.Import_ObjectRuleFailed:
                                conflict = GetText(1221, "Objekt uppfyller inte regler för sin dimension");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;

                            //AccountStd
                            case SieConflict.Import_AccountRuleFailed:
                                conflict = GetText(1222, "Konto uppfyller inte regler för sin dimension");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;
                            case SieConflict.Import_AccountHasNoAccountType:
                                conflict = GetText(1183, "Konto har ingen kontotyp");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;

                            //AccountBalance
                            case SieConflict.Import_AccountBalanceHasUnknownAccountYear:
                                conflict = GetText(1392, "Ingående balans har ogiltigt redovisningsår");
                                conflict = sieConflictItem.IntData.HasValue ? conflict + " [" + sieConflictItem.IntData + "]" : conflict;
                                break;
                            case SieConflict.Import_AccountBalanceExistInAccountYear:
                                conflict = GetText(1393, "Ingånde balans finns redan för angivet konto och redovisningsår");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;
                            case SieConflict.Import_AccountBalanceUseUBInsteadOfIB_PreviousYearNotInFile:
                                conflict = GetText(5980, "Kan ej använda UB istället för IB. UB för föregående år finns ej i filen");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;

                            //Voucher and Transactions
                            case SieConflict.Import_VoucherHasNoStartLabel:
                                conflict = GetText(1192, "Verifikat saknar startklammer för transaktioner");
                                conflict += " [ { ]";
                                break;
                            case SieConflict.Import_VoucherHasNoEndLabel:
                                conflict = GetText(1193, "Verifikat saknar slutklammer för transaktioner");
                                conflict += " [ } ]";
                                break;
                            //case SieConflict.Import_VoucherHasNoTransactions:
                            //    conflict = GetText(1194, "Verifikat saknar transaktioner");
                            //    break;
                            //case SieConflict.Import_VoucherHasInvalidBalance:
                            //    conflict = GetText(1195, "Verifikat har felaktig balans");
                            //    conflict = (sieConflictItem.DataDec != null) ? conflict + " [" + sieConflictItem.DataDec + "]" : conflict;
                            //    break;
                            case SieConflict.Import_VoucherSeriesTypeDoesNotExist:
                                conflict = GetText(1205, "Angiven serie i verifikat har ingen matchande serietyp, måste läggas upp manuellt");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;
                            case SieConflict.Import_VoucherHasNoVoucherNr:
                                conflict = GetText(1175, "Angivet verifikat har inget verifikatnr");
                                break;
                            case SieConflict.Import_VoucherAlreadyExist:
                                conflict = GetText(1073, "Angivet verifikat finns redan i aktuell verifikatserie");
                                conflict = sieConflictItem.IntData.HasValue ? conflict + " [" + sieConflictItem.IntData + "]" : conflict;
                                break;
                            case SieConflict.Import_VouchersAccountYearDoesNotMatchAccountYearDefault:
                                conflict = GetText(1220, "Angivet verifikats redovisningsår matchar inte valt redovisningsår");
                                conflict = sieConflictItem.DateData.HasValue ? conflict + " [" + sieConflictItem.DateData + "]" : conflict;
                                break;
                            case SieConflict.Import_VouchersAccountYearDoesNotExist:
                                conflict = GetText(1216, "Matchande redovisningsår finns inte, måste läggas upp manuellt");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;
                            case SieConflict.Import_VouchersAccountPeriodDoesNotExist:
                                conflict = GetText(1217, "Matchande period finns inte, måste läggas upp manuellt");
                                conflict = sieConflictItem.DateData.HasValue ? conflict + " [" + sieConflictItem.DateData + "]" : conflict;
                                break;
                            case SieConflict.Import_VouchersAccountPeriodIsNotOpen:
                                conflict = GetText(11969, "Angiven period är ej öppen, måste öppnas först");
                                conflict = sieConflictItem.DateData.HasValue ? conflict + " [" + sieConflictItem.DateData + "]" : conflict;
                                break;
                            case SieConflict.Import_VouchersAccountDoesNotExist:
                                conflict = GetText(1218, "Angivet konto i transaktion finns inte");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;
                            case SieConflict.Import_VouchersObjectDoesNotExist:
                                conflict = GetText(1219, "Angivet internkonto i transaktion finns inte");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;

                            //AccountStdBalance
                            case SieConflict.Import_AccountDoesNotExist:
                                conflict = GetText(1637, "Angivet konto finns inte");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;

                            //AccountYear
                            case SieConflict.Import_AccountYearIsNotOpen:
                                conflict = GetText(1434, "Angivet redovisningsår är inte öppet");
                                conflict = !String.IsNullOrEmpty(sieConflictItem.StrData) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
                                break;
                        }
                        if (String.IsNullOrEmpty(lbl.Text))
                            lbl.Text = conflict;
                        else
                            lbl.Text += HttpUtility.HtmlDecode("<br>" + conflict);
                    }
                    phConflict.Controls.Add(lbl);
                }
                #endregion
            }
        }

        #endregion
    }
}
