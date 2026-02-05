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

namespace SoftOne.Soe.Web.soe.economy.export.sie
{
	public partial class _default : PageBase
    {
        #region Variables

        private SieExportContainer ec;
		private int exportType;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
			this.HasAngularHost = true;
            this.Feature = Feature.Economy_Export_Sie;
            base.Page_Init(sender, e);
            Scripts.Add("default.js");
        }

		protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            ec = new SieExportContainer();

            //Mandatory parameters
            if (int.TryParse(QS["type"], out exportType))
            {
                SelectionStd.AdjustForOnlyFromPeriodInterval = !ShowPeriodIntervall();

                switch (exportType)
                {
                    case (int)SieExportType.Type1:
                        this.Feature = Feature.Economy_Export_Sie_Type1;
                        Form1.SetTabHeaderText(1, GetText(1420, "SIE typ") + " 1 - " + GetText(1416, "Bokslutssaldon"));

                        SelectionVoucher.Visible = false;
                        SelectionAccount.OnlyAccountDimStd = true;
                        ExportObject.Visible = false;
                        break;
                    case (int)SieExportType.Type2:
                        this.Feature = Feature.Economy_Export_Sie_Type2;

                        Form1.SetTabHeaderText(1, GetText(1420, "SIE typ") + " 2 - " + GetText(1417, "Periodsaldon"));

                        SelectionVoucher.Visible = false;
                        SelectionAccount.OnlyAccountDimStd = true;
                        ExportObject.Visible = false;
                        break;
                    case (int)SieExportType.Type3:
                        this.Feature = Feature.Economy_Export_Sie_Type3;
                        Form1.SetTabHeaderText(1, GetText(1420, "SIE typ") + " 3 - " + GetText(1418, "Objektsaldon"));

                        NotImplementedInstruction.HeaderText = GetText(1804, "Information");
                        NotImplementedInstruction.Numeric = true;
                        NotImplementedInstruction.Instructions = new List<string>()
		                {
			                GetText(5290, "Export av ingående balans för objekt (#OIB) stöds ej")
		                };

                        SelectionVoucher.Visible = false;
                        break;
                    case (int)SieExportType.Type4:
                        this.Feature = Feature.Economy_Export_Sie_Type4;
                        Form1.SetTabHeaderText(1, GetText(1420, "SIE typ") + " 4 - " + GetText(1419, "Transaktioner"));

                        NotImplementedInstruction.HeaderText = GetText(1804, "Information");
                        NotImplementedInstruction.Numeric = true;
                        NotImplementedInstruction.Instructions = new List<string>()
		                {
			                GetText(5290, "Export av ingående balans för objekt (#OIB) stöds ej")
		                };
                        break;
                    default:
                        throw new SoeGeneralException("Unknown SIE export type", this.ToString());
                }
            }
            else
            {
				//throw new SoeQuerystringException("type", this.ToString());
				RedirectToModuleRoot();
                return;
            }

			//Mode 
			PreOptionalParameterCheck(Request.Url.AbsolutePath + "?type=" + exportType, Request.Url.PathAndQuery);

			//Mode
            PostOptionalParameterCheck(Form1, null, true);

            SoeGrid1.Title = GetText(5426, "Resultat");

            #endregion

            #region UserControls

            //Set UserControl parameters
			SelectionStd.SoeForm = Form1;
			SelectionVoucher.SoeForm = Form1;
			SelectionAccount.SoeForm = Form1;

            #endregion

            #region Populate

            //Populate
			bool repopulate = Mode == SoeFormMode.Repopulate;
			int accountYearIdFrom;
			int accountYearIdTo;
			SelectionStd.Populate(repopulate);
			SelectionStd.GetSelectedAccountYearId(repopulate, out accountYearIdFrom, out accountYearIdTo);
			SelectionVoucher.Populate(repopulate, accountYearIdFrom, accountYearIdTo);
			SelectionAccount.Populate(repopulate);

			if (!repopulate)
			{
				ExportPreviousYear.Value = Boolean.TrueString;
				ExportObject.Value = Boolean.TrueString;
				ExportAccount.Value = Boolean.TrueString;
				ExportAccountType.Value = Boolean.TrueString;
				ExportSruCodes.Value = Boolean.TrueString;
            }

            #endregion

            #region Actions

            if (Form1.IsPosted)
			{
				//Create temp filename in the temp directory on the server
                string filePath = ConfigSettings.SOE_SERVER_DIR_TEMP_SIE_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_SIE_SUFFIX;
				var result = Export(filePath);

                if (result.Success)
				{
					Form1.MessageError = "";

                    if (ec.HasConflicts())
                        Form1.MessageWarning = GetText(1432, "SIE export klar") + ". " + GetText(1169, "Konflikter uppstod");
                    else
                        Form1.MessageSuccess = GetText(1432, "SIE export klar");

                    string fileName = Constants.SOE_SERVER_FILENAME_PREFIX + Constants.SOE_SERVER_SIE_PREFIX + exportType + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Constants.SOE_SERVER_FILE_SIE_SUFFIX;

					DownloadSieItem downloadSieItem = new DownloadSieItem()
					{
						FileNameOnClient = fileName,
						FilePathOnServer = filePath,
					};

					Session[Constants.SESSION_DOWNLOAD_SIE_ITEM] = downloadSieItem;
					Response.Redirect("download/");
				}
				else
				{
                    Form1.MessageError = GetText(1431, "SIE export misslyckades:" + result.ErrorMessage);
                    if (ec.HasConflicts())
                        Form1.MessageError += ". " + GetText(1169, "Konflikter uppstod");
				}
			}

