using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Web;

namespace SoftOne.Soe.Web.soe.common.excelimport
{
    public partial class _default : PageBase
    {
        #region Variables

        //Managers
        private ExcelImportManager eim;

        //Module specifics
        protected bool EnableEconomy { get; set; }
        protected bool EnableBilling { get; set; }
        protected bool EnableTime { get; set; }
        protected bool loadFinnishFiles { get; set; }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            HasAngularSpaHost = true;

            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            if (base.GetLanguageId() == (int)TermGroup_Languages.Finnish)
                loadFinnishFiles = true;
            else
                loadFinnishFiles = false;

            // Add scripts and style sheets
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Import_ExcelImport:
                        EnableEconomy = true;
                        break;
                    case Feature.Billing_Import_ExcelImport:
                        EnableBilling = true;
                        break;
                    case Feature.Time_Import_ExcelImport:
                        EnableTime = true;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            eim = new ExcelImportManager(ParameterObject);

            //Mandatory parameters

            //Optional parameters
            //No PreOptionalParameterCheck needed

            //Mode
            Form1.SetTabHeaderText(1, GetText(4259, "Excelimport"));
            PostOptionalParameterCheck(Form1, null, true);

            SoeGrid1.Title = GetText(5426, "Resultat");
            SoeGrid1.Visible = false;

            DoNotModifyWithEmpty.InfoText = GetText(5885, "Obligatoriska fält måste alltid anges");

            #endregion

            #region Actions

            if (Request.Form["action"] == "upload")
            {
                #region Upload

                bool doNotModifyWithEmpty = StringUtility.GetBool(F["DoNotModifyWithEmpty"]);

                HttpPostedFile file = Request.Files["File"];
                if (file != null && file.ContentLength > 0)
                {
                    #region File found

                    string fileName = "";
                    string pathOnServer = "";

                    try
                    {
                        //Validate
                        fileName = eim.ValidatePostedFile(file.FileName, true);

                        //Save temp-file
                        pathOnServer = eim.SaveTempFileToServer(file.InputStream, fileName);

                        //Import
                        var result = eim.Import(pathOnServer, SoeCompany.ActorCompanyId, doNotModifyWithEmpty: doNotModifyWithEmpty);
                        if (result.Success)
                        {
                            Form1.MessageSuccess = result.ErrorMessage;
                        }
                        else
                        {
                            string errorMessage = "";
                            switch (result.ErrorNumber)
                            {
                                case (int)ActionResultSave.UserCannotBeAddedLicenseViolation:
                                    errorMessage = String.Format(GetText(2055, "Alla användare kunde inte importeras, licensen tillåter inte fler användare. Max {0} st"), result.IntegerValue);
                                    break;
                                case (int)ActionResultSave.EmployeeCannotBeAddedLicenseViolation:
                                    errorMessage = String.Format(GetText(5766, "Alla anställda kunde inte importeras, licensen tillåter inte fler anställda. Max {0} st"), result.IntegerValue);
                                    break;
                                case (int)ActionResultSave.EmployeeNumberExists:
                                    errorMessage = String.Format(GetText(5882, "Anställningsnumret '{0}' är upptaget"), result.StringValue);
                                    break;
                                default:
                                    errorMessage = result.ErrorMessage;
                                    break;

                            }

                            Form1.MessageError = errorMessage;
                        }

                        List<ImportExportConflictItem> conflicts = result.Value as List<ImportExportConflictItem>;
                        SoeGrid1.Visible = conflicts != null && conflicts.Count > 0;
                        SoeGrid1.DataSource = conflicts;
                        SoeGrid1.DataBind();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString(); //prevent compiler warning
                    }
                    finally
                    {
                        //Remove temp-file
                        if (!String.IsNullOrEmpty(pathOnServer))
                            eim.RemoveFileFromServer(pathOnServer);
                    }

                    #endregion
                }
                else
                {
                    #region File not found

                    Form1.MessageWarning = GetText(1179, "Filen hittades inte");

                    #endregion
                }

                #endregion
            }

            #endregion
        }
    }
}

