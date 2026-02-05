using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.soe.common.distribution.reports.permissions
{
    public partial class _default : PageBase
    {
        #region Variables

        private CompanyManager cm;
        private ReportManager rm;

        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        //private SoeModule TargetSoeModule = SoeModule.None;
        private Feature FeatureEdit = Feature.None;

        private Report report;
        private Dictionary<Company, List<Role>> validCompanyAndRolesDict;

        #endregion

        private bool IsAuthorized
        {
            get
            {
                if (report == null)
                    return true;
                return rm.HasReportRolePermission(report.ReportId, RoleId);
            }
        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Distribution_Reports_ReportRolePermission:
                        EnableEconomy = true;
                        //TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Economy_Distribution_Reports_ReportRolePermission;
                        break;
                    case Feature.Billing_Distribution_Reports_ReportRolePermission:
                        EnableBilling = true;
                        //TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Billing_Distribution_Reports_ReportRolePermission;
                        break;
                    case Feature.Time_Distribution_Reports_ReportRolePermission:
                        EnableTime = true;
                        //sTargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Time_Distribution_Reports_ReportRolePermission;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CompanyManager(ParameterObject);
            rm = new ReportManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters
            if (Int32.TryParse(QS["report"], out int reportId))
            {
                report = rm.GetReport(reportId, SoeCompany.ActorCompanyId, false, false, true);
                if (report == null)
                    Form1.MessageWarning = GetText(1336, "Rapport hittades inte");
            }

            Form1.Title = report.Name;

            #endregion

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.ReportPermissionMissing);

            #endregion

            #region Actions

            //Needed in save
            LoadValidCompaniesAndRoles();

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            RenderCompanyAndRoles();

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(5669, "Roll kopplad till rapport");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(5670, "Roll kunde inte kopplas till rapport");
            }

            #endregion

            #region Navigation

            Form1.AddLink(GetText(1323, "Redigera rapport"), "../edit/?report=" + report.ReportId,
                FeatureEdit, Permission.Readonly);

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            bool result = true;

            //Build a list of checked Roles
            var permissions = new List<ReportRolePermission>();

            foreach (var pair in this.validCompanyAndRolesDict)
            {
                Company validCompany = pair.Key;
                List<Role> validRoles = pair.Value;

                foreach (Role validRole in validRoles)
                {
                    #region Role

                    string id = GetCheckID(validCompany, validRole);
                    if (StringUtility.GetBool(F[id]))
                        AddReportRolePermission(permissions, report.ReportId, validRole.RoleId, SoeCompany.ActorCompanyId);

                    #endregion
                }
            }

            //Add permission for current Role, otherwise redirect to unauthorized directly after save
            if (permissions.Count > 0)
                AddReportRolePermission(permissions, report.ReportId, this.RoleId, this.SoeCompany.ActorCompanyId);

            result = rm.SaveReportRolePermission(permissions, report.ReportId, this.SoeCompany.ActorCompanyId).Success;
            if (result)
                RedirectToSelf("SAVED");
            else
                RedirectToSelf("NOTSAVED", true);
        }

        #endregion

        #region Help-methods

        private void LoadValidCompaniesAndRoles()
        {
            this.validCompanyAndRolesDict = cm.GetValidCompanyAndRoles(IsAdmin, UserId, SoeUser.LicenseId);
        }

        private void RenderCompanyAndRoles()
        {
            foreach (var pair in this.validCompanyAndRolesDict)
            {
                Company validCompany = pair.Key;
                List<Role> validRoles = pair.Value;

                if (validRoles.IsNullOrEmpty())
                    continue;

                #region Prefix

                HtmlTableRow tRow;
                HtmlTableCell tCell;

                var divCompanyRow = new HtmlGenericControl("div");
                var divCompany = new HtmlGenericControl("div");
                var fieldset = new HtmlGenericControl("fieldset");
                var legend = new HtmlGenericControl("legend")
                {
                    InnerText = validCompany.Name,
                };
                fieldset.Controls.Add(legend);

                var tableCompany = new HtmlTable();
                tableCompany.CellSpacing = 2;

                #endregion

                #region Header

                var tRowHeader = new HtmlTableRow();
                var tCellHeader = new HtmlTableCell();

                tRowHeader.Controls.Add(new HtmlTableCell()); //Empty
                tRowHeader.Controls.Add(new HtmlTableCell()); //Empty

                tableCompany.Rows.Add(tRowHeader);

                #endregion

                foreach (Role validRole in validRoles)
                {
                    #region Role

                    var reportRolePermissions = (from rrp in report.ReportRolePermission
                                                 where rrp.RoleId == validRole.RoleId
                                                 select rrp).FirstOrDefault();

                    tRow = new HtmlTableRow();

                    //Role
                    tCell = new HtmlTableCell();
                    if (HasRolePermission(Feature.Manage_Roles_Edit, Permission.Readonly))
                    {
                        var link = new Link()
                        {
                            ID = GetRoleID(validCompany, validRole),
                            Href = GetRoleLink(validCompany.ActorCompanyId, validRole.RoleId),
                            Value = validRole.Name,
                            Alt = validRole.Name,
                        };
                        tCell.Controls.Add(link);
                    }
                    else
                    {
                        var text = new Text()
                        {
                            ID = GetRoleID(validCompany, validRole),
                            LabelSetting = validRole.Name,
                            FitInTable = true,
                        };
                        tCell.Controls.Add(text);
                    }
                    tCell.Attributes.Add("style", "width:200px");
                    tRow.Controls.Add(tCell);

                    //Check
                    tCell = new HtmlTableCell();
                    var check = new CheckBoxEntry()
                    {
                        ID = GetCheckID(validCompany, validRole),
                        HideLabel = true,
                        DisableSettings = true,
                        FitInTable = true,
                        Value = reportRolePermissions != null ? Boolean.TrueString : Boolean.FalseString,
                    };
                    tCell.Controls.Add(check);
                    tRow.Controls.Add(tCell);

                    //Info
                    tCell = new HtmlTableCell();
                    var info = new Link()
                    {
                        Href = "#",
                        ImageSrc = "/img/information.png",
                        CssClass = "infoLink",
                        SkipTabstop = true,
                        Invisible = reportRolePermissions == null,
                        Alt = cm.GetCreatedModified(reportRolePermissions as EntityObject),
                    };
                    tCell.Controls.Add(check);
                    tRow.Controls.Add(tCell);

                    tableCompany.Rows.Add(tRow);

                    #endregion
                }

                #region Postfix

                fieldset.Controls.Add(tableCompany);
                divCompany.Controls.Add(fieldset);
                divCompanyRow.Controls.Add(divCompany);
                RoleMapping.Controls.Add(divCompany);
                RoleMapping.Controls.Add(divCompanyRow);

                #endregion
            }
        }

        private string GetRoleID(Company company, Role role)
        {
            return GetID("Role", company, role);
        }

        private string GetCheckID(Company company, Role role)
        {
            return GetID("Check", company, role);
        }

        private string GetID(string prefix, Company company, Role role)
        {
            return prefix + "_" + company.ActorCompanyId + "_" + role.RoleId;
        }

        private string GetRoleLink(int actorCompanyId, int roleId)
        {
            return String.Format("/soe/{0}/roles/edit/?license={1}&licenseNr={2}&company={3}&role={4}", SoeModule.Manage.ToString().ToLower(), SoeLicense.LicenseId, SoeLicense.LicenseNr, actorCompanyId, roleId);
        }

        private void AddReportRolePermission(List<ReportRolePermission> permissions, int reportId, int roleId, int actorCompanyId)
        {
            if (permissions == null)
                permissions = new List<ReportRolePermission>();

            if (!permissions.Any(i => i.ReportId == reportId && i.RoleId == roleId && i.ActorCompanyId == actorCompanyId))
            {
                ReportRolePermission permission = new ReportRolePermission()
                {
                    //Set FK
                    ReportId = reportId,
                    RoleId = roleId,
                    ActorCompanyId = actorCompanyId,
                };

                permissions.Add(permission);
            }
        }

        #endregion
    }
}