			List<SieImportItemBase> conflictsColl = ec.GetConflicts();
			if (conflictsColl.Count <= 0)
				SoeGrid1.Visible = false;

			SoeGrid1.DataSource = conflictsColl;
			SoeGrid1.RowDataBound += SoeGrid1_RowDataBound;
			SoeGrid1.DataBind();

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "EVALUATE_FAILED")
                    Form1.MessageWarning = GetText(1450, "Felaktigt urval");
                if (MessageFromSelf == "MANDATORY_ACCOUNTYEAR")
                    Form1.MessageWarning = GetText(5291, "Ange redovisningsår");
            }

            #endregion
        }

        #region Action-methods

		private bool ShowPeriodIntervall()
		{
			return (exportType == (int)SieExportType.Type3) || (exportType == (int)SieExportType.Type4);

        }

        private ActionResult Export(string filePath)
		{
			var result = new ActionResult(false);

			try
			{
				TextWriter writer = new StreamWriter(filePath, false, Constants.ENCODING_IBM437);
				bool evaluated = false;

				ec.LoginName = SoeUser.LoginName;
				ec.ActorCompanyId = SoeCompany.ActorCompanyId;
				ec.Program = Constants.APPLICATION_NAME;
				ec.Version = Constants.APPLICATION_VERSION;
				ec.Comment = Request.Form["Comment"];
				ec.NoOfDimensions = 6;
				ec.ExportPreviousYear = StringUtility.GetBool(Request.Form["ExportPreviousYear"]);
				ec.ExportObject = StringUtility.GetBool(Request.Form["ExportObject"]);
				ec.ExportAccount = StringUtility.GetBool(Request.Form["ExportAccount"]);
				ec.ExportAccountType = StringUtility.GetBool(Request.Form["ExportAccountType"]);
				ec.ExportSruCodes = StringUtility.GetBool(Request.Form["ExportSruCodes"]);
				ec.StreamReader = writer;
				ec.ExportType = (SieExportType)exportType;

				Selection s = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName);
				s.Evaluated.OnlyActiveAccounts = true;

				//SelectionStd
				SelectionStd.F = Request.Form;
				s.SelectionStd = new SelectionStd();
				if (SelectionStd.Evaluate(s.SelectionStd, s.Evaluated))
				{
					if (s.Evaluated.SSTD_AccountYearId <= 0)
						RedirectToSelf("MANDATORY_ACCOUNTYEAR", true);

					//AccountPeriod: AccountYear from -> AccountPeriod to
					if (!ShowPeriodIntervall())
					{
						s.Evaluated.DateFrom = Convert.ToDateTime(s.SelectionStd.AccountYearFromDate);

                    }

					//SelectionVoucher
					SelectionVoucher.F = Request.Form;
					s.SelectionVoucher = new SelectionVoucher();
					if (SelectionVoucher.Evaluate(s.SelectionVoucher, s.Evaluated))
					{
						//SelectionAccount
						SelectionAccount.F = Request.Form;
						s.SelectionAccount = new SelectionAccount();
						evaluated = SelectionAccount.Evaluate(s.SelectionAccount, s.Evaluated);
					}
				}

				if (evaluated)
				{
					ec.Es = s.Evaluated;
				}
				else
				{
					RedirectToSelf("EVALUATE_FAILED", true);
				}

				SieManager sm = new SieManager(ParameterObject);
				result = sm.Export(ec);

			}
			finally
			{
				ec.CloseWriter();
				if (!result.Success)
					File.Delete(filePath);
			}

			return result;
        }

        #endregion

        #region Help-methods

        private void SoeGrid1_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			SieImportItemBase sieImportItem = ((e.Row.DataItem) as SieImportItemBase);
			if (sieImportItem != null)
			{
				e.Row.VerticalAlign = VerticalAlign.Top;

				#region conflict

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

							//Export
							case SieConflict.Export_WriteFailed:
								conflict = GetText(1487, "Kunde inte skriva rad");
								conflict = (sieConflictItem.StrData != null) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
								break;
							case SieConflict.Export_AccountStdIsNotNumeric:
								conflict = GetText(1488, "Kontonr måste vara numeriskt");
								conflict = (sieConflictItem.StrData != null) ? conflict + " [" + sieConflictItem.StrData + "]" : conflict;
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
