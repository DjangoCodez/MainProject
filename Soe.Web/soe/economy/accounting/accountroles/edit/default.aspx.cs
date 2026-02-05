using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.economy.accounting.accountroles.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected AccountManager am;
        protected AccountDim accountDim;
        private int accountDimId;
        public string legendLabel;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_AccountRoles_Edit;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters
            if (Int32.TryParse(QS["dim"], out accountDimId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    accountDim = am.GetPrevNextAccountDim(accountDimId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (accountDim != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?dim=" + accountDim.AccountDimId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?dim=" + accountDimId);
                }
                else
                {
                    accountDim = am.GetAccountDim(accountDimId, SoeCompany.ActorCompanyId);
                    if (accountDim == null)
                    {
                        Form1.MessageWarning = GetText(1279, "Konteringsnivå hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                    else
                    {
                        bool importedAccounts = StringUtility.GetBool(QS["imported"]);
                        if (importedAccounts)
                            Form1.MessageSuccess = GetText(1945, "Kontoplan importerad");
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(1069, "Redigera konteringsnivå");
            string registerModeTabHeaderText = GetText(1557, "Registrera konteringsnivå");
            legendLabel = GetText(1065, "Konteringsnivå");
            if (accountDim != null && accountDim.IsStandard)
            {
                editModeTabHeaderText = GetText(8046, "Redigera kontoplan");
                registerModeTabHeaderText = GetText(8047, "Registrera kontoplan");
                legendLabel = GetText(3775, "Kontoplan");
            }

            PostOptionalParameterCheck(Form1, accountDim, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = accountDim != null ? accountDim.Name : "";

            #endregion

            #region UserControls

            if (accountDim != null && HasRolePermission(Feature.Common_Language, Permission.Modify))
            {
                Translations.Visible = true;
                Translations.InitControl(CompTermsRecordType.AccountDimName, accountDim.AccountDimId);
            }

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(0, "");
            for (int i = 1; i < 10; i++)
                dict.Add(i, i.ToString());

            MinChar.ConnectDataSource(dict);
            MaxChar.ConnectDataSource(dict);
            SysSieDim.ConnectDataSource(GetGrpText(TermGroup.SieAccountDim, addEmptyRow: true));

            if (accountDim != null && accountDim.IsStandard)
                ExternalAccounting.ConnectDataSource(am.GetSysAccountStdTypesDict(true));

            #endregion

            #region Set data

            LinkedToProjectExplanation.DefaultIdentifier = " ";
            LinkedToProjectExplanation.DisableFieldset = true;
            LinkedToProjectExplanation.Instructions = new List<string>()
				{
					GetText(3354, "Endast en konteringsnivå kan vara länkat till projekt."),
                    GetText(3355, "En länkad konteringsnivå administreras via projektbilden."),
				};

            if (accountDim != null)
            {
                AccountDimNr.Value = Convert.ToString(accountDim.AccountDimNr);
                Name.Value = accountDim.Name;
                ShortName.Value = accountDim.ShortName;
                MinChar.Value = Convert.ToString(accountDim.MinChar);
                MaxChar.Value = Convert.ToString(accountDim.MaxChar);
                LinkedToProject.Value = accountDim.LinkedToProject.ToString();

                if (accountDim.IsStandard)
                {
                    // Cannot delete AccountDim Standard
                    Form1.EnableDelete = false;

                    MinChar.Visible = false;
                    MaxChar.Visible = false;

                    ExternalAccounting.Visible = true;
                    ExternalAccounting.Value = Convert.ToString(accountDim.SysAccountStdTypeParentId);

                    // Cannot link Standard AccountDim to project
                    LinkedToProject.Visible = false;
                    LinkedToProjectExplanation.Visible = false;

                    // Cannot change AccountDimNr for Standard
                    AccountDimNr.ReadOnly = true;
                    DivSie.Visible = false;
                }
                else
                {
                    SysSieDim.Value = Convert.ToString(accountDim.SysSieDimNr);
                }
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                bool isAccountDimStd = (accountDim != null && accountDim.IsStandard);

                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = isAccountDimStd ? GetText(8048, "Kontoplan sparat") : GetText(1075, "Konteringsnivå sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = isAccountDimStd ? GetText(8049, "Kontoplan kunde inte sparas") : GetText(1106, "Konteringsnivå kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = isAccountDimStd ? GetText(8050, "Kontoplan uppdaterat") : GetText(1244, "Konteringsnivå uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = isAccountDimStd ? GetText(8051, "Kontoplan kunde inte uppdateras") : GetText(1473, "Konteringsnivå kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(1076, "Konteringsnivå finns redan");
                else if (MessageFromSelf == "SIE_EXIST")
                    Form1.MessageInformation = GetText(1145, "Konteringsnivå med angiven SIE dimension finns redan");
                else if (MessageFromSelf == "PROJECT_ACCOUNTDIM_EXIST")
                    Form1.MessageInformation = GetText(3356, "Konteringsnivå länkad till projekt finns redan");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(3376, "Konteringsnivån borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(1277, "Konteringsnivå kunde inte tas bort, kontrollera att det inte används");
                else if (MessageFromSelf == "INVALID_CHARLENGTH")
                    Form1.MessageWarning = GetText(1225, "Minimum längd får inte vara större än max längd");
                else if (MessageFromSelf == "IMPORT_SUCCESS")
                    Form1.MessageSuccess = GetText(1262, "Import klar");
                else if (MessageFromSelf == "IMPORT_FAILED")
                    Form1.MessageError = GetText(1263, "Fel uppstod vid import");
                else if (MessageFromSelf == "INVALID_DIMENSION")
                    Form1.MessageWarning = GetText(2155, "Nummer för konteringsnivån måste vara större än ett");
            }

            #endregion

            #region Navigation

            if (accountDim != null)
            {
                Form1.SetRegLink(GetText(1067, "Registrera konteringsnivå"), "",
                    Feature.Economy_Accounting_AccountRoles_Edit, Permission.Modify);

                if (accountDim.IsStandard)
                {
                    Form1.AddLink(GetText(1094, "Importera kontoplan"), "/modalforms/ImportAccountStds.aspx",
                        Feature.Economy_Accounting_AccountRoles_Edit, Permission.Modify, true);
                }
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            int accountDimNr = 1;
            string name = F["Name"];
            string shortName = F["ShortName"];

            int? sysSieDimNr = null;
            if (!String.IsNullOrEmpty(F["SysSieDim"]))
                sysSieDimNr = Convert.ToInt32(F["SysSieDim"]);

            int? minChar = null;
            if (!String.IsNullOrEmpty(F["MinChar"]))
                minChar = Convert.ToInt32(F["MinChar"]);

            int? maxChar = null;
            if (!String.IsNullOrEmpty(F["MaxChar"]))
                maxChar = Convert.ToInt32(F["MaxChar"]);

            if (minChar > maxChar)
                RedirectToSelf("INVALID_CHARLENGTH", true);

            bool linkedToProject = StringUtility.GetBool(F["LinkedToProject"]);

            if (accountDim == null)
            {
                #region Add AccountDIm

                if (sysSieDimNr.HasValue)
                {
                    if (sysSieDimNr.Value > 0)
                    {
                        if (am.AccountDimSieExist(sysSieDimNr.Value, SoeCompany.ActorCompanyId))
                            RedirectToSelf("SIE_EXIST", true);
                    }
                }

                accountDimNr = Convert.ToInt32(F["AccountDimNr"]);

                //Validation: AccountDimNr not already exist
                if (am.AccountDimExist(accountDimNr, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                if (accountDimNr <= 1)
                    RedirectToSelf("INVALID_DIMENSION", true);

                //Add AccountDim
                accountDim = new AccountDim()
                {
                    AccountDimNr = accountDimNr,
                    Name = name,
                    ShortName = shortName,
                    SysSieDimNr = sysSieDimNr,
                    MinChar = minChar,
                    MaxChar = maxChar,
                    LinkedToProject = linkedToProject,
                };

                var result = am.AddAccountDim(accountDim, SoeCompany.ActorCompanyId);
                if (result.Success)
                    RedirectToSelf("SAVED");
                else
                {
                    if (result.ErrorNumber == (int)ActionResultSave.ProjectAccountDimExists)
                        RedirectToSelf("PROJECT_ACCOUNTDIM_EXIST", true);
                    else
                        RedirectToSelf("NOTSAVED", true);
                }

                #endregion
            }
            else
            {
                #region Update AccountDim

                if (F["AccountDimNr"] != null)
                    accountDimNr = Convert.ToInt32(F["AccountDimNr"]);

                if (sysSieDimNr.HasValue)
                {
                    if (sysSieDimNr.Value > 0)
                    {
                        AccountDim ad = am.GetAccountDimFromSieDimNr(Convert.ToInt32(sysSieDimNr.Value), SoeCompany.ActorCompanyId);
                        if (ad != null)
                        {
                            if (ad.AccountDimNr != accountDim.AccountDimNr)
                            {
                                RedirectToSelf("SIE_EXIST", true);
                            }
                        }
                    }
                }

                //Validation: AccountDimNr not already exist
                if (accountDim.AccountDimNr != accountDimNr)
                {
                    //Validation: AccountDimId not already exist
                    if (am.AccountDimExist(accountDimNr, SoeCompany.ActorCompanyId))
                        RedirectToSelf("EXIST", true);
                }

                if (accountDimNr < 1)
                    RedirectToSelf("INVALID_DIMENSION", true);

                //Update AccountDim
                accountDim.AccountDimNr = accountDimNr;
                accountDim.Name = name;
                accountDim.ShortName = shortName;
                accountDim.SysSieDimNr = sysSieDimNr;
                accountDim.MinChar = minChar;
                accountDim.MaxChar = maxChar;
                accountDim.LinkedToProject = linkedToProject;

                if (accountDim.IsStandard)
                {
                    accountDim.SysAccountStdTypeParentId = null;
                    if (!String.IsNullOrEmpty(F["ExternalAccounting"]))
                    {
                        int id = Convert.ToInt32(F["ExternalAccounting"]);
                        if (id > 0)
                            accountDim.SysAccountStdTypeParentId = id;
                    }
                }

                var result = am.UpdateAccountDim(accountDim, SoeCompany.ActorCompanyId);
                if (result.Success)
                {
                    Translations.SaveTranslations();

                    RedirectToSelf("UPDATED");
                }
                else
                {
                    if (result.ErrorNumber == (int)ActionResultSave.ProjectAccountDimExists)
                        RedirectToSelf("PROJECT_ACCOUNTDIM_EXIST", true);
                    else
                        RedirectToSelf("NOTUPDATED", true);
                }

                #endregion
            }
        }

        protected override void Delete()
        {
            if (am.DeleteAccountDim(accountDim, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion

        #region Help-methods

        #endregion
    }
}
