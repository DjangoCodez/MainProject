using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.accounting.voucherseries.edit
{
	public partial class _default : PageBase
    {
        #region Variables

        private VoucherManager vm;
		
        protected VoucherSeriesType voucherSeriesType;
		private int voucherSeriesTypeId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_AccountPeriods_MapToVoucherSeries;
            base.Page_Init(sender, e);
        }

		protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            vm = new VoucherManager(ParameterObject);

			//Mandatory parameters

			//Mode 
			PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

			//Optional parameters
			if (Int32.TryParse(QS["type"], out voucherSeriesTypeId))
			{
				if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
				{
					voucherSeriesType = vm.GetPrevNextVoucherSeriesType(voucherSeriesTypeId, SoeCompany.ActorCompanyId, Mode);
					ClearSoeFormObject();
					if (voucherSeriesType != null)
						Response.Redirect(Request.Url.AbsolutePath + "?type=" + voucherSeriesType.VoucherSeriesTypeId);
					else
						Response.Redirect(Request.Url.AbsolutePath + "?type=" + voucherSeriesTypeId);
				}
				else
				{
					voucherSeriesType = vm.GetVoucherSeriesType(voucherSeriesTypeId, SoeCompany.ActorCompanyId);
					if (voucherSeriesType == null)
					{
                        Form1.MessageWarning = GetText(1280, "Verifikatserietyp hittades inte");
						return;
					}
				}
			}

			//Mode
			string editModeTabHeaderText = GetText(2086, "Redigera verifikatserie");
			string registerModeTabHeaderText = GetText(1547, "Registrera verifikatserie");
            PostOptionalParameterCheck(Form1, voucherSeriesType, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = voucherSeriesType != null ? voucherSeriesType.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
			{
				Save();
            }

            #endregion

            #region Set data

            if (voucherSeriesType != null)
			{
				VoucherSerieTypeNr.Value = Convert.ToString(voucherSeriesType.VoucherSeriesTypeNr);
				Name.Value = voucherSeriesType.Name;
				StartNr.Value = Convert.ToString(voucherSeriesType.StartNr);
                YearEndSerie.Value = Convert.ToString(voucherSeriesType.YearEndSerie);
                ExternalSerie.Value = Convert.ToString(voucherSeriesType.ExternalSerie);
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(2126, "Verifikatserie sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(2127, "Verifikatserie kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(2128, "Verifikatserie uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(2129, "Verifikatserie kunde inte uppdateras");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1980, "Verifikatserie borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(1278, "Verifikatserie kunde inte tas bort, kontrollera att den inte används");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(2087, "Verifikatserie med angivet serienr eller benämning finns redan");
                else if (MessageFromSelf == "MANDATORY_VOUCHERSERIESTYPENR_INVALID")
                    Form1.MessageInformation = GetText(5209, "Serienr måste vara större än noll");
                else if (MessageFromSelf == "MANDATORY_STARTNR_INVALID")
                    Form1.MessageInformation = GetText(5210, "Startnummer måste vara större än noll");
            }

            #endregion

            #region Navigation

            if (voucherSeriesType != null)
            {
                Form1.SetRegLink(GetText(2078, "Registrera verifikatserie"), "",
                    Feature.Economy_Accounting_VoucherSeries_Edit, Permission.Modify);
            }

            Form1.AddLink(GetText(2074, "Verifikatserier"), "../",
                Feature.Economy_Accounting_VoucherSeries, Permission.Readonly);

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string name = F["Name"];
            bool yearEndSerie = StringUtility.GetBool(F["YearEndSerie"]);
            bool externalSerie = StringUtility.GetBool(F["ExternalSerie"]);

            //Check VoucherSeriesTypeNr
            int voucherSeriesTypeNr;
            if (!(Int32.TryParse(F["VoucherSerieTypeNr"], out voucherSeriesTypeNr) && voucherSeriesTypeNr > 0))
                RedirectToSelf("MANDATORY_VOUCHERSERIESTYPENR_INVALID", true);

            //Check StartNr
            int startNr;
            if (!(Int32.TryParse(F["StartNr"], out startNr) && startNr > 0))
                RedirectToSelf("MANDATORY_STARTNR_INVALID", true);

			if (voucherSeriesType == null)
			{
				//Validation: VoucherSeriesType nr or name not already exist
                if (vm.VoucherSeriesTypeExist(voucherSeriesTypeNr, name, SoeCompany.ActorCompanyId))
					RedirectToSelf("EXIST", true);

				//Create VoucherSeriesType
				voucherSeriesType = new VoucherSeriesType()
				{
					StartNr = startNr,
					Name = name,
					VoucherSeriesTypeNr = voucherSeriesTypeNr,
                    YearEndSerie = yearEndSerie,
                    ExternalSerie = externalSerie,
				};

                if (vm.AddVoucherSeriesType(voucherSeriesType, SoeCompany.ActorCompanyId).Success)
					RedirectToSelf("SAVED");
				else
					RedirectToSelf("NOTSAVED", true);
			}
			else
			{
				//Validation: VoucherSeriesType name not already exist
                if (voucherSeriesType.VoucherSeriesTypeNr != voucherSeriesTypeNr || voucherSeriesType.Name != name)
				{
                    if (vm.VoucherSeriesTypeExist(voucherSeriesTypeNr, name, SoeCompany.ActorCompanyId, voucherSeriesType.VoucherSeriesTypeId))
						RedirectToSelf("EXIST", true);
				}

				//Update VoucherSeriesType
				voucherSeriesType.Name = name;
				voucherSeriesType.StartNr = startNr;
				voucherSeriesType.VoucherSeriesTypeNr = voucherSeriesTypeNr;
                voucherSeriesType.YearEndSerie = yearEndSerie;
                voucherSeriesType.ExternalSerie = externalSerie;

                if (vm.UpdateVoucherSeriesType(voucherSeriesType, SoeCompany.ActorCompanyId).Success)
					RedirectToSelf("UPDATED");
				else
					RedirectToSelf("NOTUPDATED", true);		
			}
		}

		protected override void Delete()
		{
            if (vm.DeleteVoucherSeriesType(voucherSeriesType.VoucherSeriesTypeId, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", false, true);
			else
				RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
