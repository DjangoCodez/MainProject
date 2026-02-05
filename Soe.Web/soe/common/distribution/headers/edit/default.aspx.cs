using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.ReportGroups;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SoftOne.Soe.Web.soe.common.distribution.headers.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private IReportGroupService ReportService;

        //Module specifics
        bool grossProfitCodesPermission = false;
        bool oppositeCharacterPermission = false;
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        private SoeModule TargetSoeModule = SoeModule.None;
        private Feature FeatureEdit = Feature.None;

        protected ReportHeaderDTO reportHeader;
        private int reportHeaderId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            grossProfitCodesPermission = HasRolePermission(Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes, Permission.Readonly);
            oppositeCharacterPermission = HasRolePermission(Feature.Economy_Distribution_Headers_OppositeCharacter, Permission.Readonly);

            var rm = new ReportManager(ParameterObject);
            this.ReportService = ReportGroupServiceFactory.Create(this.IsSupportAdmin, this.IsSupportLicense, rm, this.SoeActorCompanyId.GetValueOrDefault());

            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            // Add scripts and style sheets
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Distribution_Headers_Edit:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Economy_Distribution_Headers_Edit;
                        break;
                    case Feature.Billing_Distribution_Headers_Edit:
                        TargetSoeModule = SoeModule.Billing;
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Billing_Distribution_Headers_Edit;
                        break;
                    case Feature.Time_Distribution_Headers_Edit:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Time_Distribution_Headers_Edit;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init


            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters
            if (Int32.TryParse(QS["header"], out reportHeaderId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    reportHeader = ReportService.GetPrevNextReportHeaderById(reportHeaderId, (int)TargetSoeModule, Mode);
                    ClearSoeFormObject();
                    if (reportHeader != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?header=" + reportHeader.ReportHeaderId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?header=" + reportHeaderId);
                }
                else
                {
                    reportHeader = ReportService.GetReportHeader(reportHeaderId, false);
                    if (reportHeader == null)
                    {
                        Form1.MessageWarning = GetText(2219, "Rapportrubrik hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(1435, "Redigera rapportrubrik");
            string registerModeTabHeaderText = GetText(1372, "Registrera rapportrubrik");

            var entity = reportHeader != null ?
                new ReportHeader()
                {
                    Created = reportHeader.Created,
                    CreatedBy = reportHeader.CreatedBy,
                    Modified = reportHeader.Modified,
                    ModifiedBy = reportHeader.ModifiedBy
                } :
                null;

            PostOptionalParameterCheck(Form1, entity, true, editModeTabHeaderText, registerModeTabHeaderText);


            Form1.Title = reportHeader != null ? reportHeader.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            //Interval
            Interval.PreviousForm = Form1.PreviousForm;

            if (grossProfitCodesPermission)
            {
                Interval.HideLabel = false;
                Interval.LabelType = (int)SoeFormIntervalEntryType.Numeric;

                //GrossProfitManager gpm = new GrossProfitManager(ParameterObject);
                //Interval.Labels = gpm.GetGrossProfitCodesDict(SoeCompany.ActorCompanyId, true);
            }

            if (!oppositeCharacterPermission)
            {
                InvertRow.Visible = false;
            }

            TemplateType.ConnectDataSource(GetGrpText(TermGroup.SysReportTemplateType, addEmptyRow: true, sortByValue: true));

            #endregion

            #region Set data

            ShowRow.Value = Boolean.TrueString;
            ShowZeroRow.Value = Boolean.TrueString;
            ShowLabel.Value = Boolean.TrueString;
            ShowSum.Value = Boolean.TrueString;
            DoNotSummerizeOnGroup.Value = Boolean.TrueString;
            InvertRow.Value = Boolean.TrueString;              

            if (reportHeader != null)
            {
                Name.Value = reportHeader.Name;
                Description.Value = reportHeader.Description;
                TemplateType.Value = reportHeader.TemplateTypeId.ToString();

                if (!reportHeader.ShowRow)
                    ShowRow.Value = Boolean.FalseString;
                if (!reportHeader.ShowZeroRow)
                    ShowZeroRow.Value = Boolean.FalseString;
                if (!reportHeader.ShowSum)
                    ShowSum.Value = Boolean.FalseString;
                if (!reportHeader.ShowLabel)
                    ShowLabel.Value = Boolean.FalseString;
                if (!reportHeader.DoNotSummarizeOnGroup)
                    DoNotSummerizeOnGroup.Value = Boolean.FalseString;
                if (!reportHeader.InvertRow)
                    InvertRow.Value = Boolean.FalseString;

                // Get ReportHeaderIntervals
                IEnumerable<ReportHeaderIntervalDTO> intervals = ReportService.GetReportHeaderIntervals(reportHeader.ReportHeaderId);
                if (intervals.Any())
                {
                    int pos = 0;
                    foreach (var interval in intervals)
                    {
                        Interval.AddLabelValue(pos, interval.SelectValue.ToString());
                        Interval.AddValueFrom(pos, interval.IntervalFrom.ToString());
                        Interval.AddValueTo(pos, interval.IntervalTo.ToString());

                        pos++;
                        if (pos == Interval.NoOfIntervals)
                            break;
                    }

                    /*string[] fromValues = new string[10];
                    string[] toValues = new string[10];
                    int index = 0;
                    foreach (ReportHeaderInterval interval in intervals)
                    {
                        fromValues[index] = interval.IntervalFrom;
                        toValues[index] = interval.IntervalTo;
                        index++;
                    }
                    Interval.ValuesFrom = fromValues;
                    Interval.ValuesTo = toValues;*/
                }
            }
            else
            {
                TemplateType.Value = Convert.ToString((int)SoeReportTemplateType.Generic);
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(2172, "Rapportrubrik sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(2174, "Rapportrubrik kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(2173, "Rapportrubrik uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(1548, "Rapportrubrik kunde inte uppdateras");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1970, "Rapportrubrik borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(2175, "Rapportrubrik kunde inte tas bort, kontrollera att den inte används");
            }

            #endregion

            #region Navigation

            if (reportHeader != null)
            {
                Form1.SetRegLink(GetText(2210, "Registrera rapportrubrik"), "",
                    FeatureEdit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            int actorCompanyId = SoeCompany.ActorCompanyId;

            Collection<FormIntervalEntryItem> formIntervalEntryItems = null;

            //Get all intervals

            int nrOfInterval = Interval.GetNoOfIntervals(F);
            if (nrOfInterval > 0)
                formIntervalEntryItems = Interval.GetData(F);

            string name = F["Name"];
            string description = F["Description"];
            bool showRow = StringUtility.GetBool(F["ShowRow"]);
            bool showZeroRow = StringUtility.GetBool(F["ShowZeroRow"]);
            bool showLabel = StringUtility.GetBool(F["ShowLabel"]);
            bool showSum = StringUtility.GetBool(F["ShowSum"]);
            bool doNotSummerizeOnGroup = StringUtility.GetBool(F["DoNotSummerizeOnGroup"]);
            bool invertRow = StringUtility.GetBool(F["InvertRow"]);
            int templateTypeId = Convert.ToInt32(Request.Form["TemplateType"]);

            if ((!String.IsNullOrEmpty(name)) && (nrOfInterval > 0) && (actorCompanyId > 0))
            {
                if (reportHeader == null)
                {
                    reportHeader = new ReportHeaderDTO()
                    {
                        Name = name,
                        Description = description,
                        TemplateTypeId = templateTypeId,
                        ShowRow = showRow,
                        ShowZeroRow = showZeroRow,
                        ShowSum = showSum,
                        ShowLabel = showLabel,
                        DoNotSummarizeOnGroup = doNotSummerizeOnGroup,
                        InvertRow = invertRow,
                        Module = TargetSoeModule,
                    };

                    if (ReportService.AddReportHeader(reportHeader, formIntervalEntryItems).Success)
                        RedirectToSelf("SAVED");
                    else
                        RedirectToSelf("NOTSAVED", true);
                }
                else
                {
                    reportHeader.Name = name;
                    reportHeader.Description = description;
                    reportHeader.TemplateTypeId = templateTypeId;
                    reportHeader.ShowRow = showRow;
                    reportHeader.ShowZeroRow = showZeroRow;
                    reportHeader.DoNotSummarizeOnGroup = doNotSummerizeOnGroup;
                    reportHeader.ShowSum = showSum;
                    reportHeader.ShowLabel = showLabel;
                    reportHeader.InvertRow = invertRow;

                    if (ReportService.UpdateReportHeader(reportHeader, formIntervalEntryItems).Success)
                    {
                        //if (ReportService.UpdateReportHeaderInterval(reportHeader.ReportHeaderId, formIntervalEntryItems).Success)
                        RedirectToSelf("UPDATED");
                    }
                    else
                    {
                        RedirectToSelf("NOTUPDATED");
                    }
                }
            }

            RedirectToSelf("NOTSAVED", true);
        }

        protected override void Delete()
        {
            if (ReportService.DeleteReportHeader(reportHeader).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
