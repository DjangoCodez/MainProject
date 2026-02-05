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

namespace SoftOne.Soe.Web.soe.common.distribution.reports.reportgroupmapping
{
    public partial class _default : PageBase
    {
        #region Variables

        private IReportGroupService ReportService;
        private int reportId;
        protected ReportAbstractionDTO report;

        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        private SoeModule TargetSoeModule = SoeModule.None;

        #endregion

        private bool IsAuthorized
        {
            get
            {
                if (report == null)
                    return true;
                return ReportService.HasReportRolePermission(report.ReportId, this.RoleId);
            }
        }

        protected override void Page_Init(object sender, EventArgs e)   
        {
            //Set variables to reuse page with different contet
            var rm = new ReportManager(ParameterObject);
            this.ReportService = ReportGroupServiceFactory.Create(this.IsSupportAdmin, this.IsSupportLicense, rm, this.SoeActorCompanyId.GetValueOrDefault());
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
                    case Feature.Economy_Distribution_Reports_ReportGroupMapping:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        break;
                    case Feature.Billing_Distribution_Reports_ReportGroupMapping:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        break;
                    case Feature.Time_Distribution_Reports_ReportGroupMapping:
                        EnableTime = true;
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
            report = null;
            if (Int32.TryParse(QS["report"], out reportId))
            {
                report = ReportService.GetReport(reportId);
                if (report == null)
                {
                    Form1.MessageWarning = GetText(1336, "Rapport hittades inte");
                    Mode = SoeFormMode.Register;
                }
            }

            //Mode
            PostOptionalParameterCheck(Form1, report != null ? new Report() : null, true);

            Form1.Title = report != null ? report.Name : "";

            #endregion

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.ReportPermissionMissing);

            #endregion

            #region Populate


            ReportGroups.ConnectDataSource(ReportService.GetReportGroups((int)TargetSoeModule, report.SysTemplateTypeId, false, false), "NameAndDescription", "ReportGroupId");

            #endregion

            #region Render

            // Print out all voucher series connected to actual year and company
            bool printHead = true;

            HtmlTableRow tRow;
            HtmlInputButton tButton;
            HtmlTableCell tCell;
            Text label;

            int order = 0;

            var reportGroupMappings = ReportService.GetReportGroupMappings(reportId);
            foreach (var reportGroupMapping in reportGroupMappings)
            {
                order = reportGroupMapping.Order;

                #region Header

                if (printHead)
                {
                    tRow = new HtmlTableRow();

                    //Number
                    tCell = new HtmlTableCell();
                    tCell.Style["WIDTH"] = "80px";
                    label = new Text()
                    {
                        TermID = 2181,
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
                        TermID = 2182,
                        DefaultTerm = "Namn",
                        FitInTable = true,
                    };
                    tCell.Controls.Add(label);
                    tRow.Cells.Add(tCell);
                    tCell = new HtmlTableCell();

                    //Description
                    tCell = new HtmlTableCell();
                    tCell.Style["WIDTH"] = "150px";
                    label = new Text()
                    {
                        TermID = 2183,
                        DefaultTerm = "Beskrivning",
                        FitInTable = true,
                    };
                    tCell.Controls.Add(label);
                    tRow.Cells.Add(tCell);
                    tCell = new HtmlTableCell();

                    Groups.Rows.Add(tRow);

                    printHead = false;
                }

                #endregion

                tRow = new HtmlTableRow();

                //Order
                tCell = new HtmlTableCell();
                tCell.Controls.Add(new LiteralControl(Convert.ToString(reportGroupMapping.Order)));
                tRow.Cells.Add(tCell);

                //Name
                tCell = new HtmlTableCell();
                Link link = new Link()
                {
                    Href = "/soe/economy/distribution/groups/edit/?group=" + reportGroupMapping.ReportGroupId,
                    Value = reportGroupMapping.ReportGroup.Name,
                    Alt = reportGroupMapping.ReportGroup.Name,
                };
                tCell.Controls.Add(link);
                tRow.Cells.Add(tCell);

                //Description
                tCell = new HtmlTableCell();
                tCell.Controls.Add(new LiteralControl(reportGroupMapping.ReportGroup.Description));
                tRow.Cells.Add(tCell);

                // Move up button
                tCell = new HtmlTableCell();
                tButton = new HtmlInputButton("submit");
                tButton.Value = GetText(2299, "Upp");
                tButton.ID = "up_" + reportGroupMapping.ReportId + "_" + reportGroupMapping.ReportGroupId;
                tCell.Controls.Add(tButton);
                tRow.Cells.Add(tCell);

                // Move down button
                tCell = new HtmlTableCell();
                tButton = new HtmlInputButton("submit");
                tButton.Value = GetText(2300, "Ner");
                tButton.ID = "down_" + reportGroupMapping.ReportId + "_" + reportGroupMapping.ReportGroupId;
                tCell.Controls.Add(tButton);
                tRow.Cells.Add(tCell);

                // Remove button
                tCell = new HtmlTableCell();
                tButton = new HtmlInputButton("submit");
                tButton.Value = GetText(2185, "Ta bort");
                tButton.ID = "remove_" + reportGroupMapping.ReportId + "_" + reportGroupMapping.ReportGroupId;
                tCell.Controls.Add(tButton);
                tRow.Cells.Add(tCell);

                Groups.Rows.Add(tRow);
            }
            order++;

