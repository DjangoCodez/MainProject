using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.ReportGroups;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.soe.common.distribution.groups.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private ReportManager rm;
        private IReportGroupService ReportGroupService;

        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        private SoeModule TargetSoeModule = SoeModule.None;
        private Feature FeatureEdit = Feature.None;

        protected ReportGroupDTO reportGroup;
        private int reportGroupId;
        private int order;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            this.rm = new ReportManager(ParameterObject);
            this.ReportGroupService = ReportGroupServiceFactory.Create(this.IsSupportAdmin, this.IsSupportLicense, rm, this.SoeActorCompanyId.GetValueOrDefault());
            
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
                    case Feature.Economy_Distribution_Groups_Edit:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Economy_Distribution_Groups_Edit;
                        break;
                    case Feature.Billing_Distribution_Groups_Edit:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Billing_Distribution_Groups_Edit;
                        break;
                    case Feature.Time_Distribution_Groups_Edit:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Time_Distribution_Groups_Edit;
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
            if (Int32.TryParse(QS["group"], out reportGroupId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    reportGroup = ReportGroupService.GetPrevNextReportGroupById(reportGroupId, (int)TargetSoeModule, Mode);
                    ClearSoeFormObject();
                    if (reportGroup != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?group=" + reportGroup.ReportGroupId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?group=" + reportGroupId);
                }
                else
                {
                    reportGroup = ReportGroupService.GetReportGroup(reportGroupId, false, false);
                    if (reportGroup == null)
                    {
                        Form1.MessageWarning = GetText(1511, "Rapportgrupp hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(2220, "Redigera rapportgrupp");
            string registerModeTabHeaderText = GetText(1559, "Registrera rapportgrupp");

            var entity = reportGroup != null ?
                new ReportGroup()
                {
                    Created = reportGroup.Created,
                    CreatedBy = reportGroup.CreatedBy,
                    Modified = reportGroup.Modified,
                    ModifiedBy = reportGroup.ModifiedBy
                } :
                null;

            PostOptionalParameterCheck(Form1, entity, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = reportGroup != null ? reportGroup.Name : "";

            #endregion

            #region Populate

            int templateTypeId = -1;
            if (reportGroup != null)
                templateTypeId = reportGroup.TemplateTypeId;

            ReportHeaders.ConnectDataSource(ReportGroupService.GetReportHeaders((int)TargetSoeModule, templateTypeId, false), "NameAndDescription", "ReportHeaderId");
            TemplateType.ConnectDataSource(GetGrpText(TermGroup.SysReportTemplateType, addEmptyRow: true, sortByValue: true));

            #endregion

            #region Set data

            if (reportGroup != null)
            {
                Name.Value = reportGroup.Name;
                Description.Value = reportGroup.Description;
                TemplateType.Value = reportGroup.TemplateTypeId.ToString();
                ShowSum.Value = reportGroup.ShowSum ? Boolean.TrueString : Boolean.FalseString;
                ShowLabel.Value = reportGroup.ShowLabel ? Boolean.TrueString : Boolean.FalseString;
                InvertRow.Value = reportGroup.InvertRow ? Boolean.TrueString : Boolean.FalseString;

                RenderReportGroupMapping();
            }
            else
            {
                ReportHeaders.Visible = false;
                TemplateType.Value = Convert.ToString((int)SoeReportTemplateType.Generic);
                ShowLabel.Value = Boolean.TrueString;
                ShowSum.Value = Boolean.TrueString;
                InvertRow.Value = Boolean.TrueString;
            }

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(2226, "Rapportgrupp sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(2227, "Rapportgrupp kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(1527, "Rapportgrupp uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(1528, "Rapportgrupp kunde inte uppdateras");
                else if (MessageFromSelf == "REMOVED")
                    Form1.MessageSuccess = GetText(2224, "Rapportrubrik borttagen från rapportgrupp");
                else if (MessageFromSelf == "NOTREMOVED")
                    Form1.MessageError = GetText(2225, "Rapportrubrik kunde inte tas bort från rapportgrupp");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1969, "Rapportrubrik borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(2229, "Rapportgrupp kunde inte tas bort, kontrollera att den inte används");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(1524, "Rapportgrupp innehåller redan vald rapportrubrik");
            }

            #endregion

            #region Navigation

            if (reportGroup != null)
            {
                Form1.SetRegLink(GetText(2190, "Registrera rapportgrupp"), "",
                    FeatureEdit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            // Check if serie should be removed
            bool add = false;
            bool update = false;
            foreach (string curr in F.AllKeys)
            {
                if (curr.StartsWith("add"))
                {
                    add = true;
                }
                else if (curr.StartsWith("upd"))
                {
                    update = true;
                }
                else if (curr.StartsWith("remove_"))
                {
                    String[] split = curr.Split('_');
                    int groupId = Convert.ToInt32(split[1]);
                    int headerId = Convert.ToInt32(split[2]);
                    if (ReportGroupService.DeleteReportGroupHeaderMapping(groupId, headerId).Success)
                    {
                        ReportGroupService.ReorderReportGroupHeaderMapping(groupId, -1, false);
                        RedirectToSelf("REMOVED");
                    }
                    else
                    {
                        RedirectToSelf("NOTREMOVED", true);
                    }
                }
                else if ((curr.StartsWith("up_")) || (curr.StartsWith("down_")))
                {
                    bool isUp = false;
                    if (curr.StartsWith("up_"))
                        isUp = true;

                    String[] split = curr.Split('_');
                    int groupId = Convert.ToInt32(split[1]);
                    int headerId = Convert.ToInt32(split[2]);
                    if (ReportGroupService.ReorderReportGroupHeaderMapping(groupId, headerId, isUp).Success)
                        RedirectToSelf("");
                    else
                        RedirectToSelf("NOTSAVED", true);
                }
            }

            bool showLabel = StringUtility.GetBool(F["ShowLabel"]);
            bool showSum = StringUtility.GetBool(F["ShowSum"]);
            bool invertRow = StringUtility.GetBool(F["InvertRow"]);
            string name = F["Name"];
            string description = F["Description"];
            if (reportGroup != null)
            {
                if (add)
                {
                    int reportHeaderId = Convert.ToInt32(F["ReportHeaders"]);
                    //reportGroup.ParentGuid = Guid.NewGuid();

                    if (ReportGroupService.ReportHeaderExistInReportGroup(reportGroupId, reportHeaderId))
                        RedirectToSelf("EXIST", true);

                    ReportGroupHeaderMappingDTO reportGroupHeaderMapping = new ReportGroupHeaderMappingDTO()
                    {
                        Order = order,
                    };

                    if (ReportGroupService.AddReportGroupHeaderMapping(reportGroupHeaderMapping, reportGroupId, reportHeaderId).Success)
                        RedirectToSelf("SAVED");
                    else
                        RedirectToSelf("NOTSAVED", true);
                }
                else if (update && !String.IsNullOrEmpty(name))
                {
                    reportGroup.Name = name;
                    reportGroup.Description = description;
                    reportGroup.TemplateTypeId = Convert.ToInt32(Request.Form["TemplateType"]);
                    reportGroup.ShowSum = showSum;
                    reportGroup.ShowLabel = showLabel;
                    reportGroup.InvertRow = invertRow;
                    //if (reportGroup.ParentGuid==null)
                    //{
                    //  reportGroup.ParentGuid = Guid.NewGuid();
                    //}

                    if (ReportGroupService.UpdateReportGroup(reportGroup).Success)
                        RedirectToSelf("UPDATED");
                    else
                        RedirectToSelf("NOTUPDATED", true);
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(name))
                {
                    reportGroup = new ReportGroupDTO()
                    {
                        Name = name,
                        Description = description,
                        TemplateTypeId = Convert.ToInt32(Request.Form["TemplateType"]),
                        ShowSum = showSum,
                        ShowLabel = showLabel,
                        InvertRow = invertRow,
                        Module = TargetSoeModule,
                    };
                    var result = ReportGroupService.AddReportGroup(reportGroup);
                    if (result.Success)
                        Response.Redirect(Request.Url.AbsolutePath + "?group=" + result.IntegerValue);
                    else
                        RedirectToSelf("NOTSAVED", true);
                }
            }
        }

        protected override void Delete()
        {
            if (ReportGroupService.DeleteReportGroupHeaderMappings(reportGroup.ReportGroupId).Success)
            {
                if (ReportGroupService.DeleteReportGroup(reportGroup.ReportGroupId).Success)
                    RedirectToSelf("DELETED", false, true);
            }
            else
            {
                RedirectToSelf("NOTDELETED", true);
            }
        }

        #endregion

        #region Help-methods

        private void RenderReportGroupMapping()
        {
            bool printHead = true;

            HtmlTableRow tRow;
            HtmlInputButton tButton;
            HtmlTableCell tCell;
            Text label;

            IEnumerable<ReportGroupHeaderMappingDTO> reportGroupHeaderMappings = ReportGroupService.GetReportGroupHeaderMappings(reportGroupId);
            foreach (ReportGroupHeaderMappingDTO reportGroupHeaderMapping in reportGroupHeaderMappings)
            {
                order = reportGroupHeaderMapping.Order;

                #region Header

                if (printHead)
                {
                    tRow = new HtmlTableRow();

                    //Number
                    tCell = new HtmlTableCell();
                    tCell.Style["WIDTH"] = "80px";
                    label = new Text()
                    {
                        TermID = 2222,
                        DefaultTerm = "Nummer",
                        FitInTable = true,
                    };
                    tCell.Controls.Add(label);
                    tRow.Cells.Add(tCell);

                    //Name
                    tCell = new HtmlTableCell();
                    tCell.Style["WIDTH"] = "200px";
                    label = new Text()
                    {
                        TermID = 2223,
                        DefaultTerm = "Namn",
                        FitInTable = true,
                    };
                    tCell.Controls.Add(label);
                    tRow.Cells.Add(tCell);

                    Headers.Rows.Add(tRow);

                    printHead = false;
                }

                #endregion

                #region Rows

                tRow = new HtmlTableRow();

                //Order
                tCell = new HtmlTableCell();
                tCell.Controls.Add(new LiteralControl(Convert.ToString(reportGroupHeaderMapping.Order)));
                tRow.Cells.Add(tCell);

                //Name
                tCell = new HtmlTableCell();
                Link link = new Link()
                {
                    Href = "/soe/economy/distribution/headers/edit/?header=" + reportGroupHeaderMapping.ReportHeaderId,
                    Value = reportGroupHeaderMapping.ReportHeader.Name,
                    Alt = reportGroupHeaderMapping.ReportHeader.Name,
                };
                tCell.Controls.Add(link);
                tRow.Cells.Add(tCell);

                // Move up button
                tCell = new HtmlTableCell();
                tButton = new HtmlInputButton("submit");
                tButton.Value = "Upp";
                tButton.ID = "up_" + reportGroupHeaderMapping.ReportGroupId + "_" + reportGroupHeaderMapping.ReportHeaderId;
                tCell.Controls.Add(tButton);
                tRow.Cells.Add(tCell);

                // Move down button
                tCell = new HtmlTableCell();
                tButton = new HtmlInputButton("submit");
                tButton.Value = "Ner";
                tButton.ID = "down_" + reportGroupHeaderMapping.ReportGroupId + "_" + reportGroupHeaderMapping.ReportHeaderId;
                tCell.Controls.Add(tButton);
                tRow.Cells.Add(tCell);

                // Remove button
                tButton = new HtmlInputButton("submit");
                tButton.Value = GetText(2185, "Ta bort");
                tButton.ID = "remove_" + reportGroupHeaderMapping.ReportGroupId + "_" + reportGroupHeaderMapping.ReportHeaderId;

                tCell = new HtmlTableCell();
                tCell.Controls.Add(tButton);
                tRow.Cells.Add(tCell);

                Headers.Rows.Add(tRow);

                #endregion
            }

            order++;
        }

        #endregion
    }
}
