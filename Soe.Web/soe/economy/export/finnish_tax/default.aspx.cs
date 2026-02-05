using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;

namespace SoftOne.Soe.Web.soe.economy.export.finnish_tax
{
    public partial class _default : PageBase
    {
        #region Variables

        private int exportType;
        private Dictionary<string, string> taxEras = new Dictionary<string, string>();        
        private Dictionary<int, string> correctionReasons = new Dictionary<int, string>();

        private string taxEra;
        private int taxPeriod;
        private int taxYear;
        private bool noActivity;        
        private int selectedReason;


        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.HasAngularHost = true;
            base.Page_Init(sender, e);
            //Feature set in Page_Load depending on type 
            //TL20250122: I can only see that this page is used in one place, and not feature was set below, so I'll be setting it from now on...
            this.Feature = Feature.Economy_Export_Finnish_Tax;


            //Add scripts and style sheets
            Scripts.Add("default.js");            
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init
            //Mandatory parameters
            if (Int32.TryParse(QS["type"], out exportType))
            {
                switch (exportType)
                {
                    case 1: //Intervall VAT report
                        Form1.SetTabHeaderText(1, GetText(4581, "Skatter på eget initiativ"));
                        break;
                    default:
                        throw new SoeGeneralException("Unknown exception", this.ToString());
                }
            }
            else
            {
                throw new SoeQuerystringException("type", this.ToString());
            }            

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath + "?type=" + exportType, Request.Url.PathAndQuery);

            //Mode
            string editModeTabHeaderText = GetText(1058, "Redigera företag");

            #endregion


            #region Populate

            //Populate
            bool repopulate = Mode == SoeFormMode.Repopulate;

            //Tax eras
            taxEras.Add("K", GetText(4840, "Månad").ToString());
            taxEras.Add("Q", GetText(4841, "Kvartalet").ToString());
            taxEras.Add("V", GetText(4842, "År").ToString());
            TaxEras_S.ConnectDataSource(taxEras);            

            //Tax year
            int currentYear = DateTime.Now.Year;
            TaxYear_S.Value = currentYear.ToString();

            //Reasons for correction
            correctionReasons.Add(0, "");
            correctionReasons.Add(1, GetText(4849, "Räknefel/Felaktigt ifylld deklaration"));
            correctionReasons.Add(2, GetText(4850, "Handledning i samband med skatterevision"));
            correctionReasons.Add(3, GetText(4851, "Ändrad rättspraxis"));
            correctionReasons.Add(4, GetText(4852, "Felaktig lagtolkning"));
            Reasons_S.ConnectDataSource(correctionReasons);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                ValidateValues();
                
                //Create temp filename in the temp directory on the server
                string filePath = ConfigSettings.SOE_SERVER_DIR_TEMP_FI_TAX_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_FI_TAX_SUFFIX;
                if (Export(filePath))
                {
                    Form1.MessageSuccess = GetText(4583, "Skattedeklaration export klar");

                    string fileName = Constants.SOE_SERVER_FILENAME_PREFIX + Constants.SOE_SERVER_FI_TAX_PREFIX + DateTime.Now.ToString("yyyyMMdd") + Constants.SOE_SERVER_FILE_FI_TAX_SUFFIX;

                    DownloadFiTaxItem downloadFiTaxItem = new DownloadFiTaxItem()
                    {
                        FileNameOnClient = fileName,
                        FilePathOnServer = filePath,
                    };

                    Session[Constants.SESSION_DOWNLOAD_FI_TAX_ITEM] = downloadFiTaxItem;
                    Response.Redirect("download/");
                }
                else
                {
                    Form1.MessageError = GetText(4582, "Skattedeklaration export misslyckades");
                }
                
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "ALLOWED_PERIOD_BETWEEN_1_AND_12")
                    Form1.MessageWarning = GetText(4853, "Period måste vara mellan 1 och 12");
                if (MessageFromSelf == "ALLOWED_PERIOD_BETWEEN_1_AND_4")
                    Form1.MessageWarning = GetText(4854, "Period måste vara mellan 1 och 4");
                if (MessageFromSelf == "FALSE_YEAR_LENGTH")
                    Form1.MessageWarning = GetText(4855, "Felaktigt format på året. Tillåtet format är ÅÅÅÅ.");
                if (MessageFromSelf == "YEAR_TOO_FAR_IN_PAST")
                    Form1.MessageWarning = GetText(4857, "Året är för mycket i det förflutna");
                if (MessageFromSelf == "MANDATORY_REASON")
                    Form1.MessageWarning = GetText(4856, "Orsak måste anges");
            }

            #endregion
        }

        #region Action-methods

        private void ValidateValues()
        {
            
            //era
            taxEra = Convert.ToString(F["TaxEras_S"]);            

            //period            
            Int32.TryParse(Convert.ToString(F["TaxPeriods_S"]), out taxPeriod);            

            if (taxEra == "K" && (taxPeriod == 0 || taxPeriod > 12))
                RedirectToSelf("ALLOWED_PERIOD_BETWEEN_1_AND_12", true);

            if (taxEra == "Q" && (taxPeriod == 0 || taxPeriod > 4))
                RedirectToSelf("ALLOWED_PERIOD_BETWEEN_1_AND_4", true);

            //year
            taxYear = Convert.ToInt32(F["TaxYear_S"]);
            string taxYearStr = Convert.ToString(F["TaxYear_S"]);

            if (taxYearStr.Length != 4)
                RedirectToSelf("FALSE_YEAR_LENGTH", true);
            if (taxYear < DateTime.Now.Year - 4)
                RedirectToSelf("YEAR_TOO_FAR_IN_PAST", true);

            //no activity
            noActivity = StringUtility.GetBool(F["NoActivity_CB"]);            

            //correction
            bool correction = StringUtility.GetBool(F["Correction_CB"]);

            //reason for correction
            selectedReason = Convert.ToInt32(F["Reasons_S"]);

            if (correction && selectedReason == 0)
                RedirectToSelf("MANDATORY_REASON", true);
            
        }

        private bool Export(string filePath)
        {
            ActionResult success = new ActionResult(false);

            try
            {
                TextWriter writer = new StreamWriter(filePath, false, Constants.ENCODING_LATIN1);
                if (writer != null)
                {
                    FI_TaxDeclarationManager tdm = new FI_TaxDeclarationManager(ParameterObject);
                    success = tdm.Export(writer, SoeCompany.ActorCompanyId, taxEra, taxPeriod, taxYear, noActivity, selectedReason);
                    writer.Close();
                }
            }
            finally
            {
                if (!success.Success)
                    File.Delete(filePath);
            }

            return success.BooleanValue;
        }

        #endregion

        #region Help-methods

        #endregion
    }
}