            #endregion

            #region Actions

            //Variable order causes Save to be done last
            if (Form1.IsPosted)
            {
                Save(order);
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(2188, "Rapportgrupp kopplad till rapport");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(2189, "Rapportgrupp kunde inte kopplas till rapport");
                else if (MessageFromSelf == "REMOVED")
                    Form1.MessageSuccess = GetText(2186, "Rapportgrupp borttagen från rapport");
                else if (MessageFromSelf == "NOTREMOVED")
                    Form1.MessageError = GetText(2187, "Rapportgrupp kunde inte tas bort från rapport");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(1526, "Rapportgrupp redan kopplad till rapport");
            }

            #endregion
        }

        #region Action-methods

        private void Save(int order)
        {
            // Check if serie should be removed
            foreach (string curr in F.AllKeys)
            {
                if (curr.StartsWith("remove_"))
                {
                    String[] split = curr.Split('_');
                    int rId = Convert.ToInt32(split[1]);
                    int groupId = Convert.ToInt32(split[2]);
                    if (ReportService.DeleteReportGroupMapping(rId, groupId).Success)
                    {
                        ReportService.ReorderReportGroupMapping(rId, -1, false);
                        RedirectToSelf("REMOVED");
                    }
                    else
                        RedirectToSelf("NOTREMOVED", true);
                }
                else if ((curr.StartsWith("up_")) || (curr.StartsWith("down_")))
                {
                    bool isUp = false;
                    if (curr.StartsWith("up_"))
                        isUp = true;
                    String[] split = curr.Split('_');
                    int rId = Convert.ToInt32(split[1]);
                    int groupId = Convert.ToInt32(split[2]);
                    if (ReportService.ReorderReportGroupMapping(rId, groupId, isUp).Success)
                        RedirectToSelf("");
                    else
                        RedirectToSelf("NOTSAVED", true);
                }
            }

            int reportGroupId = Convert.ToInt32(F["ReportGroups"]);

            if (ReportService.ReportGroupExistsInReport(reportId, reportGroupId))
                RedirectToSelf("EXIST");

            ReportGroupMappingDTO reportGroupMapping = new ReportGroupMappingDTO()
            {
                Order = order,
            };

            if (ReportService.AddReportGroupMapping(reportGroupMapping, reportId, reportGroupId).Success)
                RedirectToSelf("SAVED");
            else
                RedirectToSelf("NOTSAVED", true);
        }

        #endregion
    }
}
